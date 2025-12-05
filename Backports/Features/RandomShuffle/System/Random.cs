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

#if !SUPPORTS_RANDOM_SHUFFLE

using Guard;

namespace System;

public static partial class RandomExtensions {
  extension(Random @this) {
    /// <summary>
    /// Performs an in-place shuffle of an array.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="values">The array to shuffle.</param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
    public void Shuffle<T>(T[] values) {
      if (values == null)
        AlwaysThrow.ArgumentNullException(nameof(values));

      var n = values.Length;
      for (var i = n - 1; i > 0; --i) {
        var j = @this.Next(i + 1);
        (values[i], values[j]) = (values[j], values[i]);
      }
    }

    /// <summary>
    /// Performs an in-place shuffle of a span.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="values">The span to shuffle.</param>
    public void Shuffle<T>(Span<T> values) {
      var n = values.Length;
      for (var i = n - 1; i > 0; --i) {
        var j = @this.Next(i + 1);
        (values[i], values[j]) = (values[j], values[i]);
      }
    }
  }
}

#endif
