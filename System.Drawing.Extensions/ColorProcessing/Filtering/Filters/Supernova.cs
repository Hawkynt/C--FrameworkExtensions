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
/// Radial starburst light rays added to source image.
/// Computes exponential falloff from a configurable center with spoke-based angular modulation.
/// </summary>
[FilterInfo("Supernova",
  Description = "Radial starburst light rays added to source", Category = FilterCategory.Render)]
public readonly struct Supernova(float brightness = 0.7f, int spokes = 20, float posX = 0.5f, float posY = 0.5f, int seed = 0)
  : IPixelFilter, IFrameFilter {
  private readonly float _brightness = Math.Max(0f, Math.Min(1f, brightness));
  private readonly int _spokes = Math.Max(1, spokes);
  private readonly float _posX = Math.Max(0f, Math.Min(1f, posX));
  private readonly float _posY = Math.Max(0f, Math.Min(1f, posY));

  public Supernova() : this(0.7f, 20, 0.5f, 0.5f, 0) { }

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
    => callback.Invoke(new SupernovaPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new SupernovaFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._brightness, this._spokes, this._posX, this._posY, seed, sourceWidth, sourceHeight));

  public static Supernova Default => new();
}

file readonly struct SupernovaPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct SupernovaFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float brightness, int spokes, float posX, float posY, int seed, int sourceWidth, int sourceHeight)
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

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var cx = posX * sourceWidth;
    var cy = posY * sourceHeight;
    var dx = destX - cx;
    var dy = destY - cy;
    var dist = (float)Math.Sqrt(dx * dx + dy * dy);
    var angle = (float)Math.Atan2(dy, dx);
    var ray = brightness * (float)Math.Exp(-dist / (Math.Max(sourceWidth, sourceHeight) * 0.25f)) * (0.5f + 0.5f * (float)Math.Sin(angle * spokes + _Hash(destX, destY, seed)));

    r = Math.Max(0f, Math.Min(1f, r + ray));
    g = Math.Max(0f, Math.Min(1f, g + ray));
    b = Math.Max(0f, Math.Min(1f, b + ray));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
