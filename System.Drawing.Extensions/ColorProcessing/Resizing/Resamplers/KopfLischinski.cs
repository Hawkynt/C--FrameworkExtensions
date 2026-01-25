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
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// Kopf-Lischinski depixelization algorithm for pixel art.
/// </summary>
/// <remarks>
/// <para>Algorithm: Depixelizing Pixel Art by Kopf and Lischinski (2011)</para>
/// <para>Reference: https://johanneskopf.de/publications/pixelart/</para>
/// <para>Paper: "Depixelizing Pixel Art" (SIGGRAPH 2011)</para>
/// <para></para>
/// <para>The algorithm builds a similarity graph between pixels using <typeparamref name="TEquality"/>,
/// resolves diagonal ambiguities using valence and curve heuristics, and produces smooth
/// output at any target resolution by interpolating based on edge relationships.</para>
/// <para></para>
/// <para>This is a raster-based approximation of the original vector algorithm.</para>
/// </remarks>
[ScalerInfo("Kopf-Lischinski", Author = "Kopf & Lischinski", Year = 2011,
  Url = "https://johanneskopf.de/publications/pixelart/paper/pixel.pdf",
  Description = "Depixelizing pixel art algorithm", Category = ScalerCategory.ContentAware)]
public readonly struct KopfLischinski : IEdgeAwareResampler {

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult>(
    IEdgeAwareResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TEquality, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    TEquality equality = default,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TEquality : struct, IColorEquality<TKey>
    => callback.Invoke(new KopfLischinskiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, equality, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static KopfLischinski Default => new();
}

#region Kopf-Lischinski Kernel

file struct KopfLischinskiKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>(
  int sourceWidth,
  int sourceHeight,
  int targetWidth,
  int targetHeight,
  TEquality equality,
  bool useCenteredGrid
)
  : IEdgeAwareResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode, TEquality>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TEquality : struct, IColorEquality<TKey> {
  private readonly TEquality _equality = equality;

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;

  // Heuristic weights for diagonal ambiguity resolution
  private const float ValenceWeight = 0.4f;
  private const float CurveWeight = 0.3f;

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {

    // Map destination pixel to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer coordinates of center source pixel
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts for subpixel position
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Get 3x3 neighborhood around the center pixel
    var nw = frame[x0 - 1, y0 - 1];
    var n = frame[x0, y0 - 1];
    var ne = frame[x0 + 1, y0 - 1];
    var w = frame[x0 - 1, y0];
    var c = frame[x0, y0];
    var e = frame[x0 + 1, y0];
    var sw = frame[x0 - 1, y0 + 1];
    var s = frame[x0, y0 + 1];
    var se = frame[x0 + 1, y0 + 1];

    // Build local similarity graph using TEquality
    var leftEdge = this._equality.Equals(c.Key, w.Key) ? 1f : 0f;
    var rightEdge = this._equality.Equals(c.Key, e.Key) ? 1f : 0f;
    var topEdge = this._equality.Equals(c.Key, n.Key) ? 1f : 0f;
    var bottomEdge = this._equality.Equals(c.Key, s.Key) ? 1f : 0f;

    // Diagonal edges for ambiguity resolution
    var neWeight = this._equality.Equals(sw.Key, ne.Key) ? 1f : 0f;
    var nwWeight = this._equality.Equals(se.Key, nw.Key) ? 1f : 0f;

    // Resolve diagonal ambiguities using heuristics
    if (neWeight > 0.5f && nwWeight > 0.5f) {
      var valenceNE = this._CalculateValence(frame, sw.Key, x0, y0);
      var valenceNW = this._CalculateValence(frame, nw.Key, x0, y0);

      var curveScoreNE = this._CalculateCurveScore(sw.Key, c.Key, ne.Key);
      var curveScoreNW = this._CalculateCurveScore(nw.Key, c.Key, se.Key);

      var totalNE = neWeight + valenceNE * ValenceWeight + curveScoreNE * CurveWeight;
      var totalNW = nwWeight + valenceNW * ValenceWeight + curveScoreNW * CurveWeight;

      if (totalNE > totalNW)
        nwWeight *= 0.2f;
      else
        neWeight *= 0.2f;
    }

    // Interpolate color based on edge connectivity and subpixel position
    var leftWeight = (1f - fx) * leftEdge * 0.5f;
    var rightWeight = fx * rightEdge * 0.5f;
    var topWeight = (1f - fy) * topEdge * 0.5f;
    var bottomWeight = fy * bottomEdge * 0.5f;
    var centerWeight = 1f - leftWeight - rightWeight - topWeight - bottomWeight;

    // Ensure non-negative weights
    if (centerWeight < 0f) {
      var scale = 1f / (1f - centerWeight);
      leftWeight *= scale;
      rightWeight *= scale;
      topWeight *= scale;
      bottomWeight *= scale;
      centerWeight = 0f;
    }

    // Accumulate weighted colors
    Accum4F<TWork> acc = default;
    acc.AddMul(c.Work, centerWeight);
    acc.AddMul(w.Work, leftWeight);
    acc.AddMul(e.Work, rightWeight);
    acc.AddMul(n.Work, topWeight);
    acc.AddMul(s.Work, bottomWeight);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _CalculateValence(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    in TKey cornerKey, int cx, int cy) {
    var valence = 0f;

    // Check similarity to each cardinal neighbor
    if (this._equality.Equals(cornerKey, frame[cx, cy - 1].Key)) valence += 1f;
    if (this._equality.Equals(cornerKey, frame[cx + 1, cy].Key)) valence += 1f;
    if (this._equality.Equals(cornerKey, frame[cx - 1, cy].Key)) valence += 1f;
    if (this._equality.Equals(cornerKey, frame[cx, cy + 1].Key)) valence += 1f;

    // Lower valence is better (sparse features take priority)
    return 4f - valence;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private float _CalculateCurveScore(in TKey p1, in TKey center, in TKey p2) {
    var score = 0f;

    // Check if the colors form a continuous curve
    if (this._equality.Equals(p1, center)) score += 1f;
    if (this._equality.Equals(center, p2)) score += 1f;

    return score;
  }
}

#endregion
