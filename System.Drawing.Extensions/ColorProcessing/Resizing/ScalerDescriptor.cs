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
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Extensions.ColorProcessing.Resizing;
using System.Linq;
using System.Reflection;
using Hawkynt.ColorProcessing.Resizing.Rescalers;
using Hawkynt.Drawing;

namespace Hawkynt.ColorProcessing.Resizing;

/// <summary>
/// Describes a scaling algorithm with its metadata and capabilities.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="ScalerRegistry"/> to enumerate all available scalers at runtime.
/// Each descriptor provides metadata from the <see cref="ScalerInfoAttribute"/>
/// and can create default instances of the scaler.
/// </para>
/// </remarks>
public sealed class ScalerDescriptor {

  /// <summary>
  /// Compile-time factory used by the source generator. Exposed as <c>internal</c> so only the
  /// generated registry partials in this assembly may construct descriptors directly; callers
  /// should still use <see cref="ScalerRegistry"/> for discovery. The underscored prefix flags
  /// it as an implementation detail that must not be called by normal user code.
  /// </summary>
  internal static ScalerDescriptor __CreateFromGenerator(
    Type type,
    string name,
    string? author,
    string? description,
    string? url,
    int year,
    ScalerCategory category,
    ScaleFactor[] supportedScales,
    bool isRescaler,
    bool isResampler,
    int radius)
    => new(type, name, author, description, url, year, category, supportedScales, isRescaler, isResampler, radius);


  /// <summary>
  /// Gets the concrete type of the scaler.
  /// </summary>
  public Type Type { get; }

  /// <summary>
  /// Gets the display name of the scaler.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets the author of the algorithm.
  /// </summary>
  public string? Author { get; }

  /// <summary>
  /// Gets a description of the algorithm.
  /// </summary>
  public string? Description { get; }

  /// <summary>
  /// Gets the reference URL for the algorithm.
  /// </summary>
  public string? Url { get; }

  /// <summary>
  /// Gets the year the algorithm was created.
  /// </summary>
  public int Year { get; }

  /// <summary>
  /// Gets the category of the scaler.
  /// </summary>
  public ScalerCategory Category { get; }

  /// <summary>
  /// Supported scale factors. For rescalers (fixed-scale pattern-based), lists the discrete
  /// factors the algorithm was designed for (2x2, 3x3, 2x3, …). For resamplers (arbitrary-scale
  /// kernel/vector/content-aware), empty because any target dimensions are supported.
  /// </summary>
  public ScaleFactor[] SupportedScales { get; }

  /// <summary>True when this algorithm is a <see cref="IRescaler"/> (fixed integer scale, pattern-based).</summary>
  public bool IsRescaler { get; }

  /// <summary>True when this algorithm is a <see cref="IResampler"/> (arbitrary scale, source-faithful math).</summary>
  public bool IsResampler { get; }

  /// <summary>
  /// Kernel support radius (only meaningful for resamplers). Zero for rescalers.
  /// </summary>
  public int Radius { get; }

  private ScalerDescriptor(
    Type type,
    string name,
    string? author,
    string? description,
    string? url,
    int year,
    ScalerCategory category,
    ScaleFactor[] supportedScales,
    bool isRescaler,
    bool isResampler,
    int radius) {
    this.Type = type;
    this.Name = name;
    this.Author = author;
    this.Description = description;
    this.Url = url;
    this.Year = year;
    this.Category = category;
    this.SupportedScales = supportedScales;
    this.IsRescaler = isRescaler;
    this.IsResampler = isResampler;
    this.Radius = radius;
  }

  /// <summary>
  /// Creates a descriptor from a scaler type.
  /// </summary>
  /// <param name="type">The scaler type (must implement <see cref="IRescaler"/> or <see cref="IResampler"/>).</param>
  /// <returns>A descriptor, or <c>null</c> if the type doesn't have the <see cref="ScalerInfoAttribute"/>.</returns>
  internal static ScalerDescriptor? FromType(Type type) {
    var attr = type.GetCustomAttribute<ScalerInfoAttribute>();
    if (attr == null)
      return null;

    var isRescaler = typeof(IRescaler).IsAssignableFrom(type);
    var isResampler = typeof(IResampler).IsAssignableFrom(type);

    if (!isRescaler && !isResampler)
      return null;

    // Try to get SupportedScales static property
    var supportedScales = Array.Empty<ScaleFactor>();
    var supportedScalesProp = type.GetProperty(nameof(HawkyntTv.SupportedScales), BindingFlags.Public | BindingFlags.Static);
    if (supportedScalesProp?.GetValue(null!) is ScaleFactor[] scales)
      supportedScales = scales;

    // Try to get Radius from a default instance for resamplers
    var radius = 0;
    if (isResampler) {
      try {
        var instance = Activator.CreateInstance(type);
        if (instance is IResampler resampler)
          radius = resampler.Radius;
      } catch {
        // Ignore - radius stays 0
      }
    }

    return new(
      type,
      attr.Name,
      attr.Author,
      attr.Description,
      attr.Url,
      attr.Year,
      attr.Category,
      supportedScales,
      isRescaler,
      isResampler,
      radius
    );
  }

  /// <summary>
  /// Creates a default instance of this scaler.
  /// </summary>
  /// <returns>A new instance of the scaler with default configuration.</returns>
  /// <remarks>
  /// <para>
  /// The returned instance is a boxed struct cast to <see cref="IScalerInfo"/>.
  /// For strongly-typed usage, cast to the concrete type.
  /// </para>
  /// </remarks>
  public IScalerInfo CreateDefault() => (IScalerInfo)Activator.CreateInstance(this.Type)!;

  /// <summary>
  /// Creates a default instance of this scaler as the specified type.
  /// </summary>
  /// <typeparam name="TScaler">The expected scaler type.</typeparam>
  /// <returns>A new instance of the scaler with default configuration.</returns>
  /// <exception cref="InvalidCastException">Thrown if the scaler is not of type <typeparamref name="TScaler"/>.</exception>
  public TScaler CreateDefault<TScaler>() where TScaler : struct, IScalerInfo
    => (TScaler)Activator.CreateInstance(this.Type)!;

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Category})";

  #region Scaling Methods

  // Cached generic method definitions for performance
  private static readonly ConcurrentDictionary<Type, MethodInfo> _upscaleMethodCache = new();
  private static readonly ConcurrentDictionary<Type, MethodInfo> _resampleMethodCache = new();
  private static MethodInfo? _upscaleGenericDef;
  private static MethodInfo? _resampleGenericDef;

  private static MethodInfo GetUpscaleMethod(Type scalerType) => _upscaleMethodCache.GetOrAdd(scalerType, type => {
    _upscaleGenericDef ??= typeof(BitmapScalerExtensions)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m => m.Name == nameof(BitmapScalerExtensions.Upscale) && m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType.IsGenericParameter)
      ;
    return _upscaleGenericDef!.MakeGenericMethod(type);
  });

  private static MethodInfo GetResampleMethod(Type resamplerType) => _resampleMethodCache.GetOrAdd(resamplerType, type => {
    _resampleGenericDef ??= typeof(BitmapScalerExtensions)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m => m.Name == nameof(BitmapScalerExtensions.Resample) && m.GetParameters().Length == 5 && m.IsGenericMethod && m.GetParameters()[1].ParameterType.IsGenericParameter)
      ;
    return _resampleGenericDef!.MakeGenericMethod(type);
  });

  // Cached lookup for the OOB/canvas/centred-grid overload (9 parameters incl. the tag).
  private static readonly ConcurrentDictionary<Type, MethodInfo> _resampleOobMethodCache = new();
  private static MethodInfo? _resampleOobGenericDef;
  private static MethodInfo GetResampleOobMethod(Type resamplerType) => _resampleOobMethodCache.GetOrAdd(resamplerType, type => {
    _resampleOobGenericDef ??= typeof(BitmapScalerExtensions)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m => m.Name == nameof(BitmapScalerExtensions.Resample)
                  && m.IsGenericMethod
                  && m.GetParameters().Length == 9
                  && m.GetParameters()[1].ParameterType.IsGenericParameter)
      ;
    return _resampleOobGenericDef!.MakeGenericMethod(type);
  });

  /// <summary>
  /// Scales a bitmap using this scaler with default configuration.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <returns>A new scaled bitmap.</returns>
  /// <remarks>
  /// <para>For pixel scalers, applies the scaler's native scale factor.</para>
  /// <para>For resamplers, scales to 2x the original dimensions.</para>
  /// </remarks>
  public Bitmap Scale(Bitmap source) => this.IsRescaler 
    ? this.Upscale(source) 
    : this.Resample(source, source.Width * 2, source.Height * 2)
    ;

  /// <summary>
  /// Scales a bitmap to the specified dimensions.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new scaled bitmap.</returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown if attempting to resample with a pixel scaler or vice versa when dimensions don't match.
  /// </exception>
  public Bitmap Scale(Bitmap source, int targetWidth, int targetHeight) => this.IsResampler 
    ? this.Resample(source, targetWidth, targetHeight)
    // For pixel scalers, use the native scale and let the caller handle any mismatch
    : this.Upscale(source)
    ;
  
  /// <summary>
  /// Upscales a bitmap using this pixel scaler.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="quality">The quality mode for scaling.</param>
  /// <returns>A new upscaled bitmap.</returns>
  /// <exception cref="InvalidOperationException">Thrown if this is not a pixel scaler.</exception>
  public Bitmap Upscale(Bitmap source, ScalerQuality quality = ScalerQuality.Fast) {
    if (!this.IsRescaler)
      throw new InvalidOperationException($"{this.Name} is not a pixel scaler. Use Resample() instead.");

    var scaler = this.CreateDefault();
    var method = GetUpscaleMethod(this.Type);
    return (Bitmap)method.Invoke(null, [source, scaler, quality])!;
  }

  /// <summary>
  /// Upscales a bitmap using a pre-created scaler instance.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="scaler">The scaler instance (must match this descriptor's type).</param>
  /// <param name="quality">The quality mode for scaling.</param>
  /// <returns>A new upscaled bitmap.</returns>
  /// <exception cref="InvalidOperationException">Thrown if this is not a pixel scaler.</exception>
  public Bitmap Upscale(Bitmap source, object scaler, ScalerQuality quality = ScalerQuality.Fast) {
    if (!this.IsRescaler)
      throw new InvalidOperationException($"{this.Name} is not a pixel scaler. Use Resample() instead.");

    var method = GetUpscaleMethod(this.Type);
    return (Bitmap)method.Invoke(null, [source, scaler, quality])!;
  }

  /// <summary>
  /// Resamples a bitmap to the specified dimensions.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new resampled bitmap.</returns>
  /// <exception cref="InvalidOperationException">Thrown if this is not a resampler.</exception>
  public Bitmap Resample(Bitmap source, int targetWidth, int targetHeight) {
    if (!this.IsResampler)
      throw new InvalidOperationException($"{this.Name} is not a resampler. Use Upscale() instead.");

    var resampler = this.CreateDefault();
    var method = GetResampleMethod(this.Type);
    var tag = _CreateResamplerTag(this.Type);
    return (Bitmap)method.Invoke(null, [source, resampler, targetWidth, targetHeight, tag])!;
  }

  /// <summary>
  /// Resamples a bitmap using a pre-created resampler instance.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="resampler">The resampler instance (must match this descriptor's type).</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <returns>A new resampled bitmap.</returns>
  /// <exception cref="InvalidOperationException">Thrown if this is not a resampler.</exception>
  public Bitmap Resample(Bitmap source, object resampler, int targetWidth, int targetHeight) {
    if (!this.IsResampler)
      throw new InvalidOperationException($"{this.Name} is not a resampler. Use Upscale() instead.");

    var method = GetResampleMethod(this.Type);
    var tag = _CreateResamplerTag(this.Type);
    return (Bitmap)method.Invoke(null, [source, resampler, targetWidth, targetHeight, tag])!;
  }

  /// <summary>
  /// Resamples a bitmap with full control over out-of-bounds handling, canvas fill colour and grid centring.
  /// </summary>
  /// <param name="source">Source bitmap.</param>
  /// <param name="targetWidth">Target width.</param>
  /// <param name="targetHeight">Target height.</param>
  /// <param name="horizontalMode">Behaviour for samples that fall outside the source's horizontal range.</param>
  /// <param name="verticalMode">Behaviour for samples that fall outside the source's vertical range.</param>
  /// <param name="canvasColor">Fill colour used when either axis is in <see cref="OutOfBoundsMode.FlatColor"/> mode (the "canvas" painted around the source image).</param>
  /// <param name="useCenteredGrid">If <c>true</c> (default), pixel centres are aligned when mapping destination→source; if <c>false</c>, top-left corners are.</param>
  /// <exception cref="InvalidOperationException">Thrown if this descriptor is not a resampler.</exception>
  public Bitmap Resample(
    Bitmap source,
    int targetWidth,
    int targetHeight,
    OutOfBoundsMode horizontalMode,
    OutOfBoundsMode verticalMode,
    Color canvasColor,
    bool useCenteredGrid) {
    if (!this.IsResampler)
      throw new InvalidOperationException($"{this.Name} is not a resampler. Use Upscale() instead.");

    var resampler = this.CreateDefault();
    var method = GetResampleOobMethod(this.Type);
    var tag = _CreateResamplerTag(this.Type);
    return (Bitmap)method.Invoke(null, [source, resampler, targetWidth, targetHeight, horizontalMode, verticalMode, canvasColor, useCenteredGrid, tag])!;
  }

  private static readonly ConcurrentDictionary<Type, object> _resamplerTagCache = new();

  private static object _CreateResamplerTag(Type resamplerType) => _resamplerTagCache.GetOrAdd(resamplerType, type => {
    var tagTypeDef = typeof(__ResamplerTag<>);
    var tagType = tagTypeDef.MakeGenericType(type);
    return Activator.CreateInstance(tagType)!;
  });

  #endregion
}
