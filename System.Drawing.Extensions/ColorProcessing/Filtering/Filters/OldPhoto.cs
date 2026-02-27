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
/// Combined vintage photograph effect: sepia tone, reduced contrast, and warm color shift.
/// </summary>
[FilterInfo("OldPhoto",
  Description = "Vintage photograph effect with sepia tone and reduced contrast", Category = FilterCategory.Artistic)]
public readonly struct OldPhoto(float intensity = 1f) : IPixelFilter {
  private readonly float _intensity = Math.Max(0f, Math.Min(1f, intensity));

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
    => callback.Invoke(new OldPhotoKernel<TWork, TKey, TPixel, TEncode>(this._intensity));

  public static OldPhoto Default => new();
}

file readonly struct OldPhotoKernel<TWork, TKey, TPixel, TEncode>(float intensity)
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

    // Sepia matrix
    var sr = Math.Min(1f, r * 0.393f + g * 0.769f + b * 0.189f);
    var sg = Math.Min(1f, r * 0.349f + g * 0.686f + b * 0.168f);
    var sb = Math.Min(1f, r * 0.272f + g * 0.534f + b * 0.131f);

    // Reduce contrast
    sr = (sr - 0.5f) * (1f - 0.3f * intensity) + 0.5f;
    sg = (sg - 0.5f) * (1f - 0.3f * intensity) + 0.5f;
    sb = (sb - 0.5f) * (1f - 0.3f * intensity) + 0.5f;

    // Warm shift
    sr += 0.02f * intensity;
    sg += 0.01f * intensity;

    // Blend with original
    var or = r + (sr - r) * intensity;
    var og = g + (sg - g) * intensity;
    var ob = b + (sb - b) * intensity;

    dest[0] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(
      Math.Max(0f, Math.Min(1f, or)),
      Math.Max(0f, Math.Min(1f, og)),
      Math.Max(0f, Math.Min(1f, ob)), a));
  }
}
