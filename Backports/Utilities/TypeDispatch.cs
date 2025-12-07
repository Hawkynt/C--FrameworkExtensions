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
//

using System.Runtime.CompilerServices;

namespace Utilities;

internal enum CachedTypeCode {
  Unknown,
  Byte,
  SByte,
  Char,
  UInt16,
  Int16,
  UInt32,
  Int32,
  UInt64,
  Int64,
  Single,
  Double,
  Decimal,
  Pointer,
  UPointer,
  Boolean
}

internal static class TypeCodeCache<T> {

  public static CachedTypeCode Code {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => typeof(T) == typeof(char) ? CachedTypeCode.Char :
      typeof(T) == typeof(byte) ? CachedTypeCode.Byte :
      typeof(T) == typeof(sbyte) ? CachedTypeCode.SByte :
      typeof(T) == typeof(ushort) ? CachedTypeCode.UInt16 :
      typeof(T) == typeof(short) ? CachedTypeCode.Int16 :
      typeof(T) == typeof(uint) ? CachedTypeCode.UInt32 :
      typeof(T) == typeof(int) ? CachedTypeCode.Int32 :
      typeof(T) == typeof(ulong) ? CachedTypeCode.UInt64 :
      typeof(T) == typeof(long) ? CachedTypeCode.Int64 :
      typeof(T) == typeof(float) ? CachedTypeCode.Single :
      typeof(T) == typeof(double) ? CachedTypeCode.Double :
      typeof(T) == typeof(decimal) ? CachedTypeCode.Decimal :
      typeof(T) == typeof(nint) ? CachedTypeCode.Pointer :
      typeof(T) == typeof(nuint) ? CachedTypeCode.UPointer :
      typeof(T) == typeof(bool) ? CachedTypeCode.Boolean :
      CachedTypeCode.Unknown;
  }
}
