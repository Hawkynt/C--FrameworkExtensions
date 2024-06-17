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

namespace System.Collections.Specialized;

public static partial class StringDictionaryExtensions {
  /// <summary>
  ///   Adds or updates the specified key.
  /// </summary>
  /// <param name="this">This StringDictionary.</param>
  /// <param name="key">The key.</param>
  /// <param name="value">The value.</param>
  public static void AddOrUpdate(this StringDictionary @this, string key, string value) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(key);

    if (@this.ContainsKey(key))
      @this[key] = value;
    else
      @this.Add(key, value);
  }
}
