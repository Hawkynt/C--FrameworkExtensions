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
/// Error diffusion ditherer with embedded matrix data.
/// </summary>
/// <remarks>
/// <para>Use static fields for pre-configured matrices: <c>ErrorDiffusion.FloydSteinberg</c></para>
/// <para>Chain properties for configuration: <c>ErrorDiffusion.FloydSteinberg.Serpentine</c></para>
/// </remarks>
[Ditherer("Error Diffusion", Description = "Error diffusion dithering with various matrices", Type = DitheringType.ErrorDiffusion)]
public readonly struct ErrorDiffusion : IDitherer {

  // Marker for current pixel position in matrix (treated as weight 0)
  private const byte X = byte.MaxValue;

  #region fields

  private readonly byte[] _weights;
  private readonly byte _rowCount;
  private readonly byte _columnCount;
  private readonly byte _shift;
  private readonly ushort _divisor;

  #endregion

  #region properties

  /// <summary>Error diffusion strength (0-1). Default is 1.</summary>
  public float Strength { get; }

  /// <summary>If true, alternates scan direction per row for reduced artifacts.</summary>
  public bool UseSerpentine { get; }

  /// <summary>Number of rows in the diffusion matrix.</summary>
  public int RowCount => this._rowCount;

  /// <summary>Number of columns in the diffusion matrix.</summary>
  public int ColumnCount => this._columnCount;

  /// <summary>Column offset to the current pixel position.</summary>
  public int Shift => this._shift;

  /// <summary>Divisor used to normalize the matrix weights.</summary>
  public int Divisor => this._divisor;

  #endregion

  #region fluent API

  /// <summary>Returns this ditherer with serpentine scan enabled.</summary>
  public ErrorDiffusion Serpentine => new(this._weights, this._rowCount, this._columnCount, this._shift, this._divisor, this.Strength, true);

  /// <summary>Returns this ditherer with specified strength.</summary>
  public ErrorDiffusion WithStrength(float strength) => new(this._weights, this._rowCount, this._columnCount, this._shift, this._divisor, strength, this.UseSerpentine);

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
  /// <param name="useSerpentine">If true, alternates scan direction per row.</param>
  public ErrorDiffusion(byte[,] matrix, float strength = 1f, bool useSerpentine = false) {
    var rows = matrix.GetLength(0);
    var cols = matrix.GetLength(1);

    this._rowCount = (byte)rows;
    this._columnCount = (byte)cols;
    this._weights = new byte[rows * cols];
    this.Strength = Math.Clamp(strength, 0f, 1f);
    this.UseSerpentine = useSerpentine;

    // Find X position in row 0 and calculate divisor
    byte shift = 0;
    var divisor = 0;

    for (var r = 0; r < rows; ++r)
    for (var c = 0; c < cols; ++c) {
      var w = matrix[r, c];
      var idx = r * cols + c;

      if (w == X) {
        shift = (byte)c;
        this._weights[idx] = 0;
      } else {
        this._weights[idx] = w;
        divisor += w;
      }
    }

    this._shift = shift;
    this._divisor = (ushort)divisor;
  }

  // Private constructor for fluent API
  private ErrorDiffusion(byte[] weights, byte rowCount, byte columnCount, byte shift, ushort divisor, float strength, bool useSerpentine) {
    this._weights = weights;
    this._rowCount = rowCount;
    this._columnCount = columnCount;
    this._shift = shift;
    this._divisor = divisor;
    this.Strength = strength;
    this.UseSerpentine = useSerpentine;
  }

  #endregion

  #region methods

  /// <summary>Gets the weight at the specified row and column.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int GetWeight(int row, int column) => this._weights[row * this._columnCount + column];

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
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;

    // Error buffer as ring buffer: [rowCount, width, 4 channels]
    var rowStride = width * 4;
    var errors = new float[this._rowCount * rowStride];
    var invDivisor = 1f / this._divisor;
    var baseRow = 0; // Ring buffer base index

    for (var y = startY; y < endY; ++y) {
      // Serpentine: alternate direction each row
      var reverseRow = this.UseSerpentine && (y & 1) == 1;

      // Current row offset in ring buffer (row 0 = current row being processed)
      var row0Offset = baseRow * rowStride;

      // Pre-calculate row base indices
      var rowSourceBase = y * sourceStride;
      var rowTargetBase = y * targetStride;

      // Direction-specific setup
      int x, xEnd, xStep, sourceIdx, targetIdx;
      if (reverseRow) {
        x = width - 1;
        xEnd = -1;
        xStep = -1;
        sourceIdx = rowSourceBase + width - 1;
        targetIdx = rowTargetBase + width - 1;
      } else {
        x = 0;
        xEnd = width;
        xStep = 1;
        sourceIdx = rowSourceBase;
        targetIdx = rowTargetBase;
      }

      for (; x != xEnd; x += xStep, sourceIdx += xStep, targetIdx += xStep) {
        // Decode source pixel
        var color = decoder.Decode(source[sourceIdx]);

        // Get accumulated error for this pixel and apply
        var errIdx = row0Offset + x * 4;
        var adjustedColor = _ApplyError(color, errors, errIdx, invDivisor, this.Strength);

        // Clear this pixel's error using span
        errors.AsSpan(errIdx, 4).Clear();

        // Find nearest palette color
        var nearestIdx = lookup.FindNearest(adjustedColor, out var nearestColor);

        // Calculate and distribute error using ring buffer
        _DistributeErrorRing(adjustedColor, nearestColor, errors, x, y, width, endY,
          reverseRow, this._weights, this._rowCount, this._columnCount, this._shift, baseRow, rowStride);

        // Store the palette index
        indices[targetIdx] = (byte)nearestIdx;
      }

      // Rotate ring buffer: clear current row (becomes last row after rotation)
      errors.AsSpan(row0Offset, rowStride).Clear();

      // Advance base row in ring buffer
      baseRow = (baseRow + 1) % this._rowCount;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static TWork _ApplyError<TWork>(in TWork color, float[] errors, int errIdx, float invDivisor, float strength)
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
  private static void _DistributeErrorRing<TWork>(
    in TWork adjustedColor,
    in TWork nearestColor,
    float[] errors,
    int x, int y,
    int width, int height,
    bool reverseRow,
    byte[] weights,
    int matrixRowCount, int colCount, int shift,
    int baseRow, int rowStride)
    where TWork : unmanaged, IColorSpace4<TWork> {
    // Calculate quantization error
    var (ac1, ac2, ac3, aa) = adjustedColor.ToNormalized();
    var (nc1, nc2, nc3, na) = nearestColor.ToNormalized();
    var e1 = ac1.ToFloat() - nc1.ToFloat();
    var e2 = ac2.ToFloat() - nc2.ToFloat();
    var e3 = ac3.ToFloat() - nc3.ToFloat();
    var ea = aa.ToFloat() - na.ToFloat();

    // Distribute error using matrix with ring buffer indexing
    for (var row = 0; row < matrixRowCount; ++row) {
      var newY = y + row;
      if (newY >= height)
        break;

      // Ring buffer row index
      var ringRow = (baseRow + row) % matrixRowCount;
      var ringRowOffset = ringRow * rowStride;

      for (var col = 0; col < colCount; ++col) {
        var weight = weights[row * colCount + col];
        if (weight == 0)
          continue;

        var newX = reverseRow ? x - (col - shift) : x + (col - shift);
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

  #endregion

  #region pre-configured matrices

  /// <summary>Floyd-Steinberg (1976): Classic 4-neighbor diffusion. Quality: 8/10</summary>
  /// <remarks>Reference: R.W. Floyd, L. Steinberg "An Adaptive Algorithm for Spatial Greyscale"</remarks>
  public static ErrorDiffusion FloydSteinberg { get; } = new(new byte[,] {
    //     X 7
    //   3 5 1
    { 0, X, 7 },
    { 3, 5, 1 }
  });

  /// <summary>Equal Floyd-Steinberg: Equal weight distribution. Quality: 7/10</summary>
  public static ErrorDiffusion EqualFloydSteinberg { get; } = new(new byte[,] {
    //     X 4
    //   4 4 4
    { 0, X, 4 },
    { 4, 4, 4 }
  });

  /// <summary>False Floyd-Steinberg: Simplified 3-neighbor variant. Quality: 5/10</summary>
  public static ErrorDiffusion FalseFloydSteinberg { get; } = new(new byte[,] {
    //   X 3
    //   3 2
    { X, 3 },
    { 3, 2 }
  });

  /// <summary>Simple: Single neighbor diffusion. Quality: 3/10</summary>
  public static ErrorDiffusion Simple { get; } = new(new byte[,] {
    //   X 1
    { X, 1 }
  });

  /// <summary>Jarvis-Judice-Ninke (1976): High quality 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: J.F. Jarvis, C.N. Judice, W.H. Ninke "A Survey of Techniques for the Display of Continuous Tone Pictures on Bilevel Displays"</remarks>
  public static ErrorDiffusion JarvisJudiceNinke { get; } = new(new byte[,] {
    //       X 7 5
    //   3 5 7 5 3
    //   1 3 5 3 1
    { 0, 0, X, 7, 5 },
    { 3, 5, 7, 5, 3 },
    { 1, 3, 5, 3, 1 }
  });

  /// <summary>Stucki (1981): Smooth gradients, 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: P. Stucki "MECCA - A Multiple-Error Correcting Computation Algorithm"</remarks>
  public static ErrorDiffusion Stucki { get; } = new(new byte[,] {
    //       X 8 4
    //   2 4 8 4 2
    //   1 2 4 2 1
    { 0, 0, X, 8, 4 },
    { 2, 4, 8, 4, 2 },
    { 1, 2, 4, 2, 1 }
  });

  /// <summary>Atkinson: Apple Macintosh style, only 75% diffused. Quality: 7/10</summary>
  /// <remarks>Classic Macintosh look.</remarks>
  public static ErrorDiffusion Atkinson { get; } = new(new byte[,] {
    //     X 1 1
    //   1 1 1
    //     1
    { 0, X, 1, 1 },
    { 1, 1, 1, 0 },
    { 0, 1, 0, 0 }
  });

  /// <summary>Burkes (1988): Simplified Stucki. Quality: 8/10</summary>
  public static ErrorDiffusion Burkes { get; } = new(new byte[,] {
    //       X 8 4
    //   2 4 8 4 2
    { 0, 0, X, 8, 4 },
    { 2, 4, 8, 4, 2 }
  });

  /// <summary>Sierra (1989): 10-neighbor diffusion. Quality: 8/10</summary>
  /// <remarks>Reference: Frankie Sierra</remarks>
  public static ErrorDiffusion Sierra { get; } = new(new byte[,] {
    //       X 5 3
    //   2 4 5 4 2
    //     2 3 2
    { 0, 0, X, 5, 3 },
    { 2, 4, 5, 4, 2 },
    { 0, 2, 3, 2, 0 }
  });

  /// <summary>Two-Row Sierra (1990): Faster 7-neighbor variant. Quality: 7/10</summary>
  /// <remarks>Reference: Frankie Sierra</remarks>
  public static ErrorDiffusion TwoRowSierra { get; } = new(new byte[,] {
    //       X 4 3
    //   1 2 3 2 1
    { 0, 0, X, 4, 3 },
    { 1, 2, 3, 2, 1 }
  });

  /// <summary>Sierra Lite: Fastest Sierra, 3-neighbor. Quality: 6/10</summary>
  public static ErrorDiffusion SierraLite { get; } = new(new byte[,] {
    //     X 2
    //   1 1
    { 0, X, 2 },
    { 1, 1, 0 }
  });

  /// <summary>Stevenson-Arce (1985): Hexagonal sampling, 12-neighbor. Quality: 9/10</summary>
  /// <remarks>Reference: R.L. Stevenson, G.R. Arce "Binary Display of Hexagonally Sampled Continuous-Tone Images"</remarks>
  public static ErrorDiffusion StevensonArce { get; } = new(new byte[,] {
    //          X  0 32  0
    //   12  0 26  0 30  0 16
    //    0 12  0 26  0 12  0
    //    5  0 12  0 12  0  5
    {  0,  0,  0, X,  0, 32,  0 },
    { 12,  0, 26, 0, 30,  0, 16 },
    {  0, 12,  0,26,  0, 12,  0 },
    {  5,  0, 12, 0, 12,  0,  5 }
  });

  /// <summary>Pigeon: Steven Pigeon's algorithm. Quality: 7/10</summary>
  /// <remarks>Reference: https://hbfs.wordpress.com/2013/12/31/dithering/</remarks>
  public static ErrorDiffusion Pigeon { get; } = new(new byte[,] {
    //       X 2 1
    //     2 2 2
    //   1   1   1
    { 0, 0, X, 2, 1 },
    { 0, 2, 2, 2, 0 },
    { 1, 0, 1, 0, 1 }
  });

  /// <summary>Shiau-Fan (1993): Patent 5353127. Quality: 7/10</summary>
  /// <remarks>Reference: J.N. Shiau, Z. Fan US Patent 5353127</remarks>
  public static ErrorDiffusion ShiauFan { get; } = new(new byte[,] {
    //     X 4
    //   1 1 2
    { 0, X, 4 },
    { 1, 1, 2 }
  });

  /// <summary>Shiau-Fan 2 (1993): Extended variant. Quality: 7/10</summary>
  /// <remarks>Reference: J.N. Shiau, Z. Fan</remarks>
  public static ErrorDiffusion ShiauFan2 { get; } = new(new byte[,] {
    //       X 8
    //   1 1 2 4
    { 0, 0, X, 8 },
    { 1, 1, 2, 4 }
  });

  /// <summary>Fan 93: Z. Fan's modification. Quality: 7/10</summary>
  /// <remarks>Reference: Z. Fan "A Simple Modification of Error Diffusion Weights"</remarks>
  public static ErrorDiffusion Fan93 { get; } = new(new byte[,] {
    //     X 7
    //   1 3 5
    { 0, X, 7 },
    { 1, 3, 5 }
  });

  /// <summary>2D: Simple 2-neighbor. Quality: 4/10</summary>
  public static ErrorDiffusion TwoD { get; } = new(new byte[,] {
    //   X 1
    //   1
    { X, 1 },
    { 1, 0 }
  });

  /// <summary>Down: Single pixel below. Quality: 3/10</summary>
  public static ErrorDiffusion Down { get; } = new(new byte[,] {
    //   X
    //   1
    { X },
    { 1 }
  });

  /// <summary>Double Down: Two rows down. Quality: 4/10</summary>
  public static ErrorDiffusion DoubleDown { get; } = new(new byte[,] {
    //   X
    //   2
    //   1 1
    { X, 0 },
    { 2, 0 },
    { 1, 1 }
  });

  /// <summary>Diagonal: Single diagonal neighbor. Quality: 4/10</summary>
  public static ErrorDiffusion Diagonal { get; } = new(new byte[,] {
    //   X
    //     1
    { X, 0 },
    { 0, 1 }
  });

  /// <summary>Vertical Diamond: Diamond pattern, vertical bias. Quality: 6/10</summary>
  public static ErrorDiffusion VerticalDiamond { get; } = new(new byte[,] {
    //       X
    //     3 6 3
    //   1   2   1
    { 0, 0, X, 0, 0 },
    { 0, 3, 6, 3, 0 },
    { 1, 0, 2, 0, 1 }
  });

  /// <summary>Horizontal Diamond: Diamond pattern, horizontal bias. Quality: 6/10</summary>
  public static ErrorDiffusion HorizontalDiamond { get; } = new(new byte[,] {
    //   X 6 2
    //     3
    //       1
    { X, 6, 2 },
    { 0, 3, 0 },
    { 0, 0, 1 }
  });

  /// <summary>Diamond: Symmetric diamond pattern. Quality: 7/10</summary>
  public static ErrorDiffusion Diamond { get; } = new(new byte[,] {
    //       X 6 2
    //     3 6 3
    //   1   2   1
    { 0, 0, X, 6, 2 },
    { 0, 3, 6, 3, 0 },
    { 1, 0, 2, 0, 1 }
  });

  #endregion

}
