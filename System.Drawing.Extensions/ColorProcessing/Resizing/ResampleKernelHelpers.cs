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
using System.Drawing;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Helpers shared by every separable resampler kernel to compute the destination-pixel
/// region where the whole sample window falls inside the source image. The pipeline then
/// splits the outer loop into 4 edge bands + 1 safe interior and dispatches the interior
/// to an OOB-free inner loop — zero per-pixel branch, ready for SIMD vectorisation.
/// </summary>
public static class ResampleKernelHelpers {

  /// <summary>
  /// Computes the destination-pixel rectangle where a kernel with sample window
  /// <c>[x0 + kxMin, x0 + kxMaxExcl)</c> × <c>[y0 + kyMin, y0 + kyMaxExcl)</c> (with
  /// <c>x0 = floor(destX * scaleX + offsetX)</c>) is guaranteed to sample only in-bounds
  /// source pixels.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Returned as <c>Rectangle.FromLTRB(safeLeft, safeTop, safeRightExcl, safeBottomExcl)</c>.
  /// Zero-sized rectangle means no safe interior (image too small for the kernel) and the
  /// pipeline should fall back to the edge path for every destination pixel.
  /// </para>
  /// <para>
  /// For the library's separable kernels the window parameters are <c>kxMin = -radius + 1</c>,
  /// <c>kxMaxExcl = radius + 1</c> (same on Y). Bicubic &amp; Mitchell-Netravali use
  /// <c>(-1, +3)</c>, Bilinear <c>(0, +2)</c>, NearestNeighbor <c>(0, +1)</c>.
  /// </para>
  /// </remarks>
  public static Rectangle ComputeSafeDestinationRegion(
    int kxMin, int kxMaxExcl, float scaleX, float offsetX, int sourceWidth, int targetWidth,
    int kyMin, int kyMaxExcl, float scaleY, float offsetY, int sourceHeight, int targetHeight) {

    // x0 + kxMin >= 0  ⇔  floor(destX*sx + ox) >= -kxMin  ⇔  destX*sx + ox >= -kxMin
    var safeLeft = Math.Max(0, (int)Math.Ceiling((-kxMin - offsetX) / scaleX));

    // x0 + kxMaxExcl <= sourceWidth  ⇔  floor(destX*sx + ox) <= sourceWidth - kxMaxExcl
    //                              ⇔  destX*sx + ox < sourceWidth - kxMaxExcl + 1
    // First unsafe destX:  destX*sx + ox >= sourceWidth - kxMaxExcl + 1
    var safeRightExcl = Math.Min(targetWidth, (int)Math.Ceiling((sourceWidth - kxMaxExcl + 1 - offsetX) / scaleX));

    var safeTop = Math.Max(0, (int)Math.Ceiling((-kyMin - offsetY) / scaleY));
    var safeBottomExcl = Math.Min(targetHeight, (int)Math.Ceiling((sourceHeight - kyMaxExcl + 1 - offsetY) / scaleY));

    if (safeRightExcl < safeLeft) safeRightExcl = safeLeft;
    if (safeBottomExcl < safeTop) safeBottomExcl = safeTop;
    return Rectangle.FromLTRB(safeLeft, safeTop, safeRightExcl, safeBottomExcl);
  }

  /// <summary>
  /// Convenience overload for the <c>[-r+1, r+1)</c> window family used by every separable
  /// resampler in this library except NearestNeighbor (0,1) and Bilinear/Hermite/Cosine/Smoothstep (0,2).
  /// </summary>
  public static Rectangle ComputeSafeDestinationRegion(
    int radius,
    float scaleX, float offsetX, int sourceWidth, int targetWidth,
    float scaleY, float offsetY, int sourceHeight, int targetHeight)
    => ComputeSafeDestinationRegion(
      -radius + 1, radius + 1, scaleX, offsetX, sourceWidth, targetWidth,
      -radius + 1, radius + 1, scaleY, offsetY, sourceHeight, targetHeight);
}
