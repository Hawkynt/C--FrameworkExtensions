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
/// Halftone — amplitude-modulated (AM) circular dot screen.
/// </summary>
/// <remarks>
/// <para>Simulates classical photo-mechanical halftone printing (Talbot 1850s, Meisenbach
/// 1882): tonal value is encoded by varying the SIZE of dots on a fixed grid rather
/// than dot density (frequency modulation). Default rotation angle is 45° to minimise
/// perceived dot patterns. Each output pixel is black when the local luminance
/// dictates a dot covering that location, otherwise white.</para>
/// <para>Reference: any reprographics textbook; e.g. M. Yule, "Principles of Color
/// Reproduction" (Wiley 1967), Chapter 9. For the FM-halftone alternative
/// (stochastic / blue-noise screening), see <see cref="HalftonePattern"/>.</para>
/// </remarks>
[FilterInfo("Halftone",
  Description = "Print-style halftone dot pattern effect", Category = FilterCategory.Artistic)]
public readonly struct Halftone : IPixelFilter, IFrameFilter {
  private readonly int _dotSize;
  private readonly float _cos;
  private readonly float _sin;

  public Halftone() : this(6, 45f) { }

  public Halftone(int dotSize = 6, float angle = 45f) {
    this._dotSize = Math.Max(2, dotSize);
    var rad = angle * (float)(Math.PI / 180.0);
    this._cos = (float)Math.Cos(rad);
    this._sin = (float)Math.Sin(rad);
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
    => throw new NotSupportedException("Halftone requires IFrameFilter dispatch (UsesFrameAccess=true); IPixelFilter direct invocation is not supported. Use Bitmap.ApplyFilter(...) which routes IFrameFilter filters through the resampler pipeline.");

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
    => callback.Invoke(new HalftoneFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._dotSize, this._cos, this._sin, sourceWidth, sourceHeight));

  public static Halftone Default => new();
}

file readonly struct HalftoneFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int dotSize, float cos, float sin, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => dotSize;
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
    var rx = cos * destX + sin * destY;
    var ry = -sin * destX + cos * destY;

    var cx = (float)(Math.Floor(rx / dotSize) * dotSize + dotSize * 0.5);
    var cy = (float)(Math.Floor(ry / dotSize) * dotSize + dotSize * 0.5);

    var dx = rx - cx;
    var dy = ry - cy;
    var distance = (float)Math.Sqrt(dx * dx + dy * dy);

    var center = frame[destX, destY].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in center);
    var luminance = 0.299f * r + 0.587f * g + 0.114f * b;

    float or, og, ob;
    // Classical AM halftone (offset-printing convention): on a white background, ink
    // coverage rises with darkness — black dots grow LARGER as luminance DECREASES.
    // Use (1 − luminance) so bright cells get tiny dots and black cells fully fill.
    if (distance < dotSize * 0.5f * (1f - luminance)) {
      or = 0f;
      og = 0f;
      ob = 0f;
    } else {
      or = 1f;
      og = 1f;
      ob = 1f;
    }

    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(or, og, ob, a));
  }
}
