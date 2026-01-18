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

using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

#region Xbr (Unified)

/// <summary>
/// XBR pixel-art scaling algorithm by Hyllian (2x, 3x, 4x).
/// </summary>
/// <remarks>
/// <para>Scales images using edge-directed interpolation with neighborhood analysis.</para>
/// <para>XBR uses both pattern matching (equality) and weighted distance metrics for optimal edge detection.</para>
/// <para>Developed by Hyllian in 2011 as an improvement over Scale2x with better edge handling.</para>
/// <para>Reference: http://board.byuu.org/viewtopic.php?f=10&amp;t=2248</para>
/// </remarks>
[ScalerInfo("XBR", Author = "Hyllian", Year = 2011,
  Description = "Edge-directed upscaler with neighborhood analysis", Category = ScalerCategory.PixelArt,
  Url = "http://board.byuu.org/viewtopic.php?f=10&t=2248")]
public readonly struct Xbr : IPixelScaler {

  private readonly int _scale;
  private readonly bool _allowAlphaBlending;
  private readonly bool _useOriginalImplementation;

  /// <summary>
  /// Creates an XBR scaler with specified scale factor and options.
  /// </summary>
  /// <param name="scale">The scale factor (2, 3, or 4).</param>
  /// <param name="allowAlphaBlending">Whether to allow alpha blending for smoother results.</param>
  /// <param name="useOriginalImplementation">Whether to use the original implementation variant (only used by 3x).</param>
  public Xbr(int scale = 2, bool allowAlphaBlending = true, bool useOriginalImplementation = false) {
    if (scale is not (2 or 3 or 4))
      throw new System.ArgumentOutOfRangeException(nameof(scale), scale, "XBR supports 2x, 3x, 4x scaling");
    this._scale = scale;
    this._allowAlphaBlending = allowAlphaBlending;
    this._useOriginalImplementation = useOriginalImplementation;
  }

  /// <summary>
  /// Gets whether alpha blending is enabled.
  /// </summary>
  public bool AllowAlphaBlending => this._allowAlphaBlending;

  /// <summary>
  /// Gets whether the original 3x implementation variant is used.
  /// </summary>
  public bool UseOriginalImplementation => this._useOriginalImplementation;

  /// <inheritdoc />
  public ScaleFactor Scale => this._scale == 0 ? new(2, 2) : new(this._scale, this._scale);

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
    => this._scale switch {
      0 or 2 => callback.Invoke(new Xbr2xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, equality, default, lerp)),
      3 => callback.Invoke(new Xbr3xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, this._useOriginalImplementation, equality, default, lerp)),
      4 => callback.Invoke(new Xbr4xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, equality, default, lerp)),
      _ => throw new System.InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported by XBR.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4)];

  /// <summary>
  /// Determines whether XBR supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>
  /// Creates a new XBR with the specified alpha blending setting.
  /// </summary>
  public Xbr WithAlphaBlending(bool allow) => new(this._scale, allow, this._useOriginalImplementation);

  /// <summary>
  /// Creates a new XBR 3x with the original implementation variant.
  /// </summary>
  public Xbr WithOriginalImplementation(bool useOriginal) => new(this._scale, this._allowAlphaBlending, useOriginal);

  /// <summary>Gets an XBR 2x scaler.</summary>
  public static Xbr Scale2x => new(2);

  /// <summary>Gets an XBR 3x scaler.</summary>
  public static Xbr Scale3x => new(3);

  /// <summary>Gets an XBR 4x scaler.</summary>
  public static Xbr Scale4x => new(4);

  /// <summary>Gets the default XBR configuration (2x).</summary>
  public static Xbr Default => new(2);
}

#endregion

#region XBR 2x Kernel

/// <summary>
/// Internal kernel for XBR 2x algorithm.
/// </summary>
/// <remarks>
/// XBR 2x uses a 5x5 neighborhood with edge-directed interpolation.
///
/// Neighborhood naming:
///         a1  b1  c1         (row -2, columns -1, 0, +1)
///     a0  pa  pb  pc  c4     (row -1, columns -2..-+2)
///     d0  pd  pe  pf  f4     (row 0, center row)
///     g0  pg  ph  pi  i4     (row +1)
///         g5  h5  i5         (row +2)
///
/// Output 2x2 block:
/// e0 e1
/// e2 e3
/// </remarks>
file readonly struct Xbr2xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode>(
  bool allowBlending = true,
  TEquality equality = default,
  TMetric metric = default,
  TLerp lerp = default
) : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int ScaleX => 2;

  /// <inheritdoc />
  public int ScaleY => 2;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Map NeighborWindow to XBR naming convention (5x5 kernel)
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    // Row -2: a1, b1, c1 (cols -1, 0, +1)
    var a1 = window.M2M1; // (row -2, col -1)
    var b1 = window.M2P0; // (row -2, col 0)
    var c1 = window.M2P1; // (row -2, col +1)

    // Row -1: a0, pa, pb, pc, c4 (cols -2, -1, 0, +1, +2)
    var a0 = window.M1M2; // (row -1, col -2)
    var pa = window.M1M1; // (row -1, col -1)
    var pb = window.M1P0; // (row -1, col 0) = top
    var pc = window.M1P1; // (row -1, col +1)
    var c4 = window.M1P2; // (row -1, col +2)

    // Row 0 (center): d0, pd, pe, pf, f4 (cols -2, -1, 0, +1, +2)
    var d0 = window.P0M2; // (row 0, col -2)
    var pd = window.P0M1; // (row 0, col -1) = left
    var pe = window.P0P0; // (row 0, col 0) = center
    var pf = window.P0P1; // (row 0, col +1) = right
    var f4 = window.P0P2; // (row 0, col +2)

    // Row +1: g0, pg, ph, pi, i4 (cols -2, -1, 0, +1, +2)
    var g0 = window.P1M2; // (row +1, col -2)
    var pg = window.P1M1; // (row +1, col -1)
    var ph = window.P1P0; // (row +1, col 0) = bottom
    var pi = window.P1P1; // (row +1, col +1)
    var i4 = window.P1P2; // (row +1, col +2)

    // Row +2: g5, h5, i5 (cols -1, 0, +1)
    var g5 = window.P2M1; // (row +2, col -1)
    var h5 = window.P2P0; // (row +2, col 0)
    var i5 = window.P2P1; // (row +2, col +1)

    // Initialize output to center pixel
    var e0 = pe.Work;
    var e1 = pe.Work;
    var e2 = pe.Work;
    var e3 = pe.Work;

    // Apply kernels for each corner (rotated 4 times)
    this._Kernel2Xv5(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref e1, ref e2, ref e3);
    this._Kernel2Xv5(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e0, ref e3, ref e1);
    this._Kernel2Xv5(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e2, ref e1, ref e0);
    this._Kernel2Xv5(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref e3, ref e0, ref e2);

    // Write output
    destTopLeft[0] = encoder.Encode(e0);
    destTopLeft[1] = encoder.Encode(e1);
    destTopLeft[destStride] = encoder.Encode(e2);
    destTopLeft[destStride + 1] = encoder.Encode(e3);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Kernel2Xv5(
    in NeighborPixel<TWork, TKey> pe,
    in NeighborPixel<TWork, TKey> pi,
    in NeighborPixel<TWork, TKey> ph,
    in NeighborPixel<TWork, TKey> pf,
    in NeighborPixel<TWork, TKey> pg,
    in NeighborPixel<TWork, TKey> pc,
    in NeighborPixel<TWork, TKey> pd,
    in NeighborPixel<TWork, TKey> pb,
    in NeighborPixel<TWork, TKey> f4,
    in NeighborPixel<TWork, TKey> i4,
    in NeighborPixel<TWork, TKey> h5,
    in NeighborPixel<TWork, TKey> i5,
    ref TWork n1,
    ref TWork n2,
    ref TWork n3
  ) {
    // Calculate edge weights
    var e = metric.Distance(pe.Key, pc.Key) + metric.Distance(pe.Key, pg.Key)
          + metric.Distance(pi.Key, h5.Key) + metric.Distance(pi.Key, f4.Key)
          + (metric.Distance(ph.Key, pf.Key) * 4);
    var i = metric.Distance(ph.Key, pd.Key) + metric.Distance(ph.Key, i5.Key)
          + metric.Distance(pf.Key, i4.Key) + metric.Distance(pf.Key, pb.Key)
          + (metric.Distance(pe.Key, pi.Key) * 4);

    // Determine interpolation direction
    var px = metric.Distance(pe.Key, pf.Key) <= metric.Distance(pe.Key, ph.Key) ? pf.Work : ph.Work;

    if (e < i && (!equality.Equals(pf.Key, pb.Key) && !equality.Equals(ph.Key, pd.Key)
                  || equality.Equals(pe.Key, pi.Key) && !equality.Equals(pf.Key, i4.Key) && !equality.Equals(ph.Key, i5.Key)
                  || equality.Equals(pe.Key, pg.Key)
                  || equality.Equals(pe.Key, pc.Key))) {
      var ke = metric.Distance(pf.Key, pg.Key);
      var ki = metric.Distance(ph.Key, pc.Key);
      var ex2 = !equality.Equals(pe.Key, pc.Key) && !equality.Equals(pb.Key, pc.Key);
      var ex3 = !equality.Equals(pe.Key, pg.Key) && !equality.Equals(pd.Key, pg.Key);

      if (ke * 2 <= ki && ex3 || ke >= ki * 2 && ex2) {
        if (ke * 2 <= ki && ex3)
          this._Left2_2X(ref n3, ref n2, px);
        if (ke >= ki * 2 && ex2)
          this._Up2_2X(ref n3, ref n1, px);
      } else
        this._Dia_2X(ref n3, px);
    } else if (e <= i)
      this._AlphaBlend64W(ref n3, px);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend32W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src, 7, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend64W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src, 3, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend128W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend192W(ref TWork dst, in TWork src) {
    dst = allowBlending ? lerp.Lerp(dst, src, 1, 3) : src;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend224W(ref TWork dst, in TWork src) {
    dst = allowBlending ? lerp.Lerp(dst, src, 1, 7) : src;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Left2_2X(ref TWork n3, ref TWork n2, in TWork pixel) {
    this._AlphaBlend192W(ref n3, pixel);
    this._AlphaBlend64W(ref n2, pixel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2_2X(ref TWork n3, ref TWork n1, in TWork pixel) {
    this._AlphaBlend192W(ref n3, pixel);
    this._AlphaBlend64W(ref n1, pixel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia_2X(ref TWork n3, in TWork pixel) => this._AlphaBlend128W(ref n3, pixel);
}

#endregion

#region XBR 3x Kernel

/// <summary>
/// Internal kernel for XBR 3x algorithm.
/// </summary>
/// <remarks>
/// Output 3x3 block:
/// e0 e1 e2
/// e3 e4 e5
/// e6 e7 e8
/// </remarks>
file readonly struct Xbr3xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode>(
  bool allowBlending = true,
  bool useOriginalImplementation = false,
  TEquality equality = default,
  TMetric metric = default,
  TLerp lerp = default
) : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int ScaleX => 3;

  /// <inheritdoc />
  public int ScaleY => 3;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Map NeighborWindow to XBR naming convention (5x5 kernel)
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    // Row -2: a1, b1, c1 (cols -1, 0, +1)
    var a1 = window.M2M1; // (row -2, col -1)
    var b1 = window.M2P0; // (row -2, col 0)
    var c1 = window.M2P1; // (row -2, col +1)

    // Row -1: a0, pa, pb, pc, c4 (cols -2, -1, 0, +1, +2)
    var a0 = window.M1M2; // (row -1, col -2)
    var pa = window.M1M1; // (row -1, col -1)
    var pb = window.M1P0; // (row -1, col 0) = top
    var pc = window.M1P1; // (row -1, col +1)
    var c4 = window.M1P2; // (row -1, col +2)

    // Row 0 (center): d0, pd, pe, pf, f4 (cols -2, -1, 0, +1, +2)
    var d0 = window.P0M2; // (row 0, col -2)
    var pd = window.P0M1; // (row 0, col -1) = left
    var pe = window.P0P0; // (row 0, col 0) = center
    var pf = window.P0P1; // (row 0, col +1) = right
    var f4 = window.P0P2; // (row 0, col +2)

    // Row +1: g0, pg, ph, pi, i4 (cols -2, -1, 0, +1, +2)
    var g0 = window.P1M2; // (row +1, col -2)
    var pg = window.P1M1; // (row +1, col -1)
    var ph = window.P1P0; // (row +1, col 0) = bottom
    var pi = window.P1P1; // (row +1, col +1)
    var i4 = window.P1P2; // (row +1, col +2)

    // Row +2: g5, h5, i5 (cols -1, 0, +1)
    var g5 = window.P2M1; // (row +2, col -1)
    var h5 = window.P2P0; // (row +2, col 0)
    var i5 = window.P2P1; // (row +2, col +1)

    // Initialize output to center pixel
    var e0 = pe.Work;
    var e1 = pe.Work;
    var e2 = pe.Work;
    var e3 = pe.Work;
    var e4 = pe.Work;
    var e5 = pe.Work;
    var e6 = pe.Work;
    var e7 = pe.Work;
    var e8 = pe.Work;

    // Apply kernels for each corner (rotated 4 times)
    this._Kernel3X(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref e2, ref e5, ref e6, ref e7, ref e8);
    this._Kernel3X(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e0, ref e1, ref e8, ref e5, ref e2);
    this._Kernel3X(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e6, ref e3, ref e2, ref e1, ref e0);
    this._Kernel3X(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref e8, ref e7, ref e0, ref e3, ref e6);

    // Write output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = destTopLeft + destStride * 2;

    row0[0] = encoder.Encode(e0);
    row0[1] = encoder.Encode(e1);
    row0[2] = encoder.Encode(e2);
    row1[0] = encoder.Encode(e3);
    row1[1] = encoder.Encode(e4);
    row1[2] = encoder.Encode(e5);
    row2[0] = encoder.Encode(e6);
    row2[1] = encoder.Encode(e7);
    row2[2] = encoder.Encode(e8);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Kernel3X(
    in NeighborPixel<TWork, TKey> pe,
    in NeighborPixel<TWork, TKey> pi,
    in NeighborPixel<TWork, TKey> ph,
    in NeighborPixel<TWork, TKey> pf,
    in NeighborPixel<TWork, TKey> pg,
    in NeighborPixel<TWork, TKey> pc,
    in NeighborPixel<TWork, TKey> pd,
    in NeighborPixel<TWork, TKey> pb,
    in NeighborPixel<TWork, TKey> f4,
    in NeighborPixel<TWork, TKey> i4,
    in NeighborPixel<TWork, TKey> h5,
    in NeighborPixel<TWork, TKey> i5,
    ref TWork n2,
    ref TWork n5,
    ref TWork n6,
    ref TWork n7,
    ref TWork n8
  ) {
    // Calculate edge weights
    var e = metric.Distance(pe.Key, pc.Key) + metric.Distance(pe.Key, pg.Key)
          + metric.Distance(pi.Key, h5.Key) + metric.Distance(pi.Key, f4.Key)
          + (metric.Distance(ph.Key, pf.Key) * 4);
    var i = metric.Distance(ph.Key, pd.Key) + metric.Distance(ph.Key, i5.Key)
          + metric.Distance(pf.Key, i4.Key) + metric.Distance(pf.Key, pb.Key)
          + (metric.Distance(pe.Key, pi.Key) * 4);

    bool state;
    if (useOriginalImplementation)
      state = e < i && (!equality.Equals(pf.Key, pb.Key) && !equality.Equals(ph.Key, pd.Key)
                        || equality.Equals(pe.Key, pi.Key) && !equality.Equals(pf.Key, i4.Key) && !equality.Equals(ph.Key, i5.Key)
                        || equality.Equals(pe.Key, pg.Key)
                        || equality.Equals(pe.Key, pc.Key));
    else
      state = e < i && (!equality.Equals(pf.Key, pb.Key) && !equality.Equals(pf.Key, pc.Key)
                        || !equality.Equals(ph.Key, pd.Key) && !equality.Equals(ph.Key, pg.Key)
                        || equality.Equals(pe.Key, pi.Key) && (!equality.Equals(pf.Key, f4.Key) && !equality.Equals(pf.Key, i4.Key)
                                                               || !equality.Equals(ph.Key, h5.Key) && !equality.Equals(ph.Key, i5.Key))
                        || equality.Equals(pe.Key, pg.Key)
                        || equality.Equals(pe.Key, pc.Key));

    if (state) {
      var ke = metric.Distance(pf.Key, pg.Key);
      var ki = metric.Distance(ph.Key, pc.Key);
      var ex2 = !equality.Equals(pe.Key, pc.Key) && !equality.Equals(pb.Key, pc.Key);
      var ex3 = !equality.Equals(pe.Key, pg.Key) && !equality.Equals(pd.Key, pg.Key);
      var px = metric.Distance(pe.Key, pf.Key) <= metric.Distance(pe.Key, ph.Key) ? pf.Work : ph.Work;

      var condLeft = ke * 2 <= ki && ex3;
      var condUp = ke >= ki * 2 && ex2;

      if (condLeft && condUp)
        this._LeftUp2_3X(ref n7, out n5, ref n6, ref n2, out n8, px);
      else if (condLeft)
        this._Left2_3X(ref n7, ref n5, ref n6, out n8, px);
      else if (condUp)
        this._Up2_3X(ref n5, ref n7, ref n2, out n8, px);
      else
        this._Dia_3X(ref n8, ref n5, ref n7, px);
    } else if (e <= i)
      this._AlphaBlend128W(ref n8, metric.Distance(pe.Key, pf.Key) <= metric.Distance(pe.Key, ph.Key) ? pf.Work : ph.Work);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend32W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src, 7, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend64W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src, 3, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend128W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend192W(ref TWork dst, in TWork src) {
    dst = allowBlending ? lerp.Lerp(dst, src, 1, 3) : src;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend224W(ref TWork dst, in TWork src) {
    dst = allowBlending ? lerp.Lerp(dst, src, 1, 7) : src;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LeftUp2_3X(ref TWork n7, out TWork n5, ref TWork n6, ref TWork n2, out TWork n8, in TWork pixel) {
    this._AlphaBlend192W(ref n7, pixel);
    this._AlphaBlend64W(ref n6, pixel);
    n5 = n7;
    n2 = n6;
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Left2_3X(ref TWork n7, ref TWork n5, ref TWork n6, out TWork n8, in TWork pixel) {
    this._AlphaBlend192W(ref n7, pixel);
    this._AlphaBlend64W(ref n5, pixel);
    this._AlphaBlend64W(ref n6, pixel);
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2_3X(ref TWork n5, ref TWork n7, ref TWork n2, out TWork n8, in TWork pixel) {
    this._AlphaBlend192W(ref n5, pixel);
    this._AlphaBlend64W(ref n7, pixel);
    this._AlphaBlend64W(ref n2, pixel);
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia_3X(ref TWork n8, ref TWork n5, ref TWork n7, in TWork pixel) {
    this._AlphaBlend224W(ref n8, pixel);
    this._AlphaBlend32W(ref n5, pixel);
    this._AlphaBlend32W(ref n7, pixel);
  }
}

#endregion

#region XBR 4x Kernel

/// <summary>
/// Internal kernel for XBR 4x algorithm.
/// </summary>
/// <remarks>
/// Output 4x4 block:
/// e0  e1  e2  e3
/// e4  e5  e6  e7
/// e8  e9  ea  eb
/// ec  ed  ee  ef
/// </remarks>
file readonly struct Xbr4xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode>(
  bool allowBlending = true,
  TEquality equality = default,
  TMetric metric = default,
  TLerp lerp = default
) : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int ScaleX => 4;

  /// <inheritdoc />
  public int ScaleY => 4;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Map NeighborWindow to XBR naming convention (5x5 kernel)
    // NeighborWindow uses [Row][Column] naming: M1P0 = row -1, col 0 = TOP
    // Row -2: a1, b1, c1 (cols -1, 0, +1)
    var a1 = window.M2M1; // (row -2, col -1)
    var b1 = window.M2P0; // (row -2, col 0)
    var c1 = window.M2P1; // (row -2, col +1)

    // Row -1: a0, pa, pb, pc, c4 (cols -2, -1, 0, +1, +2)
    var a0 = window.M1M2; // (row -1, col -2)
    var pa = window.M1M1; // (row -1, col -1)
    var pb = window.M1P0; // (row -1, col 0) = top
    var pc = window.M1P1; // (row -1, col +1)
    var c4 = window.M1P2; // (row -1, col +2)

    // Row 0 (center): d0, pd, pe, pf, f4 (cols -2, -1, 0, +1, +2)
    var d0 = window.P0M2; // (row 0, col -2)
    var pd = window.P0M1; // (row 0, col -1) = left
    var pe = window.P0P0; // (row 0, col 0) = center
    var pf = window.P0P1; // (row 0, col +1) = right
    var f4 = window.P0P2; // (row 0, col +2)

    // Row +1: g0, pg, ph, pi, i4 (cols -2, -1, 0, +1, +2)
    var g0 = window.P1M2; // (row +1, col -2)
    var pg = window.P1M1; // (row +1, col -1)
    var ph = window.P1P0; // (row +1, col 0) = bottom
    var pi = window.P1P1; // (row +1, col +1)
    var i4 = window.P1P2; // (row +1, col +2)

    // Row +2: g5, h5, i5 (cols -1, 0, +1)
    var g5 = window.P2M1; // (row +2, col -1)
    var h5 = window.P2P0; // (row +2, col 0)
    var i5 = window.P2P1; // (row +2, col +1)

    // Initialize output to center pixel
    var e0 = pe.Work;
    var e1 = pe.Work;
    var e2 = pe.Work;
    var e3 = pe.Work;
    var e4 = pe.Work;
    var e5 = pe.Work;
    var e6 = pe.Work;
    var e7 = pe.Work;
    var e8 = pe.Work;
    var e9 = pe.Work;
    var ea = pe.Work;
    var eb = pe.Work;
    var ec = pe.Work;
    var ed = pe.Work;
    var ee = pe.Work;
    var ef = pe.Work;

    // Apply kernels for each corner (rotated 4 times)
    this._Kernel4Xv2(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref ef, ref ee, ref eb, ref e3, ref e7, ref ea, ref ed, ref ec);
    this._Kernel4Xv2(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e3, ref e7, ref e2, ref e0, ref e1, ref e6, ref eb, ref ef);
    this._Kernel4Xv2(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e0, ref e1, ref e4, ref ec, ref e8, ref e5, ref e2, ref e3);
    this._Kernel4Xv2(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref ec, ref e8, ref ed, ref ef, ref ee, ref e9, ref e4, ref e0);

    // Write output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = destTopLeft + destStride * 2;
    var row3 = destTopLeft + destStride * 3;

    row0[0] = encoder.Encode(e0);
    row0[1] = encoder.Encode(e1);
    row0[2] = encoder.Encode(e2);
    row0[3] = encoder.Encode(e3);
    row1[0] = encoder.Encode(e4);
    row1[1] = encoder.Encode(e5);
    row1[2] = encoder.Encode(e6);
    row1[3] = encoder.Encode(e7);
    row2[0] = encoder.Encode(e8);
    row2[1] = encoder.Encode(e9);
    row2[2] = encoder.Encode(ea);
    row2[3] = encoder.Encode(eb);
    row3[0] = encoder.Encode(ec);
    row3[1] = encoder.Encode(ed);
    row3[2] = encoder.Encode(ee);
    row3[3] = encoder.Encode(ef);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Kernel4Xv2(
    in NeighborPixel<TWork, TKey> pe,
    in NeighborPixel<TWork, TKey> pi,
    in NeighborPixel<TWork, TKey> ph,
    in NeighborPixel<TWork, TKey> pf,
    in NeighborPixel<TWork, TKey> pg,
    in NeighborPixel<TWork, TKey> pc,
    in NeighborPixel<TWork, TKey> pd,
    in NeighborPixel<TWork, TKey> pb,
    in NeighborPixel<TWork, TKey> f4,
    in NeighborPixel<TWork, TKey> i4,
    in NeighborPixel<TWork, TKey> h5,
    in NeighborPixel<TWork, TKey> i5,
    ref TWork n15,
    ref TWork n14,
    ref TWork n11,
    ref TWork n3,
    ref TWork n7,
    ref TWork n10,
    ref TWork n13,
    ref TWork n12
  ) {
    // Calculate edge weights
    var e = metric.Distance(pe.Key, pc.Key) + metric.Distance(pe.Key, pg.Key)
          + metric.Distance(pi.Key, h5.Key) + metric.Distance(pi.Key, f4.Key)
          + (metric.Distance(ph.Key, pf.Key) * 4);
    var i = metric.Distance(ph.Key, pd.Key) + metric.Distance(ph.Key, i5.Key)
          + metric.Distance(pf.Key, i4.Key) + metric.Distance(pf.Key, pb.Key)
          + (metric.Distance(pe.Key, pi.Key) * 4);
    var px = metric.Distance(pe.Key, pf.Key) <= metric.Distance(pe.Key, ph.Key) ? pf.Work : ph.Work;

    if (e < i && (!equality.Equals(pf.Key, pb.Key) && !equality.Equals(ph.Key, pd.Key)
                  || equality.Equals(pe.Key, pi.Key) && !equality.Equals(pf.Key, i4.Key) && !equality.Equals(ph.Key, i5.Key)
                  || equality.Equals(pe.Key, pg.Key)
                  || equality.Equals(pe.Key, pc.Key))) {
      var ke = metric.Distance(pf.Key, pg.Key);
      var ki = metric.Distance(ph.Key, pc.Key);
      var ex2 = !equality.Equals(pe.Key, pc.Key) && !equality.Equals(pb.Key, pc.Key);
      var ex3 = !equality.Equals(pe.Key, pg.Key) && !equality.Equals(pd.Key, pg.Key);

      if (ke * 2 <= ki && ex3 || ke >= ki * 2 && ex2) {
        if (ke * 2 <= ki && ex3)
          this._Left2(out n15, out n14, ref n11, ref n13, ref n12, ref n10, px);
        if (ke >= ki * 2 && ex2)
          this._Up2(out n15, ref n14, out n11, ref n3, ref n7, ref n10, px);
      } else
        this._Dia(out n15, ref n14, ref n11, px);
    } else if (e <= i)
      this._AlphaBlend128W(ref n15, px);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend64W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src, 3, 1);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend128W(ref TWork dst, in TWork src) {
    if (allowBlending)
      dst = lerp.Lerp(dst, src);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _AlphaBlend192W(ref TWork dst, in TWork src) {
    dst = allowBlending ? lerp.Lerp(dst, src, 1, 3) : src;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _LeftUp2(out TWork n15, out TWork n14, out TWork n11, ref TWork n13, ref TWork n12, out TWork n10, out TWork n7, out TWork n3, in TWork pixel) {
    this._AlphaBlend192W(ref n13, pixel);
    this._AlphaBlend64W(ref n12, pixel);
    n15 = pixel;
    n14 = pixel;
    n11 = pixel;
    n10 = n12;
    n3 = n12;
    n7 = n13;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Left2(out TWork n15, out TWork n14, ref TWork n11, ref TWork n13, ref TWork n12, ref TWork n10, in TWork pixel) {
    this._AlphaBlend192W(ref n11, pixel);
    this._AlphaBlend192W(ref n13, pixel);
    this._AlphaBlend64W(ref n10, pixel);
    this._AlphaBlend64W(ref n12, pixel);
    n14 = pixel;
    n15 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2(out TWork n15, ref TWork n14, out TWork n11, ref TWork n3, ref TWork n7, ref TWork n10, in TWork pixel) {
    this._AlphaBlend192W(ref n14, pixel);
    this._AlphaBlend192W(ref n7, pixel);
    this._AlphaBlend64W(ref n10, pixel);
    this._AlphaBlend64W(ref n3, pixel);
    n11 = pixel;
    n15 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia(out TWork n15, ref TWork n14, ref TWork n11, in TWork pixel) {
    this._AlphaBlend128W(ref n11, pixel);
    this._AlphaBlend128W(ref n14, pixel);
    n15 = pixel;
  }
}

#endregion
