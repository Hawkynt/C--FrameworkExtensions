#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  public static partial class SyntaxHighlightPattern {
    private readonly struct Pattern(Regex regularExpression, ISyntaxStyle style, IReadOnlyDictionary<string, ISyntaxStyle> groupStyles = null)
      : ISyntaxHighlightPattern {
      #region Implementation of ISyntaxHighlightPattern

      public Regex RegularExpression { get; } = regularExpression;
      public ISyntaxStyle Style { get; } = style;
      public IReadOnlyDictionary<string, ISyntaxStyle> GroupStyles { get; } = groupStyles;

      #endregion
    }
  }
}
