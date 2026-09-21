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
using System.Collections.Generic;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Rescalers;

#region Xbrz (Unified)

/// <summary>
/// Which xBRZ release's output a scaler instance reproduces.
/// </summary>
/// <remarks>
/// Internal because the public surface exposes the two shipping variants as their own scaler types
/// (<see cref="Xbrz"/> and <see cref="Xbrz19"/>). The two single-change members exist only so tests
/// can attribute an output difference to one of 1.9's two changes rather than to both at once.
/// </remarks>
internal enum XbrzVariant {

  /// <summary>xBRZ as released up to and including 1.8.</summary>
  Classic = 0,

  /// <summary>xBRZ 1.9: a 2.4 steep threshold and blends rounded to nearest.</summary>
  Version19 = 1,

  /// <summary>Test-only: 1.9's steep threshold, pre-1.9 truncating blends.</summary>
  SteepThresholdOnly = 2,

  /// <summary>Test-only: 1.9's rounded blends, pre-1.9 steep threshold.</summary>
  RoundedBlendsOnly = 3
}

/// <summary>
/// xBRZ pixel-art scaling algorithm by Zenju (2x, 3x, 4x, 5x, 6x).
/// </summary>
/// <remarks>
/// <para>Extension of XBR algorithm with improved edge detection and blending.</para>
/// <para>Uses YCbCr color distance with 4-rotation symmetric pattern analysis.</para>
/// <para>Key parameters: luminanceWeight=1, equalColorTolerance=30, dominantDirectionThreshold=3.6, steepDirectionThreshold=2.2</para>
/// <para>
/// Reproduces the algorithm as released up to and including 1.8; see <see cref="Xbrz19"/> for
/// release 1.9, which raised the steep threshold to 2.4 and rounds its blends. The two are
/// separate scalers rather than a switch on this one so that existing scripts and saved settings
/// naming <c>xBRZ</c> keep getting exactly the pixels they got before.
/// </para>
/// <para>Reference: https://sourceforge.net/projects/xbrz</para>
/// </remarks>
[ScalerInfo("xBRZ", Author = "Zenju", Year = 2012,
  Description = "xBRZ with improved edge detection and blending", Category = ScalerCategory.Rescaler,
  Url = "https://sourceforge.net/projects/xbrz")]
public readonly struct Xbrz : IRescaler {

  private readonly int _scale;
  private readonly XbrzVariant _variant;

  /// <summary>
  /// Creates an xBRZ scaler with specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor (2, 3, 4, 5, or 6).</param>
  /// <remarks>
  /// Reproduces xBRZ as released up to and including 1.8. For release 1.9 use
  /// <see cref="Xbrz19"/>; this type's output is unchanged and stays that way.
  /// </remarks>
  public Xbrz(int scale = 2) : this(scale, XbrzVariant.Classic) { }

  /// <summary>
  /// Creates an xBRZ scaler reproducing a specific release's output.
  /// </summary>
  /// <param name="scale">The scale factor (2, 3, 4, 5, or 6).</param>
  /// <param name="variant">Which release's behaviour to reproduce.</param>
  internal Xbrz(int scale, XbrzVariant variant) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 6);
    this._scale = scale;
    this._variant = variant;
  }

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
    => this._variant switch {
      XbrzVariant.Version19 => XbrzDispatch.Invoke<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult, Xbrz19Variant>(this._scale, callback, equality, lerp),
      XbrzVariant.SteepThresholdOnly => XbrzDispatch.Invoke<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult, XbrzSteepOnlyVariant>(this._scale, callback, equality, lerp),
      XbrzVariant.RoundedBlendsOnly => XbrzDispatch.Invoke<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult, XbrzRoundOnlyVariant>(this._scale, callback, equality, lerp),
      _ => XbrzDispatch.Invoke<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult, XbrzClassicVariant>(this._scale, callback, equality, lerp)
    };

  /// <summary>
  /// Gets the list of scale factors supported by xBRZ.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(5, 5), new(6, 6)];

  /// <summary>
  /// Determines whether xBRZ supports the specified scale factor.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 } or { X: 5, Y: 5 } or { X: 6, Y: 6 };

  /// <summary>
  /// Enumerates all possible target dimensions for xBRZ.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 5, sourceHeight * 5);
    yield return (sourceWidth * 6, sourceHeight * 6);
  }

  /// <summary>Gets an xBRZ 2x scaler.</summary>
  public static Xbrz Scale2x => new(2);

  /// <summary>Gets an xBRZ 3x scaler.</summary>
  public static Xbrz Scale3x => new(3);

  /// <summary>Gets an xBRZ 4x scaler.</summary>
  public static Xbrz Scale4x => new(4);

  /// <summary>Gets an xBRZ 5x scaler.</summary>
  public static Xbrz Scale5x => new(5);

  /// <summary>Gets an xBRZ 6x scaler.</summary>
  public static Xbrz Scale6x => new(6);

  /// <summary>Gets the default xBRZ configuration (2x).</summary>
  public static Xbrz Default => Scale2x;
}

/// <summary>
/// Picks the per-scale kernel and closes it over a variant.
/// </summary>
/// <remarks>
/// A file-local type because the variant constraint names a file-local interface, which C# does not
/// allow in the signature of a non-file-local member.
/// </remarks>
file static class XbrzDispatch {

  public static TResult Invoke<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult, TVariant>(
    int scale,
    IKernelCallback<TWork, TKey, TPixel, TEncode, TResult> callback,
    TEquality equality,
    TLerp lerp)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDistance : struct, IColorMetric<TKey>, INormalizedMetric
    where TEquality : struct, IColorEquality<TKey>
    where TLerp : struct, ILerp<TWork>
    where TEncode : struct, IEncode<TWork, TPixel>
    where TVariant : struct, IXbrzVariant
    => scale switch {
      0 or 2 => callback.Invoke(new Xbrz2xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode, TVariant>(equality, default, lerp)),
      3 => callback.Invoke(new Xbrz3xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode, TVariant>(equality, default, lerp)),
      4 => callback.Invoke(new Xbrz4xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode, TVariant>(equality, default, lerp)),
      5 => callback.Invoke(new Xbrz5xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode, TVariant>(equality, default, lerp)),
      6 => callback.Invoke(new Xbrz6xKernel<TWork, TKey, TPixel, TEquality, TDistance, TLerp, TEncode, TVariant>(equality, default, lerp)),
      _ => throw new InvalidOperationException($"Invalid scale factor: {scale}")
    };
}

#endregion

#region Xbrz 1.9

/// <summary>
/// xBRZ as released in 1.9 (2x, 3x, 4x, 5x, 6x).
/// </summary>
/// <remarks>
/// <para>
/// Differs from <see cref="Xbrz"/> in exactly two ways, both of which 1.9 introduced. The
/// steep/shallow direction threshold rises from 2.2 to 2.4, so fewer near-diagonal runs are treated
/// as lines; and the weighted and 50/50 blends round to nearest instead of truncating, so blended
/// pixels stop being biased downwards by up to one step per channel.
/// </para>
/// <para>
/// Releases 1.5 through 1.8 needed nothing: 1.5 and 1.6 changed only the API and performance, 1.7's
/// centre direction bias made a hard-coded 4 configurable with 4 as its default and its asymmetric
/// colour distance was an artefact of a lookup table this port never used, and 1.8's
/// transparent-outside-area rule is already reachable per axis through <c>OutOfBoundsMode</c>. See
/// <c>Xbrz-VersionNotes.md</c> next to this file for the derivation.
/// </para>
/// <para>Reference: https://sourceforge.net/projects/xbrz</para>
/// </remarks>
[ScalerInfo("xBRZ 1.9", Author = "Zenju", Year = 2026,
  Description = "xBRZ 1.9 — 2.4 steep threshold and blends rounded to nearest",
  Category = ScalerCategory.Rescaler,
  Url = "https://sourceforge.net/projects/xbrz")]
public readonly struct Xbrz19 : IRescaler {

  private readonly int _scale;

  /// <summary>
  /// Creates an xBRZ 1.9 scaler with the specified scale factor.
  /// </summary>
  /// <param name="scale">The scale factor (2, 3, 4, 5, or 6).</param>
  public Xbrz19(int scale = 2) {
    ArgumentOutOfRangeException.ThrowIfLessThan(scale, 2);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(scale, 6);
    this._scale = scale;
  }

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
    => new Xbrz(this._scale == 0 ? 2 : this._scale, XbrzVariant.Version19)
      .InvokeKernel<TWork, TKey, TPixel, TDistance, TEquality, TLerp, TEncode, TResult>(callback, equality, lerp);

  /// <summary>Gets the list of scale factors supported by xBRZ 1.9.</summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2), new(3, 3), new(4, 4), new(5, 5), new(6, 6)];

  /// <summary>Determines whether xBRZ 1.9 supports the specified scale factor.</summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 } or { X: 3, Y: 3 } or { X: 4, Y: 4 } or { X: 5, Y: 5 } or { X: 6, Y: 6 };

  /// <summary>Enumerates all possible target dimensions for xBRZ 1.9.</summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
    yield return (sourceWidth * 3, sourceHeight * 3);
    yield return (sourceWidth * 4, sourceHeight * 4);
    yield return (sourceWidth * 5, sourceHeight * 5);
    yield return (sourceWidth * 6, sourceHeight * 6);
  }

  /// <summary>Gets an xBRZ 1.9 2x scaler.</summary>
  public static Xbrz19 Scale2x => new(2);

  /// <summary>Gets an xBRZ 1.9 3x scaler.</summary>
  public static Xbrz19 Scale3x => new(3);

  /// <summary>Gets an xBRZ 1.9 4x scaler.</summary>
  public static Xbrz19 Scale4x => new(4);

  /// <summary>Gets an xBRZ 1.9 5x scaler.</summary>
  public static Xbrz19 Scale5x => new(5);

  /// <summary>Gets an xBRZ 1.9 6x scaler.</summary>
  public static Xbrz19 Scale6x => new(6);

  /// <summary>Gets the default xBRZ 1.9 configuration (2x).</summary>
  public static Xbrz19 Default => Scale2x;
}

#endregion

#region xBRZ Algorithm Helpers

/// <summary>
/// Blend type indicators for xBRZ preprocessing.
/// </summary>
file enum BlendType : byte {
  /// <summary>Do not blend.</summary>
  None = 0,
  /// <summary>Normal blending indication.</summary>
  Normal = 1,
  /// <summary>Strong/dominant blending indication.</summary>
  Dominant = 2
}

/// <summary>
/// Stores blend results for all 4 corners of a 2x2 area.
/// </summary>
file struct BlendResult {
  public BlendType F;
  public BlendType G;
  public BlendType J;
  public BlendType K;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Reset() {
    this.F = this.G = this.J = this.K = BlendType.None;
  }
}

/// <summary>
/// Utility for packing/unpacking blend info into a single byte.
/// Layout: [bits 7-6: BottomL] [bits 5-4: BottomR] [bits 3-2: TopR] [bits 1-0: TopL]
/// </summary>
file static class BlendInfo {
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BlendType GetTopL(byte b) => (BlendType)(b & 0x3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BlendType GetTopR(byte b) => (BlendType)((b >> 2) & 0x3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BlendType GetBottomR(byte b) => (BlendType)((b >> 4) & 0x3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static BlendType GetBottomL(byte b) => (BlendType)((b >> 6) & 0x3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetTopL(byte b, BlendType bt) => (byte)(b | (byte)bt);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetTopR(byte b, BlendType bt) => (byte)(b | ((byte)bt << 2));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetBottomR(byte b, BlendType bt) => (byte)(b | ((byte)bt << 4));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte SetBottomL(byte b, BlendType bt) => (byte)(b | ((byte)bt << 6));

  /// <summary>
  /// Rotates blend info clockwise by rotDeg * 90 degrees.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte Rotate(byte b, int rotDeg) {
    var l = rotDeg << 1;
    var r = 8 - l;
    return (byte)((b << l) | (b >> r));
  }
}

/// <summary>
/// The knobs that differ between the xBRZ releases this port reproduces.
/// </summary>
/// <remarks>
/// <para>
/// Carried as a struct type parameter rather than a field so each kernel is specialised per
/// variant and both members fold to constants, leaving the inner loop with no version branch.
/// </para>
/// <para>
/// Only the two knobs that actually change output are here. See
/// <c>Xbrz-VersionNotes.md</c> next to this file for why 1.5 through 1.8 need none.
/// </para>
/// </remarks>
file interface IXbrzVariant {

  /// <summary>
  /// Gets the numerator of the steep/shallow direction threshold expressed against a denominator
  /// of 10: <c>22</c> for the 2.2 threshold used up to 1.8, <c>24</c> for the 2.4 threshold of 1.9.
  /// </summary>
  ulong SteepNumerator { get; }

  /// <summary>
  /// Gets whether weighted and 50/50 blends round to nearest (1.9) rather than truncate (up to 1.8).
  /// </summary>
  bool RoundsBlends { get; }
}

/// <summary>
/// xBRZ as released up to and including 1.8 — a 2.2 steep threshold and truncating blends.
/// </summary>
file readonly struct XbrzClassicVariant : IXbrzVariant {
  public ulong SteepNumerator => 22;
  public bool RoundsBlends => false;
}

/// <summary>
/// xBRZ 1.9 — a 2.4 steep threshold and blends rounded to nearest.
/// </summary>
file readonly struct Xbrz19Variant : IXbrzVariant {
  public ulong SteepNumerator => 24;
  public bool RoundsBlends => true;
}

/// <summary>
/// 1.9's steep threshold with the pre-1.9 truncating blends. Exists so a test can attribute an
/// output difference to the threshold alone; not reachable through the public surface.
/// </summary>
file readonly struct XbrzSteepOnlyVariant : IXbrzVariant {
  public ulong SteepNumerator => 24;
  public bool RoundsBlends => false;
}

/// <summary>
/// 1.9's rounded blends with the pre-1.9 steep threshold. Exists so a test can attribute an output
/// difference to the rounding alone; not reachable through the public surface.
/// </summary>
file readonly struct XbrzRoundOnlyVariant : IXbrzVariant {
  public ulong SteepNumerator => 22;
  public bool RoundsBlends => true;
}

/// <summary>
/// xBRZ configuration and helper methods.
/// </summary>
file static class XbrzHelpers {
  public const float LuminanceWeight = 1.0f;
  public const float DominantDirectionThreshold = 3.6f;

  /// <summary>The steep/shallow direction threshold up to release 1.8; 1.9 raised it to 2.4.</summary>
  public const float SteepDirectionThreshold = 2.2f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float Square(float v) => v * v;

  /// <summary>
  /// Compute color distance using YCbCr (ITU-R BT.709).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static UNorm32 ColorDistance<TKey, TMetric>(in TKey c1, in TKey c2, TMetric metric)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    => metric.Distance(c1, c2);

  /// <summary>
  /// Detect blend direction for the corner between F,G,J,K in a 4x4 kernel.
  /// </summary>
  /// <remarks>
  /// Kernel layout:
  /// | A | B | C | D |
  /// | E | F | G | H |
  /// | I | J | K | L |
  /// | M | N | O | P |
  /// Evaluates the corner between F, G, J, K (input pixel at F).
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static void PreProcessCorners<TKey, TMetric, TEquality>(
    in TKey b, in TKey c,
    in TKey e, in TKey f, in TKey g, in TKey h,
    in TKey i, in TKey j, in TKey k, in TKey l,
    in TKey n, in TKey o,
    ref BlendResult blendResult,
    TMetric metric,
    TEquality equality)
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey> {

    blendResult.Reset();

    // Early exit: if F==G && J==K or F==J && G==K, no blending needed
    var equalFG = equality.Equals(f, g);
    var equalJK = equality.Equals(j, k);
    var equalFJ = equality.Equals(f, j);
    var equalGK = equality.Equals(g, k);

    if (equalFG && equalJK)
      return;
    if (equalFJ && equalGK)
      return;

    const uint weight = 4;
    var jg = (ulong)metric.Distance(i, f).RawValue + metric.Distance(f, c).RawValue + metric.Distance(n, k).RawValue + metric.Distance(k, h).RawValue + weight * metric.Distance(j, g).RawValue;
    var fk = (ulong)metric.Distance(e, j).RawValue + metric.Distance(j, o).RawValue + metric.Distance(b, g).RawValue + metric.Distance(g, l).RawValue + weight * metric.Distance(f, k).RawValue;

    if (jg < fk) {
      var dominantGradient = jg * 36 < fk * 10; // DominantDirectionThreshold (3.6) as integer ratio
      if (!equalFG || !equalFJ)
        blendResult.F = dominantGradient ? BlendType.Dominant : BlendType.Normal;
      if (!equalJK || !equalGK)
        blendResult.K = dominantGradient ? BlendType.Dominant : BlendType.Normal;
    } else if (fk < jg) {
      var dominantGradient = fk * 36 < jg * 10; // DominantDirectionThreshold (3.6) as integer ratio
      if (!equalFJ || !equalJK)
        blendResult.J = dominantGradient ? BlendType.Dominant : BlendType.Normal;
      if (!equalFG || !equalGK)
        blendResult.G = dominantGradient ? BlendType.Dominant : BlendType.Normal;
    }
  }

  /// <summary>
  /// Computes the combined blend info byte for all 4 corners of the current pixel,
  /// using online preprocessing from the 5x5 neighbor window.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte ComputeBlendInfo<TWork, TKey, TMetric, TEquality>(
    in NeighborWindow<TWork, TKey> window,
    TMetric metric,
    TEquality equality)
    where TWork : unmanaged, IColorSpace
    where TKey : unmanaged, IColorSpace
    where TMetric : struct, IColorMetric<TKey>
    where TEquality : struct, IColorEquality<TKey> {

    byte blendXy = 0;
    var result = new BlendResult();

    // TopL: preprocess at (-1,-1), use result.K
    // Kernel: F at (-1,-1)
    // b=M2M1, c=M2P0, e=M1M2, f=M1M1, g=M1P0, h=M1P1
    // i=P0M2, j=P0M1, k=P0P0, l=P0P1, n=P1M1, o=P1P0
    PreProcessCorners(
      window.M2M1.Key, window.M2P0.Key,
      window.M1M2.Key, window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M2.Key, window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key,
      ref result, metric, equality);
    blendXy = BlendInfo.SetTopL(blendXy, result.K);

    // TopR: preprocess at (0,-1), use result.J
    // Kernel: F at (0,-1)
    // b=M2P0, c=M2P1, e=M1M1, f=M1P0, g=M1P1, h=M1P2
    // i=P0M1, j=P0P0, k=P0P1, l=P0P2, n=P1P0, o=P1P1
    PreProcessCorners(
      window.M2P0.Key, window.M2P1.Key,
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key, window.M1P2.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key, window.P0P2.Key,
      window.P1P0.Key, window.P1P1.Key,
      ref result, metric, equality);
    blendXy = BlendInfo.SetTopR(blendXy, result.J);

    // BottomL: preprocess at (-1,0), use result.G
    // Kernel: F at (-1,0)
    // b=M1M1, c=M1P0, e=P0M2, f=P0M1, g=P0P0, h=P0P1
    // i=P1M2, j=P1M1, k=P1P0, l=P1P1, n=P2M1, o=P2P0
    PreProcessCorners(
      window.M1M1.Key, window.M1P0.Key,
      window.P0M2.Key, window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M2.Key, window.P1M1.Key, window.P1P0.Key, window.P1P1.Key,
      window.P2M1.Key, window.P2P0.Key,
      ref result, metric, equality);
    blendXy = BlendInfo.SetBottomL(blendXy, result.G);

    // BottomR: preprocess at (0,0), use result.F
    // Kernel: F at (0,0)
    // b=M1P0, c=M1P1, e=P0M1, f=P0P0, g=P0P1, h=P0P2
    // i=P1M1, j=P1P0, k=P1P1, l=P1P2, n=P2P0, o=P2P1
    PreProcessCorners(
      window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key, window.P0P2.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key, window.P1P2.Key,
      window.P2P0.Key, window.P2P1.Key,
      ref result, metric, equality);
    blendXy = BlendInfo.SetBottomR(blendXy, result.F);

    return blendXy;
  }
}

/// <summary>
/// Rotation lookup table for mapping output positions across 4 rotations.
/// </summary>
file static class RotationLookup {
  private const int MaxScale = 6;
  private const int MaxRots = 4;

  // Stores (rotatedI, rotatedJ) for each (scale, rotation, i, j)
  private static readonly (int I, int J)[,,,] _lookup;

  static RotationLookup() {
    _lookup = new (int, int)[MaxScale + 1, MaxRots, MaxScale, MaxScale];

    for (var scale = 2; scale <= MaxScale; ++scale)
      for (var rot = 0; rot < MaxRots; ++rot)
        for (var i = 0; i < scale; ++i)
          for (var j = 0; j < scale; ++j)
            _lookup[scale, rot, i, j] = BuildRotation(rot, i, j, scale);
  }

  private static (int I, int J) BuildRotation(int rotDeg, int i, int j, int n) {
    if (rotDeg == 0)
      return (i, j);

    var (prevI, prevJ) = BuildRotation(rotDeg - 1, i, j, n);
    // Rotate 90 degrees clockwise: (row, col) -> (n-1-col, row)
    return (n - 1 - prevJ, prevI);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (int I, int J) Get(int scale, int rotation, int i, int j) => _lookup[scale, rotation, i, j];
}

/// <summary>
/// 3x3 kernel values rotated for pattern matching.
/// </summary>
file static class Rot3x3 {
  // Original positions: a=0,b=1,c=2,d=3,e=4,f=5,g=6,h=7,i=8
  // Rotation lookup: index = (position * 4 + rotation)
  public static readonly int[] Lookup = new int[9 * 4];

  static Rot3x3() {
    int[] deg0 = [0, 1, 2, 3, 4, 5, 6, 7, 8];
    int[] deg90 = [6, 3, 0, 7, 4, 1, 8, 5, 2];
    int[] deg180 = [8, 7, 6, 5, 4, 3, 2, 1, 0];
    int[] deg270 = [2, 5, 8, 1, 4, 7, 0, 3, 6];

    int[][] rotations = [deg0, deg90, deg180, deg270];

    for (var rot = 0; rot < 4; ++rot)
      for (var pos = 0; pos < 9; ++pos)
        Lookup[(pos << 2) + rot] = rotations[rot][pos];
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Get(int position, int rotation) => Lookup[(position << 2) + rotation];
}

#endregion

#region xBRZ 2x Kernel

file readonly struct Xbrz2xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode, TVariant>(TEquality equality = default, TMetric metric = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TVariant : struct, IXbrzVariant {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n4 = window.P0P0;
    var w4 = n4.Work;
    var pC = encoder.Encode(w4);

    // Initialize all output pixels to center
    dest[0] = pC; dest[1] = pC;
    dest[destStride] = pC; dest[destStride + 1] = pC;

    // Compute blend info for all 4 corners
    var blendXy = XbrzHelpers.ComputeBlendInfo(window, metric, equality);
    if (blendXy == 0)
      return;

    // 3x3 kernel for pattern matching
    TKey[] ker = [
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key
    ];
    TWork[] work = [
      window.M1M1.Work, window.M1P0.Work, window.M1P1.Work,
      window.P0M1.Work, window.P0P0.Work, window.P0P1.Work,
      window.P1M1.Work, window.P1P0.Work, window.P1P1.Work
    ];

    // Process all 4 rotations
    for (var rot = 0; rot < 4; ++rot)
      ScalePixel2x(ker, work, blendXy, rot, dest, destStride, encoder, lerp, metric, equality);
  }

  /// <summary>
  /// Applies a weighted blend in the rounding mode this variant calls for.
  /// </summary>
  /// <remarks>
  /// <typeparamref name="TVariant"/> is a struct type parameter, so the branch is resolved when the
  /// kernel is specialised and costs the pre-1.9 path nothing.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b, int w1, int w2)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b, w1, w2) : lerp.Lerp(a, b, w1, w2);

  /// <summary>
  /// Applies a 50/50 blend in the rounding mode this variant calls for.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b) : lerp.Lerp(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ScalePixel2x(
    TKey[] ker, TWork[] work,
    byte blendXy, int rotDeg,
    TPixel* dest, int destStride,
    in TEncode encoder, TLerp lerp, TMetric metric, TEquality equality) {

    var blend = BlendInfo.Rotate(blendXy, rotDeg);
    if (BlendInfo.GetBottomR(blend) == BlendType.None)
      return;

    // Get rotated kernel values
    var b = ker[Rot3x3.Get(1, rotDeg)];
    var c = ker[Rot3x3.Get(2, rotDeg)];
    var d = ker[Rot3x3.Get(3, rotDeg)];
    var e = ker[Rot3x3.Get(4, rotDeg)];
    var f = ker[Rot3x3.Get(5, rotDeg)];
    var g = ker[Rot3x3.Get(6, rotDeg)];
    var h = ker[Rot3x3.Get(7, rotDeg)];

    var we = work[Rot3x3.Get(4, rotDeg)];
    var wf = work[Rot3x3.Get(5, rotDeg)];
    var wh = work[Rot3x3.Get(7, rotDeg)];

    bool doLineBlend;
    if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
      doLineBlend = true;
    else if (BlendInfo.GetTopR(blend) != BlendType.None && !equality.Equals(e, g))
      doLineBlend = false;
    else if (BlendInfo.GetBottomL(blend) != BlendType.None && !equality.Equals(e, c))
      doLineBlend = false;
    else if (equality.Equals(g, h) && equality.Equals(h, ker[Rot3x3.Get(8, rotDeg)]) &&
             equality.Equals(ker[Rot3x3.Get(8, rotDeg)], f) && equality.Equals(f, c) &&
             !equality.Equals(e, ker[Rot3x3.Get(8, rotDeg)]))
      doLineBlend = false;
    else
      doLineBlend = true;

    var px = metric.Distance(e, f) <= metric.Distance(e, h) ? wf : wh;

    var (ri1, rj1) = RotationLookup.Get(2, rotDeg, 1, 1);

    if (!doLineBlend) {
      // BlendCorner for 2x: blend pixel (1,1) with weight 21/100
      dest[ri1 * destStride + rj1] = encoder.Encode(Blend(lerp, we, px, 79, 21));
      return;
    }

    var fg = metric.Distance(f, g);
    var hc = metric.Distance(h, c);
    var steepNumerator = default(TVariant).SteepNumerator;

    var haveShallowLine = (ulong)fg.RawValue * steepNumerator <= (ulong)hc.RawValue * 10 && !equality.Equals(e, g) && !equality.Equals(d, g); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on
    var haveSteepLine = (ulong)hc.RawValue * steepNumerator <= (ulong)fg.RawValue * 10 && !equality.Equals(e, c) && !equality.Equals(b, c); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on

    var (ri0, rj1_0) = RotationLookup.Get(2, rotDeg, 1, 0);
    var (ri1_0, rj0) = RotationLookup.Get(2, rotDeg, 0, 1);

    if (haveShallowLine) {
      if (haveSteepLine) {
        // BlendLineSteepAndShallow
        dest[ri0 * destStride + rj1_0] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri1_0 * destStride + rj0] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri1 * destStride + rj1] = encoder.Encode(Blend(lerp, we, px, 1, 5));
      } else {
        // BlendLineShallow
        dest[ri0 * destStride + rj1_0] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri1 * destStride + rj1] = encoder.Encode(Blend(lerp, we, px, 1, 3));
      }
    } else {
      if (haveSteepLine) {
        // BlendLineSteep
        dest[ri1_0 * destStride + rj0] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri1 * destStride + rj1] = encoder.Encode(Blend(lerp, we, px, 1, 3));
      } else {
        // BlendLineDiagonal
        dest[ri1 * destStride + rj1] = encoder.Encode(Blend(lerp, we, px));
      }
    }
  }
}

#endregion

#region xBRZ 3x Kernel

file readonly struct Xbrz3xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode, TVariant>(TEquality equality = default, TMetric metric = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TVariant : struct, IXbrzVariant {

  public int ScaleX => 3;
  public int ScaleY => 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n4 = window.P0P0;
    var w4 = n4.Work;
    var pC = encoder.Encode(w4);

    // Initialize all 9 output pixels to center
    for (var y = 0; y < 3; ++y)
      for (var x = 0; x < 3; ++x)
        dest[y * destStride + x] = pC;

    var blendXy = XbrzHelpers.ComputeBlendInfo(window, metric, equality);
    if (blendXy == 0)
      return;

    TKey[] ker = [
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key
    ];
    TWork[] work = [
      window.M1M1.Work, window.M1P0.Work, window.M1P1.Work,
      window.P0M1.Work, window.P0P0.Work, window.P0P1.Work,
      window.P1M1.Work, window.P1P0.Work, window.P1P1.Work
    ];

    for (var rot = 0; rot < 4; ++rot)
      ScalePixel3x(ker, work, blendXy, rot, dest, destStride, encoder, lerp, metric, equality);
  }

  /// <summary>
  /// Applies a weighted blend in the rounding mode this variant calls for.
  /// </summary>
  /// <remarks>
  /// <typeparamref name="TVariant"/> is a struct type parameter, so the branch is resolved when the
  /// kernel is specialised and costs the pre-1.9 path nothing.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b, int w1, int w2)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b, w1, w2) : lerp.Lerp(a, b, w1, w2);

  /// <summary>
  /// Applies a 50/50 blend in the rounding mode this variant calls for.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b) : lerp.Lerp(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ScalePixel3x(
    TKey[] ker, TWork[] work,
    byte blendXy, int rotDeg,
    TPixel* dest, int destStride,
    in TEncode encoder, TLerp lerp, TMetric metric, TEquality equality) {

    var blend = BlendInfo.Rotate(blendXy, rotDeg);
    if (BlendInfo.GetBottomR(blend) == BlendType.None)
      return;

    var b = ker[Rot3x3.Get(1, rotDeg)];
    var c = ker[Rot3x3.Get(2, rotDeg)];
    var d = ker[Rot3x3.Get(3, rotDeg)];
    var e = ker[Rot3x3.Get(4, rotDeg)];
    var f = ker[Rot3x3.Get(5, rotDeg)];
    var g = ker[Rot3x3.Get(6, rotDeg)];
    var h = ker[Rot3x3.Get(7, rotDeg)];
    var i = ker[Rot3x3.Get(8, rotDeg)];

    var we = work[Rot3x3.Get(4, rotDeg)];
    var wf = work[Rot3x3.Get(5, rotDeg)];
    var wh = work[Rot3x3.Get(7, rotDeg)];

    bool doLineBlend;
    if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
      doLineBlend = true;
    else if (BlendInfo.GetTopR(blend) != BlendType.None && !equality.Equals(e, g))
      doLineBlend = false;
    else if (BlendInfo.GetBottomL(blend) != BlendType.None && !equality.Equals(e, c))
      doLineBlend = false;
    else if (equality.Equals(g, h) && equality.Equals(h, i) && equality.Equals(i, f) && equality.Equals(f, c) && !equality.Equals(e, i))
      doLineBlend = false;
    else
      doLineBlend = true;

    var px = metric.Distance(e, f) <= metric.Distance(e, h) ? wf : wh;

    var (ri2j2i, ri2j2j) = RotationLookup.Get(3, rotDeg, 2, 2);

    if (!doLineBlend) {
      // BlendCorner for 3x: blend pixel (2,2) with weight 45/100
      dest[ri2j2i * destStride + ri2j2j] = encoder.Encode(Blend(lerp, we, px, 11, 9));
      return;
    }

    var fg = metric.Distance(f, g);
    var hc = metric.Distance(h, c);
    var steepNumerator = default(TVariant).SteepNumerator;

    var haveShallowLine = (ulong)fg.RawValue * steepNumerator <= (ulong)hc.RawValue * 10 && !equality.Equals(e, g) && !equality.Equals(d, g); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on
    var haveSteepLine = (ulong)hc.RawValue * steepNumerator <= (ulong)fg.RawValue * 10 && !equality.Equals(e, c) && !equality.Equals(b, c); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on

    if (haveShallowLine) {
      if (haveSteepLine) {
        // BlendLineSteepAndShallow for 3x
        var (ri20, rj20) = RotationLookup.Get(3, rotDeg, 2, 0);
        var (ri02, rj02) = RotationLookup.Get(3, rotDeg, 0, 2);
        var (ri21, rj21) = RotationLookup.Get(3, rotDeg, 2, 1);
        var (ri12, rj12) = RotationLookup.Get(3, rotDeg, 1, 2);

        dest[ri20 * destStride + rj20] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri02 * destStride + rj02] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri21 * destStride + rj21] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri12 * destStride + rj12] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri2j2i * destStride + ri2j2j] = encoder.Encode(px);
      } else {
        // BlendLineShallow for 3x
        var (ri20, rj20) = RotationLookup.Get(3, rotDeg, 2, 0);
        var (ri12, rj12) = RotationLookup.Get(3, rotDeg, 1, 2);
        var (ri21, rj21) = RotationLookup.Get(3, rotDeg, 2, 1);

        dest[ri20 * destStride + rj20] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri12 * destStride + rj12] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri21 * destStride + rj21] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri2j2i * destStride + ri2j2j] = encoder.Encode(px);
      }
    } else {
      if (haveSteepLine) {
        // BlendLineSteep for 3x
        var (ri02, rj02) = RotationLookup.Get(3, rotDeg, 0, 2);
        var (ri21, rj21) = RotationLookup.Get(3, rotDeg, 2, 1);
        var (ri12, rj12) = RotationLookup.Get(3, rotDeg, 1, 2);

        dest[ri02 * destStride + rj02] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri21 * destStride + rj21] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri12 * destStride + rj12] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri2j2i * destStride + ri2j2j] = encoder.Encode(px);
      } else {
        // BlendLineDiagonal for 3x
        var (ri12, rj12) = RotationLookup.Get(3, rotDeg, 1, 2);
        var (ri21, rj21) = RotationLookup.Get(3, rotDeg, 2, 1);

        dest[ri12 * destStride + rj12] = encoder.Encode(Blend(lerp, we, px, 7, 1));
        dest[ri21 * destStride + rj21] = encoder.Encode(Blend(lerp, we, px, 7, 1));
        dest[ri2j2i * destStride + ri2j2j] = encoder.Encode(Blend(lerp, we, px, 1, 7));
      }
    }
  }
}

#endregion

#region xBRZ 4x Kernel

file readonly struct Xbrz4xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode, TVariant>(TEquality equality = default, TMetric metric = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TVariant : struct, IXbrzVariant {

  public int ScaleX => 4;
  public int ScaleY => 4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n4 = window.P0P0;
    var w4 = n4.Work;
    var pC = encoder.Encode(w4);

    for (var y = 0; y < 4; ++y)
      for (var x = 0; x < 4; ++x)
        dest[y * destStride + x] = pC;

    var blendXy = XbrzHelpers.ComputeBlendInfo(window, metric, equality);
    if (blendXy == 0)
      return;

    TKey[] ker = [
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key
    ];
    TWork[] work = [
      window.M1M1.Work, window.M1P0.Work, window.M1P1.Work,
      window.P0M1.Work, window.P0P0.Work, window.P0P1.Work,
      window.P1M1.Work, window.P1P0.Work, window.P1P1.Work
    ];

    for (var rot = 0; rot < 4; ++rot)
      ScalePixel4x(ker, work, blendXy, rot, dest, destStride, encoder, lerp, metric, equality);
  }

  /// <summary>
  /// Applies a weighted blend in the rounding mode this variant calls for.
  /// </summary>
  /// <remarks>
  /// <typeparamref name="TVariant"/> is a struct type parameter, so the branch is resolved when the
  /// kernel is specialised and costs the pre-1.9 path nothing.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b, int w1, int w2)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b, w1, w2) : lerp.Lerp(a, b, w1, w2);

  /// <summary>
  /// Applies a 50/50 blend in the rounding mode this variant calls for.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b) : lerp.Lerp(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ScalePixel4x(
    TKey[] ker, TWork[] work,
    byte blendXy, int rotDeg,
    TPixel* dest, int destStride,
    in TEncode encoder, TLerp lerp, TMetric metric, TEquality equality) {

    var blend = BlendInfo.Rotate(blendXy, rotDeg);
    if (BlendInfo.GetBottomR(blend) == BlendType.None)
      return;

    var b = ker[Rot3x3.Get(1, rotDeg)];
    var c = ker[Rot3x3.Get(2, rotDeg)];
    var d = ker[Rot3x3.Get(3, rotDeg)];
    var e = ker[Rot3x3.Get(4, rotDeg)];
    var f = ker[Rot3x3.Get(5, rotDeg)];
    var g = ker[Rot3x3.Get(6, rotDeg)];
    var h = ker[Rot3x3.Get(7, rotDeg)];
    var i = ker[Rot3x3.Get(8, rotDeg)];

    var we = work[Rot3x3.Get(4, rotDeg)];
    var wf = work[Rot3x3.Get(5, rotDeg)];
    var wh = work[Rot3x3.Get(7, rotDeg)];

    bool doLineBlend;
    if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
      doLineBlend = true;
    else if (BlendInfo.GetTopR(blend) != BlendType.None && !equality.Equals(e, g))
      doLineBlend = false;
    else if (BlendInfo.GetBottomL(blend) != BlendType.None && !equality.Equals(e, c))
      doLineBlend = false;
    else if (equality.Equals(g, h) && equality.Equals(h, i) && equality.Equals(i, f) && equality.Equals(f, c) && !equality.Equals(e, i))
      doLineBlend = false;
    else
      doLineBlend = true;

    var px = metric.Distance(e, f) <= metric.Distance(e, h) ? wf : wh;

    var (ri33i, ri33j) = RotationLookup.Get(4, rotDeg, 3, 3);
    var (ri32i, ri32j) = RotationLookup.Get(4, rotDeg, 3, 2);
    var (ri23i, ri23j) = RotationLookup.Get(4, rotDeg, 2, 3);

    if (!doLineBlend) {
      // BlendCorner for 4x
      dest[ri33i * destStride + ri33j] = encoder.Encode(Blend(lerp, we, px, 8, 17));
      dest[ri32i * destStride + ri32j] = encoder.Encode(Blend(lerp, we, px, 91, 9));
      dest[ri23i * destStride + ri23j] = encoder.Encode(Blend(lerp, we, px, 91, 9));
      return;
    }

    var fg = metric.Distance(f, g);
    var hc = metric.Distance(h, c);
    var steepNumerator = default(TVariant).SteepNumerator;

    var haveShallowLine = (ulong)fg.RawValue * steepNumerator <= (ulong)hc.RawValue * 10 && !equality.Equals(e, g) && !equality.Equals(d, g); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on
    var haveSteepLine = (ulong)hc.RawValue * steepNumerator <= (ulong)fg.RawValue * 10 && !equality.Equals(e, c) && !equality.Equals(b, c); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on

    if (haveShallowLine) {
      if (haveSteepLine) {
        // BlendLineSteepAndShallow for 4x
        var (ri31i, ri31j) = RotationLookup.Get(4, rotDeg, 3, 1);
        var (ri13i, ri13j) = RotationLookup.Get(4, rotDeg, 1, 3);
        var (ri30i, ri30j) = RotationLookup.Get(4, rotDeg, 3, 0);
        var (ri03i, ri03j) = RotationLookup.Get(4, rotDeg, 0, 3);
        var (ri22i, ri22j) = RotationLookup.Get(4, rotDeg, 2, 2);

        dest[ri31i * destStride + ri31j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri13i * destStride + ri13j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri30i * destStride + ri30j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri03i * destStride + ri03j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri22i * destStride + ri22j] = encoder.Encode(Blend(lerp, we, px, 2, 1));
        dest[ri33i * destStride + ri33j] = encoder.Encode(px);
        dest[ri32i * destStride + ri32j] = encoder.Encode(px);
        dest[ri23i * destStride + ri23j] = encoder.Encode(px);
      } else {
        // BlendLineShallow for 4x
        var (ri30i, ri30j) = RotationLookup.Get(4, rotDeg, 3, 0);
        var (ri22i, ri22j) = RotationLookup.Get(4, rotDeg, 2, 2);
        var (ri31i, ri31j) = RotationLookup.Get(4, rotDeg, 3, 1);
        var (ri23i2, ri23j2) = RotationLookup.Get(4, rotDeg, 2, 3);

        dest[ri30i * destStride + ri30j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri22i * destStride + ri22j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri31i * destStride + ri31j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri23i2 * destStride + ri23j2] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri32i * destStride + ri32j] = encoder.Encode(px);
        dest[ri33i * destStride + ri33j] = encoder.Encode(px);
      }
    } else {
      if (haveSteepLine) {
        // BlendLineSteep for 4x
        var (ri03i, ri03j) = RotationLookup.Get(4, rotDeg, 0, 3);
        var (ri22i, ri22j) = RotationLookup.Get(4, rotDeg, 2, 2);
        var (ri13i, ri13j) = RotationLookup.Get(4, rotDeg, 1, 3);
        var (ri32i2, ri32j2) = RotationLookup.Get(4, rotDeg, 3, 2);

        dest[ri03i * destStride + ri03j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri22i * destStride + ri22j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri13i * destStride + ri13j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri32i2 * destStride + ri32j2] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri23i * destStride + ri23j] = encoder.Encode(px);
        dest[ri33i * destStride + ri33j] = encoder.Encode(px);
      } else {
        // BlendLineDiagonal for 4x
        var (ri32d, rj32d) = RotationLookup.Get(4, rotDeg, 3, 2);
        var (ri23d, rj23d) = RotationLookup.Get(4, rotDeg, 2, 3);

        dest[ri32d * destStride + rj32d] = encoder.Encode(Blend(lerp, we, px));
        dest[ri23d * destStride + rj23d] = encoder.Encode(Blend(lerp, we, px));
        dest[ri33i * destStride + ri33j] = encoder.Encode(px);
      }
    }
  }
}

#endregion

#region xBRZ 5x Kernel

file readonly struct Xbrz5xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode, TVariant>(TEquality equality = default, TMetric metric = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TVariant : struct, IXbrzVariant {

  public int ScaleX => 5;
  public int ScaleY => 5;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n4 = window.P0P0;
    var w4 = n4.Work;
    var pC = encoder.Encode(w4);

    for (var y = 0; y < 5; ++y)
      for (var x = 0; x < 5; ++x)
        dest[y * destStride + x] = pC;

    var blendXy = XbrzHelpers.ComputeBlendInfo(window, metric, equality);
    if (blendXy == 0)
      return;

    TKey[] ker = [
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key
    ];
    TWork[] work = [
      window.M1M1.Work, window.M1P0.Work, window.M1P1.Work,
      window.P0M1.Work, window.P0P0.Work, window.P0P1.Work,
      window.P1M1.Work, window.P1P0.Work, window.P1P1.Work
    ];

    for (var rot = 0; rot < 4; ++rot)
      ScalePixel5x(ker, work, blendXy, rot, dest, destStride, encoder, lerp, metric, equality);
  }

  /// <summary>
  /// Applies a weighted blend in the rounding mode this variant calls for.
  /// </summary>
  /// <remarks>
  /// <typeparamref name="TVariant"/> is a struct type parameter, so the branch is resolved when the
  /// kernel is specialised and costs the pre-1.9 path nothing.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b, int w1, int w2)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b, w1, w2) : lerp.Lerp(a, b, w1, w2);

  /// <summary>
  /// Applies a 50/50 blend in the rounding mode this variant calls for.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b) : lerp.Lerp(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ScalePixel5x(
    TKey[] ker, TWork[] work,
    byte blendXy, int rotDeg,
    TPixel* dest, int destStride,
    in TEncode encoder, TLerp lerp, TMetric metric, TEquality equality) {

    var blend = BlendInfo.Rotate(blendXy, rotDeg);
    if (BlendInfo.GetBottomR(blend) == BlendType.None)
      return;

    var b = ker[Rot3x3.Get(1, rotDeg)];
    var c = ker[Rot3x3.Get(2, rotDeg)];
    var d = ker[Rot3x3.Get(3, rotDeg)];
    var e = ker[Rot3x3.Get(4, rotDeg)];
    var f = ker[Rot3x3.Get(5, rotDeg)];
    var g = ker[Rot3x3.Get(6, rotDeg)];
    var h = ker[Rot3x3.Get(7, rotDeg)];
    var i = ker[Rot3x3.Get(8, rotDeg)];

    var we = work[Rot3x3.Get(4, rotDeg)];
    var wf = work[Rot3x3.Get(5, rotDeg)];
    var wh = work[Rot3x3.Get(7, rotDeg)];

    bool doLineBlend;
    if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
      doLineBlend = true;
    else if (BlendInfo.GetTopR(blend) != BlendType.None && !equality.Equals(e, g))
      doLineBlend = false;
    else if (BlendInfo.GetBottomL(blend) != BlendType.None && !equality.Equals(e, c))
      doLineBlend = false;
    else if (equality.Equals(g, h) && equality.Equals(h, i) && equality.Equals(i, f) && equality.Equals(f, c) && !equality.Equals(e, i))
      doLineBlend = false;
    else
      doLineBlend = true;

    var px = metric.Distance(e, f) <= metric.Distance(e, h) ? wf : wh;

    var (ri44i, ri44j) = RotationLookup.Get(5, rotDeg, 4, 4);
    var (ri43i, ri43j) = RotationLookup.Get(5, rotDeg, 4, 3);
    var (ri34i, ri34j) = RotationLookup.Get(5, rotDeg, 3, 4);

    if (!doLineBlend) {
      // BlendCorner for 5x
      dest[ri44i * destStride + ri44j] = encoder.Encode(Blend(lerp, we, px, 7, 43));
      dest[ri43i * destStride + ri43j] = encoder.Encode(Blend(lerp, we, px, 77, 23));
      dest[ri34i * destStride + ri34j] = encoder.Encode(Blend(lerp, we, px, 77, 23));
      return;
    }

    var fg = metric.Distance(f, g);
    var hc = metric.Distance(h, c);
    var steepNumerator = default(TVariant).SteepNumerator;

    var haveShallowLine = (ulong)fg.RawValue * steepNumerator <= (ulong)hc.RawValue * 10 && !equality.Equals(e, g) && !equality.Equals(d, g); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on
    var haveSteepLine = (ulong)hc.RawValue * steepNumerator <= (ulong)fg.RawValue * 10 && !equality.Equals(e, c) && !equality.Equals(b, c); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on

    if (haveShallowLine) {
      if (haveSteepLine) {
        // BlendLineSteepAndShallow for 5x
        var (ri04i, ri04j) = RotationLookup.Get(5, rotDeg, 0, 4);
        var (ri24i, ri24j) = RotationLookup.Get(5, rotDeg, 2, 4);
        var (ri14i, ri14j) = RotationLookup.Get(5, rotDeg, 1, 4);
        var (ri40i, ri40j) = RotationLookup.Get(5, rotDeg, 4, 0);
        var (ri42i, ri42j) = RotationLookup.Get(5, rotDeg, 4, 2);
        var (ri41i, ri41j) = RotationLookup.Get(5, rotDeg, 4, 1);
        var (ri24s, rj24s) = RotationLookup.Get(5, rotDeg, 2, 4);
        var (ri33i, ri33j) = RotationLookup.Get(5, rotDeg, 3, 3);

        dest[ri04i * destStride + ri04j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri24i * destStride + ri24j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri14i * destStride + ri14j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri40i * destStride + ri40j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri42i * destStride + ri42j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri41i * destStride + ri41j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri24s * destStride + rj24s] = encoder.Encode(px);
        dest[ri34i * destStride + ri34j] = encoder.Encode(px);
        dest[ri43i * destStride + ri43j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri33i * destStride + ri33j] = encoder.Encode(Blend(lerp, we, px, 1, 2));
      } else {
        // BlendLineShallow for 5x
        var (ri40i, ri40j) = RotationLookup.Get(5, rotDeg, 4, 0);
        var (ri32i, ri32j) = RotationLookup.Get(5, rotDeg, 3, 2);
        var (ri24i, ri24j) = RotationLookup.Get(5, rotDeg, 2, 4);
        var (ri41i, ri41j) = RotationLookup.Get(5, rotDeg, 4, 1);
        var (ri33i, ri33j) = RotationLookup.Get(5, rotDeg, 3, 3);
        var (ri42i, ri42j) = RotationLookup.Get(5, rotDeg, 4, 2);

        dest[ri40i * destStride + ri40j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri32i * destStride + ri32j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri24i * destStride + ri24j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri41i * destStride + ri41j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri33i * destStride + ri33j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri42i * destStride + ri42j] = encoder.Encode(px);
        dest[ri43i * destStride + ri43j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri34i * destStride + ri34j] = encoder.Encode(px);
      }
    } else {
      if (haveSteepLine) {
        // BlendLineSteep for 5x
        var (ri04i, ri04j) = RotationLookup.Get(5, rotDeg, 0, 4);
        var (ri23i, ri23j) = RotationLookup.Get(5, rotDeg, 2, 3);
        var (ri42i, ri42j) = RotationLookup.Get(5, rotDeg, 4, 2);
        var (ri14i, ri14j) = RotationLookup.Get(5, rotDeg, 1, 4);
        var (ri33i, ri33j) = RotationLookup.Get(5, rotDeg, 3, 3);
        var (ri24i, ri24j) = RotationLookup.Get(5, rotDeg, 2, 4);

        dest[ri04i * destStride + ri04j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri23i * destStride + ri23j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri42i * destStride + ri42j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri14i * destStride + ri14j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri33i * destStride + ri33j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri24i * destStride + ri24j] = encoder.Encode(px);
        dest[ri34i * destStride + ri34j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri43i * destStride + ri43j] = encoder.Encode(px);
      } else {
        // BlendLineDiagonal for 5x
        var (ri42i, ri42j) = RotationLookup.Get(5, rotDeg, 4, 2);
        var (ri33i, ri33j) = RotationLookup.Get(5, rotDeg, 3, 3);
        var (ri24i, ri24j) = RotationLookup.Get(5, rotDeg, 2, 4);

        dest[ri42i * destStride + ri42j] = encoder.Encode(Blend(lerp, we, px, 7, 1));
        dest[ri33i * destStride + ri33j] = encoder.Encode(Blend(lerp, we, px, 7, 1));
        dest[ri24i * destStride + ri24j] = encoder.Encode(Blend(lerp, we, px, 7, 1));
        dest[ri43i * destStride + ri43j] = encoder.Encode(Blend(lerp, we, px, 1, 7));
        dest[ri34i * destStride + ri34j] = encoder.Encode(Blend(lerp, we, px, 1, 7));
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
      }
    }
  }
}

#endregion

#region xBRZ 6x Kernel

file readonly struct Xbrz6xKernel<TWork, TKey, TPixel, TEquality, TMetric, TLerp, TEncode, TVariant>(TEquality equality = default, TMetric metric = default, TLerp lerp = default)
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEquality : struct, IColorEquality<TKey>
  where TMetric : struct, IColorMetric<TKey>
  where TLerp : struct, ILerp<TWork>
  where TEncode : struct, IEncode<TWork, TPixel>
  where TVariant : struct, IXbrzVariant {

  public int ScaleX => 6;
  public int ScaleY => 6;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(in NeighborWindow<TWork, TKey> window, TPixel* dest, int destStride, in TEncode encoder) {
    var n4 = window.P0P0;
    var w4 = n4.Work;
    var pC = encoder.Encode(w4);

    for (var y = 0; y < 6; ++y)
      for (var x = 0; x < 6; ++x)
        dest[y * destStride + x] = pC;

    var blendXy = XbrzHelpers.ComputeBlendInfo(window, metric, equality);
    if (blendXy == 0)
      return;

    TKey[] ker = [
      window.M1M1.Key, window.M1P0.Key, window.M1P1.Key,
      window.P0M1.Key, window.P0P0.Key, window.P0P1.Key,
      window.P1M1.Key, window.P1P0.Key, window.P1P1.Key
    ];
    TWork[] work = [
      window.M1M1.Work, window.M1P0.Work, window.M1P1.Work,
      window.P0M1.Work, window.P0P0.Work, window.P0P1.Work,
      window.P1M1.Work, window.P1P0.Work, window.P1P1.Work
    ];

    for (var rot = 0; rot < 4; ++rot)
      ScalePixel6x(ker, work, blendXy, rot, dest, destStride, encoder, lerp, metric, equality);
  }

  /// <summary>
  /// Applies a weighted blend in the rounding mode this variant calls for.
  /// </summary>
  /// <remarks>
  /// <typeparamref name="TVariant"/> is a struct type parameter, so the branch is resolved when the
  /// kernel is specialised and costs the pre-1.9 path nothing.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b, int w1, int w2)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b, w1, w2) : lerp.Lerp(a, b, w1, w2);

  /// <summary>
  /// Applies a 50/50 blend in the rounding mode this variant calls for.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork Blend(TLerp lerp, in TWork a, in TWork b)
    => default(TVariant).RoundsBlends ? lerp.LerpRounded(a, b) : lerp.Lerp(a, b);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ScalePixel6x(
    TKey[] ker, TWork[] work,
    byte blendXy, int rotDeg,
    TPixel* dest, int destStride,
    in TEncode encoder, TLerp lerp, TMetric metric, TEquality equality) {

    var blend = BlendInfo.Rotate(blendXy, rotDeg);
    if (BlendInfo.GetBottomR(blend) == BlendType.None)
      return;

    var b = ker[Rot3x3.Get(1, rotDeg)];
    var c = ker[Rot3x3.Get(2, rotDeg)];
    var d = ker[Rot3x3.Get(3, rotDeg)];
    var e = ker[Rot3x3.Get(4, rotDeg)];
    var f = ker[Rot3x3.Get(5, rotDeg)];
    var g = ker[Rot3x3.Get(6, rotDeg)];
    var h = ker[Rot3x3.Get(7, rotDeg)];
    var i = ker[Rot3x3.Get(8, rotDeg)];

    var we = work[Rot3x3.Get(4, rotDeg)];
    var wf = work[Rot3x3.Get(5, rotDeg)];
    var wh = work[Rot3x3.Get(7, rotDeg)];

    bool doLineBlend;
    if (BlendInfo.GetBottomR(blend) >= BlendType.Dominant)
      doLineBlend = true;
    else if (BlendInfo.GetTopR(blend) != BlendType.None && !equality.Equals(e, g))
      doLineBlend = false;
    else if (BlendInfo.GetBottomL(blend) != BlendType.None && !equality.Equals(e, c))
      doLineBlend = false;
    else if (equality.Equals(g, h) && equality.Equals(h, i) && equality.Equals(i, f) && equality.Equals(f, c) && !equality.Equals(e, i))
      doLineBlend = false;
    else
      doLineBlend = true;

    var px = metric.Distance(e, f) <= metric.Distance(e, h) ? wf : wh;

    // Extrapolated 6x pattern from 5x
    var (ri55i, ri55j) = RotationLookup.Get(6, rotDeg, 5, 5);
    var (ri54i, ri54j) = RotationLookup.Get(6, rotDeg, 5, 4);
    var (ri45i, ri45j) = RotationLookup.Get(6, rotDeg, 4, 5);

    if (!doLineBlend) {
      // BlendCorner for 6x - exact weights from reference: 97/100, 42/100, 6/100
      dest[ri55i * destStride + ri55j] = encoder.Encode(Blend(lerp, we, px, 3, 97));
      dest[ri54i * destStride + ri54j] = encoder.Encode(Blend(lerp, we, px, 29, 21));
      dest[ri45i * destStride + ri45j] = encoder.Encode(Blend(lerp, we, px, 29, 21));
      var (ri53i, ri53j) = RotationLookup.Get(6, rotDeg, 5, 3);
      var (ri35i, ri35j) = RotationLookup.Get(6, rotDeg, 3, 5);
      dest[ri53i * destStride + ri53j] = encoder.Encode(Blend(lerp, we, px, 47, 3));
      dest[ri35i * destStride + ri35j] = encoder.Encode(Blend(lerp, we, px, 47, 3));
      return;
    }

    var fg = metric.Distance(f, g);
    var hc = metric.Distance(h, c);
    var steepNumerator = default(TVariant).SteepNumerator;

    var haveShallowLine = (ulong)fg.RawValue * steepNumerator <= (ulong)hc.RawValue * 10 && !equality.Equals(e, g) && !equality.Equals(d, g); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on
    var haveSteepLine = (ulong)hc.RawValue * steepNumerator <= (ulong)fg.RawValue * 10 && !equality.Equals(e, c) && !equality.Equals(b, c); // SteepDirectionThreshold as an integer ratio: 2.2 before xBRZ 1.9, 2.4 from 1.9 on

    if (haveShallowLine) {
      if (haveSteepLine) {
        // BlendLineSteepAndShallow for 6x - from reference
        var (ri05i, ri05j) = RotationLookup.Get(6, rotDeg, 0, 5);
        var (ri24i, ri24j) = RotationLookup.Get(6, rotDeg, 2, 4);
        var (ri15i, ri15j) = RotationLookup.Get(6, rotDeg, 1, 5);
        var (ri34i, ri34j) = RotationLookup.Get(6, rotDeg, 3, 4);
        var (ri50i, ri50j) = RotationLookup.Get(6, rotDeg, 5, 0);
        var (ri42i, ri42j) = RotationLookup.Get(6, rotDeg, 4, 2);
        var (ri51i, ri51j) = RotationLookup.Get(6, rotDeg, 5, 1);
        var (ri43i, ri43j) = RotationLookup.Get(6, rotDeg, 4, 3);
        var (ri25i, ri25j) = RotationLookup.Get(6, rotDeg, 2, 5);
        var (ri35i, ri35j) = RotationLookup.Get(6, rotDeg, 3, 5);
        var (ri44i, ri44j) = RotationLookup.Get(6, rotDeg, 4, 4);
        var (ri52i, ri52j) = RotationLookup.Get(6, rotDeg, 5, 2);
        var (ri53i, ri53j) = RotationLookup.Get(6, rotDeg, 5, 3);

        // Gradient blends
        dest[ri05i * destStride + ri05j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri24i * destStride + ri24j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri15i * destStride + ri15j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri34i * destStride + ri34j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri50i * destStride + ri50j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri42i * destStride + ri42j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri51i * destStride + ri51j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri43i * destStride + ri43j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        // Solid fills
        dest[ri25i * destStride + ri25j] = encoder.Encode(px);
        dest[ri35i * destStride + ri35j] = encoder.Encode(px);
        dest[ri45i * destStride + ri45j] = encoder.Encode(px);
        dest[ri55i * destStride + ri55j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri54i * destStride + ri54j] = encoder.Encode(px);
        dest[ri52i * destStride + ri52j] = encoder.Encode(px);
        dest[ri53i * destStride + ri53j] = encoder.Encode(px);
      } else {
        // BlendLineShallow for 6x - from reference
        var (ri50i, ri50j) = RotationLookup.Get(6, rotDeg, 5, 0);
        var (ri42i, ri42j) = RotationLookup.Get(6, rotDeg, 4, 2);
        var (ri34i, ri34j) = RotationLookup.Get(6, rotDeg, 3, 4);
        var (ri51i, ri51j) = RotationLookup.Get(6, rotDeg, 5, 1);
        var (ri43i, ri43j) = RotationLookup.Get(6, rotDeg, 4, 3);
        var (ri35i, ri35j) = RotationLookup.Get(6, rotDeg, 3, 5);
        var (ri52i, ri52j) = RotationLookup.Get(6, rotDeg, 5, 2);
        var (ri53i, ri53j) = RotationLookup.Get(6, rotDeg, 5, 3);
        var (ri44i, ri44j) = RotationLookup.Get(6, rotDeg, 4, 4);

        // Gradient blends
        dest[ri50i * destStride + ri50j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri42i * destStride + ri42j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri34i * destStride + ri34j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri51i * destStride + ri51j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri43i * destStride + ri43j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri35i * destStride + ri35j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        // Solid fills
        dest[ri52i * destStride + ri52j] = encoder.Encode(px);
        dest[ri53i * destStride + ri53j] = encoder.Encode(px);
        dest[ri54i * destStride + ri54j] = encoder.Encode(px);
        dest[ri55i * destStride + ri55j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri45i * destStride + ri45j] = encoder.Encode(px);
      }
    } else {
      if (haveSteepLine) {
        // BlendLineSteep for 6x - from reference
        var (ri05i, ri05j) = RotationLookup.Get(6, rotDeg, 0, 5);
        var (ri24i, ri24j) = RotationLookup.Get(6, rotDeg, 2, 4);
        var (ri43i, ri43j) = RotationLookup.Get(6, rotDeg, 4, 3);
        var (ri15i, ri15j) = RotationLookup.Get(6, rotDeg, 1, 5);
        var (ri34i, ri34j) = RotationLookup.Get(6, rotDeg, 3, 4);
        var (ri53i, ri53j) = RotationLookup.Get(6, rotDeg, 5, 3);
        var (ri25i, ri25j) = RotationLookup.Get(6, rotDeg, 2, 5);
        var (ri35i, ri35j) = RotationLookup.Get(6, rotDeg, 3, 5);
        var (ri44i, ri44j) = RotationLookup.Get(6, rotDeg, 4, 4);

        // Gradient blends
        dest[ri05i * destStride + ri05j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri24i * destStride + ri24j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri43i * destStride + ri43j] = encoder.Encode(Blend(lerp, we, px, 3, 1));
        dest[ri15i * destStride + ri15j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri34i * destStride + ri34j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        dest[ri53i * destStride + ri53j] = encoder.Encode(Blend(lerp, we, px, 1, 3));
        // Solid fills
        dest[ri25i * destStride + ri25j] = encoder.Encode(px);
        dest[ri35i * destStride + ri35j] = encoder.Encode(px);
        dest[ri45i * destStride + ri45j] = encoder.Encode(px);
        dest[ri55i * destStride + ri55j] = encoder.Encode(px);
        dest[ri44i * destStride + ri44j] = encoder.Encode(px);
        dest[ri54i * destStride + ri54j] = encoder.Encode(px);
      } else {
        // BlendLineDiagonal for 6x - from reference
        var (ri53i, ri53j) = RotationLookup.Get(6, rotDeg, 5, 3);
        var (ri44i, ri44j) = RotationLookup.Get(6, rotDeg, 4, 4);
        var (ri35i, ri35j) = RotationLookup.Get(6, rotDeg, 3, 5);

        // Gradient blends (1/2 weight)
        dest[ri53i * destStride + ri53j] = encoder.Encode(Blend(lerp, we, px));
        dest[ri44i * destStride + ri44j] = encoder.Encode(Blend(lerp, we, px));
        dest[ri35i * destStride + ri35j] = encoder.Encode(Blend(lerp, we, px));
        // Solid fills
        dest[ri45i * destStride + ri45j] = encoder.Encode(px);
        dest[ri55i * destStride + ri55j] = encoder.Encode(px);
        dest[ri54i * destStride + ri54j] = encoder.Encode(px);
      }
    }
  }
}

#endregion
