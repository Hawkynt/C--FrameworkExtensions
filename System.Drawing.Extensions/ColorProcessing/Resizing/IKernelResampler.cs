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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Optional companion to <see cref="IResampler"/> for resamplers whose weighting
/// is expressible as a separable 1-D kernel function.
/// </summary>
/// <remarks>
/// Implemented only by resamplers with a closed-form separable weight function
/// (Bicubic, Lanczos, B-splines, OMoms, Schaum, Hermite, Bilinear, Box, ...).
/// Content- or edge-aware resamplers (DCCI, EEDI2, SeamCarving, etc.) do not
/// implement this interface because their output is not a pure function of distance.
/// Consumers that want to plot/visualise the kernel shape should test for this
/// interface and fall back to hiding the chart when it is absent.
/// </remarks>
public interface IKernelResampler : IResampler {

  /// <summary>
  /// Returns the 1-D kernel weight at the given distance from the sample centre.
  /// </summary>
  /// <param name="distance">Signed distance from the sample centre, in source pixels.</param>
  /// <returns>The kernel weight at <paramref name="distance"/>. Zero beyond <see cref="IResampler.Radius"/>.</returns>
  /// <remarks>
  /// Values outside the range <c>[-Radius, +Radius]</c> return <c>0</c>.
  /// The function is even for every kernel shipped with this library
  /// (<c>EvaluateWeight(-x) == EvaluateWeight(+x)</c>).
  /// </remarks>
  float EvaluateWeight(float distance);

}
