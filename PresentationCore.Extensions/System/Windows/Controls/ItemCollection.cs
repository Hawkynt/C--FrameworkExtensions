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

using System.Collections;
using System.Linq;
using Guard;

namespace System.Windows.Controls;

public static partial class ItemCollectionExtensions {
  /// <summary>
  ///   Gets the element in this collection by it's name tag.
  /// </summary>
  /// <param name="this">This ItemCollection.</param>
  /// <param name="name">The name.</param>
  /// <returns>The FrameworkElement that matches the given name or <c>null</c>.</returns>
  public static FrameworkElement GetElementByName(this ItemCollection @this, string name) {
    Against.ThisIsNull(@this);

    return @this.OfType<FrameworkElement>().FirstOrDefault(e => e.Name == name);
  }

  /// <summary>
  ///   Adds a bunch of items.
  /// </summary>
  /// <param name="this">This ItemCollection.</param>
  /// <param name="items">The items.</param>
  public static void AddRange(this ItemCollection @this, IEnumerable items) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(items);

    foreach (var item in items)
      @this.Add(item);
  }
}
