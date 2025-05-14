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

#if !SUPPORTS_INTRINSICS

namespace System.Runtime.Intrinsics.X86;

public static class Avx {
  public static bool IsSupported => false;

  public static unsafe Vector256<byte> LoadVector256(byte* source) => NoIntrinsicsSupport.Throw<Vector256<byte>>();
  public static unsafe void Store(byte* target, Vector256<byte> ymm0) => NoIntrinsicsSupport.Throw();

}

#endif