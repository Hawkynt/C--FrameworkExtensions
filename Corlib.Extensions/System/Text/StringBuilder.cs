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

using System.Collections.Generic;
using Guard;

namespace System.Text;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  // ReSharper disable once PartialTypeWithSinglePart
  static partial class StringBuilderExtensions {

  /// <summary>
  /// Appends a collection of lines to the end of the current <see cref="StringBuilder"/> object.
  /// Each line is appended as a new line.
  /// </summary>
  /// <param name="this">The <see cref="StringBuilder"/> to which the lines should be appended.</param>
  /// <param name="lines">An <see cref="IEnumerable{T}"/> of lines to append to the <see cref="StringBuilder"/>.</param>
  public static void AppendLines(this StringBuilder @this, IEnumerable<string> lines) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(lines);
    
    foreach (var line in lines)
      @this.AppendLine(line);
  }

}