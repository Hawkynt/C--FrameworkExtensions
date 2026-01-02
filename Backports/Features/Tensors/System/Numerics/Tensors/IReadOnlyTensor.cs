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
//

#if !OFFICIAL_TENSORS

using System.Collections.Generic;

namespace System.Numerics.Tensors;

/// <summary>
/// Represents a read-only tensor that provides read access to its elements.
/// </summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
/// <typeparam name="T">The type of elements in the tensor.</typeparam>
public interface IReadOnlyTensor<TSelf, T> : IEnumerable<T>
  where TSelf : IReadOnlyTensor<TSelf, T> {

  /// <summary>Gets a value indicating whether this tensor is empty.</summary>
  bool IsEmpty { get; }

  /// <summary>Gets the number of dimensions (rank) of the tensor.</summary>
  int Rank { get; }

  /// <summary>Gets the total number of elements in the tensor.</summary>
  nint FlattenedLength { get; }

  /// <summary>Gets the lengths of each dimension.</summary>
  ReadOnlySpan<nint> Lengths { get; }

  /// <summary>Gets the strides for each dimension.</summary>
  ReadOnlySpan<nint> Strides { get; }

  /// <summary>Gets the element at the specified indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  T this[params ReadOnlySpan<nint> indices] { get; }

  /// <summary>Copies the contents of this tensor to a destination span.</summary>
  /// <param name="destination">The destination span.</param>
  void CopyTo(Span<T> destination);

  /// <summary>Gets the value at the specified indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  /// <returns>The value at the specified indices.</returns>
  T GetValue(params ReadOnlySpan<nint> indices);

  /// <summary>Gets the flat index for the specified multi-dimensional indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  /// <returns>The flat index.</returns>
  nint GetFlatIndex(params ReadOnlySpan<nint> indices);

}

#endif
