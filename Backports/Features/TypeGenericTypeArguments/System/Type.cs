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

// Type.GenericTypeArguments was added in .NET Framework 4.5
// This provides a polyfill for .NET Framework 4.0 and earlier
#if !SUPPORTS_TYPE_GENERIC_TYPE_ARGUMENTS

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class TypePolyfills {

  extension(Type @this) {

    /// <summary>
    /// Gets an array of the generic type arguments for this type.
    /// </summary>
    /// <value>
    /// An array of the generic type arguments for this type.
    /// For a closed constructed type, returns the type arguments.
    /// For a generic type definition or non-generic type, returns an empty array.
    /// </value>
    public Type[] GenericTypeArguments {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => @this is { IsGenericType: true, IsGenericTypeDefinition: false }
        ? @this.GetGenericArguments()
        : Type.EmptyTypes;
    }

  }

}

#endif
