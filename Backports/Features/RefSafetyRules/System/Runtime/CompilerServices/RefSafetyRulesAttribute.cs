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

#if !SUPPORTS_REF_SAFETY_RULES

namespace System.Runtime.CompilerServices;

/// <summary>
/// Indicates the language version of the ref safety rules used when the module was compiled.
/// </summary>
[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
public sealed class RefSafetyRulesAttribute : Attribute {
  /// <summary>
  /// Gets the version of the ref safety rules used.
  /// </summary>
  public int Version { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="RefSafetyRulesAttribute"/> class.
  /// </summary>
  /// <param name="version">The version of the ref safety rules.</param>
  public RefSafetyRulesAttribute(int version) => this.Version = version;
}

#endif
