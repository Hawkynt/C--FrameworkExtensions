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
/// Multi-angle crosshatch drawing effect based on luminance bands.
/// Draws 45-degree, -45-degree, and horizontal hatch lines depending on how dark
/// the pixel luminance is relative to configurable thresholds.
/// Always uses frame-level random access.
/// </summary>
[FilterInfo("Crosshatch",
  Description = "Multi-angle crosshatch drawing based on luminance bands", Category = FilterCategory.Artistic)]
public readonly struct Crosshatch(float threshold1, float threshold2, float threshold3, int spacing = 4) : IPixelFilter, IFrameFilter {
  private readonly float _threshold1 = Math.Max(0f, Math.Min(1f, threshold1));
  private readonly float _threshold2 = Math.Max(0f, Math.Min(1f, threshold2));
  private readonly float _threshold3 = Math.Max(0f, Math.Min(1f, threshold3));
  private readonly int _spacing = Math.Max(1, spacing);

  public Crosshatch() : this(0.2f, 0.5f, 0.8f, 4) { }

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
    => callback.Invoke(new CrosshatchPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new CrosshatchFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._threshold1, this._threshold2, this._threshold3, this._spacing, sourceWidth, sourceHeight));

  public static Crosshatch Default => new();
}

file readonly struct CrosshatchPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct CrosshatchFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float threshold1, float threshold2, float threshold3, int spacing, int sourceWidth, int sourceHeight)
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
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorMatrices.BT601_R * cr + ColorMatrices.BT601_G * cg + ColorMatrices.BT601_B * cb;

    // Start with white
    var value = 1f;

    // Darken with 45-degree lines for mid-to-dark regions
    if (lum < threshold3)
      if ((destX + destY) % spacing == 0)
        value -= 0.35f;

    // Darken with -45-degree lines for darker regions
    if (lum < threshold2)
      if ((destX - destY + 1000 * spacing) % spacing == 0)
        value -= 0.35f;

    // Darken with horizontal lines for very dark regions
    if (lum < threshold1)
      if (destY % spacing == 0)
        value -= 0.35f;

    value = Math.Max(0f, Math.Min(1f, value));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(value, value, value, ca));
  }
}
