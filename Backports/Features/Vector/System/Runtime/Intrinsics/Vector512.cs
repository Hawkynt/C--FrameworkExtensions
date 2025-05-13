// // This file is part of Hawkynt's .NET Framework extensions.
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

#if !SUPPORTS_VECTOR_512

namespace System.Runtime.Intrinsics;

public class Vector512 {
  public static bool IsHardwareAccelerated => false;
}

public struct Vector512<T> where T : unmanaged {
  public static Vector512<T> Zero => default;
  public static bool IsSupported => typeof(T) == typeof(byte)
                                    || typeof(T) == typeof(double)
                                    || typeof(T) == typeof(short)
                                    || typeof(T) == typeof(int)
                                    || typeof(T) == typeof(long)
                                    || typeof(T) == typeof(nint)
                                    || typeof(T) == typeof(sbyte)
                                    || typeof(T) == typeof(float)
                                    || typeof(T) == typeof(ushort)
                                    || typeof(T) == typeof(uint)
                                    || typeof(T) == typeof(ulong)
                                    || typeof(T) == typeof(nuint);
}

#endif