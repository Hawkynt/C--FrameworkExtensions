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

#if !FEATURE_X86BASE_WAVE1

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Provides a base class for x86 intrinsics with support detection.
/// This is a polyfill for older frameworks where intrinsics are not available.
/// </summary>
public abstract class X86Base {

  /// <summary>
  /// Gets a value indicating whether x86 base instructions are supported.
  /// </summary>
  /// <value>Always <see langword="false"/> in this polyfill implementation.</value>
  public static bool IsSupported => false;

  /// <summary>
  /// Provides 64-bit specific x86 base operations.
  /// </summary>
  public abstract class X64 {

    /// <summary>
    /// Gets a value indicating whether 64-bit x86 base instructions are supported.
    /// </summary>
    /// <value>Always <see langword="false"/> in this polyfill implementation.</value>
    public static bool IsSupported => false;
  }
}

#endif
