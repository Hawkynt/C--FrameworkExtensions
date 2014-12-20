#region (c)2010-2020 Hawkynt
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Text.StringBuildera {
  internal static partial class StringBuilderExtensions {

    /// <summary>
    /// Appends the lines.
    /// </summary>
    /// <param name="This">This StringBuilder.</param>
    /// <param name="lines">The lines to add.</param>
    public static void AppendLines(this StringBuilder This, IEnumerable<string> lines) {
      Contract.Requires(This != null);
      Contract.Requires(lines != null);
      foreach (var line in lines)
        This.AppendLine(line);
    }

  }
}
