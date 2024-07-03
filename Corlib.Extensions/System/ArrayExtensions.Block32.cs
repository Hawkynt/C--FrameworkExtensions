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
  [StructLayout(LayoutKind.Sequential, Size = 32)]
  private struct Block32(uint u) {
    public readonly uint a = u;
    public readonly uint b = u;
    public readonly uint c = u;
    public readonly uint d = u;
    public readonly uint e = u;
    public readonly uint f = u;
    public readonly uint g = u;
    public readonly uint h = u;

    public Block32(ushort u) : this(0x00010001U * u) { }
    public Block32(byte u) : this(0x01010101U * u) { }
  }
}
