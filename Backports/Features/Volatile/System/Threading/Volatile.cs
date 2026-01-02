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

// Volatile class was introduced in .NET Framework 4.5
#if !SUPPORTS_INLINING

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

/// <summary>
/// Contains methods for performing volatile memory operations.
/// </summary>
/// <remarks>
/// On uniprocessor systems, volatile reads and writes ensure that a value is read or written
/// to memory at any point in time, and is not cached. On multiprocessor systems, it additionally
/// ensures that the memory operations are completed before any subsequent memory operations.
/// </remarks>
public static class Volatile {

  #region Read Methods

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations as follows:
  /// If a read or write appears after this method in the code, the processor cannot move it before this method.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static bool Read(ref bool location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static byte Read(ref byte location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static double Read(ref double location) {
    var value = Interlocked.CompareExchange(ref location, 0.0, 0.0);
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static short Read(ref short location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static int Read(ref int location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static long Read(ref long location) {
    var value = Interlocked.CompareExchange(ref location, 0L, 0L);
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static IntPtr Read(ref IntPtr location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static sbyte Read(ref sbyte location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static float Read(ref float location) {
    var value = Interlocked.CompareExchange(ref location, 0f, 0f);
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static ushort Read(ref ushort location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static uint Read(ref uint location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static ulong Read(ref ulong location) {
    // ulong is 64 bits, need atomic read on 32-bit systems
    // Use trick: read long atomically and reinterpret
    unsafe {
      fixed (ulong* ptr = &location) {
        var longPtr = (long*)ptr;
        var value = Interlocked.CompareExchange(ref *longPtr, 0L, 0L);
        return (ulong)value;
      }
    }
  }

  /// <summary>
  /// Reads the value of the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <param name="location">The field to read.</param>
  /// <returns>The value that was read.</returns>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static UIntPtr Read(ref UIntPtr location) {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  /// <summary>
  /// Reads the object reference from the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations.
  /// </summary>
  /// <typeparam name="T">The type of field to read. This must be a reference type.</typeparam>
  /// <param name="location">The field to read.</param>
  /// <returns>The reference to the object that was read.</returns>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static T Read<T>(ref T location) where T : class? {
    var value = location;
    Thread.MemoryBarrier();
    return value;
  }

  #endregion

  #region Write Methods

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier
  /// that prevents the processor from reordering memory operations as follows:
  /// If a read or write appears before this method in the code, the processor cannot move it after this method.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref bool location, bool value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref byte location, byte value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref double location, double value) {
    Interlocked.Exchange(ref location, value);
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref short location, short value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref int location, int value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref long location, long value) {
    Interlocked.Exchange(ref location, value);
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref IntPtr location, IntPtr value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref sbyte location, sbyte value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref float location, float value) {
    Interlocked.Exchange(ref location, value);
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref ushort location, ushort value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref uint location, uint value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref ulong location, ulong value) {
    // ulong is 64 bits, need atomic write on 32-bit systems
    unsafe {
      fixed (ulong* ptr = &location) {
        var longPtr = (long*)ptr;
        Interlocked.Exchange(ref *longPtr, (long)value);
      }
    }
  }

  /// <summary>
  /// Writes the specified value to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <param name="location">The field where the value is written.</param>
  /// <param name="value">The value to write.</param>
  [CLSCompliant(false)]
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write(ref UIntPtr location, UIntPtr value) {
    Thread.MemoryBarrier();
    location = value;
  }

  /// <summary>
  /// Writes the specified object reference to the specified field. On systems that require it, inserts a memory barrier.
  /// </summary>
  /// <typeparam name="T">The type of field to write. This must be a reference type.</typeparam>
  /// <param name="location">The field where the object reference is written.</param>
  /// <param name="value">The object reference to write.</param>
  [MethodImpl(MethodImplOptions.NoInlining)]
  public static void Write<T>(ref T location, T value) where T : class? {
    Thread.MemoryBarrier();
    location = value;
  }

  #endregion

}

#endif
