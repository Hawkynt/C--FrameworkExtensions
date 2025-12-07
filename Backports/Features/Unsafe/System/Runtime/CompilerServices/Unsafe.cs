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
//

#if !SUPPORTS_UNSAFE && !OFFICIAL_UNSAFE

using Utilities;
using MethodImplOptionsEx = Utilities.MethodImplOptions;

namespace System.Runtime.CompilerServices;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

public static unsafe class Unsafe {

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void SkipInit<T>(out T result) => result = default;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static int SizeOf<T>() =>
    // sizeof(T) doesn't work reliably for generic type parameters in older .NET Framework
    // Use type switching for known primitive types, fall back to Marshal.SizeOf
    TypeCodeCache<T>.Code switch {
      CachedTypeCode.Byte or CachedTypeCode.SByte or CachedTypeCode.Boolean => 1,
      CachedTypeCode.UInt16 or CachedTypeCode.Int16 or CachedTypeCode.Char => 2,
      CachedTypeCode.UInt32 or CachedTypeCode.Int32 or CachedTypeCode.Single => 4,
      CachedTypeCode.UInt64 or CachedTypeCode.Int64 or CachedTypeCode.Double => 8,
      CachedTypeCode.Decimal => 16,
      CachedTypeCode.Pointer or CachedTypeCode.UPointer => IntPtr.Size,
      _ => InteropServices.Marshal.SizeOf(typeof(T))
    }
  ;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  internal static T* NullPtr<T>() => (T*)IntPtr.Zero;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T NullRef<T>() => ref AsRef<T>(null);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void* AsPointer<T>(ref T value) {
    fixed (T* ptr = &value)
      return ptr;
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static T Read<T>(void* source) => *(T*)source;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void Write<T>(void* destination, T value) => *(T*)destination = value;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static T ReadUnaligned<T>(ref byte source) {
    var ptr = AsPointer(ref source);
    return *(T*)ptr;
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static T ReadUnaligned<T>(void* source) => *(T*)source;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void WriteUnaligned<T>(ref byte destination, T value) {
    var ptr = AsPointer(ref destination);
    *(T*)ptr = value;
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void WriteUnaligned<T>(void* destination, T value) => *(T*)destination = value;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void InitBlock(void* startAddress, byte value, uint byteCount) => Utilities.RawMemory.Fill(value,(byte*)startAddress,byteCount);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount) => InitBlock(startAddress, value, byteCount);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void CopyBlock(void* destination, void* source, uint byteCount) => Utilities.RawMemory.CopyWithoutChecks((byte*)source, (byte*)destination, byteCount);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void CopyBlockUnaligned(void* destination, void* source, uint byteCount) => CopyBlock(destination, source, byteCount);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static T As<T>(object o) where T : class => (T)o;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T AsRef<T>(void* source) => ref *(T*)source;

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T AsRef<T>(in T source) {
    // Get pointer to the 'in' parameter directly - 'in' is passed by reference internally
    fixed (T* ptr = &source)
      return ref *ptr;
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref TTo As<TFrom, TTo>(ref TFrom source) => ref *(TTo*)AsPointer(ref source);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T Unbox<T>(object box) where T : struct => ref *(T*)AsPointer(ref box);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T Add<T>(ref T source, int elementOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr + elementOffset * SizeOf<T>());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void* Add<T>(void* source, int elementOffset) => (byte*)source + elementOffset * SizeOf<T>();

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T Add<T>(ref T source, IntPtr elementOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr + elementOffset.ToInt64() * SizeOf<T>());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr + byteOffset.ToInt64());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T Subtract<T>(ref T source, int elementOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr - elementOffset * SizeOf<T>());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static void* Subtract<T>(void* source, int elementOffset) => (byte*)source - elementOffset * SizeOf<T>();

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T Subtract<T>(ref T source, IntPtr elementOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr - elementOffset.ToInt64() * SizeOf<T>());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset) {
    var ptr = AsPointer(ref source);
    return ref *(T*)((byte*)ptr - byteOffset.ToInt64());
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static IntPtr ByteOffset<T>(ref T origin, ref T target) {
    var pOrigin = AsPointer(ref origin);
    var pTarget = AsPointer(ref target);
    return (IntPtr)((byte*)pTarget - (byte*)pOrigin);
  }

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static bool AreSame<T>(ref T left, ref T right) => AsPointer(ref left) == AsPointer(ref right);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static bool IsAddressGreaterThan<T>(ref T left, ref T right) => AsPointer(ref left) > AsPointer(ref right);

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static bool IsAddressLessThan<T>(ref T left, ref T right) => AsPointer(ref left) < AsPointer(ref right);

}

#endif