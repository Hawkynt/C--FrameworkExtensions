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
/// Kaleidoscope distortion — reflects a single wedge of the source image
/// around the centre to create an N-fold rotationally symmetric pattern.
/// </summary>
/// <remarks>
/// <para>
/// For each destination pixel the polar coordinates <c>(r, θ)</c> are
/// computed relative to the image centre. The angle is folded into a single
/// wedge of width <c>π / N</c> (where <c>N</c> is the number of mirror
/// segments), alternating reflections at every wedge boundary to produce a
/// continuous seamless pattern. The folded angle is then rotated by
/// <c>offset</c> to choose which part of the source appears inside the
/// primary wedge.
/// </para>
/// <para>
/// This is a pure geometric remapping — no colour maths is performed and
/// alpha is preserved. Nearest-neighbour sampling is used (consistent with
/// the other distortion filters in this package, e.g. <see cref="Twirl"/>
/// and <see cref="Pinch"/>).
/// </para>
/// <para>
/// Typical use case: generative art, symmetric textures, procedural
/// backgrounds. With <c>segments = 6</c> and <c>offset = 0</c> the output
/// resembles a traditional hexagonal kaleidoscope view.
/// </para>
/// </remarks>
[FilterInfo("Kaleidoscope",
  Description = "N-fold mirrored kaleidoscope distortion", Category = FilterCategory.Distortion)]
public readonly struct Kaleidoscope(int segments, float offsetDegrees = 0f) : IPixelFilter, IFrameFilter {
  private readonly int _segments = Math.Max(2, segments);
  private readonly float _offset = offsetDegrees;

  public Kaleidoscope() : this(6, 0f) { }

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
    => callback.Invoke(new KaleidoscopePassThroughKernel<TWork, TKey, TPixel, TEncode>());

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
    => callback.Invoke(new KaleidoscopeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      this._segments, this._offset, sourceWidth, sourceHeight));

  /// <summary>Gets the default 6-segment kaleidoscope.</summary>
  public static Kaleidoscope Default => new();
}

file readonly struct KaleidoscopePassThroughKernel<TWork, TKey, TPixel, TEncode>
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

file readonly struct KaleidoscopeFrameKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int segments, float offsetDegrees, int sourceWidth, int sourceHeight)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => Math.Max(sourceWidth, sourceHeight);
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
    var cx = sourceWidth * 0.5f;
    var cy = sourceHeight * 0.5f;
    var dx = destX - cx;
    var dy = destY - cy;
    var radius = (float)Math.Sqrt(dx * dx + dy * dy);
    var theta = (float)Math.Atan2(dy, dx);

    // Wedge width in radians; alternate mirroring at every wedge boundary.
    var wedge = (float)(Math.PI / segments);

    // Bring θ to [0, 2·wedge); fold the upper half back onto the lower half.
    var twoWedge = 2f * wedge;
    var t = theta - twoWedge * (float)Math.Floor(theta / twoWedge);
    if (t > wedge)
      t = twoWedge - t;

    t += offsetDegrees * (float)(Math.PI / 180.0);

    var sx = (int)(cx + radius * (float)Math.Cos(t));
    var sy = (int)(cy + radius * (float)Math.Sin(t));

    var px = frame[sx, sy].Work;
    var (r, g, b, a) = ColorConverter.GetNormalizedRgba(in px);
    dest[destY * destStride + destX] = encoder.Encode(ColorConverter.FromNormalizedRgba<TWork>(r, g, b, a));
  }
}
