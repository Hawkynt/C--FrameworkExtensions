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

using System.Runtime.Intrinsics;

namespace Backports.Tests;

/// <summary>
/// Provides framework-agnostic helper methods for working with Vector types in tests.
/// This class abstracts differences between .NET framework versions, particularly
/// for the One property which is not available in all framework versions.
/// </summary>
public static class VectorTestHelpers {

  /// <summary>
  /// Gets a Vector64&lt;T&gt; with all elements set to one.
  /// </summary>
  /// <typeparam name="T">The vector element type.</typeparam>
  /// <returns>A Vector64&lt;T&gt; with all elements set to one.</returns>
  public static Vector64<T> GetOne64<T>() where T : struct {
    if (typeof(T) == typeof(byte))
      return (Vector64<T>)(object)Vector64.Create((byte)1);
    if (typeof(T) == typeof(sbyte))
      return (Vector64<T>)(object)Vector64.Create((sbyte)1);
    if (typeof(T) == typeof(short))
      return (Vector64<T>)(object)Vector64.Create((short)1);
    if (typeof(T) == typeof(ushort))
      return (Vector64<T>)(object)Vector64.Create((ushort)1);
    if (typeof(T) == typeof(int))
      return (Vector64<T>)(object)Vector64.Create(1);
    if (typeof(T) == typeof(uint))
      return (Vector64<T>)(object)Vector64.Create(1u);
    if (typeof(T) == typeof(long))
      return (Vector64<T>)(object)Vector64.Create(1L);
    if (typeof(T) == typeof(ulong))
      return (Vector64<T>)(object)Vector64.Create(1UL);
    if (typeof(T) == typeof(float))
      return (Vector64<T>)(object)Vector64.Create(1.0f);
    if (typeof(T) == typeof(double))
      return (Vector64<T>)(object)Vector64.Create(1.0);

    throw new System.NotSupportedException($"Type {typeof(T).Name} is not supported for Vector64<T>.One");
  }

  /// <summary>
  /// Gets a Vector128&lt;T&gt; with all elements set to one.
  /// </summary>
  /// <typeparam name="T">The vector element type.</typeparam>
  /// <returns>A Vector128&lt;T&gt; with all elements set to one.</returns>
  public static Vector128<T> GetOne128<T>() where T : struct {
    if (typeof(T) == typeof(byte))
      return (Vector128<T>)(object)Vector128.Create((byte)1);
    if (typeof(T) == typeof(sbyte))
      return (Vector128<T>)(object)Vector128.Create((sbyte)1);
    if (typeof(T) == typeof(short))
      return (Vector128<T>)(object)Vector128.Create((short)1);
    if (typeof(T) == typeof(ushort))
      return (Vector128<T>)(object)Vector128.Create((ushort)1);
    if (typeof(T) == typeof(int))
      return (Vector128<T>)(object)Vector128.Create(1);
    if (typeof(T) == typeof(uint))
      return (Vector128<T>)(object)Vector128.Create(1u);
    if (typeof(T) == typeof(long))
      return (Vector128<T>)(object)Vector128.Create(1L);
    if (typeof(T) == typeof(ulong))
      return (Vector128<T>)(object)Vector128.Create(1UL);
    if (typeof(T) == typeof(float))
      return (Vector128<T>)(object)Vector128.Create(1.0f);
    if (typeof(T) == typeof(double))
      return (Vector128<T>)(object)Vector128.Create(1.0);

    throw new System.NotSupportedException($"Type {typeof(T).Name} is not supported for Vector128<T>.One");
  }

  /// <summary>
  /// Gets a Vector256&lt;T&gt; with all elements set to one.
  /// </summary>
  /// <typeparam name="T">The vector element type.</typeparam>
  /// <returns>A Vector256&lt;T&gt; with all elements set to one.</returns>
  public static Vector256<T> GetOne256<T>() where T : struct {
    if (typeof(T) == typeof(byte))
      return (Vector256<T>)(object)Vector256.Create((byte)1);
    if (typeof(T) == typeof(sbyte))
      return (Vector256<T>)(object)Vector256.Create((sbyte)1);
    if (typeof(T) == typeof(short))
      return (Vector256<T>)(object)Vector256.Create((short)1);
    if (typeof(T) == typeof(ushort))
      return (Vector256<T>)(object)Vector256.Create((ushort)1);
    if (typeof(T) == typeof(int))
      return (Vector256<T>)(object)Vector256.Create(1);
    if (typeof(T) == typeof(uint))
      return (Vector256<T>)(object)Vector256.Create(1u);
    if (typeof(T) == typeof(long))
      return (Vector256<T>)(object)Vector256.Create(1L);
    if (typeof(T) == typeof(ulong))
      return (Vector256<T>)(object)Vector256.Create(1UL);
    if (typeof(T) == typeof(float))
      return (Vector256<T>)(object)Vector256.Create(1.0f);
    if (typeof(T) == typeof(double))
      return (Vector256<T>)(object)Vector256.Create(1.0);

    throw new System.NotSupportedException($"Type {typeof(T).Name} is not supported for Vector256<T>.One");
  }
}
