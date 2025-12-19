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

#if !SUPPORTS_QUEUE_ENSURECAPACITY

using System.Reflection;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Collections.Generic;

public static partial class QueuePolyfills {

  extension<T>(Queue<T> @this) {

    /// <summary>
    /// Ensures that the capacity of this queue is at least the specified <paramref name="capacity"/>.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this queue.</returns>
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

  private static readonly FieldInfo _arrayField = typeof(Queue<>).GetField("_array", BindingFlags.NonPublic | BindingFlags.Instance)
                                                  ?? typeof(Queue<>).GetField("array", BindingFlags.NonPublic | BindingFlags.Instance);

  private static readonly FieldInfo _headField = typeof(Queue<>).GetField("_head", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 ?? typeof(Queue<>).GetField("head", BindingFlags.NonPublic | BindingFlags.Instance);

  private static readonly FieldInfo _tailField = typeof(Queue<>).GetField("_tail", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 ?? typeof(Queue<>).GetField("tail", BindingFlags.NonPublic | BindingFlags.Instance);

  private static readonly FieldInfo _sizeField = typeof(Queue<>).GetField("_size", BindingFlags.NonPublic | BindingFlags.Instance)
                                                 ?? typeof(Queue<>).GetField("size", BindingFlags.NonPublic | BindingFlags.Instance);

  private static int _GetCapacity<T>(Queue<T> queue) {
    var arrayFieldTyped = _arrayField?.MakeGenericField(typeof(T));
    var array = arrayFieldTyped?.GetValue(queue) as T[];
    return array?.Length ?? queue.Count;
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

  private static void _SetCapacity<T>(Queue<T> queue, int capacity) {
    var arrayFieldTyped = _arrayField.DeclaringType!.MakeGenericType(typeof(T)).GetField(_arrayField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
    var headFieldTyped = _headField.DeclaringType!.MakeGenericType(typeof(T)).GetField(_headField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
    var tailFieldTyped = _tailField.DeclaringType!.MakeGenericType(typeof(T)).GetField(_tailField.Name, BindingFlags.NonPublic | BindingFlags.Instance);
    var sizeFieldTyped = _sizeField?.DeclaringType!.MakeGenericType(typeof(T)).GetField(_sizeField.Name, BindingFlags.NonPublic | BindingFlags.Instance);

    var oldArray = arrayFieldTyped!.GetValue(queue) as T[];
    var head = (int)headFieldTyped!.GetValue(queue)!;
    var size = sizeFieldTyped != null ? (int)sizeFieldTyped.GetValue(queue)! : queue.Count;

    var newArray = new T[capacity];

    if (size > 0 && oldArray != null) {
      if (head + size <= oldArray.Length)
        Array.Copy(oldArray, head, newArray, 0, size);
      else {
        // Wrap-around case: copy in two parts
        var firstPart = oldArray.Length - head;
        Array.Copy(oldArray, head, newArray, 0, firstPart);
        Array.Copy(oldArray, 0, newArray, firstPart, size - firstPart);
      }
    }

    arrayFieldTyped.SetValue(queue, newArray);
    headFieldTyped.SetValue(queue, 0);
    tailFieldTyped!.SetValue(queue, size == capacity ? 0 : size);
  }

}

file static class FieldInfoExtensions {
  public static FieldInfo? MakeGenericField(this FieldInfo field, Type typeArgument) {
    var declaringType = field.DeclaringType;
    if (declaringType is not { IsGenericTypeDefinition: true })
      return field;

    var genericType = declaringType.MakeGenericType(typeArgument);
    return genericType.GetField(field.Name, BindingFlags.NonPublic | BindingFlags.Instance);
  }
}

#endif
