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
/// Multi-octave plasma noise blended with source image.
/// Uses separate noise seeds per channel for colorful plasma patterns.
/// </summary>
[FilterInfo("PlasmaTexture",
  Description = "Multi-octave plasma noise blended with source", Category = FilterCategory.Render)]
public readonly struct PlasmaTexture(float scale = 0.02f, float blend = 0.5f, int seed = 0) : IPixelFilter, IFrameFilter {
  private readonly float _scale = Math.Max(0.001f, scale);
  private readonly float _blend = Math.Max(0f, Math.Min(1f, blend));

  public PlasmaTexture() : this(0.02f, 0.5f, 0) { }

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
    => callback.Invoke(new PlasmaTexturePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new PlasmaTextureFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._scale, this._blend, seed, sourceWidth, sourceHeight));

  public static PlasmaTexture Default => new();
}

file readonly struct PlasmaTexturePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct PlasmaTextureFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float scale, float blend, int seed, int sourceWidth, int sourceHeight)
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
  private static float _Hash(int x, int y, int s) {
    var h = (uint)(x * 374761393 + y * 668265263 + s * 1274126177);
    h = (h ^ (h >> 13)) * 1274126177;
    h ^= h >> 16;
    return (h & 0xFFFF) / 32768f - 1f;
  }

  private static float _ValueNoise(float fx, float fy, int s) {
    var ix = (int)Math.Floor(fx);
    var iy = (int)Math.Floor(fy);
    var dx = fx - ix;
    var dy = fy - iy;
    var c00 = _Hash(ix, iy, s);
    var c10 = _Hash(ix + 1, iy, s);
    var c01 = _Hash(ix, iy + 1, s);
    var c11 = _Hash(ix + 1, iy + 1, s);
    var top = c00 + (c10 - c00) * dx;
    var bot = c01 + (c11 - c01) * dx;
    return top + (bot - top) * dy;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _MultiOctaveNoise(float x, float y, float noiseScale, int noiseSeed) {
    var value = 0f;
    var amplitude = 1f;
    var frequency = noiseScale;
    var totalAmplitude = 0f;
    for (var i = 0; i < 4; ++i) {
      value += _ValueNoise(x * frequency, y * frequency, noiseSeed + i * 7) * amplitude;
      totalAmplitude += amplitude;
      frequency *= 2f;
      amplitude *= 0.5f;
    }

    return value / totalAmplitude * 0.5f + 0.5f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var noiseR = _MultiOctaveNoise(destX, destY, scale, seed);
    var noiseG = _MultiOctaveNoise(destX, destY, scale, seed + 1);
    var noiseB = _MultiOctaveNoise(destX, destY, scale, seed + 2);

    r = r * (1f - blend) + noiseR * blend;
    g = g * (1f - blend) + noiseG * blend;
    b = b * (1f - blend) + noiseB * blend;

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
