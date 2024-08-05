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
using System.Text.RegularExpressions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {

  /// <summary>
  /// Defines a pattern for syntax highlighting.
  /// </summary>
  public interface ISyntaxHighlightPattern {

    /// <summary>
    /// Gets the regular expression used to match text for syntax highlighting.
    /// </summary>
    Regex RegularExpression { get; }

    /// <summary>
    /// Gets the style to apply to the matched text.
    /// </summary>
    ISyntaxStyle Style { get; }

    /// <summary>
    /// Gets a dictionary of styles to apply to specific groups within the matched text.
    /// </summary>
    IReadOnlyDictionary<string, ISyntaxStyle> GroupStyles { get; }

  }

}
