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
/// Diffuse glow effect that creates a soft luminance-weighted bloom with optional grain.
/// Averages a small neighborhood for the glow component, then blends it with the original
/// pixel weighted by luminance. Optionally adds deterministic grain noise.
/// Always uses frame-level random access for neighborhood scanning.
/// </summary>
[FilterInfo("DiffuseGlow",
  Description = "Soft diffuse glow with luminance-weighted bloom and optional grain", Category = FilterCategory.Artistic)]
public readonly struct DiffuseGlow(float graininess, float glowAmount = 0.5f, float clearAmount = 0.5f)
  : IPixelFilter, IFrameFilter {
  private readonly float _graininess = Math.Max(0f, Math.Min(1f, graininess));
  private readonly float _glowAmount = Math.Max(0f, Math.Min(2f, glowAmount));
  private readonly float _clearAmount = Math.Max(0f, Math.Min(2f, clearAmount));

  public DiffuseGlow() : this(0f, 0.5f, 0.5f) { }

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
    => callback.Invoke(new DiffuseGlowPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new DiffuseGlowFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._graininess, this._glowAmount, this._clearAmount, sourceWidth, sourceHeight));

  public static DiffuseGlow Default => new();
}

file readonly struct DiffuseGlowPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct DiffuseGlowFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float graininess, float glowAmount, float clearAmount, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  private const int _RADIUS = 3;

  public int Radius => _RADIUS;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Hash(int x, int y) {
    var h = (uint)(x * 374761393 + y * 668265263);
    h = (h ^ (h >> 13)) * 1274126177;
    h ^= h >> 16;
    return (h & 0xFFFF) / 32768f - 1f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);
    var lum = ColorMatrices.BT601_R * cr + ColorMatrices.BT601_G * cg + ColorMatrices.BT601_B * cb;

    // Average neighborhood for glow component
    float blurR = 0, blurG = 0, blurB = 0;
    var count = 0;
    for (var dy = -_RADIUS; dy <= _RADIUS; ++dy)
    for (var dx = -_RADIUS; dx <= _RADIUS; ++dx) {
      var px = frame[destX + dx, destY + dy].Work;
      var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
      blurR += r;
      blurG += g;
      blurB += b;
      ++count;
    }

    var inv = 1f / count;
    blurR *= inv;
    blurG *= inv;
    blurB *= inv;

    // Blend: result = original * clearAmount + blurred * glowAmount * luminance
    var outR = cr * clearAmount + blurR * glowAmount * lum;
    var outG = cg * clearAmount + blurG * glowAmount * lum;
    var outB = cb * clearAmount + blurB * glowAmount * lum;

    // Add deterministic grain noise if graininess > 0
    if (graininess > 0f) {
      var noise = _Hash(destX, destY) * graininess;
      outR += noise;
      outG += noise;
      outB += noise;
    }

    outR = Math.Max(0f, Math.Min(1f, outR));
    outG = Math.Max(0f, Math.Min(1f, outG));
    outB = Math.Max(0f, Math.Min(1f, outB));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
