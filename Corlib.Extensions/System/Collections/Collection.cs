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

using Guard;
using System.Linq;
using System.Diagnostics;
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

namespace System.Collections; 

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class CollectionExtensions {

  /// <summary>
  /// Implements a faster shortcut for LINQ's .Any()
  /// </summary>
  /// <param name="this">This <see cref="ICollection"/></param>
  /// <returns><see langword="true"/> if there is at least one item in the <see cref="ICollection"/>; otherwise, <see langword="false"/>.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static bool Any(this ICollection @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

  /// <summary>
  /// Executes an action on each item.
  /// </summary>
  /// <param name="this">The <see cref="ICollection"/>.</param>
  /// <param name="call">The call to execute.</param>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static void ForEach(this ICollection @this, Action<object> call) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(call);

    foreach (var value in @this)
      call(value);
  }
  /// <summary>
  /// Converts all.
  /// </summary>
  /// <typeparam name="TOUT">The type of the output collection.</typeparam>
  /// <param name="this">The <see cref="ICollection"/> to convert.</param>
  /// <param name="converter">The converter.</param>
  /// <returns></returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static TOUT[] ConvertAll<TOUT>(this ICollection @this, Converter<object, TOUT> converter) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    return (
      from object data in @this
      select converter(data)
    ).ToArray();
  }

  /// <summary>
  /// Copies the collection into an array.
  /// </summary>
  /// <param name="this">This <see cref="ICollection"/>.</param>
  /// <returns>An array containing all elements.</returns>
  #if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  #endif
  [DebuggerStepThrough]
  public static object[] ToArray(this ICollection @this) {
    Against.ThisIsNull(@this);

    var len = @this.Count;
    var result = new object[len];
    @this.CopyTo(result, 0);
    return result;
  }

}