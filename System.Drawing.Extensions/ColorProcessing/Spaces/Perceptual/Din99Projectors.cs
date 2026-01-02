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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Spaces.Perceptual;

/// <summary>
/// Projects LabF to Din99F.
/// </summary>
/// <remarks>
/// Uses the DIN 6176 transformation from CIE Lab to DIN99.
/// </remarks>
public readonly struct LabFToDin99F : IProject<LabF, Din99F> {

  private const float Cos16 = 0.9612616959383189f;  // cos(16°)
  private const float Sin16 = 0.27563735581699916f; // sin(16°)
  private const float KE = 1.0f;   // Reference white adjustment
  private const float KCH = 1.0f;  // Chroma/hue adjustment

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Din99F Project(in LabF lab) {
    // DIN99 transformation from Lab
    // L99 = 105.509 * ln(1 + 0.0158 * L*)
    var l99 = 105.509f * MathF.Log(1.0f + 0.0158f * lab.L);

    // Rotation of a,b axes by 16°
    var e = lab.A * Cos16 + lab.B * Sin16;
    var f = 0.7f * (-lab.A * Sin16 + lab.B * Cos16);

    // Calculate chroma and hue angle
    var g = MathF.Sqrt(e * e + f * f);

    // a99 and b99
    float a99, b99;
    if (g < 1e-6f) {
      a99 = 0f;
      b99 = 0f;
    } else {
      var c99 = MathF.Log(1.0f + 0.045f * g * KCH * KE) / (0.045f * KCH * KE);
      a99 = c99 * e / g;
      b99 = c99 * f / g;
    }

    return new(l99, a99, b99);
  }
}

/// <summary>
/// Projects Din99F back to LabF.
/// </summary>
public readonly struct Din99FToLabF : IProject<Din99F, LabF> {

  private const float Cos16 = 0.9612616959383189f;
  private const float Sin16 = 0.27563735581699916f;
  private const float KE = 1.0f;
  private const float KCH = 1.0f;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LabF Project(in Din99F din99) {
    // Inverse L99 transformation
    // L* = (exp(L99 / 105.509) - 1) / 0.0158
    var lStar = (MathF.Exp(din99.L / 105.509f) - 1f) / 0.0158f;

    // Calculate chroma in DIN99 space
    var c99 = MathF.Sqrt(din99.A * din99.A + din99.B * din99.B);

    if (c99 < 1e-6f)
      return new(lStar, 0f, 0f);

    // Inverse chroma transformation
    var g = (MathF.Exp(c99 * 0.045f * KCH * KE) - 1f) / (0.045f * KCH * KE);

    // Calculate e and f
    var e = g * din99.A / c99;
    var f = g * din99.B / c99 / 0.7f;

    // Inverse rotation
    var aStar = e * Cos16 - f * Sin16;
    var bStar = e * Sin16 + f * Cos16;

    return new(lStar, aStar, bStar);
  }
}

/// <summary>
/// Projects LinearRgbF to Din99F via Lab.
/// </summary>
public readonly struct LinearRgbFToDin99F : IProject<LinearRgbF, Din99F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Din99F Project(in LinearRgbF work) {
    var toLab = new LinearRgbFToLabF();
    var lab = toLab.Project(work);
    var toDin99 = new LabFToDin99F();
    return toDin99.Project(lab);
  }
}

/// <summary>
/// Projects LinearRgbaF to Din99F via Lab.
/// </summary>
public readonly struct LinearRgbaFToDin99F : IProject<LinearRgbaF, Din99F> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Din99F Project(in LinearRgbaF work) {
    var toLab = new LinearRgbaFToLabF();
    var lab = toLab.Project(work);
    var toDin99 = new LabFToDin99F();
    return toDin99.Project(lab);
  }
}

/// <summary>
/// Projects Din99F back to LinearRgbF via Lab.
/// </summary>
public readonly struct Din99FToLinearRgbF : IProject<Din99F, LinearRgbF> {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public LinearRgbF Project(in Din99F din99) {
    var toLab = new Din99FToLabF();
    var lab = toLab.Project(din99);
    var toRgb = new LabFToLinearRgbF();
    return toRgb.Project(lab);
  }
}
