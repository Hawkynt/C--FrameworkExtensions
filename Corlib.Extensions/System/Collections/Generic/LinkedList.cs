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
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif

// ReSharper disable UnusedMember.Global
// ReSharper disable PartialTypeWithSinglePart
namespace System.Collections.Generic;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  static partial class LinkedListExtensions {

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool Any<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    return @this.Count > 0;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Enqueue<T>(this LinkedList<T> @this, T value) {
    Against.ThisIsNull(@this);

    @this.AddLast(value);
  }

  public static T Dequeue<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Queue");

    @this.RemoveFirst();
    return result.Value;
  }


  public static T Peek<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Queue/Stack");

    return result.Value;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static void Push<T>(this LinkedList<T> @this, T value) {
    Against.ThisIsNull(@this);

    @this.AddFirst(value);
  }

  public static T Pop<T>(this LinkedList<T> @this) {
    Against.ThisIsNull(@this);

    var result = @this.First;
    if (result == null)
      AlwaysThrow.InvalidOperationException("Empty Stack");

    @this.RemoveFirst();
    return result.Value;
  }

  public static bool TryPop<T>(this LinkedList<T> @this, out T result) {

    var node = @this.First;
    if (node == null) {
      result = default;
      return false;
    }

    @this.RemoveFirst();
    result = node.Value;
    return true;
  }

  public static bool TryPeek<T>(this LinkedList<T> @this, out T result) {

    var node = @this.First;
    if (node == null) {
      result = default;
      return false;
    }

    result = node.Value;
    return true;
  }

#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool TryDequeue<T>(this LinkedList<T> @this, out T result) => TryPop(@this, out result);

}
