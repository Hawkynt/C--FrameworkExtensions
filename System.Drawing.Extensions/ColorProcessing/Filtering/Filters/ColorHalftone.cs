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
/// CMYK color halftone effect — simulates process-color printing with per-channel
/// halftone screens at different angles, producing overlapping colored dots.
/// </summary>
[FilterInfo("ColorHalftone",
  Description = "CMYK color halftone with per-channel screen angles", Category = FilterCategory.Artistic)]
public readonly struct ColorHalftone : IPixelFilter, IFrameFilter {
  private readonly int _maxRadius;
  private readonly float _cCos, _cSin;
  private readonly float _mCos, _mSin;
  private readonly float _yCos, _ySin;
  private readonly float _kCos, _kSin;

  public ColorHalftone() : this(4, 108f, 162f, 90f, 45f) { }

  public ColorHalftone(int maxRadius = 4, float cAngle = 108f, float mAngle = 162f, float yAngle = 90f, float kAngle = 45f) {
    this._maxRadius = Math.Max(1, maxRadius);
    var cRad = cAngle * (float)(Math.PI / 180.0);
    var mRad = mAngle * (float)(Math.PI / 180.0);
    var yRad = yAngle * (float)(Math.PI / 180.0);
    var kRad = kAngle * (float)(Math.PI / 180.0);
    this._cCos = (float)Math.Cos(cRad);
    this._cSin = (float)Math.Sin(cRad);
    this._mCos = (float)Math.Cos(mRad);
    this._mSin = (float)Math.Sin(mRad);
    this._yCos = (float)Math.Cos(yRad);
    this._ySin = (float)Math.Sin(yRad);
    this._kCos = (float)Math.Cos(kRad);
    this._kSin = (float)Math.Sin(kRad);
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
    => callback.Invoke(new ColorHalftonePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new ColorHalftoneFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._maxRadius,
      this._cCos, this._cSin, this._mCos, this._mSin,
      this._yCos, this._ySin, this._kCos, this._kSin,
      sourceWidth, sourceHeight));

  public static ColorHalftone Default => new();
}

file readonly struct ColorHalftonePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct ColorHalftoneFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int maxRadius,
  float cCos, float cSin, float mCos, float mSin,
  float yCos, float ySin, float kCos, float kSin,
  int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => maxRadius * 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => sourceWidth;
  public int TargetHeight => sourceHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _DotInCell(float x, float y, float cos, float sin, int cellSize, float channelValue) {
    var rx = cos * x + sin * y;
    var ry = -sin * x + cos * y;
    var cx = (float)(Math.Floor(rx / cellSize) * cellSize + cellSize * 0.5);
    var cy = (float)(Math.Floor(ry / cellSize) * cellSize + cellSize * 0.5);
    var dx = rx - cx;
    var dy = ry - cy;
    var dist = (float)Math.Sqrt(dx * dx + dy * dy);
    return dist < channelValue * cellSize * 0.5f ? 1f : 0f;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest, int destStride,
    in TEncode encoder) {
    var px = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);

    var k = Math.Min(1f - r, Math.Min(1f - g, 1f - b));
    float c, m, y;
    if (k >= 1f) {
      c = 0f;
      m = 0f;
      y = 0f;
    } else {
      var invK = 1f / (1f - k);
      c = (1f - r - k) * invK;
      m = (1f - g - k) * invK;
      y = (1f - b - k) * invK;
    }

    var cellSize = maxRadius * 2;
    var cDot = _DotInCell(destX, destY, cCos, cSin, cellSize, c);
    var mDot = _DotInCell(destX, destY, mCos, mSin, cellSize, m);
    var yDot = _DotInCell(destX, destY, yCos, ySin, cellSize, y);
    var kDot = _DotInCell(destX, destY, kCos, kSin, cellSize, k);

    var outR = (1f - cDot) * (1f - kDot);
    var outG = (1f - mDot) * (1f - kDot);
    var outB = (1f - yDot) * (1f - kDot);
    outR = Math.Max(0f, Math.Min(1f, outR));
    outG = Math.Max(0f, Math.Min(1f, outG));
    outB = Math.Max(0f, Math.Min(1f, outB));

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(outR, outG, outB, a));
  }
}
