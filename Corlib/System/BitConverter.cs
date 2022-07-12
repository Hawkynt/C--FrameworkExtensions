#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System.Collections.Generic;
namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static class BitConverterExtension {
    public const byte ISNULL = 0;
    public const byte NOTNULL = 255;
    #region sbyte
    public static sbyte ToSByte(byte[] arrBytes, int intOffset = 0) {
      return ((sbyte)arrBytes[intOffset]);
    }
    public static byte[] GetBytes(sbyte sbyteVal) {
      return (new[] { (byte)sbyteVal });
    }
    #endregion
    #region sbyte?
    public static sbyte? ToNSByte(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset] == ISNULL ? (sbyte?) null : (sbyte) arrBytes[++intOffset]);
    }

    public static byte[] GetBytes(sbyte? byteVal) {
      return (byteVal.HasValue ? (new[] { NOTNULL, (byte)byteVal.Value }) : (new byte[] { ISNULL, 0 }));
    }

    #endregion
    #region byte
    public static byte ToByte(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset]);
    }
    public static byte[] GetBytes(byte byteVal) {
      return (new[] { byteVal });
    }
    #endregion
    #region byte?
    public static byte? ToNByte(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (byte?) null : arrBytes[intOffset]);
    }

    public static byte[] GetBytes(byte? byteVal) {
      return (byteVal.HasValue ? (new[] { NOTNULL, byteVal.Value }) : (new byte[] { ISNULL, 0 }));
    }

    #endregion
    #region char
    public static char ToChar(byte[] arrBytes, int intOffset = 0) {
      return ((char)arrBytes[intOffset]);
    }
    public static byte[] GetBytes(char chrVal) {
      return (new[] { (byte)chrVal });
    }
    #endregion
    #region char?
    public static char? ToNChar(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (char?) null : (char) arrBytes[intOffset]);
    }

    public static byte[] GetBytes(char? chrVal) {
      return (chrVal.HasValue ? (new[] { NOTNULL, (byte)chrVal.Value }) : (new byte[] { ISNULL, 0 }));
    }

    #endregion
    #region short
    public static short ToShort(byte[] arrBytes, int intOffset = 0) {
      return ((short)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8)));
    }
    public static byte[] GetBytes(short shortVal) {
      return (new[] { (byte)shortVal, (byte)(shortVal >> 8) });
    }
    #endregion
    #region short?
    public static short? ToNShort(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (short?) null : (short) (arrBytes[intOffset] | (arrBytes[++intOffset] << 8)));
    }

    public static byte[] GetBytes(short? shortVal) {
      return (shortVal.HasValue ? (new[] { NOTNULL, (byte)shortVal.Value, (byte)(shortVal.Value >> 8) }) : (new byte[] { ISNULL, 0, 0 }));
    }

    #endregion
    #region ushort
    public static ushort ToWord(byte[] arrBytes, int intOffset = 0) {
      return ((ushort)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8)));
    }
    public static byte[] GetBytes(ushort wordVal) {
      return (new[] { (byte)wordVal, (byte)(wordVal >> 8) });
    }
    #endregion
    #region ushort?
    public static ushort? ToNWord(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (ushort?) null : (ushort) (arrBytes[intOffset] | (arrBytes[++intOffset] << 8)));
    }

    public static byte[] GetBytes(ushort? wordVal) {
      return (wordVal.HasValue ? (new[] { NOTNULL, (byte)wordVal.Value, (byte)(wordVal.Value >> 8) }) : (new byte[] { ISNULL, 0, 0 }));
    }

    #endregion
    #region int
    public static int ToInt(byte[] arrBytes, int intOffset = 0) {
      return (((int)arrBytes[intOffset] | ((int)arrBytes[++intOffset] << 8) | ((int)arrBytes[++intOffset] << 16) | ((int)arrBytes[++intOffset] << 24)));
    }
    public static byte[] GetBytes(int intVal) {
      return (new[] { (byte)intVal, (byte)(intVal >> 8), (byte)(intVal >> 16), (byte)(intVal >> 24) });
    }
    #endregion
    #region int?
    public static int? ToNInt(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (int?) null : (int) arrBytes[intOffset] | ((int) arrBytes[++intOffset] << 8) | ((int) arrBytes[++intOffset] << 16) | ((int) arrBytes[++intOffset] << 24));
    }

    public static byte[] GetBytes(int? intVal) {
      return (intVal.HasValue ? (new[] { NOTNULL, (byte)intVal.Value, (byte)(intVal.Value >> 8), (byte)(intVal.Value >> 16), (byte)(intVal.Value >> 24) }) : (new byte[] { ISNULL, 0, 0, 0, 0 }));
    }

    #endregion
    #region uint
    public static uint ToDWord(byte[] arrBytes, int intOffset = 0) {
      return (((uint)arrBytes[intOffset] | ((uint)arrBytes[++intOffset] << 8) | ((uint)arrBytes[++intOffset] << 16) | ((uint)arrBytes[++intOffset] << 24)));
    }
    public static byte[] GetBytes(uint dwordVal) {
      return (new[] { (byte)dwordVal, (byte)(dwordVal >> 8), (byte)(dwordVal >> 16), (byte)(dwordVal >> 24) });
    }
    #endregion
    #region uint?
    public static uint? ToNDWord(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (uint?) null : (uint) arrBytes[intOffset] | ((uint) arrBytes[++intOffset] << 8) | ((uint) arrBytes[++intOffset] << 16) | ((uint) arrBytes[++intOffset] << 24));
    }

    public static byte[] GetBytes(uint? dwordVal) {
      return (dwordVal.HasValue ? (new[] { NOTNULL, (byte)dwordVal.Value, (byte)(dwordVal.Value >> 8), (byte)(dwordVal.Value >> 16), (byte)(dwordVal.Value >> 24) }) : (new byte[] { ISNULL, 0, 0, 0, 0 }));
    }

    #endregion
    #region long
    public static long ToLong(byte[] arrBytes, int intOffset = 0) {
      return ((
        (long)arrBytes[intOffset] | ((long)arrBytes[++intOffset] << 8) | ((long)arrBytes[++intOffset] << 16) | ((long)arrBytes[++intOffset] << 24) |
        ((long)arrBytes[++intOffset] << 32) | ((long)arrBytes[++intOffset] << 40) | ((long)arrBytes[++intOffset] << 48) | ((long)arrBytes[++intOffset] << 56)
      ));
    }
    public static byte[] GetBytes(long longVal) {
      return (new byte[] {  
        (byte)longVal, (byte)(longVal >> 8), (byte)(longVal >> 16), (byte)(longVal >> 24),
        (byte)(longVal>>32), (byte)(longVal >> 40), (byte)(longVal >> 48), (byte)(longVal >> 56)
      });
    }
    #endregion
    #region long?
    public static long? ToNLong(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (long?) null : (long) arrBytes[intOffset] | ((long) arrBytes[++intOffset] << 8) | ((long) arrBytes[++intOffset] << 16) | ((long) arrBytes[++intOffset] << 24) |
        ((long) arrBytes[++intOffset] << 32) | ((long) arrBytes[++intOffset] << 40) | ((long) arrBytes[++intOffset] << 48) | ((long) arrBytes[++intOffset] << 56));
    }

    public static byte[] GetBytes(long? longVal) {
      return (longVal.HasValue ? new[] {
        NOTNULL,
        (byte) longVal.Value, (byte) (longVal.Value >> 8), (byte) (longVal.Value >> 16), (byte) (longVal.Value >> 24),
        (byte) (longVal.Value >> 32), (byte) (longVal.Value >> 40), (byte) (longVal.Value >> 48), (byte) (longVal.Value >> 56)
      } : new byte[] {ISNULL, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    #endregion
    #region ulong
    public static ulong ToQWord(byte[] arrBytes, int intOffset = 0) {
      return ((
        (ulong)arrBytes[intOffset] | ((ulong)arrBytes[++intOffset] << 8) | ((ulong)arrBytes[++intOffset] << 16) | ((ulong)arrBytes[++intOffset] << 24) |
        ((ulong)arrBytes[++intOffset] << 32) | ((ulong)arrBytes[++intOffset] << 40) | ((ulong)arrBytes[++intOffset] << 48) | ((ulong)arrBytes[++intOffset] << 56)
      ));
    }
    public static byte[] GetBytes(ulong qwordVal) {
      return (new[] {  
        (byte)qwordVal, (byte)(qwordVal >> 8), (byte)(qwordVal >> 16), (byte)(qwordVal >> 24),
        (byte)(qwordVal>>32), (byte)(qwordVal >> 40), (byte)(qwordVal >> 48), (byte)(qwordVal >> 56)
      });
    }
    #endregion
    #region ulong?
    public static ulong? ToNQWord(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (ulong?) null : (ulong) arrBytes[intOffset] | ((ulong) arrBytes[++intOffset] << 8) | ((ulong) arrBytes[++intOffset] << 16) | ((ulong) arrBytes[++intOffset] << 24) |
        ((ulong) arrBytes[++intOffset] << 32) | ((ulong) arrBytes[++intOffset] << 40) | ((ulong) arrBytes[++intOffset] << 48) | ((ulong) arrBytes[++intOffset] << 56));
    }

    public static byte[] GetBytes(ulong? qwordVal) {
      return (qwordVal.HasValue ? new[] {
        NOTNULL,
        (byte) qwordVal.Value, (byte) (qwordVal.Value >> 8), (byte) (qwordVal.Value >> 16), (byte) (qwordVal.Value >> 24),
        (byte) (qwordVal.Value >> 32), (byte) (qwordVal.Value >> 40), (byte) (qwordVal.Value >> 48), (byte) (qwordVal.Value >> 56)
      } : new byte[] {ISNULL, 0, 0, 0, 0, 0, 0, 0, 0});
    }

    #endregion
    #region float
    public static float ToFloat(byte[] arrBytes, int intOffset = 0) {
      return (BitConverter.ToSingle(arrBytes, intOffset));
    }
    public static byte[] GetBytes(float fltVal) {
      return (BitConverter.GetBytes(fltVal));
    }
    #endregion
    #region float?
    public static float? ToNFloat(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (float?) null : BitConverter.ToSingle(arrBytes, intOffset));
    }

    public static byte[] GetBytes(float? fltVal) {
      if (fltVal.HasValue) {
        List<byte> arrRet = new List<byte> {NOTNULL};
        arrRet.AddRange(BitConverter.GetBytes((float)fltVal));
        return (arrRet.ToArray());
      } else {
        List<byte> arrRet = new List<byte> {ISNULL};
        arrRet.AddRange(BitConverter.GetBytes((float)0));
        return (arrRet.ToArray());
      }
    }
    #endregion
    #region double
    public static double ToDouble(byte[] arrBytes, int intOffset = 0) {
      return (BitConverter.ToDouble(arrBytes, intOffset));
    }
    public static byte[] GetBytes(double dblVal) {
      return (BitConverter.GetBytes(dblVal));
    }
    #endregion
    #region double?
    public static double? ToNDouble(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (double?) null : BitConverter.ToDouble(arrBytes, intOffset));
    }

    public static byte[] GetBytes(double? dblVal) {
      if (dblVal.HasValue) {
        List<byte> arrRet = new List<byte> {NOTNULL};
        arrRet.AddRange(BitConverter.GetBytes((double)dblVal));
        return (arrRet.ToArray());
      } else {
        List<byte> arrRet = new List<byte> {ISNULL};
        arrRet.AddRange(BitConverter.GetBytes((double)0));
        return (arrRet.ToArray());
      }
    }
    #endregion
    #region decimal
    public static decimal ToDecimal(byte[] arrBytes, int intOffset = 0) {
      return (new decimal(new[]{
        ((arrBytes[intOffset+0] | (arrBytes[intOffset+1] << 8)) | (arrBytes[intOffset+2] << 0x10)) | (arrBytes[intOffset+3] << 0x18), //lo
        ((arrBytes[intOffset+4] | (arrBytes[intOffset+5] << 8)) | (arrBytes[intOffset+6] << 0x10)) | (arrBytes[intOffset+7] << 0x18), //mid
        ((arrBytes[intOffset+8] | (arrBytes[intOffset+9] << 8)) | (arrBytes[intOffset+10] << 0x10)) | (arrBytes[intOffset+11] << 0x18), //hi
        ((arrBytes[intOffset+12] | (arrBytes[intOffset+13] << 8)) | (arrBytes[intOffset+14] << 0x10)) | (arrBytes[intOffset+15] << 0x18) //flags
      }));
    }

    public static byte[] GetBytes(decimal decVal) {
      Int32[] arrBits = decimal.GetBits(decVal);
      Int32 intLow = arrBits[0];
      Int32 intMid = arrBits[1];
      Int32 intHigh = arrBits[2];
      Int32 intFlags = arrBits[3];
      return (new[]{
        (byte)intLow,(byte)(intLow >> 8),(byte)(intLow >> 0x10),(byte)(intLow >> 0x18),
        (byte)intMid,(byte)(intMid >> 8),(byte)(intMid >> 0x10),(byte)(intMid >> 0x18),
        (byte)intHigh,(byte)(intHigh >> 8),(byte)(intHigh >> 0x10),(byte)(intHigh >> 0x18),
        (byte)intFlags,(byte)(intFlags >> 8),(byte)(intFlags >> 0x10),(byte)(intFlags >> 0x18)
      });
    }
    #endregion
    #region decimal?
    public static decimal? ToNDecimal(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (decimal?) null : BitConverterExtension.ToDecimal(arrBytes, intOffset));
    }

    public static byte[] GetBytes(decimal? decVal) {
      if (decVal.HasValue) {
        List<byte> arrRet = new List<byte> {NOTNULL};
        arrRet.AddRange(BitConverterExtension.GetBytes((decimal)decVal));
        return (arrRet.ToArray());
      } else {
        List<byte> arrRet = new List<byte> {ISNULL};
        arrRet.AddRange(BitConverterExtension.GetBytes((decimal)0));
        return (arrRet.ToArray());
      }
    }
    #endregion
    #region bool
    public static bool ToBool(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset] == NOTNULL);
    }
    public static byte[] GetBytes(bool boolVal) {
      return (new[] { boolVal ? NOTNULL : ISNULL });
    }
    #endregion
    #region bool?
    public static bool? ToNBool(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (bool?) null : arrBytes[intOffset] == NOTNULL);
    }

    public static byte[] GetBytes(bool? boolVal) {
      return (boolVal.HasValue ? (new[] { NOTNULL, boolVal.Value ? NOTNULL : ISNULL }) : (new byte[] { ISNULL, 0 }));
    }

    #endregion
    #region DateTime
    public static DateTime ToDateTime(byte[] arrBytes, int intOffset = 0) {
      return (new DateTime(BitConverter.ToInt64(arrBytes, intOffset)));
    }
    public static byte[] GetBytes(DateTime dtVal) {
      return (BitConverter.GetBytes(dtVal.Ticks));
    }
    #endregion
    #region DateTime?
    public static DateTime? ToNDateTime(byte[] arrBytes, int intOffset = 0) {
      return (arrBytes[intOffset++] == ISNULL ? (DateTime?) null : BitConverterExtension.ToDateTime(arrBytes, intOffset));
    }

    public static byte[] GetBytes(DateTime? dtVal) {
      if (dtVal.HasValue) {
        List<byte> arrRet = new List<byte> {NOTNULL};
        arrRet.AddRange(BitConverterExtension.GetBytes((DateTime)dtVal));
        return (arrRet.ToArray());
      } else {
        List<byte> arrRet = new List<byte> {ISNULL};
        arrRet.AddRange(BitConverterExtension.GetBytes(DateTime.UtcNow));
        return (arrRet.ToArray());
      }
    }
    #endregion
  }
}