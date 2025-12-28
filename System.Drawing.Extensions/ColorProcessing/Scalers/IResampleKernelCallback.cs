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

using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Pipeline;

namespace Hawkynt.ColorProcessing.Scalers;

/// <summary>
/// Callback interface for receiving a concrete resample kernel type.
/// </summary>
/// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TDecode">The decoder type (TPixel → TWork).</typeparam>
/// <typeparam name="TProject">The projector type (TWork → TKey).</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
/// <typeparam name="TResult">The return type of the callback.</typeparam>
/// <remarks>
/// <para>
/// Enables struct-constrained dispatch without per-pixel virtual calls.
/// The resampler invokes this callback with its concrete kernel type,
/// allowing the callback to call methods that require struct constraints.
/// </para>
/// </remarks>
public interface IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, out TResult>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <summary>
  /// Invokes the callback with a concrete resample kernel type.
  /// </summary>
  /// <typeparam name="TKernel">The concrete kernel type.</typeparam>
  /// <param name="kernel">The kernel instance.</param>
  /// <returns>The result of the operation.</returns>
  TResult Invoke<TKernel>(TKernel kernel)
    where TKernel : struct, IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>;
}
