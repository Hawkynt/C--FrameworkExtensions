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

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Guard;

namespace System.Net;

/// <summary>
///   This is the managed class for the routines in IPHLPAPI
/// </summary>
public static class IPHelper {
  #region nested types

  /// <summary>
  ///   This class contains all native declarations.
  /// </summary>
  private static class NativeMethods {
    #region consts

    // ReSharper disable UnusedMember.Local

    /// <summary>
    ///   MIB TCP state enumeration, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366909(v=vs.85).aspx
    /// </summary>
    public enum MibTcpState {
      Closed = 1,
      Listen = 2,
      SynSent = 3,
      SynRcvd = 4,
      Estab = 5,
      FinWait1 = 6,
      FinWait2 = 7,
      CloseWait = 8,
      Closing = 9,
      LastAck = 10,
      TimeWait = 11,
      DeleteTcb = 12,
    }

    /// <summary>
    ///   Adress family, see socket.h
    /// </summary>
    public enum AfInet {
      Unspecified = 0,
      Inet = 2,
      Ipx = 6,
      AppleTalk = 16,
      NetBios = 17,
      Inet6 = 23,
      Irda = 26,
      Bluetooth = 32,
    }

    /// <summary>
    ///   UDP table classes, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366388(v=vs.85).aspx
    /// </summary>
    public enum UdpTableClass {
      Basic,
      OwnerPid,
      OwnerPidModule,
    }

    /// <summary>
    ///   TCP table classes, see http://msdn.microsoft.com/en-us/library/windows/desktop/aa366386(v=vs.85).aspx
    /// </summary>
    public enum TcpTableClass {
      BasicListener,
      BasicConnections,
      BasicAll,
      OwnerPidListener,
      OwnerPidConnections,
      OwnerPidAll,
      OwnerModuleListener,
      OwnerModuleConnections,
      OwnerModuleAll,
    }

    /// <summary>
    ///   Windows Error Codes, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx
    /// </summary>
    public enum Win32ApiError {
      Success = 0,
      NotSupported = 0x32,
      InsufficientBuffer = 122,
    }

    // ReSharper restore UnusedMember.Local

    #endregion

    #region structs, see http: //msdn.microsoft.com/en-us/library/windows/desktop/aa366889(v=vs.85).aspx

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW {
      public readonly uint dwLocalAddr;
      public readonly uint dwLocalPort;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW {
      public readonly uint dwState;
      public readonly uint dwLocalAddr;
      public readonly uint dwLocalPort;
      public readonly uint dwRemoteAddr;
      public readonly uint dwRemotePort;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPROW_OWNER_PID {
      public readonly uint dwLocalAddr;
      public readonly uint dwLocalPort;
      public readonly uint dwOwningPid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID {
      public readonly uint dwState;
      public readonly uint dwLocalAddr;
      public readonly uint dwLocalPort;
      public readonly uint dwRemoteAddr;
      public readonly uint dwRemotePort;
      public readonly uint dwOwningPid;
    }

    #endregion

    #region native calls, see http: //msdn.microsoft.com/en-us/library/windows/desktop/aa366071(v=vs.85).aspx

    [DllImport("iphlpapi.dll", SetLastError = true, EntryPoint = "GetUdpTable")]
    public static extern Win32ApiError GetUdpTable(nint pUdpTable, ref uint pdwSize, bool bOrder);

    [DllImport("iphlpapi.dll", SetLastError = true, EntryPoint = "GetTcpTable")]
    public static extern Win32ApiError GetTcpTable(nint pTcpTable, ref uint pdwSize, bool bOrder);

    [DllImport("iphlpapi.dll", SetLastError = true, EntryPoint = "GetExtendedUdpTable")]
    public static extern Win32ApiError GetExtendedUdpTable(nint pUdpTable, ref uint pdwSize, bool bOrder, AfInet ulAf, UdpTableClass udpTableClass, uint reserved = 0);

    [DllImport("iphlpapi.dll", SetLastError = true, EntryPoint = "GetExtendedTcpTable")]
    public static extern Win32ApiError GetExtendedTcpTable(nint pTcpTable, ref uint pdwSize, bool bOrder, AfInet ulAf, TcpTableClass tcpTableClass, uint reserved = 0);

    #endregion
  } // end NativeMethods


  /// <summary>
  ///   The state of a connection.
  /// </summary>
  public enum ConnectionState {
    Unknown = 0,
    Established = NativeMethods.MibTcpState.Estab,
    Listening = NativeMethods.MibTcpState.Listen,
    SynSent = NativeMethods.MibTcpState.SynSent,
    SynReceived = NativeMethods.MibTcpState.SynRcvd,
    Closed = NativeMethods.MibTcpState.Closed,
    Closing = NativeMethods.MibTcpState.Closing,
    CloseWait = NativeMethods.MibTcpState.CloseWait,
    FinWait1 = NativeMethods.MibTcpState.FinWait1,
    FinWait2 = NativeMethods.MibTcpState.FinWait2,
    LastAcknowledgeWaiting = NativeMethods.MibTcpState.LastAck,
    TimeoutWaitingForTermination = NativeMethods.MibTcpState.TimeWait,
    DeleteTransmissionControlBlock = NativeMethods.MibTcpState.DeleteTcb,
  }

  /// <summary>
  ///   The protocol used.
  /// </summary>
  public enum ConnectionProtocol {
    Unknown,
    Tcp,
    Udp,
  }

  /// <summary>
  ///   A connection.
  /// </summary>
  public sealed class Connection {
    /// <summary>
    ///   Gets the local endpoint, ie. adress and port.
    /// </summary>
    public IPEndPoint Local { get; }

    /// <summary>
    ///   Gets the remote endpoint, ie. adress and port.
    /// </summary>
    public IPEndPoint Remote { get; }

    /// <summary>
    ///   Gets the connection state.
    /// </summary>
    public ConnectionState State { get; }

    /// <summary>
    ///   Gets the connection protocol.
    /// </summary>
    public ConnectionProtocol Protocol { get; }

    /// <summary>
    ///   Gets the source process, if known.
    /// </summary>
    public Process SourceProcess { get; }

    internal Connection(ConnectionProtocol protocol, IPAddress localAdress, int localPort, IPAddress remoteAdress, int remotePort, ConnectionState state, Process sourceProcess)
      : this(
        protocol,
        new(localAdress, localPort),
        new(remoteAdress, remotePort),
        state,
        sourceProcess
      ) { }

    internal Connection(ConnectionProtocol protocol, IPEndPoint local, IPEndPoint remote, ConnectionState state, Process sourceProcess) {
      this.State = state;
      this.SourceProcess = sourceProcess;
      this.Remote = remote;
      this.Protocol = protocol;
      this.Local = local;
    }

    public override string ToString() => $"{this.Protocol}({this.State}): {this.Local} -> {this.Remote} ({this.SourceProcess?.ProcessName}[{this.SourceProcess?.Id ?? 0}])";
  }

  private sealed class BufferSizeAndWin32Status(NativeMethods.Win32ApiError status, uint size) {
    public uint Size { get; } = size;

    public NativeMethods.Win32ApiError Status { get; } = status;
  }

  #endregion

  /// <summary>
  ///   Gets the active connections.
  /// </summary>
  /// <returns>An array of all active connections.</returns>
  public static Connection[] GetActiveConnections() => GetTcpTable().Concat(GetUdpTable()).ToArray();

  /// <summary>
  ///   Gets the TCP table.
  ///   Note: Tries the new method first and if this is unsupported on your system, uses the old one.
  /// </summary>
  /// <returns>An array with all active TCP connections.</returns>
  public static Connection[] GetTcpTable() {
    try {
      return _GetTcpTableNew();
    } catch (Win32Exception e) {
      if (e.NativeErrorCode == (int)NativeMethods.Win32ApiError.NotSupported)
        return _GetTcpTableOld();
      throw;
    }
  }

  /// <summary>
  ///   Gets the UDP table.
  ///   Note: Tries the new method first and if this is unsupported on your system, uses the old one.
  /// </summary>
  /// <returns>An array with all active UDP connections.</returns>
  public static Connection[] GetUdpTable() {
    try {
      return _GetUdpTableNew();
    } catch (Win32Exception e) {
      if (e.NativeErrorCode == (int)NativeMethods.Win32ApiError.NotSupported)
        return _GetUdpTableOld();
      throw;
    }
  }

  /// <summary>
  ///   Gets the TCP table using the pre-Vista method and without process id's.
  /// </summary>
  /// <returns>An array with all active TCP connections.</returns>
  private static Connection[] _GetTcpTableOld()
    => _GetTable<NativeMethods.MIB_TCPROW>(
      (pointer, size) => {
        var status = NativeMethods.GetTcpTable(pointer, ref size, false);
        return new(status, size);
      },
      row => new(ConnectionProtocol.Tcp, new(row.dwLocalAddr), _ConvertPort(row.dwLocalPort), new(row.dwRemoteAddr), _ConvertPort(row.dwRemotePort), (ConnectionState)row.dwState, null)
    );

  /// <summary>
  ///   Gets the UDP table using the pre-Vista method and without process id's.
  /// </summary>
  /// <returns>An array with all active UDP connections.</returns>
  private static Connection[] _GetUdpTableOld()
    => _GetTable<NativeMethods.MIB_UDPROW>(
      (pointer, size) => {
        var status = NativeMethods.GetUdpTable(pointer, ref size, false);
        return new(status, size);
      },
      row => new(ConnectionProtocol.Udp, new(row.dwLocalAddr), _ConvertPort(row.dwLocalPort), IPAddress.None, 0, ConnectionState.Unknown, null)
    );

  /// <summary>
  ///   Gets the TCP table using the Vista+ method and with process id's.
  /// </summary>
  /// <returns>An array with all active TCP connections.</returns>
  private static Connection[] _GetTcpTableNew()
    => _GetTable<NativeMethods.MIB_TCPROW_OWNER_PID>(
      (pointer, size) => {
        var status = NativeMethods.GetExtendedTcpTable(pointer, ref size, false, NativeMethods.AfInet.Inet, NativeMethods.TcpTableClass.OwnerPidAll);
        return new(status, size);
      },
      row => {
        Process process;
        try {
          process = Process.GetProcessById((int)row.dwOwningPid);
        } catch {
          process = null;
        }

        return new(ConnectionProtocol.Tcp, new(row.dwLocalAddr), _ConvertPort(row.dwLocalPort), new(row.dwRemoteAddr), _ConvertPort(row.dwRemotePort), (ConnectionState)row.dwState, process);
      }
    );

  /// <summary>
  ///   Gets the UDP table using the Vista+ method and with process id's.
  /// </summary>
  /// <returns>An array with all active UDP connections.</returns>
  private static Connection[] _GetUdpTableNew()
    => _GetTable<NativeMethods.MIB_UDPROW_OWNER_PID>(
      (pointer, size) => {
        var status = NativeMethods.GetExtendedUdpTable(pointer, ref size, false, NativeMethods.AfInet.Inet, NativeMethods.UdpTableClass.OwnerPid);
        return new(status, size);
      },
      row => {
        Process process;
        try {
          process = Process.GetProcessById((int)row.dwOwningPid);
        } catch {
          process = null;
        }

        return new(ConnectionProtocol.Udp, new(row.dwLocalAddr), _ConvertPort(row.dwLocalPort), IPAddress.None, 0, ConnectionState.Unknown, process);
      }
    );

  /// <summary>
  ///   Gets a connection table.
  ///   Note: Asks for space requirements first, allocates buffer, calls, casts, processes, deallocates, returns.
  /// </summary>
  /// <typeparam name="TRowtype">The type of the rows.</typeparam>
  /// <param name="call">The call to get the table.</param>
  /// <param name="rowProcessor">The row processor.</param>
  /// <returns>The connections from the table.</returns>
  private static Connection[] _GetTable<TRowtype>(Func<nint, uint, BufferSizeAndWin32Status> call, Func<TRowtype, Connection> rowProcessor) {
    Against.ArgumentIsNull(call);
    Against.ArgumentIsNull(rowProcessor);

    // get size of table first
    var tuple = call(IntPtr.Zero, 0);
    var size = tuple.Size;
    var status = tuple.Status;

    if (status == NativeMethods.Win32ApiError.Success)
      return Utilities.Array.Empty<Connection>();

    while (status == NativeMethods.Win32ApiError.InsufficientBuffer) {
      // allocate buffer and make sure it is de-allocated in every case
      var buffer = IntPtr.Zero;
      try {
        buffer = Marshal.AllocHGlobal((int)size);
        tuple = call(buffer, size);
        size = tuple.Size;
        status = tuple.Status;

        if (status != NativeMethods.Win32ApiError.Success)
          continue;

        // the first 32-Bits of the buffer are always the number of table entries in each table type
        var count = Marshal.ReadInt32(buffer);
        var rowPointer = (long)buffer + 4;

        var result = new Connection[count];
        var rowType = typeof(TRowtype);
        var rowSizeInBytes = Marshal.SizeOf(rowType);

        // convert each entry to a connection instance
        for (var i = 0; i < result.Length; ++i) {
          var row = (TRowtype)Marshal.PtrToStructure((nint)rowPointer, rowType);
          result[i] = rowProcessor(row);

          // move pointer to next entry
          rowPointer += rowSizeInBytes;
        }

        return result;
      } finally {
        // free buffer if allocated
        if (buffer != IntPtr.Zero)
          Marshal.FreeHGlobal(buffer);
      }
    } // retry as long as the buffer is too small

    // we failed somehow in all cases when we land here
    throw new Win32Exception((int)status);
  }

  /// <summary>
  ///   Convert the strangely Motorola port number DWord. (Swaps low and high bytes)
  /// </summary>
  /// <param name="port">The port dword.</param>
  /// <returns>The real port number</returns>
  private static int _ConvertPort(uint port) {
    var result = ((port & 0xff) << 8) | ((port >> 8) & 0xff);
    return (int)result;
  }
}
