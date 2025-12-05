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

#if !SUPPORTS_EXPERIMENTAL_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Indicates that an API is experimental and it may change in the future.
/// </summary>
/// <remarks>
/// This attribute allows call sites to be flagged with a diagnostic that indicates that an experimental
/// feature is used. Authors can use this attribute to ship preview features in their assemblies.
/// </remarks>
[AttributeUsage(
  AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct |
  AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property |
  AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate,
  Inherited = false)]
public sealed class ExperimentalAttribute : Attribute {
  /// <summary>
  /// Gets the ID that the compiler will use when reporting a use of the API the attribute applies to.
  /// </summary>
  public string DiagnosticId { get; }

  /// <summary>
  /// Gets or sets the URL for corresponding documentation. The API accepts a format string instead of an actual URL,
  /// creating a generic URL that includes the diagnostic ID.
  /// </summary>
  public string UrlFormat { get; set; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ExperimentalAttribute"/> class.
  /// </summary>
  /// <param name="diagnosticId">The ID that the compiler will use when reporting a use of the API.</param>
  public ExperimentalAttribute(string diagnosticId) => this.DiagnosticId = diagnosticId;
}

#endif
