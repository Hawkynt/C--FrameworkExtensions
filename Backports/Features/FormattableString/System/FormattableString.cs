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

// FormattableString was added in .NET Framework 4.6
// This provides a polyfill for C# 6+ string interpolation on older frameworks
#if !SUPPORTS_FORMATTABLESTRING

using System.Globalization;

namespace System {

  /// <summary>
  /// Represents a composite format string, along with the arguments to be formatted.
  /// </summary>
  /// <remarks>
  /// A <see cref="FormattableString"/> instance may result from an interpolated string in C# or Visual Basic.
  /// </remarks>
  public abstract class FormattableString : IFormattable {

    /// <summary>
    /// Initializes a new instance of the <see cref="FormattableString"/> class.
    /// </summary>
    protected FormattableString() { }

    /// <summary>
    /// Gets the format string.
    /// </summary>
    /// <value>The composite format string.</value>
    public abstract string Format { get; }

    /// <summary>
    /// Gets the number of arguments to be formatted.
    /// </summary>
    /// <value>The number of arguments to be formatted.</value>
    public abstract int ArgumentCount { get; }

    /// <summary>
    /// Returns the specified argument.
    /// </summary>
    /// <param name="index">The index of the argument.</param>
    /// <returns>The argument at the specified index.</returns>
    public abstract object? GetArgument(int index);

    /// <summary>
    /// Returns an object array that contains all arguments.
    /// </summary>
    /// <returns>An object array that contains all arguments.</returns>
    public abstract object?[] GetArguments();

    /// <summary>
    /// Returns the string that results from formatting the format string along with its arguments
    /// by using the formatting conventions of a specified culture.
    /// </summary>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <returns>A string formatted using the specified formatting conventions.</returns>
    public abstract string ToString(IFormatProvider? formatProvider);

    /// <summary>
    /// Returns the string that results from formatting the composite format string along with its arguments
    /// by using the formatting conventions of the current culture.
    /// </summary>
    /// <returns>A result string formatted by using the conventions of the current culture.</returns>
    public override string ToString() => this.ToString(CultureInfo.CurrentCulture);

    /// <summary>
    /// Returns the string that results from formatting the format string along with its arguments
    /// by using the formatting conventions of a specified culture.
    /// </summary>
    /// <param name="format">Not used.</param>
    /// <param name="formatProvider">An object that provides culture-specific formatting information.</param>
    /// <returns>A string formatted using the specified formatting conventions.</returns>
    string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => this.ToString(formatProvider);

    /// <summary>
    /// Returns the string that results from formatting the format string along with its arguments
    /// by using the formatting conventions of the current culture.
    /// </summary>
    /// <param name="formattable">The object to convert to a result string.</param>
    /// <returns>The string that results from formatting the format string along with its arguments
    /// by using the formatting conventions of the current culture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formattable"/> is <see langword="null"/>.</exception>
    public static string CurrentCulture(FormattableString formattable) {
      ArgumentNullException.ThrowIfNull(formattable);
      return formattable.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Returns a result string in which arguments are formatted by using the conventions
    /// of the invariant culture.
    /// </summary>
    /// <param name="formattable">The object to convert to a result string.</param>
    /// <returns>The string that results from formatting the current instance by using the
    /// conventions of the invariant culture.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="formattable"/> is <see langword="null"/>.</exception>
    public static string Invariant(FormattableString formattable) {
      ArgumentNullException.ThrowIfNull(formattable);
      return formattable.ToString(CultureInfo.InvariantCulture);
    }

  }

  /// <summary>
  /// A concrete FormattableString implementation used by the compiler.
  /// </summary>
  internal sealed class ConcreteFormattableString(string format, object?[] arguments) : FormattableString {

    /// <inheritdoc />
    public override string Format => format;

    /// <inheritdoc />
    public override int ArgumentCount => arguments.Length;

    /// <inheritdoc />
    public override object? GetArgument(int index) => arguments[index];

    /// <inheritdoc />
    public override object?[] GetArguments() => arguments;

    /// <inheritdoc />
    public override string ToString(IFormatProvider? formatProvider) => string.Format(formatProvider, format, arguments);

  }

}

namespace System.Runtime.CompilerServices {

  /// <summary>
  /// A factory type used by compilers to create instances of <see cref="FormattableString"/>.
  /// </summary>
  public static class FormattableStringFactory {

    /// <summary>
    /// Creates a <see cref="FormattableString"/> instance from a composite format string and its arguments.
    /// </summary>
    /// <param name="format">The composite format string.</param>
    /// <param name="arguments">The arguments to be formatted.</param>
    /// <returns>A <see cref="FormattableString"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="format"/> or <paramref name="arguments"/> is <see langword="null"/>.</exception>
    public static FormattableString Create(string format, params object?[] arguments) {
      ArgumentNullException.ThrowIfNull(format);
      ArgumentNullException.ThrowIfNull(arguments);
      return new ConcreteFormattableString(format, arguments);
    }

  }

}

#endif
