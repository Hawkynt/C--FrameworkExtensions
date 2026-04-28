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

namespace Hawkynt.ColorProcessing.LookupTable;

/// <summary>
/// Interpolation modes for <see cref="Lut3D"/>.
/// </summary>
public enum Lut3DInterpolation {

  /// <summary>Trilinear (8-tap) interpolation. Standard, smooth, slightly blurry on edges.</summary>
  Trilinear,

  /// <summary>Tetrahedral (4-tap) interpolation. Sharper than trilinear and the de-facto choice for color grading; cheaper too.</summary>
  Tetrahedral,
}

/// <summary>
/// A 3D color look-up table — three-dimensional grid of RGB samples used as the
/// canonical primitive for color grading, gamut mapping, technical LUTs, and
/// "look" application in film / video pipelines. Sampling is performed with
/// either trilinear or tetrahedral interpolation.
/// </summary>
/// <remarks>
/// <para>
/// Storage layout: a flat <c>float[size·size·size·3]</c> array, indexed as
/// <c>(((b·size + g)·size + r)·3 + channel)</c> — channel 0=R, 1=G, 2=B.
/// This matches the on-disk order used by the Adobe <c>.cube</c> format
/// (R varies fastest, then G, then B).
/// </para>
/// <para>
/// Each axis is mapped from input values in <see cref="DomainMin"/> .. <see cref="DomainMax"/>
/// (default 0..1). Output values are returned as-is — clamping or post-processing
/// is the caller's responsibility.
/// </para>
/// <para>
/// Tetrahedral interpolation (the default) splits each unit cube into six
/// tetrahedra and interpolates within the one containing the sample point. It
/// is preferred for color grading because diagonals (the most common ramp
/// direction in graded LUTs) follow tetrahedron edges exactly, eliminating the
/// faint cross-axis bleeding that trilinear can introduce.
/// </para>
/// </remarks>
public sealed class Lut3D {

  /// <summary>Size of each axis (e.g. 33 for a typical "33×33×33" grading LUT).</summary>
  public int Size { get; }

  /// <summary>Domain minimum applied per channel (default 0).</summary>
  public (float R, float G, float B) DomainMin { get; }

  /// <summary>Domain maximum applied per channel (default 1).</summary>
  public (float R, float G, float B) DomainMax { get; }

  /// <summary>Flat sample storage. Length = <see cref="Size"/>³ · 3.</summary>
  public float[] Data { get; }

  /// <summary>
  /// Creates a new 3D LUT.
  /// </summary>
  /// <param name="size">Size of each axis. Must be ≥2 and ≤256.</param>
  /// <param name="data">Flat sample data. Length must equal <c>size³·3</c>.</param>
  /// <param name="domainMin">Per-channel domain minimum (default 0,0,0).</param>
  /// <param name="domainMax">Per-channel domain maximum (default 1,1,1).</param>
  public Lut3D(int size, float[] data,
    (float R, float G, float B)? domainMin = null,
    (float R, float G, float B)? domainMax = null) {
    if (size < 2 || size > 256)
      throw new ArgumentOutOfRangeException(nameof(size), size, "Lut3D size must be in [2..256].");
    if (data == null)
      throw new ArgumentNullException(nameof(data));
    var expected = size * size * size * 3;
    if (data.Length != expected)
      throw new ArgumentException($"Lut3D data length must be {expected} (size³·3), got {data.Length}.", nameof(data));
    this.Size = size;
    this.Data = data;
    this.DomainMin = domainMin ?? (0f, 0f, 0f);
    this.DomainMax = domainMax ?? (1f, 1f, 1f);
  }

  /// <summary>
  /// Returns the identity LUT of the given size (output equals input — no transform).
  /// Useful as a base for testing or for procedural LUT construction.
  /// </summary>
  public static Lut3D Identity(int size = 17) {
    if (size < 2 || size > 256)
      throw new ArgumentOutOfRangeException(nameof(size));
    var data = new float[size * size * size * 3];
    var inv = 1f / (size - 1);
    var i = 0;
    for (var bi = 0; bi < size; ++bi)
    for (var gi = 0; gi < size; ++gi)
    for (var ri = 0; ri < size; ++ri) {
      data[i++] = ri * inv;
      data[i++] = gi * inv;
      data[i++] = bi * inv;
    }
    return new Lut3D(size, data);
  }

  /// <summary>Samples the LUT at the given normalised input color using the requested interpolation.</summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public (float R, float G, float B) Sample(float r, float g, float b, Lut3DInterpolation interpolation = Lut3DInterpolation.Tetrahedral) {
    // Map domain to [0..1].
    var dminR = this.DomainMin.R;
    var dminG = this.DomainMin.G;
    var dminB = this.DomainMin.B;
    var dmaxR = this.DomainMax.R;
    var dmaxG = this.DomainMax.G;
    var dmaxB = this.DomainMax.B;

    var nr = (r - dminR) / Math.Max(1e-12f, dmaxR - dminR);
    var ng = (g - dminG) / Math.Max(1e-12f, dmaxG - dminG);
    var nb = (b - dminB) / Math.Max(1e-12f, dmaxB - dminB);

    nr = nr < 0f ? 0f : (nr > 1f ? 1f : nr);
    ng = ng < 0f ? 0f : (ng > 1f ? 1f : ng);
    nb = nb < 0f ? 0f : (nb > 1f ? 1f : nb);

    var s = this.Size;
    var max = s - 1;

    var fr = nr * max;
    var fg = ng * max;
    var fb = nb * max;

    var ir = (int)fr;
    var ig = (int)fg;
    var ib = (int)fb;
    if (ir >= max) ir = max - 1;
    if (ig >= max) ig = max - 1;
    if (ib >= max) ib = max - 1;
    if (ir < 0) ir = 0;
    if (ig < 0) ig = 0;
    if (ib < 0) ib = 0;

    var dr = fr - ir;
    var dg = fg - ig;
    var db = fb - ib;

    return interpolation == Lut3DInterpolation.Trilinear
      ? this._SampleTrilinear(ir, ig, ib, dr, dg, db)
      : this._SampleTetrahedral(ir, ig, ib, dr, dg, db);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int _Index(int r, int g, int b) => ((b * this.Size + g) * this.Size + r) * 3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private (float R, float G, float B) _Get(int r, int g, int b) {
    var i = this._Index(r, g, b);
    return (this.Data[i], this.Data[i + 1], this.Data[i + 2]);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private (float R, float G, float B) _SampleTrilinear(int ir, int ig, int ib, float dr, float dg, float db) {
    var c000 = this._Get(ir, ig, ib);
    var c100 = this._Get(ir + 1, ig, ib);
    var c010 = this._Get(ir, ig + 1, ib);
    var c110 = this._Get(ir + 1, ig + 1, ib);
    var c001 = this._Get(ir, ig, ib + 1);
    var c101 = this._Get(ir + 1, ig, ib + 1);
    var c011 = this._Get(ir, ig + 1, ib + 1);
    var c111 = this._Get(ir + 1, ig + 1, ib + 1);

    var w000 = (1 - dr) * (1 - dg) * (1 - db);
    var w100 = dr * (1 - dg) * (1 - db);
    var w010 = (1 - dr) * dg * (1 - db);
    var w110 = dr * dg * (1 - db);
    var w001 = (1 - dr) * (1 - dg) * db;
    var w101 = dr * (1 - dg) * db;
    var w011 = (1 - dr) * dg * db;
    var w111 = dr * dg * db;

    return (
      c000.R * w000 + c100.R * w100 + c010.R * w010 + c110.R * w110 +
      c001.R * w001 + c101.R * w101 + c011.R * w011 + c111.R * w111,
      c000.G * w000 + c100.G * w100 + c010.G * w010 + c110.G * w110 +
      c001.G * w001 + c101.G * w101 + c011.G * w011 + c111.G * w111,
      c000.B * w000 + c100.B * w100 + c010.B * w010 + c110.B * w110 +
      c001.B * w001 + c101.B * w101 + c011.B * w011 + c111.B * w111);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private (float R, float G, float B) _SampleTetrahedral(int ir, int ig, int ib, float dr, float dg, float db) {
    // Standard tetrahedral interpolation (Kirk & Roundy 1990 / classical 3D LUT literature).
    // Uses corner-relative weights. Six cases by ordering of (dr,dg,db).
    var c000 = this._Get(ir, ig, ib);
    var c111 = this._Get(ir + 1, ig + 1, ib + 1);

    float r, g, b;
    if (dr > dg) {
      if (dg > db) {
        // dr > dg > db
        var c100 = this._Get(ir + 1, ig, ib);
        var c110 = this._Get(ir + 1, ig + 1, ib);
        r = (1 - dr) * c000.R + (dr - dg) * c100.R + (dg - db) * c110.R + db * c111.R;
        g = (1 - dr) * c000.G + (dr - dg) * c100.G + (dg - db) * c110.G + db * c111.G;
        b = (1 - dr) * c000.B + (dr - dg) * c100.B + (dg - db) * c110.B + db * c111.B;
      } else if (dr > db) {
        // dr > db > dg
        var c100 = this._Get(ir + 1, ig, ib);
        var c101 = this._Get(ir + 1, ig, ib + 1);
        r = (1 - dr) * c000.R + (dr - db) * c100.R + (db - dg) * c101.R + dg * c111.R;
        g = (1 - dr) * c000.G + (dr - db) * c100.G + (db - dg) * c101.G + dg * c111.G;
        b = (1 - dr) * c000.B + (dr - db) * c100.B + (db - dg) * c101.B + dg * c111.B;
      } else {
        // db > dr > dg
        var c001 = this._Get(ir, ig, ib + 1);
        var c101 = this._Get(ir + 1, ig, ib + 1);
        r = (1 - db) * c000.R + (db - dr) * c001.R + (dr - dg) * c101.R + dg * c111.R;
        g = (1 - db) * c000.G + (db - dr) * c001.G + (dr - dg) * c101.G + dg * c111.G;
        b = (1 - db) * c000.B + (db - dr) * c001.B + (dr - dg) * c101.B + dg * c111.B;
      }
    } else {
      if (db > dg) {
        // db > dg > dr (or db>dg>=dr handled by symmetry)
        var c001 = this._Get(ir, ig, ib + 1);
        var c011 = this._Get(ir, ig + 1, ib + 1);
        r = (1 - db) * c000.R + (db - dg) * c001.R + (dg - dr) * c011.R + dr * c111.R;
        g = (1 - db) * c000.G + (db - dg) * c001.G + (dg - dr) * c011.G + dr * c111.G;
        b = (1 - db) * c000.B + (db - dg) * c001.B + (dg - dr) * c011.B + dr * c111.B;
      } else if (db > dr) {
        // dg >= db > dr
        var c010 = this._Get(ir, ig + 1, ib);
        var c011 = this._Get(ir, ig + 1, ib + 1);
        r = (1 - dg) * c000.R + (dg - db) * c010.R + (db - dr) * c011.R + dr * c111.R;
        g = (1 - dg) * c000.G + (dg - db) * c010.G + (db - dr) * c011.G + dr * c111.G;
        b = (1 - dg) * c000.B + (dg - db) * c010.B + (db - dr) * c011.B + dr * c111.B;
      } else {
        // dg >= dr >= db
        var c010 = this._Get(ir, ig + 1, ib);
        var c110 = this._Get(ir + 1, ig + 1, ib);
        r = (1 - dg) * c000.R + (dg - dr) * c010.R + (dr - db) * c110.R + db * c111.R;
        g = (1 - dg) * c000.G + (dg - dr) * c010.G + (dr - db) * c110.G + db * c111.G;
        b = (1 - dg) * c000.B + (dg - dr) * c010.B + (dr - db) * c110.B + db * c111.B;
      }
    }
    return (r, g, b);
  }
}
