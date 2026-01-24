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

namespace System.Drawing.Extensions.ColorProcessing.Resizing;

/// <summary>
/// Defines prefilter parameters for B-spline and o-Moms interpolation.
/// </summary>
/// <remarks>
/// <para>
/// B-splines and o-Moms require converting discrete samples to spline coefficients
/// before interpolation. This is done using recursive IIR filtering with the
/// specified alpha (pole) coefficients.
/// </para>
/// <para>
/// The filter is applied as a cascade: for each alpha coefficient, a causal pass
/// followed by an anti-causal pass. This is applied separably (rows then columns).
/// </para>
/// <para>
/// After filtering, the result is scaled by Scale² (for 2D) to normalize.
/// </para>
/// </remarks>
public readonly struct PrefilterInfo {

  /// <summary>
  /// The pole coefficients for the recursive IIR filter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Each coefficient defines a first-order recursive filter pole.
  /// Multiple coefficients are applied in sequence (cascaded).
  /// </para>
  /// <para>
  /// Values are typically negative and between -1 and 0.
  /// Common values:
  /// <list type="bullet">
  /// <item>BSpline2: -0.1716 (exact: -3 + √8)</item>
  /// <item>BSpline3: -0.2679 (exact: √3 - 2)</item>
  /// <item>OMoms3: -0.3441 (exact: (√105 - 13) / 8)</item>
  /// </list>
  /// </para>
  /// </remarks>
  public float[] Alpha { get; }

  /// <summary>
  /// The normalization scale factor.
  /// </summary>
  /// <remarks>
  /// <para>
  /// For 1D filtering, multiply by this factor.
  /// For 2D separable filtering, multiply by Scale² after both passes.
  /// </para>
  /// <para>
  /// Common values:
  /// <list type="bullet">
  /// <item>BSpline2: 8</item>
  /// <item>BSpline3: 6</item>
  /// <item>BSpline5: 120</item>
  /// <item>OMoms3: 5.25 (exact: 21/4)</item>
  /// </list>
  /// </para>
  /// </remarks>
  public float Scale { get; }

  /// <summary>
  /// Creates a prefilter with a single alpha coefficient.
  /// </summary>
  /// <param name="alpha">The pole coefficient.</param>
  /// <param name="scale">The normalization scale factor.</param>
  public PrefilterInfo(float alpha, float scale) {
    this.Alpha = [alpha];
    this.Scale = scale;
  }

  /// <summary>
  /// Creates a prefilter with multiple cascaded alpha coefficients.
  /// </summary>
  /// <param name="alphas">The pole coefficients (applied in order).</param>
  /// <param name="scale">The normalization scale factor.</param>
  public PrefilterInfo(float[] alphas, float scale) {
    this.Alpha = alphas;
    this.Scale = scale;
  }

  #region Predefined Prefilters

  /// <summary>
  /// Prefilter for quadratic B-spline (BSpline2).
  /// </summary>
  /// <remarks>
  /// Alpha = -3 + √8 ≈ -0.1716, Scale = 8.
  /// </remarks>
  public static PrefilterInfo BSpline2 => new(-0.17157287525380990f, 8f);

  /// <summary>
  /// Prefilter for cubic B-spline (BSpline3).
  /// </summary>
  /// <remarks>
  /// Alpha = √3 - 2 ≈ -0.2679, Scale = 6.
  /// </remarks>
  public static PrefilterInfo BSpline3 => new(-0.26794919243112270f, 6f);

  /// <summary>
  /// Prefilter for quartic B-spline (BSpline4).
  /// </summary>
  /// <remarks>
  /// Two poles: α₁ ≈ -0.0620, α₂ ≈ -0.3799, Scale = 24.
  /// </remarks>
  public static PrefilterInfo BSpline4 => new(
    [-0.06198932482659890f, -0.37988461839253600f],
    24f);

  /// <summary>
  /// Prefilter for quintic B-spline (BSpline5).
  /// </summary>
  /// <remarks>
  /// Two poles: α₁ ≈ -0.0431, α₂ ≈ -0.4306, Scale = 120.
  /// </remarks>
  public static PrefilterInfo BSpline5 => new(
    [-0.04309628820326465f, -0.43057534709997460f],
    120f);

  /// <summary>
  /// Prefilter for septic B-spline (BSpline7).
  /// </summary>
  /// <remarks>
  /// Three poles, Scale = 5040.
  /// </remarks>
  public static PrefilterInfo BSpline7 => new(
    [-0.00914869480960866f, -0.12264942423286350f, -0.53528043079643820f],
    5040f);

  /// <summary>
  /// Prefilter for nonic B-spline (BSpline9).
  /// </summary>
  /// <remarks>
  /// Four poles, Scale = 362880.
  /// </remarks>
  public static PrefilterInfo BSpline9 => new(
    [-0.00209206919603087f, -0.04323181098750000f, -0.20175052019315324f, -0.60799738916862580f],
    362880f);

  /// <summary>
  /// Prefilter for 11th-degree B-spline (BSpline11).
  /// </summary>
  /// <remarks>
  /// Five poles, Scale = 39916800.
  /// </remarks>
  public static PrefilterInfo BSpline11 => new(
    [-0.00050630017228256f, -0.01673885346835230f, -0.08982424653987900f, -0.27216552070282880f, -0.66126606890073000f],
    39916800f);

  /// <summary>
  /// Prefilter for cubic o-Moms (OMoms3).
  /// </summary>
  /// <remarks>
  /// Alpha = (√105 - 13) / 8 ≈ -0.3441, Scale = 21/4 = 5.25.
  /// </remarks>
  public static PrefilterInfo OMoms3 => new(-0.34413803449364620f, 5.25f);

  /// <summary>
  /// Prefilter for quintic o-Moms (OMoms5).
  /// </summary>
  /// <remarks>
  /// Two poles, Scale ≈ 74.
  /// </remarks>
  public static PrefilterInfo OMoms5 => new(
    [-0.07092571997064390f, -0.47585963017993870f],
    74f);

  /// <summary>
  /// Prefilter for septic o-Moms (OMoms7).
  /// </summary>
  /// <remarks>
  /// Three poles, Scale ≈ 1952.8.
  /// </remarks>
  public static PrefilterInfo OMoms7 => new(
    [-0.01982420905512470f, -0.15568893196620870f, -0.56853094025808000f],
    1952.8f);

  #endregion
}
