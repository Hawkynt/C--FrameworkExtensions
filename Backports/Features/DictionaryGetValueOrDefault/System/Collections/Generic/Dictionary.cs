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

// Dictionary.GetValueOrDefault was added in .NET Core 2.0 / .NET Standard 2.1
#if !SUPPORTS_DICTIONARY_TRYADD

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class DictionaryPolyfills {

  extension<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> @this) {

    /// <summary>
    /// Gets the value associated with the specified key, or a default value if the key is not found.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value associated with the specified key if the key is found; otherwise, the default value for type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue? GetValueOrDefault(TKey key) {
      Against.ThisIsNull(@this);
      return @this.TryGetValue(key, out var value) ? value : default;
    }

    /// <summary>
    /// Gets the value associated with the specified key, or a specified default value if the key is not found.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="defaultValue">The default value to return when the key is not found.</param>
    /// <returns>The value associated with the specified key if the key is found; otherwise, <paramref name="defaultValue"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetValueOrDefault(TKey key, TValue defaultValue) {
      Against.ThisIsNull(@this);
      return @this.TryGetValue(key, out var value) ? value : defaultValue;
    }

  }

}

#endif
