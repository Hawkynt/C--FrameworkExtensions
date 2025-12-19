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

#nullable enable

using System.Collections.Generic;

namespace System;

public static class BitConverterExtension {
  public const byte ISNULL = 0;
  public const byte NOTNULL = 255;

  #region sbyte

  public static sbyte ToSByte(byte[] arrBytes, int intOffset = 0) => (sbyte)arrBytes[intOffset];

  public static byte[] GetBytes(sbyte sbyteVal) => [(byte)sbyteVal];

  #endregion

  #region sbyte?

  public static sbyte? ToNSByte(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset] == ISNULL ? null : (sbyte)arrBytes[++intOffset];

  public static byte[] GetBytes(sbyte? byteVal) => byteVal.HasValue ? [NOTNULL, (byte)byteVal.Value] : [ISNULL, 0];

  #endregion

  #region byte

  public static byte ToByte(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset];

  public static byte[] GetBytes(byte byteVal) => [byteVal];

  #endregion

  #region byte?

  public static byte? ToNByte(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset];

  public static byte[] GetBytes(byte? byteVal) => byteVal.HasValue ? [NOTNULL, byteVal.Value] : [ISNULL, 0];

  #endregion

  #region char

  public static char ToChar(byte[] arrBytes, int intOffset = 0) => (char)arrBytes[intOffset];

  public static byte[] GetBytes(char chrVal) => [(byte)chrVal];

  #endregion

  #region char?

  public static char? ToNChar(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : (char)arrBytes[intOffset];

  public static byte[] GetBytes(char? chrVal) => chrVal.HasValue ? [NOTNULL, (byte)chrVal.Value] : [ISNULL, 0];

  #endregion

  #region short

  public static short ToShort(byte[] arrBytes, int intOffset = 0) => (short)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8));

  public static byte[] GetBytes(short shortVal) => [(byte)shortVal, (byte)(shortVal >> 8)];

  #endregion

  #region short?

  public static short? ToNShort(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : (short)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8));

  public static byte[] GetBytes(short? shortVal) => shortVal.HasValue ? [NOTNULL, (byte)shortVal.Value, (byte)(shortVal.Value >> 8)] : [ISNULL, 0, 0];

  #endregion

  #region ushort

  public static ushort ToWord(byte[] arrBytes, int intOffset = 0) => (ushort)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8));

  public static byte[] GetBytes(ushort wordVal) => [(byte)wordVal, (byte)(wordVal >> 8)];

  #endregion

  #region ushort?

  public static ushort? ToNWord(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : (ushort)(arrBytes[intOffset] | (arrBytes[++intOffset] << 8));

  public static byte[] GetBytes(ushort? wordVal) => wordVal.HasValue ? [NOTNULL, (byte)wordVal.Value, (byte)(wordVal.Value >> 8)] : [ISNULL, 0, 0];

  #endregion

  #region int

  public static int ToInt(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset] | (arrBytes[++intOffset] << 8) | (arrBytes[++intOffset] << 16) | (arrBytes[++intOffset] << 24);

  public static byte[] GetBytes(int intVal) => [(byte)intVal, (byte)(intVal >> 8), (byte)(intVal >> 16), (byte)(intVal >> 24)];

  #endregion

  #region int?

  public static int? ToNInt(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset] | (arrBytes[++intOffset] << 8) | (arrBytes[++intOffset] << 16) | (arrBytes[++intOffset] << 24);

  public static byte[] GetBytes(int? intVal) => intVal.HasValue ? [NOTNULL, (byte)intVal.Value, (byte)(intVal.Value >> 8), (byte)(intVal.Value >> 16), (byte)(intVal.Value >> 24)] : [ISNULL, 0, 0, 0, 0];

  #endregion

  #region uint

  public static uint ToDWord(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset] | ((uint)arrBytes[++intOffset] << 8) | ((uint)arrBytes[++intOffset] << 16) | ((uint)arrBytes[++intOffset] << 24);

  public static byte[] GetBytes(uint dwordVal) => [(byte)dwordVal, (byte)(dwordVal >> 8), (byte)(dwordVal >> 16), (byte)(dwordVal >> 24)];

  #endregion

  #region uint?

  public static uint? ToNDWord(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset] | ((uint)arrBytes[++intOffset] << 8) | ((uint)arrBytes[++intOffset] << 16) | ((uint)arrBytes[++intOffset] << 24);

  public static byte[] GetBytes(uint? dwordVal) => dwordVal.HasValue ? [NOTNULL, (byte)dwordVal.Value, (byte)(dwordVal.Value >> 8), (byte)(dwordVal.Value >> 16), (byte)(dwordVal.Value >> 24)] : [ISNULL, 0, 0, 0, 0];

  #endregion

  #region long

  public static long ToLong(byte[] arrBytes, int intOffset = 0) =>
    arrBytes[intOffset] | ((long)arrBytes[++intOffset] << 8) | ((long)arrBytes[++intOffset] << 16) | ((long)arrBytes[++intOffset] << 24) | ((long)arrBytes[++intOffset] << 32) | ((long)arrBytes[++intOffset] << 40) | ((long)arrBytes[++intOffset] << 48) | ((long)arrBytes[++intOffset] << 56);

  public static byte[] GetBytes(long longVal) => [(byte)longVal, (byte)(longVal >> 8), (byte)(longVal >> 16), (byte)(longVal >> 24), (byte)(longVal >> 32), (byte)(longVal >> 40), (byte)(longVal >> 48), (byte)(longVal >> 56)];

  #endregion

  #region long?

  public static long? ToNLong(byte[] arrBytes, int intOffset = 0) =>
    arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset] | ((long)arrBytes[++intOffset] << 8) | ((long)arrBytes[++intOffset] << 16) | ((long)arrBytes[++intOffset] << 24) | ((long)arrBytes[++intOffset] << 32) | ((long)arrBytes[++intOffset] << 40) | ((long)arrBytes[++intOffset] << 48) | ((long)arrBytes[++intOffset] << 56);

  public static byte[] GetBytes(long? longVal) =>
    longVal.HasValue
      ? [
        NOTNULL,
        (byte)longVal.Value,
        (byte)(longVal.Value >> 8),
        (byte)(longVal.Value >> 16),
        (byte)(longVal.Value >> 24),
        (byte)(longVal.Value >> 32),
        (byte)(longVal.Value >> 40),
        (byte)(longVal.Value >> 48),
        (byte)(longVal.Value >> 56)
      ]
      : [
        ISNULL,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ];

  #endregion

  #region ulong

  public static ulong ToQWord(byte[] arrBytes, int intOffset = 0) =>
    arrBytes[intOffset] | ((ulong)arrBytes[++intOffset] << 8) | ((ulong)arrBytes[++intOffset] << 16) | ((ulong)arrBytes[++intOffset] << 24) | ((ulong)arrBytes[++intOffset] << 32) | ((ulong)arrBytes[++intOffset] << 40) | ((ulong)arrBytes[++intOffset] << 48) | ((ulong)arrBytes[++intOffset] << 56);

  public static byte[] GetBytes(ulong qwordVal) => [(byte)qwordVal, (byte)(qwordVal >> 8), (byte)(qwordVal >> 16), (byte)(qwordVal >> 24), (byte)(qwordVal >> 32), (byte)(qwordVal >> 40), (byte)(qwordVal >> 48), (byte)(qwordVal >> 56)];

  #endregion

  #region ulong?

  public static ulong? ToNQWord(byte[] arrBytes, int intOffset = 0) =>
    arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset] | ((ulong)arrBytes[++intOffset] << 8) | ((ulong)arrBytes[++intOffset] << 16) | ((ulong)arrBytes[++intOffset] << 24) | ((ulong)arrBytes[++intOffset] << 32) | ((ulong)arrBytes[++intOffset] << 40) | ((ulong)arrBytes[++intOffset] << 48) | ((ulong)arrBytes[++intOffset] << 56);

  public static byte[] GetBytes(ulong? qwordVal) =>
    qwordVal.HasValue
      ? [
        NOTNULL,
        (byte)qwordVal.Value,
        (byte)(qwordVal.Value >> 8),
        (byte)(qwordVal.Value >> 16),
        (byte)(qwordVal.Value >> 24),
        (byte)(qwordVal.Value >> 32),
        (byte)(qwordVal.Value >> 40),
        (byte)(qwordVal.Value >> 48),
        (byte)(qwordVal.Value >> 56)
      ]
      : [
        ISNULL,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      ];

  #endregion

  #region float

  public static float ToFloat(byte[] arrBytes, int intOffset = 0) => BitConverter.ToSingle(arrBytes, intOffset);

  public static byte[] GetBytes(float fltVal) => BitConverter.GetBytes(fltVal);

  #endregion

  #region float?

  public static float? ToNFloat(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : BitConverter.ToSingle(arrBytes, intOffset);

  public static byte[] GetBytes(float? fltVal) {
    if (fltVal.HasValue) {
      List<byte> arrRet = new(5) { NOTNULL };
      arrRet.AddRange(BitConverter.GetBytes((float)fltVal));
      return arrRet.ToArray();
    } else {
      List<byte> arrRet = new(5) { ISNULL };
      arrRet.AddRange(BitConverter.GetBytes((float)0));
      return arrRet.ToArray();
    }
  }

  #endregion

  #region double

  public static double ToDouble(byte[] arrBytes, int intOffset = 0) => BitConverter.ToDouble(arrBytes, intOffset);

  public static byte[] GetBytes(double dblVal) => BitConverter.GetBytes(dblVal);

  #endregion

  #region double?

  public static double? ToNDouble(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : BitConverter.ToDouble(arrBytes, intOffset);

  public static byte[] GetBytes(double? dblVal) {
    if (dblVal.HasValue) {
      List<byte> arrRet = new(9) { NOTNULL };
      arrRet.AddRange(BitConverter.GetBytes((double)dblVal));
      return arrRet.ToArray();
    } else {
      List<byte> arrRet = new(9) { ISNULL };
      arrRet.AddRange(BitConverter.GetBytes((double)0));
      return arrRet.ToArray();
    }
  }

  #endregion

  #region decimal

  public static decimal ToDecimal(byte[] arrBytes, int intOffset = 0) =>
    new(
      [
        arrBytes[intOffset + 0] | (arrBytes[intOffset + 1] << 8) | (arrBytes[intOffset + 2] << 0x10) | (arrBytes[intOffset + 3] << 0x18), //lo
        arrBytes[intOffset + 4] | (arrBytes[intOffset + 5] << 8) | (arrBytes[intOffset + 6] << 0x10) | (arrBytes[intOffset + 7] << 0x18), //mid
        arrBytes[intOffset + 8] | (arrBytes[intOffset + 9] << 8) | (arrBytes[intOffset + 10] << 0x10) | (arrBytes[intOffset + 11] << 0x18), //hi
        arrBytes[intOffset + 12] | (arrBytes[intOffset + 13] << 8) | (arrBytes[intOffset + 14] << 0x10) | (arrBytes[intOffset + 15] << 0x18) //flags
      ]
    );

  public static byte[] GetBytes(decimal decVal) {
    var arrBits = decimal.GetBits(decVal);
    var intLow = arrBits[0];
    var intMid = arrBits[1];
    var intHigh = arrBits[2];
    var intFlags = arrBits[3];
    return [
      (byte)intLow,
      (byte)(intLow >> 8),
      (byte)(intLow >> 0x10),
      (byte)(intLow >> 0x18),
      (byte)intMid,
      (byte)(intMid >> 8),
      (byte)(intMid >> 0x10),
      (byte)(intMid >> 0x18),
      (byte)intHigh,
      (byte)(intHigh >> 8),
      (byte)(intHigh >> 0x10),
      (byte)(intHigh >> 0x18),
      (byte)intFlags,
      (byte)(intFlags >> 8),
      (byte)(intFlags >> 0x10),
      (byte)(intFlags >> 0x18)
    ];
  }

  #endregion

  #region decimal?

  public static decimal? ToNDecimal(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : ToDecimal(arrBytes, intOffset);

  public static byte[] GetBytes(decimal? decVal) {
    if (decVal.HasValue) {
      List<byte> arrRet = new(17) { NOTNULL };
      arrRet.AddRange(GetBytes((decimal)decVal));
      return arrRet.ToArray();
    } else {
      List<byte> arrRet = new(17) { ISNULL };
      arrRet.AddRange(GetBytes((decimal)0));
      return arrRet.ToArray();
    }
  }

  #endregion

  #region bool

  public static bool ToBool(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset] == NOTNULL;

  public static byte[] GetBytes(bool boolVal) => [boolVal ? NOTNULL : ISNULL];

  #endregion

  #region bool?

  public static bool? ToNBool(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : arrBytes[intOffset] == NOTNULL;

  public static byte[] GetBytes(bool? boolVal) => boolVal.HasValue ? [NOTNULL, boolVal.Value ? NOTNULL : ISNULL] : [ISNULL, 0];

  #endregion

  #region DateTime

  public static DateTime ToDateTime(byte[] arrBytes, int intOffset = 0) => new(BitConverter.ToInt64(arrBytes, intOffset));

  public static byte[] GetBytes(DateTime dtVal) => BitConverter.GetBytes(dtVal.Ticks);

  #endregion

  #region DateTime?

  public static DateTime? ToNDateTime(byte[] arrBytes, int intOffset = 0) => arrBytes[intOffset++] == ISNULL ? null : ToDateTime(arrBytes, intOffset);

  public static byte[] GetBytes(DateTime? dtVal) {
    if (dtVal.HasValue) {
      List<byte> arrRet = new(9) { NOTNULL };
      arrRet.AddRange(GetBytes((DateTime)dtVal));
      return arrRet.ToArray();
    } else {
      List<byte> arrRet = new(9) { ISNULL };
      arrRet.AddRange(GetBytes(DateTime.UtcNow));
      return arrRet.ToArray();
    }
  }

  #endregion
}
