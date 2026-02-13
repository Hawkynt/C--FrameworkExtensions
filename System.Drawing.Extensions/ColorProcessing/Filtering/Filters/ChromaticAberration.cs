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
using Hawkynt.ColorProcessing.Resizing;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Filtering.Filters;

/// <summary>
/// Chromatic aberration — simulates lens chromatic aberration by shifting R/G/B channels
/// radially by different amounts. Position-dependent; always uses frame-level random access.
/// </summary>
[FilterInfo("ChromaticAberration",
  Description = "Simulates lens chromatic aberration with radial RGB channel shifting", Category = FilterCategory.Artistic)]
public readonly struct ChromaticAberration(float strength) : IPixelFilter, IFrameFilter {
  private readonly float _strength = Math.Max(0f, strength);

  public ChromaticAberration() : this(1f) { }

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
    => callback.Invoke(new ChromaticAberrationPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new ChromaticAberrationFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, sourceWidth, sourceHeight));

  public static ChromaticAberration Default => new();
}

file readonly struct ChromaticAberrationPassThroughKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder)
    => dest[0] = encoder.Encode(window.P0P0.Work);
}

file readonly struct ChromaticAberrationFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strength, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => (int)Math.Ceiling(strength);
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
    var cx = sourceWidth * 0.5f;
    var cy = sourceHeight * 0.5f;
    var dx = destX - cx;
    var dy = destY - cy;
    var dist = (float)Math.Sqrt(dx * dx + dy * dy);

    if (dist < 0.001f) {
      dest[destY * destStride + destX] = encoder.Encode(frame[destX, destY].Work);
      return;
    }

    var ndx = dx / dist;
    var ndy = dy / dist;
    var shift = strength * (dist / Math.Max(cx, cy));

    var rsx = (int)Math.Round(destX + ndx * shift);
    var rsy = (int)Math.Round(destY + ndy * shift);
    var bsx = (int)Math.Round(destX - ndx * shift);
    var bsy = (int)Math.Round(destY - ndy * shift);

    var rPixel = frame[rsx, rsy].Work;
    var gPixel = frame[destX, destY].Work;
    var bPixel = frame[bsx, bsy].Work;

    var (rr, _, _, _) = ColorConverter.GetNormalizedRgba(in rPixel);
    var (_, gg, _, ga) = ColorConverter.GetNormalizedRgba(in gPixel);
    var (_, _, bb, _) = ColorConverter.GetNormalizedRgba(in bPixel);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(rr, gg, bb, ga));
  }
}
