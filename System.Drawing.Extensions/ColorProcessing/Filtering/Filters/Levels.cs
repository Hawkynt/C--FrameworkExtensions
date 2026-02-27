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
/// Applies input/output level remapping with midtone gamma adjustment.
/// </summary>
[FilterInfo("Levels",
  Description = "Input/output level remapping with midtone gamma", Category = FilterCategory.ColorCorrection)]
public readonly struct Levels(
  float inBlack = 0f,
  float inWhite = 1f,
  float outBlack = 0f,
  float outWhite = 1f,
  float midtone = 1f
) : IPixelFilter {
  private readonly float _inBlack = inBlack;
  private readonly float _inWhite = inWhite;
  private readonly float _outBlack = outBlack;
  private readonly float _outWhite = outWhite;
  private readonly float _invMidtone = 1f / midtone;

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
    => callback.Invoke(new LevelsKernel<TWork, TKey, TPixel, TEncode>(
      this._inBlack, this._inWhite, this._outBlack, this._outWhite, this._invMidtone));

  public static Levels Default => new();
}

file readonly struct LevelsKernel<TWork, TKey, TPixel, TEncode>(
  float inBlack, float inWhite, float outBlack, float outWhite, float invMidtone)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 1;
  public int ScaleY => 1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _ApplyLevels(float v) {
    var inRange = inWhite - inBlack;
    v = inRange > 0f ? (v - inBlack) / inRange : 0f;
    v = Math.Max(0f, Math.Min(1f, v));
    v = (float)Math.Pow(v, invMidtone);
    return outBlack + v * (outWhite - outBlack);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var pixel = window.P0P0.Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in pixel);
    r = _ApplyLevels(r);
    g = _ApplyLevels(g);
    b = _ApplyLevels(b);
    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
