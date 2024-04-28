#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Text.RegularExpressions;

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  private partial class SyntaxHighlighter {
    private readonly SyntaxHighlightingConfiguration _configuration;

    public SyntaxHighlighter(SyntaxHighlightingConfiguration configuration) => this._configuration = configuration;

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
      foreach (var pattern in this._configuration.Patterns)
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
