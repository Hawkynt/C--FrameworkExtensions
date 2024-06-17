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

using System.Text.RegularExpressions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  private sealed partial class SyntaxHighlighter(SyntaxHighlightingConfiguration configuration) {
    public void ApplySyntaxHighlighting(RichTextBox rtb) {
      rtb.SuspendLayout();
      try {
        var rtbState = RtbState.Save(rtb);
        try {
          this._StyleRichTextBox(rtb);
        } finally {
          rtbState.Load();
        }
      } finally {
        rtb.ResumeLayout(true);
      }
    }

    private void _StyleRichTextBox(RichTextBox rtb) {
      var text = rtb.Text ?? rtb.Rtf;
      foreach (var pattern in configuration.Patterns)
      foreach (Match match in pattern.RegularExpression.Matches(text)) {
        ((_SyntaxStyle?)pattern.Style)?.ApplyTo(rtb, match.Index, match.Length);

        if (pattern.GroupStyles == null)
          continue;

        foreach (var kvp in pattern.GroupStyles) {
          var group = match.Groups[kvp.Key];
          if (group == null || !group.Success)
            continue;

          ((_SyntaxStyle?)kvp.Value)?.ApplyTo(rtb, group.Index, group.Length);
        }
      }
    }
  }
}
