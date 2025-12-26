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

using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Eagle pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using simple corner detection based on diagonal neighbors.</para>
/// <para>
/// For each pixel P with 8-connected neighbors S, T, U, V, X, W, Y, Z:
/// <list type="bullet">
/// <item>E1 = if S==T and S==V then S else P (top-left corner)</item>
/// <item>E2 = if T==U and T==X then T else P (top-right corner)</item>
/// <item>E3 = if V==W and V==Y then V else P (bottom-left corner)</item>
/// <item>E4 = if X==Z and X==Y then X else P (bottom-right corner)</item>
/// </list>
/// </para>
/// <para>One of the earliest pixel-art scalers, developed in 1997.</para>
/// </remarks>
[ScalerInfo("Eagle", Year = 1997,
  Description = "Early pixel-art scaler using simple corner detection", Category = ScalerCategory.PixelArt)]
public readonly struct Eagle : IPixelScaler, IScalerDispatch {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

  /// <inheritdoc />
  Bitmap IScalerDispatch.Apply(Bitmap source, ScalerQuality quality)
    => quality switch {
      ScalerQuality.Fast => BitmapScalerExtensions.Upscale<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>,
        EagleKernel<Bgra8888, Bgra8888, Bgra8888, ExactEquality<Bgra8888>, IdentityEncode<Bgra8888>>
      >(source, new()),
      ScalerQuality.HighQuality => BitmapScalerExtensions.Upscale<
        LinearRgbaF, OklabF,
        Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
        EagleKernel<LinearRgbaF, OklabF, Bgra8888, ThresholdEquality<OklabF, Euclidean3<OklabF>>, LinearRgbaFToSrgb32>
      >(source, new(new(0.02f))),
      _ => throw new System.NotSupportedException($"Quality {quality} is not supported for Eagle.")
    };

  /// <summary>
  /// Gets the list of scale factors supported by Eagle.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether Eagle supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for Eagle.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default Eagle configuration.
  /// </summary>
  public static Eagle Default => new();

  #region Nested Kernel Types

  /// <summary>
  /// Internal kernel for Eagle algorithm.
  /// </summary>
  /// <remarks>
  /// Eagle pattern (uses 3x3 neighborhood):
  ///
  /// S T U      (top-left, top, top-right)
  /// V P X      (left, center, right)
  /// W Y Z      (bottom-left, bottom, bottom-right)
  ///
  /// Output 2x2 block:
  /// E1 E2
  /// E3 E4
  ///
  /// Rules:
  /// - E1 = (S==T and S==V) ? S : P
  /// - E2 = (T==U and T==X) ? T : P
  /// - E3 = (V==W and V==Y) ? V : P
  /// - E4 = (X==Z and X==Y) ? X : P
  ///
  /// Eagle was one of the earliest pixel-art scalers, using simple corner detection.
  /// </remarks>
  private readonly struct EagleKernel<TWork, TKey, TPixel, TEquality, TEncode>(TEquality equality = default)
    : IScaler<TWork, TKey, TPixel, TEncode>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEquality : struct, IColorEquality<TKey>
    where TEncode : struct, IEncode<TWork, TPixel> {

    /// <inheritdoc />
    public int ScaleX => 2;

    /// <inheritdoc />
    public int ScaleY => 2;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Scale(
      in NeighborWindow<TWork, TKey> window,
      TPixel* destTopLeft,
      int destStride,
      in TEncode encoder
    ) {
      // Get the 3x3 source neighborhood
      var s = window.M1M1; // top-left
      var t = window.M1P0; // top
      var u = window.M1P1; // top-right
      var v = window.P0M1; // left
      var p = window.P0P0; // center
      var x = window.P0P1; // right
      var w = window.P1M1; // bottom-left
      var y = window.P1P0; // bottom
      var z = window.P1P1; // bottom-right

      var pWork = p.Work;

      // Default all outputs to center pixel
      var e1 = pWork;
      var e2 = pWork;
      var e3 = pWork;
      var e4 = pWork;

      // Pre-compute keys for equality tests
      var sKey = s.Key;
      var tKey = t.Key;
      var uKey = u.Key;
      var vKey = v.Key;
      var xKey = x.Key;
      var wKey = w.Key;
      var yKey = y.Key;
      var zKey = z.Key;

      // Eagle corner rules
      // E1: top-left corner - if S==T and S==V, use S
      if (equality.Equals(sKey, tKey) && equality.Equals(sKey, vKey))
        e1 = s.Work;

      // E2: top-right corner - if T==U and T==X, use T
      if (equality.Equals(tKey, uKey) && equality.Equals(tKey, xKey))
        e2 = t.Work;

      // E3: bottom-left corner - if V==W and V==Y, use V
      if (equality.Equals(vKey, wKey) && equality.Equals(vKey, yKey))
        e3 = v.Work;

      // E4: bottom-right corner - if X==Z and X==Y, use X
      if (equality.Equals(xKey, zKey) && equality.Equals(xKey, yKey))
        e4 = x.Work;

      // Write directly to destination with encoding
      var row0 = destTopLeft;
      var row1 = destTopLeft + destStride;
      row0[0] = encoder.Encode(e1);
      row0[1] = encoder.Encode(e2);
      row1[0] = encoder.Encode(e3);
      row1[1] = encoder.Encode(e4);
    }
  }

  #endregion
}
