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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

#region Xbr (Unified)

/// <summary>
/// XBR pixel-art scaling algorithm by Hyllian (2x, 3x, 4x, 5x).
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
  /// <param name="scale">The scale factor (2, 3, 4, or 5).</param>
  /// <param name="allowAlphaBlending">Whether to allow alpha blending for smoother results (not used by 5x).</param>
  /// <param name="useOriginalImplementation">Whether to use the original implementation variant (only used by 3x).</param>
  public Xbr(int scale = 2, bool allowAlphaBlending = true, bool useOriginalImplementation = false) {
    if (scale is not (2 or 3 or 4 or 5))
      throw new System.ArgumentOutOfRangeException(nameof(scale), scale, "XBR supports 2x, 3x, 4x, 5x scaling");
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
  public ScaleFactor Scale => new(this._scale, this._scale);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => this._scale switch {
      2 => callback.Invoke(new Xbr2xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, equality, default, lerp)),
      3 => callback.Invoke(new Xbr3xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, this._useOriginalImplementation, equality, default, lerp)),
      4 => callback.Invoke(new Xbr4xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
        this._allowAlphaBlending, equality, default, lerp)),
      5 => callback.Invoke(new Xbr5xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp)),
      _ => throw new System.InvalidOperationException($"Invalid scale factor: {this._scale}")
    };

  /// <summary>
  /// Gets the list of scale factors supported by XBR.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(5, 5)];

  /// <summary>
  /// Determines whether XBR supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 } or { X: 5, Y: 5 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 5, sourceHeight * 5);
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

  /// <summary>Gets an XBR 5x scaler.</summary>
  public static Xbr Scale5x => new(5);

  /// <summary>Gets the default XBR configuration (2x).</summary>
  public static Xbr Default => new(2);
}

#endregion

#region Xbr2x

/// <summary>
/// XBR 2x pixel-art scaling algorithm by Hyllian.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using edge-directed interpolation with 5x5 neighborhood analysis.</para>
/// <para>XBR uses both pattern matching (equality) and weighted distance metrics for optimal edge detection.</para>
/// <para>Developed by Hyllian in 2011 as an improvement over Scale2x with better edge handling.</para>
/// <para>Reference: http://board.byuu.org/viewtopic.php?f=10&amp;t=2248</para>
/// </remarks>
[ScalerInfo("XBR 2x", Author = "Hyllian", Year = 2011,
  Description = "Edge-directed 2x upscaler with 5x5 neighborhood", Category = ScalerCategory.PixelArt,
  Url = "http://board.byuu.org/viewtopic.php?f=10&t=2248")]
public readonly struct Xbr2x : IPixelScaler {

  private readonly bool _allowAlphaBlending;

  /// <summary>
  /// Creates an XBR 2x scaler with specified blending option.
  /// </summary>
  /// <param name="allowAlphaBlending">Whether to allow alpha blending for smoother results.</param>
  public Xbr2x(bool allowAlphaBlending = true) => this._allowAlphaBlending = allowAlphaBlending;

  /// <inheritdoc />
  public ScaleFactor Scale => new(2, 2);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Xbr2xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, equality, default, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by XBR 2x.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether XBR 2x supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR 2x.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default XBR 2x configuration.
  /// </summary>
  public static Xbr2x Default => new();

  /// <inheritdoc />
  public IScaler<TWork, TKey, TPixel, TEncode> GetKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => new Xbr2xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, equality, default, lerp);
}

#endregion

#region Xbr3x

/// <summary>
/// XBR 3x pixel-art scaling algorithm by Hyllian.
/// </summary>
/// <remarks>
/// <para>Scales images by 3x using edge-directed interpolation with 5x5 neighborhood analysis.</para>
/// <para>XBR uses both pattern matching (equality) and weighted distance metrics for optimal edge detection.</para>
/// <para>Developed by Hyllian in 2011.</para>
/// </remarks>
[ScalerInfo("XBR 3x", Author = "Hyllian", Year = 2011,
  Description = "Edge-directed 3x upscaler with 5x5 neighborhood", Category = ScalerCategory.PixelArt,
  Url = "http://board.byuu.org/viewtopic.php?f=10&t=2248")]
public readonly struct Xbr3x : IPixelScaler {

  private readonly bool _allowAlphaBlending;
  private readonly bool _useOriginalImplementation;

  /// <summary>
  /// Creates an XBR 3x scaler with specified options.
  /// </summary>
  /// <param name="allowAlphaBlending">Whether to allow alpha blending for smoother results.</param>
  /// <param name="useOriginalImplementation">Whether to use the original implementation variant.</param>
  public Xbr3x(bool allowAlphaBlending = true, bool useOriginalImplementation = false) {
    this._allowAlphaBlending = allowAlphaBlending;
    this._useOriginalImplementation = useOriginalImplementation;
  }

  /// <inheritdoc />
  public ScaleFactor Scale => new(3, 3);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Xbr3xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, this._useOriginalImplementation, equality, default, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by XBR 3x.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(3, 3)];

  /// <summary>
  /// Determines whether XBR 3x supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 3, Y: 3 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR 3x.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 3, sourceHeight * 3);
  }

  /// <summary>
  /// Gets the default XBR 3x configuration.
  /// </summary>
  public static Xbr3x Default => new();

  /// <inheritdoc />
  public IScaler<TWork, TKey, TPixel, TEncode> GetKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => new Xbr3xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, this._useOriginalImplementation, equality, default, lerp);
}

#endregion

#region Xbr4x

/// <summary>
/// XBR 4x pixel-art scaling algorithm by Hyllian.
/// </summary>
/// <remarks>
/// <para>Scales images by 4x using edge-directed interpolation with 5x5 neighborhood analysis.</para>
/// <para>XBR uses both pattern matching (equality) and weighted distance metrics for optimal edge detection.</para>
/// <para>Developed by Hyllian in 2011.</para>
/// </remarks>
[ScalerInfo("XBR 4x", Author = "Hyllian", Year = 2011,
  Description = "Edge-directed 4x upscaler with 5x5 neighborhood", Category = ScalerCategory.PixelArt,
  Url = "http://board.byuu.org/viewtopic.php?f=10&t=2248")]
public readonly struct Xbr4x : IPixelScaler {

  private readonly bool _allowAlphaBlending;

  /// <summary>
  /// Creates an XBR 4x scaler with specified blending option.
  /// </summary>
  /// <param name="allowAlphaBlending">Whether to allow alpha blending for smoother results.</param>
  public Xbr4x(bool allowAlphaBlending = true) => this._allowAlphaBlending = allowAlphaBlending;

  /// <inheritdoc />
  public ScaleFactor Scale => new(4, 4);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Xbr4xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, equality, default, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by XBR 4x.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(4, 4)];

  /// <summary>
  /// Determines whether XBR 4x supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 4, Y: 4 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR 4x.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 4, sourceHeight * 4);
  }

  /// <summary>
  /// Gets the default XBR 4x configuration.
  /// </summary>
  public static Xbr4x Default => new();

  /// <inheritdoc />
  public IScaler<TWork, TKey, TPixel, TEncode> GetKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => new Xbr4xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode>(
      this._allowAlphaBlending, equality, default, lerp);
}

#endregion

#region Xbr5x

/// <summary>
/// XBR 5x pixel-art scaling algorithm by Hyllian/Jararaca.
/// </summary>
/// <remarks>
/// <para>Scales images by 5x using a simplified XBR variant.</para>
/// <para>This is a simpler variant that applies pattern detection per quadrant.</para>
/// <para>Reference: https://github.com/libretro/common-shaders/blob/master/xbr/shaders/legacy/5xbr.cg</para>
/// </remarks>
[ScalerInfo("XBR 5x", Author = "Hyllian", Year = 2011,
  Description = "Simplified 5x upscaler variant", Category = ScalerCategory.PixelArt,
  Url = "https://github.com/libretro/common-shaders/blob/master/xbr/shaders/legacy/5xbr.cg")]
public readonly struct Xbr5x : IPixelScaler {

  /// <inheritdoc />
  public ScaleFactor Scale => new(5, 5);

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new Xbr5xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp));

  /// <summary>
  /// Gets the list of scale factors supported by XBR 5x.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(5, 5)];

  /// <summary>
  /// Determines whether XBR 5x supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 5, Y: 5 };

  /// <summary>
  /// Enumerates all possible target dimensions for XBR 5x.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 5, sourceHeight * 5);
  }

  /// <summary>
  /// Gets the default XBR 5x configuration.
  /// </summary>
  public static Xbr5x Default => new();

  /// <inheritdoc />
  public IScaler<TWork, TKey, TPixel, TEncode> GetKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode>(
    TEquality equality = default,
    TLerp lerp = default)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    => new Xbr5xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(equality, lerp);
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
    // Row -2: a1, b1, c1
    var a1 = window.M1M2;
    var b1 = window.P0M2;
    var c1 = window.P1M2;

    // Row -1: a0, pa, pb, pc, c4
    var a0 = window.M2M1;
    var pa = window.M1M1;
    var pb = window.P0M1;
    var pc = window.P1M1;
    var c4 = window.P2M1;

    // Row 0 (center): d0, pd, pe, pf, f4
    var d0 = window.M2P0;
    var pd = window.M1P0;
    var pe = window.P0P0;
    var pf = window.P1P0;
    var f4 = window.P2P0;

    // Row +1: g0, pg, ph, pi, i4
    var g0 = window.M2P1;
    var pg = window.M1P1;
    var ph = window.P0P1;
    var pi = window.P1P1;
    var i4 = window.P2P1;

    // Row +2: g5, h5, i5
    var g5 = window.M1P2;
    var h5 = window.P0P2;
    var i5 = window.P1P2;

    // Initialize output to center pixel
    var e0 = pe.Work;
    var e1 = pe.Work;
    var e2 = pe.Work;
    var e3 = pe.Work;

    // Apply kernels for each corner (rotated 4 times)
    _Kernel2Xv5(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref e1, ref e2, ref e3);
    _Kernel2Xv5(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e0, ref e3, ref e1);
    _Kernel2Xv5(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e2, ref e1, ref e0);
    _Kernel2Xv5(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref e3, ref e0, ref e2);

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
    // Check if edge exists (center differs from both neighbors)
    var ex = !equality.Equals(pe.Key, ph.Key) && !equality.Equals(pe.Key, pf.Key);
    if (!ex)
      return;

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
          _Left2_2X(ref n3, ref n2, px);
        if (ke >= ki * 2 && ex2)
          _Up2_2X(ref n3, ref n1, px);
      } else
        _Dia_2X(ref n3, px);
    } else if (e <= i)
      _AlphaBlend64W(ref n3, px);
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
    _AlphaBlend192W(ref n3, pixel);
    _AlphaBlend64W(ref n2, pixel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2_2X(ref TWork n3, ref TWork n1, in TWork pixel) {
    _AlphaBlend192W(ref n3, pixel);
    _AlphaBlend64W(ref n1, pixel);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia_2X(ref TWork n3, in TWork pixel) => _AlphaBlend128W(ref n3, pixel);
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
    var a1 = window.M1M2;
    var b1 = window.P0M2;
    var c1 = window.P1M2;

    var a0 = window.M2M1;
    var pa = window.M1M1;
    var pb = window.P0M1;
    var pc = window.P1M1;
    var c4 = window.P2M1;

    var d0 = window.M2P0;
    var pd = window.M1P0;
    var pe = window.P0P0;
    var pf = window.P1P0;
    var f4 = window.P2P0;

    var g0 = window.M2P1;
    var pg = window.M1P1;
    var ph = window.P0P1;
    var pi = window.P1P1;
    var i4 = window.P2P1;

    var g5 = window.M1P2;
    var h5 = window.P0P2;
    var i5 = window.P1P2;

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
    _Kernel3X(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref e2, ref e5, ref e6, ref e7, ref e8);
    _Kernel3X(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e0, ref e1, ref e8, ref e5, ref e2);
    _Kernel3X(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e6, ref e3, ref e2, ref e1, ref e0);
    _Kernel3X(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref e8, ref e7, ref e0, ref e3, ref e6);

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
    var ex = !equality.Equals(pe.Key, ph.Key) && !equality.Equals(pe.Key, pf.Key);
    if (!ex)
      return;

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

      if (ke * 2 <= ki && ex3 && ke >= ki * 2 && ex2)
        _LeftUp2_3X(ref n7, out n5, ref n6, ref n2, out n8, px);
      else if (ke * 2 <= ki && ex3)
        _Left2_3X(ref n7, ref n5, ref n6, out n8, px);
      else if (ke >= ki * 2 && ex2)
        _Up2_3X(ref n5, ref n7, ref n2, out n8, px);
      else
        _Dia_3X(ref n8, ref n5, ref n7, px);
    } else if (e <= i)
      _AlphaBlend128W(ref n8, metric.Distance(pe.Key, pf.Key) <= metric.Distance(pe.Key, ph.Key) ? pf.Work : ph.Work);
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
    _AlphaBlend192W(ref n7, pixel);
    _AlphaBlend64W(ref n6, pixel);
    n5 = n7;
    n2 = n6;
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Left2_3X(ref TWork n7, ref TWork n5, ref TWork n6, out TWork n8, in TWork pixel) {
    _AlphaBlend192W(ref n7, pixel);
    _AlphaBlend64W(ref n5, pixel);
    _AlphaBlend64W(ref n6, pixel);
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2_3X(ref TWork n5, ref TWork n7, ref TWork n2, out TWork n8, in TWork pixel) {
    _AlphaBlend192W(ref n5, pixel);
    _AlphaBlend64W(ref n7, pixel);
    _AlphaBlend64W(ref n2, pixel);
    n8 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia_3X(ref TWork n8, ref TWork n5, ref TWork n7, in TWork pixel) {
    _AlphaBlend224W(ref n8, pixel);
    _AlphaBlend32W(ref n5, pixel);
    _AlphaBlend32W(ref n7, pixel);
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
    var a1 = window.M1M2;
    var b1 = window.P0M2;
    var c1 = window.P1M2;

    var a0 = window.M2M1;
    var pa = window.M1M1;
    var pb = window.P0M1;
    var pc = window.P1M1;
    var c4 = window.P2M1;

    var d0 = window.M2P0;
    var pd = window.M1P0;
    var pe = window.P0P0;
    var pf = window.P1P0;
    var f4 = window.P2P0;

    var g0 = window.M2P1;
    var pg = window.M1P1;
    var ph = window.P0P1;
    var pi = window.P1P1;
    var i4 = window.P2P1;

    var g5 = window.M1P2;
    var h5 = window.P0P2;
    var i5 = window.P1P2;

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
    _Kernel4Xv2(pe, pi, ph, pf, pg, pc, pd, pb, f4, i4, h5, i5, ref ef, ref ee, ref eb, ref e3, ref e7, ref ea, ref ed, ref ec);
    _Kernel4Xv2(pe, pc, pf, pb, pi, pa, ph, pd, b1, c1, f4, c4, ref e3, ref e7, ref e2, ref e0, ref e1, ref e6, ref eb, ref ef);
    _Kernel4Xv2(pe, pa, pb, pd, pc, pg, pf, ph, d0, a0, b1, a1, ref e0, ref e1, ref e4, ref ec, ref e8, ref e5, ref e2, ref e3);
    _Kernel4Xv2(pe, pg, pd, ph, pa, pi, pb, pf, h5, g5, d0, g0, ref ec, ref e8, ref ed, ref ef, ref ee, ref e9, ref e4, ref e0);

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
    var ex = !equality.Equals(pe.Key, ph.Key) && !equality.Equals(pe.Key, pf.Key);
    if (!ex)
      return;

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
          _Left2(out n15, out n14, ref n11, ref n13, ref n12, ref n10, px);
        if (ke >= ki * 2 && ex2)
          _Up2(out n15, ref n14, out n11, ref n3, ref n7, ref n10, px);
      } else
        _Dia(out n15, ref n14, ref n11, px);
    } else if (e <= i)
      _AlphaBlend128W(ref n15, px);
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
    _AlphaBlend192W(ref n13, pixel);
    _AlphaBlend64W(ref n12, pixel);
    n15 = pixel;
    n14 = pixel;
    n11 = pixel;
    n10 = n12;
    n3 = n12;
    n7 = n13;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Left2(out TWork n15, out TWork n14, ref TWork n11, ref TWork n13, ref TWork n12, ref TWork n10, in TWork pixel) {
    _AlphaBlend192W(ref n11, pixel);
    _AlphaBlend192W(ref n13, pixel);
    _AlphaBlend64W(ref n10, pixel);
    _AlphaBlend64W(ref n12, pixel);
    n14 = pixel;
    n15 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Up2(out TWork n15, ref TWork n14, out TWork n11, ref TWork n3, ref TWork n7, ref TWork n10, in TWork pixel) {
    _AlphaBlend192W(ref n14, pixel);
    _AlphaBlend192W(ref n7, pixel);
    _AlphaBlend64W(ref n10, pixel);
    _AlphaBlend64W(ref n3, pixel);
    n11 = pixel;
    n15 = pixel;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void _Dia(out TWork n15, ref TWork n14, ref TWork n11, in TWork pixel) {
    _AlphaBlend128W(ref n11, pixel);
    _AlphaBlend128W(ref n14, pixel);
    n15 = pixel;
  }
}

#endregion

#region XBR 5x Kernel

/// <summary>
/// Internal kernel for XBR 5x algorithm (simplified variant).
/// </summary>
/// <remarks>
/// XBR 5x uses a simplified 3x3 neighborhood with pattern detection per quadrant.
///
/// Output 5x5 block is symmetric around the center.
/// </remarks>
file readonly struct Xbr5xKernel<TWork, TKey, TPixel, TEquality, TLerp, TEncode>(
  TEquality equality = default,
  TLerp lerp = default
) : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <inheritdoc />
  public int ScaleX => 5;

  /// <inheritdoc />
  public int ScaleY => 5;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // XBR 5x uses a simpler 3x3 neighborhood
    var a = window.M1M1; // top-left
    var b = window.P0M1; // top
    var c = window.P1M1; // top-right
    var d = window.M1P0; // left
    var e = window.P0P0; // center
    var f = window.P1P0; // right
    var g = window.M1P1; // bottom-left
    var h = window.P0P1; // bottom
    var i = window.P1P1; // bottom-right

    var e14 = e.Work;
    var e19 = e.Work;
    var e24 = e.Work;

    // Check pattern for diagonal edge
    if (equality.Equals(h.Key, f.Key) && !equality.Equals(h.Key, e.Key)
        && (equality.Equals(e.Key, g.Key) && (equality.Equals(h.Key, i.Key) || equality.Equals(e.Key, d.Key))
            || equality.Equals(e.Key, c.Key) && (equality.Equals(h.Key, i.Key) || equality.Equals(e.Key, b.Key)))) {
      e24 = f.Work;
      e19 = lerp.Lerp(e.Work, f.Work, 7, 1);
      e14 = lerp.Lerp(e.Work, f.Work, 1, 7);
    }

    // Write symmetric 5x5 output
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;
    var row2 = destTopLeft + destStride * 2;
    var row3 = destTopLeft + destStride * 3;
    var row4 = destTopLeft + destStride * 4;

    var enc24 = encoder.Encode(e24);
    var enc19 = encoder.Encode(e19);
    var enc14 = encoder.Encode(e14);
    var encE = encoder.Encode(e.Work);

    row0[0] = enc24;
    row0[1] = enc19;
    row0[2] = enc14;
    row0[3] = enc19;
    row0[4] = enc24;

    row1[0] = enc19;
    row1[1] = enc14;
    row1[2] = encE;
    row1[3] = enc14;
    row1[4] = enc19;

    row2[0] = enc14;
    row2[1] = encE;
    row2[2] = encE;
    row2[3] = encE;
    row2[4] = enc14;

    row3[0] = enc19;
    row3[1] = enc14;
    row3[2] = encE;
    row3[3] = enc14;
    row3[4] = enc19;

    row4[0] = enc24;
    row4[1] = enc19;
    row4[2] = enc14;
    row4[3] = enc19;
    row4[4] = enc24;
  }
}

#endregion
