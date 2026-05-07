#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.

#endregion

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.Drawing;

/// <summary>
/// Integer-only Unsharp Mask sharpening: <c>output = original + amount · (original − GaussianBlur(original))</c>.
/// Uses <see cref="BitmapGaussianBlurIntExtensions.GaussianBlurInt"/> as the underlying
/// blur primitive, then applies the difference-and-add in pure int arithmetic with a
/// configurable threshold to suppress amplification of low-magnitude noise.
/// </summary>
public static class BitmapUnsharpMaskIntExtensions {

  /// <param name="this">The source bitmap.</param>
  extension(Bitmap @this) {

    /// <summary>
    /// Applies an int-only Unsharp Mask sharpening pass.
    /// </summary>
    /// <param name="sigma">Gaussian σ for the underlying blur (same scale as
    /// <see cref="BitmapGaussianBlurIntExtensions.GaussianBlurInt"/>).</param>
    /// <param name="amount">Sharpening strength. 1.0 ≈ moderate, 2.0 ≈ aggressive.</param>
    /// <param name="threshold">Per-channel byte threshold below which differences are
    /// not amplified (suppresses noise enhancement). 0 = no threshold; 4-8 typical.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitmap UnsharpMaskInt(double sigma = 1.5, double amount = 1.0, int threshold = 0) {
      ArgumentOutOfRangeException.ThrowIfNegative(amount);
      ArgumentOutOfRangeException.ThrowIfNegative(threshold);
      if (sigma <= 0 || amount == 0)
        return (Bitmap)@this.Clone();

      // amount as 16.16 fixed-point: lets us multiply byte differences by amount
      // without converting to float. Range: amount ∈ [0, ~32k].
      var amountFix = (int)(amount * 65536.0 + 0.5);

      using var blurred = @this.GaussianBlurInt(sigma);
      return UnsharpMaskIntPipeline.Compose(@this, blurred, amountFix, threshold);
    }
  }
}

internal static class UnsharpMaskIntPipeline {

  public static unsafe Bitmap Compose(Bitmap original, Bitmap blurred, int amountFix16, int threshold) {
    var w = original.Width;
    var h = original.Height;
    if (blurred.Width != w || blurred.Height != h)
      throw new InvalidOperationException("UnsharpMask: blurred bitmap dimensions don't match original");

    var rect = new Rectangle(0, 0, w, h);
    var origData = original.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    var blurData = blurred.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

    var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
    var resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    try {
      var pOrig = (byte*)origData.Scan0;
      var pBlur = (byte*)blurData.Scan0;
      var pDst = (byte*)resultData.Scan0;
      var oStride = origData.Stride;
      var bStride = blurData.Stride;
      var dStride = resultData.Stride;

      for (var y = 0; y < h; ++y) {
        var oRow = pOrig + y * oStride;
        var bRow = pBlur + y * bStride;
        var dRow = pDst + y * dStride;
        for (var x = 0; x < w; ++x) {
          var off = x * 4;
          // Per-channel: result = original + amount * (original - blurred); clamp to [0, 255].
          // Use 16.16 fixed-point for the amount multiply: signed 32-bit covers
          // worst-case ±255 difference times amount up to ~32k (more than enough).
          var dB = oRow[off + 0] - bRow[off + 0];
          var dG = oRow[off + 1] - bRow[off + 1];
          var dR = oRow[off + 2] - bRow[off + 2];
          // Threshold suppression: if |diff| ≤ threshold, treat diff as 0.
          // Otherwise amplify the unscaled diff by amount.
          var addB = (dB > threshold || dB < -threshold) ? (dB * amountFix16) >> 16 : 0;
          var addG = (dG > threshold || dG < -threshold) ? (dG * amountFix16) >> 16 : 0;
          var addR = (dR > threshold || dR < -threshold) ? (dR * amountFix16) >> 16 : 0;
          var ob = oRow[off + 0] + addB;
          var og = oRow[off + 1] + addG;
          var or = oRow[off + 2] + addR;
          if (ob < 0) ob = 0; else if (ob > 255) ob = 255;
          if (og < 0) og = 0; else if (og > 255) og = 255;
          if (or < 0) or = 0; else if (or > 255) or = 255;
          dRow[off + 0] = (byte)ob;
          dRow[off + 1] = (byte)og;
          dRow[off + 2] = (byte)or;
          dRow[off + 3] = oRow[off + 3]; // alpha passthrough
        }
      }
    } finally {
      original.UnlockBits(origData);
      blurred.UnlockBits(blurData);
      result.UnlockBits(resultData);
    }
    return result;
  }
}
