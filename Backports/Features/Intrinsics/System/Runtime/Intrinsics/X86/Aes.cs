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
/// Provides AES-NI intrinsic operations.
/// This is a polyfill for older frameworks where AES-NI intrinsics are not available.
/// </summary>
public abstract class Aes : Sse2 {

  /// <summary>
  /// Gets a value indicating whether AES-NI instructions are supported by the hardware.
  /// </summary>
  public new static bool IsSupported => false;

  /// <summary>
  /// Performs one round of AES decryption.
  /// </summary>
  public static Vector128<byte> Decrypt(Vector128<byte> value, Vector128<byte> roundKey)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Performs the last round of AES decryption.
  /// </summary>
  public static Vector128<byte> DecryptLast(Vector128<byte> value, Vector128<byte> roundKey)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Performs one round of AES encryption.
  /// </summary>
  public static Vector128<byte> Encrypt(Vector128<byte> value, Vector128<byte> roundKey)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Performs the last round of AES encryption.
  /// </summary>
  public static Vector128<byte> EncryptLast(Vector128<byte> value, Vector128<byte> roundKey)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Performs the InvMixColumns transformation.
  /// </summary>
  public static Vector128<byte> InverseMixColumns(Vector128<byte> value)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Assists in AES key expansion.
  /// </summary>
  public static Vector128<byte> KeygenAssist(Vector128<byte> value, byte control)
    => NoIntrinsicsSupport.Throw<Vector128<byte>>();

  /// <summary>
  /// Provides 64-bit specific AES operations.
  /// </summary>
  public new abstract class X64 : Sse2.X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit AES-NI instructions are supported.
    /// </summary>
    public new static bool IsSupported => false;
  }
}

#endif
