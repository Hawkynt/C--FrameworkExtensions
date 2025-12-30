#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Provides recursive IIR prefiltering for B-spline and o-Moms interpolation.
/// </summary>
/// <remarks>
/// <para>
/// Implements the recursive filter algorithm from:
/// "Fast B-Spline Transforms for Continuous Image Representation and Interpolation"
/// by Michael Unser, Akram Aldroubi, and Murray Eden (IEEE 1991).
/// </para>
/// <para>
/// The algorithm applies a cascade of first-order IIR filters:
/// <list type="number">
/// <item>Causal pass (forward): y[n] = x[n] + α·y[n-1]</item>
/// <item>Anti-causal pass (backward): y[n] = α·(y[n+1] - y[n])</item>
/// </list>
/// This is applied for each alpha coefficient in sequence, then separably
/// for rows and columns.
/// </para>
/// </remarks>
public static class RecursivePrefilter {

  /// <summary>
  /// Tolerance for boundary initialization convergence.
  /// </summary>
  private const float TOLERANCE = 1e-9f;

  /// <summary>
  /// Prefilters an image buffer in-place using parallel processing for large images.
  /// </summary>
  /// <param name="buffer">The image buffer to prefilter (4 floats per pixel).</param>
  /// <param name="width">Width of the image.</param>
  /// <param name="height">Height of the image.</param>
  /// <param name="prefilter">The prefilter parameters.</param>
  public static unsafe void PrefilterImageParallel(
    float[] buffer,
    int width,
    int height,
    in PrefilterInfo prefilter) {
    var alphas = prefilter.Alpha;
    var scale2D = prefilter.Scale * prefilter.Scale;
    var minRowsForParallel = Math.Max(100, Environment.ProcessorCount * 5);

    fixed (float* bufferPointer = buffer.AsSpan()) {
      foreach (var alpha in alphas) {
        // Horizontal pass: filter each row (parallel for large images)
        if (height >= minRowsForParallel)
          PrefilterRowsParallel(bufferPointer, width, height, alpha);
        else
          PrefilterRows(bufferPointer, width, height, alpha);

        // Vertical pass: filter each column (sequential - parallelizing columns has too much overhead)
        PrefilterColumns(bufferPointer, width, height, alpha);
      }

      // Apply 2D scale factor
      ApplyScale(bufferPointer, buffer.Length, scale2D);
    }
  }

  #region Row Filtering

  /// <summary>
  /// Applies recursive filtering to a range of rows.
  /// </summary>
  /// <param name="buffer">The buffer to filter.</param>
  /// <param name="width">Width of the image.</param>
  /// <param name="y1">Start row (inclusive).</param>
  /// <param name="y2">End row (exclusive).</param>
  /// <param name="alpha">The pole coefficient.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void PrefilterRowsRange(
    float* buffer,
    int width,
    int y1,
    int y2,
    float alpha) {
    const int componentsPerPixel = 4;
    var rowStride = width * componentsPerPixel;

    for (var y = y1; y < y2; ++y) {
      var row = buffer + y * rowStride;
      PrefilterScan(row, width, alpha);
    }
  }

  /// <summary>
  /// Applies recursive filtering to all rows (sequential).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void PrefilterRows(
    float* buffer,
    int width,
    int height,
    float alpha)
    => PrefilterRowsRange(buffer, width, 0, height, alpha);

  /// <summary>
  /// Applies recursive filtering to all rows in parallel using Partitioner.
  /// </summary>
  private static unsafe void PrefilterRowsParallel(
    float* buffer,
    int width,
    int height,
    float alpha) {
    var partitioner = Partitioner.Create(0, height);
    Parallel.ForEach(partitioner, range => {
      PrefilterRowsRange(buffer, width, range.Item1, range.Item2, alpha);
    });
  }

  #endregion

  #region Column Filtering

  /// <summary>
  /// Applies recursive filtering to all columns.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void PrefilterColumns(
    float* buffer,
    int width,
    int height,
    float alpha) {
    const int componentsPerPixel = 4;
    var rowStride = width * componentsPerPixel;

    // Use a temporary column buffer for better cache performance (reused for each column)
    var column = stackalloc float[height * componentsPerPixel];

    for (var x = 0; x < width; ++x) {
      var pixelOffset = x * componentsPerPixel;

      // Extract column (all 4 components per pixel)
      for (var y = 0; y < height; ++y) {
        var srcIdx = y * rowStride + pixelOffset;
        var dstIdx = y * componentsPerPixel;
        column[dstIdx] = buffer[srcIdx];
        column[dstIdx + 1] = buffer[srcIdx + 1];
        column[dstIdx + 2] = buffer[srcIdx + 2];
        column[dstIdx + 3] = buffer[srcIdx + 3];
      }

      // Filter column
      PrefilterScan(column, height, alpha);

      // Write back
      for (var y = 0; y < height; ++y) {
        var srcIdx = y * componentsPerPixel;
        var dstIdx = y * rowStride + pixelOffset;
        buffer[dstIdx] = column[srcIdx];
        buffer[dstIdx + 1] = column[srcIdx + 1];
        buffer[dstIdx + 2] = column[srcIdx + 2];
        buffer[dstIdx + 3] = column[srcIdx + 3];
      }
    }
  }

  #endregion

  #region Scale Application

  /// <summary>
  /// Applies the scale factor to all elements.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ApplyScale(float* buffer, int bufferLength, float scale) {
    for (var i = 0; i < bufferLength; ++i)
      buffer[i] *= scale;
  }

  #endregion

  #region Core IIR Filter

  /// <summary>
  /// Applies a single first-order IIR filter (causal + anti-causal) to a 1D scan of pixels.
  /// </summary>
  /// <param name="scan">The data to filter (modified in-place, 4 components per pixel).</param>
  /// <param name="pixelCount">Number of pixels in the scan.</param>
  /// <param name="alpha">The pole coefficient.</param>
  /// <remarks>
  /// <para>
  /// The algorithm:
  /// <list type="number">
  /// <item>Initialize boundary using mirror-symmetric extension</item>
  /// <item>Causal pass: y[n] = x[n] + α·y[n-1]</item>
  /// <item>Anti-causal pass: y[n] = α·(y[n+1] - y[n])</item>
  /// </list>
  /// </para>
  /// <para>
  /// Uses half-sample symmetric boundaries: f[-1] = f[0], f[n] = f[n-1].
  /// </para>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void PrefilterScan(float* scan, int pixelCount, float alpha) {
    const int c = 4; // components per pixel
    if (pixelCount < 2)
      return;

    // Compute horizon based on tolerance and alpha
    var horizon = ComputeHorizon(alpha);

    // Process each component separately
    for (var comp = 0; comp < c; ++comp) {
      // Causal initialization
      var yPrev = InitializeCausalComponent(scan, pixelCount, comp, alpha, horizon);

      // Causal pass: y[n] = x[n] + α·y[n-1]
      for (var i = 0; i < pixelCount; ++i) {
        var idx = i * c + comp;
        var y = scan[idx] + alpha * yPrev;
        scan[idx] = y;
        yPrev = y;
      }

      // Anti-causal initialization
      var yNext = InitializeAntiCausalComponent(scan, pixelCount, comp, alpha);

      // Anti-causal pass: y[n] = α·(y[n+1] - y[n])
      for (var i = pixelCount - 1; i >= 0; --i) {
        var idx = i * c + comp;
        var y = alpha * (yNext - scan[idx]);
        scan[idx] = y;
        yNext = y;
      }
    }
  }

  /// <summary>
  /// Computes the horizon (number of terms) for boundary initialization.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int ComputeHorizon(float alpha) {
    // horizon = ceil(log(tolerance) / log(|alpha|))
    var absAlpha = MathF.Abs(alpha);
    if (absAlpha < 1e-10f)
      return 1;

    return (int)MathF.Ceiling(MathF.Log(TOLERANCE) / MathF.Log(absAlpha));
  }

  /// <summary>
  /// Initializes the causal filter for a single component using half-sample symmetric extension.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float InitializeCausalComponent(
    float* scan,
    int pixelCount,
    int comp,
    float alpha,
    int horizon) {
    const int c = 4;

    // Short signal: use closed-form solution
    if (pixelCount < horizon) {
      var sum = scan[comp];
      var alphaPow = alpha;

      for (var k = 1; k < pixelCount; ++k) {
        sum += scan[k * c + comp] * alphaPow;
        alphaPow *= alpha;
      }

      // Mirror: reflect and continue
      for (var k = pixelCount - 2; k >= 0; --k) {
        sum += scan[k * c + comp] * alphaPow;
        alphaPow *= alpha;
      }

      // Scale by 1/(1 - α^(2n))
      var scale = 1f / (1f - alphaPow * alphaPow);
      return sum * scale;
    }

    // Long signal: truncate at horizon
    var result = scan[comp];
    var power = alpha;
    for (var k = 1; k < horizon; ++k) {
      result += scan[k * c + comp] * power;
      power *= alpha;
    }

    return result;
  }

  /// <summary>
  /// Initializes the anti-causal filter for a single component.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe float InitializeAntiCausalComponent(
    float* scan,
    int pixelCount,
    int comp,
    float alpha) {
    const int c = 4;
    var scale = alpha / (alpha - 1f);
    return scan[(pixelCount - 1) * c + comp] * scale;
  }

  #endregion
}
