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
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Constants;
using Hawkynt.ColorProcessing.Spaces.Cmyk;
using Hawkynt.ColorProcessing.Spaces.Cylindrical;
using Hawkynt.ColorProcessing.Spaces.Hdr;
using Hawkynt.ColorProcessing.Spaces.Lab;
using Hawkynt.ColorProcessing.Spaces.Perceptual;
using Hawkynt.ColorProcessing.Spaces.WideGamut;
using Hawkynt.ColorProcessing.Spaces.Yuv;
using Hawkynt.ColorProcessing.Working;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.ColorMath;

/// <summary>
/// Generic color converter for extracting luminance and RGB components from arbitrary color spaces.
/// </summary>
/// <remarks>
/// <para>Provides type-safe conversion without unsafe pointer casting.</para>
/// <para>Uses fast paths for known working spaces (LinearRgbF, LinearRgbaF) and interface fallbacks for others.</para>
/// <para>All output ranges are normalized to 0.0-1.0.</para>
/// </remarks>
public static class ColorConverter {

  /// <summary>
  /// Gets the luminance of a color in the range 0.0-1.0.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to analyze.</param>
  /// <param name="highQuality">If true, uses Oklab L*; if false, uses BT.601 YUV Y.</param>
  /// <returns>Luminance value in 0.0-1.0 range.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetLuminance<TWork>(in TWork color, bool highQuality = false)
    where TWork : unmanaged, IColorSpace {

    // Fast path for LinearRgbF (JIT eliminates this check at compile time)
    if (typeof(TWork) == typeof(LinearRgbF)) {
      ref readonly var rgb = ref Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));
      return highQuality
        ? _GetOklabLuminance(rgb.R, rgb.G, rgb.B)
        : ColorConstants.BT601_R * rgb.R + ColorConstants.BT601_G * rgb.G + ColorConstants.BT601_B * rgb.B;
    }

    // Fast path for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      ref readonly var rgba = ref Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return highQuality
        ? _GetOklabLuminance(rgba.R, rgba.G, rgba.B)
        : ColorConstants.BT601_R * rgba.R + ColorConstants.BT601_G * rgba.G + ColorConstants.BT601_B * rgba.B;
    }

    // Fallback via interface
    return _GetLuminanceFallback(color, highQuality);
  }

  /// <summary>
  /// Gets normalized RGB components (0.0-1.0 range) from a color.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to extract RGB from.</param>
  /// <returns>A tuple of (R, G, B) values in 0.0-1.0 range.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float R, float G, float B) GetNormalizedRgb<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Fast path for LinearRgbF
    if (typeof(TWork) == typeof(LinearRgbF)) {
      ref readonly var rgb = ref Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));
      return (rgb.R, rgb.G, rgb.B);
    }

    // Fast path for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      ref readonly var rgba = ref Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return (rgba.R, rgba.G, rgba.B);
    }

    // Fallback via interface
    return _GetNormalizedRgbFallback(color);
  }

  /// <summary>
  /// Gets normalized RGBA components (0.0-1.0 range) from a color.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to extract RGBA from.</param>
  /// <returns>A tuple of (R, G, B, A) values in 0.0-1.0 range.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static (float R, float G, float B, float A) GetNormalizedRgba<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Fast path for LinearRgbF (no alpha, return opaque)
    if (typeof(TWork) == typeof(LinearRgbF)) {
      ref readonly var rgb = ref Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));
      return (rgb.R, rgb.G, rgb.B, 1f);
    }

    // Fast path for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      ref readonly var rgba = ref Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return (rgba.R, rgba.G, rgba.B, rgba.A);
    }

    // Fallback via interface
    var (r, g, b) = _GetNormalizedRgbFallback(color);
    var a = _GetAlphaFallback(color);
    return (r, g, b, a);
  }

  /// <summary>
  /// Gets the alpha component from a color (1.0 if not available).
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to extract alpha from.</param>
  /// <returns>Alpha value in 0.0-1.0 range, or 1.0 for 3-component colors.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float GetAlpha<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Fast path for LinearRgbF (no alpha)
    if (typeof(TWork) == typeof(LinearRgbF))
      return 1f;

    // Fast path for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      ref readonly var rgba = ref Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return rgba.A;
    }

    // Fallback via interface
    return _GetAlphaFallback(color);
  }

  /// <summary>
  /// Converts a working color to LinearRgbF.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to convert.</param>
  /// <returns>The color as LinearRgbF.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbF GetLinearRgb<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Identity for LinearRgbF
    if (typeof(TWork) == typeof(LinearRgbF))
      return Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));

    // Extract RGB from LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      ref readonly var rgba = ref Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));
      return new(rgba.R, rgba.G, rgba.B);
    }

    // Fallback via interface
    var (r, g, b) = _GetNormalizedRgbFallback(color);
    return new(r, g, b);
  }

  /// <summary>
  /// Converts a working color to LinearRgbaF.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="color">The color to convert.</param>
  /// <returns>The color as LinearRgbaF (alpha = 1.0 for 3-component colors).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static LinearRgbaF GetLinearRgba<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Expand LinearRgbF to include alpha
    if (typeof(TWork) == typeof(LinearRgbF)) {
      ref readonly var rgb = ref Unsafe.As<TWork, LinearRgbF>(ref Unsafe.AsRef(in color));
      return new(rgb.R, rgb.G, rgb.B, 1f);
    }

    // Identity for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF))
      return Unsafe.As<TWork, LinearRgbaF>(ref Unsafe.AsRef(in color));

    // Fallback via interface
    var (r, g, b) = _GetNormalizedRgbFallback(color);
    var a = _GetAlphaFallback(color);
    return new(r, g, b, a);
  }

  /// <summary>
  /// Creates a TWork color from normalized RGBA components.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="r">Red component (0.0-1.0).</param>
  /// <param name="g">Green component (0.0-1.0).</param>
  /// <param name="b">Blue component (0.0-1.0).</param>
  /// <param name="a">Alpha component (0.0-1.0).</param>
  /// <returns>A new TWork color instance.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork FromNormalizedRgba<TWork>(float r, float g, float b, float a)
    where TWork : unmanaged, IColorSpace {

    // Fast path for LinearRgbF
    if (typeof(TWork) == typeof(LinearRgbF)) {
      var result = new LinearRgbF(r, g, b);
      return Unsafe.As<LinearRgbF, TWork>(ref result);
    }

    // Fast path for LinearRgbaF
    if (typeof(TWork) == typeof(LinearRgbaF)) {
      var result = new LinearRgbaF(r, g, b, a);
      return Unsafe.As<LinearRgbaF, TWork>(ref result);
    }

    // Fallback via ColorFactory for known 4F types
    return _FromNormalizedRgbaFallback<TWork>(r, g, b, a);
  }

  /// <summary>
  /// Creates a TWork color from normalized RGB components with full opacity.
  /// </summary>
  /// <typeparam name="TWork">The working color type.</typeparam>
  /// <param name="r">Red component (0.0-1.0).</param>
  /// <param name="g">Green component (0.0-1.0).</param>
  /// <param name="b">Blue component (0.0-1.0).</param>
  /// <returns>A new TWork color instance with alpha = 1.0.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TWork FromNormalizedRgb<TWork>(float r, float g, float b)
    where TWork : unmanaged, IColorSpace
    => FromNormalizedRgba<TWork>(r, g, b, 1f);

  #region Private Helpers

  /// <summary>
  /// Computes Oklab L* (perceptual lightness) from linear RGB using the existing projector.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static float _GetOklabLuminance(float r, float g, float b)
    => default(LinearRgbFToOklabF).Project(new LinearRgbF(r, g, b)).L;

  /// <summary>
  /// Fallback luminance extraction via projectors.
  /// </summary>
  /// <remarks>
  /// Converts any supported color space to LinearRgbF using the appropriate projector,
  /// then calculates luminance. Returns 0.5 (middle gray) for unknown types.
  /// </remarks>
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static float _GetLuminanceFallback<TWork>(in TWork color, bool highQuality)
    where TWork : unmanaged, IColorSpace {
    var (r, g, b) = _GetNormalizedRgbFallback(color);
    return highQuality
      ? _GetOklabLuminance(r, g, b)
      : ColorConstants.BT601_R * r + ColorConstants.BT601_G * g + ColorConstants.BT601_B * b;
  }

  /// <summary>
  /// Fallback RGB extraction via projectors.
  /// </summary>
  /// <remarks>
  /// Converts any supported color space to LinearRgbF using the appropriate projector,
  /// then extracts RGB components. Returns middle gray for unknown types.
  /// </remarks>
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static (float R, float G, float B) _GetNormalizedRgbFallback<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace {

    // Perceptual spaces
    if (typeof(TWork) == typeof(OklabF)) {
      var c = Unsafe.As<TWork, OklabF>(ref Unsafe.AsRef(in color));
      var rgb = default(OklabFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(OklchF)) {
      var c = Unsafe.As<TWork, OklchF>(ref Unsafe.AsRef(in color));
      var rgb = default(OklchFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(LuvF)) {
      var c = Unsafe.As<TWork, LuvF>(ref Unsafe.AsRef(in color));
      var rgb = default(LuvFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(Din99F)) {
      var c = Unsafe.As<TWork, Din99F>(ref Unsafe.AsRef(in color));
      var rgb = default(Din99FToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(HunterLabF)) {
      var c = Unsafe.As<TWork, HunterLabF>(ref Unsafe.AsRef(in color));
      var rgb = default(HunterLabFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Lab space
    if (typeof(TWork) == typeof(LabF)) {
      var c = Unsafe.As<TWork, LabF>(ref Unsafe.AsRef(in color));
      var rgb = default(LabFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Cylindrical spaces
    if (typeof(TWork) == typeof(HslF)) {
      var c = Unsafe.As<TWork, HslF>(ref Unsafe.AsRef(in color));
      var rgb = default(HslFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(HsvF)) {
      var c = Unsafe.As<TWork, HsvF>(ref Unsafe.AsRef(in color));
      var rgb = default(HsvFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(HwbF)) {
      var c = Unsafe.As<TWork, HwbF>(ref Unsafe.AsRef(in color));
      var rgb = default(HwbFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(LchF)) {
      var c = Unsafe.As<TWork, LchF>(ref Unsafe.AsRef(in color));
      var rgb = default(LchFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Video spaces
    if (typeof(TWork) == typeof(YuvF)) {
      var c = Unsafe.As<TWork, YuvF>(ref Unsafe.AsRef(in color));
      var rgb = default(YuvFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(YCbCrF)) {
      var c = Unsafe.As<TWork, YCbCrF>(ref Unsafe.AsRef(in color));
      var rgb = default(YCbCrBt601FToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // HDR spaces
    if (typeof(TWork) == typeof(XyzF)) {
      var c = Unsafe.As<TWork, XyzF>(ref Unsafe.AsRef(in color));
      var rgb = default(XyzFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(JzAzBzF)) {
      var c = Unsafe.As<TWork, JzAzBzF>(ref Unsafe.AsRef(in color));
      var rgb = default(JzAzBzFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(JzCzhzF)) {
      var c = Unsafe.As<TWork, JzCzhzF>(ref Unsafe.AsRef(in color));
      var rgb = default(JzCzhzFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(ICtCpF)) {
      var c = Unsafe.As<TWork, ICtCpF>(ref Unsafe.AsRef(in color));
      var rgb = default(ICtCpFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Wide gamut spaces
    if (typeof(TWork) == typeof(AdobeRgbF)) {
      var c = Unsafe.As<TWork, AdobeRgbF>(ref Unsafe.AsRef(in color));
      var rgb = default(AdobeRgbFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(DisplayP3F)) {
      var c = Unsafe.As<TWork, DisplayP3F>(ref Unsafe.AsRef(in color));
      var rgb = default(DisplayP3FToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(ProPhotoRgbF)) {
      var c = Unsafe.As<TWork, ProPhotoRgbF>(ref Unsafe.AsRef(in color));
      var rgb = default(ProPhotoRgbFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    if (typeof(TWork) == typeof(AcesCgF)) {
      var c = Unsafe.As<TWork, AcesCgF>(ref Unsafe.AsRef(in color));
      var rgb = default(AcesCgFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Print spaces
    if (typeof(TWork) == typeof(CmykF)) {
      var c = Unsafe.As<TWork, CmykF>(ref Unsafe.AsRef(in color));
      var rgb = default(CmykFToLinearRgbF).Project(c);
      return (rgb.R, rgb.G, rgb.B);
    }

    // Unknown type - return gray
    return (0.5f, 0.5f, 0.5f);
  }

  /// <summary>
  /// Fallback alpha extraction for unknown types.
  /// </summary>
  /// <remarks>
  /// In practice, TWork is always LinearRgbF or LinearRgbaF which use the fast paths.
  /// This fallback exists only for API completeness and returns fully opaque.
  /// </remarks>
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static float _GetAlphaFallback<TWork>(in TWork color)
    where TWork : unmanaged, IColorSpace
    // Unknown type - return opaque
    => 1f;

  /// <summary>
  /// Fallback color creation via projectors.
  /// </summary>
  /// <remarks>
  /// Creates a LinearRgbF from the RGBA components and projects it to the target type
  /// using the appropriate reverse projector. Returns default for unknown types.
  /// </remarks>
  [MethodImpl(MethodImplOptions.NoInlining)]
  private static TWork _FromNormalizedRgbaFallback<TWork>(float r, float g, float b, float a)
    where TWork : unmanaged, IColorSpace {
    var rgb = new LinearRgbF(r, g, b);

    // Perceptual spaces
    if (typeof(TWork) == typeof(OklabF)) {
      var result = default(LinearRgbFToOklabF).Project(rgb);
      return Unsafe.As<OklabF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(OklchF)) {
      var result = default(LinearRgbFToOklchF).Project(rgb);
      return Unsafe.As<OklchF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(LuvF)) {
      var result = default(LinearRgbFToLuvF).Project(rgb);
      return Unsafe.As<LuvF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(Din99F)) {
      var result = default(LinearRgbFToDin99F).Project(rgb);
      return Unsafe.As<Din99F, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(HunterLabF)) {
      var result = default(LinearRgbFToHunterLabF).Project(rgb);
      return Unsafe.As<HunterLabF, TWork>(ref result);
    }

    // Lab space
    if (typeof(TWork) == typeof(LabF)) {
      var result = default(LinearRgbFToLabF).Project(rgb);
      return Unsafe.As<LabF, TWork>(ref result);
    }

    // Cylindrical spaces
    if (typeof(TWork) == typeof(HslF)) {
      var result = default(LinearRgbFToHslF).Project(rgb);
      return Unsafe.As<HslF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(HsvF)) {
      var result = default(LinearRgbFToHsvF).Project(rgb);
      return Unsafe.As<HsvF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(HwbF)) {
      var result = default(LinearRgbFToHwbF).Project(rgb);
      return Unsafe.As<HwbF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(LchF)) {
      var result = default(LinearRgbFToLchF).Project(rgb);
      return Unsafe.As<LchF, TWork>(ref result);
    }

    // Video spaces
    if (typeof(TWork) == typeof(YuvF)) {
      var result = default(LinearRgbFToYuvF).Project(rgb);
      return Unsafe.As<YuvF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(YCbCrF)) {
      var result = default(LinearRgbFToYCbCrBt601F).Project(rgb);
      return Unsafe.As<YCbCrF, TWork>(ref result);
    }

    // HDR spaces
    if (typeof(TWork) == typeof(XyzF)) {
      var result = default(LinearRgbFToXyzF).Project(rgb);
      return Unsafe.As<XyzF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(JzAzBzF)) {
      var result = default(LinearRgbFToJzAzBzF).Project(rgb);
      return Unsafe.As<JzAzBzF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(JzCzhzF)) {
      var result = default(LinearRgbFToJzCzhzF).Project(rgb);
      return Unsafe.As<JzCzhzF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(ICtCpF)) {
      var result = default(LinearRgbFToICtCpF).Project(rgb);
      return Unsafe.As<ICtCpF, TWork>(ref result);
    }

    // Wide gamut spaces
    if (typeof(TWork) == typeof(AdobeRgbF)) {
      var result = default(LinearRgbFToAdobeRgbF).Project(rgb);
      return Unsafe.As<AdobeRgbF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(DisplayP3F)) {
      var result = default(LinearRgbFToDisplayP3F).Project(rgb);
      return Unsafe.As<DisplayP3F, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(ProPhotoRgbF)) {
      var result = default(LinearRgbFToProPhotoRgbF).Project(rgb);
      return Unsafe.As<ProPhotoRgbF, TWork>(ref result);
    }

    if (typeof(TWork) == typeof(AcesCgF)) {
      var result = default(LinearRgbFToAcesCgF).Project(rgb);
      return Unsafe.As<AcesCgF, TWork>(ref result);
    }

    // Print spaces
    if (typeof(TWork) == typeof(CmykF)) {
      var result = default(LinearRgbFToCmykF).Project(rgb);
      return Unsafe.As<CmykF, TWork>(ref result);
    }

    // Unknown type - return default
    return default;
  }

  #endregion
}
