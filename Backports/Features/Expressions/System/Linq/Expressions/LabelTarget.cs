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

// LabelTarget was added in .NET 4.0 but is used by expression types that depend on the polyfilled Expression class
// Only compile for net20 where our full expression polyfill is available
#if !SUPPORTS_LINQ

namespace System.Linq.Expressions;

/// <summary>
/// Used to denote the target of a <see cref="GotoExpression"/>.
/// </summary>
public sealed class LabelTarget {

  /// <summary>
  /// Initializes a new instance of the <see cref="LabelTarget"/> class.
  /// </summary>
  internal LabelTarget(Type type, string? name) {
    this.Type = type;
    this.Name = name;
  }

  /// <summary>
  /// Gets the name of the label.
  /// </summary>
  /// <value>The name of the label, or <c>null</c> if the label has no name.</value>
  public string? Name { get; }

  /// <summary>
  /// The type of value that is passed when jumping to the label (or <see cref="System.Void"/> if no value should be passed).
  /// </summary>
  /// <value>The <see cref="System.Type"/> object representing the type of the value that is passed when jumping to the label.</value>
  public Type Type { get; }

  /// <summary>
  /// Returns a textual representation of the <see cref="LabelTarget"/>.
  /// </summary>
  public override string ToString() => this.Name ?? "UnamedLabel";

}

#endif
