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
/// ScaleNxSFX family - enhanced Scale2x/Scale3x with artifact prevention.
/// </summary>
/// <remarks>
/// <para>Reference: Sp00kyFox (ScaleNxSFX improved version)</para>
/// <para>See: https://github.com/libretro/glsl-shaders/tree/master/scalenx</para>
/// <para>Enhanced versions of Scale2x/Scale3x with smoother edges and artifact prevention.</para>
/// <para>Uses additional pattern checks to eliminate artifacts that standard ScaleNx produces.</para>
/// </remarks>
[ScalerInfo("ScaleNxSFX", Author = "Sp00kyFox",
  Url = "https://github.com/libretro/glsl-shaders/tree/master/scalenx",
  Description = "Enhanced ScaleNx with artifact prevention", Category = ScalerCategory.PixelArt)]
public readonly struct ScaleNxSfx : IPixelScaler {
  private readonly int _scale;

  /// <summary>
  /// Creates a ScaleNxSFX scaler with the specified factor.
  /// </summary>
  /// <param name="scale">Scale factor (2 or 3).</param>
  public ScaleNxSfx(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 3);
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
      0 or 2 => callback.Invoke(new Scale2xSfxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      3 => callback.Invoke(new Scale3xSfxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  #region Static Presets

  /// <summary>Gets a 2x ScaleNxSFX scaler.</summary>
  public static ScaleNxSfx X2 => new(2);

  /// <summary>Gets a 3x ScaleNxSFX scaler.</summary>
  public static ScaleNxSfx X3 => new(3);

  /// <summary>Gets the default ScaleNxSFX scaler (2x).</summary>
  public static ScaleNxSfx Default => X2;

  #endregion
}

#region Scale2xSFX Kernel

file readonly struct Scale2xSfxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
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
    // Get 3x3 neighborhood
    //  a b c
    //  d e f
    //  g h i
    var a = window.M1M1;
    var b = window.M1P0;
    var c = window.M1P1;
    var d = window.P0M1;
    var e = window.P0P0;
    var f = window.P0P1;
    var g = window.P1M1;
    var h = window.P1P0;
    var i = window.P1P1;

    // Default to center
    var e0 = e.Work;
    var e1 = e.Work;
    var e2 = e.Work;
    var e3 = e.Work;

    // Artifact prevention flags
    var art0 = equality.Equals(e.Key, c.Key) || equality.Equals(e.Key, g.Key);
    var art1 = equality.Equals(e.Key, a.Key) || equality.Equals(e.Key, i.Key);

    // Standard Scale2x condition: diagonal neighbors must differ
    if (!equality.Equals(b.Key, h.Key) && !equality.Equals(d.Key, f.Key)) {
      // Top-left pixel: blend D and B if they match and no artifact
      if (equality.Equals(d.Key, b.Key) && (!art1 || equality.Equals(a.Key, d.Key)))
        e0 = lerp.Lerp(d.Work, b.Work);

      // Top-right pixel: blend B and F if they match and no artifact
      if (equality.Equals(b.Key, f.Key) && (!art0 || equality.Equals(c.Key, f.Key)))
        e1 = lerp.Lerp(b.Work, f.Work);

      // Bottom-left pixel: blend D and H if they match and no artifact
      if (equality.Equals(d.Key, h.Key) && (!art0 || equality.Equals(g.Key, d.Key)))
        e2 = lerp.Lerp(d.Work, h.Work);

      // Bottom-right pixel: blend H and F if they match and no artifact
      if (equality.Equals(h.Key, f.Key) && (!art1 || equality.Equals(i.Key, f.Key)))
        e3 = lerp.Lerp(h.Work, f.Work);
    }

    // Write output
    dest[0] = encoder.Encode(e0);
    dest[1] = encoder.Encode(e1);
    dest[destStride] = encoder.Encode(e2);
    dest[destStride + 1] = encoder.Encode(e3);
  }
}

#endregion

#region Scale3xSFX Kernel

file readonly struct Scale3xSfxKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
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
    // Get 3x3 neighborhood
    var a = window.M1M1;
    var b = window.M1P0;
    var c = window.M1P1;
    var d = window.P0M1;
    var e = window.P0P0;
    var f = window.P0P1;
    var g = window.P1M1;
    var h = window.P1P0;
    var i = window.P1P1;

    // Default all 9 output pixels to center
    var e00 = e.Work;
    var e01 = e.Work;
    var e02 = e.Work;
    var e10 = e.Work;
    var e11 = e.Work;
    var e12 = e.Work;
    var e20 = e.Work;
    var e21 = e.Work;
    var e22 = e.Work;

    // Artifact prevention flags
    var art0 = equality.Equals(e.Key, c.Key) || equality.Equals(e.Key, g.Key);
    var art1 = equality.Equals(e.Key, a.Key) || equality.Equals(e.Key, i.Key);

    // Standard Scale3x condition
    if (!equality.Equals(b.Key, h.Key) && !equality.Equals(d.Key, f.Key)) {
      var db = equality.Equals(d.Key, b.Key);
      var bf = equality.Equals(b.Key, f.Key);
      var dh = equality.Equals(d.Key, h.Key);
      var hf = equality.Equals(h.Key, f.Key);
      var ad = equality.Equals(a.Key, d.Key);
      var cf = equality.Equals(c.Key, f.Key);
      var gd = equality.Equals(g.Key, d.Key);
      var if_ = equality.Equals(i.Key, f.Key);
      var ec = equality.Equals(e.Key, c.Key);
      var ea = equality.Equals(e.Key, a.Key);
      var eg = equality.Equals(e.Key, g.Key);
      var ei = equality.Equals(e.Key, i.Key);

      // Corner pixels
      if (db && (!art1 || ad))
        e00 = lerp.Lerp(d.Work, b.Work);

      if (bf && (!art0 || cf))
        e02 = lerp.Lerp(b.Work, f.Work);

      if (dh && (!art0 || gd))
        e20 = lerp.Lerp(d.Work, h.Work);

      if (hf && (!art1 || if_))
        e22 = lerp.Lerp(h.Work, f.Work);

      // Edge pixels - with enhanced artifact prevention
      if (db && !ec && (!art1 || ad))
        e01 = lerp.Lerp(d.Work, b.Work);
      else if (bf && !ea && (!art0 || cf))
        e01 = lerp.Lerp(b.Work, f.Work);

      if (db && !eg && (!art1 || ad))
        e10 = lerp.Lerp(d.Work, b.Work);
      else if (dh && !ea && (!art0 || gd))
        e10 = lerp.Lerp(d.Work, h.Work);

      if (bf && !ei && (!art0 || cf))
        e12 = lerp.Lerp(b.Work, f.Work);
      else if (hf && !ec && (!art1 || if_))
        e12 = lerp.Lerp(h.Work, f.Work);

      if (dh && !ei && (!art0 || gd))
        e21 = lerp.Lerp(d.Work, h.Work);
      else if (hf && !eg && (!art1 || if_))
        e21 = lerp.Lerp(h.Work, f.Work);
    }

    // Write output
    dest[0] = encoder.Encode(e00);
    dest[1] = encoder.Encode(e01);
    dest[2] = encoder.Encode(e02);
    dest[destStride] = encoder.Encode(e10);
    dest[destStride + 1] = encoder.Encode(e11);
    dest[destStride + 2] = encoder.Encode(e12);
    dest[2 * destStride] = encoder.Encode(e20);
    dest[2 * destStride + 1] = encoder.Encode(e21);
    dest[2 * destStride + 2] = encoder.Encode(e22);
  }
}

#endregion
