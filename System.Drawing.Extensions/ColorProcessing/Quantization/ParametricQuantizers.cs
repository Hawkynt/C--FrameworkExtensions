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

namespace Hawkynt.ColorProcessing.Quantization;

/// <summary>
/// One-stop registration of every parametric quantizer variant the library exposes.
/// </summary>
internal static class ParametricQuantizers {

  private static readonly object _Gate = new();
  private static bool _Registered;

  public static void EnsureRegistered() {
    if (_Registered) return;
    lock (_Gate) {
      if (_Registered) return;
      _Register();
      _Registered = true;
    }
  }

  private static void _Register() {
    _RegisterKMeans();
    _RegisterBisectingKMeans();
    _RegisterMeanShift();
    _RegisterDbscan();
    _RegisterNeuQuant();
  }

  // -- K-Means (max iterations + convergence threshold) ---------------------------------

  private static void _RegisterKMeans() {
    var p = new[] {
      ParameterDescriptor.Int("maxIterations", defaultValue: 100, min: 1, max: 1000),
      ParameterDescriptor.Float("convergenceThreshold", defaultValue: 0.001f, min: 0f, max: 1f)
    };
    ParameterMetadata.Register(
      key: "KMeans.Parametric",
      parameters: p,
      builder: values => {
        var maxIt = ParameterMetadata.Get<int>(values, p[0]);
        var eps = ParameterMetadata.Get<float>(values, p[1]);
        return (IQuantizer)new KMeansQuantizer { MaxIterations = maxIt, ConvergenceThreshold = eps };
      });

    QuantizerRegistry.RegisterParametric(
      declaringType: typeof(KMeansQuantizer),
      name: "K-Means (parametric)",
      parameterKey: "KMeans.Parametric",
      defaultFactory: static () => new KMeansQuantizer(),
      type: QuantizationType.Clustering,
      author: "J. MacQueen",
      year: 1967,
      qualityRating: 7);
  }

  // -- Bisecting K-Means (iterations + bisection trials + threshold) --------------------

  private static void _RegisterBisectingKMeans() {
    var p = new[] {
      ParameterDescriptor.Int("maxIterationsPerSplit", defaultValue: 10, min: 1, max: 200),
      ParameterDescriptor.Int("bisectionTrials", defaultValue: 3, min: 1, max: 50),
      ParameterDescriptor.Float("convergenceThreshold", defaultValue: 0.001f, min: 0f, max: 1f)
    };
    ParameterMetadata.Register(
      key: "BisectingKMeans.Parametric",
      parameters: p,
      builder: values => {
        var maxIt = ParameterMetadata.Get<int>(values, p[0]);
        var trials = ParameterMetadata.Get<int>(values, p[1]);
        var eps = ParameterMetadata.Get<float>(values, p[2]);
        return (IQuantizer)new BisectingKMeansQuantizer {
          MaxIterationsPerSplit = maxIt,
          BisectionTrials = trials,
          ConvergenceThreshold = eps,
        };
      });

    QuantizerRegistry.RegisterParametric(
      declaringType: typeof(BisectingKMeansQuantizer),
      name: "Bisecting K-Means (parametric)",
      parameterKey: "BisectingKMeans.Parametric",
      defaultFactory: static () => new BisectingKMeansQuantizer(),
      type: QuantizationType.Clustering,
      author: "M. Steinbach et al.",
      year: 2000,
      qualityRating: 7);
  }

  // -- Mean-Shift (bandwidth) -----------------------------------------------------------

  private static void _RegisterMeanShift() {
    var p = new[] {
      ParameterDescriptor.Float("bandwidth", defaultValue: 0.06f, min: 0.001f, max: 1f,
        description: "Gaussian kernel bandwidth in normalized OkLab space.")
    };
    ParameterMetadata.Register(
      key: "MeanShift.Parametric",
      parameters: p,
      builder: values => {
        var bw = ParameterMetadata.Get<float>(values, p[0]);
        return (IQuantizer)new MeanShiftQuantizer { Bandwidth = bw };
      });

    QuantizerRegistry.RegisterParametric(
      declaringType: typeof(MeanShiftQuantizer),
      name: "Mean-Shift (parametric)",
      parameterKey: "MeanShift.Parametric",
      defaultFactory: static () => new MeanShiftQuantizer(),
      type: QuantizationType.Clustering,
      author: "Comaniciu & Meer",
      year: 2002,
      qualityRating: 7);
  }

  // -- DBSCAN (epsilon + minPts) --------------------------------------------------------

  private static void _RegisterDbscan() {
    var p = new[] {
      ParameterDescriptor.Float("epsilon", defaultValue: 0.03f, min: 0.001f, max: 1f),
      ParameterDescriptor.Int("minPoints", defaultValue: 4, min: 1, max: 100)
    };
    ParameterMetadata.Register(
      key: "Dbscan.Parametric",
      parameters: p,
      builder: values => {
        var eps = ParameterMetadata.Get<float>(values, p[0]);
        var minPts = ParameterMetadata.Get<int>(values, p[1]);
        return (IQuantizer)new DbscanQuantizer { Epsilon = eps, MinPoints = minPts };
      });

    QuantizerRegistry.RegisterParametric(
      declaringType: typeof(DbscanQuantizer),
      name: "DBSCAN (parametric)",
      parameterKey: "Dbscan.Parametric",
      defaultFactory: static () => new DbscanQuantizer(),
      type: QuantizationType.Clustering,
      author: "Ester et al.",
      year: 1996,
      qualityRating: 7);
  }

  // -- NeuQuant (max iterations + initial alpha) ---------------------------------------

  private static void _RegisterNeuQuant() {
    var p = new[] {
      ParameterDescriptor.Int("maxIterations", defaultValue: 100, min: 1, max: 1000),
      ParameterDescriptor.Float("initialAlpha", defaultValue: 0.1f, min: 0.001f, max: 1f)
    };
    ParameterMetadata.Register(
      key: "NeuQuant.Parametric",
      parameters: p,
      builder: values => {
        var maxIt = ParameterMetadata.Get<int>(values, p[0]);
        var alpha = ParameterMetadata.Get<float>(values, p[1]);
        return (IQuantizer)new NeuquantQuantizer { MaxIterations = maxIt, InitialAlpha = alpha };
      });

    QuantizerRegistry.RegisterParametric(
      declaringType: typeof(NeuquantQuantizer),
      name: "NeuQuant (parametric)",
      parameterKey: "NeuQuant.Parametric",
      defaultFactory: static () => new NeuquantQuantizer(),
      type: QuantizationType.Neural,
      author: "Anthony Dekker",
      year: 1994,
      qualityRating: 9);
  }
}
