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
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Runtime.CompilerServices;
using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.ColorMath;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing.Resizing.Resamplers;

/// <summary>
/// AMD FidelityFX Super Resolution 1.0 (FSR) resampler.
/// </summary>
/// <remarks>
/// <para>Implements Edge-Adaptive Spatial Upsampling (EASU) algorithm.</para>
/// <para>Uses 12-tap filter with edge-aware weight distribution.</para>
/// <para>Detects local edge direction and applies directional interpolation.</para>
/// <para>Based on AMD's open-source FidelityFX SDK.</para>
/// </remarks>
[ScalerInfo("FSR", Author = "AMD", Year = 2021,
  Description = "FidelityFX Super Resolution 1.0 EASU upsampling", Category = ScalerCategory.Resampler)]
public readonly struct Fsr : IResampler {

  private readonly float _sharpness;

  /// <summary>
  /// Creates an FSR resampler with default sharpness.
  /// </summary>
  public Fsr() : this(0.5f) { }

  /// <summary>
  /// Creates an FSR resampler with custom sharpness.
  /// </summary>
  /// <param name="sharpness">Sharpness parameter (0-1). Higher values produce sharper results.</param>
  public Fsr(float sharpness) => this._sharpness = Math.Clamp(sharpness, 0f, 1f);

  /// <inheritdoc />
  public ScaleFactor Scale => default;

  /// <inheritdoc />
  public int Radius => 2;

  /// <inheritdoc />
  public PrefilterInfo? Prefilter => null;

  /// <inheritdoc />
  public TResult InvokeKernel<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult>(
    IResampleKernelCallback<TWork, TKey, TPixel, TDecode, TProject, TEncode, TResult> callback,
    int sourceWidth,
    int sourceHeight,
    int targetWidth,
    int targetHeight,
    bool useCenteredGrid = true)
    where TWork : unmanaged, IColorSpace4F<TWork>
    where TKey : unmanaged, IColorSpace
    where TPixel : unmanaged, IStorageSpace
    where TDecode : struct, IDecode<TPixel, TWork>
    where TProject : struct, IProject<TWork, TKey>
    where TEncode : struct, IEncode<TWork, TPixel>
    => callback.Invoke(new FsrKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
      sourceWidth, sourceHeight, targetWidth, targetHeight, this._sharpness, useCenteredGrid));

  /// <summary>
  /// Gets the default configuration.
  /// </summary>
  public static Fsr Default => new();

  /// <summary>
  /// Gets a sharper configuration.
  /// </summary>
  public static Fsr Sharp => new(0.8f);

  /// <summary>
  /// Gets a softer configuration with reduced artifacts.
  /// </summary>
  public static Fsr Soft => new(0.2f);
}

file readonly struct FsrKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>(
  int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, float sharpness, bool useCenteredGrid)
  : IResampleKernel<TPixel, TWork, TKey, TDecode, TProject, TEncode>
  where TPixel : unmanaged, IStorageSpace
  where TWork : unmanaged, IColorSpace4F<TWork>
  where TKey : unmanaged, IColorSpace
  where TDecode : struct, IDecode<TPixel, TWork>
  where TProject : struct, IProject<TWork, TKey>
  where TEncode : struct, IEncode<TWork, TPixel> {

  public int Radius => 2;
  public int SourceWidth => sourceWidth;
  public int SourceHeight => sourceHeight;
  public int TargetWidth => targetWidth;
  public int TargetHeight => targetHeight;

  // Precomputed scale factors and offsets for zero-cost grid centering
  private readonly float _scaleX = (float)sourceWidth / targetWidth;
  private readonly float _scaleY = (float)sourceHeight / targetHeight;
  private readonly float _offsetX = useCenteredGrid ? 0.5f * sourceWidth / targetWidth - 0.5f : 0f;
  private readonly float _offsetY = useCenteredGrid ? 0.5f * sourceHeight / targetHeight - 0.5f : 0f;
  private readonly float _sharpnessParam = 1f - sharpness * 0.5f; // Map 0-1 to 1-0.5 for filter width

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public unsafe void Resample(
    NeighborFrame<TPixel, TWork, TKey, TDecode, TProject> frame,
    int destX, int destY,
    TPixel* dest,
    int destStride,
    in TEncode encoder) {
    // Map destination pixel back to source coordinates
    var srcXf = destX * this._scaleX + this._offsetX;
    var srcYf = destY * this._scaleY + this._offsetY;

    // Integer base coordinates
    var x0 = (int)MathF.Floor(srcXf);
    var y0 = (int)MathF.Floor(srcYf);

    // Fractional parts
    var fx = srcXf - x0;
    var fy = srcYf - y0;

    // Sample 4x4 neighborhood for EASU
    var c00 = frame[x0 - 1, y0 - 1].Work;
    var c10 = frame[x0, y0 - 1].Work;
    var c20 = frame[x0 + 1, y0 - 1].Work;
    var c30 = frame[x0 + 2, y0 - 1].Work;

    var c01 = frame[x0 - 1, y0].Work;
    var c11 = frame[x0, y0].Work;
    var c21 = frame[x0 + 1, y0].Work;
    var c31 = frame[x0 + 2, y0].Work;

    var c02 = frame[x0 - 1, y0 + 1].Work;
    var c12 = frame[x0, y0 + 1].Work;
    var c22 = frame[x0 + 1, y0 + 1].Work;
    var c32 = frame[x0 + 2, y0 + 1].Work;

    var c03 = frame[x0 - 1, y0 + 2].Work;
    var c13 = frame[x0, y0 + 2].Work;
    var c23 = frame[x0 + 1, y0 + 2].Work;
    var c33 = frame[x0 + 2, y0 + 2].Work;

    // Compute luminance for edge detection
    var l11 = ColorConverter.GetLuminance(in c11);
    var l21 = ColorConverter.GetLuminance(in c21);
    var l12 = ColorConverter.GetLuminance(in c12);
    var l22 = ColorConverter.GetLuminance(in c22);

    // Cross gradient (edge detection)
    var lH = l11 - l21;
    var lV = l11 - l12;
    var lD1 = ColorConverter.GetLuminance(in c00) - ColorConverter.GetLuminance(in c22);
    var lD2 = ColorConverter.GetLuminance(in c20) - ColorConverter.GetLuminance(in c02);

    // Compute edge direction
    var lenH = MathF.Abs(lH);
    var lenV = MathF.Abs(lV);
    var lenD1 = MathF.Abs(lD1) * 0.707f; // Diagonal weight factor
    var lenD2 = MathF.Abs(lD2) * 0.707f;

    // Edge-aware weight calculation
    var dirH = lenH / (lenH + lenV + 0.0001f);
    var dirV = lenV / (lenH + lenV + 0.0001f);
    var dirD1 = lenD1 / (lenD1 + lenD2 + 0.0001f);
    var dirD2 = lenD2 / (lenD1 + lenD2 + 0.0001f);

    // Blend factor based on edge strength
    var edgeStrength = MathF.Max(lenH + lenV, lenD1 + lenD2);
    var edgeFactor = MathF.Min(edgeStrength * 4f, 1f);

    // Compute 12-tap EASU weights based on edge direction
    var pp = stackalloc float[12];
    var colors = stackalloc TWork[12];

    // Core 4 samples (bicubic positions)
    colors[0] = c11;
    colors[1] = c21;
    colors[2] = c12;
    colors[3] = c22;

    // Extended 8 samples (edge-aware positions)
    colors[4] = c10;
    colors[5] = c20;
    colors[6] = c01;
    colors[7] = c31;
    colors[8] = c02;
    colors[9] = c32;
    colors[10] = c13;
    colors[11] = c23;

    // Base bilinear weights for core 4
    var w11 = (1f - fx) * (1f - fy);
    var w21 = fx * (1f - fy);
    var w12 = (1f - fx) * fy;
    var w22 = fx * fy;

    // Apply sharpness and edge awareness
    var sharp = this._sharpnessParam;
    pp[0] = w11 * sharp;
    pp[1] = w21 * sharp;
    pp[2] = w12 * sharp;
    pp[3] = w22 * sharp;

    // Edge-directed sample weights
    var edgeWeight = (1f - sharp) * edgeFactor;
    var uniformWeight = (1f - sharp) * (1f - edgeFactor);

    // Vertical edge contribution (horizontal samples more important)
    var hWeight = edgeWeight * dirH;
    pp[4] = hWeight * (1f - fy) * 0.5f;  // c10
    pp[5] = hWeight * (1f - fy) * 0.5f;  // c20
    pp[10] = hWeight * fy * 0.5f;        // c13
    pp[11] = hWeight * fy * 0.5f;        // c23

    // Horizontal edge contribution (vertical samples more important)
    var vWeight = edgeWeight * dirV;
    pp[6] = vWeight * (1f - fx) * 0.5f;  // c01
    pp[7] = vWeight * fx * 0.5f;         // c31
    pp[8] = vWeight * (1f - fx) * 0.5f;  // c02
    pp[9] = vWeight * fx * 0.5f;         // c32

    // Add uniform contribution
    var uniContrib = uniformWeight / 12f;
    for (var i = 0; i < 12; ++i)
      pp[i] += uniContrib;

    // Normalize weights
    var totalWeight = 0f;
    for (var i = 0; i < 12; ++i)
      totalWeight += pp[i];

    var invTotal = 1f / totalWeight;
    for (var i = 0; i < 12; ++i)
      pp[i] *= invTotal;

    // Accumulate weighted result
    Accum4F<TWork> acc = default;
    for (var i = 0; i < 12; ++i)
      if (pp[i] > 0f)
        acc.AddMul(colors[i], pp[i]);

    dest[destY * destStride + destX] = encoder.Encode(acc.Result);
  }

}
