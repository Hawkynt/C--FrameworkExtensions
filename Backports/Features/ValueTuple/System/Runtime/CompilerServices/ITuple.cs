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

#if !SUPPORTS_VALUE_TUPLE

namespace System.Runtime.CompilerServices;

/// <summary>
///   This interface is required for types that want to be indexed into by dynamic patterns.
/// </summary>
public interface ITuple {
  /// <summary>
  ///   The number of positions in this data structure.
  /// </summary>
  int Length { get; }

  /// <summary>
  ///   Get the element at position
  ///   <param name="index" />
  ///   .
  /// </summary>
  object this[int index] { get; }
}

#endif
