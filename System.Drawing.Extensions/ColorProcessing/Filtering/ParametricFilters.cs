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

using Hawkynt.ColorProcessing.Filtering.Filters;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// One-stop registration of every parametric pixel-filter variant the library exposes.
/// </summary>
internal static class ParametricFilters {

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
    _RegisterGaussianBlur();
    _RegisterBilateral();
    _RegisterUnsharpMask();
    _RegisterMedian();
    _RegisterErode();
    _RegisterDilate();
  }

  // -- Gaussian Blur (radiusX, radiusY) -------------------------------------------------

  private static void _RegisterGaussianBlur() {
    var p = new[] {
      ParameterDescriptor.Int("radiusX", defaultValue: 1, min: 0, max: 32),
      ParameterDescriptor.Int("radiusY", defaultValue: 1, min: 0, max: 32)
    };
    ParameterMetadata.Register(
      key: "GaussianBlur.Parametric",
      parameters: p,
      builder: values => {
        var rx = ParameterMetadata.Get<int>(values, p[0]);
        var ry = ParameterMetadata.Get<int>(values, p[1]);
        return (IPixelFilter)new GaussianBlur(rx, ry);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(GaussianBlur),
      name: "GaussianBlur (parametric)",
      parameterKey: "GaussianBlur.Parametric",
      description: "Gaussian blur with configurable per-axis kernel radius.",
      category: FilterCategory.Enhancement);
  }

  // -- Bilateral (radius, sigma_s, sigma_r) ---------------------------------------------

  private static void _RegisterBilateral() {
    var p = new[] {
      ParameterDescriptor.Int("radius", defaultValue: 3, min: 1, max: 32),
      ParameterDescriptor.Float("spatialSigma", defaultValue: 3f, min: 0.1f, max: 32f),
      ParameterDescriptor.Float("rangeSigma", defaultValue: 0.1f, min: 0.001f, max: 1f)
    };
    ParameterMetadata.Register(
      key: "Bilateral.Parametric",
      parameters: p,
      builder: values => {
        var r = ParameterMetadata.Get<int>(values, p[0]);
        var ss = ParameterMetadata.Get<float>(values, p[1]);
        var sr = ParameterMetadata.Get<float>(values, p[2]);
        return (IPixelFilter)new BilateralFilter(r, ss, sr);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(BilateralFilter),
      name: "BilateralFilter (parametric)",
      parameterKey: "Bilateral.Parametric",
      description: "Edge-preserving bilateral smoothing with configurable spatial and range sigmas.",
      category: FilterCategory.Enhancement);
  }

  // -- Unsharp Mask (amount, threshold, radius) -----------------------------------------

  private static void _RegisterUnsharpMask() {
    var p = new[] {
      ParameterDescriptor.Float("amount", defaultValue: 1f, min: 0f, max: 10f),
      ParameterDescriptor.Float("threshold", defaultValue: 0f, min: 0f, max: 1f),
      ParameterDescriptor.Int("radiusX", defaultValue: 1, min: 0, max: 32),
      ParameterDescriptor.Int("radiusY", defaultValue: 1, min: 0, max: 32)
    };
    ParameterMetadata.Register(
      key: "UnsharpMask.Parametric",
      parameters: p,
      builder: values => {
        var amount = ParameterMetadata.Get<float>(values, p[0]);
        var thresh = ParameterMetadata.Get<float>(values, p[1]);
        var rx = ParameterMetadata.Get<int>(values, p[2]);
        var ry = ParameterMetadata.Get<int>(values, p[3]);
        return (IPixelFilter)new UnsharpMask(amount, thresh, rx, ry);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(UnsharpMask),
      name: "UnsharpMask (parametric)",
      parameterKey: "UnsharpMask.Parametric",
      description: "Unsharp-mask sharpening with configurable amount, threshold and per-axis radius.",
      category: FilterCategory.Enhancement);
  }

  // -- Median (radius) -----------------------------------------------------------------

  private static void _RegisterMedian() {
    var p = new[] {
      ParameterDescriptor.Choice("radius", defaultValue: 1, allowedValues: [1, 2, 3, 4])
    };
    ParameterMetadata.Register(
      key: "Median.Parametric",
      parameters: p,
      builder: values => {
        var r = ParameterMetadata.Get<int>(values, p[0]);
        return (IPixelFilter)new MedianFilter(r);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(MedianFilter),
      name: "MedianFilter (parametric)",
      parameterKey: "Median.Parametric",
      description: "Median filter with selectable window radius (window size = 2r+1).",
      category: FilterCategory.Enhancement);
  }

  // -- Erode (radius) ------------------------------------------------------------------

  private static void _RegisterErode() {
    var p = new[] {
      ParameterDescriptor.Int("radius", defaultValue: 1, min: 0, max: 32)
    };
    ParameterMetadata.Register(
      key: "Erode.Parametric",
      parameters: p,
      builder: values => {
        var r = ParameterMetadata.Get<int>(values, p[0]);
        return (IPixelFilter)new Erode(r);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(Erode),
      name: "Erode (parametric)",
      parameterKey: "Erode.Parametric",
      description: "Morphological erosion with configurable kernel radius.",
      category: FilterCategory.Enhancement);
  }

  // -- Dilate (radius) -----------------------------------------------------------------

  private static void _RegisterDilate() {
    var p = new[] {
      ParameterDescriptor.Int("radius", defaultValue: 1, min: 0, max: 32)
    };
    ParameterMetadata.Register(
      key: "Dilate.Parametric",
      parameters: p,
      builder: values => {
        var r = ParameterMetadata.Get<int>(values, p[0]);
        return (IPixelFilter)new Dilate(r);
      });

    FilterRegistry.RegisterParametric(
      type: typeof(Dilate),
      name: "Dilate (parametric)",
      parameterKey: "Dilate.Parametric",
      description: "Morphological dilation with configurable kernel radius.",
      category: FilterCategory.Enhancement);
  }
}
