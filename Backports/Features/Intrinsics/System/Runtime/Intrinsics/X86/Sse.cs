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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Software fallback implementation of SSE intrinsics.
/// </summary>
public abstract class Sse : X86Base {

  /// <summary>Gets a value indicating whether SSE instructions are supported.</summary>
  public new static bool IsSupported => false;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe Vector128<float> LoadVector128(float* source)
    => Vector128.Create(source[0], source[1], source[2], source[3]);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void Store(float* destination, Vector128<float> source) {
    for (var i = 0; i < 4; ++i)
      destination[i] = Vector128.GetElement(source, i);
  }

  /// <summary>Provides 64-bit specific SSE operations.</summary>
  public new abstract class X64 : X86Base.X64 {

    /// <summary>Gets a value indicating whether 64-bit SSE instructions are supported.</summary>
    public new static bool IsSupported => false;
  }
}

#endif