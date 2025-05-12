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

#if !SUPPORTS_INTRINSICS

namespace System.Runtime.Intrinsics.X86;

public static class Sse2 {
  public static bool IsSupported => false;

  public static unsafe Vector128<byte> LoadVector128(byte* source) => NoIntrinsicsSupport.Throw<Vector128<byte>>();
  public static Vector128<byte> Xor(Vector128<byte> xmm0, Vector128<byte> xmm1) => NoIntrinsicsSupport.Throw<Vector128<byte>>();
  public static unsafe void Store(byte* target, Vector128<byte> xmm0) => NoIntrinsicsSupport.Throw();

}

#endif