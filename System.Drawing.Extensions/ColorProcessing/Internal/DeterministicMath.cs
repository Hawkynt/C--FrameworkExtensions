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

namespace Hawkynt.ColorProcessing.Internal;

/// <summary>
/// Deterministic, integer-only math helpers.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="System.Math.Cbrt(double)"/> is platform-dependent: net48 may return
/// <c>3.9999...</c> for the cube root of 64, while net6+ returns exactly <c>4.0</c>.
/// That single bit can flip a <c>(int)Math.Floor</c> / <c>Math.Ceiling</c> / <c>Math.Round</c>
/// result, leading to non-deterministic palette structures across target frameworks.
/// </para>
/// <para>
/// These helpers perform the cube-root operation using only integer arithmetic, so the
/// results are bit-identical on every TFM.
/// </para>
/// </remarks>
internal static class DeterministicMath {

  /// <summary>
  /// Returns the smallest non-negative integer <c>k</c> such that <c>k³ &gt;= n</c>.
  /// Equivalent to <c>(int)Math.Ceiling(Math.Cbrt(n))</c> but deterministic across TFMs.
  /// </summary>
  /// <param name="n">A non-negative integer.</param>
  /// <returns>The integer ceiling of the cube root of <paramref name="n"/>.</returns>
  public static int IntCbrtCeil(int n) {
    if (n <= 0)
      return 0;
    var k = 1;
    // Guard against overflow: 1290² × 1290 > int.MaxValue, but inputs here are
    // small palette counts (<= 256). Keep an explicit cap nonetheless.
    while (k < 1290 && k * k * k < n)
      ++k;
    return k;
  }

  /// <summary>
  /// Returns the integer <c>k</c> minimising <c>|k³ - n|</c>.
  /// Equivalent to <c>(int)Math.Round(Math.Cbrt(n))</c> but deterministic across TFMs.
  /// </summary>
  /// <param name="n">A non-negative integer.</param>
  /// <returns>The integer rounded to the nearest cube root of <paramref name="n"/>.</returns>
  /// <remarks>
  /// Ties (when <paramref name="n"/> is exactly halfway between two cubes) round to the
  /// larger value, matching the most common rounding mode used by the affected callers.
  /// </remarks>
  public static int IntCbrtRound(int n) {
    if (n <= 0)
      return 0;
    var ceil = IntCbrtCeil(n);
    if (ceil == 0)
      return 0;
    var floor = ceil;
    if ((long)ceil * ceil * ceil > n)
      --floor;
    if (floor < 0)
      return 0;
    var floorCube = (long)floor * floor * floor;
    var ceilCube = (long)ceil * ceil * ceil;
    var floorDiff = n - floorCube;
    var ceilDiff = ceilCube - n;
    return ceilDiff <= floorDiff ? ceil : floor;
  }
}
