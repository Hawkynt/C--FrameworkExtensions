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

#if !SUPPORTS_STRING_SYNTAX_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies the syntax used in a string.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class StringSyntaxAttribute : Attribute {
  /// <summary>
  /// The syntax identifier for strings containing composite formats for string formatting.
  /// </summary>
  public const string CompositeFormat = nameof(CompositeFormat);

  /// <summary>
  /// The syntax identifier for strings containing date format specifiers.
  /// </summary>
  public const string DateOnlyFormat = nameof(DateOnlyFormat);

  /// <summary>
  /// The syntax identifier for strings containing date and time format specifiers.
  /// </summary>
  public const string DateTimeFormat = nameof(DateTimeFormat);

  /// <summary>
  /// The syntax identifier for strings containing <see cref="Enum"/> format specifiers.
  /// </summary>
  public const string EnumFormat = nameof(EnumFormat);

  /// <summary>
  /// The syntax identifier for strings containing <see cref="Guid"/> format specifiers.
  /// </summary>
  public const string GuidFormat = nameof(GuidFormat);

  /// <summary>
  /// The syntax identifier for strings containing JavaScript Object Notation (JSON).
  /// </summary>
  public const string Json = nameof(Json);

  /// <summary>
  /// The syntax identifier for strings containing numeric format specifiers.
  /// </summary>
  public const string NumericFormat = nameof(NumericFormat);

  /// <summary>
  /// The syntax identifier for strings containing regular expressions.
  /// </summary>
  public const string Regex = nameof(Regex);

  /// <summary>
  /// The syntax identifier for strings containing time format specifiers.
  /// </summary>
  public const string TimeOnlyFormat = nameof(TimeOnlyFormat);

  /// <summary>
  /// The syntax identifier for strings containing <see cref="TimeSpan"/> format specifiers.
  /// </summary>
  public const string TimeSpanFormat = nameof(TimeSpanFormat);

  /// <summary>
  /// The syntax identifier for strings containing URIs.
  /// </summary>
  public const string Uri = nameof(Uri);

  /// <summary>
  /// The syntax identifier for strings containing XML.
  /// </summary>
  public const string Xml = nameof(Xml);

  /// <summary>
  /// Gets the identifier of the syntax used.
  /// </summary>
  public string Syntax { get; }

  /// <summary>
  /// Gets optional arguments associated with the specific syntax employed.
  /// </summary>
  public object[] Arguments { get; }

  /// <summary>
  /// Initializes the <see cref="StringSyntaxAttribute"/> with the identifier of the syntax used.
  /// </summary>
  /// <param name="syntax">The syntax identifier.</param>
  public StringSyntaxAttribute(string syntax) {
    this.Syntax = syntax;
    this.Arguments = _emptyArguments;
  }

  private static readonly object[] _emptyArguments =
#if SUPPORTS_ARRAY_EMPTY
    Array.Empty<object>();
#else
    new object[0];
#endif

  /// <summary>
  /// Initializes the <see cref="StringSyntaxAttribute"/> with the identifier of the syntax used.
  /// </summary>
  /// <param name="syntax">The syntax identifier.</param>
  /// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
  public StringSyntaxAttribute(string syntax, params object[] arguments) {
    this.Syntax = syntax;
    this.Arguments = arguments;
  }
}

#endif
