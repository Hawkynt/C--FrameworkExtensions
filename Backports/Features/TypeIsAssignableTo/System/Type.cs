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

#if !SUPPORTS_TYPE_ISASSIGNABLETO

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TypePolyfills {

  extension(Type @this) {

    /// <summary>
    /// Determines whether the current type can be assigned to a variable of the specified <paramref name="targetType"/>.
    /// </summary>
    /// <param name="targetType">The type to compare with the current type.</param>
    /// <returns><see langword="true"/> if any of the following conditions is true:
    /// <list type="bullet">
    /// <item><description>The current instance and <paramref name="targetType"/> represent the same type.</description></item>
    /// <item><description>The current type is derived either directly or indirectly from <paramref name="targetType"/>.</description></item>
    /// <item><description><paramref name="targetType"/> is an interface that the current type implements.</description></item>
    /// <item><description>The current type is a generic type parameter, and <paramref name="targetType"/> represents one of the constraints of the current type.</description></item>
    /// <item><description>The current type represents a value type, and <paramref name="targetType"/> represents <see cref="Nullable{T}"/> of the current type.</description></item>
    /// </list>
    /// <see langword="false"/> if none of these conditions are true, or if <paramref name="targetType"/> is <see langword="null"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAssignableTo(Type targetType)
      => targetType?.IsAssignableFrom(@this) ?? false;

  }

}

#endif
