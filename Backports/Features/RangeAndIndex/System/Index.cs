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

#if !SUPPORTS_RANGE_AND_INDEX

using System.Diagnostics.CodeAnalysis;
using Guard;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public readonly struct Index : IEquatable<Index> {
  private readonly int _index;

  public static Index Start {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(0);
  }

  public static Index End {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(~0);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Index(int value, bool fromEnd) {
    if (value < 0)
      _ThrowValueNegative();

    this._index = fromEnd ? ~value : value;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  [DoesNotReturn]
  private static void _ThrowValueNegative() => AlwaysThrow.ArgumentOutOfRangeException("value", "value must be non-negative");

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private Index(int value) => this._index = value;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Index FromStart(int value) {
    if (value < 0)
      _ThrowValueNegative();

    return new(value);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Index FromEnd(int value) {
    if (value < 0)
      _ThrowValueNegative();

    return new(~value);
  }

  public int Value {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._index < 0 ? ~this._index : this._index;
  }

  public bool IsFromEnd {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this._index < 0;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int GetOffset(int length) {
    var offset = this._index;
    if (offset < 0)
      offset += length + 1;

    return offset;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object value) => value is Index index && this.Equals(index);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Index other) => this._index == other._index;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => this._index;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static implicit operator Index(int value) => FromStart(value);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => this._index < 0 ? "^" + ~this._index : this._index.ToString();

}

#endif
