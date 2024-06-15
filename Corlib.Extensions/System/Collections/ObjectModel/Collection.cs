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

using System.Collections.Generic;
using Guard;

namespace System.Collections.ObjectModel;

public static partial class CollectionExtensions {
  /// <summary>
  ///   Adds items to a collection.
  /// </summary>
  /// <typeparam name="TItem">The type of the items.</typeparam>
  /// <param name="this">This Collection.</param>
  /// <param name="items">The items.</param>
  public static void AddRange<TItem>(this Collection<TItem> @this, IEnumerable<TItem> items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Add(item);
  }
}
