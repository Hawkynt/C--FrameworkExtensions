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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Guard;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Process-wide registry that maps an algorithm key (a stable string id, usually
/// "Type.Variant") to the list of <see cref="ParameterDescriptor"/>s describing its
/// tunable parameters and to a builder delegate that materializes an instance from a
/// dictionary of parameter values.
/// </summary>
/// <remarks>
/// <para>
/// Algorithms register their parameter surface eagerly from a static constructor so the
/// metadata is visible to <see cref="Filtering.FilterDescriptor"/>,
/// <see cref="Dithering.DithererDescriptor"/> and
/// <see cref="Quantization.QuantizerDescriptor"/> via lookups. This avoids touching the
/// source generator while still giving descriptors a non-empty Parameters list when the
/// algorithm opts in.
/// </para>
/// <para>
/// Looking up an unknown key returns an empty list — fully backward-compatible: every
/// existing algorithm that does not register parameter metadata reports zero parameters
/// and behaves exactly as before.
/// </para>
/// </remarks>
public static class ParameterMetadata {

  private static readonly ConcurrentDictionary<string, Entry> _Entries = new(StringComparer.Ordinal);

  /// <summary>
  /// Snapshot of a single registered algorithm's parameter surface.
  /// </summary>
  public sealed class Entry {
    public IReadOnlyList<ParameterDescriptor> Parameters { get; }
    public Func<IReadOnlyDictionary<string, object?>, object> Builder { get; }
    public Entry(IReadOnlyList<ParameterDescriptor> parameters, Func<IReadOnlyDictionary<string, object?>, object> builder) {
      this.Parameters = parameters;
      this.Builder = builder;
    }
  }

  /// <summary>
  /// Registers parameter metadata for a given algorithm key. Subsequent registrations for
  /// the same key are ignored — first registration wins, so static-constructor ordering
  /// does not matter.
  /// </summary>
  /// <param name="key">Stable algorithm id (typically a static name on the type).</param>
  /// <param name="parameters">Descriptors for each tunable parameter.</param>
  /// <param name="builder">Materializes an instance from a dictionary of parameter values.</param>
  public static void Register(
    string key,
    IList<ParameterDescriptor> parameters,
    Func<IReadOnlyDictionary<string, object?>, object> builder) {
    Against.ArgumentIsNullOrEmpty(key);
    Against.ArgumentIsNull(parameters);
    Against.ArgumentIsNull(builder);

    _Entries.TryAdd(key, new Entry(new ReadOnlyList<ParameterDescriptor>(parameters), builder));
  }

  /// <summary>
  /// Looks up the parameter descriptors for the given key, returning an empty list when
  /// the key is unknown.
  /// </summary>
  private static readonly IReadOnlyList<ParameterDescriptor> _Empty
    = new ReadOnlyList<ParameterDescriptor>(new ParameterDescriptor[0]);

  public static IReadOnlyList<ParameterDescriptor> GetParameters(string key)
    => _Entries.TryGetValue(key, out var entry) ? entry.Parameters : _Empty;

  /// <summary>
  /// Looks up the registered builder for the given key, returning <see langword="null"/>
  /// when the key is unknown.
  /// </summary>
  public static Func<IReadOnlyDictionary<string, object?>, object>? GetBuilder(string key)
    => _Entries.TryGetValue(key, out var entry) ? entry.Builder : null;

  /// <summary>
  /// Helper that pulls a parameter value from a user-supplied dictionary, falling back to
  /// the descriptor default when the user did not specify it. Performs a lightweight
  /// coercion for the common int/float promotions that occur when values come from a UI
  /// or JSON binding layer.
  /// </summary>
  public static T Get<T>(IReadOnlyDictionary<string, object?> values, ParameterDescriptor descriptor) {
    var raw = values != null && values.TryGetValue(descriptor.Name, out var v) ? v : descriptor.DefaultValue;
    if (raw is T t)
      return t;

    // Light coercion path — avoids forcing callers to box int as float etc.
    if (raw == null)
      return default!;

    var targetType = typeof(T);
    if (targetType == typeof(float) && raw is int i)
      return (T)(object)(float)i;
    if (targetType == typeof(double) && raw is int i2)
      return (T)(object)(double)i2;
    if (targetType == typeof(int) && raw is long l)
      return (T)(object)(int)l;
    if (targetType.IsEnum && raw is int ie)
      return (T)System.Enum.ToObject(targetType, ie);

    try {
      return (T)Convert.ChangeType(raw, targetType, System.Globalization.CultureInfo.InvariantCulture);
    } catch {
      return (T)descriptor.DefaultValue!;
    }
  }
}
