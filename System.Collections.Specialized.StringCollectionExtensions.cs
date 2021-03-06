﻿#region (c)2010-2020 Hawkynt
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

using System.Diagnostics.Contracts;

namespace System.Collections.Specialized {
  internal static partial class StringCollectionExtensions {

    /// <summary>
    /// Copies the content of this collection to an array.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <returns></returns>
    public static string[] ToArray(this StringCollection This) {
      Contract.Requires(This != null);
      var result = new string[This.Count];
      This.CopyTo(result, 0);
      return (result);
    }
  }
}