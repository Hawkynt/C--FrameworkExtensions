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

namespace System.Windows.Controls.Primitives;

public static partial class SelectorExtensions {
  /// <summary>
  ///   Tries to cast the selected value into the given type.
  /// </summary>
  /// <typeparam name="TType">The type of value.</typeparam>
  /// <param name="this">This Selector.</param>
  /// <param name="value">The value.</param>
  /// <returns><c>true</c> on success; otherwise, <c>false</c>.</returns>
  public static bool TryCastSelectedValue<TType>(this Selector @this, ref TType value) {
    Against.ThisIsNull(@this);

    try {
      value = (TType)@this.SelectedValue;
      return true;
    } catch (InvalidCastException) {
      return false;
    }
  }
}
