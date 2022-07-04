#region (c)2010-2020 Hawkynt
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
  public struct UnsignedDouble {
    private const string _errExceptionText = @"Value needs to be positive";
    private readonly double _value;

    public UnsignedDouble(double value) {
      if (value < 0)
        throw new ArgumentException(_errExceptionText);
      _value = value;
    }

    #region casts
    public static implicit operator double(UnsignedDouble value) {
      return (value._value);
    }

    // This conversion is not always safe, so we make it explicit
    public static explicit operator UnsignedDouble(double value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(_errExceptionText);
      return (new UnsignedDouble(value));
    }
    #endregion

    #region arithmetic
    #region byte
    public static implicit operator UnsignedDouble(byte value) {
      return (new UnsignedDouble(value));
    }

    public static UnsignedDouble operator +(UnsignedDouble a, byte b) {
      return (new UnsignedDouble(a._value + b));
    }

    public static UnsignedDouble operator +(byte a, UnsignedDouble b) {
      return (new UnsignedDouble(a + b._value));
    }

    public static UnsignedDouble operator *(UnsignedDouble a, byte b) {
      return (new UnsignedDouble(a._value * b));
    }

    public static UnsignedDouble operator *(byte a, UnsignedDouble b) {
      return (new UnsignedDouble(a * b._value));
    }
    #endregion
    #region char
    public static implicit operator UnsignedDouble(char value) {
      return (new UnsignedDouble(value));
    }

    public static UnsignedDouble operator +(UnsignedDouble a, char b) {
      return (new UnsignedDouble(a._value + b));
    }

    public static UnsignedDouble operator +(char a, UnsignedDouble b) {
      return (new UnsignedDouble(a + b._value));
    }

    public static UnsignedDouble operator *(UnsignedDouble a, char b) {
      return (new UnsignedDouble(a._value * b));
    }

    public static UnsignedDouble operator *(char a, UnsignedDouble b) {
      return (new UnsignedDouble(a * b._value));
    }
    #endregion
    #region ushort
    public static implicit operator UnsignedDouble(ushort value) {
      return (new UnsignedDouble(value));
    }

    public static UnsignedDouble operator +(UnsignedDouble a, ushort b) {
      return (new UnsignedDouble(a._value + b));
    }

    public static UnsignedDouble operator +(ushort a, UnsignedDouble b) {
      return (new UnsignedDouble(a + b._value));
    }

    public static UnsignedDouble operator *(UnsignedDouble a, ushort b) {
      return (new UnsignedDouble(a._value * b));
    }

    public static UnsignedDouble operator *(ushort a, UnsignedDouble b) {
      return (new UnsignedDouble(a * b._value));
    }
    #endregion
    #region uint
    public static implicit operator UnsignedDouble(uint value) {
      return (new UnsignedDouble(value));
    }

    public static UnsignedDouble operator +(UnsignedDouble a, uint b) {
      return (new UnsignedDouble(a._value + b));
    }

    public static UnsignedDouble operator +(uint a, UnsignedDouble b) {
      return (new UnsignedDouble(a + b._value));
    }

    public static UnsignedDouble operator *(UnsignedDouble a, uint b) {
      return (new UnsignedDouble(a._value * b));
    }

    public static UnsignedDouble operator *(uint a, UnsignedDouble b) {
      return (new UnsignedDouble(a * b._value));
    }
    #endregion
    #region ulong
    public static implicit operator UnsignedDouble(ulong value) {
      return (new UnsignedDouble(value));
    }

    public static UnsignedDouble operator +(UnsignedDouble a, ulong b) {
      return (new UnsignedDouble(a._value + b));
    }

    public static UnsignedDouble operator +(ulong a, UnsignedDouble b) {
      return (new UnsignedDouble(a + b._value));
    }

    public static UnsignedDouble operator *(UnsignedDouble a, ulong b) {
      return (new UnsignedDouble(a._value * b));
    }

    public static UnsignedDouble operator *(ulong a, UnsignedDouble b) {
      return (new UnsignedDouble(a * b._value));
    }
    #endregion
    #region UnsignedDouble
    public static UnsignedDouble operator +(UnsignedDouble a, UnsignedDouble b) {
      return (new UnsignedDouble(a._value + b._value));
    }
    public static UnsignedDouble operator *(UnsignedDouble a, UnsignedDouble b) {
      return (new UnsignedDouble(a._value * b._value));
    }
    #endregion
    #endregion

  }
}