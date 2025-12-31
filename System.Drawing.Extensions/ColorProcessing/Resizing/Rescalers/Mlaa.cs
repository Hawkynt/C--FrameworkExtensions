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
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

/// <summary>
/// Defines the MLAA threshold variant.
/// </summary>
public enum MlaaVariant : byte {
  /// <summary>Standard edge detection threshold (balanced).</summary>
  Standard = 0,
  /// <summary>Conservative edge detection (less smoothing).</summary>
  Subtle = 1,
  /// <summary>Sensitive edge detection (more smoothing).</summary>
  Aggressive = 2
}

/// <summary>
/// MLAA (Morphological Anti-Aliasing) - Edge-based pattern detection scaler (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Post-process anti-aliasing that detects and smooths jagged edges.</para>
/// <para>
/// Phase 1: Edge detection to find color discontinuities.
/// Phase 2: Pattern recognition to identify edge shapes (L, Z, U patterns).
/// Phase 3: Blending based on detected patterns to smooth jaggies.
/// </para>
/// <para>Algorithm by Alexander Reshetov (Intel), 2009.</para>
/// </remarks>
[ScalerInfo("MLAA", Author = "Alexander Reshetov", Year = 2009,
  Description = "Morphological Anti-Aliasing", Category = ScalerCategory.PixelArt)]
public readonly struct Mlaa : IPixelScaler {

  private readonly int _scale;
  private readonly MlaaVariant _variant;

  /// <summary>Standard edge detection threshold (balanced).</summary>
  public const float StandardThreshold = 32f * Bgra8888.ByteToNormalized;

  /// <summary>Subtle edge detection threshold (conservative, less smoothing).</summary>
  public const float SubtleThreshold = 48f * Bgra8888.ByteToNormalized;

  /// <summary>Aggressive edge detection threshold (sensitive, more smoothing).</summary>
  public const float AggressiveThreshold = 16f * Bgra8888.ByteToNormalized;

  /// <summary>
  /// Creates a new MLAA instance.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  /// <param name="variant">The threshold variant to use.</param>
  public Mlaa(int scale = 2, MlaaVariant variant = MlaaVariant.Standard) {
    if (scale is not (2 or 3 or 4))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "MLAA supports 2x, 3x, 4x scaling");
    this._scale = scale;
    this._variant = variant;
  }

  /// <summary>
  /// Gets the edge threshold based on variant.
  /// </summary>
  public float Threshold => this._variant switch {
    MlaaVariant.Subtle => SubtleThreshold,
    MlaaVariant.Aggressive => AggressiveThreshold,
    _ => StandardThreshold
  };

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
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new Mlaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Threshold)),
      3 => callback.Invoke(new Mlaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Threshold)),
      4 => callback.Invoke(new Mlaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp, this.Threshold)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

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
  /// Creates a new MLAA with the specified variant.
  /// </summary>
  public Mlaa WithVariant(MlaaVariant variant) => new(this._scale == 0 ? 2 : this._scale, variant);

  /// <summary>
  /// Gets a 2x scale instance.
  /// </summary>
  public static Mlaa Scale2x => new(2);

  /// <summary>
  /// Gets a 3x scale instance.
  /// </summary>
  public static Mlaa Scale3x => new(3);

  /// <summary>
  /// Gets a 4x scale instance.
  /// </summary>
  public static Mlaa Scale4x => new(4);

  /// <summary>
  /// Gets the default configuration (2x, Standard).
  /// </summary>
  public static Mlaa Default => Scale2x;

  /// <summary>
  /// Detects if there is an edge between two colors based on total RGB difference.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static bool IsEdge<TWork>(in TWork a, in TWork b, float threshold) where TWork : unmanaged, IColorSpace {
    var (ar, ag, ab) = ColorConverter.GetNormalizedRgb(a);
    var (br, bg, bb) = ColorConverter.GetNormalizedRgb(b);

    var diff = MathF.Abs(ar - br) + MathF.Abs(ag - bg) + MathF.Abs(ab - bb);
    return diff > threshold * 3f; // Threshold is per-channel, multiply by 3 for total
  }

  /// <summary>
  /// Calculates the blend color based on edge pattern (L, Z, or U shape).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static TWork CalculateEdgeBlend<TWork, TLerp>(
    float pos, float edgeDist, bool edgeA, bool edgeB,
    in TWork sideA, in TWork sideB, in TWork edgeNeighbor, TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // L-shape: edge on one side only
    if (edgeA && !edgeB && pos < 0.5f)
      return lerp.Lerp(edgeNeighbor, sideA, pos);

    if (edgeB && !edgeA && pos > 0.5f)
      return lerp.Lerp(edgeNeighbor, sideB, 1f - pos);

    // Default: use edge neighbor
    return edgeNeighbor;
  }

  /// <summary>
  /// Applies diagonal blending for L and Z patterns.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  internal static void ApplyDiagonalBlending<TWork, TLerp>(
    ref TWork result, float fx, float fy, in TWork center,
    in TWork top, in TWork bottom, in TWork left, in TWork right,
    in TWork topLeft, in TWork topRight, in TWork bottomLeft, in TWork bottomRight,
    float threshold, TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork> {
    // Detect diagonal patterns and apply subtle blending
    var isNwCorner = fx < 0.5f && fy < 0.5f;
    var isNeCorner = fx > 0.5f && fy < 0.5f;
    var isSwCorner = fx < 0.5f && fy > 0.5f;
    var isSeCorner = fx > 0.5f && fy > 0.5f;

    if (isNwCorner) {
      // Check for NW diagonal edge (L-pattern going up-left)
      if (IsEdge(center, top, threshold) && IsEdge(center, left, threshold) && !IsEdge(top, left, threshold)) {
        var diagonalBlend = (0.5f - fx) * (0.5f - fy) * 2f * 0.3f;
        result = lerp.Lerp(result, topLeft, diagonalBlend);
      }
    }

    if (isNeCorner) {
      // Check for NE diagonal edge
      if (IsEdge(center, top, threshold) && IsEdge(center, right, threshold) && !IsEdge(top, right, threshold)) {
        var diagonalBlend = (fx - 0.5f) * (0.5f - fy) * 2f * 0.3f;
        result = lerp.Lerp(result, topRight, diagonalBlend);
      }
    }

    if (isSwCorner) {
      // Check for SW diagonal edge
      if (IsEdge(center, bottom, threshold) && IsEdge(center, left, threshold) && !IsEdge(bottom, left, threshold)) {
        var diagonalBlend = (0.5f - fx) * (fy - 0.5f) * 2f * 0.3f;
        result = lerp.Lerp(result, bottomLeft, diagonalBlend);
      }
    }

    if (isSeCorner) {
      // Check for SE diagonal edge
      if (IsEdge(center, bottom, threshold) && IsEdge(center, right, threshold) && !IsEdge(bottom, right, threshold)) {
        var diagonalBlend = (fx - 0.5f) * (fy - 0.5f) * 2f * 0.3f;
        result = lerp.Lerp(result, bottomRight, diagonalBlend);
      }
    }
  }
}

file readonly struct Mlaa2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float threshold)
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
    const int scale = 2;

    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var topLeft = window.M1M1.Work;
    var topRight = window.M1P1.Work;
    var bottomLeft = window.P1M1.Work;
    var bottomRight = window.P1P1.Work;

    // Detect edges in each direction
    var edgeTop = Mlaa.IsEdge(center, top, threshold);
    var edgeBottom = Mlaa.IsEdge(center, bottom, threshold);
    var edgeLeft = Mlaa.IsEdge(center, left, threshold);
    var edgeRight = Mlaa.IsEdge(center, right, threshold);

    // Process each output pixel in the 2x2 block
    for (var dy = 0; dy < scale; ++dy) {
      var row = destTopLeft + dy * destStride;

      for (var dx = 0; dx < scale; ++dx) {
        // Calculate position within block (0 to 1)
        var fx = (dx + 0.5f) / scale;
        var fy = (dy + 0.5f) / scale;

        var result = center;

        // Apply MLAA blending based on detected edge patterns
        if (edgeTop && fy < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, 0.5f - fy, edgeLeft, edgeRight, left, right, top, lerp);
          result = lerp.Lerp(center, blend, (0.5f - fy) * 2f);
        } else if (edgeBottom && fy > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, fy - 0.5f, edgeLeft, edgeRight, left, right, bottom, lerp);
          result = lerp.Lerp(center, blend, (fy - 0.5f) * 2f);
        }

        if (edgeLeft && fx < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, 0.5f - fx, edgeTop, edgeBottom, top, bottom, left, lerp);
          result = lerp.Lerp(result, blend, (0.5f - fx) * 0.5f);
        } else if (edgeRight && fx > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, fx - 0.5f, edgeTop, edgeBottom, top, bottom, right, lerp);
          result = lerp.Lerp(result, blend, (fx - 0.5f) * 0.5f);
        }

        // Apply diagonal pattern detection for L and Z shapes
        Mlaa.ApplyDiagonalBlending(ref result, fx, fy, center,
          top, bottom, left, right,
          topLeft, topRight, bottomLeft, bottomRight, threshold, lerp);

        row[dx] = encoder.Encode(result);
      }
    }
  }
}

file readonly struct Mlaa3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float threshold)
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
    const int scale = 3;

    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var topLeft = window.M1M1.Work;
    var topRight = window.M1P1.Work;
    var bottomLeft = window.P1M1.Work;
    var bottomRight = window.P1P1.Work;

    // Detect edges in each direction
    var edgeTop = Mlaa.IsEdge(center, top, threshold);
    var edgeBottom = Mlaa.IsEdge(center, bottom, threshold);
    var edgeLeft = Mlaa.IsEdge(center, left, threshold);
    var edgeRight = Mlaa.IsEdge(center, right, threshold);

    // Process each output pixel in the 3x3 block
    for (var dy = 0; dy < scale; ++dy) {
      var row = destTopLeft + dy * destStride;

      for (var dx = 0; dx < scale; ++dx) {
        var fx = (dx + 0.5f) / scale;
        var fy = (dy + 0.5f) / scale;

        var result = center;

        if (edgeTop && fy < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, 0.5f - fy, edgeLeft, edgeRight, left, right, top, lerp);
          result = lerp.Lerp(center, blend, (0.5f - fy) * 2f);
        } else if (edgeBottom && fy > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, fy - 0.5f, edgeLeft, edgeRight, left, right, bottom, lerp);
          result = lerp.Lerp(center, blend, (fy - 0.5f) * 2f);
        }

        if (edgeLeft && fx < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, 0.5f - fx, edgeTop, edgeBottom, top, bottom, left, lerp);
          result = lerp.Lerp(result, blend, (0.5f - fx) * 0.5f);
        } else if (edgeRight && fx > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, fx - 0.5f, edgeTop, edgeBottom, top, bottom, right, lerp);
          result = lerp.Lerp(result, blend, (fx - 0.5f) * 0.5f);
        }

        Mlaa.ApplyDiagonalBlending(ref result, fx, fy, center,
          top, bottom, left, right,
          topLeft, topRight, bottomLeft, bottomRight, threshold, lerp);

        row[dx] = encoder.Encode(result);
      }
    }
  }
}

file readonly struct Mlaa4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp, float threshold)
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
    const int scale = 4;

    // Get 3x3 neighborhood
    var center = window.P0P0.Work;
    var top = window.M1P0.Work;
    var bottom = window.P1P0.Work;
    var left = window.P0M1.Work;
    var right = window.P0P1.Work;
    var topLeft = window.M1M1.Work;
    var topRight = window.M1P1.Work;
    var bottomLeft = window.P1M1.Work;
    var bottomRight = window.P1P1.Work;

    // Detect edges in each direction
    var edgeTop = Mlaa.IsEdge(center, top, threshold);
    var edgeBottom = Mlaa.IsEdge(center, bottom, threshold);
    var edgeLeft = Mlaa.IsEdge(center, left, threshold);
    var edgeRight = Mlaa.IsEdge(center, right, threshold);

    // Process each output pixel in the 4x4 block
    for (var dy = 0; dy < scale; ++dy) {
      var row = destTopLeft + dy * destStride;

      for (var dx = 0; dx < scale; ++dx) {
        var fx = (dx + 0.5f) / scale;
        var fy = (dy + 0.5f) / scale;

        var result = center;

        if (edgeTop && fy < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, 0.5f - fy, edgeLeft, edgeRight, left, right, top, lerp);
          result = lerp.Lerp(center, blend, (0.5f - fy) * 2f);
        } else if (edgeBottom && fy > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fx, fy - 0.5f, edgeLeft, edgeRight, left, right, bottom, lerp);
          result = lerp.Lerp(center, blend, (fy - 0.5f) * 2f);
        }

        if (edgeLeft && fx < 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, 0.5f - fx, edgeTop, edgeBottom, top, bottom, left, lerp);
          result = lerp.Lerp(result, blend, (0.5f - fx) * 0.5f);
        } else if (edgeRight && fx > 0.5f) {
          var blend = Mlaa.CalculateEdgeBlend(fy, fx - 0.5f, edgeTop, edgeBottom, top, bottom, right, lerp);
          result = lerp.Lerp(result, blend, (fx - 0.5f) * 0.5f);
        }

        Mlaa.ApplyDiagonalBlending(ref result, fx, fy, center,
          top, bottom, left, right,
          topLeft, topRight, bottomLeft, bottomRight, threshold, lerp);

        row[dx] = encoder.Encode(result);
      }
    }
  }
}
