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

namespace System.Net {
  internal static partial class WebHeaderCollectionExntenions {
    /// <summary>
    /// Adds a bunch of headers.
    /// </summary>
    /// <param name="This">This WebHeaderCollection.</param>
    /// <param name="headers">The headers.</param>
    public static void AddRange(this WebHeaderCollection This, IEnumerable<KeyValuePair<string, string>> headers) {
      Contract.Requires(This != null);
      Contract.Requires(headers != null);
      foreach (var kvp in headers)
        This.Add(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Adds a bunch of headers.
    /// </summary>
    /// <param name="This">This WebHeaderCollection.</param>
    /// <param name="headers">The headers.</param>
    public static void AddRange(this WebHeaderCollection This, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers) {
      Contract.Requires(This != null);
      Contract.Requires(headers != null);
      foreach (var kvp in headers)
        This.Add(kvp.Key, kvp.Value);
    }

    /// <summary>
    /// Adds a bunch of headers.
    /// </summary>
    /// <param name="This">This WebHeaderCollection.</param>
    /// <param name="headers">The headers.</param>
    public static void AddRange(this WebHeaderCollection This, IEnumerable<KeyValuePair<HttpResponseHeader, string>> headers) {
      Contract.Requires(This != null);
      Contract.Requires(headers != null);
      foreach (var kvp in headers)
        This.Add(kvp.Key, kvp.Value);
    }
  }
}
