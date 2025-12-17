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

#if !SUPPORTS_STRINGBUILDER_APPENDJOIN

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Text;

public static partial class StringBuilderPolyfills {

  extension(StringBuilder @this) {

    /// <summary>
    /// Concatenates the string representations of the elements in the provided array of objects,
    /// using the specified separator between each member, then appends the result to the current
    /// instance of the string builder.
    /// </summary>
    /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin(string separator, params object[] values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoin(@this, separator ?? string.Empty, values);
    }

    /// <summary>
    /// Concatenates the strings of the provided array, using the specified separator between each string,
    /// then appends the result to the current instance of the string builder.
    /// </summary>
    /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin(string separator, params string[] values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoin(@this, separator ?? string.Empty, values);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in the provided collection,
    /// using the specified separator between each member, then appends the result to the current
    /// instance of the string builder.
    /// </summary>
    /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
    /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin<T>(string separator, IEnumerable<T> values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoinEnumerable(@this, separator ?? string.Empty, values);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in the provided array of objects,
    /// using the specified char separator between each member, then appends the result to the current
    /// instance of the string builder.
    /// </summary>
    /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin(char separator, params object[] values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoin(@this, separator.ToString(), values);
    }

    /// <summary>
    /// Concatenates the strings of the provided array, using the specified char separator between each string,
    /// then appends the result to the current instance of the string builder.
    /// </summary>
    /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">An array that contains the strings to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin(char separator, params string[] values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoin(@this, separator.ToString(), values);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in the provided collection,
    /// using the specified char separator between each member, then appends the result to the current
    /// instance of the string builder.
    /// </summary>
    /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
    /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the joined strings only if <paramref name="values"/> has more than one element.</param>
    /// <param name="values">A collection that contains the objects to concatenate and append to the current instance of the string builder.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuilder AppendJoin<T>(char separator, IEnumerable<T> values) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(values);

      return _AppendJoinEnumerable(@this, separator.ToString(), values);
    }

  }

  private static StringBuilder _AppendJoin<T>(StringBuilder sb, string separator, T[] values) {
    if (values.Length == 0)
      return sb;

    sb.Append(values[0]);
    for (var i = 1; i < values.Length; ++i) {
      sb.Append(separator);
      sb.Append(values[i]);
    }

    return sb;
  }

  private static StringBuilder _AppendJoinEnumerable<T>(StringBuilder sb, string separator, IEnumerable<T> values) {
    using var enumerator = values.GetEnumerator();
    if (!enumerator.MoveNext())
      return sb;

    sb.Append(enumerator.Current);
    while (enumerator.MoveNext()) {
      sb.Append(separator);
      sb.Append(enumerator.Current);
    }

    return sb;
  }

}

#endif
