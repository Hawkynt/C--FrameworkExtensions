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

namespace System; 

static partial class ArrayExtensions {
  private static class RuntimeConfiguration {
    public static readonly int MaxDegreeOfParallelism = Environment.ProcessorCount;

    public static bool Has16BitRegisters => IntPtr.Size >= 2;

    public static bool Has32BitRegisters => IntPtr.Size >= 4;

    public static bool Has64BitRegisters => IntPtr.Size >= 8;

    public const int MIN_ITEMS_FOR_PARALELLISM = 2048;
    public const int MIN_ITEMS_PER_THREAD = 128;

    public const int DEFAULT_MAX_CHUNK_SIZE = 1024 * 64;

    public const int ALLOCATION_WORD = 128;
    public const int ALLOCATION_DWORD = 256;
    public const int ALLOCATION_QWORD = 512;

    public const int BLOCKCOPY_WORD = 2;
    public const int BLOCKCOPY_DWORD = 4;
    public const int BLOCKCOPY_QWORD = 8;
  }
}