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
/// Provides linear interpolation for 3-component byte color spaces.
/// </summary>
/// <remarks>
/// Uses integer math with rounding for fast byte-based lerp.
/// </remarks>
public readonly struct Color3BLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace3B<TWork> {

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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create3B<TWork>(
      (byte)(a.C1 * invT + b.C1 * t + 0.5f),
      (byte)(a.C2 * invT + b.C2 * t + 0.5f),
      (byte)(a.C3 * invT + b.C3 * t + 0.5f)
    );
  }
}

/// <summary>
/// Provides linear interpolation for 4-component byte color spaces.
/// </summary>
/// <remarks>
/// Uses integer math with rounding for fast byte-based lerp.
/// </remarks>
public readonly struct Color4BLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace4B<TWork> {

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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create4B<TWork>(
      (byte)(a.C1 * invT + b.C1 * t + 0.5f),
      (byte)(a.C2 * invT + b.C2 * t + 0.5f),
      (byte)(a.C3 * invT + b.C3 * t + 0.5f),
      (byte)(a.A * invT + b.A * t + 0.5f)
    );
  }
}

/// <summary>
/// Provides linear interpolation for 5-component byte color spaces.
/// </summary>
/// <remarks>
/// Uses integer math with rounding for fast byte-based lerp.
/// </remarks>
public readonly struct Color5BLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace5B<TWork> {

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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create5B<TWork>(
      (byte)(a.C1 * invT + b.C1 * t + 0.5f),
      (byte)(a.C2 * invT + b.C2 * t + 0.5f),
      (byte)(a.C3 * invT + b.C3 * t + 0.5f),
      (byte)(a.C4 * invT + b.C4 * t + 0.5f),
      (byte)(a.A * invT + b.A * t + 0.5f)
    );
  }
}

/// <summary>
/// Provides linear interpolation for 3-component float color spaces.
/// </summary>
public readonly struct Color3FLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace3F<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create3F<TWork>(
      (a.C1 + b.C1) * 0.5f,
      (a.C2 + b.C2) * 0.5f,
      (a.C3 + b.C3) * 0.5f
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var invTotal = 1f / (w1 + w2);
    return ColorFactory.Create3F<TWork>(
      (a.C1 * w1 + b.C1 * w2) * invTotal,
      (a.C2 * w1 + b.C2 * w2) * invTotal,
      (a.C3 * w1 + b.C3 * w2) * invTotal
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create3F<TWork>(
      a.C1 * invT + b.C1 * t,
      a.C2 * invT + b.C2 * t,
      a.C3 * invT + b.C3 * t
    );
  }
}

/// <summary>
/// Provides linear interpolation for 4-component float color spaces.
/// </summary>
public readonly struct Color4FLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace4F<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create4F<TWork>(
      (a.C1 + b.C1) * 0.5f,
      (a.C2 + b.C2) * 0.5f,
      (a.C3 + b.C3) * 0.5f,
      (a.A + b.A) * 0.5f
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var invTotal = 1f / (w1 + w2);
    return ColorFactory.Create4F<TWork>(
      (a.C1 * w1 + b.C1 * w2) * invTotal,
      (a.C2 * w1 + b.C2 * w2) * invTotal,
      (a.C3 * w1 + b.C3 * w2) * invTotal,
      (a.A * w1 + b.A * w2) * invTotal
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create4F<TWork>(
      a.C1 * invT + b.C1 * t,
      a.C2 * invT + b.C2 * t,
      a.C3 * invT + b.C3 * t,
      a.A * invT + b.A * t
    );
  }
}

/// <summary>
/// Provides linear interpolation for 5-component float color spaces.
/// </summary>
public readonly struct Color5FLerp<TWork> : ILerp<TWork> where TWork : unmanaged, IColorSpace5F<TWork> {

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b)
    => ColorFactory.Create5F<TWork>(
      (a.C1 + b.C1) * 0.5f,
      (a.C2 + b.C2) * 0.5f,
      (a.C3 + b.C3) * 0.5f,
      (a.C4 + b.C4) * 0.5f,
      (a.A + b.A) * 0.5f
    );

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, int w1, int w2) {
    var invTotal = 1f / (w1 + w2);
    return ColorFactory.Create5F<TWork>(
      (a.C1 * w1 + b.C1 * w2) * invTotal,
      (a.C2 * w1 + b.C2 * w2) * invTotal,
      (a.C3 * w1 + b.C3 * w2) * invTotal,
      (a.C4 * w1 + b.C4 * w2) * invTotal,
      (a.A * w1 + b.A * w2) * invTotal
    );
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public TWork Lerp(in TWork a, in TWork b, float t) {
    var invT = 1f - t;

    return ColorFactory.Create5F<TWork>(
      a.C1 * invT + b.C1 * t,
      a.C2 * invT + b.C2 * t,
      a.C3 * invT + b.C3 * t,
      a.C4 * invT + b.C4 * t,
      a.A * invT + b.A * t
    );
  }
}

/// <summary>
/// A no-op lerp implementation for scalers that don't use interpolation.
/// </summary>
/// <typeparam name="TWork">The working color type.</typeparam>
/// <remarks>
/// Returns the first color unchanged. Used as a placeholder type parameter
/// for scalers that don't perform color interpolation.
/// </remarks>
public readonly struct NoLerp<TWork> : ILerp<TWork> where TWork : unmanaged {

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
