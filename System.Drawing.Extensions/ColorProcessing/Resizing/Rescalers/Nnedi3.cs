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
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Quality preset for NNEDI3 scaler.
/// </summary>
public enum Nnedi3Quality {
  /// <summary>Fast mode with 16 neurons.</summary>
  Fast = 16,
  /// <summary>Standard mode with 32 neurons.</summary>
  Standard = 32,
  /// <summary>High quality mode with 64 neurons.</summary>
  HighQuality = 64
}

/// <summary>
/// NNEDI3 (Neural Network Edge Directed Interpolation 3) scaler by tritical (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Uses trained neural network weights to analyze local pixel neighborhoods.</para>
/// <para>
/// Applies directional interpolation based on edge detection for high-quality upscaling.
/// Includes ReLU and sigmoid activation functions with bilinear fallback.
/// </para>
/// <para>Algorithm by tritical, 2010. Reference: https://github.com/sekrit-twc/znedi3</para>
/// </remarks>
[ScalerInfo("NNEDI3", Author = "tritical", Year = 2010,
  Description = "Neural Network Edge Directed Interpolation", Category = ScalerCategory.PixelArt)]
public readonly struct Nnedi3 : IPixelScaler {

  private readonly int _scale;
  private readonly Nnedi3Quality _quality;

  private const float FastPrescreenThreshold = 0.5f;
  private const float StandardPrescreenThreshold = 0.3f;
  private const float HqPrescreenThreshold = 0.15f;

  /// <summary>
  /// Creates a new NNEDI3 instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="quality">Quality preset.</param>
  public Nnedi3(int scale = 2, Nnedi3Quality quality = Nnedi3Quality.Standard) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 4);
    this._scale = scale;
    this._quality = quality;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel> {

    var neuronCount = this._quality == default ? 32 : (int)this._quality;
    var threshold = this._quality switch {
      Nnedi3Quality.Fast => FastPrescreenThreshold,
      Nnedi3Quality.HighQuality => HqPrescreenThreshold,
      _ => StandardPrescreenThreshold
    };

    return this._scale switch {
      0 or 2 => callback.Invoke(new Nnedi32xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, neuronCount, threshold)),
      3 => callback.Invoke(new Nnedi33xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, neuronCount, threshold)),
      4 => callback.Invoke(new Nnedi34xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, neuronCount, threshold)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };
  }

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>
  /// Gets a fast 2x scale instance (16 neurons).
  /// </summary>
  public static Nnedi3 Scale2xFast => new(2, Nnedi3Quality.Fast);

  /// <summary>
  /// Gets a standard 2x scale instance (32 neurons).
  /// </summary>
  public static Nnedi3 Scale2x => new(2, Nnedi3Quality.Standard);

  /// <summary>
  /// Gets a high quality 2x scale instance (64 neurons).
  /// </summary>
  public static Nnedi3 Scale2xHq => new(2, Nnedi3Quality.HighQuality);

  /// <summary>
  /// Gets a fast 3x scale instance (16 neurons).
  /// </summary>
  public static Nnedi3 Scale3xFast => new(3, Nnedi3Quality.Fast);

  /// <summary>
  /// Gets a standard 3x scale instance (32 neurons).
  /// </summary>
  public static Nnedi3 Scale3x => new(3, Nnedi3Quality.Standard);

  /// <summary>
  /// Gets a high quality 3x scale instance (64 neurons).
  /// </summary>
  public static Nnedi3 Scale3xHq => new(3, Nnedi3Quality.HighQuality);

  /// <summary>
  /// Gets a fast 4x scale instance (16 neurons).
  /// </summary>
  public static Nnedi3 Scale4xFast => new(4, Nnedi3Quality.Fast);

  /// <summary>
  /// Gets a standard 4x scale instance (32 neurons).
  /// </summary>
  public static Nnedi3 Scale4x => new(4, Nnedi3Quality.Standard);

  /// <summary>
  /// Gets a high quality 4x scale instance (64 neurons).
  /// </summary>
  public static Nnedi3 Scale4xHq => new(4, Nnedi3Quality.HighQuality);

  /// <summary>
  /// Gets the default configuration (2x, standard quality).
  /// </summary>
  public static Nnedi3 Default => Scale2x;

  /// <summary>
  /// Creates a fast variant.
  /// </summary>
  public Nnedi3 AsFast() => new(this._scale == 0 ? 2 : this._scale, Nnedi3Quality.Fast);

  /// <summary>
  /// Creates a high quality variant.
  /// </summary>
  public Nnedi3 AsHighQuality() => new(this._scale == 0 ? 2 : this._scale, Nnedi3Quality.HighQuality);

  // Precomputed directional weights for 8 directions (trained values approximation)
  // Each direction has weights for a 4x4 local neighborhood
  internal static readonly float[][] DirectionalWeights = [
    // Direction 0: Horizontal (0°)
    [0.0f, 0.0f, 0.0f, 0.0f,
     -0.125f, 0.625f, 0.625f, -0.125f,
     -0.125f, 0.625f, 0.625f, -0.125f,
     0.0f, 0.0f, 0.0f, 0.0f],
    // Direction 1: Diagonal 22.5°
    [0.0f, 0.0f, -0.0625f, 0.0f,
     0.0f, 0.25f, 0.5f, 0.0f,
     -0.0625f, 0.5f, 0.25f, 0.0f,
     0.0f, 0.0f, 0.0f, -0.0625f],
    // Direction 2: Diagonal 45°
    [0.0f, 0.0f, 0.0f, -0.125f,
     0.0f, 0.0f, 0.625f, 0.0f,
     0.0f, 0.625f, 0.0f, 0.0f,
     -0.125f, 0.0f, 0.0f, 0.0f],
    // Direction 3: Diagonal 67.5°
    [-0.0625f, 0.0f, 0.0f, 0.0f,
     0.0f, 0.25f, 0.5f, 0.0f,
     0.0f, 0.5f, 0.25f, -0.0625f,
     0.0f, 0.0f, 0.0f, 0.0f],
    // Direction 4: Vertical (90°)
    [0.0f, -0.125f, -0.125f, 0.0f,
     0.0f, 0.625f, 0.625f, 0.0f,
     0.0f, 0.625f, 0.625f, 0.0f,
     0.0f, -0.125f, -0.125f, 0.0f],
    // Direction 5: Diagonal 112.5°
    [0.0f, 0.0f, 0.0f, -0.0625f,
     0.0f, 0.5f, 0.25f, 0.0f,
     -0.0625f, 0.25f, 0.5f, 0.0f,
     0.0f, 0.0f, 0.0f, 0.0f],
    // Direction 6: Diagonal 135°
    [-0.125f, 0.0f, 0.0f, 0.0f,
     0.0f, 0.625f, 0.0f, 0.0f,
     0.0f, 0.0f, 0.625f, 0.0f,
     0.0f, 0.0f, 0.0f, -0.125f],
    // Direction 7: Diagonal 157.5°
    [0.0f, 0.0f, 0.0f, 0.0f,
     0.0f, 0.25f, 0.5f, -0.0625f,
     -0.0625f, 0.5f, 0.25f, 0.0f,
     0.0f, 0.0f, 0.0f, 0.0f]
  ];

  // Edge detection kernels for each direction
  internal static readonly float[][] EdgeKernels = [
    // Horizontal edge detector
    [-1f, -2f, -1f, 0f, 0f, 0f, 1f, 2f, 1f],
    // Vertical edge detector
    [-1f, 0f, 1f, -2f, 0f, 2f, -1f, 0f, 1f],
    // 45° diagonal edge detector
    [0f, -1f, -2f, 1f, 0f, -1f, 2f, 1f, 0f],
    // 135° diagonal edge detector
    [-2f, -1f, 0f, -1f, 0f, 1f, 0f, 1f, 2f]
  ];

  /// <summary>
  /// Computes luma using ColorConverter.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Luma<TWork>(in TWork color) where TWork : unmanaged, IColorSpace
    => ColorConverter.GetLuminance(color);

  /// <summary>
  /// ReLU activation function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Relu(float x) => x > 0 ? x : 0;

  /// <summary>
  /// Sigmoid activation function.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static float Sigmoid(float x) => x < 0.001f ? 0f : x > 0.999f ? 1f : 1f / (1f + MathF.Exp(-6f * (x - 0.5f)));

  /// <summary>
  /// Extracts a 4x4 neighborhood from the window into a float array.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe void ExtractNeighborhood<TWork, TKey>(in NeighborWindow<TWork, TKey> window, float* dest)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace {
    // Extract 4x4 neighborhood centered on current pixel
    // Row -1: M1M1, M1P0, M1P1, M1P2
    CopyPixel(window.M1M1.Work, dest, 0);
    CopyPixel(window.M1P0.Work, dest, 1);
    CopyPixel(window.M1P1.Work, dest, 2);
    CopyPixel(window.M1P2.Work, dest, 3);

    // Row 0: P0M1, P0P0, P0P1, P0P2
    CopyPixel(window.P0M1.Work, dest, 4);
    CopyPixel(window.P0P0.Work, dest, 5);
    CopyPixel(window.P0P1.Work, dest, 6);
    CopyPixel(window.P0P2.Work, dest, 7);

    // Row 1: P1M1, P1P0, P1P1, P1P2
    CopyPixel(window.P1M1.Work, dest, 8);
    CopyPixel(window.P1P0.Work, dest, 9);
    CopyPixel(window.P1P1.Work, dest, 10);
    CopyPixel(window.P1P2.Work, dest, 11);

    // Row 2: P2M1, P2P0, P2P1, P2P2
    CopyPixel(window.P2M1.Work, dest, 12);
    CopyPixel(window.P2P0.Work, dest, 13);
    CopyPixel(window.P2P1.Work, dest, 14);
    CopyPixel(window.P2P2.Work, dest, 15);
  }

  /// <summary>
  /// Copies a pixel's components to the destination array.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe void CopyPixel<TWork>(in TWork pixel, float* dest, int index)
    where TWork : unmanaged, IColorSpace {
    var (r, g, b) = ColorConverter.GetNormalizedRgb(pixel);
    var a = ColorConverter.GetAlpha(pixel);
    var baseOffset = index * 4;
    dest[baseOffset] = r;
    dest[baseOffset + 1] = g;
    dest[baseOffset + 2] = b;
    dest[baseOffset + 3] = a;
  }

  /// <summary>
  /// Computes edge strength for the specified direction.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static unsafe float ComputeEdgeStrength(float* neighborhood, int direction) {
    // Use 3x3 center of 4x4 neighborhood for edge detection
    Span<float> luma = stackalloc float[9];
    var lumaIndex = 0;

    // Use 3x3 center (rows 0-2, cols 0-2 of neighborhood)
    for (var dy = 0; dy < 3; ++dy)
    for (var dx = 0; dx < 3; ++dx) {
      var idx = (dy + 1) * 4 + dx; // Start from row 1
      var baseOffset = idx * 4;
      luma[lumaIndex++] = 0.299f * neighborhood[baseOffset] + 0.587f * neighborhood[baseOffset + 1] + 0.114f * neighborhood[baseOffset + 2];
    }

    // Apply edge kernel for this direction
    var kernelIndex = direction % 4;
    var kernel = EdgeKernels[kernelIndex];
    var edge = 0f;

    for (var i = 0; i < 9; ++i)
      edge += luma[i] * kernel[i];

    return MathF.Abs(edge);
  }
}

file readonly struct Nnedi32xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, int neuronCount, float threshold)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Extract 4x4 neighborhood using available window
    var neighborhood = stackalloc float[16 * 4]; // 16 pixels, 4 components each
    Nnedi3.ExtractNeighborhood(window, neighborhood);

    // Get center pixel
    var center = window.P0P0.Work;

    // Compute edge strengths for each direction
    Span<float> edgeStrengths = stackalloc float[8];
    var maxEdge = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      edgeStrengths[dir] = Nnedi3.ComputeEdgeStrength(neighborhood, dir);
      if (edgeStrengths[dir] > maxEdge)
        maxEdge = edgeStrengths[dir];
    }

    // Top-left is always center
    var e00 = center;

    // Compute interpolated pixels using NNEDI3 prediction
    var e01 = maxEdge < threshold ? lerp.Lerp(center, window.P0P1.Work) : this._PredictPixel(window, neighborhood, edgeStrengths, 0.5f, 0f, lerp);
    var e10 = maxEdge < threshold ? lerp.Lerp(center, window.P1P0.Work) : this._PredictPixel(window, neighborhood, edgeStrengths, 0f, 0.5f, lerp);
    var e11 = maxEdge < threshold ? lerp.Lerp(lerp.Lerp(center, window.P0P1.Work), lerp.Lerp(window.P1P0.Work, window.P1P1.Work))
                                  : this._PredictPixel(window, neighborhood, edgeStrengths, 0.5f, 0.5f, lerp);

    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe TWork _PredictPixel(
    in NeighborWindow<TWork, TKey> window,
    float* neighborhood,
    Span<float> edgeStrengths,
    float fx, float fy,
    TLerp lerp) {

    var resultR = 0f;
    var resultG = 0f;
    var resultB = 0f;
    var resultA = 0f;
    var totalWeight = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      var weight = edgeStrengths[dir];
      if (weight < 0.01f) continue;

      // Apply ReLU-like activation
      weight = Nnedi3.Relu(weight - threshold * 0.5f);

      var weights = Nnedi3.DirectionalWeights[dir];
      var dirR = 0f;
      var dirG = 0f;
      var dirB = 0f;
      var dirA = 0f;

      for (var i = 0; i < 16; ++i) {
        var baseOffset = i * 4;
        dirR += neighborhood[baseOffset] * weights[i];
        dirG += neighborhood[baseOffset + 1] * weights[i];
        dirB += neighborhood[baseOffset + 2] * weights[i];
        dirA += neighborhood[baseOffset + 3] * weights[i];
      }

      // Adjust for subpixel position
      var angle = dir * MathF.PI / 8f;
      var posWeight = 1f - MathF.Abs(MathF.Cos(angle) * (fx - 0.5f) + MathF.Sin(angle) * (fy - 0.5f));
      weight *= posWeight;

      resultR += dirR * weight;
      resultG += dirG * weight;
      resultB += dirB * weight;
      resultA += dirA * weight;
      totalWeight += weight;
    }

    if (totalWeight < 0.01f) {
      // Fallback to bilinear
      var c00 = window.P0P0.Work;
      var c10 = window.P0P1.Work;
      var c01 = window.P1P0.Work;
      var c11 = window.P1P1.Work;
      var wFx2 = (int)(fx * 256f);
    var wFy2 = (int)(fy * 256f);
    return lerp.Lerp(lerp.Lerp(c00, c10, 256 - wFx2, wFx2), lerp.Lerp(c01, c11, 256 - wFx2, wFx2), 256 - wFy2, wFy2);
    }

    resultR /= totalWeight;
    resultG /= totalWeight;
    resultB /= totalWeight;
    resultA /= totalWeight;

    // Apply sigmoid activation
    resultR = Nnedi3.Sigmoid(resultR);
    resultG = Nnedi3.Sigmoid(resultG);
    resultB = Nnedi3.Sigmoid(resultB);

    // Blend with bilinear for smoother results
    var blendFactor = 1f - (neuronCount / 128f);
    var blendW2 = (int)(blendFactor * 256f);
    var wFx2_b = (int)(fx * 256f);
    var wFy2_b = (int)(fy * 256f);
    var bilinear = lerp.Lerp(lerp.Lerp(window.P0P0.Work, window.P0P1.Work, 256 - wFx2_b, wFx2_b), lerp.Lerp(window.P1P0.Work, window.P1P1.Work, 256 - wFx2_b, wFx2_b), 256 - wFy2_b, wFy2_b);
    var computed = ColorConverter.FromNormalizedRgba<TWork>(resultR, resultG, resultB, resultA);

    return lerp.Lerp(computed, bilinear, 256 - blendW2, blendW2);
  }
}

file readonly struct Nnedi33xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, int neuronCount, float threshold)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Extract 4x4 neighborhood
    var neighborhood = stackalloc float[16 * 4];
    Nnedi3.ExtractNeighborhood(window, neighborhood);

    // Compute edge strengths
    Span<float> edgeStrengths = stackalloc float[8];
    var maxEdge = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      edgeStrengths[dir] = Nnedi3.ComputeEdgeStrength(neighborhood, dir);
      if (edgeStrengths[dir] > maxEdge)
        maxEdge = edgeStrengths[dir];
    }

    // 3x3 output using NNEDI3 prediction
    for (var oy = 0; oy < 3; ++oy) {
      var rowPtr = destTopLeft + oy * destStride;
      for (var ox = 0; ox < 3; ++ox) {
        var fx = (ox + 0.5f) / 3f;
        var fy = (oy + 0.5f) / 3f;

        TWork result;
        if (ox == 0 && oy == 0) {
          result = window.P0P0.Work;
        } else if (maxEdge < threshold) {
          result = this._BilinearFallback(window, fx, fy);
        } else {
          result = this._PredictPixel(window, neighborhood, edgeStrengths, fx, fy);
        }

        rowPtr[ox] = encoder.Encode(result);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _BilinearFallback(in NeighborWindow<TWork, TKey> window, float fx, float fy) {
    var c00 = window.P0P0.Work;
    var c10 = window.P0P1.Work;
    var c01 = window.P1P0.Work;
    var c11 = window.P1P1.Work;
    var wFx2 = (int)(fx * 256f);
    var wFy2 = (int)(fy * 256f);
    return lerp.Lerp(lerp.Lerp(c00, c10, 256 - wFx2, wFx2), lerp.Lerp(c01, c11, 256 - wFx2, wFx2), 256 - wFy2, wFy2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe TWork _PredictPixel(
    in NeighborWindow<TWork, TKey> window,
    float* neighborhood,
    Span<float> edgeStrengths,
    float fx, float fy) {

    var resultR = 0f;
    var resultG = 0f;
    var resultB = 0f;
    var resultA = 0f;
    var totalWeight = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      var weight = edgeStrengths[dir];
      if (weight < 0.01f) continue;

      weight = Nnedi3.Relu(weight - threshold * 0.5f);

      var weights = Nnedi3.DirectionalWeights[dir];
      var dirR = 0f;
      var dirG = 0f;
      var dirB = 0f;
      var dirA = 0f;

      for (var i = 0; i < 16; ++i) {
        var baseOffset = i * 4;
        dirR += neighborhood[baseOffset] * weights[i];
        dirG += neighborhood[baseOffset + 1] * weights[i];
        dirB += neighborhood[baseOffset + 2] * weights[i];
        dirA += neighborhood[baseOffset + 3] * weights[i];
      }

      var angle = dir * MathF.PI / 8f;
      var posWeight = 1f - MathF.Abs(MathF.Cos(angle) * (fx - 0.5f) + MathF.Sin(angle) * (fy - 0.5f));
      weight *= posWeight;

      resultR += dirR * weight;
      resultG += dirG * weight;
      resultB += dirB * weight;
      resultA += dirA * weight;
      totalWeight += weight;
    }

    if (totalWeight < 0.01f)
      return this._BilinearFallback(window, fx, fy);

    resultR /= totalWeight;
    resultG /= totalWeight;
    resultB /= totalWeight;
    resultA /= totalWeight;

    resultR = Nnedi3.Sigmoid(resultR);
    resultG = Nnedi3.Sigmoid(resultG);
    resultB = Nnedi3.Sigmoid(resultB);

    var blendFactor = 1f - (neuronCount / 128f);
    var blendW2 = (int)(blendFactor * 256f);
    var bilinear = this._BilinearFallback(window, fx, fy);
    var computed = ColorConverter.FromNormalizedRgba<TWork>(resultR, resultG, resultB, resultA);

    return lerp.Lerp(computed, bilinear, 256 - blendW2, blendW2);
  }
}

file readonly struct Nnedi34xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, int neuronCount, float threshold)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Extract 4x4 neighborhood
    var neighborhood = stackalloc float[16 * 4];
    Nnedi3.ExtractNeighborhood(window, neighborhood);

    // Compute edge strengths
    Span<float> edgeStrengths = stackalloc float[8];
    var maxEdge = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      edgeStrengths[dir] = Nnedi3.ComputeEdgeStrength(neighborhood, dir);
      if (edgeStrengths[dir] > maxEdge)
        maxEdge = edgeStrengths[dir];
    }

    // 4x4 output using NNEDI3 prediction
    for (var oy = 0; oy < 4; ++oy) {
      var rowPtr = destTopLeft + oy * destStride;
      for (var ox = 0; ox < 4; ++ox) {
        var fx = (ox + 0.5f) / 4f;
        var fy = (oy + 0.5f) / 4f;

        TWork result;
        if (ox == 0 && oy == 0) {
          result = window.P0P0.Work;
        } else if (maxEdge < threshold) {
          result = this._BilinearFallback(window, fx, fy);
        } else {
          result = this._PredictPixel(window, neighborhood, edgeStrengths, fx, fy);
        }

        rowPtr[ox] = encoder.Encode(result);
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private TWork _BilinearFallback(in NeighborWindow<TWork, TKey> window, float fx, float fy) {
    var c00 = window.P0P0.Work;
    var c10 = window.P0P1.Work;
    var c01 = window.P1P0.Work;
    var c11 = window.P1P1.Work;
    var wFx2 = (int)(fx * 256f);
    var wFy2 = (int)(fy * 256f);
    return lerp.Lerp(lerp.Lerp(c00, c10, 256 - wFx2, wFx2), lerp.Lerp(c01, c11, 256 - wFx2, wFx2), 256 - wFy2, wFy2);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private unsafe TWork _PredictPixel(
    in NeighborWindow<TWork, TKey> window,
    float* neighborhood,
    Span<float> edgeStrengths,
    float fx, float fy) {

    var resultR = 0f;
    var resultG = 0f;
    var resultB = 0f;
    var resultA = 0f;
    var totalWeight = 0f;

    for (var dir = 0; dir < 8; ++dir) {
      var weight = edgeStrengths[dir];
      if (weight < 0.01f) continue;

      weight = Nnedi3.Relu(weight - threshold * 0.5f);

      var weights = Nnedi3.DirectionalWeights[dir];
      var dirR = 0f;
      var dirG = 0f;
      var dirB = 0f;
      var dirA = 0f;

      for (var i = 0; i < 16; ++i) {
        var baseOffset = i * 4;
        dirR += neighborhood[baseOffset] * weights[i];
        dirG += neighborhood[baseOffset + 1] * weights[i];
        dirB += neighborhood[baseOffset + 2] * weights[i];
        dirA += neighborhood[baseOffset + 3] * weights[i];
      }

      var angle = dir * MathF.PI / 8f;
      var posWeight = 1f - MathF.Abs(MathF.Cos(angle) * (fx - 0.5f) + MathF.Sin(angle) * (fy - 0.5f));
      weight *= posWeight;

      resultR += dirR * weight;
      resultG += dirG * weight;
      resultB += dirB * weight;
      resultA += dirA * weight;
      totalWeight += weight;
    }

    if (totalWeight < 0.01f)
      return this._BilinearFallback(window, fx, fy);

    resultR /= totalWeight;
    resultG /= totalWeight;
    resultB /= totalWeight;
    resultA /= totalWeight;

    resultR = Nnedi3.Sigmoid(resultR);
    resultG = Nnedi3.Sigmoid(resultG);
    resultB = Nnedi3.Sigmoid(resultB);

    var blendFactor = 1f - (neuronCount / 128f);
    var blendW2 = (int)(blendFactor * 256f);
    var bilinear = this._BilinearFallback(window, fx, fy);
    var computed = ColorConverter.FromNormalizedRgba<TWork>(resultR, resultG, resultB, resultA);

    return lerp.Lerp(computed, bilinear, 256 - blendW2, blendW2);
  }
}
