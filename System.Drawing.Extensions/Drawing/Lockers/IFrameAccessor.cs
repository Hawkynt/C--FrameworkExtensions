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
using Hawkynt.ColorProcessing.Pipeline;

namespace Hawkynt.Drawing.Lockers;

/// <summary>
/// Provides access to pixel data as a frame with typed pixel access.
/// </summary>
/// <typeparam name="TPixel">The pixel type.</typeparam>
internal interface IFrameAccessor<TPixel> : IBitmapLocker where TPixel : unmanaged {
  /// <summary>Stride in pixels.</summary>
  int Stride { get; }

  /// <summary>
  /// Gets the pixel data as a span.
  /// </summary>
  Span<TPixel> Pixels { get; }

  /// <summary>
  /// Creates a PixelFrame view over this accessor.
  /// </summary>
  PixelFrame<TPixel> AsFrame();
}
