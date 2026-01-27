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
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Error diffusion ditherer with linear (left-to-right) scan.
/// </summary>
/// <remarks>
/// <para>Use static fields for pre-configured matrices: <c>ErrorDiffusion.FloydSteinberg</c></para>
/// <para>Chain <c>.Serpentine</c> for alternating scan: <c>ErrorDiffusion.FloydSteinberg.Serpentine</c></para>
/// <para>This struct uses linear scanning. For serpentine scanning (alternating direction per row),
/// use <see cref="Serpentine"/> which returns a zero-cost <see cref="ErrorDiffusionSerpentine"/> struct.</para>
/// </remarks>
[Ditherer("Error Diffusion", Description = "Error diffusion dithering with various matrices", Type = DitheringType.ErrorDiffusion)]
public readonly struct ErrorDiffusion : IDitherer {

  #region fields

  internal readonly ErrorDiffusionData _data;

  #endregion

  #region properties

  /// <summary>Error diffusion strength (0-1). Default is 1.</summary>
  public float Strength => this._data.Strength;

  /// <summary>Number of rows in the diffusion matrix.</summary>
  public int RowCount => this._data.RowCount;

  /// <summary>Number of columns in the diffusion matrix.</summary>
  public int ColumnCount => this._data.ColumnCount;

  /// <summary>Column offset to the current pixel position.</summary>
  public int Shift => this._data.Shift;

  /// <summary>Divisor used to normalize the matrix weights.</summary>
  public int Divisor => this._data.Divisor;

  #endregion

  #region fluent API

  /// <summary>Returns a serpentine-scanning ditherer (alternates direction per row).</summary>
  /// <remarks>This is a zero-cost abstraction - the serpentine logic is baked into the type.</remarks>
  public ErrorDiffusionSerpentine Serpentine => new(this._data);

  /// <summary>Returns this ditherer with specified strength.</summary>
  public ErrorDiffusion WithStrength(float strength) => new(this._data.WithStrength(strength));

  #endregion

  #region constructors

  /// <summary>
  /// Creates an error diffusion ditherer from a matrix.
  /// </summary>
  /// <param name="matrix">
  /// The diffusion matrix where <c>X._</c> marks the current pixel position.
  /// Shift is auto-calculated from X position in row 0.
  /// Divisor is auto-calculated as sum of all weights.
  /// </param>
  /// <param name="strength">Error diffusion strength (0-1). Default is 1.</param>
  public ErrorDiffusion(byte[,] matrix, float strength = 1f)
    => this._data = new(matrix, strength);

  internal ErrorDiffusion(ErrorDiffusionData data) => this._data = data;

  #endregion

  #region IDitherer

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>
    => ErrorDiffusionCore.DitherLinear(
      source, indices, width, height, sourceStride, targetStride, startY,
      decoder, metric, palette, this._data);

  #endregion

  #region pre-configured matrices

  /// <summary>Floyd-Steinberg (1976): Classic 4-neighbor diffusion. Quality: 8/10</summary>
  /// <remarks>Reference: R.W. Floyd, L. Steinberg "An Adaptive Algorithm for Spatial Greyscale"</remarks>
  public static ErrorDiffusion FloydSteinberg { get; } = new(ErrorDiffusionData.FloydSteinberg);

  /// <summary>Equal Floyd-Steinberg: Equal weight distribution. Quality: 7/10</summary>
  public static ErrorDiffusion EqualFloydSteinberg { get; } = new(ErrorDiffusionData.EqualFloydSteinberg);

  /// <summary>False Floyd-Steinberg: Simplified 3-neighbor variant. Quality: 5/10</summary>
  public static ErrorDiffusion FalseFloydSteinberg { get; } = new(ErrorDiffusionData.FalseFloydSteinberg);

  /// <summary>Simple: Single neighbor diffusion. Quality: 3/10</summary>
  public static ErrorDiffusion Simple { get; } = new(ErrorDiffusionData.Simple);

  /// <summary>Jarvis-Judice-Ninke (1976): High quality 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: J.F. Jarvis, C.N. Judice, W.H. Ninke "A Survey of Techniques for the Display of Continuous Tone Pictures on Bilevel Displays"</remarks>
  public static ErrorDiffusion JarvisJudiceNinke { get; } = new(ErrorDiffusionData.JarvisJudiceNinke);

  /// <summary>Stucki (1981): Smooth gradients, 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: P. Stucki "MECCA - A Multiple-Error Correcting Computation Algorithm"</remarks>
  public static ErrorDiffusion Stucki { get; } = new(ErrorDiffusionData.Stucki);

  /// <summary>Atkinson: Apple Macintosh style, only 75% diffused. Quality: 7/10</summary>
  /// <remarks>Classic Macintosh look.</remarks>
  public static ErrorDiffusion Atkinson { get; } = new(ErrorDiffusionData.Atkinson);

  /// <summary>Burkes (1988): Simplified Stucki. Quality: 8/10</summary>
  public static ErrorDiffusion Burkes { get; } = new(ErrorDiffusionData.Burkes);

  /// <summary>Sierra (1989): 10-neighbor diffusion. Quality: 8/10</summary>
  /// <remarks>Reference: Frankie Sierra</remarks>
  public static ErrorDiffusion Sierra { get; } = new(ErrorDiffusionData.Sierra);

  /// <summary>Two-Row Sierra (1990): Faster 7-neighbor variant. Quality: 7/10</summary>
  /// <remarks>Reference: Frankie Sierra</remarks>
  public static ErrorDiffusion TwoRowSierra { get; } = new(ErrorDiffusionData.TwoRowSierra);

  /// <summary>Sierra Lite: Fastest Sierra, 3-neighbor. Quality: 6/10</summary>
  public static ErrorDiffusion SierraLite { get; } = new(ErrorDiffusionData.SierraLite);

  /// <summary>Stevenson-Arce (1985): Hexagonal sampling, 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: R.L. Stevenson, G.R. Arce "Binary Display of Hexagonally Sampled Continuous-Tone Images"</remarks>
  public static ErrorDiffusion StevensonArce { get; } = new(ErrorDiffusionData.StevensonArce);

  /// <summary>Pigeon: Steven Pigeon's algorithm. Quality: 7/10</summary>
  /// <remarks>Reference: https://hbfs.wordpress.com/2013/12/31/dithering/</remarks>
  public static ErrorDiffusion Pigeon { get; } = new(ErrorDiffusionData.Pigeon);

  /// <summary>Shiau-Fan (1993): Patent 5353127. Quality: 7/10</summary>
  /// <remarks>Reference: J.N. Shiau, Z. Fan US Patent 5353127</remarks>
  public static ErrorDiffusion ShiauFan { get; } = new(ErrorDiffusionData.ShiauFan);

  /// <summary>Shiau-Fan 2 (1993): Extended variant. Quality: 7/10</summary>
  /// <remarks>Reference: J.N. Shiau, Z. Fan</remarks>
  public static ErrorDiffusion ShiauFan2 { get; } = new(ErrorDiffusionData.ShiauFan2);

  /// <summary>Fan 93: Z. Fan's modification. Quality: 7/10</summary>
  /// <remarks>Reference: Z. Fan "A Simple Modification of Error Diffusion Weights"</remarks>
  public static ErrorDiffusion Fan93 { get; } = new(ErrorDiffusionData.Fan93);

  /// <summary>2D: Simple 2-neighbor. Quality: 4/10</summary>
  public static ErrorDiffusion TwoD { get; } = new(ErrorDiffusionData.TwoD);

  /// <summary>Down: Single pixel below. Quality: 3/10</summary>
  public static ErrorDiffusion Down { get; } = new(ErrorDiffusionData.Down);

  /// <summary>Double Down: Two rows down. Quality: 4/10</summary>
  public static ErrorDiffusion DoubleDown { get; } = new(ErrorDiffusionData.DoubleDown);

  /// <summary>Diagonal: Single diagonal neighbor. Quality: 4/10</summary>
  public static ErrorDiffusion Diagonal { get; } = new(ErrorDiffusionData.Diagonal);

  /// <summary>Vertical Diamond: Diamond pattern, vertical bias. Quality: 6/10</summary>
  public static ErrorDiffusion VerticalDiamond { get; } = new(ErrorDiffusionData.VerticalDiamond);

  /// <summary>Horizontal Diamond: Diamond pattern, horizontal bias. Quality: 6/10</summary>
  public static ErrorDiffusion HorizontalDiamond { get; } = new(ErrorDiffusionData.HorizontalDiamond);

  /// <summary>Diamond: Symmetric diamond pattern. Quality: 7/10</summary>
  public static ErrorDiffusion Diamond { get; } = new(ErrorDiffusionData.Diamond);

  #endregion

}

/// <summary>
/// Error diffusion ditherer with serpentine (alternating direction) scan.
/// </summary>
/// <remarks>
/// <para>Serpentine scanning alternates left-to-right and right-to-left on each row,
/// reducing directional artifacts common in error diffusion.</para>
/// <para>This is a zero-cost abstraction - use <c>ErrorDiffusion.FloydSteinberg.Serpentine</c> to obtain.</para>
/// </remarks>
[Ditherer("Error Diffusion (Serpentine)", Description = "Error diffusion with alternating scan direction", Type = DitheringType.ErrorDiffusion)]
public readonly struct ErrorDiffusionSerpentine : IDitherer {

  #region fields

  internal readonly ErrorDiffusionData _data;

  #endregion

  #region properties

  /// <summary>Error diffusion strength (0-1). Default is 1.</summary>
  public float Strength => this._data.Strength;

  /// <summary>Number of rows in the diffusion matrix.</summary>
  public int RowCount => this._data.RowCount;

  /// <summary>Number of columns in the diffusion matrix.</summary>
  public int ColumnCount => this._data.ColumnCount;

  /// <summary>Column offset to the current pixel position.</summary>
  public int Shift => this._data.Shift;

  /// <summary>Divisor used to normalize the matrix weights.</summary>
  public int Divisor => this._data.Divisor;

  #endregion

  #region fluent API

  /// <summary>Returns a linear-scanning ditherer (always left-to-right).</summary>
  public ErrorDiffusion Linear => new(this._data);

  /// <summary>Returns this ditherer with specified strength.</summary>
  public ErrorDiffusionSerpentine WithStrength(float strength) => new(this._data.WithStrength(strength));

  #endregion

  #region constructors

  /// <summary>
  /// Creates a serpentine error diffusion ditherer with Floyd-Steinberg matrix (default).
  /// </summary>
  /// <remarks>
  /// Use <c>ErrorDiffusion.FloydSteinberg.Serpentine</c> for explicit matrix selection.
  /// </remarks>
  public ErrorDiffusionSerpentine() => this._data = ErrorDiffusionData.FloydSteinberg;

  internal ErrorDiffusionSerpentine(ErrorDiffusionData data) => this._data = data;

  #endregion

  #region IDitherer

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork>
    => ErrorDiffusionCore.DitherSerpentine(
      source, indices, width, height, sourceStride, targetStride, startY,
      decoder, metric, palette, this._data);

  #endregion

}

/// <summary>
/// Shared matrix data for error diffusion ditherers.
/// </summary>
internal readonly struct ErrorDiffusionData {

  // Marker for current pixel position in matrix (treated as weight 0)
  private const byte X = byte.MaxValue;

  public readonly byte[] Weights;
  public readonly byte RowCount;
  public readonly byte ColumnCount;
  public readonly byte Shift;
  public readonly ushort Divisor;
  public readonly float Strength;

  public ErrorDiffusionData(byte[,] matrix, float strength = 1f) {
    var rows = matrix.GetLength(0);
    var cols = matrix.GetLength(1);

    this.RowCount = (byte)rows;
    this.ColumnCount = (byte)cols;
    this.Weights = new byte[rows * cols];
    this.Strength = Math.Clamp(strength, 0f, 1f);

    byte shift = 0;
    var divisor = 0;

    for (var r = 0; r < rows; ++r)
    for (var c = 0; c < cols; ++c) {
      var w = matrix[r, c];
      var idx = r * cols + c;

      if (w == X) {
        shift = (byte)c;
        this.Weights[idx] = 0;
      } else {
        this.Weights[idx] = w;
        divisor += w;
      }
    }

    this.Shift = shift;
    this.Divisor = (ushort)divisor;
  }

  private ErrorDiffusionData(byte[] weights, byte rowCount, byte columnCount, byte shift, ushort divisor, float strength) {
    this.Weights = weights;
    this.RowCount = rowCount;
    this.ColumnCount = columnCount;
    this.Shift = shift;
    this.Divisor = divisor;
    this.Strength = strength;
  }

  public ErrorDiffusionData WithStrength(float strength)
    => new(this.Weights, this.RowCount, this.ColumnCount, this.Shift, this.Divisor, Math.Clamp(strength, 0f, 1f));

  #region pre-configured matrices

  public static ErrorDiffusionData FloydSteinberg { get; } = new(new byte[,] {
    { 0, X, 7 },
    { 3, 5, 1 }
  });

  public static ErrorDiffusionData EqualFloydSteinberg { get; } = new(new byte[,] {
    { 0, X, 4 },
    { 4, 4, 4 }
  });

  public static ErrorDiffusionData FalseFloydSteinberg { get; } = new(new byte[,] {
    { X, 3 },
    { 3, 2 }
  });

  public static ErrorDiffusionData Simple { get; } = new(new byte[,] {
    { X, 1 }
  });

  public static ErrorDiffusionData JarvisJudiceNinke { get; } = new(new byte[,] {
    { 0, 0, X, 7, 5 },
    { 3, 5, 7, 5, 3 },
    { 1, 3, 5, 3, 1 }
  });

  public static ErrorDiffusionData Stucki { get; } = new(new byte[,] {
    { 0, 0, X, 8, 4 },
    { 2, 4, 8, 4, 2 },
    { 1, 2, 4, 2, 1 }
  });

  public static ErrorDiffusionData Atkinson { get; } = new(new byte[,] {
    { 0, X, 1, 1 },
    { 1, 1, 1, 0 },
    { 0, 1, 0, 0 }
  });

  public static ErrorDiffusionData Burkes { get; } = new(new byte[,] {
    { 0, 0, X, 8, 4 },
    { 2, 4, 8, 4, 2 }
  });

  public static ErrorDiffusionData Sierra { get; } = new(new byte[,] {
    { 0, 0, X, 5, 3 },
    { 2, 4, 5, 4, 2 },
    { 0, 2, 3, 2, 0 }
  });

  public static ErrorDiffusionData TwoRowSierra { get; } = new(new byte[,] {
    { 0, 0, X, 4, 3 },
    { 1, 2, 3, 2, 1 }
  });

  public static ErrorDiffusionData SierraLite { get; } = new(new byte[,] {
    { 0, X, 2 },
    { 1, 1, 0 }
  });

  public static ErrorDiffusionData StevensonArce { get; } = new(new byte[,] {
    {  0,  0,  0, X,  0, 32,  0 },
    { 12,  0, 26, 0, 30,  0, 16 },
    {  0, 12,  0,26,  0, 12,  0 },
    {  5,  0, 12, 0, 12,  0,  5 }
  });

  public static ErrorDiffusionData Pigeon { get; } = new(new byte[,] {
    { 0, 0, X, 2, 1 },
    { 0, 2, 2, 2, 0 },
    { 1, 0, 1, 0, 1 }
  });

  public static ErrorDiffusionData ShiauFan { get; } = new(new byte[,] {
    { 0, X, 4 },
    { 1, 1, 2 }
  });

  public static ErrorDiffusionData ShiauFan2 { get; } = new(new byte[,] {
    { 0, 0, X, 8 },
    { 1, 1, 2, 4 }
  });

  public static ErrorDiffusionData Fan93 { get; } = new(new byte[,] {
    { 0, X, 7 },
    { 1, 3, 5 }
  });

  public static ErrorDiffusionData TwoD { get; } = new(new byte[,] {
    { X, 1 },
    { 1, 0 }
  });

  public static ErrorDiffusionData Down { get; } = new(new byte[,] {
    { X },
    { 1 }
  });

  public static ErrorDiffusionData DoubleDown { get; } = new(new byte[,] {
    { X, 0 },
    { 2, 0 },
    { 1, 1 }
  });

  public static ErrorDiffusionData Diagonal { get; } = new(new byte[,] {
    { X, 0 },
    { 0, 1 }
  });

  public static ErrorDiffusionData VerticalDiamond { get; } = new(new byte[,] {
    { 0, 0, X, 0, 0 },
    { 0, 3, 6, 3, 0 },
    { 1, 0, 2, 0, 1 }
  });

  public static ErrorDiffusionData HorizontalDiamond { get; } = new(new byte[,] {
    { X, 6, 2 },
    { 0, 3, 0 },
    { 0, 0, 1 }
  });

  public static ErrorDiffusionData Diamond { get; } = new(new byte[,] {
    { 0, 0, X, 6, 2 },
    { 0, 3, 6, 3, 0 },
    { 1, 0, 2, 0, 1 }
  });

  #endregion

}

/// <summary>
/// Core error diffusion implementation shared between linear and serpentine variants.
/// </summary>
file static class ErrorDiffusionCore {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void DitherLinear<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette,
    in ErrorDiffusionData data)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    var rowStride = width * 4;
    var errors = new float[data.RowCount * rowStride];
    var invDivisor = 1f / data.Divisor;
    var baseRow = 0;

    for (var y = startY; y < endY; ++y) {
      var row0Offset = baseRow * rowStride;
      var rowSourceBase = y * sourceStride;
      var rowTargetBase = y * targetStride;

      // Linear: always left-to-right
      for (var x = 0; x < width; ++x) {
        var sourceIdx = rowSourceBase + x;
        var targetIdx = rowTargetBase + x;

        var color = decoder.Decode(source[sourceIdx]);
        var errIdx = row0Offset + x * 4;
        var adjustedColor = ApplyError(color, errors, errIdx, invDivisor, data.Strength);
        errors.AsSpan(errIdx, 4).Clear();

        var nearestIdx = lookup.FindNearest(adjustedColor, out var nearestColor);
        DistributeErrorLinear(adjustedColor, nearestColor, errors, x, y, width, endY,
          data.Weights, data.RowCount, data.ColumnCount, data.Shift, baseRow, rowStride);

        indices[targetIdx] = (byte)nearestIdx;
      }

      errors.AsSpan(row0Offset, rowStride).Clear();
      baseRow = (baseRow + 1) % data.RowCount;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static unsafe void DitherSerpentine<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette,
    in ErrorDiffusionData data)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    var rowStride = width * 4;
    var errors = new float[data.RowCount * rowStride];
    var invDivisor = 1f / data.Divisor;
    var baseRow = 0;

    for (var y = startY; y < endY; ++y) {
      var row0Offset = baseRow * rowStride;
      var rowSourceBase = y * sourceStride;
      var rowTargetBase = y * targetStride;

      // Serpentine: alternate direction each row
      var reverseRow = (y & 1) == 1;

      if (reverseRow) {
        // Right-to-left
        for (var x = width - 1; x >= 0; --x) {
          var sourceIdx = rowSourceBase + x;
          var targetIdx = rowTargetBase + x;

          var color = decoder.Decode(source[sourceIdx]);
          var errIdx = row0Offset + x * 4;
          var adjustedColor = ApplyError(color, errors, errIdx, invDivisor, data.Strength);
          errors.AsSpan(errIdx, 4).Clear();

          var nearestIdx = lookup.FindNearest(adjustedColor, out var nearestColor);
          DistributeErrorReverse(adjustedColor, nearestColor, errors, x, y, width, endY,
            data.Weights, data.RowCount, data.ColumnCount, data.Shift, baseRow, rowStride);

          indices[targetIdx] = (byte)nearestIdx;
        }
      } else {
        // Left-to-right
        for (var x = 0; x < width; ++x) {
          var sourceIdx = rowSourceBase + x;
          var targetIdx = rowTargetBase + x;

          var color = decoder.Decode(source[sourceIdx]);
          var errIdx = row0Offset + x * 4;
          var adjustedColor = ApplyError(color, errors, errIdx, invDivisor, data.Strength);
          errors.AsSpan(errIdx, 4).Clear();

          var nearestIdx = lookup.FindNearest(adjustedColor, out var nearestColor);
          DistributeErrorLinear(adjustedColor, nearestColor, errors, x, y, width, endY,
            data.Weights, data.RowCount, data.ColumnCount, data.Shift, baseRow, rowStride);

          indices[targetIdx] = (byte)nearestIdx;
        }
      }

      errors.AsSpan(row0Offset, rowStride).Clear();
      baseRow = (baseRow + 1) % data.RowCount;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork ApplyError<TWork>(in TWork color, float[] errors, int errIdx, float invDivisor, float strength)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var (c1, c2, c3, a) = color.ToNormalized();
    var scale = invDivisor * strength;
    return ColorFactory.FromNormalized_4<TWork>(
      UNorm32.FromFloatClamped(c1.ToFloat() + errors[errIdx] * scale),
      UNorm32.FromFloatClamped(c2.ToFloat() + errors[errIdx + 1] * scale),
      UNorm32.FromFloatClamped(c3.ToFloat() + errors[errIdx + 2] * scale),
      UNorm32.FromFloatClamped(a.ToFloat() + errors[errIdx + 3] * scale)
    );
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void DistributeErrorLinear<TWork>(
    in TWork adjustedColor,
    in TWork nearestColor,
    float[] errors,
    int x, int y,
    int width, int height,
    byte[] weights,
    int matrixRowCount, int colCount, int shift,
    int baseRow, int rowStride)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var (ac1, ac2, ac3, aa) = adjustedColor.ToNormalized();
    var (nc1, nc2, nc3, na) = nearestColor.ToNormalized();
    var e1 = ac1.ToFloat() - nc1.ToFloat();
    var e2 = ac2.ToFloat() - nc2.ToFloat();
    var e3 = ac3.ToFloat() - nc3.ToFloat();
    var ea = aa.ToFloat() - na.ToFloat();

    for (var row = 0; row < matrixRowCount; ++row) {
      var newY = y + row;
      if (newY >= height)
        break;

      var ringRow = (baseRow + row) % matrixRowCount;
      var ringRowOffset = ringRow * rowStride;

      for (var col = 0; col < colCount; ++col) {
        var weight = weights[row * colCount + col];
        if (weight == 0)
          continue;

        var newX = x + (col - shift);
        if (newX < 0 || newX >= width)
          continue;

        var targetIdx = ringRowOffset + newX * 4;
        errors[targetIdx] += e1 * weight;
        errors[targetIdx + 1] += e2 * weight;
        errors[targetIdx + 2] += e3 * weight;
        errors[targetIdx + 3] += ea * weight;
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void DistributeErrorReverse<TWork>(
    in TWork adjustedColor,
    in TWork nearestColor,
    float[] errors,
    int x, int y,
    int width, int height,
    byte[] weights,
    int matrixRowCount, int colCount, int shift,
    int baseRow, int rowStride)
    where TWork : unmanaged, IColorSpace4<TWork> {
    var (ac1, ac2, ac3, aa) = adjustedColor.ToNormalized();
    var (nc1, nc2, nc3, na) = nearestColor.ToNormalized();
    var e1 = ac1.ToFloat() - nc1.ToFloat();
    var e2 = ac2.ToFloat() - nc2.ToFloat();
    var e3 = ac3.ToFloat() - nc3.ToFloat();
    var ea = aa.ToFloat() - na.ToFloat();

    for (var row = 0; row < matrixRowCount; ++row) {
      var newY = y + row;
      if (newY >= height)
        break;

      var ringRow = (baseRow + row) % matrixRowCount;
      var ringRowOffset = ringRow * rowStride;

      for (var col = 0; col < colCount; ++col) {
        var weight = weights[row * colCount + col];
        if (weight == 0)
          continue;

        // Reverse direction: mirror column offset
        var newX = x - (col - shift);
        if (newX < 0 || newX >= width)
          continue;

        var targetIdx = ringRowOffset + newX * 4;
        errors[targetIdx] += e1 * weight;
        errors[targetIdx + 1] += e2 * weight;
        errors[targetIdx + 2] += e3 * weight;
        errors[targetIdx + 3] += ea * weight;
      }
    }
  }

}
