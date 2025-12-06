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

#if !SUPPORTS_THROWIFNULLOREMPTY || !SUPPORTS_THROWIFNULLORWHITESPACE

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class ArgumentExceptionPolyfills {
  extension(ArgumentException) {

#if !SUPPORTS_THROWIFNULLOREMPTY

    /// <summary>
    /// Throws an exception if <paramref name="argument"/> is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
      if (string.IsNullOrEmpty(argument))
        _ThrowNullOrEmpty(argument, paramName);
    }

    [DoesNotReturn]
    private static void _ThrowNullOrEmpty(string? argument, string? paramName) {
      ArgumentNullException.ThrowIfNull(argument, paramName);
      throw new ArgumentException("The value cannot be an empty string.", paramName);
    }

#endif

#if !SUPPORTS_THROWIFNULLORWHITESPACE

    /// <summary>
    /// Throws an exception if <paramref name="argument"/> is <see langword="null"/>, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null) {
      if (_IsNullOrWhiteSpace(argument))
        _ThrowNullOrWhiteSpace(argument, paramName);
    }

    [DoesNotReturn]
    private static void _ThrowNullOrWhiteSpace(string? argument, string? paramName) {
      ArgumentNullException.ThrowIfNull(argument, paramName);
      throw new ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);
    }

#if SUPPORTS_STRING_IS_NULL_OR_WHITESPACE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool _IsNullOrWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value);
#else
    private static bool _IsNullOrWhiteSpace(string? value) {
      if (value is null)
        return true;

      for (var i = 0; i < value.Length; ++i)
        if (!char.IsWhiteSpace(value[i]))
          return false;

      return true;
    }
#endif

#endif

  }
}

#endif
