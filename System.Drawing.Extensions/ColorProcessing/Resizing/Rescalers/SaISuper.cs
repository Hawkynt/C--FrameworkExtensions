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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

#region Super2xSaI

/// <summary>
/// Kreed's Super2xSaI pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using enhanced diagonal edge detection.</para>
/// <para>
/// An improvement over 2xSaI with better handling of edge cases
/// and smoother blending in ambiguous areas.
/// </para>
/// <para>Developed by Kreed in 1999 as an enhanced version of 2xSaI.</para>
/// </remarks>
[ScalerInfo("Super2xSaI", Author = "Kreed", Year = 1999,
  Description = "Enhanced 2xSaI with improved edge handling", Category = ScalerCategory.PixelArt)]
public readonly struct Super2xSaI : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

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
    => callback.Invoke(new Super2xSaIKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by Super2xSaI.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether Super2xSaI supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for Super2xSaI.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default Super2xSaI configuration.
  /// </summary>
  public static Super2xSaI Default => new();
}

/// <summary>
/// Internal kernel for Super2xSaI algorithm.
/// </summary>
/// <remarks>
/// Super2xSaI uses a 4x4 neighborhood with enhanced edge detection:
///
/// C0 C1 C2 D3     (row -1)
/// C3 C4 C5 D4     (row 0, center row)
/// C6 C7 C8 D5     (row +1)
/// D0 D1 D2 D6     (row +2)
///
/// Output 2x2 block:
/// E00 E01
/// E10 E11
/// </remarks>
file readonly struct Super2xSaIKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
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
    // Get the 4x4 source neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c0 = window.M1M1; // top-left (row -1, col -1)
    var c1 = window.M1P0; // top (row -1, col 0)
    var c2 = window.M1P1; // top-right (row -1, col +1)
    var d3 = window.M1P2; // top far-right (row -1, col +2)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var d4 = window.P0P2; // far-right (row 0, col +2)
    var c6 = window.P1M1; // bottom-left (row +1, col -1)
    var c7 = window.P1P0; // bottom (row +1, col 0)
    var c8 = window.P1P1; // bottom-right (row +1, col +1)
    var d5 = window.P1P2; // bottom far-right (row +1, col +2)
    var d0 = window.P2M1; // far-bottom left (row +2, col -1)
    var d1 = window.P2P0; // far-bottom (row +2, col 0)
    var d2 = window.P2P1; // far-bottom right (row +2, col +1)
    var d6 = window.P2P2; // far-bottom far-right (row +2, col +2)

    var c4Work = c4.Work;

    // Default outputs - all start at center pixel
    var e00 = c4Work;
    var e01 = c4Work;
    var e10 = c4Work;
    var e11 = c4Work;

    var c7Like5 = equality.Equals(c7.Key, c5.Key);
    var c4Like8 = equality.Equals(c4.Key, c8.Key);

    if (c7Like5 && !c4Like8) {
      var c57 = lerp.Lerp(c7.Work, c5.Work);
      e11 = c57;
      e01 = c57;
    } else if (c4Like8 && !c7Like5) {
      // Keep defaults
    } else if (c4Like8 && c7Like5) {
      var c57 = lerp.Lerp(c7.Work, c5.Work);
      var c48 = lerp.Lerp(c4.Work, c8.Work);
      var conc2D = 0;
      conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c6.Key, d1.Key);
      conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c3.Key, c1.Key);
      conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, d2.Key, d5.Key);
      conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c2.Key, d4.Key);

      if (conc2D > 0) {
        e11 = c57;
        e01 = c57;
      } else if (conc2D == 0) {
        var blend = lerp.Lerp(c48, c57);
        e11 = blend;
        e01 = blend;
      }
    } else {
      // Neither diagonal matches - complex blending
      var c8Like5 = equality.Equals(c8.Key, c5.Key);
      var c8LikeD1 = equality.Equals(c8.Key, d1.Key);
      var c7NotLikeD2 = !equality.Equals(c7.Key, d2.Key);
      var c8NotLikeD0 = !equality.Equals(c8.Key, d0.Key);

      if (c8Like5 && c8LikeD1 && c7NotLikeD2 && c8NotLikeD0) {
        var blend3 = lerp.Lerp(c8.Work, c5.Work, d1.Work);
        e11 = lerp.Lerp(blend3, c7.Work, 3, 1);
      } else {
        var c7Like4 = equality.Equals(c7.Key, c4.Key);
        var c7LikeD2 = equality.Equals(c7.Key, d2.Key);
        var c7NotLikeD6 = !equality.Equals(c7.Key, d6.Key);
        var c8NotLikeD1 = !equality.Equals(c8.Key, d1.Key);

        if (c7Like4 && c7LikeD2 && c7NotLikeD6 && c8NotLikeD1) {
          var blend3 = lerp.Lerp(c7.Work, c4Work, d2.Work);
          e11 = lerp.Lerp(blend3, c8.Work, 3, 1);
        } else {
          e11 = lerp.Lerp(c7.Work, c8.Work);
        }
      }

      var c5Like8 = equality.Equals(c5.Key, c8.Key);
      var c5Like1 = equality.Equals(c5.Key, c1.Key);
      var c5NotLike0 = !equality.Equals(c5.Key, c0.Key);
      var c4NotLike2 = !equality.Equals(c4.Key, c2.Key);

      if (c5Like8 && c5Like1 && c5NotLike0 && c4NotLike2) {
        var blend3 = lerp.Lerp(c5.Work, c8.Work, c1.Work);
        e01 = lerp.Lerp(blend3, c4Work, 3, 1);
      } else {
        var c4Like7 = equality.Equals(c4.Key, c7.Key);
        var c4Like2 = equality.Equals(c4.Key, c2.Key);
        var c5NotLike1 = !equality.Equals(c5.Key, c1.Key);
        var c4NotLikeD3 = !equality.Equals(c4.Key, d3.Key);

        if (c4Like7 && c4Like2 && c5NotLike1 && c4NotLikeD3) {
          var blend3 = lerp.Lerp(c4Work, c7.Work, c2.Work);
          e01 = lerp.Lerp(blend3, c5.Work, 3, 1);
        } else {
          e01 = lerp.Lerp(c4Work, c5.Work);
        }
      }
    }

    // E10 logic
    var c4Like8For10 = equality.Equals(c4.Key, c8.Key);
    var c4Like3 = equality.Equals(c4.Key, c3.Key);
    var c7NotLike5 = !equality.Equals(c7.Key, c5.Key);
    var c4NotLikeD2 = !equality.Equals(c4.Key, d2.Key);

    if (c4Like8For10 && c4Like3 && c7NotLike5 && c4NotLikeD2)
      e10 = lerp.Lerp(c7.Work, lerp.Lerp(c4Work, c8.Work, c3.Work));
    else {
      var c4Like6 = equality.Equals(c4.Key, c6.Key);
      var c4Like5 = equality.Equals(c4.Key, c5.Key);
      var c7NotLike3 = !equality.Equals(c7.Key, c3.Key);
      var c4NotLikeD0 = !equality.Equals(c4.Key, d0.Key);

      if (c4Like6 && c4Like5 && c7NotLike3 && c4NotLikeD0)
        e10 = lerp.Lerp(c7.Work, lerp.Lerp(c4Work, c6.Work, c5.Work));
      else
        e10 = lerp.Lerp(c4Work, c7.Work);
    }

    // E00 logic
    var c7Like5For00 = equality.Equals(c7.Key, c5.Key);
    var c7Like6 = equality.Equals(c7.Key, c6.Key);
    var c4NotLike8 = !equality.Equals(c4.Key, c8.Key);
    var c7NotLike2 = !equality.Equals(c7.Key, c2.Key);

    if (c7Like5For00 && c7Like6 && c4NotLike8 && c7NotLike2)
      e00 = lerp.Lerp(lerp.Lerp(c7.Work, c5.Work, c6.Work), c4Work);
    else {
      var c7Like3 = equality.Equals(c7.Key, c3.Key);
      var c7Like8 = equality.Equals(c7.Key, c8.Key);
      var c4NotLike6 = !equality.Equals(c4.Key, c6.Key);
      var c7NotLike0 = !equality.Equals(c7.Key, c0.Key);

      if (c7Like3 && c7Like8 && c4NotLike6 && c7NotLike0)
        e00 = lerp.Lerp(lerp.Lerp(c7.Work, c3.Work, c8.Work), c4Work);
    }

    // Write to destination
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  /// <summary>
  /// Computes concurrency value for diagonal detection.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _Conc2D(TKey c00, TKey c01, TKey c10, TKey c11, TKey test0, TKey test1) {
    var result = 0;

    var acLike = equality.Equals(c00, c10);
    var x = acLike ? 1 : 0;
    var y = equality.Equals(c01, c10) && !acLike ? 1 : 0;

    var adLike = equality.Equals(c00, c11);
    x += adLike ? 1 : 0;
    y += equality.Equals(c01, c11) && !adLike ? 1 : 0;

    if (x <= 1)
      ++result;
    if (y <= 1)
      --result;

    return result;
  }
}

#endregion

#region SuperEagle

/// <summary>
/// Kreed's SuperEagle pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using enhanced diagonal edge detection and blending.</para>
/// <para>
/// Combines elements of Eagle and SaI algorithms for improved edge detection
/// with sophisticated weighted blending for smooth transitions.
/// </para>
/// <para>Developed by Kreed in 1999.</para>
/// </remarks>
[ScalerInfo("SuperEagle", Author = "Kreed", Year = 1999,
  Description = "Enhanced Eagle with SaI-style edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct SuperEagle : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

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
    => callback.Invoke(new SuperEagleKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by SuperEagle.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether SuperEagle supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for SuperEagle.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default SuperEagle configuration.
  /// </summary>
  public static SuperEagle Default => new();
}

/// <summary>
/// Internal kernel for SuperEagle algorithm.
/// </summary>
/// <remarks>
/// SuperEagle uses a 4x4 neighborhood with complex weighted blending:
///
/// C0 C1 C2      (row -1)
/// C3 C4 C5 D4   (row 0, center row)
/// C6 C7 C8 D5   (row +1)
///    D1 D2      (row +2)
///
/// Output 2x2 block:
/// E00 E01
/// E10 E11
/// </remarks>
file readonly struct SuperEagleKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
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
    // Get the neighborhood
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    var c0 = window.M1M1; // top-left (row -1, col -1)
    var c1 = window.M1P0; // top (row -1, col 0)
    var c2 = window.M1P1; // top-right (row -1, col +1)
    var c3 = window.P0M1; // left (row 0, col -1)
    var c4 = window.P0P0; // center (row 0, col 0)
    var c5 = window.P0P1; // right (row 0, col +1)
    var d4 = window.P0P2; // far-right (row 0, col +2)
    var c6 = window.P1M1; // bottom-left (row +1, col -1)
    var c7 = window.P1P0; // bottom (row +1, col 0)
    var c8 = window.P1P1; // bottom-right (row +1, col +1)
    var d5 = window.P1P2; // bottom far-right (row +1, col +2)
    var d1 = window.P2P0; // far-bottom (row +2, col 0)
    var d2 = window.P2P1; // far-bottom right (row +2, col +1)

    var c4Work = c4.Work;
    var c5Work = c5.Work;
    var c7Work = c7.Work;
    var c8Work = c8.Work;

    // Default outputs
    var e00 = c4Work;
    var e01 = c4Work;
    var e10 = c4Work;
    var e11 = c4Work;

    var c4Like8 = equality.Equals(c4.Key, c8.Key);

    if (c4Like8) {
      var c48 = lerp.Lerp(c4Work, c8Work);
      var c7Like5 = equality.Equals(c7.Key, c5.Key);

      if (c7Like5) {
        var c57 = lerp.Lerp(c5Work, c7Work);
        var conc2D = 0;
        conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c6.Key, d1.Key);
        conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c3.Key, c1.Key);
        conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, d2.Key, d5.Key);
        conc2D += this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c2.Key, d4.Key);

        if (conc2D > 0) {
          e10 = c57;
          e01 = c57;
          e00 = e11 = lerp.Lerp(c48, c57);
        } else if (conc2D < 0) {
          e01 = e10 = lerp.Lerp(c48, c57);
        } else {
          e01 = e10 = c57;
        }
      } else {
        // C4 like C8 but C7 not like C5
        var c48Like1 = equality.Equals(c4.Key, c1.Key) || equality.Equals(c8.Key, c1.Key);
        var c48LikeD5 = equality.Equals(c4.Key, d5.Key) || equality.Equals(c8.Key, d5.Key);

        if (c48Like1 && c48LikeD5)
          e01 = lerp.Lerp(lerp.Lerp(c48, c1.Work, d5.Work), c5Work, 3, 1);
        else if (c48Like1)
          e01 = lerp.Lerp(lerp.Lerp(c48, c1.Work), c5Work, 3, 1);
        else if (c48LikeD5)
          e01 = lerp.Lerp(lerp.Lerp(c48, d5.Work), c5Work, 3, 1);
        else
          e01 = lerp.Lerp(c48, c5Work);

        var c48LikeD2 = equality.Equals(c4.Key, d2.Key) || equality.Equals(c8.Key, d2.Key);
        var c48Like3 = equality.Equals(c4.Key, c3.Key) || equality.Equals(c8.Key, c3.Key);

        if (c48LikeD2 && c48Like3)
          e10 = lerp.Lerp(lerp.Lerp(c48, d2.Work, c3.Work), c7Work, 3, 1);
        else if (c48LikeD2)
          e10 = lerp.Lerp(lerp.Lerp(c48, d2.Work), c7Work, 3, 1);
        else if (c48Like3)
          e10 = lerp.Lerp(lerp.Lerp(c48, c3.Work), c7Work, 3, 1);
        else
          e10 = lerp.Lerp(c48, c7Work);
      }
    } else {
      var c7Like5 = equality.Equals(c7.Key, c5.Key);

      if (c7Like5) {
        var c57 = lerp.Lerp(c5Work, c7Work);
        e01 = c57;
        e10 = c57;

        var c57Like6 = equality.Equals(c5.Key, c6.Key) || equality.Equals(c7.Key, c6.Key);
        var c57Like2 = equality.Equals(c5.Key, c2.Key) || equality.Equals(c7.Key, c2.Key);

        if (c57Like6 && c57Like2)
          e00 = lerp.Lerp(lerp.Lerp(c57, c6.Work, c2.Work), c4Work, 3, 1);
        else if (c57Like6)
          e00 = lerp.Lerp(lerp.Lerp(c57, c6.Work), c4Work, 3, 1);
        else if (c57Like2)
          e00 = lerp.Lerp(lerp.Lerp(c57, c2.Work), c4Work, 3, 1);
        else
          e00 = lerp.Lerp(c57, c4Work);

        var c57LikeD4 = equality.Equals(c5.Key, d4.Key) || equality.Equals(c7.Key, d4.Key);
        var c57LikeD1 = equality.Equals(c5.Key, d1.Key) || equality.Equals(c7.Key, d1.Key);

        if (c57LikeD4 && c57LikeD1)
          e11 = lerp.Lerp(lerp.Lerp(c57, d4.Work, d1.Work), c8Work, 3, 1);
        else if (c57LikeD4)
          e11 = lerp.Lerp(lerp.Lerp(c57, d4.Work), c8Work, 3, 1);
        else if (c57LikeD1)
          e11 = lerp.Lerp(lerp.Lerp(c57, d1.Work), c8Work, 3, 1);
        else
          e11 = lerp.Lerp(c57, c8Work);
      } else {
        // Neither diagonal matches - use weighted blending
        e11 = lerp.Lerp(c8Work, c7Work, c5Work, 6, 1, 1);
        e00 = lerp.Lerp(c4Work, c7Work, c5Work, 6, 1, 1);
        e10 = lerp.Lerp(c7Work, c4Work, c8Work, 6, 1, 1);
        e01 = lerp.Lerp(c5Work, c4Work, c8Work, 6, 1, 1);
      }
    }

    // Write to destination
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    row0[0] = encoder.Encode(e00);
    row0[1] = encoder.Encode(e01);
    row1[0] = encoder.Encode(e10);
    row1[1] = encoder.Encode(e11);
  }

  /// <summary>
  /// Computes concurrency value for diagonal detection.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _Conc2D(TKey c00, TKey c01, TKey c10, TKey c11, TKey test0, TKey test1) {
    var result = 0;

    var acLike = equality.Equals(c00, c10);
    var x = acLike ? 1 : 0;
    var y = equality.Equals(c01, c10) && !acLike ? 1 : 0;

    var adLike = equality.Equals(c00, c11);
    x += adLike ? 1 : 0;
    y += equality.Equals(c01, c11) && !adLike ? 1 : 0;

    if (x <= 1)
      ++result;
    if (y <= 1)
      --result;

    return result;
  }
}

#endregion
