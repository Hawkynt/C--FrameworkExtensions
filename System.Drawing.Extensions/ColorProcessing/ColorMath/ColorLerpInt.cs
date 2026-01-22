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
using Hawkynt.ColorProcessing.Metrics;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Provides pure integer linear interpolation for 3-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// </remarks>
public readonly struct Color3BLerpInt<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace3B<TWork> {

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

}

/// <summary>
/// Provides pure integer linear interpolation for 4-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// </remarks>
public readonly struct Color4BLerpInt<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace4B<TWork> {

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

}

/// <summary>
/// Provides pure integer linear interpolation for 5-component byte color spaces.
/// </summary>
/// <remarks>
/// <para>Uses shift operations for 50/50 blends and integer division for weighted blends.</para>
/// </remarks>
public readonly struct Color5BLerpInt<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace5B<TWork> {

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

}

/// <summary>
/// Provides 32-bit precision linear interpolation for any 4-component color space via UNorm32.
/// </summary>
/// <remarks>
/// <para>Uses <see cref="UNorm32"/> arithmetic (32-bit unsigned normalized values) for maximum
/// integer precision without floating-point overhead.</para>
/// <para>Works with any type implementing <see cref="IColorSpace4{T}"/> via ToNormalized/FromNormalized.</para>
/// <para>Compared to <see cref="Color4BLerpInt{TWork}"/>, provides 16M× more precision (32-bit vs 8-bit).</para>
/// </remarks>
public readonly struct Color4UnormLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace4<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b) {
    var (a1, a2, a3, aa) = a.ToNormalized();
    var (b1, b2, b3, ba) = b.ToNormalized();
    return ColorFactory.FromNormalized_4<TWork>(
      UNorm32.Midpoint(a1, b1),
      UNorm32.Midpoint(a2, b2),
      UNorm32.Midpoint(a3, b3),
      UNorm32.Midpoint(aa, ba)
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var (a1, a2, a3, aa) = a.ToNormalized();
    var (b1, b2, b3, ba) = b.ToNormalized();
    var total = (ulong)(w1 + w2);
    return ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromRaw((uint)(((ulong)a1.RawValue * (uint)w1 + (ulong)b1.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a2.RawValue * (uint)w1 + (ulong)b2.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a3.RawValue * (uint)w1 + (ulong)b3.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)aa.RawValue * (uint)w1 + (ulong)ba.RawValue * (uint)w2) / total))
    );
  }

}

/// <summary>
/// Provides 32-bit precision linear interpolation for any 3-component color space via UNorm32.
/// </summary>
/// <remarks>
/// <para>Uses <see cref="UNorm32"/> arithmetic (32-bit unsigned normalized values) for maximum
/// integer precision without floating-point overhead.</para>
/// <para>Works with any type implementing <see cref="IColorSpace3{T}"/> via ToNormalized/FromNormalized.</para>
/// <para>Compared to <see cref="Color3BLerpInt{TWork}"/>, provides 16M× more precision (32-bit vs 8-bit).</para>
/// </remarks>
public readonly struct Color3UnormLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace3<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b) {
    var (a1, a2, a3) = a.ToNormalized();
    var (b1, b2, b3) = b.ToNormalized();
    return ColorFactory.FromNormalized_3<TWork>(
      UNorm32.Midpoint(a1, b1),
      UNorm32.Midpoint(a2, b2),
      UNorm32.Midpoint(a3, b3)
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var (a1, a2, a3) = a.ToNormalized();
    var (b1, b2, b3) = b.ToNormalized();
    var total = (ulong)(w1 + w2);
    return ColorFactory.FromNormalized_3<TWork>(
      UNorm32.FromRaw((uint)(((ulong)a1.RawValue * (uint)w1 + (ulong)b1.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a2.RawValue * (uint)w1 + (ulong)b2.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a3.RawValue * (uint)w1 + (ulong)b3.RawValue * (uint)w2) / total))
    );
  }

}

/// <summary>
/// Provides 32-bit precision linear interpolation for any 5-component color space via UNorm32.
/// </summary>
/// <remarks>
/// <para>Uses <see cref="UNorm32"/> arithmetic (32-bit unsigned normalized values) for maximum
/// integer precision without floating-point overhead.</para>
/// <para>Works with any type implementing <see cref="IColorSpace5{T}"/> via ToNormalized/FromNormalized.</para>
/// <para>Compared to <see cref="Color5BLerpInt{TWork}"/>, provides 16M× more precision (32-bit vs 8-bit).</para>
/// </remarks>
public readonly struct Color5UnormLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace5<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b) {
    var (a1, a2, a3, a4, aa) = a.ToNormalized();
    var (b1, b2, b3, b4, ba) = b.ToNormalized();
    return ColorFactory.FromNormalized_5<TWork>(
      UNorm32.Midpoint(a1, b1),
      UNorm32.Midpoint(a2, b2),
      UNorm32.Midpoint(a3, b3),
      UNorm32.Midpoint(a4, b4),
      UNorm32.Midpoint(aa, ba)
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var (a1, a2, a3, a4, aa) = a.ToNormalized();
    var (b1, b2, b3, b4, ba) = b.ToNormalized();
    var total = (ulong)(w1 + w2);
    return ColorFactory.FromNormalized_5<TWork>(
      UNorm32.FromRaw((uint)(((ulong)a1.RawValue * (uint)w1 + (ulong)b1.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a2.RawValue * (uint)w1 + (ulong)b2.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a3.RawValue * (uint)w1 + (ulong)b3.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)a4.RawValue * (uint)w1 + (ulong)b4.RawValue * (uint)w2) / total)),
      UNorm32.FromRaw((uint)(((ulong)aa.RawValue * (uint)w1 + (ulong)ba.RawValue * (uint)w2) / total))
    );
  }

}

/// <summary>
/// A no-op integer lerp implementation for scalers that don't use interpolation.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <remarks>
/// <para>Returns the first color unchanged. Used as a placeholder type parameter
/// for scalers that don't perform color interpolation.</para>
/// </remarks>
public readonly struct NoLerpInt<TWork> : ILerp<TWork> where TWork : unmanaged {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b) => a;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) => a;
}
