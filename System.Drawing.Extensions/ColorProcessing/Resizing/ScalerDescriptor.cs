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
using System.Linq;
using System.Reflection;

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
  /// Gets the supported scale factors for this scaler.
  /// </summary>
  /// <remarks>
  /// For pixel-art scalers, this contains discrete scale factors like 2x2, 3x3.
  /// For resamplers, this is empty since they support arbitrary target dimensions.
  /// </remarks>
  public ScaleFactor[] SupportedScales { get; }

  /// <summary>
  /// Gets whether this is a pixel-art scaler (implements <see cref="IPixelScaler"/>).
  /// </summary>
  public bool IsPixelScaler { get; }

  /// <summary>
  /// Gets whether this is a resampler (implements <see cref="IResampler"/>).
  /// </summary>
  public bool IsResampler { get; }

  /// <summary>
  /// Gets the kernel radius for resamplers.
  /// </summary>
  /// <remarks>
  /// Only meaningful for resamplers. Returns 0 for pixel-art scalers.
  /// </remarks>
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
    bool isPixelScaler,
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
    this.IsPixelScaler = isPixelScaler;
    this.IsResampler = isResampler;
    this.Radius = radius;
  }

  /// <summary>
  /// Creates a descriptor from a scaler type.
  /// </summary>
  /// <param name="type">The scaler type (must implement <see cref="IPixelScaler"/> or <see cref="IResampler"/>).</param>
  /// <returns>A descriptor, or <c>null</c> if the type doesn't have the <see cref="ScalerInfoAttribute"/>.</returns>
  internal static ScalerDescriptor? FromType(Type type) {
    var attr = type.GetCustomAttribute<ScalerInfoAttribute>();
    if (attr == null)
      return null;

    var isPixelScaler = typeof(IPixelScaler).IsAssignableFrom(type);
    var isResampler = typeof(IResampler).IsAssignableFrom(type);

    if (!isPixelScaler && !isResampler)
      return null;

    // Try to get SupportedScales static property
    var supportedScales = Array.Empty<ScaleFactor>();
    var supportedScalesProp = type.GetProperty("SupportedScales", BindingFlags.Public | BindingFlags.Static);
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
      isPixelScaler,
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

  private static MethodInfo GetUpscaleMethod(Type scalerType) {
    return _upscaleMethodCache.GetOrAdd(scalerType, type => {
      _upscaleGenericDef ??= AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(a => {
          try { return a.GetTypes(); }
          catch { return []; }
        })
        .FirstOrDefault(t => t.FullName == "Hawkynt.Drawing.BitmapScalerExtensions")
        ?.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == "Upscale" && m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType.IsGenericParameter);
      return _upscaleGenericDef!.MakeGenericMethod(type);
    });
  }

  private static MethodInfo GetResampleMethod(Type resamplerType) {
    return _resampleMethodCache.GetOrAdd(resamplerType, type => {
      _resampleGenericDef ??= AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(a => {
          try { return a.GetTypes(); }
          catch { return []; }
        })
        .FirstOrDefault(t => t.FullName == "Hawkynt.Drawing.BitmapScalerExtensions")
        ?.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == "Resample" && m.GetParameters().Length == 4 && m.IsGenericMethod);
      return _resampleGenericDef!.MakeGenericMethod(type);
    });
  }

  /// <summary>
  /// Scales a bitmap using this scaler with default configuration.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <returns>A new scaled bitmap.</returns>
  /// <remarks>
  /// <para>For pixel scalers, applies the scaler's native scale factor.</para>
  /// <para>For resamplers, scales to 2x the original dimensions.</para>
  /// </remarks>
  public Bitmap Scale(Bitmap source) {
    if (this.IsPixelScaler)
      return this.Upscale(source);

    return this.Resample(source, source.Width * 2, source.Height * 2);
  }

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
  public Bitmap Scale(Bitmap source, int targetWidth, int targetHeight) {
    if (this.IsResampler)
      return this.Resample(source, targetWidth, targetHeight);

    // For pixel scalers, use the native scale and let the caller handle any mismatch
    return this.Upscale(source);
  }

  /// <summary>
  /// Upscales a bitmap using this pixel scaler.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="quality">The quality mode for scaling.</param>
  /// <returns>A new upscaled bitmap.</returns>
  /// <exception cref="InvalidOperationException">Thrown if this is not a pixel scaler.</exception>
  public Bitmap Upscale(Bitmap source, ScalerQuality quality = ScalerQuality.Fast) {
    if (!this.IsPixelScaler)
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
    if (!this.IsPixelScaler)
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
    return (Bitmap)method.Invoke(null, [source, resampler, targetWidth, targetHeight])!;
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
    return (Bitmap)method.Invoke(null, [source, resampler, targetWidth, targetHeight])!;
  }

  #endregion
}
