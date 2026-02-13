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
using Hawkynt.ColorProcessing.FrequencyDomain;

namespace Hawkynt.Drawing;

/// <summary>
/// Provides frequency domain extension methods for <see cref="Bitmap"/>.
/// </summary>
public static class BitmapFrequencyDomainExtensions {

  extension(Bitmap @this) {

    /// <summary>
    /// Converts a bitmap to frequency domain using FFT (grayscale).
    /// </summary>
    /// <returns>A 2D complex array containing the frequency spectrum.</returns>
    public Complex[,] ToFrequencyDomain() {
      var w = @this.Width;
      var h = @this.Height;
      var pw = Fft1D.NextPowerOf2(w);
      var ph = Fft1D.NextPowerOf2(h);
      var data = new Complex[ph, pw];

      using var locker = @this.Lock(ImageLockMode.ReadOnly);
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var c = locker[x, y];
        var gray = (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 255f;
        data[y, x] = new Complex(gray, 0f);
      }

      Fft2D.Forward(data);
      return data;
    }

    /// <summary>
    /// Converts a bitmap to DCT domain (grayscale).
    /// </summary>
    /// <returns>A 2D float array containing DCT coefficients.</returns>
    public float[,] ToDctDomain() {
      var w = @this.Width;
      var h = @this.Height;
      var data = new float[h, w];

      using var locker = @this.Lock(ImageLockMode.ReadOnly);
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var c = locker[x, y];
        data[y, x] = (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 255f;
      }

      Dct2D.Forward(data);
      return data;
    }

    /// <summary>
    /// Visualizes the magnitude spectrum (log-scaled) of the bitmap's FFT.
    /// </summary>
    /// <returns>A new bitmap showing the magnitude spectrum.</returns>
    public Bitmap GetMagnitudeSpectrum() {
      var spectrum = @this.ToFrequencyDomain();
      var h = spectrum.GetLength(0);
      var w = spectrum.GetLength(1);

      var mag = new float[h, w];
      var maxMag = 0f;
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var m = (float)Math.Log(1 + spectrum[y, x].Magnitude);
        mag[y, x] = m;
        if (m > maxMag)
          maxMag = m;
      }

      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var locker = result.Lock(ImageLockMode.WriteOnly);
      var scale = maxMag > 0 ? 255f / maxMag : 0f;
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        // Shift zero-frequency to center
        var sy = (y + h / 2) % h;
        var sx = (x + w / 2) % w;
        var v = (int)Math.Min(255, mag[sy, sx] * scale);
        locker[x, y] = Color.FromArgb(255, v, v, v);
      }

      return result;
    }

    /// <summary>
    /// Visualizes the phase spectrum of the bitmap's FFT.
    /// </summary>
    /// <returns>A new bitmap showing the phase spectrum.</returns>
    public Bitmap GetPhaseSpectrum() {
      var spectrum = @this.ToFrequencyDomain();
      var h = spectrum.GetLength(0);
      var w = spectrum.GetLength(1);

      var result = new Bitmap(w, h, PixelFormat.Format32bppArgb);
      using var locker = result.Lock(ImageLockMode.WriteOnly);
      for (var y = 0; y < h; ++y)
      for (var x = 0; x < w; ++x) {
        var sy = (y + h / 2) % h;
        var sx = (x + w / 2) % w;
        var phase = spectrum[sy, sx].Phase;
        var normalized = (phase + (float)Math.PI) / (2f * (float)Math.PI);
        var v = (int)Math.Min(255, Math.Max(0, normalized * 255f));
        locker[x, y] = Color.FromArgb(255, v, v, v);
      }

      return result;
    }
  }

  /// <summary>
  /// Reconstructs a bitmap from a complex frequency spectrum using inverse FFT.
  /// </summary>
  /// <param name="spectrum">The frequency spectrum.</param>
  /// <param name="width">The target width.</param>
  /// <param name="height">The target height.</param>
  /// <returns>A new grayscale bitmap.</returns>
  public static Bitmap FromFrequencyDomain(Complex[,] spectrum, int width, int height) {
    var copy = (Complex[,])spectrum.Clone();
    Fft2D.Inverse(copy);

    var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using var locker = result.Lock(ImageLockMode.WriteOnly);
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var v = (int)Math.Min(255, Math.Max(0, copy[y, x].Real * 255f + 0.5f));
      locker[x, y] = Color.FromArgb(255, v, v, v);
    }

    return result;
  }

  /// <summary>
  /// Reconstructs a bitmap from DCT coefficients using inverse DCT.
  /// </summary>
  /// <param name="coefficients">The DCT coefficients.</param>
  /// <param name="width">The target width.</param>
  /// <param name="height">The target height.</param>
  /// <returns>A new grayscale bitmap.</returns>
  public static Bitmap FromDctDomain(float[,] coefficients, int width, int height) {
    var copy = (float[,])coefficients.Clone();
    Dct2D.Inverse(copy);

    var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using var locker = result.Lock(ImageLockMode.WriteOnly);
    for (var y = 0; y < height; ++y)
    for (var x = 0; x < width; ++x) {
      var v = (int)Math.Min(255, Math.Max(0, copy[y, x] * 255f + 0.5f));
      locker[x, y] = Color.FromArgb(255, v, v, v);
    }

    return result;
  }
}
