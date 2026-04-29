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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Fast Curvature-Based Interpolation (FCBI) resampler.
/// </summary>
/// <remarks>
/// <para>Edge-aware variant of ICBI that uses local second-derivative magnitude along the two
/// diagonals to pick the smoother direction, then performs linear interpolation along it.
/// Roughly 2-3× faster than the structure-tensor + Laplacian flavour of ICBI for similar
/// visual quality on edges.</para>
/// <para>Reference: Giachetti &amp; Asuni 2008, "Real-time artifact-free image upscaling",
/// Image and Vision Computing.</para>
/// <para>Algorithm (per output pixel):</para>
/// <list type="number">
/// <item>Sample the 2×2 source neighbourhood (c00, c10, c01, c11).</item>
/// <item>Estimate luminance second-derivative magnitudes along the NW–SE and NE–SW diagonals.</item>
/// <item>If an edge is detected (one diagonal noticeably smoother than the other), blend the
///   default bilinear sample with a 2-point lerp along the smoother diagonal, weighted by the
///   imbalance.</item>
/// <item>Otherwise, fall back to plain bilinear.</item>
/// </list>
/// </remarks>
[ScalerInfo("FCBI", Author = "Giachetti & Asuni", Year = 2008,
  Description = "Fast curvature-based edge-directed interpolation",
  Category = ScalerCategory.Resampler)]
public readonly struct Fcbi : IResampler {

  /// <summary>Default edge strength scaling (0.7).</summary>
  public const float DefaultEdgeStrength = 0.7f;

  private readonly float _edgeStrength;

  /// <summary>Creates an FCBI resampler with default edge-strength.</summary>
  public Fcbi() : this(DefaultEdgeStrength) { }

  /// <summary>Creates an FCBI resampler with custom edge-strength scaling.</summary>
  /// <param name="edgeStrength">
  /// Scales how strongly the diagonal-aware blend overrides plain bilinear when an edge is
  /// detected. 0 = always bilinear; 1 = full diagonal lerp at strongest detection. Clamped to [0,1].
  /// </param>
  public Fcbi(float edgeStrength) {
    ArgumentOutOfRangeException.ThrowIfNegative(edgeStrength);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(edgeStrength, 1f);
    this._edgeStrength = edgeStrength;
  }

  /// <summary>Gets the edge-strength scaling.</summary>
  public float EdgeStrength => this._edgeStrength == 0f ? DefaultEdgeStrength : this._edgeStrength;

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new FcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this.EdgeStrength, useCenteredGrid));

  /// <summary>Gets the default configuration.</summary>
  public static Fcbi Default => new();
}

file readonly struct FcbiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight,
  float edgeStrength, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // 2x2 source neighbourhood: c00 = NW, c10 = NE, c01 = SW, c11 = SE.
    var c00 = frame[x0, y0].Work;
    var c10 = frame[x0 + 1, y0].Work;
    var c01 = frame[x0, y0 + 1].Work;
    var c11 = frame[x0 + 1, y0 + 1].Work;

    // Plain bilinear baseline.
    var bilinear = BilinearInterpolate(c00, c10, c01, c11, fx, fy);

    // Per-channel sums as luminance proxy — avoids a Y-conversion in the hot loop and is
    // sufficient for choosing the smoother diagonal.
    var l00 = c00.C1 + c00.C2 + c00.C3;
    var l10 = c10.C1 + c10.C2 + c10.C3;
    var l01 = c01.C1 + c01.C2 + c01.C3;
    var l11 = c11.C1 + c11.C2 + c11.C3;

    // Diagonal "smoothness" magnitudes (FCBI: pick the diagonal whose endpoints differ less,
    // i.e., where there's no edge running across).
    var dNwSe = MathF.Abs(l00 - l11);
    var dNeSw = MathF.Abs(l10 - l01);
    var dSum = dNwSe + dNeSw;

    if (dSum < 1e-6f) {
      // Flat region — bilinear is correct.
      dest[destY * destStride + destX] = encoder.Encode(bilinear);
      return;
    }

    // Imbalance ∈ [0, 1]: 0 means both diagonals equally bumpy (no preferred edge direction);
    // 1 means one diagonal is perfectly smooth and the other carries all the variation.
    var imbalance = MathF.Abs(dNwSe - dNeSw) / dSum;
    var w = imbalance * edgeStrength;

    TWork directional;
    if (dNwSe < dNeSw) {
      // NW-SE is the smoother direction — interpolate along it. Project (fx, fy) onto the
      // NW-SE axis (where c00 sits at t=0 and c11 at t=1) to get the lerp parameter.
      var t = 0.5f * (fx + fy);
      directional = Lerp(c00, c11, t);
    } else {
      // NE-SW is smoother. c10 sits at t=0, c01 at t=1 along the / diagonal; project (fx, fy):
      var t = 0.5f * (fx + (1f - fy));
      directional = Lerp(c10, c01, t);
    }

    Accum4F<TWork> acc = default;
    acc.AddMul(bilinear, 1f - w);
    acc.AddMul(directional, w);
    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork BilinearInterpolate(in TWork c00, in TWork c10, in TWork c01, in TWork c11, float fx, float fy) {
    var invFx = 1f - fx;
    var invFy = 1f - fy;
    Accum4F<TWork> acc = default;
    acc.AddMul(c00, invFx * invFy);
    acc.AddMul(c10, fx * invFy);
    acc.AddMul(c01, invFx * fy);
    acc.AddMul(c11, fx * fy);
    return acc.Result;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Lerp(in TWork a, in TWork b, float t) {
    Accum4F<TWork> acc = default;
    acc.AddMul(a, 1f - t);
    acc.AddMul(b, t);
    return acc.Result;
  }
}
