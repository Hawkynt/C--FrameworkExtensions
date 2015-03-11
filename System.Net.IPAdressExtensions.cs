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

#if NET35
using System.Diagnostics;
#else
using System.Diagnostics.Contracts;
#endif

using System.Linq;

namespace System.Net {
  internal static partial class IPAdressExtensions {
    /// <summary>
    /// Determines whether the specified IPAdress is loopback.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <returns>
    ///   <c>true</c> if the specified IPAdress is loopback; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLoopback(this IPAddress This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (IPAddress.IsLoopback(This) || new[] { IPAddress.Any, IPAddress.IPv6Any, IPAddress.Loopback, IPAddress.IPv6Loopback }.Contains(This));
    }

    /// <summary>
    /// Gets the host name for the given ip-adress.
    /// </summary>
    /// <param name="This">This IPAdress.</param>
    /// <returns>The host name or <c>null</c>.</returns>
    public static string GetHostName(this IPAddress This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      try {
        var hostEntry = Dns.GetHostEntry(This);
        return (hostEntry.HostName);
      } catch (Exception) {
        return (null);
      }
    }
  }
}