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

using Hawkynt.ColorProcessing.Codecs;

namespace Hawkynt.ColorProcessing.Pipeline;

/// <summary>
/// Represents a downscaling kernel that uses the 5x5 NeighborWindow.
/// </summary>
/// <typeparam name="TWork">The working color type (for accumulation).</typeparam>
/// <typeparam name="TKey">The key color type (for pattern matching compatibility).</typeparam>
/// <typeparam name="TPixel">The storage pixel type.</typeparam>
/// <typeparam name="TEncode">The encoder type (TWork → TPixel).</typeparam>
/// <remarks>
/// <para>
/// Downscale kernels read from the 5x5 NeighborWindow and produce a single output pixel.
/// Maximum supported ratio is 5:1 due to the window size.
/// </para>
/// <para>
/// Unlike upscaling kernels that write multiple output pixels per source pixel,
/// downscaling kernels read multiple source pixels and return one output pixel.
/// </para>
/// <para>
/// Kernel implementations handle accumulation internally using IAccum with proper precision.
/// </para>
/// </remarks>
public interface IDownscaleKernel<TWork, TKey, TPixel, TEncode>
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TPixel : unmanaged, IStorageSpace
  where TEncode : struct, IEncode<TWork, TPixel> {

  /// <summary>
  /// The horizontal downscale ratio (2 = 1/2, 3 = 1/3, etc.).
  /// </summary>
  int RatioX { get; }

  /// <summary>
  /// The vertical downscale ratio (2 = 1/2, 3 = 1/3, etc.).
  /// </summary>
  int RatioY { get; }

  /// <summary>
  /// Averages pixels from the window and returns a single encoded output pixel.
  /// </summary>
  /// <param name="window">The 5x5 neighborhood centered on the source block.</param>
  /// <param name="encoder">The encoder to convert TWork to TPixel.</param>
  /// <returns>The averaged and encoded output pixel.</returns>
  /// <remarks>
  /// <para>
  /// For 2:1 downscaling, reads P0P0, P0P1, P1P0, P1P1 (4 pixels).
  /// For 3:1 downscaling, reads M1M1 through P1P1 (9 pixels).
  /// For 4:1 downscaling, reads M1M1 through P2P2 (16 pixels).
  /// For 5:1 downscaling, reads M2M2 through P2P2 (25 pixels, full window).
  /// </para>
  /// <para>
  /// Implementations use internal accumulation for weighted averaging.
  /// </para>
  /// </remarks>
  TPixel Average(
    in NeighborWindow<TWork, TKey> window,
    in TEncode encoder
  );
}
