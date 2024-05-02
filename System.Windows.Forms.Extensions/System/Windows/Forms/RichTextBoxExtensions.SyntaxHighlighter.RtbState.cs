﻿#region (c)2010-2042 Hawkynt

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

namespace System.Windows.Forms;

public static partial class RichTextBoxExtensions {
  private partial class SyntaxHighlighter {
    private readonly struct RtbState {
      private readonly RichTextBox _owner;
      private readonly int _selectionStart;
      private readonly int _selectionLength;

      private RtbState(RichTextBox owner) {
        this._owner = owner;
        this._selectionStart = owner.SelectionStart;
        this._selectionLength = owner.SelectionLength;
      }

      public static RtbState Save(RichTextBox owner) => new(owner);

      public void Load() {
        var rtb = this._owner;
        rtb.SelectionStart = this._selectionStart;
        rtb.SelectionLength = this._selectionLength;
      }
    }
  }
}