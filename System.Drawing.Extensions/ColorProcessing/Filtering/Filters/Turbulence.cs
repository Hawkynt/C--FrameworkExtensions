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
/// Turbulence distortion — displaces pixels using fractal value noise with configurable octaves.
/// </summary>
[FilterInfo("Turbulence",
  Description = "Fractal noise displacement with configurable octaves and strength", Category = FilterCategory.Distortion)]
public readonly struct Turbulence(float strength, float scale = 0.05f, int octaves = 4, int seed = 0) : IPixelFilter, IFrameFilter {
  private readonly float _strength = Math.Max(0f, strength);
  private readonly float _scale = Math.Max(0.001f, scale);
  private readonly int _octaves = Math.Max(1, Math.Min(8, octaves));

  public Turbulence() : this(10f, 0.05f, 4, 0) { }

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
    => callback.Invoke(new TurbulencePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new TurbulenceFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._strength, this._scale, this._octaves, seed, sourceWidth, sourceHeight));

  public static Turbulence Default => new();
}

file readonly struct TurbulencePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct TurbulenceFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float strength, float scale, int octaves, int seed, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => (int)Math.Ceiling(strength) + 1;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _HashF(int x, int y, int s) {
    var h = (uint)(x * 374761393 + y * 668265263 + s * 1274126177);
    h = (h ^ (h >> 13)) * 1274126177;
    h ^= h >> 16;
    return (h & 0xFFFF) / 65535f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _Noise(float x, float y, int s) {
    var ix = (int)Math.Floor(x);
    var iy = (int)Math.Floor(y);
    var fx = x - ix;
    var fy = y - iy;
    fx = fx * fx * (3f - 2f * fx);
    fy = fy * fy * (3f - 2f * fy);
    var a = _HashF(ix, iy, s);
    var b = _HashF(ix + 1, iy, s);
    var c = _HashF(ix, iy + 1, s);
    var d = _HashF(ix + 1, iy + 1, s);
    return a + fx * (b - a) + fy * (c - a) + fx * fy * (a - b - c + d);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var noiseX = 0f;
    var noiseY = 0f;
    var freq = scale;
    var amp = 1f;
    var totalAmp = 0f;

    for (var i = 0; i < octaves; ++i) {
      noiseX += (_Noise(destX * freq, destY * freq, seed) - 0.5f) * amp;
      noiseY += (_Noise(destX * freq, destY * freq, seed + 31) - 0.5f) * amp;
      totalAmp += amp;
      freq *= 2f;
      amp *= 0.5f;
    }

    if (totalAmp > 0f) {
      noiseX /= totalAmp;
      noiseY /= totalAmp;
    }

    var sx = (int)(destX + noiseX * strength * 2f);
    var sy = (int)(destY + noiseY * strength * 2f);

    var px = frame[sx, sy].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
