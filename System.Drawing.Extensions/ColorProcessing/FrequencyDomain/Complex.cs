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

using System;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.FrequencyDomain;

/// <summary>
/// A simple complex number type for FFT/DCT operations.
/// </summary>
public readonly struct Complex(float real, float imaginary) {
  public float Real { get; } = real;
  public float Imaginary { get; } = imaginary;

  public float Magnitude {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (float)Math.Sqrt(Real * Real + Imaginary * Imaginary);
  }

  public float Phase {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => (float)Math.Atan2(Imaginary, Real);
  }

  public Complex Conjugate {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => new(Real, -Imaginary);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex FromPolar(float magnitude, float phase)
    => new(magnitude * (float)Math.Cos(phase), magnitude * (float)Math.Sin(phase));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex operator +(Complex a, Complex b) => new(a.Real + b.Real, a.Imaginary + b.Imaginary);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex operator -(Complex a, Complex b) => new(a.Real - b.Real, a.Imaginary - b.Imaginary);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex operator *(Complex a, Complex b)
    => new(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex operator *(Complex a, float s) => new(a.Real * s, a.Imaginary * s);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Complex operator *(float s, Complex a) => new(a.Real * s, a.Imaginary * s);

  public static readonly Complex Zero = new(0f, 0f);
  public static readonly Complex One = new(1f, 0f);
  public static readonly Complex ImaginaryOne = new(0f, 1f);

  public override string ToString() => $"({Real}, {Imaginary}i)";
}
