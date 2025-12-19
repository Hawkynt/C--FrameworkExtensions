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

#if !SUPPORTS_MEMORY && !OFFICIAL_MEMORY

namespace System.Buffers;

/// <summary>
/// Represents a linked list node that contains a memory segment.
/// </summary>
/// <typeparam name="T">The type of the elements in the memory segment.</typeparam>
public abstract class ReadOnlySequenceSegment<T> {
  /// <summary>
  /// Gets or sets the memory block for this segment.
  /// </summary>
  public ReadOnlyMemory<T> Memory { get; protected set; }

  /// <summary>
  /// Gets or sets the next segment in the linked list.
  /// </summary>
  public ReadOnlySequenceSegment<T>? Next { get; protected set; }

  /// <summary>
  /// Gets or sets the running index of this segment.
  /// </summary>
  public long RunningIndex { get; protected set; }
}

#endif
