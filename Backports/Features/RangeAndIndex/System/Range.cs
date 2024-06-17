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

using System.Runtime.CompilerServices;

namespace System;

public readonly struct Range(Index start, Index end) : IEquatable<Range> {
  public Index Start { get; } = start;
  public Index End { get; } = end;

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

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
