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
/// Adjusts color balance in shadows, midtones, and highlights independently.
/// </summary>
[FilterInfo("ColorBalance",
  Description = "Adjust color balance in shadows, midtones, and highlights", Category = FilterCategory.ColorCorrection)]
public readonly struct ColorBalance(
  float shadowsCyanRed = 0f,
  float shadowsMagentaGreen = 0f,
  float shadowsYellowBlue = 0f,
  float midtonesCyanRed = 0f,
  float midtonesMagentaGreen = 0f,
  float midtonesYellowBlue = 0f,
  float highlightsCyanRed = 0f,
  float highlightsMagentaGreen = 0f,
  float highlightsYellowBlue = 0f
)
  : IPixelFilter {
  private readonly float _sCR = Math.Max(-1f, Math.Min(1f, shadowsCyanRed)), _sMG = Math.Max(-1f, Math.Min(1f, shadowsMagentaGreen)), _sYB = Math.Max(-1f, Math.Min(1f, shadowsYellowBlue));
  private readonly float _mCR = Math.Max(-1f, Math.Min(1f, midtonesCyanRed)), _mMG = Math.Max(-1f, Math.Min(1f, midtonesMagentaGreen)), _mYB = Math.Max(-1f, Math.Min(1f, midtonesYellowBlue));
  private readonly float _hCR = Math.Max(-1f, Math.Min(1f, highlightsCyanRed)), _hMG = Math.Max(-1f, Math.Min(1f, highlightsMagentaGreen)), _hYB = Math.Max(-1f, Math.Min(1f, highlightsYellowBlue));

  public ColorBalance() : this(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f) { }

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
    => callback.Invoke(new ColorBalanceKernel<TWork, TKey, TPixel, TEncode>(
      this._sCR, this._sMG, this._sYB,
      this._mCR, this._mMG, this._mYB,
      this._hCR, this._hMG, this._hYB));

  public static ColorBalance Default => new();
}

file readonly struct ColorBalanceKernel<TWork, TKey, TPixel, TEncode>(
  float sCR, float sMG, float sYB,
  float mCR, float mMG, float mYB,
  float hCR, float hMG, float hYB)
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
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);
    var lum = ColorMatrices.BT601_R * r + ColorMatrices.BT601_G * g + ColorMatrices.BT601_B * b;

    var shadowW = 1f - Math.Min(1f, lum * 4f);
    var highlightW = Math.Max(0f, (lum - 0.75f) * 4f);
    var midW = 1f - shadowW - highlightW;

    var rShift = shadowW * sCR + midW * mCR + highlightW * hCR;
    var gShift = shadowW * sMG + midW * mMG + highlightW * hMG;
    var bShift = shadowW * sYB + midW * mYB + highlightW * hYB;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Max(0f, Math.Min(1f, r + rShift * 0.25f)),
      Math.Max(0f, Math.Min(1f, g + gShift * 0.25f)),
      Math.Max(0f, Math.Min(1f, b + bShift * 0.25f)), a));
  }
}
