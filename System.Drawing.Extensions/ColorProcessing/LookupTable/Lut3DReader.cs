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
using System.Globalization;
using System.IO;
using System.Text;
using Guard;
using Hawkynt.ColorProcessing.Internal;

namespace Hawkynt.ColorProcessing.LookupTable;

/// <summary>
/// Parses 3D LUTs from common text formats:
/// <list type="bullet">
///   <item>Adobe <c>.cube</c> — published 2013, the de-facto color-grading interchange.</item>
///   <item>Autodesk <c>.3dl</c> — older format used by Lustre / Autodesk Flame.</item>
/// </list>
/// </summary>
/// <remarks>
/// <para>
/// .cube spec: <see href="https://wwwimages2.adobe.com/content/dam/acom/en/products/speedgrade/cc/pdfs/cube-lut-specification-1.0.pdf"/>.
/// Lines beginning with <c>#</c> are comments. Required keywords:
/// <c>LUT_3D_SIZE n</c>, optional: <c>DOMAIN_MIN r g b</c>, <c>DOMAIN_MAX r g b</c>,
/// <c>TITLE "..."</c>. Sample order: R varies fastest, then G, then B (matches
/// <see cref="Lut3D"/> internal layout).
/// </para>
/// <para>
/// .3dl is integer-valued, line-oriented; the implementation infers the LUT size
/// from the cube root of the sample count.
/// </para>
/// </remarks>
public static class Lut3DReader {

  /// <summary>Reads a <c>.cube</c> file from <paramref name="path"/>.</summary>
  public static Lut3D ReadCube(string path) {
    Against.ArgumentIsNullOrEmpty(path);
    using var sr = new StreamReader(path, Encoding.UTF8);
    return ReadCube(sr);
  }

  /// <summary>Reads a <c>.cube</c> definition from <paramref name="text"/>.</summary>
  public static Lut3D ReadCubeFromString(string text) {
    Against.ArgumentIsNull(text);
    using var sr = new StringReader(text);
    return ReadCube(sr);
  }

  /// <summary>Reads a <c>.cube</c> definition from a <see cref="TextReader"/>.</summary>
  public static Lut3D ReadCube(TextReader reader) {
    Against.ArgumentIsNull(reader);

    var size = -1;
    (float R, float G, float B) domainMin = (0, 0, 0);
    (float R, float G, float B) domainMax = (1, 1, 1);
    float[]? data = null;
    var writeIndex = 0;

    string? line;
    while ((line = reader.ReadLine()) != null) {
      // Strip BOM, comments, and whitespace.
      if (line.Length > 0 && line[0] == '﻿')
        line = line.Substring(1);
      var hash = line.IndexOf('#');
      if (hash >= 0)
        line = line.Substring(0, hash);
      var trimmed = line.Trim();
      if (trimmed.Length == 0)
        continue;

      // Tokenise on whitespace.
      var tokens = trimmed.Split(_Whitespace, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length == 0)
        continue;

      var head = tokens[0];

      if (string.Equals(head, "TITLE", StringComparison.OrdinalIgnoreCase)) {
        // Ignored; metadata only.
        continue;
      }
      if (string.Equals(head, "LUT_1D_SIZE", StringComparison.OrdinalIgnoreCase))
        throw new FormatException("LUT_1D_SIZE found — only 3D LUTs are supported here.");
      if (string.Equals(head, "LUT_3D_SIZE", StringComparison.OrdinalIgnoreCase)) {
        if (tokens.Length < 2 || !int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out size))
          throw new FormatException($"Invalid LUT_3D_SIZE on line: '{trimmed}'.");
        if (size < 2 || size > 256)
          throw new FormatException($"LUT_3D_SIZE {size} out of range [2..256].");
        data = new float[size * size * size * 3];
        continue;
      }
      if (string.Equals(head, "DOMAIN_MIN", StringComparison.OrdinalIgnoreCase)) {
        domainMin = _ParseTriple(tokens, trimmed);
        continue;
      }
      if (string.Equals(head, "DOMAIN_MAX", StringComparison.OrdinalIgnoreCase)) {
        domainMax = _ParseTriple(tokens, trimmed);
        continue;
      }
      if (string.Equals(head, "LUT_3D_INPUT_RANGE", StringComparison.OrdinalIgnoreCase)) {
        // Older variant: LUT_3D_INPUT_RANGE min max  (applies to all three channels)
        if (tokens.Length < 3
            || !float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var mn)
            || !float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var mx))
          throw new FormatException($"Invalid LUT_3D_INPUT_RANGE on line: '{trimmed}'.");
        domainMin = (mn, mn, mn);
        domainMax = (mx, mx, mx);
        continue;
      }

      // Otherwise, it's a sample triple.
      if (data == null)
        throw new FormatException("Sample data appeared before LUT_3D_SIZE.");
      if (tokens.Length < 3)
        throw new FormatException($"Expected three sample values, got: '{trimmed}'.");
      if (writeIndex + 3 > data.Length)
        throw new FormatException("More samples than LUT_3D_SIZE³ allowed.");
      if (!float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
          || !float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
          || !float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
        throw new FormatException($"Invalid sample triple: '{trimmed}'.");
      data[writeIndex++] = r;
      data[writeIndex++] = g;
      data[writeIndex++] = b;
    }

    if (size < 0 || data == null)
      throw new FormatException("LUT_3D_SIZE was never specified.");
    if (writeIndex != data.Length)
      throw new FormatException($"Expected {data.Length / 3} samples, got {writeIndex / 3}.");

    return new Lut3D(size, data, domainMin, domainMax);
  }

  /// <summary>Reads an Autodesk <c>.3dl</c> file from <paramref name="path"/>.</summary>
  /// <remarks>
  /// Only the simple body-of-integer-triples variant is supported (output domain
  /// inferred from the maximum integer encountered, rounded to the next 2ⁿ−1).
  /// </remarks>
  public static Lut3D Read3DL(string path) {
    Against.ArgumentIsNullOrEmpty(path);
    using var sr = new StreamReader(path, Encoding.UTF8);
    return Read3DL(sr);
  }

  /// <summary>Reads an Autodesk <c>.3dl</c> definition from a <see cref="TextReader"/>.</summary>
  public static Lut3D Read3DL(TextReader reader) {
    Against.ArgumentIsNull(reader);

    // Two-line preamble: "Mesh w bits" lines or coordinate ramp + integer body.
    // Strategy: collect every triple into a list, then infer cube size from list size.
    var triples = new System.Collections.Generic.List<int>();
    string? line;
    while ((line = reader.ReadLine()) != null) {
      var hash = line.IndexOf('#');
      if (hash >= 0) line = line.Substring(0, hash);
      var trimmed = line.Trim();
      if (trimmed.Length == 0) continue;
      var tokens = trimmed.Split(_Whitespace, StringSplitOptions.RemoveEmptyEntries);
      if (tokens.Length < 3)
        continue; // header line; skip
      if (!int.TryParse(tokens[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)
          || !int.TryParse(tokens[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var g)
          || !int.TryParse(tokens[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var b))
        continue; // probably the 1-D ramp at top of the file
      triples.Add(r);
      triples.Add(g);
      triples.Add(b);
    }

    if (triples.Count == 0)
      throw new FormatException("No sample triples found.");
    var sampleCount = triples.Count / 3;
    // Integer cube-root rounding — `Math.Cbrt(sampleCount)` is platform-dependent
    // (e.g. net48 returns 3.9999... vs net6+ exact 4.0 for sampleCount=64), which can
    // flip Math.Round's result and cause the perfect-cube check below to misfire on net48.
    var size = DeterministicMath.IntCbrtRound(sampleCount);
    if (size * size * size != sampleCount)
      throw new FormatException($"Sample count {sampleCount} is not a perfect cube.");
    if (size < 2 || size > 256)
      throw new FormatException($"Inferred LUT size {size} out of range [2..256].");

    var maxVal = 1;
    for (var i = 0; i < triples.Count; ++i)
      if (triples[i] > maxVal)
        maxVal = triples[i];

    // Normalise by next power-of-two-minus-one above maxVal so 10/12/16-bit LUTs map cleanly.
    var bits = 0;
    var probe = 1;
    while (probe - 1 < maxVal && bits < 30) {
      ++bits;
      probe <<= 1;
    }
    var denom = (float)Math.Max(1, probe - 1);

    var data = new float[size * size * size * 3];
    for (var i = 0; i < triples.Count; ++i)
      data[i] = triples[i] / denom;

    return new Lut3D(size, data);
  }

  private static readonly char[] _Whitespace = [' ', '\t', '\r', '\n'];

  private static (float R, float G, float B) _ParseTriple(string[] tokens, string lineForError) {
    if (tokens.Length < 4)
      throw new FormatException($"Expected three values: '{lineForError}'.");
    if (!float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var r)
        || !float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var g)
        || !float.TryParse(tokens[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
      throw new FormatException($"Invalid triple: '{lineForError}'.");
    return (r, g, b);
  }
}
