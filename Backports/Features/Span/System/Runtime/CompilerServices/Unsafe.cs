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

#if !SUPPORTS_SPAN

using MethodImplOptionsEx = Utilities.MethodImplOptions;

namespace System.Runtime.CompilerServices;

public static unsafe class Unsafe {

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  public static int SizeOf<T>() => sizeof(T);
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  internal static T* NullPtr<T>() => (T*)IntPtr.Zero;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
  public static ref T AsRef<T>(void* source) => ref *(T*)source;
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

  [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
  public static ref T NullRef<T>() => ref AsRef<T>(null);
  
}

#endif