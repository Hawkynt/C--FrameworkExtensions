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
/// Derek Liauw Kie Fa's 2xSaI (Scale and Interpolation) pixel-art scaling algorithm.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using advanced diagonal edge detection and interpolation.</para>
/// <para>
/// 2xSaI uses a 4x4 neighborhood to detect edge directions and applies intelligent
/// interpolation to smooth diagonal lines while preserving sharp edges.
/// </para>
/// <para>One of the most popular pixel-art scalers, widely used in emulators.</para>
/// </remarks>
[ScalerInfo("2xSaI", Author = "Derek Liauw Kie Fa", Year = 1999,
  Description = "Scale and Interpolation - advanced diagonal edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct Sai2x : IPixelScaler, IScalerDispatch {

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

  /// <inheritdoc />
  Bitmap IScalerDispatch.Apply(Bitmap source, ScalerQuality quality)
    => quality switch {
      ScalerQuality.Fast => BitmapScalerExtensions.Upscale<
        Bgra8888, Bgra8888,
        IdentityDecode<Bgra8888>, IdentityProject<Bgra8888>, IdentityEncode<Bgra8888>,
        Sai2xKernel<Bgra8888, Bgra8888, Bgra8888, ExactEquality<Bgra8888>, Color4BLerp<Bgra8888>, IdentityEncode<Bgra8888>>
      >(source, new()),
      ScalerQuality.HighQuality => BitmapScalerExtensions.Upscale<
        LinearRgbaF, OklabF,
        Srgb32ToLinearRgbaF, LinearRgbaFToOklabF, LinearRgbaFToSrgb32,
        Sai2xKernel<LinearRgbaF, OklabF, Bgra8888, ThresholdEquality<OklabF, Euclidean3<OklabF>>, LinearRgbaFLerp, LinearRgbaFToSrgb32>
      >(source, new(new(0.02f))),
      _ => throw new System.NotSupportedException($"Quality {quality} is not supported for 2xSaI.")
    };

  /// <summary>
  /// Gets the list of scale factors supported by 2xSaI.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether 2xSaI supports the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor to check.</param>
  /// <returns><c>true</c> if the scale is 2x2; otherwise, <c>false</c>.</returns>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for 2xSaI.
  /// </summary>
  /// <param name="sourceWidth">The source image width.</param>
  /// <param name="sourceHeight">The source image height.</param>
  /// <returns>The target dimensions (2x in both dimensions).</returns>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default 2xSaI configuration.
  /// </summary>
  public static Sai2x Default => new();

  #region Nested Kernel Types

  /// <summary>
  /// Internal kernel for 2xSaI (Scale and Interpolation) algorithm.
  /// </summary>
  /// <remarks>
  /// 2xSaI uses a 4x4 neighborhood for analysis:
  ///
  /// C0 C1 C2 D3     (row -1)
  /// C3 C4 C5 D4     (row 0, center row)
  /// C6 C7 C8 D5     (row +1)
  /// D0 D1 D2        (row +2)
  ///
  /// Output 2x2 block:
  /// E00 E01
  /// E10 E11
  ///
  /// Originally by Derek Liauw Kie Fa, uses complex edge detection
  /// with comparison functions to determine pixel relationships.
  /// </remarks>
  private readonly struct Sai2xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(TEquality equality = default, TLerp lerp = default)
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
      var c0 = window.M1M1; // top-left
      var c1 = window.M1P0; // top
      var c2 = window.M1P1; // top-right
      var d3 = window.M1P2; // top far-right
      var c3 = window.P0M1; // left
      var c4 = window.P0P0; // center
      var c5 = window.P0P1; // right
      var d4 = window.P0P2; // far-right
      var c6 = window.P1M1; // bottom-left
      var c7 = window.P1P0; // bottom
      var c8 = window.P1P1; // bottom-right
      var d5 = window.P1P2; // bottom far-right
      var d0 = window.P2M1; // far-bottom left
      var d1 = window.P2P0; // far-bottom
      var d2 = window.P2P1; // far-bottom right

      var c4Work = c4.Work;

      // Default all outputs to center pixel
      var e00 = c4Work;
      var e01 = c4Work;
      var e10 = c4Work;
      var e11 = c4Work;

      var c4Like8 = equality.Equals(c4.Key, c8.Key);
      var c5Like7 = equality.Equals(c5.Key, c7.Key);

      if (c4Like8 && !c5Like7) {
        // Diagonal c4-c8 wins
        var c48 = lerp.Lerp(c4.Work, c8.Work);

        // E01 logic
        var c48Like1 = equality.Equals(c4.Key, c1.Key) || equality.Equals(c8.Key, c1.Key);
        var c5LikeD5 = equality.Equals(c5.Key, d5.Key);
        var c48Like7 = equality.Equals(c4.Key, c7.Key) || equality.Equals(c8.Key, c7.Key);
        var c48Like2 = equality.Equals(c4.Key, c2.Key) || equality.Equals(c8.Key, c2.Key);
        var c5NotLike1 = !equality.Equals(c5.Key, c1.Key);
        var c5LikeD3 = equality.Equals(c5.Key, d3.Key);

        if (!((c48Like1 && c5LikeD5) || (c48Like7 && c48Like2 && c5NotLike1 && c5LikeD3)))
          e01 = lerp.Lerp(c48, c5.Work);

        // E10 logic
        var c48Like3 = equality.Equals(c4.Key, c3.Key) || equality.Equals(c8.Key, c3.Key);
        var c7LikeD2 = equality.Equals(c7.Key, d2.Key);
        var c48Like5 = equality.Equals(c4.Key, c5.Key) || equality.Equals(c8.Key, c5.Key);
        var c48Like6 = equality.Equals(c4.Key, c6.Key) || equality.Equals(c8.Key, c6.Key);
        var c3NotLike7 = !equality.Equals(c3.Key, c7.Key);
        var c7LikeD0 = equality.Equals(c7.Key, d0.Key);

        if (!((c48Like3 && c7LikeD2) || (c48Like5 && c48Like6 && c3NotLike7 && c7LikeD0)))
          e10 = lerp.Lerp(c48, c7.Work);

      } else if (c5Like7 && !c4Like8) {
        // Diagonal c5-c7 wins
        var c57 = lerp.Lerp(c5.Work, c7.Work);

        // E01 logic
        var c57Like2 = equality.Equals(c5.Key, c2.Key) || equality.Equals(c7.Key, c2.Key);
        var c4Like6 = equality.Equals(c4.Key, c6.Key);
        var c57Like1 = equality.Equals(c5.Key, c1.Key) || equality.Equals(c7.Key, c1.Key);
        var c57Like8 = equality.Equals(c5.Key, c8.Key) || equality.Equals(c7.Key, c8.Key);
        var c4NotLike2 = !equality.Equals(c4.Key, c2.Key);
        var c4Like0 = equality.Equals(c4.Key, c0.Key);

        if (c57Like2 && c4Like6 || (c57Like1 && c57Like8 && c4NotLike2 && c4Like0))
          e01 = c57;
        else
          e01 = lerp.Lerp(c4Work, c57);

        // E10 logic
        var c57Like6 = equality.Equals(c5.Key, c6.Key) || equality.Equals(c7.Key, c6.Key);
        var c4Like2 = equality.Equals(c4.Key, c2.Key);
        var c57Like3 = equality.Equals(c5.Key, c3.Key) || equality.Equals(c7.Key, c3.Key);
        var c4NotLike6 = !equality.Equals(c4.Key, c6.Key);

        if (c57Like6 && c4Like2 || (c57Like3 && c57Like8 && c4NotLike6 && c4Like0))
          e10 = c57;
        else
          e10 = lerp.Lerp(c4Work, c57);

        e11 = c57;

      } else if (c4Like8 && c5Like7) {
        // Both diagonals match
        var c48 = lerp.Lerp(c4.Work, c8.Work);
        var c57 = lerp.Lerp(c5.Work, c7.Work);

        // Check if the blended diagonals produce similar results
        // When c4≈c8 and c5≈c7, the blends are similar if c4≈c5 (all four pixels similar)
        if (!equality.Equals(c4.Key, c5.Key)) {
          var conc2D = 0;
          conc2D += this._Conc2D(c4.Key, c8.Key, c5.Key, c7.Key, c3.Key, c1.Key);
          conc2D -= this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, d4.Key, c2.Key);
          conc2D -= this._Conc2D(c5.Key, c7.Key, c4.Key, c8.Key, c6.Key, d1.Key);
          conc2D += this._Conc2D(c4.Key, c8.Key, c5.Key, c7.Key, d5.Key, d2.Key);

          if (conc2D < 0)
            e11 = c57;
          else if (conc2D == 0)
            e11 = lerp.Lerp(c48, c57);

          e10 = lerp.Lerp(c48, c57);
          e01 = lerp.Lerp(c48, c57);
        }

      } else {
        // Neither diagonal matches clearly - bilinear-ish fallback
        e11 = this._Lerp4(c4.Work, c5.Work, c7.Work, c8.Work);

        // E01 logic
        var c4Like7 = equality.Equals(c4.Key, c7.Key);
        var c4Like2 = equality.Equals(c4.Key, c2.Key);
        var c5NotLike1 = !equality.Equals(c5.Key, c1.Key);
        var c5LikeD3 = equality.Equals(c5.Key, d3.Key);
        var c5Like1 = equality.Equals(c5.Key, c1.Key);
        var c5Like8 = equality.Equals(c5.Key, c8.Key);
        var c4NotLike2 = !equality.Equals(c4.Key, c2.Key);
        var c4Like0 = equality.Equals(c4.Key, c0.Key);

        if (c4Like7 && c4Like2 && c5NotLike1 && c5LikeD3) {
          // Keep e01 = c4
        } else if (c5Like1 && c5Like8 && c4NotLike2 && c4Like0) {
          e01 = this._Lerp3(c5.Work, c1.Work, c8.Work);
        } else {
          e01 = lerp.Lerp(c4Work, c5.Work);
        }

        // E10 logic
        var c4Like5 = equality.Equals(c4.Key, c5.Key);
        var c4Like6 = equality.Equals(c4.Key, c6.Key);
        var c3NotLike7 = !equality.Equals(c3.Key, c7.Key);
        var c7LikeD0 = equality.Equals(c7.Key, d0.Key);
        var c7Like3 = equality.Equals(c7.Key, c3.Key);
        var c7Like8 = equality.Equals(c7.Key, c8.Key);
        var c4NotLike6 = !equality.Equals(c4.Key, c6.Key);

        if (c4Like5 && c4Like6 && c3NotLike7 && c7LikeD0) {
          // Keep e10 = c4
        } else if (c7Like3 && c7Like8 && c4NotLike6 && c4Like0) {
          e10 = this._Lerp3(c7.Work, c3.Work, c8.Work);
        } else {
          e10 = lerp.Lerp(c4Work, c7.Work);
        }
      }

      // Write directly to destination with encoding
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

    /// <summary>
    /// Blends three colors equally.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TWork _Lerp3(in TWork a, in TWork b, in TWork c) {
      var ab = lerp.Lerp(a, b, 0.5f);
      return lerp.Lerp(ab, c, 1f / 3f);
    }

    /// <summary>
    /// Blends four colors equally.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TWork _Lerp4(in TWork a, in TWork b, in TWork c, in TWork d) {
      var ab = lerp.Lerp(a, b, 0.5f);
      var cd = lerp.Lerp(c, d, 0.5f);
      return lerp.Lerp(ab, cd, 0.5f);
    }
  }

  #endregion
}
