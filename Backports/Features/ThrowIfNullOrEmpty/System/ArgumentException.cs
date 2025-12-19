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

#if !SUPPORTS_THROWIFNULLOREMPTY

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArgumentExceptionPolyfills {
  extension(ArgumentException) {

    /// <summary>
    /// Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
      if (_IsNullOrEmpty(argument))
        _ThrowNullOrEmpty(argument, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsNullOrEmpty([NotNullWhen(false)] string? value) => string.IsNullOrEmpty(value);

    [DoesNotReturn]
    private static void _ThrowNullOrEmpty(string? argument, string? paramName) {
      ArgumentNullException.ThrowIfNull(argument, paramName);
      throw new ArgumentException("The value cannot be an empty string.", paramName);
    }

  }
}

#endif
