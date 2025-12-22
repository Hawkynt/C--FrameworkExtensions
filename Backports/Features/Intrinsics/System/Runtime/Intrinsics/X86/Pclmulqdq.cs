#region (c)2010-2042 Hawkynt

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

#if !SUPPORTS_INTRINSICS

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Provides PCLMULQDQ (Carry-Less Multiplication) intrinsic operations.
/// This is a polyfill for older frameworks where PCLMULQDQ intrinsics are not available.
/// </summary>
public abstract class Pclmulqdq : Sse2 {

  /// <summary>
  /// Gets a value indicating whether PCLMULQDQ instructions are supported by the hardware.
  /// </summary>
  public new static bool IsSupported => false;

  /// <summary>
  /// Performs a carry-less multiplication of two 64-bit integers.
  /// </summary>
  /// <param name="left">The first operand.</param>
  /// <param name="right">The second operand.</param>
  /// <param name="control">Control byte specifying which quadwords to multiply.</param>
  /// <returns>The 128-bit result of the carry-less multiplication.</returns>
  public static Vector128<long> CarrylessMultiply(Vector128<long> left, Vector128<long> right, byte control)
    => NoIntrinsicsSupport.Throw<Vector128<long>>();

  /// <summary>
  /// Performs a carry-less multiplication of two 64-bit integers.
  /// </summary>
  public static Vector128<ulong> CarrylessMultiply(Vector128<ulong> left, Vector128<ulong> right, byte control)
    => NoIntrinsicsSupport.Throw<Vector128<ulong>>();

  /// <summary>
  /// Provides 64-bit specific PCLMULQDQ operations.
  /// </summary>
  public new abstract class X64 : Sse2.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit PCLMULQDQ instructions are supported.
    /// </summary>
    public new static bool IsSupported => false;
  }
}

#endif
