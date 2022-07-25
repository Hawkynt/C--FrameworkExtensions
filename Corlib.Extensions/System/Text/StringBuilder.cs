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

#if NET40_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD
#define SUPPORTS_CONTRACTS 
#endif

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

namespace System.Text.StringBuildera {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class StringBuilderExtensions {

    /// <summary>
    /// Appends the lines.
    /// </summary>
    /// <param name="This">This StringBuilder.</param>
    /// <param name="lines">The lines to add.</param>
    public static void AppendLines(this StringBuilder This, IEnumerable<string> lines) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(lines != null);
#endif
      foreach (var line in lines)
        This.AppendLine(line);
    }

  }
}