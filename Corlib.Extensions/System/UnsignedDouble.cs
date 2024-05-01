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

namespace System;

public struct UnsignedDouble {
  private const string _errExceptionText = @"Value needs to be positive";
  private readonly double _value;

  public UnsignedDouble(double value) {
    if (value < 0)
      throw new ArgumentException(_errExceptionText);
    
    this._value = value;
  }

  #region casts
  public static implicit operator double(UnsignedDouble value) => value._value;

  // This conversion is not always safe, so we make it explicit
  public static explicit operator UnsignedDouble(double value) {
    if (value < 0)
      throw new ArgumentOutOfRangeException(_errExceptionText);
    return new(value);
  }
  #endregion

  #region arithmetic
  #region byte
  public static implicit operator UnsignedDouble(byte value) => new(value);

  public static UnsignedDouble operator +(UnsignedDouble a, byte b) => new(a._value + b);

  public static UnsignedDouble operator +(byte a, UnsignedDouble b) => new(a + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, byte b) => new(a._value * b);

  public static UnsignedDouble operator *(byte a, UnsignedDouble b) => new(a * b._value);

  #endregion
  #region char
  public static implicit operator UnsignedDouble(char value) => new(value);

  public static UnsignedDouble operator +(UnsignedDouble a, char b) => new(a._value + b);

  public static UnsignedDouble operator +(char a, UnsignedDouble b) => new(a + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, char b) => new(a._value * b);

  public static UnsignedDouble operator *(char a, UnsignedDouble b) => new(a * b._value);

  #endregion
  #region ushort
  public static implicit operator UnsignedDouble(ushort value) => new(value);

  public static UnsignedDouble operator +(UnsignedDouble a, ushort b) => new(a._value + b);

  public static UnsignedDouble operator +(ushort a, UnsignedDouble b) => new(a + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, ushort b) => new(a._value * b);

  public static UnsignedDouble operator *(ushort a, UnsignedDouble b) => new(a * b._value);

  #endregion
  #region uint
  public static implicit operator UnsignedDouble(uint value) => new(value);

  public static UnsignedDouble operator +(UnsignedDouble a, uint b) => new(a._value + b);

  public static UnsignedDouble operator +(uint a, UnsignedDouble b) => new(a + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, uint b) => new(a._value * b);

  public static UnsignedDouble operator *(uint a, UnsignedDouble b) => new(a * b._value);

  #endregion
  #region ulong
  public static implicit operator UnsignedDouble(ulong value) => new(value);

  public static UnsignedDouble operator +(UnsignedDouble a, ulong b) => new(a._value + b);

  public static UnsignedDouble operator +(ulong a, UnsignedDouble b) => new(a + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, ulong b) => new(a._value * b);

  public static UnsignedDouble operator *(ulong a, UnsignedDouble b) => new(a * b._value);

  #endregion
  #region UnsignedDouble
  public static UnsignedDouble operator +(UnsignedDouble a, UnsignedDouble b) => new(a._value + b._value);

  public static UnsignedDouble operator *(UnsignedDouble a, UnsignedDouble b) => new(a._value * b._value);

  #endregion
  #endregion

}