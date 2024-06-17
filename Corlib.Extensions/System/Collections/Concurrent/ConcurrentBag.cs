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

using Guard;

#if SUPPORTS_CONCURRENT_COLLECTIONS

namespace System.Collections.Concurrent;

public static partial class ConcurrentBagExtensions {
  /// <summary>
  ///   Clears the specified Bag.
  /// </summary>
  /// <typeparam name="TValue">The type of the values.</typeparam>
  /// <param name="this">This <see cref="ConcurrentBag{T}" /></param>
  public static void Clear<TValue>(this ConcurrentBag<TValue> @this) {
    Against.ThisIsNull(@this);

    while (@this.TryTake(out _)) { }
  }
}

#endif
