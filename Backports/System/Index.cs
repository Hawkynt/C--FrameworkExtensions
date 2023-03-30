using System.Runtime.CompilerServices;

#if !SUPPORTS_RANGE_AND_INDEX

namespace System;

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