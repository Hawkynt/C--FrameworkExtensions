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

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Categorizes a resizing algorithm by the fidelity axis: how faithfully the output reflects
/// the source, and what scaling grammar applies.
/// </summary>
/// <remarks>
/// <para>
/// The overall activity (changing output dimensions) is called <b>resizing</b>; the verb pair
/// is <b>upsize</b>/<b>downsize</b>. Within resizing, this library distinguishes three kinds of
/// algorithm by their source-fidelity contract:
/// </para>
/// <list type="bullet">
///   <item>
///     <description>
///     <b>Rescaler</b> — verb pair <b>upscale</b>/<b>downscale</b>. Fixed integer scale factor,
///     pattern-matched against source pixel neighbourhoods. Examples: HQ2/3/4x, XBR, XBRz, Eagle,
///     Scale2x (Epx), SuperXbr, NNEDI3. Output pixels are chosen from a pre-tabulated decision
///     tree keyed on the surrounding pixels; no new content is invented.
///     </description>
///   </item>
///   <item>
///     <description>
///     <b>Resampler</b> — verb pair <b>upsample</b>/<b>downsample</b>. Arbitrary target dimensions,
///     source-faithful. Every output pixel is derived from real source samples via deterministic
///     math: kernel convolution (Lanczos, Bicubic, Mitchell-Netravali, B-splines, OMoms, Jinc,
///     Gaussian, …), vector tracing (Kopf-Lischinski), content-aware edge direction (DCCI,
///     EEDI2), or content shuffling (seam carving). No hallucination.
///     </description>
///   </item>
///   <item>
///     <description>
///     <b>Regenerator</b> — no verb pair; always both up AND down. Arbitrary target dimensions,
///     regenerative. The output is synthesised (not interpolated) by a learned model that can
///     invent plausible detail not present in the source — at the cost of hallucinations that
///     may diverge from the source. Examples: AI super-resolution, diffusion-based upscalers.
///     </description>
///   </item>
/// </list>
/// </remarks>
public enum ScalerCategory {

  /// <summary>
  /// Pattern-matched fixed integer scale (upscale/downscale): HQ, XBR, XBRz, Eagle, Scale2x, SuperXbr, NNEDI3, …
  /// </summary>
  Rescaler,

  /// <summary>
  /// Deterministic, source-faithful arbitrary-scale (upsample/downsample): kernel convolution
  /// (Lanczos, Bicubic, Mitchell-Netravali, …), vector tracing (Kopf-Lischinski), edge-directed
  /// interpolation (DCCI, EEDI2), seam carving.
  /// </summary>
  Resampler,

  /// <summary>
  /// Regenerative (learned/neural) arbitrary-scale. Synthesises pixels via a model — may
  /// hallucinate detail that diverges from the source. AI super-resolution, diffusion upscalers.
  /// </summary>
  Regenerator
}

/// <summary>
/// Provides metadata about a scaling algorithm.
/// </summary>
/// <remarks>
/// Apply this attribute to scaler structs to provide display name,
/// author information, and reference URLs.
/// </remarks>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class ScalerInfoAttribute : Attribute {

  /// <summary>
  /// Gets the display name of the scaler.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets or sets the author of the algorithm.
  /// </summary>
  public string? Author { get; init; }

  /// <summary>
  /// Gets or sets the reference URL for the algorithm.
  /// </summary>
  public string? Url { get; init; }

  /// <summary>
  /// Gets or sets the year the algorithm was created.
  /// </summary>
  public int Year { get; init; }

  /// <summary>
  /// Gets or sets a description of the algorithm.
  /// </summary>
  public string? Description { get; init; }

  /// <summary>
  /// Gets or sets the category of the scaler.
  /// </summary>
  public ScalerCategory Category { get; init; } = ScalerCategory.Rescaler;

  /// <summary>
  /// Initializes a new instance of the <see cref="ScalerInfoAttribute"/> class.
  /// </summary>
  /// <param name="name">The display name of the scaler.</param>
  public ScalerInfoAttribute(string name) => this.Name = name;
}
