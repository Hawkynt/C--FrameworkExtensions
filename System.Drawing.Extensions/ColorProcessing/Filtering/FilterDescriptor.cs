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
using System.Runtime.InteropServices;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.Drawing;

namespace Hawkynt.ColorProcessing.Filtering;

/// <summary>
/// Describes a pixel filter algorithm with its metadata and capabilities.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="FilterRegistry"/> to enumerate all available filters at runtime.
/// Each descriptor provides metadata from the <see cref="FilterInfoAttribute"/>
/// and can create default instances or apply the filter to bitmaps.
/// </para>
/// </remarks>
public sealed class FilterDescriptor {

  /// <summary>
  /// Gets the concrete type of the filter.
  /// </summary>
  public Type Type { get; }

  /// <summary>
  /// Gets the display name of the filter.
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
  /// Gets the category of the filter.
  /// </summary>
  public FilterCategory Category { get; }

  private FilterDescriptor(
    Type type,
    string name,
    string? author,
    string? description,
    string? url,
    int year,
    FilterCategory category) {
    this.Type = type;
    this.Name = name;
    this.Author = author;
    this.Description = description;
    this.Url = url;
    this.Year = year;
    this.Category = category;
  }

  /// <summary>
  /// Creates a descriptor from a filter type.
  /// </summary>
  /// <param name="type">The filter type (must implement <see cref="IPixelFilter"/>).</param>
  /// <returns>A descriptor, or <c>null</c> if the type doesn't have the <see cref="FilterInfoAttribute"/>.</returns>
  internal static FilterDescriptor? FromType(Type type) {
    var attr = type.GetCustomAttribute<FilterInfoAttribute>();
    if (attr == null)
      return null;

    if (!typeof(IPixelFilter).IsAssignableFrom(type))
      return null;

    return new(
      type,
      attr.Name,
      attr.Author,
      attr.Description,
      attr.Url,
      attr.Year,
      attr.Category
    );
  }

  /// <summary>
  /// Creates a default instance of this filter by invoking the constructor with default parameter values.
  /// </summary>
  /// <returns>A new instance of the filter with default configuration.</returns>
  public IPixelFilter CreateDefault() => (IPixelFilter)_CreateDefault();

  /// <summary>
  /// Creates a default instance of this filter as the specified type.
  /// </summary>
  /// <typeparam name="TFilter">The expected filter type.</typeparam>
  /// <returns>A new instance of the filter with default configuration.</returns>
  /// <exception cref="InvalidCastException">Thrown if the filter is not of type <typeparamref name="TFilter"/>.</exception>
  public TFilter CreateDefault<TFilter>() where TFilter : struct, IPixelFilter
    => (TFilter)_CreateDefault();

  private object _CreateDefault() {
    var parameterlessCtor = this.Type.GetConstructor(Type.EmptyTypes);
    if (parameterlessCtor != null)
      return parameterlessCtor.Invoke(null);

    var ctor = this.Type.GetConstructors().FirstOrDefault();
    if (ctor == null)
      return Activator.CreateInstance(this.Type)!;

    var parameters = ctor.GetParameters();
    var args = new object[parameters.Length];
    for (var i = 0; i < parameters.Length; ++i)
      args[i] = (parameters[i].Attributes & ParameterAttributes.HasDefault) != 0
        ? parameters[i].DefaultValue!
        : parameters[i].ParameterType.IsValueType
          ? Activator.CreateInstance(parameters[i].ParameterType)!
          : null!;

    return ctor.Invoke(args);
  }

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Category})";

  #region Apply Methods

  private static readonly ConcurrentDictionary<Type, MethodInfo> _applyMethodCache = new();
  private static MethodInfo? _applyGenericDef;

  private static MethodInfo GetApplyMethod(Type filterType) => _applyMethodCache.GetOrAdd(filterType, type => {
    _applyGenericDef ??= typeof(BitmapFilterExtensions)
      .GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m => m.Name == nameof(BitmapFilterExtensions.ApplyFilter) && m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType.IsGenericParameter);
    return _applyGenericDef!.MakeGenericMethod(type);
  });

  /// <summary>
  /// Applies this filter to a bitmap using default configuration.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="quality">The quality mode for filtering.</param>
  /// <returns>A new filtered bitmap.</returns>
  public Bitmap Apply(Bitmap source, ScalerQuality quality = ScalerQuality.Fast) {
    var filter = this.CreateDefault();
    var method = GetApplyMethod(this.Type);
    return (Bitmap)method.Invoke(null, [source, filter, quality])!;
  }

  /// <summary>
  /// Applies this filter to a bitmap using a pre-created filter instance.
  /// </summary>
  /// <param name="source">The source bitmap.</param>
  /// <param name="filter">The filter instance (must match this descriptor's type).</param>
  /// <param name="quality">The quality mode for filtering.</param>
  /// <returns>A new filtered bitmap.</returns>
  public Bitmap Apply(Bitmap source, object filter, ScalerQuality quality = ScalerQuality.Fast) {
    var method = GetApplyMethod(this.Type);
    return (Bitmap)method.Invoke(null, [source, filter, quality])!;
  }

  #endregion
}
