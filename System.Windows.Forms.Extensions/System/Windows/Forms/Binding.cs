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

namespace System.Windows.Forms;

public static partial class BindingExtensions {
  /// <summary>
  ///   Adds a value converter for a certain type of values.
  /// </summary>
  /// <typeparam name="TType">The type of the value to convert.</typeparam>
  /// <param name="this">This Binding.</param>
  /// <param name="converter">The converter.</param>
  public static void AddTypeConverter<TType>(this Binding @this, Func<object, TType> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    @this.Format += (_, e) => {
      if (e.DesiredType != typeof(TType))
        return;

      e.Value = converter(e.Value);
    };
  }
}
