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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Guard;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Describes a single tunable parameter on a parametric filter / quantizer / ditherer.
/// </summary>
/// <remarks>
/// <para>
/// This is the *metadata* surface that lets a UI auto-generate sliders, drop-downs and
/// number-boxes for parameter-driven algorithm presets. Algorithms stay strongly typed
/// internally — the metadata layer is purely advisory and is consumed via the
/// <c>Parameters</c> property on each descriptor.
/// </para>
/// <para>
/// All values are boxed as <see cref="object"/> so the descriptor can carry mixed types
/// (int, float, bool, enum) in a single uniform list. Consumers should pattern-match on
/// <see cref="Type"/> before unboxing.
/// </para>
/// </remarks>
public sealed class ParameterDescriptor {

  /// <summary>The parameter's identifier — used as the key in the values dictionary.</summary>
  public string Name { get; }

  /// <summary>Optional human-friendly label for UIs. Falls back to <see cref="Name"/>.</summary>
  public string DisplayName { get; }

  /// <summary>The .NET type of the parameter value (e.g. <c>typeof(int)</c>, <c>typeof(float)</c>).</summary>
  public Type Type { get; }

  /// <summary>Default value used when the user supplies no override.</summary>
  public object? DefaultValue { get; }

  /// <summary>
  /// Inclusive lower bound for numeric parameters. <see langword="null"/> if unconstrained.
  /// </summary>
  public object? MinValue { get; }

  /// <summary>
  /// Inclusive upper bound for numeric parameters. <see langword="null"/> if unconstrained.
  /// </summary>
  public object? MaxValue { get; }

  /// <summary>
  /// Allowed discrete values (e.g. enum members or fixed kernel sizes); empty if continuous.
  /// </summary>
  public IReadOnlyList<object> AllowedValues { get; }

  /// <summary>Optional one-line description of the parameter's effect.</summary>
  public string? Description { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
  /// </summary>
  public ParameterDescriptor(
    string name,
    Type type,
    object? defaultValue,
    object? minValue = null,
    object? maxValue = null,
    IList<object>? allowedValues = null,
    string? displayName = null,
    string? description = null) {
    Against.ArgumentIsNullOrEmpty(name);
    Against.ArgumentIsNull(type);

    this.Name = name;
    this.Type = type;
    this.DefaultValue = defaultValue;
    this.MinValue = minValue;
    this.MaxValue = maxValue;
    this.AllowedValues = allowedValues != null
      ? new ReadOnlyList<object>(allowedValues)
      : (IReadOnlyList<object>)new ReadOnlyList<object>(new object[0]);
    this.DisplayName = displayName ?? name;
    this.Description = description;
  }

  /// <summary>Convenience factory for an integer parameter with inclusive range.</summary>
  public static ParameterDescriptor Int(
    string name, int defaultValue, int? min = null, int? max = null, string? description = null)
    => new(name, typeof(int), defaultValue, min, max, description: description);

  /// <summary>Convenience factory for a float parameter with inclusive range.</summary>
  public static ParameterDescriptor Float(
    string name, float defaultValue, float? min = null, float? max = null, string? description = null)
    => new(name, typeof(float), defaultValue, min, max, description: description);

  /// <summary>Convenience factory for a boolean parameter.</summary>
  public static ParameterDescriptor Bool(
    string name, bool defaultValue, string? description = null)
    => new(name, typeof(bool), defaultValue, description: description);

  /// <summary>Convenience factory for a parameter with discrete integer choices.</summary>
  public static ParameterDescriptor Choice(
    string name, int defaultValue, params int[] allowedValues)
    => new(name, typeof(int), defaultValue, allowedValues: allowedValues.Cast<object>().ToList());

  /// <summary>Convenience factory for a parameter with discrete enum choices.</summary>
  public static ParameterDescriptor Enum<TEnum>(string name, TEnum defaultValue, string? description = null)
    where TEnum : struct, System.Enum {
    var values = System.Enum.GetValues(typeof(TEnum)).Cast<object>().ToList();
    return new(name, typeof(TEnum), defaultValue, allowedValues: values, description: description);
  }

  /// <inheritdoc />
  public override string ToString() => $"{this.Name} ({this.Type.Name}, default={this.DefaultValue})";
}
