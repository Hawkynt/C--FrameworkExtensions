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

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Optional interface for filters that require multiple passes to achieve their effect.
/// </summary>
/// <remarks>
/// <para>
/// Some filters (e.g., large-radius Gaussian blur) cannot be computed in a single
/// 5×5 kernel pass. By implementing this interface, the filter signals that
/// <see cref="IPixelFilter.InvokeKernel{TWork,TKey,TPixel,TDistance,TEquality,TLerp,TEncode,TResult}"/>
/// should be applied <see cref="PassCount"/> times, feeding each pass's output
/// as the next pass's input.
/// </para>
/// </remarks>
public interface IMultiPassFilter {

  /// <summary>
  /// Gets the number of times the kernel should be applied.
  /// </summary>
  int PassCount { get; }
}
