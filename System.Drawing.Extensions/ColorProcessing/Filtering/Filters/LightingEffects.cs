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
/// Phong-like lighting effect using luminance as a height map.
/// Computes Sobel gradients on luminance to derive surface normals, then applies
/// directional diffuse lighting from a configurable angle and elevation.
/// Always uses frame-level random access for Sobel neighborhood.
/// </summary>
[FilterInfo("LightingEffects",
  Description = "Phong-like directional lighting using luminance as height map", Category = FilterCategory.Artistic)]
public readonly struct LightingEffects(float angle, float elevation = 45f, float intensity = 1f, float ambientLight = 0.2f)
  : IPixelFilter, IFrameFilter {
  private readonly float _angle = angle;
  private readonly float _elevation = Math.Max(0f, Math.Min(90f, elevation));
  private readonly float _intensity = Math.Max(0f, Math.Min(5f, intensity));
  private readonly float _ambientLight = Math.Max(0f, Math.Min(1f, ambientLight));

  public LightingEffects() : this(315f, 45f, 1f, 0.2f) { }

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
    => callback.Invoke(new LightingEffectsPassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new LightingEffectsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._angle, this._elevation, this._intensity, this._ambientLight, sourceWidth, sourceHeight));

  public static LightingEffects Default => new();
}

file readonly struct LightingEffectsPassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct LightingEffectsFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  float angle, float elevation, float intensity, float ambientLight, int sourceWidth, int sourceHeight)
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
  private static float _Lum(in NeighborPixel<TWork, TKey> p) {
    var px = p.Work;
    var (r, g, b, _) = ColorConverter.GetNormalizedRgba(in px);
    return ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var center = frame[destX, destY].Work;
    var (cr, cg, cb, ca) = ColorConverter.GetNormalizedRgba(in center);

    // Sobel gradient on luminance
    var tl = _Lum(frame[destX - 1, destY - 1]);
    var t = _Lum(frame[destX, destY - 1]);
    var tr = _Lum(frame[destX + 1, destY - 1]);
    var l = _Lum(frame[destX - 1, destY]);
    var r = _Lum(frame[destX + 1, destY]);
    var bl = _Lum(frame[destX - 1, destY + 1]);
    var b = _Lum(frame[destX, destY + 1]);
    var br = _Lum(frame[destX + 1, destY + 1]);

    // Sobel X: [-1,0,1; -2,0,2; -1,0,1]
    var gx = -tl + tr - 2f * l + 2f * r - bl + br;
    // Sobel Y: [-1,-2,-1; 0,0,0; 1,2,1]
    var gy = -tl - 2f * t - tr + bl + 2f * b + br;

    // Light direction from angle and elevation
    var angleRad = angle * (float)(Math.PI / 180.0);
    var elevRad = elevation * (float)(Math.PI / 180.0);
    var cosElev = (float)Math.Cos(elevRad);
    var lx = (float)Math.Cos(angleRad) * cosElev;
    var ly = (float)Math.Sin(angleRad) * cosElev;
    var lz = (float)Math.Sin(elevRad);

    // Surface normal from gradient: (-gx, -gy, 1) normalized
    var nx = -gx;
    var ny = -gy;
    const float nz = 1f;
    var len = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
    if (len > 0f) {
      var invLen = 1f / len;
      nx *= invLen;
      ny *= invLen;
    }

    var nzNorm = nz / (len > 0f ? len : 1f);

    // Diffuse = max(0, dot(normal, light))
    var diffuse = Math.Max(0f, nx * lx + ny * ly + nzNorm * lz);

    // Result = original * (ambientLight + intensity * diffuse)
    var factor = ambientLight + intensity * diffuse;
    var outR = Math.Max(0f, Math.Min(1f, cr * factor));
    var outG = Math.Max(0f, Math.Min(1f, cg * factor));
    var outB = Math.Max(0f, Math.Min(1f, cb * factor));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, ca));
  }
}
