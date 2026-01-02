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
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Internal;

/// <summary>
/// Internal helper for 16.16 fixed-point math operations in color space conversions.
/// </summary>
/// <remarks>
/// Format: 32-bit signed integer where upper 16 bits = integer, lower 16 bits = fraction.
/// 1.0 = 65536 (0x10000), Range: -32768.0 to +32767.99998, Precision: ~0.000015
/// </remarks>
internal static class FixedPointMath {

  #region Core Constants

  /// <summary>1.0 in 16.16 fixed-point</summary>
  public const int One = 65536;

  /// <summary>0.5 in 16.16 fixed-point</summary>
  public const int Half = 32768;

  /// <summary>1/3 in 16.16 fixed-point</summary>
  public const int OneThird = 21845;

  /// <summary>2/3 in 16.16 fixed-point</summary>
  public const int TwoThirds = 43691;

  /// <summary>1/6 in 16.16 fixed-point</summary>
  public const int OneSixth = 10923;

  /// <summary>Multiplier for byte to 16.16: 65536/255 ≈ 257.003</summary>
  private const int ByteToFixedMultiplier = 257;

  #endregion

  #region YCbCr Constants (ITU-R BT.601)

  // RGB to Y: 0.299*R + 0.587*G + 0.114*B
  public const int YCbCr_Y_R = 19595;   // 0.299 * 65536
  public const int YCbCr_Y_G = 38470;   // 0.587 * 65536
  public const int YCbCr_Y_B = 7471;    // 0.114 * 65536 (adjusted so sum = 65536)

  // RGB to Cb: -0.168736*R - 0.331264*G + 0.5*B
  public const int YCbCr_Cb_R = -11058; // -0.168736 * 65536
  public const int YCbCr_Cb_G = -21710; // -0.331264 * 65536
  public const int YCbCr_Cb_B = 32768;  // 0.5 * 65536

  // RGB to Cr: 0.5*R - 0.418688*G - 0.081312*B
  public const int YCbCr_Cr_R = 32768;  // 0.5 * 65536
  public const int YCbCr_Cr_G = -27439; // -0.418688 * 65536
  public const int YCbCr_Cr_B = -5329;  // -0.081312 * 65536

  // YCbCr to RGB
  public const int YCbCr_R_Cr = 91881;  // 1.402 * 65536
  public const int YCbCr_G_Cb = -22554; // -0.344136 * 65536
  public const int YCbCr_G_Cr = -46802; // -0.714136 * 65536
  public const int YCbCr_B_Cb = 116130; // 1.772 * 65536

  #endregion

  #region XYZ Matrix Constants (sRGB, D65 illuminant)

  // sRGB to XYZ
  public const int Xyz_XR = 27030;  // 0.4124564 * 65536
  public const int Xyz_XG = 23434;  // 0.3575761 * 65536
  public const int Xyz_XB = 11833;  // 0.1804375 * 65536
  public const int Xyz_YR = 13933;  // 0.2126729 * 65536
  public const int Xyz_YG = 46871;  // 0.7151522 * 65536
  public const int Xyz_YB = 4732;   // 0.0721750 * 65536
  public const int Xyz_ZR = 1267;   // 0.0193339 * 65536
  public const int Xyz_ZG = 7811;   // 0.1191920 * 65536
  public const int Xyz_ZB = 62284;  // 0.9503041 * 65536

  // XYZ to sRGB
  public const int Rgb_RX = 212574;  // 3.2404542 * 65536
  public const int Rgb_RY = -100752; // -1.5371385 * 65536
  public const int Rgb_RZ = -32674;  // -0.4985314 * 65536
  public const int Rgb_GX = -63541;  // -0.9692660 * 65536
  public const int Rgb_GY = 122972;  // 1.8760108 * 65536
  public const int Rgb_GZ = 2723;    // 0.0415560 * 65536
  public const int Rgb_BX = 3646;    // 0.0556434 * 65536
  public const int Rgb_BY = -13383;  // -0.2040259 * 65536
  public const int Rgb_BZ = 69295;   // 1.0572252 * 65536

  // D65 reference white point (16.16 format, but scaled for typical usage)
  public const int D65_Xn = 62327;  // 0.95047 * 65536
  public const int D65_Yn = 65536;  // 1.00000 * 65536
  public const int D65_Zn = 71362;  // 1.08883 * 65536

  #endregion

  #region Lab Constants

  // CIE Lab constants in 16.16
  public const int Lab_Epsilon = 580;     // 0.008856 * 65536 = (6/29)^3
  public const int Lab_Kappa = 59204813;  // 903.3 * 65536 = (29/3)^3
  public const int Lab_116 = 7602176;     // 116 * 65536
  public const int Lab_16 = 1048576;      // 16 * 65536
  public const int Lab_500 = 32768000;    // 500 * 65536
  public const int Lab_200 = 13107200;    // 200 * 65536

  #endregion

  #region Branchless Integer Operations

  /// <summary>
  /// Branchless absolute difference |a - b| for integers.
  /// Uses Hacker's Delight sign-bit manipulation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int BranchlessAbsDiff(int a, int b) {
    var diff = a - b;
    var mask = diff >> 31; // All 1s if negative, all 0s if positive
    return (diff ^ mask) - mask;
  }

  /// <summary>
  /// Branchless minimum for integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int BranchlessMin(int a, int b) {
    var diff = a - b;
    return b + (diff & (diff >> 31));
  }

  /// <summary>
  /// Branchless maximum for integers.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int BranchlessMax(int a, int b) {
    var diff = a - b;
    return a - (diff & (diff >> 31));
  }

  /// <summary>
  /// Branchless saturating clamp to 0-255 range.
  /// Uses Hacker's Delight overflow detection.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int SaturateToU8(int v) {
    // If v > 255, set all bits high then mask to 255
    // If v < 0, mask becomes 0
    var overflow = (255 - v) >> 31; // -1 if v > 255, 0 otherwise
    var underflow = v >> 31;        // -1 if v < 0, 0 otherwise
    return (v | overflow) & ~underflow & 255;
  }

  /// <summary>
  /// Branchless saturating clamp to 0-65535 range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int SaturateToU16(int v) {
    var overflow = (65535 - v) >> 31;
    var underflow = v >> 31;
    return (v | overflow) & ~underflow & 65535;
  }

  /// <summary>
  /// Fast division by 255 using multiply+shift (Hacker's Delight).
  /// Exact for inputs 0-65535.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int DivBy255(int v)
    => (v * 0x8081) >> 23;

  /// <summary>
  /// Fast division by 255 with rounding.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int DivBy255Rounded(int v)
    => ((v * 257) + 257) >> 16;

  #endregion

  #region Conversion Methods

  /// <summary>
  /// Converts a byte (0-255) to 16.16 fixed-point normalized (0-65536).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int ByteToFixed(int v) => v * ByteToFixedMultiplier;

  /// <summary>
  /// Converts 16.16 fixed-point normalized (0-65536) to byte (0-255) with rounding.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte FixedToByte(int v) {
    // Clamp and convert: (v * 255 + 32768) >> 16
    if (v <= 0)
      return 0;
    if (v >= One)
      return 255;
    return (byte)((v * 255 + Half) >> 16);
  }

  /// <summary>
  /// Converts 16.16 fixed-point to byte with clamping (for values that may exceed 0-1 range).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte FixedToByteClamp(int v) {
    if (v < 0)
      v = 0;
    else if (v > One)
      v = One;
    return (byte)((v * 255 + Half) >> 16);
  }

  /// <summary>
  /// Clamps a 16.16 value to 0-65536 range.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Clamp(int v) => v < 0 ? 0 : v > One ? One : v;

  /// <summary>
  /// Multiplies two 16.16 fixed-point values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Mul(int a, int b) => (int)(((long)a * b) >> 16);

  /// <summary>
  /// Divides two 16.16 fixed-point values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int Div(int a, int b) => b == 0 ? 0 : (int)(((long)a << 16) / b);

  #endregion

  #region Gamma LUTs

  private static int[]? _gammaExpansionLut;
  private static byte[]? _gammaCompressionLut;

  /// <summary>
  /// Gets the gamma expansion lookup table (sRGB byte to linear 16.16).
  /// </summary>
  public static int[] GammaExpansionLut => _gammaExpansionLut ??= _BuildGammaExpansionLut();

  /// <summary>
  /// Gets the gamma compression lookup table (linear 16.16 >> 8 index to sRGB byte).
  /// </summary>
  public static byte[] GammaCompressionLut => _gammaCompressionLut ??= _BuildGammaCompressionLut();

  private static int[] _BuildGammaExpansionLut() {
    var lut = new int[256];
    for (var i = 0; i < 256; ++i) {
      var v = i / 255.0;
      var linear = v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
      lut[i] = (int)(linear * 65536.0 + 0.5);
    }
    return lut;
  }

  private static byte[] _BuildGammaCompressionLut() {
    // 257 entries for indices 0-256 (linear value >> 8)
    var lut = new byte[257];
    for (var i = 0; i <= 256; ++i) {
      var linear = i / 256.0;
      var v = linear <= 0.0031308 ? linear * 12.92 : 1.055 * Math.Pow(linear, 1.0 / 2.4) - 0.055;
      lut[i] = (byte)Math.Min(255, Math.Max(0, (int)(v * 255.0 + 0.5)));
    }
    return lut;
  }

  /// <summary>
  /// Applies gamma expansion using LUT (sRGB byte to linear 16.16).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GammaExpand(byte v) => GammaExpansionLut[v];

  /// <summary>
  /// Applies gamma compression using LUT (linear 16.16 to sRGB byte).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static byte GammaCompress(int v) {
    if (v <= 0)
      return 0;
    if (v >= One)
      return 255;
    return GammaCompressionLut[v >> 8];
  }

  #endregion

  #region Lab F-function LUTs

  private static int[]? _labFLut;
  private static int[]? _labFInverseLut;

  /// <summary>
  /// Gets the Lab f() function lookup table.
  /// </summary>
  public static int[] LabFLut => _labFLut ??= _BuildLabFLut();

  /// <summary>
  /// Gets the Lab f^-1() function lookup table.
  /// </summary>
  public static int[] LabFInverseLut => _labFInverseLut ??= _BuildLabFInverseLut();

  private static int[] _BuildLabFLut() {
    // 257 entries for input values 0-256 (representing 0.0-1.0 normalized XYZ/reference)
    var lut = new int[257];
    const double epsilon = 0.008856;  // (6/29)^3
    const double kappa = 903.3;       // (29/3)^3

    for (var i = 0; i <= 256; ++i) {
      var t = i / 256.0;
      var f = t > epsilon ? Math.Pow(t, 1.0 / 3.0) : (kappa * t + 16.0) / 116.0;
      lut[i] = (int)(f * 65536.0 + 0.5);
    }
    return lut;
  }

  private static int[] _BuildLabFInverseLut() {
    // 257 entries for input f values (approximately 0.0-1.0 range)
    var lut = new int[257];
    const double epsilon = 0.008856;

    for (var i = 0; i <= 256; ++i) {
      var f = i / 256.0;
      var t3 = f * f * f;
      var t = t3 > epsilon ? t3 : (116.0 * f - 16.0) / 903.3;
      lut[i] = (int)(Math.Max(0, Math.Min(1, t)) * 65536.0 + 0.5);
    }
    return lut;
  }

  /// <summary>
  /// Applies Lab f() function using LUT.
  /// Input: 16.16 fixed-point (0-65536 for 0.0-1.0)
  /// Output: 16.16 fixed-point
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LabF(int v) {
    if (v <= 0)
      return LabFLut[0];
    if (v >= One)
      return LabFLut[256];
    return LabFLut[v >> 8];
  }

  /// <summary>
  /// Applies Lab f^-1() function using LUT.
  /// Input: 16.16 fixed-point
  /// Output: 16.16 fixed-point (0-65536 for 0.0-1.0)
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LabFInverse(int v) {
    if (v <= 0)
      return LabFInverseLut[0];
    if (v >= One)
      return LabFInverseLut[256];
    return LabFInverseLut[v >> 8];
  }

  #endregion

  #region Cube Root LUT (for Oklab)

  private static float[]? _cbrtLut;

  /// <summary>
  /// Gets the cube root lookup table (257 entries for 0.0-1.0 range with interpolation).
  /// </summary>
  public static float[] CbrtLut => _cbrtLut ??= _BuildCbrtLut();

  private static float[] _BuildCbrtLut() {
    var lut = new float[257];
    for (var i = 0; i <= 256; ++i)
      lut[i] = MathF.Cbrt(i / 256f);
    return lut;
  }

  /// <summary>
  /// Fast cube root using LUT with linear interpolation.
  /// Optimized for 0-1 range, falls back to MathF.Cbrt for out-of-range values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float FastCbrt(float x) {
    if (x <= 0f) return 0f;
    if (x >= 1f) return x > 1f ? MathF.Cbrt(x) : 1f; // cbrt(1)=1 fast-path, fallback for out-of-gamut
    var idx = x * 256f;
    var i = (int)idx;
    var frac = idx - i;
    var lut = CbrtLut;
    return lut[i] + (lut[i + 1] - lut[i]) * frac;
  }

  #endregion

  #region PQ Transfer LUTs (for JzAzBz)

  // PQ constants
  private const float PQ_C1 = 0.8359375f;         // 3424/4096
  private const float PQ_C2 = 18.8515625f;        // 2413/128
  private const float PQ_C3 = 18.6875f;           // 2392/128
  private const float PQ_N = 0.15930175781f;      // 2610/16384
  private const float PQ_P = 134.034375f;         // 1.7*2523/32

  private static float[]? _pqForwardLut;
  private static float[]? _pqInverseLut;

  /// <summary>
  /// Gets the PQ forward transfer lookup table.
  /// </summary>
  public static float[] PqForwardLut => _pqForwardLut ??= _BuildPqForwardLut();

  /// <summary>
  /// Gets the PQ inverse transfer lookup table.
  /// </summary>
  public static float[] PqInverseLut => _pqInverseLut ??= _BuildPqInverseLut();

  private static float[] _BuildPqForwardLut() {
    // 4097 entries for input range 0-1 (normalized LMS) - high resolution for better precision
    var lut = new float[4097];
    for (var i = 0; i <= 4096; ++i) {
      var x = i / 4096f;
      if (x <= 0f) {
        lut[i] = 0f;
        continue;
      }
      var xn = MathF.Pow(x, PQ_N);
      lut[i] = MathF.Pow((PQ_C1 + PQ_C2 * xn) / (1f + PQ_C3 * xn), PQ_P);
    }
    return lut;
  }

  private static float[] _BuildPqInverseLut() {
    // 4097 entries for input range 0-1 (PQ-encoded values) - high resolution for better precision
    var lut = new float[4097];
    for (var i = 0; i <= 4096; ++i) {
      var x = i / 4096f;
      if (x <= 0f) {
        lut[i] = 0f;
        continue;
      }
      var xp = MathF.Pow(x, 1f / PQ_P);
      var num = PQ_C1 - xp;
      var den = PQ_C3 * xp - PQ_C2;
      if (den >= 0f) {
        lut[i] = 0f;
        continue;
      }
      lut[i] = MathF.Pow(num / den, 1f / PQ_N);
    }
    return lut;
  }

  /// <summary>
  /// Fast PQ forward transfer using LUT with linear interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float FastPqForward(float x) {
    if (x <= 0f) return 0f;
    if (x >= 1f) return PqForwardLut[4096];
    var idx = x * 4096f;
    var i = (int)idx;
    var frac = idx - i;
    var lut = PqForwardLut;
    return lut[i] + (lut[i + 1] - lut[i]) * frac;
  }

  /// <summary>
  /// Fast PQ inverse transfer using LUT with linear interpolation.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float FastPqInverse(float x) {
    if (x <= 0f) return 0f;
    if (x >= 1f) return PqInverseLut[4096];
    var idx = x * 4096f;
    var i = (int)idx;
    var frac = idx - i;
    var lut = PqInverseLut;
    return lut[i] + (lut[i + 1] - lut[i]) * frac;
  }

  #endregion

}
