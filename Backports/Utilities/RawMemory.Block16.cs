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
  [StructLayout(LayoutKind.Explicit, Size = 16)]
  private readonly struct Block16(ulong value0, ulong value1) {
    [FieldOffset(0)] readonly ulong value0=value0;
    [FieldOffset(8)] readonly ulong value1 = value1;

    public Block16(byte value) : this(0x0101010101010101UL * value) { }
    public Block16(ushort value) : this(0x0001000100010001UL * value) { }
    public Block16(uint value) : this(0x0000000100000001UL * value) { }
    public Block16(ulong value) : this(value, value) { }
    
  }
}
