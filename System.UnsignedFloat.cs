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
  public struct UnsignedFloat {
    private const string _errExceptionText = @"Value needs to be positive";
    private readonly float _value;

    public UnsignedFloat(float value) {
      if (value < 0)
        throw new ArgumentException(_errExceptionText);
      _value = value;
    }

    // This conversion is safe, we can make it implicit
    public static implicit operator float(UnsignedFloat value) {
      return (value._value);
    }

    // This conversion is not always safe, so we make it explicit
    public static explicit operator UnsignedFloat(float value) {
      if (value < 0)
        throw new ArgumentOutOfRangeException(_errExceptionText);
      return (new UnsignedFloat(value));
    }

    #region arithmetic
    #region byte
    public static implicit operator UnsignedFloat(byte value) {
      return (new UnsignedFloat(value));
    }

    public static UnsignedFloat operator +(UnsignedFloat a, byte b) {
      return (new UnsignedFloat(a._value + b));
    }

    public static UnsignedFloat operator +(byte a, UnsignedFloat b) {
      return (new UnsignedFloat(a + b._value));
    }

    public static UnsignedFloat operator *(UnsignedFloat a, byte b) {
      return (new UnsignedFloat(a._value * b));
    }

    public static UnsignedFloat operator *(byte a, UnsignedFloat b) {
      return (new UnsignedFloat(a * b._value));
    }
    #endregion
    #region char
    public static implicit operator UnsignedFloat(char value) {
      return (new UnsignedFloat(value));
    }

    public static UnsignedFloat operator +(UnsignedFloat a, char b) {
      return (new UnsignedFloat(a._value + b));
    }

    public static UnsignedFloat operator +(char a, UnsignedFloat b) {
      return (new UnsignedFloat(a + b._value));
    }

    public static UnsignedFloat operator *(UnsignedFloat a, char b) {
      return (new UnsignedFloat(a._value * b));
    }

    public static UnsignedFloat operator *(char a, UnsignedFloat b) {
      return (new UnsignedFloat(a * b._value));
    }
    #endregion
    #region ushort
    public static implicit operator UnsignedFloat(ushort value) {
      return (new UnsignedFloat(value));
    }

    public static UnsignedFloat operator +(UnsignedFloat a, ushort b) {
      return (new UnsignedFloat(a._value + b));
    }

    public static UnsignedFloat operator +(ushort a, UnsignedFloat b) {
      return (new UnsignedFloat(a + b._value));
    }

    public static UnsignedFloat operator *(UnsignedFloat a, ushort b) {
      return (new UnsignedFloat(a._value * b));
    }

    public static UnsignedFloat operator *(ushort a, UnsignedFloat b) {
      return (new UnsignedFloat(a * b._value));
    }
    #endregion
    #region uint
    public static implicit operator UnsignedFloat(uint value) {
      return (new UnsignedFloat(value));
    }

    public static UnsignedFloat operator +(UnsignedFloat a, uint b) {
      return (new UnsignedFloat(a._value + b));
    }

    public static UnsignedFloat operator +(uint a, UnsignedFloat b) {
      return (new UnsignedFloat(a + b._value));
    }

    public static UnsignedFloat operator *(UnsignedFloat a, uint b) {
      return (new UnsignedFloat(a._value * b));
    }

    public static UnsignedFloat operator *(uint a, UnsignedFloat b) {
      return (new UnsignedFloat(a * b._value));
    }
    #endregion
    #region ulong
    public static implicit operator UnsignedFloat(ulong value) {
      return (new UnsignedFloat(value));
    }

    public static UnsignedFloat operator +(UnsignedFloat a, ulong b) {
      return (new UnsignedFloat(a._value + b));
    }

    public static UnsignedFloat operator +(ulong a, UnsignedFloat b) {
      return (new UnsignedFloat(a + b._value));
    }

    public static UnsignedFloat operator *(UnsignedFloat a, ulong b) {
      return (new UnsignedFloat(a._value * b));
    }

    public static UnsignedFloat operator *(ulong a, UnsignedFloat b) {
      return (new UnsignedFloat(a * b._value));
    }
    #endregion
    #region UnsignedFloat
    public static UnsignedFloat operator +(UnsignedFloat a, UnsignedFloat b) {
      return (new UnsignedFloat(a._value + b._value));
    }
    public static UnsignedFloat operator *(UnsignedFloat a, UnsignedFloat b) {
      return (new UnsignedFloat(a._value * b._value));
    }
    #endregion

    #endregion

  }
}