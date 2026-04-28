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
/// Fisheye lens projection — implements an equidistant fisheye mapping centred on the
/// image, where source angle <c>θ = π·r_dest/2</c> from the optical axis is sampled
/// from the input via the standard pinhole inverse <c>r_src = tan(θ)/tan(θ_max)</c>.
/// Pixels outside the unit disk are clamped to the image edge.
/// </summary>
/// <remarks>
/// <para>
/// Distinct from <see cref="Spherize"/> (which is a smooth spherical-refraction
/// bulge/pinch with an adjustable amount): Fisheye applies a hard equidistant lens
/// projection with a fixed default field-of-view of 180°, producing the characteristic
/// circular wide-angle look used in security-cam imagery and VR captures.
/// </para>
/// <para>
/// Default <paramref name="strength"/> is 1.0 — a full fisheye. 0.0 returns the source
/// image. Values &gt; 1.0 are saturated.
/// </para>
/// </remarks>
[FilterInfo("Fisheye",
  Url = "https://en.wikipedia.org/wiki/Fisheye_lens",
  Description = "Equidistant fisheye lens projection (180° FOV)",
  Category = FilterCategory.Distortion)]
public readonly struct Fisheye : IPixelFilter, IFrameFilter {
  private readonly float _strength;

  public Fisheye() : this(1f) { }

  public Fisheye(float strength) {
    this._strength = Math.Max(0f, Math.Min(1f, strength));
  }

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
    => callback.Invoke(new FisheyePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new FisheyeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, sourceWidth, sourceHeight));

  public static Fisheye Default => new();
}

file readonly struct FisheyePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct FisheyeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strength, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(sourceWidth, sourceHeight);
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
    var maxR = Math.Min(cx, cy);

    var dx = (destX - cx) / maxR;
    var dy = (destY - cy) / maxR;
    var r = (float)Math.Sqrt(dx * dx + dy * dy);

    int sx, sy;
    if (r <= 1f && r > 0f) {
      // Equidistant fisheye: angle θ = r · π/2, then map back via tan.
      var theta = r * (float)(Math.PI / 2.0);
      var rPin = (float)Math.Tan(theta);
      var maxPin = (float)Math.Tan(Math.PI / 2.0 - 1e-3); // saturate at edge
      var rNew = rPin / maxPin;
      // Blend distorted ↔ original by strength.
      var rOut = strength * rNew + (1f - strength) * r;
      var sxF = cx + (dx / r) * rOut * maxR;
      var syF = cy + (dy / r) * rOut * maxR;
      sx = (int)sxF;
      sy = (int)syF;
      if (sx < 0) sx = 0; else if (sx >= sourceWidth) sx = sourceWidth - 1;
      if (sy < 0) sy = 0; else if (sy >= sourceHeight) sy = sourceHeight - 1;
    } else if (r > 1f) {
      // Outside unit disk → clamp to nearest valid sample on circle edge.
      var inv = r > 0f ? 1f / r : 0f;
      sx = (int)(cx + dx * inv * maxR);
      sy = (int)(cy + dy * inv * maxR);
      if (sx < 0) sx = 0; else if (sx >= sourceWidth) sx = sourceWidth - 1;
      if (sy < 0) sy = 0; else if (sy >= sourceHeight) sy = sourceHeight - 1;
    } else {
      sx = destX;
      sy = destY;
    }

    var px = frame[sx, sy].Work;
    var (rr, gg, bb, aa) = ColorConverter.GetNormalizedRgba(in px);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(rr, gg, bb, aa));
  }
}
