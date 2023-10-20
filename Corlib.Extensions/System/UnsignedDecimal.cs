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
namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  struct UnsignedDecimal {
    private const string _errExceptionText = @"Value needs to be positive";
    private readonly decimal _value;

    public UnsignedDecimal(decimal value) {
      if (value < 0)
        throw new ArgumentException(_errExceptionText);
      _value = value;
    }

    // This conversion is safe, we can make it implicit
    public static implicit operator decimal(UnsignedDecimal value) {
      return (value._value);
    }

    // This conversion is not always safe, so we make it explicit
    public static explicit operator UnsignedDecimal(decimal value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(_errExceptionText);
      return (new(value));
    }

    #region arithmetic
    #region byte
    public static implicit operator UnsignedDecimal(byte value) {
      return (new(value));
    }

    public static UnsignedDecimal operator +(UnsignedDecimal a, byte b) {
      return (new(a._value + b));
    }

    public static UnsignedDecimal operator +(byte a, UnsignedDecimal b) {
      return (new(a + b._value));
    }

    public static UnsignedDecimal operator *(UnsignedDecimal a, byte b) {
      return (new(a._value * b));
    }

    public static UnsignedDecimal operator *(byte a, UnsignedDecimal b) {
      return (new(a * b._value));
    }
    #endregion
    #region char
    public static implicit operator UnsignedDecimal(char value) {
      return (new(value));
    }

    public static UnsignedDecimal operator +(UnsignedDecimal a, char b) {
      return (new(a._value + b));
    }

    public static UnsignedDecimal operator +(char a, UnsignedDecimal b) {
      return (new(a + b._value));
    }

    public static UnsignedDecimal operator *(UnsignedDecimal a, char b) {
      return (new(a._value * b));
    }

    public static UnsignedDecimal operator *(char a, UnsignedDecimal b) {
      return (new(a * b._value));
    }
    #endregion
    #region ushort
    public static implicit operator UnsignedDecimal(ushort value) {
      return (new(value));
    }

    public static UnsignedDecimal operator +(UnsignedDecimal a, ushort b) {
      return (new(a._value + b));
    }

    public static UnsignedDecimal operator +(ushort a, UnsignedDecimal b) {
      return (new(a + b._value));
    }

    public static UnsignedDecimal operator *(UnsignedDecimal a, ushort b) {
      return (new(a._value * b));
    }

    public static UnsignedDecimal operator *(ushort a, UnsignedDecimal b) {
      return (new(a * b._value));
    }
    #endregion
    #region uint
    public static implicit operator UnsignedDecimal(uint value) {
      return (new(value));
    }

    public static UnsignedDecimal operator +(UnsignedDecimal a, uint b) {
      return (new(a._value + b));
    }

    public static UnsignedDecimal operator +(uint a, UnsignedDecimal b) {
      return (new(a + b._value));
    }

    public static UnsignedDecimal operator *(UnsignedDecimal a, uint b) {
      return (new(a._value * b));
    }

    public static UnsignedDecimal operator *(uint a, UnsignedDecimal b) {
      return (new(a * b._value));
    }
    #endregion
    #region ulong
    public static implicit operator UnsignedDecimal(ulong value) {
      return (new(value));
    }

    public static UnsignedDecimal operator +(UnsignedDecimal a, ulong b) {
      return (new(a._value + b));
    }

    public static UnsignedDecimal operator +(ulong a, UnsignedDecimal b) {
      return (new(a + b._value));
    }

    public static UnsignedDecimal operator *(UnsignedDecimal a, ulong b) {
      return (new(a._value * b));
    }

    public static UnsignedDecimal operator *(ulong a, UnsignedDecimal b) {
      return (new(a * b._value));
    }
    #endregion
    #region UnsignedDecimal
    public static UnsignedDecimal operator +(UnsignedDecimal a, UnsignedDecimal b) {
      return (new(a._value + b._value));
    }
    public static UnsignedDecimal operator *(UnsignedDecimal a, UnsignedDecimal b) {
      return (new(a._value * b._value));
    }
    #endregion

    #endregion

  }
}