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

// System.Dynamic was introduced in .NET 4.0
// Only polyfill for net20/net35 where no DLR exists
#if !SUPPORTS_DYNAMIC

namespace System.Dynamic;

/// <summary>
/// Represents the dynamic convert operation at the call site, providing the binding semantics
/// for dynamic type conversion operations.
/// </summary>
public abstract class ConvertBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="ConvertBinder"/> class.
  /// </summary>
  /// <param name="type">The type to convert to.</param>
  /// <param name="explicit">
  /// <see langword="true"/> if this is an explicit conversion (cast);
  /// <see langword="false"/> if it's an implicit conversion.
  /// </param>
  /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
  protected ConvertBinder(Type type, bool @explicit) {
    this.Type = type ?? throw new ArgumentNullException(nameof(type));
    this.Explicit = @explicit;
  }

  /// <summary>
  /// Gets the type to convert to.
  /// </summary>
  public Type Type { get; }

  /// <summary>
  /// Gets a value indicating whether this is an explicit conversion (cast).
  /// </summary>
  /// <value>
  /// <see langword="true"/> if this is an explicit conversion; otherwise, <see langword="false"/> for implicit conversions.
  /// </value>
  public bool Explicit { get; }

  /// <summary>
  /// Gets the return type of the operation.
  /// </summary>
  public sealed override Type ReturnType => this.Type;

  /// <summary>
  /// Performs the binding of the dynamic convert operation.
  /// </summary>
  /// <param name="target">The target of the dynamic convert operation.</param>
  /// <param name="args">An array of arguments (should be empty).</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    return target.BindConvert(this);
  }

  /// <summary>
  /// Provides the implementation for the dynamic convert operation.
  /// </summary>
  /// <param name="target">The target of the dynamic convert operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackConvert(DynamicMetaObject target);

  /// <summary>
  /// Provides the implementation for the dynamic convert operation.
  /// </summary>
  /// <param name="target">The target of the dynamic convert operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion) =>
    this.FallbackConvert(target);

}

#endif
