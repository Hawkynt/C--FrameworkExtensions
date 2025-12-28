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

using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Metrics;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// HQ2x4 pattern lookup table implementation.
/// Applies one of 256 possible patterns based on neighbor pixel comparisons.
/// Produces a 2x4 output grid (8 pixels) for each input pixel.
/// </summary>
internal static class Hq2x4Patterns {
  /// <summary>
  /// Applies the HQ2x4 pattern for the given pattern byte.
  /// </summary>
  public static void Apply<TWork, TKey, TLerp, TEquality>(
    byte pattern,
    in TWork w0, in TWork w1, in TWork w2, in TWork w3, in TWork w4, in TWork w5, in TWork w6, in TWork w7, in TWork w8,
    in TKey c0, in TKey c1, in TKey c2, in TKey c3, in TKey c4, in TKey c5, in TKey c6, in TKey c7, in TKey c8,
    ref TWork e00, ref TWork e01,
    ref TWork e10, ref TWork e11,
    ref TWork e20, ref TWork e21,
    ref TWork e30, ref TWork e31,
    TLerp lerp, TEquality equality)
    where TWork : unmanaged
    where TKey : unmanaged
    where TLerp : struct, ILerp<TWork>
    where TEquality : struct, IColorEquality<TKey> {
    switch (pattern) {
      case 0:
      case 1:
      case 4:
      case 5:
      case 32:
      case 33:
      case 36:
      case 37:
      case 128:
      case 129:
      case 132:
      case 133:
      case 160:
      case 161:
      case 164:
      case 165:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 2:
      case 34:
      case 130:
      case 162:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 3:
      case 35:
      case 131:
      case 163:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 6:
      case 38:
      case 134:
      case 166:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 7:
      case 39:
      case 135:
      case 167:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 8:
      case 12:
      case 136:
      case 140:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 9:
      case 13:
      case 137:
      case 141:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 10:
      case 138:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 11:
      case 139:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 14:
      case 142:
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e01 = lerp.Lerp(w4, w5, 3, 1);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w1, w3, 9, 7);
          e01 = lerp.Lerp(w1, w4, 1, 1);
          e10 = lerp.Lerp(w4, w3, w1, 8, 5, 3);
        }
        break;
      case 15:
      case 143:
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e01 = lerp.Lerp(w4, w5, 3, 1);
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, 9, 7);
          e01 = lerp.Lerp(w1, w4, 1, 1);
          e10 = lerp.Lerp(w4, w3, w1, 8, 5, 3);
        }
        break;
      case 16:
      case 17:
      case 48:
      case 49:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 18:
      case 50:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 19:
      case 51:
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e00 = lerp.Lerp(w4, w3, 3, 1);
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e00 = lerp.Lerp(w1, w4, 1, 1);
          e01 = lerp.Lerp(w1, w5, 9, 7);
          e11 = lerp.Lerp(w4, w5, w1, 8, 5, 3);
        }
        break;
      case 20:
      case 21:
      case 52:
      case 53:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 22:
      case 54:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 23:
      case 55:
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e00 = lerp.Lerp(w4, w3, 3, 1);
          e01 = w4;
          e11 = w4;
        } else {
          e00 = lerp.Lerp(w1, w4, 1, 1);
          e01 = lerp.Lerp(w1, w5, 9, 7);
          e11 = lerp.Lerp(w4, w5, w1, 8, 5, 3);
        }
        break;
      case 24:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 25:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 26:
      case 31:
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 27:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 28:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 29:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 30:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 40:
      case 44:
      case 168:
      case 172:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 41:
      case 45:
      case 169:
      case 173:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        break;
      case 42:
      case 170:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
          e20 = lerp.Lerp(w4, w7, 7, 1);
          e30 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 4, 3, 1);
          e10 = lerp.Lerp(w3, w4, w1, 3, 3, 2);
          e20 = lerp.Lerp(w4, w3, w7, 9, 6, 1);
          e30 = lerp.Lerp(w4, w7, w3, 11, 3, 2);
        }
        break;
      case 43:
      case 171:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
          e20 = lerp.Lerp(w4, w7, 7, 1);
          e30 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 4, 3, 1);
          e10 = lerp.Lerp(w3, w4, w1, 3, 3, 2);
          e20 = lerp.Lerp(w4, w3, w7, 9, 6, 1);
          e30 = lerp.Lerp(w4, w7, w3, 11, 3, 2);
        }
        break;
      case 46:
      case 174:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        break;
      case 47:
      case 175:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = w4;
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w5, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, w5, 9, 4, 3);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        break;
      case 56:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 57:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 58:
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 59:
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 60:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 61:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        break;
      case 62:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 63:
        e10 = w4;
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w8, w7, 5, 2, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 64:
      case 65:
      case 68:
      case 69:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 66:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 67:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 70:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 71:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 72:
      case 76:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 73:
      case 77:
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e00 = lerp.Lerp(w4, w1, 5, 3);
          e10 = lerp.Lerp(w4, w1, 7, 1);
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 11, 3, 2);
          e10 = lerp.Lerp(w4, w3, w1, 9, 6, 1);
          e20 = lerp.Lerp(w3, w4, w7, 3, 3, 2);
          e30 = lerp.Lerp(w7, w3, w4, 4, 3, 1);
        }
        break;
      case 74:
      case 107:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 75:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 78:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        break;
      case 79:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 80:
      case 81:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 82:
      case 214:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 83:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 84:
      case 85:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e01 = lerp.Lerp(w4, w1, 5, 3);
          e11 = lerp.Lerp(w4, w1, 7, 1);
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 11, 3, 2);
          e11 = lerp.Lerp(w4, w5, w1, 9, 6, 1);
          e21 = lerp.Lerp(w4, w5, w7, 3, 3, 2);
          e31 = lerp.Lerp(w7, w5, w4, 4, 3, 1);
        }
        break;
      case 86:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 87:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 88:
      case 248:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 89:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        break;
      case 90:
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 91:
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 92:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        break;
      case 93:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        break;
      case 94:
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 95:
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 96:
      case 97:
      case 100:
      case 101:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 98:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 99:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 102:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 103:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        break;
      case 104:
      case 108:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 105:
      case 109:
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e00 = lerp.Lerp(w4, w1, 5, 3);
          e10 = lerp.Lerp(w4, w1, 7, 1);
          e20 = w4;
          e30 = w4;
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 11, 3, 2);
          e10 = lerp.Lerp(w4, w3, w1, 9, 6, 1);
          e20 = lerp.Lerp(w3, w4, w7, 3, 3, 2);
          e30 = lerp.Lerp(w7, w3, w4, 4, 3, 1);
        }
        break;
      case 106:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 110:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 111:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = w4;
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, w8, 6, 1, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        break;
      case 112:
      case 113:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e30 = lerp.Lerp(w4, w3, 3, 1);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, w7, 8, 5, 3);
          e30 = lerp.Lerp(w4, w7, 1, 1);
          e31 = lerp.Lerp(w7, w5, 9, 7);
        }
        break;
      case 114:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 115:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 116:
      case 117:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        break;
      case 118:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 119:
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c1, c5)) {
          e00 = lerp.Lerp(w4, w3, 3, 1);
          e01 = w4;
          e11 = w4;
        } else {
          e00 = lerp.Lerp(w1, w4, 1, 1);
          e01 = lerp.Lerp(w1, w5, 9, 7);
          e11 = lerp.Lerp(w4, w5, w1, 8, 5, 3);
        }
        break;
      case 120:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 121:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        break;
      case 122:
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = lerp.Lerp(w4, w8, 13, 3);
          e31 = lerp.Lerp(w4, w8, 11, 5);
        } else {
          e21 = lerp.Lerp(w4, w5, 7, 1);
          e31 = lerp.Lerp(w4, w7, w5, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 123:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 124:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        break;
      case 125:
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e00 = lerp.Lerp(w4, w1, 5, 3);
          e10 = lerp.Lerp(w4, w1, 7, 1);
          e20 = w4;
          e30 = w4;
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 11, 3, 2);
          e10 = lerp.Lerp(w4, w3, w1, 9, 6, 1);
          e20 = lerp.Lerp(w3, w4, w7, 3, 3, 2);
          e30 = lerp.Lerp(w7, w3, w4, 4, 3, 1);
        }
        break;
      case 126:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 127:
        e10 = w4;
        e21 = lerp.Lerp(w4, w8, 13, 3);
        e31 = lerp.Lerp(w4, w8, 11, 5);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 144:
      case 145:
      case 176:
      case 177:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 146:
      case 178:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
          e21 = lerp.Lerp(w4, w7, 7, 1);
          e31 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e01 = lerp.Lerp(w1, w5, w4, 4, 3, 1);
          e11 = lerp.Lerp(w4, w5, w1, 3, 3, 2);
          e21 = lerp.Lerp(w4, w5, w7, 9, 6, 1);
          e31 = lerp.Lerp(w4, w7, w5, 11, 3, 2);
        }
        break;
      case 147:
      case 179:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 148:
      case 149:
      case 180:
      case 181:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 150:
      case 182:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
          e21 = lerp.Lerp(w4, w7, 7, 1);
          e31 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e01 = lerp.Lerp(w1, w5, w4, 4, 3, 1);
          e11 = lerp.Lerp(w4, w5, w1, 3, 3, 2);
          e21 = lerp.Lerp(w4, w5, w7, 9, 6, 1);
          e31 = lerp.Lerp(w4, w7, w5, 11, 3, 2);
        }
        break;
      case 151:
      case 183:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = w4;
        e20 = lerp.Lerp(w4, w3, w7, 11, 3, 2);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, w3, 9, 4, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 152:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 153:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 154:
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 155:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 156:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 157:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 158:
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 159:
        e11 = w4;
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w6, w7, 5, 2, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 184:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 185:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 186:
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 187:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
          e20 = lerp.Lerp(w4, w7, 7, 1);
          e30 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 4, 3, 1);
          e10 = lerp.Lerp(w3, w4, w1, 3, 3, 2);
          e20 = lerp.Lerp(w4, w3, w7, 9, 6, 1);
          e30 = lerp.Lerp(w4, w7, w3, 11, 3, 2);
        }
        break;
      case 188:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 189:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        break;
      case 190:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
          e21 = lerp.Lerp(w4, w7, 7, 1);
          e31 = lerp.Lerp(w4, w7, 5, 3);
        } else {
          e01 = lerp.Lerp(w1, w5, w4, 4, 3, 1);
          e11 = lerp.Lerp(w4, w5, w1, 3, 3, 2);
          e21 = lerp.Lerp(w4, w5, w7, 9, 6, 1);
          e31 = lerp.Lerp(w4, w7, w5, 11, 3, 2);
        }
        break;
      case 191:
        e10 = w4;
        e11 = w4;
        e20 = lerp.Lerp(w4, w7, 7, 1);
        e21 = lerp.Lerp(w4, w7, 7, 1);
        e30 = lerp.Lerp(w4, w7, 5, 3);
        e31 = lerp.Lerp(w4, w7, 5, 3);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 192:
      case 193:
      case 196:
      case 197:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 194:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 195:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 198:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 199:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 200:
      case 204:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
          e31 = lerp.Lerp(w4, w5, 3, 1);
        } else {
          e20 = lerp.Lerp(w4, w3, w7, 8, 5, 3);
          e30 = lerp.Lerp(w7, w3, 9, 7);
          e31 = lerp.Lerp(w4, w7, 1, 1);
        }
        break;
      case 201:
      case 205:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        break;
      case 202:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        break;
      case 203:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 206:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        break;
      case 207:
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e01 = lerp.Lerp(w4, w5, 3, 1);
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, 9, 7);
          e01 = lerp.Lerp(w1, w4, 1, 1);
          e10 = lerp.Lerp(w4, w3, w1, 8, 5, 3);
        }
        break;
      case 208:
      case 209:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 210:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 211:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 212:
      case 213:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e01 = lerp.Lerp(w4, w1, 5, 3);
          e11 = lerp.Lerp(w4, w1, 7, 1);
          e21 = w4;
          e31 = w4;
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 11, 3, 2);
          e11 = lerp.Lerp(w4, w5, w1, 9, 6, 1);
          e21 = lerp.Lerp(w4, w5, w7, 3, 3, 2);
          e31 = lerp.Lerp(w7, w5, w4, 4, 3, 1);
        }
        break;
      case 215:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = w4;
        e20 = lerp.Lerp(w4, w3, w6, 6, 1, 1);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 216:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 217:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 218:
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 219:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 220:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = lerp.Lerp(w4, w6, 13, 3);
          e30 = lerp.Lerp(w4, w6, 11, 5);
        } else {
          e20 = lerp.Lerp(w4, w3, 7, 1);
          e30 = lerp.Lerp(w4, w7, w3, 5, 2, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 221:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e01 = lerp.Lerp(w4, w1, 5, 3);
          e11 = lerp.Lerp(w4, w1, 7, 1);
          e21 = w4;
          e31 = w4;
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 11, 3, 2);
          e11 = lerp.Lerp(w4, w5, w1, 9, 6, 1);
          e21 = lerp.Lerp(w4, w5, w7, 3, 3, 2);
          e31 = lerp.Lerp(w7, w5, w4, 4, 3, 1);
        }
        break;
      case 222:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 223:
        e11 = w4;
        e20 = lerp.Lerp(w4, w6, 13, 3);
        e30 = lerp.Lerp(w4, w6, 11, 5);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 224:
      case 225:
      case 228:
      case 229:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 226:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 227:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 230:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 231:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        break;
      case 232:
      case 236:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
          e31 = lerp.Lerp(w4, w5, 3, 1);
        } else {
          e20 = lerp.Lerp(w4, w3, w7, 8, 5, 3);
          e30 = lerp.Lerp(w7, w3, 9, 7);
          e31 = lerp.Lerp(w4, w7, 1, 1);
        }
        break;
      case 233:
      case 237:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, w5, 9, 4, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w5, w1, 11, 3, 2);
        e20 = w4;
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        break;
      case 234:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = lerp.Lerp(w4, w0, 11, 5);
          e10 = lerp.Lerp(w4, w0, 13, 3);
        } else {
          e00 = lerp.Lerp(w4, w1, w3, 5, 2, 1);
          e10 = lerp.Lerp(w4, w3, 7, 1);
        }
        break;
      case 235:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, w5, 6, 1, 1);
        e20 = w4;
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 238:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e21 = lerp.Lerp(w4, w5, 3, 1);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
          e31 = lerp.Lerp(w4, w5, 3, 1);
        } else {
          e20 = lerp.Lerp(w4, w3, w7, 8, 5, 3);
          e30 = lerp.Lerp(w7, w3, 9, 7);
          e31 = lerp.Lerp(w4, w7, 1, 1);
        }
        break;
      case 239:
        e01 = lerp.Lerp(w4, w5, 3, 1);
        e10 = w4;
        e11 = lerp.Lerp(w4, w5, 3, 1);
        e20 = w4;
        e21 = lerp.Lerp(w4, w5, 3, 1);
        e31 = lerp.Lerp(w4, w5, 3, 1);
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        break;
      case 240:
      case 241:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e30 = lerp.Lerp(w4, w3, 3, 1);
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, w7, 8, 5, 3);
          e30 = lerp.Lerp(w4, w7, 1, 1);
          e31 = lerp.Lerp(w7, w5, 9, 7);
        }
        break;
      case 242:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e30 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c5)) {
          e01 = lerp.Lerp(w4, w2, 11, 5);
          e11 = lerp.Lerp(w4, w2, 13, 3);
        } else {
          e01 = lerp.Lerp(w4, w1, w5, 5, 2, 1);
          e11 = lerp.Lerp(w4, w5, 7, 1);
        }
        break;
      case 243:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e30 = lerp.Lerp(w4, w3, 3, 1);
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, w7, 8, 5, 3);
          e30 = lerp.Lerp(w4, w7, 1, 1);
          e31 = lerp.Lerp(w7, w5, 9, 7);
        }
        break;
      case 244:
      case 245:
        e00 = lerp.Lerp(w4, w1, w3, 9, 4, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w3, w1, 11, 3, 2);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = w4;
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        break;
      case 246:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, w3, 6, 1, 1);
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = w4;
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 247:
        e00 = lerp.Lerp(w4, w3, 3, 1);
        e10 = lerp.Lerp(w4, w3, 3, 1);
        e11 = w4;
        e20 = lerp.Lerp(w4, w3, 3, 1);
        e21 = w4;
        e30 = lerp.Lerp(w4, w3, 3, 1);
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
      case 249:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w2, w1, 5, 2, 1);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = w4;
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 250:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        break;
      case 251:
        e01 = lerp.Lerp(w4, w2, 11, 5);
        e11 = lerp.Lerp(w4, w2, 13, 3);
        e20 = w4;
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        if (!equality.Equals(c7, c5)) {
          e21 = w4;
          e31 = w4;
        } else {
          e21 = lerp.Lerp(w4, w5, 3, 1);
          e31 = lerp.Lerp(w7, w4, w5, 2, 1, 1);
        }
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e10 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 2, 1, 1);
          e10 = lerp.Lerp(w4, w3, 3, 1);
        }
        break;
      case 252:
        e00 = lerp.Lerp(w4, w0, w1, 5, 2, 1);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e21 = w4;
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        break;
      case 253:
        e00 = lerp.Lerp(w4, w1, 5, 3);
        e01 = lerp.Lerp(w4, w1, 5, 3);
        e10 = lerp.Lerp(w4, w1, 7, 1);
        e11 = lerp.Lerp(w4, w1, 7, 1);
        e20 = w4;
        e21 = w4;
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        break;
      case 254:
        e00 = lerp.Lerp(w4, w0, 11, 5);
        e10 = lerp.Lerp(w4, w0, 13, 3);
        e21 = w4;
        if (!equality.Equals(c7, c3)) {
          e20 = w4;
          e30 = w4;
        } else {
          e20 = lerp.Lerp(w4, w3, 3, 1);
          e30 = lerp.Lerp(w7, w3, w4, 2, 1, 1);
        }
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        if (!equality.Equals(c1, c5)) {
          e01 = w4;
          e11 = w4;
        } else {
          e01 = lerp.Lerp(w1, w4, w5, 2, 1, 1);
          e11 = lerp.Lerp(w4, w5, 3, 1);
        }
        break;
      case 255:
        e10 = w4;
        e11 = w4;
        e20 = w4;
        e21 = w4;
        e30 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        e31 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 6, 1, 1);
        break;
    }
  }
}
