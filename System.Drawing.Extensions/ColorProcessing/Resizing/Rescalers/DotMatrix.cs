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
/// DotMatrix scaler - simulates dot-matrix display appearance.
/// </summary>
/// <remarks>
/// <para>Reference: ScummVM project (https://wiki.scummvm.org/index.php/Scalers)</para>
/// <para>Algorithm: Creates a dot-matrix pattern by varying pixel brightness in a grid.</para>
/// <para>Simulates the appearance of LCD/LED dot-matrix displays with visible gaps.</para>
/// </remarks>
[ScalerInfo("DotMatrix", Author = "ScummVM Team",
  Url = "https://wiki.scummvm.org/index.php/Scalers",
  Description = "Dot-matrix display simulation", Category = ScalerCategory.PixelArt)]
public readonly struct DotMatrix : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a DotMatrix scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2, 3, or 4).</param>
  public DotMatrix(int scale = 2) {
    if (scale is < 2 or > 4)
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "DotMatrix supports 2x, 3x, or 4x scaling");
    this._scale = scale;
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
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      0 or 2 => callback.Invoke(new DotMatrix2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      3 => callback.Invoke(new DotMatrix3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
      4 => callback.Invoke(new DotMatrix4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(lerp)),
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

  #region Static Presets

  /// <summary>Gets a 2x DotMatrix scaler.</summary>
  public static DotMatrix X2 => new(2);

  /// <summary>Gets a 3x DotMatrix scaler.</summary>
  public static DotMatrix X3 => new(3);

  /// <summary>Gets a 4x DotMatrix scaler.</summary>
  public static DotMatrix X4 => new(4);

  /// <summary>Gets the default DotMatrix scaler (2x).</summary>
  public static DotMatrix Default => X2;

  #endregion
}

#region DotMatrix Helpers

file static class DotMatrixHelpers {
  /// <summary>Weight scale for integer lerp operations.</summary>
  public const int WeightScale = 1000;

  /// <summary>Full brightness weight (100%).</summary>
  public const int FullWeight = WeightScale;

  /// <summary>Medium brightness weight (75%).</summary>
  public const int MediumWeight = 750;

  /// <summary>Dark brightness weight (50%).</summary>
  public const int DarkWeight = 500;

  /// <summary>
  /// Applies brightness multiplier to a color using lerp with black.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork ApplyBrightness<TWork, TLerp>(in TLerp lerp, in TWork color, int weight)
    where TWork : unmanaged, IColorSpace
    where TLerp : struct, ILerp<TWork>
    => lerp.Lerp(default, color, WeightScale - weight, weight);
}

#endregion

#region DotMatrix 2x Kernel

file readonly struct DotMatrix2xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;

    // Create dot-matrix pattern:
    // [Full   Medium]
    // [Medium Dark  ]
    dest[0] = encoder.Encode(pixel);
    dest[1] = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.MediumWeight));
    dest[destStride] = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.MediumWeight));
    dest[destStride + 1] = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.DarkWeight));
  }
}

#endregion

#region DotMatrix 3x Kernel

file readonly struct DotMatrix3xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var pixelEncoded = encoder.Encode(pixel);
    var medium = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.MediumWeight));
    var dark = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.DarkWeight));

    // Create dot-matrix pattern:
    // [Full   Full   Medium]
    // [Full   Full   Medium]
    // [Medium Medium Dark  ]
    dest[0] = pixelEncoded;
    dest[1] = pixelEncoded;
    dest[2] = medium;
    dest[destStride] = pixelEncoded;
    dest[destStride + 1] = pixelEncoded;
    dest[destStride + 2] = medium;
    dest[2 * destStride] = medium;
    dest[2 * destStride + 1] = medium;
    dest[2 * destStride + 2] = dark;
  }
}

#endregion

#region DotMatrix 4x Kernel

file readonly struct DotMatrix4xKernel<TWork, TKey, TPixel, TLerp, TEncode>(TLerp lerp = default)
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
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var pixelEncoded = encoder.Encode(pixel);
    var medium = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.MediumWeight));
    var dark = encoder.Encode(DotMatrixHelpers.ApplyBrightness(lerp, pixel, DotMatrixHelpers.DarkWeight));

    // Create dot-matrix pattern with larger dot and darker border:
    // [Full   Full   Full   Medium]
    // [Full   Full   Full   Medium]
    // [Full   Full   Full   Medium]
    // [Medium Medium Medium Dark  ]

    // Fill 3x3 full brightness core
    for (var dy = 0; dy < 3; ++dy) {
      var row = dest + dy * destStride;
      for (var dx = 0; dx < 3; ++dx)
        row[dx] = pixelEncoded;
    }

    // Right edge (medium)
    dest[3] = medium;
    dest[destStride + 3] = medium;
    dest[2 * destStride + 3] = medium;

    // Bottom edge (medium)
    dest[3 * destStride] = medium;
    dest[3 * destStride + 1] = medium;
    dest[3 * destStride + 2] = medium;

    // Corner (dark)
    dest[3 * destStride + 3] = dark;
  }
}

#endregion
