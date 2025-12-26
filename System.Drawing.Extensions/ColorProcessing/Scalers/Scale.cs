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
using System.Drawing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Andrea Mazzoleni's Scale2X/Scale3X pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x or 3x using edge-aware corner interpolation.</para>
/// <para>
/// Core pattern: If neighbors B (top) and H (bottom) differ, and neighbors D (left) and F (right) differ,
/// then corners are interpolated based on matching neighbors.
/// </para>
/// </remarks>
[ScalerInfo("Scale", Author = "Andrea Mazzoleni", Year = 2001, Url = "https://www.scale2x.it/algorithm",
  Description = "Edge-aware 2x/3x scaling from the AdvanceMAME project", Category = ScalerCategory.PixelArt)]
public readonly struct Scale : IPixelScaler, IScalerDispatch {

  public enum Mode {
    X2,X3
  }

  private readonly int _scaleFactor;

  /// <summary>
  /// Creates a Scale scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  /// <exception cref="ArgumentOutOfRangeException">Thrown when scale is not 2 or 3.</exception>
  public Scale(Mode scale) {
    if (scale is not (Mode.X2 or Mode.X3))
      throw new ArgumentOutOfRangeException(nameof(scale), scale, "Scale factor must be 2 or 3.");

    this._scaleFactor = scale == Mode.X2 ? 2 : 3;
  }

  /// <inheritdoc />
  ScaleFactor IScalerInfo.Scale => new(this._scaleFactor, this._scaleFactor);

  /// <summary>
  /// Gets the list of scale factors supported by Scale.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether Scale supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2 or 3x3; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions for Scale.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x and 3x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  #region Static Presets

  /// <summary>Gets a 2x Scale scaler.</summary>
  public static Scale X2 => new(Mode.X2);

  /// <summary>Gets a 3x Scale scaler.</summary>
  public static Scale X3 => new(Mode.X3);

  /// <summary>Gets the default Scale scaler (2x).</summary>
  public static Scale Default => X2;

  #endregion

  /// <inheritdoc />
  Bitmap IScalerDispatch.Apply(Bitmap source, ScalerQuality quality)
    => this._scaleFactor switch {
      2 => _ApplyScale2x(source, quality),
      3 => _ApplyScale3x(source, quality),
      _ => throw new NotSupportedException($"Scale factor {this._scaleFactor} is not supported.")
    };

  private static Bitmap _ApplyScale2x(Bitmap source, ScalerQuality quality)
    => quality switch {
      ScalerQuality.Fast => BitmapScalerExtensions.Upscale<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>,
        Scale2xKernel<Bgra8888, Bgra8888, Bgra8888, ExactEquality<Bgra8888>, Color4BLerp<Bgra8888>, IdentityEncode<Bgra8888>>
      >(source, new()),
      ScalerQuality.HighQuality => BitmapScalerExtensions.Upscale<
        LinearRgbaF, OklabF,
        Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
        Scale2xKernel<LinearRgbaF, OklabF, Bgra8888, ThresholdEquality<OklabF, Euclidean3<OklabF>>, LinearRgbaFLerp, LinearRgbaFToSrgb32>
      >(source, new(new(0.02f))),
      _ => throw new NotSupportedException($"Quality {quality} is not supported for Scale2X.")
    };

  private static Bitmap _ApplyScale3x(Bitmap source, ScalerQuality quality)
    => quality switch {
      ScalerQuality.Fast => BitmapScalerExtensions.Upscale<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>,
        Scale3xKernel<Bgra8888, Bgra8888, Bgra8888, ExactEquality<Bgra8888>, Color4BLerp<Bgra8888>, IdentityEncode<Bgra8888>>
      >(source, new()),
      ScalerQuality.HighQuality => BitmapScalerExtensions.Upscale<
        LinearRgbaF, OklabF,
        Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
        Scale3xKernel<LinearRgbaF, OklabF, Bgra8888, ThresholdEquality<OklabF, Euclidean3<OklabF>>, LinearRgbaFLerp, LinearRgbaFToSrgb32>
      >(source, new(new(0.02f))),
      _ => throw new NotSupportedException($"Quality {quality} is not supported for Scale3X.")
    };

  #region Nested Kernel Types

  /// <summary>
  /// Internal kernel for Scale2X algorithm.
  /// </summary>
  private readonly struct Scale2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default) : IScaler<TWork, TKey, TPixel, TEncode>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
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
      // Scale2x pattern:
      //   B        (top)
      // D P F    (left, center, right)
      //   H        (bottom)
      //
      // Output 2x2 block:
      // E0 E1
      // E2 E3

      var b = window.M1P0; // top
      var d = window.P0M1; // left
      var p = window.P0P0; // center
      var f = window.P0P1; // right
      var h = window.P1P0; // bottom

      var pWork = p.Work;

      // Default all outputs to center pixel
      var e0 = pWork;
      var e1 = pWork;
      var e2 = pWork;
      var e3 = pWork;

      // Core Scale2x condition: B != H and D != F
      if (!equality.Equals(b.Key, h.Key) && !equality.Equals(d.Key, f.Key)) {
        // Check corners and interpolate if matching
        if (equality.Equals(d.Key, b.Key))
          e0 = lerp.Lerp(d.Work, b.Work);

        if (equality.Equals(b.Key, f.Key))
          e1 = lerp.Lerp(b.Work, f.Work);

        if (equality.Equals(d.Key, h.Key))
          e2 = lerp.Lerp(d.Work, h.Work);

        if (equality.Equals(h.Key, f.Key))
          e3 = lerp.Lerp(h.Work, f.Work);
      }

      // Write directly to destination with encoding
      var row0 = destTopLeft;
      var row1 = destTopLeft + destStride;
      row0[0] = encoder.Encode(e0);
      row0[1] = encoder.Encode(e1);
      row1[0] = encoder.Encode(e2);
      row1[1] = encoder.Encode(e3);
    }
  }

  /// <summary>
  /// Internal kernel for Scale3X algorithm.
  /// </summary>
  private readonly struct Scale3xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default) : IScaler<TWork, TKey, TPixel, TEncode>
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel> {

    /// <inheritdoc />
    public int ScaleX => 3;

    /// <inheritdoc />
    public int ScaleY => 3;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void Scale(
      in NeighborWindow<TWork, TKey> window,
      TPixel* destTopLeft,
      int destStride,
      in TEncode encoder
    ) {
      // Scale3x pattern:
      // A B C     (top row)
      // D P F     (center row)
      // G H I     (bottom row)
      //
      // Output 3x3 block:
      // E0 E1 E2
      // E3 E4 E5
      // E6 E7 E8

      var a = window.M1M1; // top-left
      var b = window.M1P0; // top
      var c = window.M1P1; // top-right
      var d = window.P0M1; // left
      var p = window.P0P0; // center
      var f = window.P0P1; // right
      var g = window.P1M1; // bottom-left
      var h = window.P1P0; // bottom
      var i = window.P1P1; // bottom-right

      var pWork = p.Work;

      // Default all outputs to center pixel
      var e0 = pWork;
      var e1 = pWork;
      var e2 = pWork;
      var e3 = pWork;
      var e4 = pWork;
      var e5 = pWork;
      var e6 = pWork;
      var e7 = pWork;
      var e8 = pWork;

      // Core Scale3x condition: B != H and D != F
      if (!equality.Equals(b.Key, h.Key) && !equality.Equals(d.Key, f.Key)) {
        // Corners
        if (equality.Equals(d.Key, b.Key))
          e0 = lerp.Lerp(d.Work, b.Work);

        if (equality.Equals(b.Key, f.Key))
          e2 = lerp.Lerp(b.Work, f.Work);

        if (equality.Equals(d.Key, h.Key))
          e6 = lerp.Lerp(d.Work, h.Work);

        if (equality.Equals(h.Key, f.Key))
          e8 = lerp.Lerp(h.Work, f.Work);

        // Edges - more complex logic
        var db = equality.Equals(d.Key, b.Key);
        var bf = equality.Equals(b.Key, f.Key);
        var dh = equality.Equals(d.Key, h.Key);
        var hf = equality.Equals(h.Key, f.Key);

        // Top edge
        if (db && bf && !equality.Equals(p.Key, c.Key) && !equality.Equals(p.Key, a.Key))
          e1 = lerp.Lerp(lerp.Lerp(b.Work, d.Work), f.Work);
        else if (db && !equality.Equals(p.Key, c.Key))
          e1 = lerp.Lerp(d.Work, b.Work);
        else if (bf && !equality.Equals(p.Key, a.Key))
          e1 = lerp.Lerp(b.Work, f.Work);

        // Left edge
        if (db && dh && !equality.Equals(p.Key, g.Key) && !equality.Equals(p.Key, a.Key))
          e3 = lerp.Lerp(lerp.Lerp(d.Work, b.Work), h.Work);
        else if (db && !equality.Equals(p.Key, g.Key))
          e3 = lerp.Lerp(d.Work, b.Work);
        else if (dh && !equality.Equals(p.Key, a.Key))
          e3 = lerp.Lerp(d.Work, h.Work);

        // Right edge
        if (bf && hf && !equality.Equals(p.Key, i.Key) && !equality.Equals(p.Key, c.Key))
          e5 = lerp.Lerp(lerp.Lerp(f.Work, b.Work), h.Work);
        else if (bf && !equality.Equals(p.Key, i.Key))
          e5 = lerp.Lerp(b.Work, f.Work);
        else if (hf && !equality.Equals(p.Key, c.Key))
          e5 = lerp.Lerp(h.Work, f.Work);

        // Bottom edge
        if (dh && hf && !equality.Equals(p.Key, i.Key) && !equality.Equals(p.Key, g.Key))
          e7 = lerp.Lerp(lerp.Lerp(h.Work, d.Work), f.Work);
        else if (dh && !equality.Equals(p.Key, i.Key))
          e7 = lerp.Lerp(d.Work, h.Work);
        else if (hf && !equality.Equals(p.Key, g.Key))
          e7 = lerp.Lerp(h.Work, f.Work);
      }

      // Write directly to destination with encoding
      var row0 = destTopLeft;
      var row1 = destTopLeft + destStride;
      var row2 = row1 + destStride;
      row0[0] = encoder.Encode(e0);
      row0[1] = encoder.Encode(e1);
      row0[2] = encoder.Encode(e2);
      row1[0] = encoder.Encode(e3);
      row1[1] = encoder.Encode(e4);
      row1[2] = encoder.Encode(e5);
      row2[0] = encoder.Encode(e6);
      row2[1] = encoder.Encode(e7);
      row2[2] = encoder.Encode(e8);
    }
  }

  #endregion
}
