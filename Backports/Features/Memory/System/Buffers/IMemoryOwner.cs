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
/// Identifies the owner of a block of memory who is responsible for disposing of the underlying memory appropriately.
/// </summary>
/// <typeparam name="T">The type of elements to store in memory.</typeparam>
public interface IMemoryOwner<T> : IDisposable {
  /// <summary>
  /// Gets the memory belonging to this owner.
  /// </summary>
  Memory<T> Memory { get; }
}

#endif
