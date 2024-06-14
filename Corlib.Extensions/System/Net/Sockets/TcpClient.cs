#region (c)2020-2042 Hawkynt
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

namespace System.Net.Sockets;

public static partial class TcpClientExtensions {

  /// <summary>
  /// Gets the state of the tcp client connection
  /// </summary>
  /// <param name="this">This <see href="TcpClient">TcpClient</see></param>
  /// <returns></returns>
  public static TcpState GetState(this TcpClient @this) =>
    IPGlobalProperties.GetIPGlobalProperties()
      .GetActiveTcpConnections()
      .FirstOrDefault(ci => ci.LocalEndPoint.Equals(@this.Client.LocalEndPoint))?.State ?? TcpState.Unknown
  ;

  /// <summary>
  /// Checks if the tcp client is still connected
  /// </summary>
  /// <param name="this">This <see href="TcpClient">TcpClient</see></param>
  /// <returns><c>true</c> if client is still connected; otherwise <c>false</c></returns>
  public static bool IsStillConnected(this TcpClient @this) => GetState(@this) == TcpState.Established;

}