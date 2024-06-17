#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Net.NetworkInformation;

namespace System.Net.Sockets;

public static partial class TcpClientExtensions {
  /// <summary>
  ///   Gets the state of the tcp client connection
  /// </summary>
  /// <param name="this">This <see href="TcpClient">TcpClient</see></param>
  /// <returns></returns>
  public static TcpState GetState(this TcpClient @this) =>
    IPGlobalProperties
      .GetIPGlobalProperties()
      .GetActiveTcpConnections()
      .FirstOrDefault(ci => ci.LocalEndPoint.Equals(@this.Client.LocalEndPoint))
      ?.State
    ?? TcpState.Unknown;

  /// <summary>
  ///   Checks if the tcp client is still connected
  /// </summary>
  /// <param name="this">This <see href="TcpClient">TcpClient</see></param>
  /// <returns><c>true</c> if client is still connected; otherwise <c>false</c></returns>
  public static bool IsStillConnected(this TcpClient @this) => GetState(@this) == TcpState.Established;
}
