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

namespace System;

using Runtime.InteropServices;

public static partial class ArrayExtensions {
  [StructLayout(LayoutKind.Sequential, Size = 64)]
  private struct Block64(ulong u) {
    public readonly ulong a = u;
    public readonly ulong b = u;
    public readonly ulong c = u;
    public readonly ulong d = u;
    public readonly ulong e = u;
    public readonly ulong f = u;
    public readonly ulong g = u;
    public readonly ulong h = u;

    public Block64(uint u) : this(0x0000000100000001UL * u) { }
    public Block64(ushort u) : this(0x0001000100010001UL * u) { }
    public Block64(byte u) : this(0x0101010101010101UL * u) { }
  }
}
