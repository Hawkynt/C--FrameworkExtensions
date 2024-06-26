﻿#region (c)2010-2042 Hawkynt

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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Guard;

namespace System.Net.NetworkInformation;

public static partial class PhysicalAddressExtensions {
  /// <summary>
  ///   Gets the MacAdress.
  /// </summary>
  /// <param name="this">This PhysicalAddress.</param>
  /// <returns>The mac adress string, delimited with ":"</returns>
  public static string MacAdress(this PhysicalAddress @this) {
    Against.ThisIsNull(@this);

#if SUPPORTS_JOIN_ENUMERABLES
    return string.Join(":", @this.GetAddressBytes().Select(b => $"{b:X2}"));
#else
    return string.Join(":", @this.GetAddressBytes().Select(b => $"{b:X2}").ToArray());
#endif
  }

  /// <summary>
  ///   Gets the ip adresses for a MAC.
  /// </summary>
  /// <param name="this">This PhysicalAddress.</param>
  /// <returns>A number of IPAdress instances</returns>
  public static IEnumerable<IPAddress> GetIpAdresses(this PhysicalAddress @this) {
    Against.ThisIsNull(@this);

    return Invoke(@this);

    static IEnumerable<IPAddress> Invoke(PhysicalAddress @this)
      => from ip in _GetArpCache()
        where ip.Item1.Equals(@this)
        select ip.Item2
    ;
  }

  private static class NativeMethods {
    // Define the MIB_IPNETROW structure.
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_IPNETROW {
      [MarshalAs(UnmanagedType.U4)] public uint dwIndex;
      [MarshalAs(UnmanagedType.U4)] public uint dwPhysAddrLen;
      [MarshalAs(UnmanagedType.U1)] public byte mac0;
      [MarshalAs(UnmanagedType.U1)] public byte mac1;
      [MarshalAs(UnmanagedType.U1)] public byte mac2;
      [MarshalAs(UnmanagedType.U1)] public byte mac3;
      [MarshalAs(UnmanagedType.U1)] public byte mac4;
      [MarshalAs(UnmanagedType.U1)] public byte mac5;
      [MarshalAs(UnmanagedType.U1)] public byte mac6;
      [MarshalAs(UnmanagedType.U1)] public byte mac7;
      [MarshalAs(UnmanagedType.U4)] public int dwAddr;
      [MarshalAs(UnmanagedType.U4)] public int dwType;
    }

    // Declare the GetIpNetTable function.
    [DllImport("IpHlpApi.dll")]
    [return: MarshalAs(UnmanagedType.U4)]
    public static extern int GetIpNetTable(
      IntPtr pIpNetTable,
      ref uint pdwSize,
      bool bOrder
    );

    [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int FreeMibTable(IntPtr plpNetTable);

    // The insufficient buffer error.
    public const int ERROR_INSUFFICIENT_BUFFER = 122;
  }

  private static IEnumerable<Tuple<PhysicalAddress, IPAddress>> _GetArpCache() {
    // The number of bytes needed.
    var bytesNeeded = 0U;

    // The result from the API call.
    var result = NativeMethods.GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

    // Call the function, expecting an insufficient buffer.
    if (result != NativeMethods.ERROR_INSUFFICIENT_BUFFER)
      throw new Win32Exception(result);


    // Allocate the memory, do it in a try/finally block, to ensure
    // that it is released.
    var buffer = IntPtr.Zero;

    // Try/finally.
    try {
      // Allocate the memory.
      buffer = Marshal.AllocCoTaskMem((int)bytesNeeded);

      // Make the call again. If it did not succeed, then
      // raise an error.
      result = NativeMethods.GetIpNetTable(buffer, ref bytesNeeded, false);

      // If the result is not 0 (no error), then throw an exception.
      if (result != 0)
        throw new Win32Exception(result);

      // Now we have the buffer, we have to marshal it. We can read
      // the first 4 bytes to get the length of the buffer.
      var entries = Marshal.ReadInt32(buffer);

      // Increment the memory pointer by the size of the int.
      IntPtr currentBuffer = new(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));

      // Cycle through the entries.
      for (var index = 0; index < entries; index++) {
        // Call PtrToStructure, getting the structure information.
        var row = (NativeMethods.MIB_IPNETROW)Marshal.PtrToStructure(
          new(currentBuffer.ToInt64() + index * Marshal.SizeOf(typeof(NativeMethods.MIB_IPNETROW))),
          typeof(NativeMethods.MIB_IPNETROW)
        );

        IPAddress ip = new(BitConverter.GetBytes(row.dwAddr));
        PhysicalAddress physicalAddress = new([row.mac0, row.mac1, row.mac2, row.mac3, row.mac4, row.mac5]);

        if (physicalAddress.GetAddressBytes().Any(b => b != 0))
          yield return Tuple.Create(physicalAddress, ip);
      }
    } finally {
      // Release the memory.
      if (buffer != IntPtr.Zero)
        NativeMethods.FreeMibTable(buffer);
    }
  }
}
