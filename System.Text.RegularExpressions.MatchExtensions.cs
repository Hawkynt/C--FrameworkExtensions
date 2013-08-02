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

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace System.Text.RegularExpressions {
  internal static partial class MatchExtensions {
    /// <summary>
    /// Gets the linenumber of this match in the original text.
    /// </summary>
    /// <param name="This">This Match.</param>
    /// <returns>The number of \n in the text before this match started.</returns>
    public static int LineNumber(this Match This) {
      Contract.Requires(This != null);
      if (!This.Success)
        return (-1);

      var text = This.GetTextSource();
      var before = text.Substring(0, This.Index);
      var result = before.Count(c => c == '\n');

      return (result);

    }

    /// <summary>
    /// Gets the text source.
    /// </summary>
    /// <param name="This">This Match.</param>
    /// <returns>The text that generated this match.</returns>
    public static string GetTextSource(this Match This) {
      return (This._GetPrivateTextFieldValue());
    }

    /// <summary>
    /// Gets the private field value of a match.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <returns></returns>
    private static string _GetPrivateTextFieldValue(this Match match) {
      Contract.Requires(match != null);
      const string propName = "_text";

      var type = typeof(Match);
      FieldInfo fieldInfo = null;
      while (fieldInfo == null && type != null) {
        fieldInfo = type.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        type = type.BaseType;
      }

      if (fieldInfo == null)
        throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, typeof(Match).FullName));

      return (string)fieldInfo.GetValue(match);
    }
  }
}
