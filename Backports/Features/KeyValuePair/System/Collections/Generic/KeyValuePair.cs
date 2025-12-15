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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

#if !SUPPORTS_KEYVALUEPAIR_CREATE

// Note: KeyValuePair (non-generic static class) doesn't exist in older frameworks,
// so we define it directly rather than using extension syntax.
/// <summary>
/// Creates instances of the <see cref="KeyValuePair{TKey, TValue}"/> struct.
/// </summary>
public static class KeyValuePair {
  /// <summary>
  /// Creates a new key/value pair instance using the specified key and value.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  /// <param name="key">The key of the new <see cref="KeyValuePair{TKey, TValue}"/>.</param>
  /// <param name="value">The value of the new <see cref="KeyValuePair{TKey, TValue}"/>.</param>
  /// <returns>A new <see cref="KeyValuePair{TKey, TValue}"/> containing the specified key and value.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value)
    => new(key, value);
}

#endif


