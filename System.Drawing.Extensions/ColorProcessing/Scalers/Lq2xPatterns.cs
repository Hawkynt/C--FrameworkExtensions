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
/// LQ2x pattern lookup table implementation.
/// Applies one of 256 possible patterns based on neighbor pixel comparisons.
/// Uses simpler interpolation than HQ2x for faster processing.
/// </summary>
internal static class Lq2xPatterns {
  /// <summary>
  /// Applies the LQ2x pattern for the given pattern byte.
  /// </summary>
  public static void Apply<TWork, TKey, TLerp, TEquality>(
    byte pattern,
    in TWork w0, in TWork w1, in TWork w2, in TWork w3, in TWork w4, in TWork w5, in TWork w6, in TWork w7, in TWork w8,
    in TKey c0, in TKey c1, in TKey c2, in TKey c3, in TKey c4, in TKey c5, in TKey c6, in TKey c7, in TKey c8,
    ref TWork e00, ref TWork e01,
    ref TWork e10, ref TWork e11,
    TLerp lerp, TEquality equality)
    where TWork : unmanaged
    where TKey : unmanaged
    where TLerp : struct, ILerp<TWork>
    where TEquality : struct, IColorEquality<TKey> {
    switch (pattern) {
      // Cases 0-63
      case 0:
      case 2:
      case 4:
      case 6:
      case 8:
      case 12:
      case 16:
      case 20:
      case 24:
      case 28:
      case 32:
      case 34:
      case 36:
      case 38:
      case 40:
      case 44:
      case 48:
      case 52:
      case 56:
      case 60:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = w0;
        break;
      case 1:
      case 5:
      case 9:
      case 13:
      case 17:
      case 21:
      case 25:
      case 29:
      case 33:
      case 37:
      case 41:
      case 45:
      case 49:
      case 53:
      case 57:
      case 61:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = w1;
        break;
      case 3:
      case 35:
        e00 = w2;
        e01 = w2;
        e10 = w2;
        e11 = w2;
        break;
      case 7:
      case 39:
        e00 = w3;
        e01 = w3;
        e10 = w3;
        e11 = w3;
        break;
      case 10:
        e01 = w0;
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 2, 1, 1);
        break;
      case 11:
      case 27:
        e01 = w2;
        e10 = w2;
        e11 = w2;
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 14:
        e10 = w0;
        e11 = w0;
        if (!equality.Equals(c1, c3)) {
          e00 = w0;
          e01 = w0;
        } else {
          e00 = lerp.Lerp(w1, w3, w0, 3, 3, 2);
          e01 = lerp.Lerp(w0, w1, 3, 1);
        }
        break;
      case 15:
        e10 = w4;
        e11 = w4;
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e01 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 3, 3, 2);
          e01 = lerp.Lerp(w4, w1, 3, 1);
        }
        break;
      case 18:
      case 22:
      case 30:
      case 50:
      case 54:
      case 62:
        e00 = w0;
        e10 = w0;
        e11 = w0;
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 19:
      case 51:
        e10 = w2;
        e11 = w2;
        if (!equality.Equals(c1, c5)) {
          e00 = w2;
          e01 = w2;
        } else {
          e00 = lerp.Lerp(w2, w1, 3, 1);
          e01 = lerp.Lerp(w1, w5, w2, 3, 3, 2);
        }
        break;
      case 23:
      case 55:
        e10 = w3;
        e11 = w3;
        if (!equality.Equals(c1, c5)) {
          e00 = w3;
          e01 = w3;
        } else {
          e00 = lerp.Lerp(w3, w1, 3, 1);
          e01 = lerp.Lerp(w1, w5, w3, 3, 3, 2);
        }
        break;
      case 26:
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 31:
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 2, 1, 1);
        break;
      case 42:
        e01 = w0;
        e11 = w0;
        if (!equality.Equals(c1, c3)) {
          e00 = w0;
          e10 = w0;
        } else {
          e00 = lerp.Lerp(w1, w3, w0, 3, 3, 2);
          e10 = lerp.Lerp(w0, w3, 3, 1);
        }
        break;
      case 43:
        e01 = w2;
        e11 = w2;
        if (!equality.Equals(c1, c3)) {
          e00 = w2;
          e10 = w2;
        } else {
          e00 = lerp.Lerp(w1, w3, w2, 3, 3, 2);
          e10 = lerp.Lerp(w2, w3, 3, 1);
        }
        break;
      case 46:
        e01 = w0;
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        break;
      case 47:
        e01 = w4;
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        break;
      case 58:
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 59:
        e10 = w2;
        e11 = w2;
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w2 : lerp.Lerp(w2, w1, w5, 6, 1, 1);
        break;
      case 63:
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 2, 1, 1);
        break;
      // Cases 64-127
      case 64:
      case 66:
      case 68:
      case 70:
      case 96:
      case 98:
      case 100:
      case 102:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = w0;
        break;
      case 65:
      case 69:
      case 97:
      case 101:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = w1;
        break;
      case 67:
      case 99:
        e00 = w2;
        e01 = w2;
        e10 = w2;
        e11 = w2;
        break;
      case 71:
      case 103:
        e00 = w3;
        e01 = w3;
        e10 = w3;
        e11 = w3;
        break;
      case 72:
      case 76:
      case 104:
      case 106:
      case 108:
      case 110:
      case 120:
      case 124:
        e00 = w0;
        e01 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        break;
      case 73:
      case 77:
      case 105:
      case 109:
      case 125:
        e01 = w1;
        e11 = w1;
        if (!equality.Equals(c7, c3)) {
          e00 = w1;
          e10 = w1;
        } else {
          e00 = lerp.Lerp(w1, w3, 3, 1);
          e10 = lerp.Lerp(w3, w7, w1, 3, 3, 2);
        }
        break;
      case 74:
        e01 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 2, 1, 1);
        break;
      case 75:
      case 123:
        e01 = w2;
        e11 = w2;
        e10 = !equality.Equals(c7, c3) ? w2 : lerp.Lerp(w2, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 78:
        e01 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        break;
      case 79:
        e01 = w4;
        e11 = w4;
        e10 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 2, 1, 1);
        break;
      case 80:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        break;
      case 81:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 2, 1, 1);
        break;
      case 82:
        e00 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 83:
      case 115:
        e00 = w2;
        e10 = w2;
        e11 = !equality.Equals(c7, c5) ? w2 : lerp.Lerp(w2, w5, w7, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w2 : lerp.Lerp(w2, w1, w5, 6, 1, 1);
        break;
      case 84:
        e00 = w0;
        e10 = w0;
        if (!equality.Equals(c7, c5)) {
          e01 = w0;
          e11 = w0;
        } else {
          e01 = lerp.Lerp(w0, w5, 3, 1);
          e11 = lerp.Lerp(w5, w7, w0, 3, 3, 2);
        }
        break;
      case 85:
        e00 = w1;
        e10 = w1;
        if (!equality.Equals(c7, c5)) {
          e01 = w1;
          e11 = w1;
        } else {
          e01 = lerp.Lerp(w1, w5, 3, 1);
          e11 = lerp.Lerp(w5, w7, w1, 3, 3, 2);
        }
        break;
      case 86:
      case 118:
        e00 = w0;
        e10 = w0;
        e11 = w0;
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 87:
        e00 = w3;
        e10 = w3;
        e11 = !equality.Equals(c7, c5) ? w3 : lerp.Lerp(w3, w5, w7, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w3 : lerp.Lerp(w3, w1, w5, 2, 1, 1);
        break;
      case 88:
        e00 = w0;
        e01 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        break;
      case 89:
      case 93:
        e00 = w1;
        e01 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 6, 1, 1);
        break;
      case 90:
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 91:
        e10 = !equality.Equals(c7, c3) ? w2 : lerp.Lerp(w2, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w2 : lerp.Lerp(w2, w5, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w2 : lerp.Lerp(w2, w1, w5, 6, 1, 1);
        break;
      case 92:
        e00 = w0;
        e01 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        break;
      case 94:
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 95:
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 2, 1, 1);
        break;
      case 107:
        e01 = w2;
        e11 = w2;
        e10 = !equality.Equals(c7, c3) ? w2 : lerp.Lerp(w2, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 111:
        e01 = w4;
        e11 = w4;
        e10 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        break;
      case 112:
        e00 = w0;
        e01 = w0;
        if (!equality.Equals(c7, c5)) {
          e10 = w0;
          e11 = w0;
        } else {
          e10 = lerp.Lerp(w0, w7, 3, 1);
          e11 = lerp.Lerp(w5, w7, w0, 3, 3, 2);
        }
        break;
      case 113:
        e00 = w1;
        e01 = w1;
        if (!equality.Equals(c7, c5)) {
          e10 = w1;
          e11 = w1;
        } else {
          e10 = lerp.Lerp(w1, w7, 3, 1);
          e11 = lerp.Lerp(w5, w7, w1, 3, 3, 2);
        }
        break;
      case 114:
        e00 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 116:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        break;
      case 117:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 6, 1, 1);
        break;
      case 119:
        e10 = w3;
        e11 = w3;
        if (!equality.Equals(c1, c5)) {
          e00 = w3;
          e01 = w3;
        } else {
          e00 = lerp.Lerp(w3, w1, 3, 1);
          e01 = lerp.Lerp(w1, w5, w3, 3, 3, 2);
        }
        break;
      case 121:
        e00 = w1;
        e01 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 6, 1, 1);
        break;
      case 122:
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 126:
        e00 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 127:
        e11 = w4;
        e10 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 2, 1, 1);
        break;
      // Cases 128-191
      case 128:
      case 130:
      case 132:
      case 134:
      case 136:
      case 140:
      case 144:
      case 148:
      case 152:
      case 156:
      case 160:
      case 162:
      case 164:
      case 166:
      case 168:
      case 172:
      case 176:
      case 180:
      case 184:
      case 188:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = w0;
        break;
      case 129:
      case 133:
      case 137:
      case 141:
      case 145:
      case 149:
      case 153:
      case 157:
      case 161:
      case 165:
      case 169:
      case 173:
      case 177:
      case 181:
      case 185:
      case 189:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = w1;
        break;
      case 131:
      case 163:
        e00 = w2;
        e01 = w2;
        e10 = w2;
        e11 = w2;
        break;
      case 135:
      case 167:
        e00 = w3;
        e01 = w3;
        e10 = w3;
        e11 = w3;
        break;
      case 138:
        e01 = w0;
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 2, 1, 1);
        break;
      case 139:
      case 155:
        e01 = w2;
        e10 = w2;
        e11 = w2;
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 142:
        e10 = w0;
        e11 = w0;
        if (!equality.Equals(c1, c3)) {
          e00 = w0;
          e01 = w0;
        } else {
          e00 = lerp.Lerp(w1, w3, w0, 3, 3, 2);
          e01 = lerp.Lerp(w0, w1, 3, 1);
        }
        break;
      case 143:
        e10 = w4;
        e11 = w4;
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e01 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 3, 3, 2);
          e01 = lerp.Lerp(w4, w1, 3, 1);
        }
        break;
      case 146:
      case 150:
      case 178:
      case 182:
      case 190:
        e00 = w0;
        e10 = w0;
        if (!equality.Equals(c1, c5)) {
          e01 = w0;
          e11 = w0;
        } else {
          e01 = lerp.Lerp(w1, w5, w0, 3, 3, 2);
          e11 = lerp.Lerp(w0, w5, 3, 1);
        }
        break;
      case 147:
      case 179:
        e00 = w2;
        e10 = w2;
        e11 = w2;
        e01 = !equality.Equals(c1, c5) ? w2 : lerp.Lerp(w2, w1, w5, 6, 1, 1);
        break;
      case 151:
      case 183:
        e00 = w3;
        e10 = w3;
        e11 = w3;
        e01 = !equality.Equals(c1, c5) ? w3 : lerp.Lerp(w3, w1, w5, 14, 1, 1);
        break;
      case 154:
      case 186:
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 158:
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 159:
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 14, 1, 1);
        break;
      case 170:
        e01 = w0;
        e11 = w0;
        if (!equality.Equals(c1, c3)) {
          e00 = w0;
          e10 = w0;
        } else {
          e00 = lerp.Lerp(w1, w3, w0, 3, 3, 2);
          e10 = lerp.Lerp(w0, w3, 3, 1);
        }
        break;
      case 171:
      case 187:
        e01 = w2;
        e11 = w2;
        if (!equality.Equals(c1, c3)) {
          e00 = w2;
          e10 = w2;
        } else {
          e00 = lerp.Lerp(w1, w3, w2, 3, 3, 2);
          e10 = lerp.Lerp(w2, w3, 3, 1);
        }
        break;
      case 174:
        e01 = w0;
        e10 = w0;
        e11 = w0;
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        break;
      case 175:
        e01 = w4;
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        break;
      case 191:
        e10 = w4;
        e11 = w4;
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 14, 1, 1);
        break;
      // Cases 192-255
      case 192:
      case 194:
      case 196:
      case 198:
      case 224:
      case 226:
      case 228:
      case 230:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = w0;
        break;
      case 193:
      case 197:
      case 225:
      case 229:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = w1;
        break;
      case 195:
      case 227:
        e00 = w2;
        e01 = w2;
        e10 = w2;
        e11 = w2;
        break;
      case 199:
      case 231:
        e00 = w3;
        e01 = w3;
        e10 = w3;
        e11 = w3;
        break;
      case 200:
      case 204:
      case 232:
      case 236:
      case 238:
        e00 = w0;
        e01 = w0;
        if (!equality.Equals(c7, c3)) {
          e10 = w0;
          e11 = w0;
        } else {
          e10 = lerp.Lerp(w3, w7, w0, 3, 3, 2);
          e11 = lerp.Lerp(w0, w7, 3, 1);
        }
        break;
      case 201:
      case 205:
        e00 = w1;
        e01 = w1;
        e11 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 6, 1, 1);
        break;
      case 202:
      case 206:
        e01 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        break;
      case 203:
        e01 = w2;
        e10 = w2;
        e11 = w2;
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 207:
        e10 = w4;
        e11 = w4;
        if (!equality.Equals(c1, c3)) {
          e00 = w4;
          e01 = w4;
        } else {
          e00 = lerp.Lerp(w1, w3, w4, 3, 3, 2);
          e01 = lerp.Lerp(w4, w1, 3, 1);
        }
        break;
      case 208:
      case 210:
      case 216:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        break;
      case 209:
      case 217:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 2, 1, 1);
        break;
      case 211:
        e00 = w2;
        e01 = w2;
        e10 = w2;
        e11 = !equality.Equals(c7, c5) ? w2 : lerp.Lerp(w2, w5, w7, 2, 1, 1);
        break;
      case 212:
        e00 = w0;
        e10 = w0;
        if (!equality.Equals(c7, c5)) {
          e01 = w0;
          e11 = w0;
        } else {
          e01 = lerp.Lerp(w0, w5, 3, 1);
          e11 = lerp.Lerp(w5, w7, w0, 3, 3, 2);
        }
        break;
      case 213:
      case 221:
        e00 = w1;
        e10 = w1;
        if (!equality.Equals(c7, c5)) {
          e01 = w1;
          e11 = w1;
        } else {
          e01 = lerp.Lerp(w1, w5, 3, 1);
          e11 = lerp.Lerp(w5, w7, w1, 3, 3, 2);
        }
        break;
      case 214:
      case 222:
        e00 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 215:
        e00 = w3;
        e10 = w3;
        e11 = !equality.Equals(c7, c5) ? w3 : lerp.Lerp(w3, w5, w7, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w3 : lerp.Lerp(w3, w1, w5, 14, 1, 1);
        break;
      case 218:
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 219:
        e01 = w2;
        e10 = w2;
        e11 = !equality.Equals(c7, c5) ? w2 : lerp.Lerp(w2, w5, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 220:
        e00 = w0;
        e01 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 6, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        break;
      case 223:
        e10 = w4;
        e11 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 14, 1, 1);
        break;
      case 233:
      case 237:
        e00 = w1;
        e01 = w1;
        e11 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 14, 1, 1);
        break;
      case 234:
        e01 = w0;
        e11 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w0 : lerp.Lerp(w0, w1, w3, 6, 1, 1);
        break;
      case 235:
        e01 = w2;
        e11 = w2;
        e10 = !equality.Equals(c7, c3) ? w2 : lerp.Lerp(w2, w3, w7, 14, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 239:
        e01 = w4;
        e11 = w4;
        e10 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 14, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        break;
      case 240:
        e00 = w0;
        e01 = w0;
        if (!equality.Equals(c7, c5)) {
          e10 = w0;
          e11 = w0;
        } else {
          e10 = lerp.Lerp(w0, w7, 3, 1);
          e11 = lerp.Lerp(w5, w7, w0, 3, 3, 2);
        }
        break;
      case 241:
        e00 = w1;
        e01 = w1;
        if (!equality.Equals(c7, c5)) {
          e10 = w1;
          e11 = w1;
        } else {
          e10 = lerp.Lerp(w1, w7, 3, 1);
          e11 = lerp.Lerp(w5, w7, w1, 3, 3, 2);
        }
        break;
      case 242:
        e00 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 6, 1, 1);
        break;
      case 243:
        e00 = w2;
        e01 = w2;
        if (!equality.Equals(c7, c5)) {
          e10 = w2;
          e11 = w2;
        } else {
          e10 = lerp.Lerp(w2, w7, 3, 1);
          e11 = lerp.Lerp(w5, w7, w2, 3, 3, 2);
        }
        break;
      case 244:
        e00 = w0;
        e01 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 14, 1, 1);
        break;
      case 245:
        e00 = w1;
        e01 = w1;
        e10 = w1;
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 14, 1, 1);
        break;
      case 246:
        e00 = w0;
        e10 = w0;
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 247:
        e00 = w3;
        e10 = w3;
        e11 = !equality.Equals(c7, c5) ? w3 : lerp.Lerp(w3, w5, w7, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w3 : lerp.Lerp(w3, w1, w5, 14, 1, 1);
        break;
      case 248:
      case 250:
        e00 = w0;
        e01 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 2, 1, 1);
        break;
      case 249:
        e00 = w1;
        e01 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 14, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 2, 1, 1);
        break;
      case 251:
        e01 = w2;
        e10 = !equality.Equals(c7, c3) ? w2 : lerp.Lerp(w2, w3, w7, 14, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w2 : lerp.Lerp(w2, w5, w7, 2, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w2 : lerp.Lerp(w2, w1, w3, 2, 1, 1);
        break;
      case 252:
        e00 = w0;
        e01 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 14, 1, 1);
        break;
      case 253:
        e00 = w1;
        e01 = w1;
        e10 = !equality.Equals(c7, c3) ? w1 : lerp.Lerp(w1, w3, w7, 14, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w1 : lerp.Lerp(w1, w5, w7, 14, 1, 1);
        break;
      case 254:
        e00 = w0;
        e10 = !equality.Equals(c7, c3) ? w0 : lerp.Lerp(w0, w3, w7, 2, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w0 : lerp.Lerp(w0, w5, w7, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w0 : lerp.Lerp(w0, w1, w5, 2, 1, 1);
        break;
      case 255:
        e10 = !equality.Equals(c7, c3) ? w4 : lerp.Lerp(w4, w3, w7, 14, 1, 1);
        e11 = !equality.Equals(c7, c5) ? w4 : lerp.Lerp(w4, w5, w7, 14, 1, 1);
        e00 = !equality.Equals(c1, c3) ? w4 : lerp.Lerp(w4, w1, w3, 14, 1, 1);
        e01 = !equality.Equals(c1, c5) ? w4 : lerp.Lerp(w4, w1, w5, 14, 1, 1);
        break;
    }
  }
}
