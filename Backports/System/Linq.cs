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

using System.Collections.Generic;

#if !SUPPORTS_LINQ

namespace System.Linq;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static partial class EnumerableExtensions {
  public static TResult[] ToArray<TResult>(this IEnumerable<TResult> @this) {
    switch (@this) {
      case ICollection<TResult> collection: {
        var result = new TResult[collection.Count];
        collection.CopyTo(result, 0);
        return result;
      }
      default: {
        var result = new TResult[64];
        var length = 0;

        foreach (var item in @this) {
          if (result.Length == length) {
            var next = new TResult[length + 128];
            Array.Copy(result, 0, next, 0, length);
            result = next;
          }

          result[length++] = item;
        }

        if (length != result.Length) {
          var next = new TResult[length];
          Array.Copy(result, 0, next, 0, length);
          result = next;
        }

        return result;
      }
    }
  }
}

#endif