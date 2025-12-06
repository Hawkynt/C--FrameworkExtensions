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

#if !SUPPORTS_RANDOM_GETITEMS

using Guard;

namespace System;

public static partial class RandomPolyfills {
  extension(Random @this) {
    /// <summary>
    /// Fills the elements of a specified span with items chosen at random from the provided set of choices.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="choices">The items to use to populate the span.</param>
    /// <param name="destination">The span to be filled with items.</param>
    /// <exception cref="ArgumentException"><paramref name="choices"/> is empty.</exception>
    public void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination) {
      if (choices.IsEmpty)
        AlwaysThrow.ArgumentException("The choices span is empty.", nameof(choices));

      for (var i = 0; i < destination.Length; ++i)
        destination[i] = choices[@this.Next(choices.Length)];
    }

    /// <summary>
    /// Creates an array populated with items chosen at random from the provided set of choices.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="choices">The items to use to populate the array.</param>
    /// <param name="length">The length of the array to return.</param>
    /// <returns>An array filled with random items from <paramref name="choices"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="choices"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="choices"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public T[] GetItems<T>(T[] choices, int length) {
      ArgumentNullException.ThrowIfNull(choices);
      if (choices.Length == 0)
        AlwaysThrow.ArgumentException("The choices array is empty.", nameof(choices));
      ArgumentOutOfRangeException.ThrowIfNegative(length);

      var result = new T[length];
      for (var i = 0; i < length; ++i)
        result[i] = choices[@this.Next(choices.Length)];

      return result;
    }
  }
}

#endif
