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

using System.Net.NetworkInformation;
using System.Net.Sockets;
#if SUPPORTS_ASYNC
using System.Threading.Tasks;
#endif
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#else
using System.Diagnostics;
#endif
// ReSharper disable UnusedMember.Global

namespace System.Net {
  // ReSharper disable once PartialTypeWithSinglePart

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class IPAddressExtensions {
    /// <summary>
    /// Determines whether the specified IPAddress is loopback.
    /// </summary>
    /// <param name="this">The this.</param>
    /// <returns>
    ///   <c>true</c> if the specified IPAddress is loopback; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsLoopback(this IPAddress @this)
      => IPAddress.IsLoopback(@this) || new[] { IPAddress.Any, IPAddress.IPv6Any, IPAddress.Loopback, IPAddress.IPv6Loopback }.Contains(@this)
      ;

    /// <summary>
    /// Gets the host name for the given ip-Address.
    /// </summary>
    /// <param name="this">This IPAddress.</param>
    /// <returns>The host name or <c>null</c>.</returns>
    public static string GetHostName(this IPAddress @this) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#else
      Debug.Assert(@this != null);
#endif
      try {
        var hostEntry = Dns.GetHostEntry(@this);
        return hostEntry.HostName;
      } catch (Exception) {
        return null;
      }
    }

#if SUPPORTS_ASYNC && NET45_OR_GREATER // this doesnt work on 4.0 but above
    /// <summary>
    /// Gets the host name for the given ip-Address.
    /// </summary>
    /// <param name="This">This IPAddress.</param>
    /// <returns>The host name or <c>null</c>.</returns>
    public static async Task<string> GetHostNameAsync(this IPAddress This) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
#endif
      try {
        var hostEntry = await Dns.GetHostEntryAsync(This);
        return hostEntry.HostName;
      } catch (Exception) {
        return null;
      }
    }
#endif

    public static Tuple<bool, string, PingReply, Exception> Ping(this IPAddress @this, uint retryCount = 0, TimeSpan? timeout = null, PingOptions options = null) {
      const int ttl = 128;
      const bool dontFragment = true;

      if (options == null)
        options = new PingOptions(ttl, dontFragment);

      if (timeout == null)
        timeout = TimeSpan.FromSeconds(5);

      var receiveBuffer = new byte[32];

      Tuple<bool, string, PingReply, Exception> result;
      using (var ping = new Ping()) {
        do {
          try {
            var pingReply = ping.Send(@this, (int)timeout.Value.TotalMilliseconds, receiveBuffer, options);
            //make sure we dont have a null reply
            if (pingReply == null) {
              result = Tuple.Create(false, "Got empty packet.", (PingReply)null, (Exception)null);
              continue;
            }

            switch (pingReply.Status) {
              case IPStatus.Success:
                return Tuple.Create(true, (string)null, pingReply, (Exception)null);
              case IPStatus.TimedOut: {
                  result = Tuple.Create(false, "Remote connection timed out.", pingReply, (Exception)null);
                  break;
                }
              case IPStatus.DestinationHostUnreachable: {
                  result = Tuple.Create(false, "Destination host unreachable.", pingReply, (Exception)null);
                  break;
                }
              case IPStatus.DestinationNetworkUnreachable: {
                  result = Tuple.Create(false, "Destination network unreachable.", pingReply, (Exception)null);
                  break;
                }
              case IPStatus.DestinationPortUnreachable: {
                  result = Tuple.Create(false, "Destination port unreachable.", pingReply, (Exception)null);
                  break;
                }
              case IPStatus.DestinationUnreachable: {
                  result = Tuple.Create(false, "Destination unreachable.", pingReply, (Exception)null);
                  break;
                }
              case IPStatus.TtlExpired: {
                  result = Tuple.Create(false, "Too many hops.", pingReply, (Exception)null);
                  break;
                }
              default: {
                  result = Tuple.Create(false, pingReply.Status.ToString(), pingReply, (Exception)null);
                  break;
                }
            }
          } catch (PingException e) {
            result = Tuple.Create(false, e.Message, (PingReply)null, (Exception)e);
          } catch (SocketException e) {
            result = Tuple.Create(false, e.Message, (PingReply)null, (Exception)e);
          }
        } while (retryCount-- > 0);
      }
      return result;
    }

  }
}