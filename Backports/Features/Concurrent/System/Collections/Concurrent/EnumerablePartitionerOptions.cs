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

#if !SUPPORTS_ENUMERABLE_PARTITIONER_OPTIONS

namespace System.Collections.Concurrent;

/// <summary>
/// Specifies options to control the buffering behavior of a partitioner.
/// </summary>
[Flags]
public enum EnumerablePartitionerOptions {
  /// <summary>
  /// Use the default behavior, which is to use buffering to achieve optimal performance.
  /// </summary>
  None = 0,

  /// <summary>
  /// Create a partitioner that takes items from the source enumerable one at a time
  /// and does not use intermediate storage that can be accessed more efficiently by multiple threads.
  /// This option provides support for low latency (items will be processed as soon as they are available)
  /// and provides partial support for dependencies between items (a thread cannot deadlock
  /// waiting for an item that the thread itself is responsible for processing).
  /// </summary>
  NoBuffering = 1,
}

#endif
