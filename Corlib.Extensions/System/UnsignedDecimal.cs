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

namespace System;

public struct UnsignedDecimal {
  private const string _errExceptionText = @"Value needs to be positive";
  private readonly decimal _value;

  public UnsignedDecimal(decimal value) {
    if (value < 0)
      throw new ArgumentException(_errExceptionText);

    this._value = value;
  }

  // This conversion is safe, we can make it implicit
  public static implicit operator decimal(UnsignedDecimal value) => value._value;

  // This conversion is not always safe, so we make it explicit
  public static explicit operator UnsignedDecimal(decimal value) {
    if (value < 0)
      throw new ArgumentOutOfRangeException(_errExceptionText);
    return new(value);
  }

  #region arithmetic

  #region byte

  public static implicit operator UnsignedDecimal(byte value) => new(value);

  public static UnsignedDecimal operator +(UnsignedDecimal a, byte b) => new(a._value + b);

  public static UnsignedDecimal operator +(byte a, UnsignedDecimal b) => new(a + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, byte b) => new(a._value * b);

  public static UnsignedDecimal operator *(byte a, UnsignedDecimal b) => new(a * b._value);

  #endregion

  #region char

  public static implicit operator UnsignedDecimal(char value) => new(value);

  public static UnsignedDecimal operator +(UnsignedDecimal a, char b) => new(a._value + b);

  public static UnsignedDecimal operator +(char a, UnsignedDecimal b) => new(a + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, char b) => new(a._value * b);

  public static UnsignedDecimal operator *(char a, UnsignedDecimal b) => new(a * b._value);

  #endregion

  #region ushort

  public static implicit operator UnsignedDecimal(ushort value) => new(value);

  public static UnsignedDecimal operator +(UnsignedDecimal a, ushort b) => new(a._value + b);

  public static UnsignedDecimal operator +(ushort a, UnsignedDecimal b) => new(a + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, ushort b) => new(a._value * b);

  public static UnsignedDecimal operator *(ushort a, UnsignedDecimal b) => new(a * b._value);

  #endregion

  #region uint

  public static implicit operator UnsignedDecimal(uint value) => new(value);

  public static UnsignedDecimal operator +(UnsignedDecimal a, uint b) => new(a._value + b);

  public static UnsignedDecimal operator +(uint a, UnsignedDecimal b) => new(a + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, uint b) => new(a._value * b);

  public static UnsignedDecimal operator *(uint a, UnsignedDecimal b) => new(a * b._value);

  #endregion

  #region ulong

  public static implicit operator UnsignedDecimal(ulong value) => new(value);

  public static UnsignedDecimal operator +(UnsignedDecimal a, ulong b) => new(a._value + b);

  public static UnsignedDecimal operator +(ulong a, UnsignedDecimal b) => new(a + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, ulong b) => new(a._value * b);

  public static UnsignedDecimal operator *(ulong a, UnsignedDecimal b) => new(a * b._value);

  #endregion

  #region UnsignedDecimal

  public static UnsignedDecimal operator +(UnsignedDecimal a, UnsignedDecimal b) => new(a._value + b._value);

  public static UnsignedDecimal operator *(UnsignedDecimal a, UnsignedDecimal b) => new(a._value * b._value);

  #endregion

  #endregion
}
