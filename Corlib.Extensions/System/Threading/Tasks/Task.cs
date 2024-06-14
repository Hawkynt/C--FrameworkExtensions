#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if SUPPORTS_ASYNC

namespace System.Threading.Tasks;

public static partial class TaskExtensions {

  /// <summary>
  /// Gets the result or a default value.
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

#endif