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

#if !SUPPORTS_JOIN_ENUMERABLES

using System.Collections.Generic;
using System.Text;

namespace System;

public static partial class StringPolyfills {

  extension(string) {

    /// <summary>
    /// Concatenates the members of a collection, using the specified separator between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="values">A collection that contains the objects to concatenate.</param>
    /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string.</returns>
    public static string Join<T>(string separator, IEnumerable<T> values) {
      ArgumentNullException.ThrowIfNull(values);
      var sb = new StringBuilder();
      var first = true;
      foreach (var value in values) {
        if (first)
          first = false;
        else
          sb.Append(separator);

        sb.Append(value);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
    /// using the specified separator between each member.
    /// </summary>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="values">A collection that contains the strings to concatenate.</param>
    /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string.</returns>
    public static string Join(string separator, IEnumerable<string> values) {
      ArgumentNullException.ThrowIfNull(values);
      var sb = new StringBuilder();
      var first = true;
      foreach (var value in values) {
        if (first)
          first = false;
        else
          sb.Append(separator);

        sb.Append(value);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Concatenates the elements of an object array, using the specified separator between each element.
    /// </summary>
    /// <param name="separator">The string to use as a separator.</param>
    /// <param name="values">An array that contains the elements to concatenate.</param>
    /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> string.</returns>
    public static string Join(string separator, params object[] values) {
      ArgumentNullException.ThrowIfNull(values);
      if (values.Length == 0)
        return string.Empty;

      var strings = new string?[values.Length];
      for (var i = 0; i < values.Length; ++i)
        strings[i] = values[i]?.ToString();

      return string.Join(separator, strings!);
    }

  }

}

#endif
