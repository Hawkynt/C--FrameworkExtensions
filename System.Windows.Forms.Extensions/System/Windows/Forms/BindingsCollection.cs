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
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class BindingsCollectionExtensions {

  /// <summary>
  /// Retrieves the first binding in the <see cref="System.Windows.Forms.BindingsCollection"/> that matches the specified property name.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.BindingsCollection"/> instance.</param>
  /// <param name="propertyName">The name of the property to match.</param>
  /// <returns>The first <see cref="System.Windows.Forms.Binding"/> that matches the specified property name.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="propertyName"/> is <see langword="null"/> or white space.</exception>
  /// <exception cref="System.InvalidOperationException">Thrown if no binding matches the specified property name.</exception>
  /// <example>
  /// <code>
  /// BindingsCollection bindings = new BindingsCollection();
  /// // Assuming bindings have been added to the collection
  /// string propertyName = "Text";
  /// Binding binding = bindings.GetBindingByPropertyName(propertyName);
  /// Console.WriteLine(binding.PropertyName);
  /// </code>
  /// </example>
  public static Binding GetBindingByPropertyName(this BindingsCollection @this, string propertyName) => GetBindingsByPropertyName(@this, propertyName).First();

  /// <summary>
  /// Retrieves all bindings in the <see cref="System.Windows.Forms.BindingsCollection"/> that match the specified property name.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.BindingsCollection"/> instance.</param>
  /// <param name="propertyName">The name of the property to match.</param>
  /// <returns>An <see cref="System.Collections.Generic.IEnumerable{Binding}"/> containing all bindings that match the specified property name.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="propertyName"/> is <see langword="null"/> or white space.</exception>
  /// <example>
  /// <code>
  /// BindingsCollection bindings = new BindingsCollection();
  /// // Assuming bindings have been added to the collection
  /// string propertyName = "Text";
  /// IEnumerable&lt;Binding&gt; matchedBindings = bindings.GetBindingsByPropertyName(propertyName);
  /// foreach (var binding in matchedBindings)
  /// {
  ///     Console.WriteLine(binding.PropertyName);
  /// }
  /// </code>
  /// </example>
  public static IEnumerable<Binding> GetBindingsByPropertyName(this BindingsCollection @this, string propertyName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrWhiteSpace(propertyName);

    return @this.Cast<Binding>().Where(b => b.PropertyName == propertyName);
  }
}
