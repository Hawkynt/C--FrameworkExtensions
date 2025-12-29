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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Pipeline;
using Hawkynt.ColorProcessing.Storage;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Hyllian's Reverse Anti-Alias filter.
/// </summary>
/// <remarks>
/// <para>Scales images by 2x using reverse anti-aliasing technique.</para>
/// <para>
/// Uses a 5-point cross pattern (extended by 2 pixels in each cardinal direction)
/// to compute tilt values based on neighboring pixel gradients.
/// The tilt is clamped based on pixel values and applied to create smooth edges.
/// </para>
/// <para>By Christoph Feck (christoph@maxiom.de) / Hyllian.</para>
/// </remarks>
[ScalerInfo("Reverse Anti-Alias", Author = "Christoph Feck / Hyllian", Year = 2011,
  Description = "Reverse anti-aliasing for smooth edge detection", Category = ScalerCategory.PixelArt)]
public readonly struct ReverseAa : IPixelScaler {

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
    where TEncode : struct, IEncode<TWork, TPixel> {
    if (typeof(TPixel) != typeof(Bgra8888))
      throw new NotSupportedException($"{nameof(ReverseAa)} requires TPixel to be {nameof(Bgra8888)}, but got {typeof(TPixel).Name}");

    return callback.Invoke(new ReverseAaKernel<TWork, TKey, TPixel, TEncode>());
  }

  /// <summary>
  /// Gets the list of scale factors supported.
  /// </summary>
  public static ScaleFactor[] SupportedScales { get; } = [new(2, 2)];

  /// <summary>
  /// Determines whether the specified scale factor is supported.
  /// </summary>
  public static bool SupportsScale(ScaleFactor scale) => scale is { X: 2, Y: 2 };

  /// <summary>
  /// Enumerates all possible target dimensions.
  /// </summary>
  public static IEnumerable<(int Width, int Height)> GetPossibleTargets(int sourceWidth, int sourceHeight) {
    yield return (sourceWidth * 2, sourceHeight * 2);
  }

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static ReverseAa Default => new();
}

file readonly struct ReverseAaKernel<TWork, TKey, TPixel, TEncode>
  : IScaler<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int ScaleX => 2;
  public int ScaleY => 2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Scale(
    in NeighborWindow<TWork, TKey> window,
    TPixel* destTopLeft,
    int destStride,
    in TEncode encoder
  ) {
    // Extended cross pattern (needs -2 to +2 in each direction)
    // Encode all neighbors to TPixel, then reinterpret as Bgra8888
    var b1Enc = encoder.Encode(window.P0M2.Work); // 2 above
    var bEnc = encoder.Encode(window.P0M1.Work);  // 1 above
    var dEnc = encoder.Encode(window.M1P0.Work);  // 1 left
    var eEnc = encoder.Encode(window.P0P0.Work);  // center
    var fEnc = encoder.Encode(window.P1P0.Work);  // 1 right
    var hEnc = encoder.Encode(window.P0P1.Work);  // 1 below
    var h5Enc = encoder.Encode(window.P0P2.Work); // 2 below
    var d0Enc = encoder.Encode(window.M2P0.Work); // 2 left
    var f4Enc = encoder.Encode(window.P2P0.Work); // 2 right

    // Reinterpret as Bgra8888 and get normalized float components
    ref readonly var b1 = ref Unsafe.As<TPixel, Bgra8888>(ref b1Enc);
    ref readonly var b = ref Unsafe.As<TPixel, Bgra8888>(ref bEnc);
    ref readonly var d = ref Unsafe.As<TPixel, Bgra8888>(ref dEnc);
    ref readonly var e = ref Unsafe.As<TPixel, Bgra8888>(ref eEnc);
    ref readonly var f = ref Unsafe.As<TPixel, Bgra8888>(ref fEnc);
    ref readonly var h = ref Unsafe.As<TPixel, Bgra8888>(ref hEnc);
    ref readonly var h5 = ref Unsafe.As<TPixel, Bgra8888>(ref h5Enc);
    ref readonly var d0 = ref Unsafe.As<TPixel, Bgra8888>(ref d0Enc);
    ref readonly var f4 = ref Unsafe.As<TPixel, Bgra8888>(ref f4Enc);

    // Process each channel using normalized floats (0-1 range)
    var redPart = ReverseAaChannel(
      b1.RNormalized, b.RNormalized, d.RNormalized, e.RNormalized, f.RNormalized,
      h.RNormalized, h5.RNormalized, d0.RNormalized, f4.RNormalized);
    var greenPart = ReverseAaChannel(
      b1.GNormalized, b.GNormalized, d.GNormalized, e.GNormalized, f.GNormalized,
      h.GNormalized, h5.GNormalized, d0.GNormalized, f4.GNormalized);
    var bluePart = ReverseAaChannel(
      b1.BNormalized, b.BNormalized, d.BNormalized, e.BNormalized, f.BNormalized,
      h.BNormalized, h5.BNormalized, d0.BNormalized, f4.BNormalized);
    var alphaPart = ReverseAaChannel(
      b1.ANormalized, b.ANormalized, d.ANormalized, e.ANormalized, f.ANormalized,
      h.ANormalized, h5.ANormalized, d0.ANormalized, f4.ANormalized);

    // Compose output pixels from per-channel results (convert back to bytes)
    var res0 = new Bgra8888(
      Bgra8888.ClampToByte(redPart.e0 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(greenPart.e0 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(bluePart.e0 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(alphaPart.e0 * Bgra8888.NormalizedToByte));
    var res1 = new Bgra8888(
      Bgra8888.ClampToByte(redPart.e1 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(greenPart.e1 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(bluePart.e1 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(alphaPart.e1 * Bgra8888.NormalizedToByte));
    var res2 = new Bgra8888(
      Bgra8888.ClampToByte(redPart.e2 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(greenPart.e2 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(bluePart.e2 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(alphaPart.e2 * Bgra8888.NormalizedToByte));
    var res3 = new Bgra8888(
      Bgra8888.ClampToByte(redPart.e3 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(greenPart.e3 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(bluePart.e3 * Bgra8888.NormalizedToByte),
      Bgra8888.ClampToByte(alphaPart.e3 * Bgra8888.NormalizedToByte));

    // Write 2x2 output (Unsafe.As back to TPixel)
    var row0 = destTopLeft;
    var row1 = destTopLeft + destStride;

    row0[0] = Unsafe.As<Bgra8888, TPixel>(ref res0);
    row0[1] = Unsafe.As<Bgra8888, TPixel>(ref res1);
    row1[0] = Unsafe.As<Bgra8888, TPixel>(ref res2);
    row1[1] = Unsafe.As<Bgra8888, TPixel>(ref res3);
  }

  /// <summary>
  /// Computes reverse anti-aliasing for a single channel.
  /// Algorithm by Christoph Feck / Hyllian.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static (float e0, float e1, float e2, float e3) ReverseAaChannel(
    float b1, float b, float d, float e, float f, float h, float h5, float d0, float f4) {

    // Vertical pass: compute s0 and s1 from vertical neighbors
    var n1 = b1;
    var n2 = b;
    var s = e;
    var n3 = h;
    var n4 = h5;

    var aa = n2 - n1;
    var bb = s - n2;
    var cc = n3 - s;
    var dd = n4 - n3;

    var tilt = (7f * (bb + cc) - 3f * (aa + dd)) / 16f;

    // Clamp based on center value and neighbor differences
    var m = s < 0.5f ? 2f * s : 2f * (1f - s);
    m = Math.Min(m, 2f * Math.Abs(bb));
    m = Math.Min(m, 2f * Math.Abs(cc));
    tilt = Math.Clamp(tilt, -m, m);

    var s1 = s + tilt / 2f;
    var s0 = s1 - tilt;

    // Horizontal pass for s0
    n1 = d0;
    n2 = d;
    s = s0;
    n3 = f;
    n4 = f4;

    aa = n2 - n1;
    bb = s - n2;
    cc = n3 - s;
    dd = n4 - n3;

    tilt = (7f * (bb + cc) - 3f * (aa + dd)) / 16f;

    m = s < 0.5f ? 2f * s : 2f * (1f - s);
    m = Math.Min(m, 2f * Math.Abs(bb));
    m = Math.Min(m, 2f * Math.Abs(cc));
    tilt = Math.Clamp(tilt, -m, m);

    var resE1 = s + tilt / 2f;
    var resE0 = resE1 - tilt;

    // Horizontal pass for s1
    s = s1;
    bb = s - n2;
    cc = n3 - s;

    tilt = (7f * (bb + cc) - 3f * (aa + dd)) / 16f;

    m = s < 0.5f ? 2f * s : 2f * (1f - s);
    m = Math.Min(m, 2f * Math.Abs(bb));
    m = Math.Min(m, 2f * Math.Abs(cc));
    tilt = Math.Clamp(tilt, -m, m);

    var resE3 = s + tilt / 2f;
    var resE2 = resE3 - tilt;

    return (resE0, resE1, resE2, resE3);
  }
}
