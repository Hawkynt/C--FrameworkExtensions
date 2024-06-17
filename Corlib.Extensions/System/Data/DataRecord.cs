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

namespace System.Data;

public static partial class DataRecordExtensions {
  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TType">The type of the value.</typeparam>
  /// <param name="this">The data record itself.</param>
  /// <param name="fieldName">Name of the field.</param>
  /// <returns>The value from the database or the default value.</returns>
  public static TType GetValueOrDefault<TType>(this IDataRecord @this, string fieldName) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(fieldName);

    var value = @this[fieldName];
    return value is DBNull or null ? default : (TType)value;
  }

  /// <summary>
  ///   Gets the value or default.
  /// </summary>
  /// <typeparam name="TType">The type of the value.</typeparam>
  /// <param name="this">The data record itself.</param>
  /// <param name="fieldName">Name of the field.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns>
  ///   The value from the database or the default value.
  /// </returns>
  public static TType GetValueOrDefault<TType>(this IDataRecord @this, string fieldName, TType defaultValue) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(fieldName);

    var value = @this[fieldName];
    return value is DBNull or null ? defaultValue : (TType)value;
  }
}
