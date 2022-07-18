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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Reflection;

namespace System.Text.RegularExpressions {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class MatchExtensions {
    /// <summary>
    /// Gets the linenumber of this match in the original text.
    /// </summary>
    /// <param name="This">This Match.</param>
    /// <returns>The number of \n in the text before this match started.</returns>
    public static int LineNumber(this Match This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
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
    public static string GetTextSource(this Match This) => This._GetPrivateTextFieldValue();

    /// <summary>
    /// Gets the private field value of a match.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <returns></returns>
    private static string _GetPrivateTextFieldValue(this Match match) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(match != null);
#endif
      const string fieldName = "_text";
      var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var type = typeof(Match);
      FieldInfo fieldInfo = null;

      while (fieldInfo == null && type != null) {
        fieldInfo = type.GetField(fieldName, bindingFlags);
        type = type.BaseType;
      }

      if (fieldInfo != null)
        return (string) fieldInfo.GetValue(match);

      const string propName = "Text";
      type = typeof(Match);
      PropertyInfo fieldProperty = null;

      while (fieldProperty == null && type != null) {
        fieldProperty = type.GetProperty(propName, bindingFlags);
        type = type.BaseType;
      }

      return (string)fieldProperty?.GetValue(match,null)
             ?? throw new ArgumentOutOfRangeException("propName", $"Neither field {fieldName} nor property {propName} was found in Type {typeof(Match).FullName}");
    }
  }
}
