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

using System.Runtime.InteropServices;

namespace Utilities;

partial class RawMemory {
  [StructLayout(LayoutKind.Explicit, Size = 64)]
  private readonly struct Block64(ulong value0, ulong value1, ulong value2, ulong value3, ulong value4, ulong value5, ulong value6, ulong value7) {
    [FieldOffset(0)] readonly ulong value0 = value0;
    [FieldOffset(8)] readonly ulong value1 = value1;
    [FieldOffset(16)] readonly ulong value2 = value2;
    [FieldOffset(24)] readonly ulong value3 = value3;
    [FieldOffset(32)] readonly ulong value4 = value4;
    [FieldOffset(40)] readonly ulong value5 = value5;
    [FieldOffset(48)] readonly ulong value6 = value6;
    [FieldOffset(56)] readonly ulong value7 = value7;

    public Block64(byte value) : this(0x0101010101010101UL * value) { }
    public Block64(ushort value) : this(0x0001000100010001UL * value) { }
    public Block64(uint value) : this(0x0000000100000001UL * value) { }
    public Block64(ulong value) : this(value, value, value, value, value, value, value, value) { }

  }
}