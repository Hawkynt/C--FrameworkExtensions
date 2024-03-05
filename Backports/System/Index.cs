#if !SUPPORTS_RANGE_AND_INDEX
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
using Runtime.CompilerServices;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
readonly struct Index:IEquatable<Index> {
  private readonly int _index;

  public static Index Start => new(0);
  public static Index End => new(~0);
  
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public Index(int value, bool fromEnd) {
    if (value < 0)
      _ThrowValueNegative();

    this._index = fromEnd ? ~value : value;
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
#if SUPPORTS_DOES_NOT_RETURN_ATTRIBUTE
  [DoestNotReturn]
#endif
  private static void _ThrowValueNegative()=> throw new ArgumentOutOfRangeException("value", "value must be non-negative");

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  private Index(int value) => this._index = value;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Index FromStart(int value) {
    if (value < 0) 
      _ThrowValueNegative();

    return new(value);
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static Index FromEnd(int value) {
    if (value < 0)
      _ThrowValueNegative();

    return new(~value);
  }

  public int Value => this._index < 0 ? ~this._index : this._index;
  public bool IsFromEnd=>this._index < 0;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public int GetOffset(int length) {
    var offset = this._index;
    if (offset<0)
      offset += length + 1;

    return offset;
  }

  public override bool Equals(object value) => value is Index index && this.Equals(index);
  public bool Equals(Index other) => this._index == other._index;
  public override int GetHashCode() => this._index;
  public static implicit operator Index(int value) => FromStart(value);
  public override string ToString() => this._index < 0 ? "^" + ~this._index : this._index.ToString();

}
#endif