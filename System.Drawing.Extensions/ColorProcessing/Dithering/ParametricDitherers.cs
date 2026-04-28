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

using System.Collections.Generic;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// One-stop registration of every parametric ditherer variant the library exposes.
/// </summary>
/// <remarks>
/// <para>
/// Each <c>Register*</c> invocation:
/// </para>
/// <list type="bullet">
///   <item>declares the parameter surface (names, types, ranges, defaults) via
///         <see cref="ParameterDescriptor"/>, and</item>
///   <item>publishes a single registry entry that points to a concrete factory which
///         materialises the algorithm from a values dictionary.</item>
/// </list>
/// <para>
/// The corresponding fixed-default registry entries (the ones produced by the
/// <c>[Ditherer]</c> attribute / source generator) keep registering and behaving
/// identically — defaults of every parametric variant therefore byte-match the
/// pre-existing fixed entry, which is asserted in the unit tests.
/// </para>
/// </remarks>
internal static class ParametricDitherers {

  private static readonly object _Gate = new();
  private static bool _Registered;

  /// <summary>
  /// Triggers registration. Idempotent: safe to call repeatedly from the registry.
  /// </summary>
  public static void EnsureRegistered() {
    if (_Registered) return;
    lock (_Gate) {
      if (_Registered) return;
      _Register();
      _Registered = true;
    }
  }

  private static void _Register() {
    _RegisterBayer();
    _RegisterVoidAndCluster();
    _RegisterClusterDot();
    _RegisterBlueNoise();
    _RegisterFloydSteinberg();
    _RegisterErrorDiffusionStrengthVariants();
  }

  // -- Bayer (matrix size 2/4/8/16/32) ---------------------------------------------------

  private static void _RegisterBayer() {
    var p = new[] {
      ParameterDescriptor.Choice("size", defaultValue: 8, allowedValues: [2, 4, 8, 16, 32])
    };
    ParameterMetadata.Register(
      key: "Bayer.Parametric",
      parameters: p,
      builder: values => {
        var size = ParameterMetadata.Get<int>(values, p[0]);
        return size == 32
          ? (IDitherer)new Bayer32x32Ditherer()
          : new OrderedDitherer(BayerMatrix.Generate(size));
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(OrderedDitherer),
      name: "Bayer (parametric)",
      parameterKey: "Bayer.Parametric",
      defaultFactory: static () => OrderedDitherer.Bayer8x8,
      type: DitheringType.Ordered,
      author: "Bryce Bayer",
      description: "Bayer ordered dithering with selectable matrix size (2/4/8/16/32).",
      year: 1973);
  }

  // -- Void-and-Cluster (matrix size) ----------------------------------------------------

  private static void _RegisterVoidAndCluster() {
    var p = new[] {
      ParameterDescriptor.Choice("matrixSize", defaultValue: 4, allowedValues: [4, 8, 16, 32])
    };
    ParameterMetadata.Register(
      key: "VoidAndCluster.Parametric",
      parameters: p,
      builder: values => {
        var size = ParameterMetadata.Get<int>(values, p[0]);
        return size switch {
          4 => (IDitherer)VoidAndClusterDitherer.Size4x4,
          8 => VoidAndClusterDitherer.Size8x8,
          16 => VoidAndClusterDitherer.Size16x16,
          32 => VoidAndClusterDitherer.Size32x32,
          _ => new VoidAndClusterDitherer(size),
        };
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(VoidAndClusterDitherer),
      name: "VoidAndCluster (parametric)",
      parameterKey: "VoidAndCluster.Parametric",
      defaultFactory: static () => VoidAndClusterDitherer.Size4x4,
      type: DitheringType.Ordered,
      author: "Robert Ulichney",
      description: "Void-and-cluster blue-noise ordered dithering with selectable matrix size.",
      year: 1993);
  }

  // -- Cluster-Dot (cluster size) --------------------------------------------------------

  private static void _RegisterClusterDot() {
    var p = new[] {
      ParameterDescriptor.Choice("clusterSize", defaultValue: 4, allowedValues: [4, 8]),
      ParameterDescriptor.Float("strength", defaultValue: 1f, min: 0f, max: 1f)
    };
    ParameterMetadata.Register(
      key: "ClusterDot.Parametric",
      parameters: p,
      builder: values => {
        var clusterSize = ParameterMetadata.Get<int>(values, p[0]);
        var strength = ParameterMetadata.Get<float>(values, p[1]);
        var matrix = clusterSize == 8 ? _ClusterDot8 : _ClusterDot4;
        var ord = new OrderedDitherer(matrix, strength);
        return (IDitherer)ord;
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(ClusterDotDitherer),
      name: "ClusterDot (parametric)",
      parameterKey: "ClusterDot.Parametric",
      defaultFactory: static () => OrderedDitherer.ClusterDot4x4,
      type: DitheringType.Ordered,
      description: "Halftone-style cluster-dot ordered dithering with selectable cluster size and strength.");
  }

  // -- Blue Noise (tile size) ------------------------------------------------------------

  private static void _RegisterBlueNoise() {
    var p = new[] {
      ParameterDescriptor.Choice("tileSize", defaultValue: 8, allowedValues: [8, 64, 128])
    };
    ParameterMetadata.Register(
      key: "BlueNoise.Parametric",
      parameters: p,
      builder: values => {
        var size = ParameterMetadata.Get<int>(values, p[0]);
        return size switch {
          8 => (IDitherer)BlueNoiseDitherer.Size8x8,
          64 => BlueNoiseDitherer.Size64x64,
          128 => BlueNoiseDitherer.Size128x128,
          _ => new BlueNoiseDitherer(size),
        };
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(BlueNoiseDitherer),
      name: "BlueNoise (parametric)",
      parameterKey: "BlueNoise.Parametric",
      defaultFactory: static () => BlueNoiseDitherer.Size8x8,
      type: DitheringType.Noise,
      description: "Blue-noise ordered dithering with selectable tile size.");
  }

  // -- Floyd-Steinberg (serpentine + strength) ------------------------------------------

  private static void _RegisterFloydSteinberg() {
    var p = new[] {
      ParameterDescriptor.Bool("serpentine", defaultValue: false,
        description: "Alternate scan direction per row (reduces directional artefacts)."),
      ParameterDescriptor.Float("strength", defaultValue: 1f, min: 0f, max: 1f,
        description: "Error-diffusion strength (0 = no diffusion, 1 = full).")
    };
    ParameterMetadata.Register(
      key: "FloydSteinberg.Parametric",
      parameters: p,
      builder: values => {
        var serp = ParameterMetadata.Get<bool>(values, p[0]);
        var strength = ParameterMetadata.Get<float>(values, p[1]);
        var fs = ErrorDiffusion.FloydSteinberg.WithStrength(strength);
        return serp ? (IDitherer)fs.Serpentine : fs;
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(ErrorDiffusion),
      name: "FloydSteinberg (parametric)",
      parameterKey: "FloydSteinberg.Parametric",
      defaultFactory: static () => ErrorDiffusion.FloydSteinberg,
      type: DitheringType.ErrorDiffusion,
      author: "R.W. Floyd, L. Steinberg",
      description: "Floyd-Steinberg error diffusion with optional serpentine scan and adjustable strength.",
      year: 1976);
  }

  // -- Stucki / Burkes / JJN (strength) -------------------------------------------------

  private static void _RegisterErrorDiffusionStrengthVariants() {
    _RegisterEdStrength("Stucki", () => ErrorDiffusion.Stucki, "P. Stucki", 1981);
    _RegisterEdStrength("Burkes", () => ErrorDiffusion.Burkes, null, 1988);
    _RegisterEdStrength("JarvisJudiceNinke", () => ErrorDiffusion.JarvisJudiceNinke, "J.F. Jarvis et al.", 1976);
  }

  private static void _RegisterEdStrength(string baseName, System.Func<ErrorDiffusion> baseFactory, string? author, int year) {
    var p = new[] {
      ParameterDescriptor.Float("strength", defaultValue: 1f, min: 0f, max: 1f)
    };
    var key = baseName + ".Parametric";
    ParameterMetadata.Register(
      key: key,
      parameters: p,
      builder: values => {
        var strength = ParameterMetadata.Get<float>(values, p[0]);
        return (IDitherer)baseFactory().WithStrength(strength);
      });

    DithererRegistry.RegisterParametric(
      declaringType: typeof(ErrorDiffusion),
      name: baseName + " (parametric)",
      parameterKey: key,
      defaultFactory: () => baseFactory(),
      type: DitheringType.ErrorDiffusion,
      author: author,
      description: baseName + " error diffusion with adjustable strength.",
      year: year);
  }

  // -- Reusable cluster-dot threshold matrices ------------------------------------------

  private static readonly float[,] _ClusterDot4 = {
    { 12,  5,  6, 13 },
    {  4,  0,  1,  7 },
    { 11,  3,  2,  8 },
    { 15, 10,  9, 14 }
  };

  private static readonly float[,] _ClusterDot8 = {
    { 24, 10, 12, 26, 35, 47, 49, 37 },
    {  8,  0,  2, 14, 45, 59, 61, 51 },
    { 22,  6,  4, 16, 43, 57, 63, 53 },
    { 30, 20, 18, 28, 33, 41, 55, 39 },
    { 34, 46, 48, 36, 25, 11, 13, 27 },
    { 44, 58, 60, 50,  9,  1,  3, 15 },
    { 42, 56, 62, 52, 23,  7,  5, 17 },
    { 32, 40, 54, 38, 31, 21, 19, 29 }
  };
}
