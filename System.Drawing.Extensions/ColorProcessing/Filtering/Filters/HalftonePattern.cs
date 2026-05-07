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
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Ordered dither halftone pattern producing black-and-white output based on luminance thresholds.
/// Supports dot (0), line (1), and cross (2) pattern types.
/// </summary>
[FilterInfo("HalftonePattern",
  Description = "Ordered dither halftone pattern", Category = FilterCategory.Artistic)]
public readonly struct HalftonePattern(int patternSize = 4, int patternType = 0) : IPixelFilter, IFrameFilter {

  /// <inheritdoc />
  public bool UsesFrameAccess => true;

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
    => throw new NotSupportedException("HalftonePattern requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

  /// <inheritdoc />
  public TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new HalftonePatternFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      patternSize, patternType, sourceWidth, sourceHeight));

  public static HalftonePattern Default => new(4, 0);
}

file readonly struct HalftonePatternFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int patternSize, int patternType, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    var lum = ColorConverter.LuminanceFromRgb(r, g, b);

    var ps = Math.Max(1, patternSize);
    float threshold;

    switch (patternType) {
      case 1:
        // Line pattern
        threshold = (destY % ps) / (float)ps;
        break;
      case 2:
        // Cross pattern
        threshold = Math.Min(destX % ps, destY % ps) / (float)ps;
        break;
      default:
        // Bayer 4×4 ordered-dither matrix per Bayer 1973 (also called "M-matrix"). Each
        // entry t ∈ [0, 15] is normalised by 16 to the [0, 1) threshold range.
        // Pattern (canonical order):
        //   [ 0  8  2 10]
        //   [12  4 14  6]
        //   [ 3 11  1  9]
        //   [15  7 13  5]
        // Reference: B.E. Bayer 1973, "An Optimum Method for Two-Level Rendition of
        // Continuous-Tone Pictures", IEEE ICC. The recursive doubling-construction is
        // standard but for `ps != 4` we fall back to a coarser modulus to preserve
        // the user-tuned cell size.
        if (ps == 4) {
          var bxx = destX & 3;
          var byy = destY & 3;
          var idx = byy * 4 + bxx;
          // Encoded as a flat lookup table (column-major sequence above).
          var bayer = idx switch {
            0  => 0,  1 =>  8,  2 =>  2,  3 => 10,
            4 => 12,  5 =>  4,  6 => 14,  7 =>  6,
            8 =>  3,  9 => 11, 10 =>  1, 11 =>  9,
            12 => 15, 13 =>  7, 14 => 13, 15 =>  5,
            _ => 0
          };
          threshold = (bayer + 0.5f) / 16f;
        } else {
          threshold = ((destX % ps + destY % ps * ps) % (ps * ps)) / (float)(ps * ps);
        }
        break;
    }

    var bw = lum > threshold ? 1f : 0f;
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(bw, bw, bw, a));
  }
}
