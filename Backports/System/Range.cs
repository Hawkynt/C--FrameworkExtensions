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

using System.Runtime.CompilerServices;

namespace System;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
readonly struct Range : IEquatable<Range> {
  public Index Start { get; }
  public Index End { get; }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public Range(Index start, Index end) {
    this.Start = start;
    this.End = end;
  }

  public override bool Equals(object value) => value is Range r && this.Equals(r);
  public bool Equals(Range other) => other.Start.Equals(this.Start) && other.End.Equals(this.End);
  public override int GetHashCode() => this.Start.GetHashCode() * 31 + this.End.GetHashCode();
  public override string ToString() => this.Start + ".." + this.End;
  public static Range StartAt(Index start) => new(start, Index.End);
  public static Range EndAt(Index end) => new(Index.Start, end);
  public static Range All => new(Index.Start, Index.End);

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public (int Offset, int Length) GetOffsetAndLength(int length) {
    int start;
    var startIndex = this.Start;
    if (startIndex.IsFromEnd)
      start = length - startIndex.Value;
    else
      start = startIndex.Value;

    int end;
    var endIndex = this.End;
    if (endIndex.IsFromEnd)
      end = length - endIndex.Value;
    else
      end = endIndex.Value;

    if ((uint)end > (uint)length || (uint)start > (uint)end)
      _ThrowLengthOutOfRange();

    return (start, end - start);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
#if SUPPORTS_DOES_NOT_RETURN_ATTRIBUTE
  [DoestNotReturn]
#endif
  private static void _ThrowLengthOutOfRange() => throw new ArgumentOutOfRangeException("length");

}
#endif