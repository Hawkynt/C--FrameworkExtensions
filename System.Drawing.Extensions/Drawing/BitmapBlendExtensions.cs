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
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Blending;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.Drawing.Lockers;
using NormalBlendMode = Hawkynt.ColorProcessing.Blending.BlendModes.Normal;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides extension methods for blending bitmaps using various blend modes.
/// </summary>
public static class BitmapBlendExtensions {

  /// <param name="this">The background bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Blends an overlay bitmap on top using the specified blend mode.
    /// </summary>
    /// <typeparam name="TMode">The blend mode type.</typeparam>
    /// <param name="overlay">The foreground bitmap to blend on top.</param>
    /// <param name="strength">The blend strength in [0,1]. 0 = original, 1 = full blend.</param>
    /// <returns>A new bitmap with the blend applied.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap BlendWith<TMode>(Bitmap overlay, float strength = 1f)
      where TMode : struct, IBlendMode {
      var result = new Bitmap(@this.Width, @this.Height, PixelFormat.Format32bppArgb);
      using var bgLock = @this.Lock();
      using var fgLock = overlay.Lock();
      using var destLock = result.Lock();
      _Blend<TMode>(bgLock, fgLock, destLock, strength);
      return result;
    }

    /// <summary>
    /// Blends an overlay bitmap on top in-place using the specified blend mode.
    /// </summary>
    /// <typeparam name="TMode">The blend mode type.</typeparam>
    /// <param name="overlay">The foreground bitmap to blend on top.</param>
    /// <param name="strength">The blend strength in [0,1]. 0 = no change, 1 = full blend.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void BlendInto<TMode>(Bitmap overlay, float strength = 1f)
      where TMode : struct, IBlendMode {
      using var bgLock = @this.Lock();
      using var fgLock = overlay.Lock();
      _Blend<TMode>(bgLock, fgLock, bgLock, strength);
    }

    /// <summary>
    /// Applies a pixel filter and blends the result using the specified blend mode.
    /// </summary>
    /// <typeparam name="TMode">The blend mode type.</typeparam>
    /// <typeparam name="TFilter">The pixel filter type.</typeparam>
    /// <param name="filter">The filter instance to apply.</param>
    /// <param name="strength">The blend strength in [0,1]. 0 = original, 1 = full filter effect.</param>
    /// <returns>A new bitmap with the filter applied and blended.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap BlendWith<TMode, TFilter>(TFilter filter, float strength = 1f)
      where TMode : struct, IBlendMode
      where TFilter : struct, IPixelFilter {
      using var filtered = @this.ApplyFilter(filter);
      return @this.BlendWith<TMode>(filtered, strength);
    }

    /// <summary>
    /// Applies a pixel filter and blends the result using Normal blend mode (opacity fade).
    /// </summary>
    /// <typeparam name="TFilter">The pixel filter type.</typeparam>
    /// <param name="filter">The filter instance to apply.</param>
    /// <param name="strength">The blend strength in [0,1]. 0 = original, 1 = full filter effect.</param>
    /// <returns>A new bitmap with the filter applied at the specified opacity.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap BlendWith<TFilter>(TFilter filter, float strength = 1f)
      where TFilter : struct, IPixelFilter
      => @this.BlendWith<NormalBlendMode, TFilter>(filter, strength);
  }

  private static void _Blend<TMode>(IBitmapLocker bg, IBitmapLocker fg, IBitmapLocker dest, float strength)
    where TMode : struct, IBlendMode {
    var mode = default(TMode);
    var isFullPixel = mode is IFullPixelBlendMode;
    var width = Math.Min(bg.Width, fg.Width);
    var height = Math.Min(bg.Height, fg.Height);

    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var bgColor = bg[x, y];
      var fgColor = fg[x, y];

      var bgA = bgColor.A / 255f;
      var fgA = fgColor.A / 255f;

      var bgR = bgColor.R / 255f;
      var bgG = bgColor.G / 255f;
      var bgB = bgColor.B / 255f;
      var fgR = fgColor.R / 255f;
      var fgG = fgColor.G / 255f;
      var fgB = fgColor.B / 255f;

      float blendR, blendG, blendB;
      if (isFullPixel) {
        var fullMode = (IFullPixelBlendMode)mode;
        (blendR, blendG, blendB) = fullMode.BlendPixel(bgR, bgG, bgB, fgR, fgG, fgB);
      } else {
        blendR = mode.Blend(bgR, fgR);
        blendG = mode.Blend(bgG, fgG);
        blendB = mode.Blend(bgB, fgB);
      }

      var effectiveAlpha = fgA * strength;

      var outR = bgR + (blendR - bgR) * effectiveAlpha;
      var outG = bgG + (blendG - bgG) * effectiveAlpha;
      var outB = bgB + (blendB - bgB) * effectiveAlpha;
      var outA = bgA + fgA * (1f - bgA);

      dest[x, y] = Color.FromArgb(
        _ToByte(outA),
        _ToByte(outR),
        _ToByte(outG),
        _ToByte(outB)
      );
    }

    if (dest.Width > width || dest.Height > height)
      _CopyBackground(bg, dest, width, height);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static byte _ToByte(float value)
    => (byte)Math.Max(0f, Math.Min(255f, value * 255f + 0.5f));

  private static void _CopyBackground(IBitmapLocker bg, IBitmapLocker dest, int blendWidth, int blendHeight) {
    for (var y = 0; y < dest.Height; ++y)
    for (var x = 0; x < dest.Width; ++x) {
      if (x < blendWidth && y < blendHeight)
        continue;

      dest[x, y] = x < bg.Width && y < bg.Height ? bg[x, y] : Color.Transparent;
    }
  }
}
