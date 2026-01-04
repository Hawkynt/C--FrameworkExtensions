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

// Architecture was added in .NET Standard 1.1 / .NET Core 1.0
// Available via NuGet package for net45+
#if !SUPPORTS_RUNTIME_INFORMATION && !OFFICIAL_RUNTIME_INFORMATION

namespace System.Runtime.InteropServices;

/// <summary>
/// Indicates the processor architecture.
/// </summary>
public enum Architecture {
  /// <summary>
  /// An Intel-based 32-bit processor architecture.
  /// </summary>
  X86 = 0,

  /// <summary>
  /// An Intel-based 64-bit processor architecture.
  /// </summary>
  X64 = 1,

  /// <summary>
  /// A 32-bit ARM processor architecture.
  /// </summary>
  Arm = 2,

  /// <summary>
  /// A 64-bit ARM processor architecture.
  /// </summary>
  Arm64 = 3,

  /// <summary>
  /// The WebAssembly platform.
  /// </summary>
  Wasm = 4,

  /// <summary>
  /// The S390x platform architecture.
  /// </summary>
  S390x = 5,

  /// <summary>
  /// A LoongArch64 processor architecture.
  /// </summary>
  LoongArch64 = 6,

  /// <summary>
  /// A 32-bit ARMv6 processor architecture.
  /// </summary>
  Armv6 = 7,

  /// <summary>
  /// A PowerPC 64-bit (little-endian) processor architecture.
  /// </summary>
  Ppc64le = 8,

  /// <summary>
  /// A RISC-V 64-bit processor architecture.
  /// </summary>
  RiscV64 = 9
}

#endif
