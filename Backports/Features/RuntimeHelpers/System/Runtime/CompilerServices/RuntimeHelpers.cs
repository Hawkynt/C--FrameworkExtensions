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

// RuntimeHelpers.GetHashCode is available in BCL since .NET 1.1, but our other
// RuntimeHelpers polyfills may shadow the BCL class, so we need to provide this
// method in our partial class as well.
#if !SUPPORTS_RUNTIMEHELPERS_GETSUBARRAY

#pragma warning disable CS0436 // Type conflicts with imported type - intentional polyfill for compiler well-known members

namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides extension methods for <see cref="RuntimeHelpers"/> on older frameworks.
/// </summary>
public static partial class RuntimeHelpersPolyfills {

  extension(RuntimeHelpers) {

    /// <summary>
    /// Serves as a hash function for a particular object, and is suitable for use in
    /// algorithms and data structures that use hash codes, such as a hash table.
    /// </summary>
    /// <param name="o">An object to retrieve the hash code for.</param>
    /// <returns>
    /// A hash code for the object identified by the <paramref name="o"/> parameter,
    /// regardless of any instance method that the object's type might override.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The <see cref="GetHashCode(object)"/> method always calls the <see cref="object.GetHashCode"/>
    /// method non-virtually, even if the object's type has overridden the <see cref="object.GetHashCode"/>
    /// method.
    /// </para>
    /// <para>
    /// This polyfill implementation uses <see cref="object.GetHashCode"/> which may be overridden.
    /// However, this is acceptable for use cases like <see cref="ConditionalWeakTable{TKey,TValue}"/>
    /// where reference equality is confirmed after hash comparison.
    /// </para>
    /// </remarks>
    public static int GetHashCode(object? o) => o?.GetHashCode() ?? 0;

  }

}

#pragma warning restore CS0436

#endif
