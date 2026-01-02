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

namespace System.Numerics.Tensors;

/// <summary>
/// Represents a tensor that provides read and write access to its elements.
/// </summary>
/// <typeparam name="TSelf">The type that implements this interface.</typeparam>
/// <typeparam name="T">The type of elements in the tensor.</typeparam>
public interface ITensor<TSelf, T> : IReadOnlyTensor<TSelf, T>
  where TSelf : ITensor<TSelf, T> {

  /// <summary>Gets or sets the element at the specified indices.</summary>
  /// <param name="indices">The indices for each dimension.</param>
  new T this[params ReadOnlySpan<nint> indices] { get; set; }

  /// <summary>Sets the value at the specified indices.</summary>
  /// <param name="value">The value to set.</param>
  /// <param name="indices">The indices for each dimension.</param>
  void SetValue(T value, params ReadOnlySpan<nint> indices);

  /// <summary>Fills the tensor with the specified value.</summary>
  /// <param name="value">The value to fill with.</param>
  void Fill(T value);

  /// <summary>Clears the tensor, setting all elements to their default value.</summary>
  void Clear();

}

#endif
