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
/// Chromatic aberration — simulates transverse-CA lens artefacts via radial RGB channel
/// shifting (red samples inward, blue outward).
/// </summary>
/// <remarks>
/// <para>Models transverse (lateral) chromatic aberration: the wavelength-dependent
/// magnification differences in real lenses cause red and blue components to land
/// at different distances from the optical axis. The blue (short-wavelength) image
/// is magnified more than red (long-wavelength), so blue is sampled radially
/// outward and red radially inward.</para>
/// <para>Reference: E. Hecht, "Optics" (5th ed., Pearson 2017), §6.3.2 (transverse
/// chromatic aberration). For longitudinal-CA modelling (out-of-focus colour
/// fringes) a depth-aware simulation would be required.</para>
/// </remarks>
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
    => throw new NotSupportedException("ChromaticAberration requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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

    // Canonical transverse CA convention (Hecht "Optics" §6.3.2): red is magnified
    // less than blue at typical positive-lens dispersion, so in the captured image
    // red appears at a smaller radius than blue (relative to green). Inverse warp:
    // red samples from source closer to center, blue samples from further out —
    // producing the familiar blue/purple fringe on the outside of bright edges.
    var rsx = (int)Math.Round(destX - ndx * shift);
    var rsy = (int)Math.Round(destY - ndy * shift);
    var bsx = (int)Math.Round(destX + ndx * shift);
    var bsy = (int)Math.Round(destY + ndy * shift);

    var rPixel = frame[rsx, rsy].Work;
    var gPixel = frame[destX, destY].Work;
    var bPixel = frame[bsx, bsy].Work;

    var (rr, _, _, _) = ColorConverter.GetNormalizedRgba(in rPixel);
    var (_, gg, _, ga) = ColorConverter.GetNormalizedRgba(in gPixel);
    var (_, _, bb, _) = ColorConverter.GetNormalizedRgba(in bPixel);

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(rr, gg, bb, ga));
  }
}
