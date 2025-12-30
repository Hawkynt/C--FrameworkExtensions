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

#if !SUPPORTS_READONLYDICTIONARY_EMPTY

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.ObjectModel;

/// <summary>
/// Polyfills for <see cref="ReadOnlyDictionary{TKey, TValue}"/> Empty property added in .NET 8.0.
/// </summary>
public static partial class ReadOnlyDictionaryPolyfills {
  extension<TKey, TValue>(ReadOnlyDictionary<TKey, TValue>) where TKey : notnull {
    /// <summary>
    /// Gets an empty <see cref="ReadOnlyDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>An empty <see cref="ReadOnlyDictionary{TKey, TValue}"/>.</returns>
    public static ReadOnlyDictionary<TKey, TValue> Empty {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => EmptyReadOnlyDictionary<TKey, TValue>.Instance;
    }
  }

  private static class EmptyReadOnlyDictionary<TKey, TValue> where TKey : notnull {
    internal static readonly ReadOnlyDictionary<TKey, TValue> Instance = new(new Dictionary<TKey, TValue>());
  }
}

#endif
