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

namespace System.Threading.Tasks;

public static partial class TaskExtensions {
  /// <summary>
  ///   Gets the result or a default value.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">This Task.</param>
  /// <param name="defaultValue">The default value.</param>
  /// <returns></returns>
  public static TResult GetResultOrDefault<TResult>(this Task<TResult> @this, TResult defaultValue = default) {
    if (@this.IsFaulted)
      return defaultValue;

    if (@this.IsCanceled)
      return defaultValue;

    try {
      return @this.Result;
    } catch {
      return defaultValue;
    }
  }
}
