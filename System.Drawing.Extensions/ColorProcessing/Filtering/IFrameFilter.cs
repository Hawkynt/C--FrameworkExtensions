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

using System.Drawing.Extensions.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Resizing;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Optional interface for filters that need arbitrary pixel access via <see cref="NeighborFrame{TPixel,TWork,TKey,TDecode,TProject}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Filters with large neighborhoods (e.g., Gaussian blur with radius &gt; 2) cannot
/// use the fixed 5×5 <c>NeighborWindow</c>. By implementing this interface, the filter
/// routes through the resampler pipeline which provides random-access to any source pixel.
/// </para>
/// </remarks>
public interface IFrameFilter {

  /// <summary>
  /// Gets whether this filter instance requires frame-level access.
  /// When <c>true</c>, <see cref="InvokeFrameKernel{TWork,TKey,TPixel,TDecode,TProject,TEncode,TResult}"/>
  /// is used instead of the standard 5×5 kernel path.
  /// </summary>
  bool UsesFrameAccess { get; }

  /// <summary>
  /// Invokes a callback with a concrete resample kernel for frame-level filtering.
  /// </summary>
  TResult InvokeFrameKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth, int sourceHeight)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>;
}
