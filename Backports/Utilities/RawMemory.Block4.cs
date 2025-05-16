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
  [StructLayout(LayoutKind.Explicit, Size = 4)]

  private readonly struct Block4(uint value) {
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

    [FieldOffset(0)] readonly uint value = value;

    public Block4(byte value) : this(0x01010101U * value) { }
    public Block4(ushort value) : this(0x00010001U * value) { }

  }
}
