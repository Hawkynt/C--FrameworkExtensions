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

#nullable enable

#if !SUPPORTS_STACK_ENSURECAPACITY

using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class StackPolyfills {

  extension<T>(Stack<T> @this) {

    /// <summary>
    /// Ensures that the capacity of this stack is at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this stack.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EnsureCapacity(int capacity) {
      ArgumentNullException.ThrowIfNull(@this);
      ArgumentOutOfRangeException.ThrowIfNegative(capacity);

      var currentCapacity = _GetCapacity(@this);
      if (currentCapacity >= capacity)
        return currentCapacity;

      var newCapacity = _CalculateNewCapacity(currentCapacity, capacity);
      _SetCapacity(@this, newCapacity);
      return newCapacity;
    }

  }

  private static readonly FieldInfo? _stackArrayField = typeof(Stack<>).GetField("_array", BindingFlags.NonPublic | BindingFlags.Instance)
                                                       ?? typeof(Stack<>).GetField("array", BindingFlags.NonPublic | BindingFlags.Instance);

  private static int _GetCapacity<T>(Stack<T> stack) {
    var arrayFieldTyped = _stackArrayField?.DeclaringType!.MakeGenericType(typeof(T)).GetField(_stackArrayField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
    var array = arrayFieldTyped?.GetValue(stack) as T[];
    return array?.Length ?? stack.Count;
  }

  private static int _CalculateNewCapacity(int currentCapacity, int minCapacity) {
    // Use the same growth strategy as .NET: double the capacity, but at least get to minCapacity
    var newCapacity = currentCapacity == 0 ? 4 : currentCapacity * 2;
    if ((uint)newCapacity > int.MaxValue)
      newCapacity = int.MaxValue;
    if (newCapacity < minCapacity)
      newCapacity = minCapacity;
    return newCapacity;
  }

  private static void _SetCapacity<T>(Stack<T> stack, int capacity) {
    var arrayFieldTyped = _stackArrayField!.DeclaringType!.MakeGenericType(typeof(T)).GetField(_stackArrayField.Name, BindingFlags.NonPublic | BindingFlags.Instance);

    var oldArray = arrayFieldTyped!.GetValue(stack) as T[];
    var size = stack.Count;

    var newArray = new T[capacity];

    if (size > 0 && oldArray != null)
      Array.Copy(oldArray, 0, newArray, 0, size);

    arrayFieldTyped.SetValue(stack, newArray);
  }

}

#endif
