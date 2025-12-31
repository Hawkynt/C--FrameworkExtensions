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

using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Provides pure integer linear interpolation for 3-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// <para>Implements both <see cref="ILerpInt{T}"/> and <see cref="ILerp{T}"/> for compatibility
/// with the scaler infrastructure while maintaining integer-only math.</para>
/// </remarks>
public readonly struct Color3BLerpInt<TWork> : ILerpInt<TWork>, ILerp<TWork> where TWork : unmanaged, IColorSpace3B<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create3B<TWork>(
      (byte)((a.C1 + b.C1) >> 1),
      (byte)((a.C2 + b.C2) >> 1),
      (byte)((a.C3 + b.C3) >> 1)
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var total = w1 + w2;
    return ColorFactory.Create3B<TWork>(
      (byte)((a.C1 * w1 + b.C1 * w2) / total),
      (byte)((a.C2 * w1 + b.C2 * w2) / total),
      (byte)((a.C3 * w1 + b.C3 * w2) / total)
    );
  }

  /// <inheritdoc />
  /// <remarks>Converts float t to integer weights (256 scale) and delegates to integer lerp.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var w2 = (int)(t * 256f + 0.5f);
    var w1 = 256 - w2;
    return this.Lerp(a, b, w1, w2);
  }
}

/// <summary>
/// Provides pure integer linear interpolation for 4-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// <para>Implements both <see cref="ILerpInt{T}"/> and <see cref="ILerp{T}"/> for compatibility
/// with the scaler infrastructure while maintaining integer-only math.</para>
/// </remarks>
public readonly struct Color4BLerpInt<TWork> : ILerpInt<TWork>, ILerp<TWork> where TWork : unmanaged, IColorSpace4B<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create4B<TWork>(
      (byte)((a.C1 + b.C1) >> 1),
      (byte)((a.C2 + b.C2) >> 1),
      (byte)((a.C3 + b.C3) >> 1),
      (byte)((a.A + b.A) >> 1)
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var total = w1 + w2;
    return ColorFactory.Create4B<TWork>(
      (byte)((a.C1 * w1 + b.C1 * w2) / total),
      (byte)((a.C2 * w1 + b.C2 * w2) / total),
      (byte)((a.C3 * w1 + b.C3 * w2) / total),
      (byte)((a.A * w1 + b.A * w2) / total)
    );
  }

  /// <inheritdoc />
  /// <remarks>Converts float t to integer weights (256 scale) and delegates to integer lerp.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var w2 = (int)(t * 256f + 0.5f);
    var w1 = 256 - w2;
    return this.Lerp(a, b, w1, w2);
  }
}

/// <summary>
/// Provides pure integer linear interpolation for 5-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// <para>Implements both <see cref="ILerpInt{T}"/> and <see cref="ILerp{T}"/> for compatibility
/// with the scaler infrastructure while maintaining integer-only math.</para>
/// </remarks>
public readonly struct Color5BLerpInt<TWork> : ILerpInt<TWork>, ILerp<TWork> where TWork : unmanaged, IColorSpace5B<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create5B<TWork>(
      (byte)((a.C1 + b.C1) >> 1),
      (byte)((a.C2 + b.C2) >> 1),
      (byte)((a.C3 + b.C3) >> 1),
      (byte)((a.C4 + b.C4) >> 1),
      (byte)((a.A + b.A) >> 1)
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var total = w1 + w2;
    return ColorFactory.Create5B<TWork>(
      (byte)((a.C1 * w1 + b.C1 * w2) / total),
      (byte)((a.C2 * w1 + b.C2 * w2) / total),
      (byte)((a.C3 * w1 + b.C3 * w2) / total),
      (byte)((a.C4 * w1 + b.C4 * w2) / total),
      (byte)((a.A * w1 + b.A * w2) / total)
    );
  }

  /// <inheritdoc />
  /// <remarks>Converts float t to integer weights (256 scale) and delegates to integer lerp.</remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var w2 = (int)(t * 256f + 0.5f);
    var w1 = 256 - w2;
    return this.Lerp(a, b, w1, w2);
  }
}

/// <summary>
/// A no-op integer lerp implementation for scalers that don't use interpolation.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <remarks>
/// <para>Returns the first color unchanged. Used as a placeholder type parameter
/// for scalers that don't perform color interpolation.</para>
/// <para>Implements both <see cref="ILerpInt{T}"/> and <see cref="ILerp{T}"/> for compatibility
/// with the scaler infrastructure.</para>
/// </remarks>
public readonly struct NoLerpInt<TWork> : ILerpInt<TWork>, ILerp<TWork> where TWork : unmanaged {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b) => a;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) => a;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) => a;
}
