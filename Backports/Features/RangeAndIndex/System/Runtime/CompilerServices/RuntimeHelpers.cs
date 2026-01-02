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

#if !SUPPORTS_RUNTIMEHELPERS_GETSUBARRAY

namespace System.Runtime.CompilerServices;

#pragma warning disable CS0436 // Type conflicts with imported type - intentional polyfill for compiler well-known members

/// <summary>
/// Provides compiler-required members for <see cref="RuntimeHelpers"/> on older frameworks.
/// </summary>
/// <remarks>
/// This partial class shadows the BCL's RuntimeHelpers to provide compiler well-known members
/// like GetSubArray (for range syntax) and OffsetToStringData (for fixed statements on strings).
/// These members MUST be on the actual type for the compiler to find them.
/// For external callers, prefer using the extension methods via <see cref="RuntimeHelpersPolyfills"/>.
/// </remarks>
public static partial class RuntimeHelpers {

  /// <summary>
  /// Gets the offset in bytes from the start of the string object to the first character.
  /// </summary>
  /// <value>The offset to string data.</value>
  /// <remarks>
  /// This property is used by the compiler for fixed statements on strings.
  /// On 32-bit systems this returns 8 (4-byte object header + 4-byte length).
  /// On 64-bit systems this returns 12 (8-byte object header + 4-byte length).
  /// </remarks>
  [Obsolete("Dummy for Compiler – DO NOT CALL!", true)]
  public static int OffsetToStringData => IntPtr.Size + sizeof(int);

  /// <summary>
  /// Slices the specified array using the specified range.
  /// </summary>
  /// <typeparam name="T">The type of elements in the array.</typeparam>
  /// <param name="array">The array to slice.</param>
  /// <param name="range">The range of elements to include.</param>
  /// <returns>A new array containing the specified range of elements.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// This method is called by the compiler when using range syntax on arrays (e.g., array[1..5]).
  /// </remarks>
  public static T[] GetSubArray<T>(T[] array, Range range) {
    ArgumentNullException.ThrowIfNull(array);

    var (offset, length) = range.GetOffsetAndLength(array.Length);
    if (length == 0)
      return [];

    var result = new T[length];
    Array.Copy(array, offset, result, 0, length);
    return result;
  }

}

#pragma warning restore CS0436

/// <summary>
/// Provides extension methods for <see cref="RuntimeHelpers"/> on older frameworks.
/// </summary>
public static partial class RuntimeHelpersPolyfills {

  extension(RuntimeHelpers) {

    /// <summary>
    /// Slices the specified array using the specified range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to slice.</param>
    /// <param name="range">The range of elements to include.</param>
    /// <returns>A new array containing the specified range of elements.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    public static T[] GetSubArray<T>(T[] array, Range range) => RuntimeHelpers.GetSubArray(array, range);

  }

}

#endif
