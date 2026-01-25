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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Energy calculation modes for seam carving.
/// </summary>
public enum SeamCarvingEnergyMode {
  /// <summary>Simple gradient magnitude using absolute color differences.</summary>
  Gradient,
  /// <summary>Sobel operator for gradient magnitude (better edge detection).</summary>
  Sobel,
  /// <summary>Forward energy considers the cost of seam insertion (best quality).</summary>
  Forward
}

/// <summary>
/// Seam Carving content-aware image resizing by Avidan and Shamir.
/// </summary>
/// <remarks>
/// <para>Seam Carving Reference: Avidan and Shamir 2007</para>
/// <para>Paper: "Seam Carving for Content-Aware Image Resizing"</para>
/// <para>Algorithm:</para>
/// <list type="number">
/// <item>Compute energy map using gradient magnitude (Sobel, Gradient, or Forward Energy)</item>
/// <item>Use dynamic programming to find minimum energy vertical/horizontal seams</item>
/// <item>For upscaling: Insert seams (duplicate low-energy paths) to expand image</item>
/// <item>For downscaling: Remove seams (delete low-energy paths) to shrink image</item>
/// </list>
/// <para>Energy Functions:</para>
/// <list type="bullet">
/// <item>Gradient: Simple absolute difference (fastest)</item>
/// <item>Sobel: Sobel operator gradient magnitude (balanced)</item>
/// <item>Forward: Forward energy considers insertion cost (best quality, slowest)</item>
/// </list>
/// <para>
/// Energy is computed using the provided <typeparamref name="TMetric"/> in key space,
/// allowing customization of how color differences are measured.
/// </para>
/// </remarks>
[ScalerInfo("SeamCarving", Author = "Avidan, Shamir", Year = 2007,
  Description = "Content-aware seam carving resizer", Category = ScalerCategory.ContentAware)]
public readonly struct SeamCarving : IContentAwareResampler {

  private readonly SeamCarvingEnergyMode _energyMode;

  /// <summary>
  /// Creates a SeamCarving resampler with default parameters (Gradient energy mode).
  /// </summary>
  public SeamCarving() : this(SeamCarvingEnergyMode.Gradient) { }

  /// <summary>
  /// Creates a SeamCarving resampler with the specified energy mode.
  /// </summary>
  /// <param name="energyMode">The energy calculation mode.</param>
  public SeamCarving(SeamCarvingEnergyMode energyMode) => this._energyMode = energyMode;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp, TResult>(
    IContentAwareKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TMetric : struct, IColorMetric<TKey>
    where TLerp : struct, ILerp<TWork>
    => callback.Invoke(new SeamCarvingKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TMetric, TLerp>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._energyMode));

  /// <summary>
  /// Gets the default configuration (Gradient energy mode).
  /// </summary>
  public static SeamCarving Default => new();

  /// <summary>
  /// Gets a configuration using Sobel energy (better edge detection).
  /// </summary>
  public static SeamCarving Sobel => new(SeamCarvingEnergyMode.Sobel);

  /// <summary>
  /// Gets a configuration using Forward energy (best quality).
  /// </summary>
  public static SeamCarving Forward => new(SeamCarvingEnergyMode.Forward);
}

file readonly struct SeamCarvingKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TMetric, TLerp>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, SeamCarvingEnergyMode energyMode)
  : IContentAwareKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TMetric, TLerp>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork> {

  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  private const float SEAM_PENALTY = 1e9f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resize(
    TPixel* source,
    int sourceStride,
    TPixel* dest,
    int destStride,
    in TDecode decoder,
    in TProject projector,
    in TEncode encoder,
    in TMetric metric,
    in TLerp lerp) {
    // Copy source pixels to working buffer
    var pixels = new TWork[sourceWidth * sourceHeight];
    var keys = new TKey[sourceWidth * sourceHeight];

    for (var y = 0; y < sourceHeight; ++y)
    for (var x = 0; x < sourceWidth; ++x) {
      var idx = y * sourceWidth + x;
      var work = decoder.Decode(source[y * sourceStride + x]);
      pixels[idx] = work;
      keys[idx] = projector.Project(work);
    }

    // Scale horizontally by inserting/removing vertical seams
    var horizontallyScaled = targetWidth >= sourceWidth
      ? InsertVerticalSeams(pixels, keys, sourceWidth, sourceHeight, targetWidth, metric, lerp, projector)
      : RemoveVerticalSeams(pixels, keys, sourceWidth, sourceHeight, targetWidth, metric);

    // Scale vertically by inserting/removing horizontal seams
    var fullyScaled = targetHeight >= horizontallyScaled.height
      ? InsertHorizontalSeams(horizontallyScaled.pixels, horizontallyScaled.keys, horizontallyScaled.width, horizontallyScaled.height, targetHeight, metric, lerp, projector)
      : RemoveHorizontalSeams(horizontallyScaled.pixels, horizontallyScaled.keys, horizontallyScaled.width, horizontallyScaled.height, targetHeight, metric);

    // Write result to destination
    for (var y = 0; y < targetHeight; ++y)
    for (var x = 0; x < targetWidth; ++x)
      dest[y * destStride + x] = encoder.Encode(fullyScaled.pixels[y * targetWidth + x]);
  }

  #region Vertical Seam Operations

  private (TWork[] pixels, TKey[] keys, int width, int height) InsertVerticalSeams(
    TWork[] source,
    TKey[] sourceKeys,
    int width,
    int height,
    int targetWidth,
    in TMetric metric,
    in TLerp lerp,
    in TProject projector) {
    if (targetWidth <= width)
      return (source, sourceKeys, width, height);

    var seamsToInsert = targetWidth - width;
    var result = source;
    var resultKeys = sourceKeys;
    var currentWidth = width;

    // Insert seams in passes, each pass finds up to currentWidth seams
    while (seamsToInsert > 0) {
      var seamsThisPass = Math.Min(seamsToInsert, currentWidth);

      // Find all seams for this pass
      var seamsToFind = new int[seamsThisPass][];
      var seamMask = new bool[currentWidth * height];

      for (var i = 0; i < seamsThisPass; ++i) {
        var energy = ComputeEnergyMap(resultKeys, currentWidth, height, metric, seamMask);
        var seam = FindVerticalSeam(energy, currentWidth, height);
        seamsToFind[i] = seam;

        // Mark this seam in the mask
        for (var y = 0; y < height; ++y)
          seamMask[y * currentWidth + seam[y]] = true;
      }

      // Insert all seams at once
      var newWidth = currentWidth + seamsThisPass;
      var newPixels = new TWork[newWidth * height];
      var newKeys = new TKey[newWidth * height];

      for (var y = 0; y < height; ++y) {
        // Collect seam x-positions for this row and sort them
        var seamPositions = new int[seamsThisPass];
        for (var i = 0; i < seamsThisPass; ++i)
          seamPositions[i] = seamsToFind[i][y];
        Array.Sort(seamPositions);

        // Insert pixels with seam duplications
        var srcX = 0;
        var dstX = 0;
        var seamIdx = 0;

        while (srcX < currentWidth) {
          newPixels[y * newWidth + dstX] = result[y * currentWidth + srcX];
          newKeys[y * newWidth + dstX] = resultKeys[y * currentWidth + srcX];
          ++dstX;

          // Check if this position has a seam to duplicate
          while (seamIdx < seamsThisPass && srcX == seamPositions[seamIdx]) {
            // Insert averaged pixel (blend with neighbor for smoother result)
            var left = result[y * currentWidth + srcX];
            var rightX = Math.Min(srcX + 1, currentWidth - 1);
            var right = result[y * currentWidth + rightX];
            var blended = lerp.Lerp(in left, in right);
            newPixels[y * newWidth + dstX] = blended;
            newKeys[y * newWidth + dstX] = projector.Project(blended);
            ++dstX;
            ++seamIdx;
          }

          ++srcX;
        }
      }

      result = newPixels;
      resultKeys = newKeys;
      currentWidth = newWidth;
      seamsToInsert -= seamsThisPass;
    }

    return (result, resultKeys, currentWidth, height);
  }

  private (TWork[] pixels, TKey[] keys, int width, int height) RemoveVerticalSeams(
    TWork[] source,
    TKey[] sourceKeys,
    int width,
    int height,
    int targetWidth,
    in TMetric metric) {
    if (targetWidth >= width)
      return (source, sourceKeys, width, height);

    var seamsToRemove = width - targetWidth;
    var result = source;
    var resultKeys = sourceKeys;
    var currentWidth = width;

    for (var i = 0; i < seamsToRemove; ++i) {
      var energy = ComputeEnergyMap(resultKeys, currentWidth, height, metric, null);
      var seam = FindVerticalSeam(energy, currentWidth, height);

      // Remove the seam
      var newWidth = currentWidth - 1;
      var newPixels = new TWork[newWidth * height];
      var newKeys = new TKey[newWidth * height];

      for (var y = 0; y < height; ++y) {
        var seamX = seam[y];
        var dstX = 0;
        for (var x = 0; x < currentWidth; ++x) {
          if (x != seamX) {
            newPixels[y * newWidth + dstX] = result[y * currentWidth + x];
            newKeys[y * newWidth + dstX] = resultKeys[y * currentWidth + x];
            ++dstX;
          }
        }
      }

      result = newPixels;
      resultKeys = newKeys;
      currentWidth = newWidth;
    }

    return (result, resultKeys, currentWidth, height);
  }

  #endregion

  #region Horizontal Seam Operations

  private (TWork[] pixels, TKey[] keys, int width, int height) InsertHorizontalSeams(
    TWork[] source,
    TKey[] sourceKeys,
    int width,
    int height,
    int targetHeight,
    in TMetric metric,
    in TLerp lerp,
    in TProject projector) {
    if (targetHeight <= height)
      return (source, sourceKeys, width, height);

    // Transpose, insert vertical seams, transpose back
    var transposed = Transpose(source, width, height);
    var transposedKeys = Transpose(sourceKeys, width, height);
    var expanded = InsertVerticalSeams(transposed, transposedKeys, height, width, targetHeight, metric, lerp, projector);
    var result = Transpose(expanded.pixels, expanded.width, expanded.height);
    var resultKeys = Transpose(expanded.keys, expanded.width, expanded.height);

    return (result, resultKeys, expanded.height, expanded.width);
  }

  private (TWork[] pixels, TKey[] keys, int width, int height) RemoveHorizontalSeams(
    TWork[] source,
    TKey[] sourceKeys,
    int width,
    int height,
    int targetHeight,
    in TMetric metric) {
    if (targetHeight >= height)
      return (source, sourceKeys, width, height);

    // Transpose, remove vertical seams, transpose back
    var transposed = Transpose(source, width, height);
    var transposedKeys = Transpose(sourceKeys, width, height);
    var reduced = RemoveVerticalSeams(transposed, transposedKeys, height, width, targetHeight, metric);
    var result = Transpose(reduced.pixels, reduced.width, reduced.height);
    var resultKeys = Transpose(reduced.keys, reduced.width, reduced.height);

    return (result, resultKeys, reduced.height, reduced.width);
  }

  private static T[] Transpose<T>(T[] source, int width, int height) {
    var result = new T[width * height];
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x)
      result[x * height + y] = source[y * width + x];
    return result;
  }

  #endregion

  #region Energy Computation

  private float[] ComputeEnergyMap(TKey[] keys, int width, int height, in TMetric metric, bool[]? seamMask) {
    var energy = new float[width * height];

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var idx = y * width + x;

      if (seamMask != null && seamMask[idx]) {
        energy[idx] = SEAM_PENALTY;
        continue;
      }

      energy[idx] = energyMode switch {
        SeamCarvingEnergyMode.Sobel => ComputeSobelEnergy(keys, width, height, x, y, metric),
        SeamCarvingEnergyMode.Forward => ComputeForwardEnergy(keys, width, height, x, y, metric),
        _ => ComputeGradientEnergy(keys, width, height, x, y, metric)
      };
    }

    return energy;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeGradientEnergy(TKey[] keys, int width, int height, int x, int y, in TMetric metric) {
    var idx = y * width + x;
    var center = keys[idx];

    var left = x > 0 ? keys[idx - 1] : center;
    var right = x < width - 1 ? keys[idx + 1] : center;
    var up = y > 0 ? keys[(y - 1) * width + x] : center;
    var down = y < height - 1 ? keys[(y + 1) * width + x] : center;

    var dxEnergy = (float)metric.Distance(in left, in right);
    var dyEnergy = (float)metric.Distance(in up, in down);

    return dxEnergy + dyEnergy;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeSobelEnergy(TKey[] keys, int width, int height, int x, int y, in TMetric metric) {
    var x0 = Math.Max(0, x - 1);
    var x2 = Math.Min(width - 1, x + 1);
    var y0 = Math.Max(0, y - 1);
    var y2 = Math.Min(height - 1, y + 1);

    // Get 3x3 neighborhood in key space
    var p00 = keys[y0 * width + x0];
    var p10 = keys[y0 * width + x];
    var p20 = keys[y0 * width + x2];
    var p01 = keys[y * width + x0];
    var p21 = keys[y * width + x2];
    var p02 = keys[y2 * width + x0];
    var p12 = keys[y2 * width + x];
    var p22 = keys[y2 * width + x2];

    // Compute Sobel gradients using metric distances
    // Horizontal gradient: -1 0 +1 / -2 0 +2 / -1 0 +1
    var gxTop = (float)metric.Distance(in p00, in p20);
    var gxMid = (float)metric.Distance(in p01, in p21) * 2f;
    var gxBot = (float)metric.Distance(in p02, in p22);

    // Vertical gradient: -1 -2 -1 / 0 0 0 / +1 +2 +1
    var gyLeft = (float)metric.Distance(in p00, in p02);
    var gyMid = (float)metric.Distance(in p10, in p12) * 2f;
    var gyRight = (float)metric.Distance(in p20, in p22);

    var gx = gxTop + gxMid + gxBot;
    var gy = gyLeft + gyMid + gyRight;

    return MathF.Sqrt(gx * gx + gy * gy);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float ComputeForwardEnergy(TKey[] keys, int width, int height, int x, int y, in TMetric metric) {
    var idx = y * width + x;
    var center = keys[idx];

    var left = x > 0 ? keys[idx - 1] : center;
    var right = x < width - 1 ? keys[idx + 1] : center;
    var up = y > 0 ? keys[(y - 1) * width + x] : center;

    var cU = (float)metric.Distance(in left, in right);
    var cL = cU + (float)metric.Distance(in up, in left);
    var cR = cU + (float)metric.Distance(in up, in right);

    return Math.Min(cU, Math.Min(cL, cR));
  }

  #endregion

  #region Seam Finding

  private static int[] FindVerticalSeam(float[] energy, int width, int height) {
    var cumulative = new float[width * height];
    var backtrack = new int[width * height];

    // First row: cumulative = energy
    for (var x = 0; x < width; ++x)
      cumulative[x] = energy[x];

    // Build cumulative map
    for (var y = 1; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var idx = y * width + x;
      var upIdx = (y - 1) * width + x;

      var minEnergy = cumulative[upIdx];
      var minX = x;

      if (x > 0 && cumulative[upIdx - 1] < minEnergy) {
        minEnergy = cumulative[upIdx - 1];
        minX = x - 1;
      }

      if (x < width - 1 && cumulative[upIdx + 1] < minEnergy) {
        minEnergy = cumulative[upIdx + 1];
        minX = x + 1;
      }

      cumulative[idx] = energy[idx] + minEnergy;
      backtrack[idx] = minX;
    }

    // Find minimum in last row
    var seam = new int[height];
    var lastRow = (height - 1) * width;
    var minIdx = 0;
    var minVal = cumulative[lastRow];

    for (var x = 1; x < width; ++x) {
      if (cumulative[lastRow + x] < minVal) {
        minVal = cumulative[lastRow + x];
        minIdx = x;
      }
    }

    seam[height - 1] = minIdx;

    // Backtrack to find the seam
    for (var y = height - 2; y >= 0; --y)
      seam[y] = backtrack[(y + 1) * width + seam[y + 1]];

    return seam;
  }

  #endregion
}
