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
    this._strength = ColorConverter.Saturate(strength);
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
    => throw new NotSupportedException("Fisheye requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
      // Equidistant fisheye projection (Wikipedia "Fisheye lens"): destination radius r ∈ [0,1]
      // maps to source radius r_src = tan(r·θ_max) / tan(θ_max), where θ_max ∈ (0, π/2)
      // is the half-FOV. r=0 → 0 and r=1 → 1 (corners anchored); interior is monotonically
      // contracted toward the center, producing the characteristic center-magnified bulge.
      // strength controls θ_max linearly; cap at 0.45π so strength=1 stays well-behaved
      // (avoids tan(π/2)=∞ singularity). At strength=1 dest r=0.5 samples src ≈ 0.20
      // (clear center magnification); at strength→0 the mapping limits to identity.
      var thetaMax = strength * 0.45f * (float)Math.PI;
      float rOut;
      if (thetaMax < 1e-4f) {
        rOut = r;
      } else {
        var rNew = (float)Math.Tan(r * thetaMax) / (float)Math.Tan(thetaMax);
        rOut = rNew;
      }
      var sxF = cx + (dx / r) * rOut * maxR;
      var syF = cy + (dy / r) * rOut * maxR;
      sx = (int)sxF;
      sy = (int)syF;
      if (sx < 0) sx = 0; else if (sx >= sourceWidth) sx = sourceWidth - 1;
      if (sy < 0) sy = 0; else if (sy >= sourceHeight) sy = sourceHeight - 1;
    } else if (r > 1f) {
      // Outside unit disk → clamp to nearest valid sample on circle edge.
      var inv = r > 0f ? 1f / r : 0f;
      sx = (int)Math.Floor(cx + dx * inv * maxR);
      sy = (int)Math.Floor(cy + dy * inv * maxR);
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
