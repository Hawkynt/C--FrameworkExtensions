﻿#region (c)2010-2042 Hawkynt

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
using System.Linq;

namespace System.Windows.Forms;

public static partial class BindingsCollectionExtensions {
  /// <summary>
  ///   Gets a binding by its bound property.
  /// </summary>
  /// <param name="this">This BindingCollection.</param>
  /// <param name="propertyName">Name of the property.</param>
  /// <returns>The first binding that matches the given property name.</returns>
  public static Binding GetBindingByPropertyName(this BindingsCollection @this, string propertyName) => @this.GetBindingsByPropertyName(propertyName).First();

  /// <summary>
  ///   Gets bindings by their bound property.
  /// </summary>
  /// <param name="this">This BindingCollection.</param>
  /// <param name="propertyName">Name of the property.</param>
  /// <returns>A list of bindings that match the given property name.</returns>
  public static IEnumerable<Binding> GetBindingsByPropertyName(this BindingsCollection @this, string propertyName) => @this.Cast<Binding>().Where(b => b.PropertyName == propertyName);
}
