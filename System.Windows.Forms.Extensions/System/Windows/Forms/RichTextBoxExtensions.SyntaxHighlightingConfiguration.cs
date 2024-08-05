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

using System.Collections.Generic;
using System.Linq;
using Guard;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {

  /// <summary>
  /// Represents the configuration for syntax highlighting.
  /// </summary>
  public readonly struct SyntaxHighlightingConfiguration {

    /// <summary>
    /// Gets the array of syntax highlight patterns.
    /// </summary>
    public ISyntaxHighlightPattern[] Patterns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxHighlightingConfiguration"/> struct with the specified patterns.
    /// </summary>
    /// <param name="patterns">The collection of syntax highlight patterns.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="patterns"/> is <see langword="null"/>.</exception>
    public SyntaxHighlightingConfiguration(IEnumerable<ISyntaxHighlightPattern> patterns) {
      Against.ArgumentIsNull(patterns);

      this.Patterns = patterns.ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyntaxHighlightingConfiguration"/> struct with the specified patterns.
    /// </summary>
    /// <param name="patterns">An array of syntax highlight patterns.</param>
    public SyntaxHighlightingConfiguration(params ISyntaxHighlightPattern[] patterns) => this.Patterns = patterns;

  }

}
