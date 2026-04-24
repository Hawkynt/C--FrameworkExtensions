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
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Dithering;

/// <summary>
/// Multi-scale hybrid ditherer — adds a low-frequency ordered pre-bias
/// (Bayer 8×8) to the input, then runs Floyd-Steinberg error diffusion on
/// the biased pixels, producing a composite dither that spreads grain
/// across both low- and high-frequency bands.
/// </summary>
/// <remarks>
/// <para>
/// Plain error diffusion concentrates residual energy in mid-to-high
/// spatial frequencies (the "blue-noise" that makes FS output look so
/// clean on smooth ramps), but at low palette counts the FS carry can
/// build up systematic offsets visible as slow bands. Plain ordered
/// dithering distributes energy evenly across all frequencies but produces
/// the tell-tale "cross-hatch" pattern Bayer screens are known for.
/// </para>
/// <para>
/// The multi-scale hybrid splits the energy budget across both: an 8×8
/// Bayer pre-bias injects low-frequency randomness into the source pixel
/// (breaking up potential FS bands before they can accumulate), then the
/// Floyd-Steinberg propagator handles the high-frequency residual. The
/// result has the visual sharpness of FS on edges with the long-gradient
/// behaviour of ordered dither — composite output quality sits above
/// either technique used alone on difficult images.
/// </para>
/// <para>
/// References: multi-scale / frequency-split dithering is a classical
/// halftoning theme; see D. Lau &amp; G. Arce, <i>Modern Digital
/// Halftoning</i>, CRC Press 2008, §5.4 "Hybrid screening". The specific
/// Bayer-preconditioned-FS recipe is discussed in F. Knight et al. 2004,
/// "Digital halftoning by hybrid ordered / error-diffusion dither",
/// <i>Proc. NIP20</i>.
/// </para>
/// <para>Sequential (error-diffusion). Deterministic.</para>
/// </remarks>
[Ditherer("Multi-Scale Hybrid", Description = "Bayer-8x8 low-freq pre-bias followed by Floyd-Steinberg error diffusion", Type = DitheringType.ErrorDiffusion)]
public readonly struct MultiScaleHybridDitherer : IDitherer {

  private const int _BAYER = 8;
  private static readonly float[] _Bayer8 = _BuildBayer();

  private readonly float _preBiasStrength;

  /// <summary>Default instance (pre-bias strength 0.3).</summary>
  public static MultiScaleHybridDitherer Instance { get; } = new();

  /// <summary>Creates a multi-scale hybrid ditherer.</summary>
  /// <param name="preBiasStrength">
  /// Amount of low-frequency ordered bias to inject before error diffusion,
  /// in [0, 1]. 0 = pure FS; 1 = full Bayer-8 amplitude. Default 0.3.
  /// </param>
  public MultiScaleHybridDitherer(float preBiasStrength = 0.3f) {
    this._preBiasStrength = Math.Max(0f, Math.Min(1f, preBiasStrength));
  }

  /// <summary>Returns this ditherer with the specified pre-bias strength.</summary>
  public MultiScaleHybridDitherer WithPreBiasStrength(float strength) => new(strength);

  /// <inheritdoc />
  public bool RequiresSequentialProcessing => true;

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Dither<TWork, TPixel, TDecode, TMetric>(
    TPixel* source,
    byte* indices,
    int width,
    int height,
    int sourceStride,
    int targetStride,
    int startY,
    in TDecode decoder,
    in TMetric metric,
    TWork[] palette)
    where TWork : unmanaged, IColorSpace4<TWork>
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TMetric : struct, IColorMetric<TWork> {

    var lookup = new PaletteLookup<TWork, TMetric>(palette, metric);
    var endY = startY + height;
    var biasStrength = this._preBiasStrength > 0 ? this._preBiasStrength : 0.3f;

    var errR = new float[width, height];
    var errG = new float[width, height];
    var errB = new float[width, height];

    for (var y = startY; y < endY; ++y) {
      var localY = y - startY;
      for (var x = 0; x < width; ++x) {
        var color = decoder.Decode(source[y * sourceStride + x]);
        var (c1, c2, c3, alpha) = color.ToNormalized();

        // Low-freq ordered pre-bias.
        var bias = _Bayer8[(y & (_BAYER - 1)) * _BAYER + (x & (_BAYER - 1))] * biasStrength;
        var pr = c1.ToFloat() + errR[x, localY] + bias;
        var pg = c2.ToFloat() + errG[x, localY] + bias;
        var pb = c3.ToFloat() + errB[x, localY] + bias;
        var adjR = Math.Max(0f, Math.Min(1f, pr));
        var adjG = Math.Max(0f, Math.Min(1f, pg));
        var adjB = Math.Max(0f, Math.Min(1f, pb));

        var adj = ColorFactory.FromNormalized_4<TWork>(
          UNorm32.FromFloatClamped(adjR),
          UNorm32.FromFloatClamped(adjG),
          UNorm32.FromFloatClamped(adjB),
          alpha);

        var idx = (byte)lookup.FindNearest(adj, out var nearest);
        indices[y * targetStride + x] = idx;

        var (n1, n2, n3, _) = nearest.ToNormalized();
        // FS residual — error against the biased value, not the source.
        var er = adjR - n1.ToFloat();
        var eg = adjG - n2.ToFloat();
        var eb = adjB - n3.ToFloat();

        _Deposit(errR, errG, errB, x + 1, localY, er, eg, eb, 7f / 16f, width, height);
        if (localY + 1 < height) {
          _Deposit(errR, errG, errB, x - 1, localY + 1, er, eg, eb, 3f / 16f, width, height);
          _Deposit(errR, errG, errB, x, localY + 1, er, eg, eb, 5f / 16f, width, height);
          _Deposit(errR, errG, errB, x + 1, localY + 1, er, eg, eb, 1f / 16f, width, height);
        }
      }
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void _Deposit(float[,] errR, float[,] errG, float[,] errB, int x, int y, float er, float eg, float eb, float w, int width, int height) {
    if (x < 0 || x >= width || y < 0 || y >= height) return;
    errR[x, y] += er * w;
    errG[x, y] += eg * w;
    errB[x, y] += eb * w;
  }

  private static float[] _BuildBayer() {
    var raw = BayerMatrix.Generate(_BAYER);
    var max = _BAYER * _BAYER;
    var result = new float[max];
    for (var y = 0; y < _BAYER; ++y)
    for (var x = 0; x < _BAYER; ++x)
      result[y * _BAYER + x] = (raw[y, x] + 0.5f) / max - 0.5f;
    return result;
  }
}
