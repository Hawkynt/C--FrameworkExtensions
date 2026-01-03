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
/// Represents the dynamic set index operation at the call site, providing the binding semantics
/// for dynamic indexer set operations.
/// </summary>
public abstract class SetIndexBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="SetIndexBinder"/> class.
  /// </summary>
  /// <param name="callInfo">The <see cref="CallInfo"/> that describes the signature of the indexer.</param>
  /// <exception cref="ArgumentNullException"><paramref name="callInfo"/> is null.</exception>
  protected SetIndexBinder(CallInfo callInfo) =>
    this.CallInfo = callInfo ?? throw new ArgumentNullException(nameof(callInfo));

  /// <summary>
  /// Gets the <see cref="CallInfo"/> that describes the signature of the indexer.
  /// </summary>
  public CallInfo CallInfo { get; }

  /// <summary>
  /// Performs the binding of the dynamic set index operation.
  /// </summary>
  /// <param name="target">The target of the dynamic set index operation.</param>
  /// <param name="args">An array of arguments. The last element is the value to set; the rest are indexes.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    if (args == null || args.Length < 1)
      throw new ArgumentException("SetIndexBinder requires at least one argument (the value).", nameof(args));

    // The last argument is the value, the rest are indexes
    var value = args[args.Length - 1];
    var indexes = new DynamicMetaObject[args.Length - 1];
    Array.Copy(args, 0, indexes, 0, indexes.Length);

    return target.BindSetIndex(this, indexes, value);
  }

  /// <summary>
  /// Provides the implementation for operations that set a value by index.
  /// </summary>
  /// <param name="target">The target of the dynamic set index operation.</param>
  /// <param name="indexes">The indexes used in the operation.</param>
  /// <param name="value">The value to set.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public abstract DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value);

  /// <summary>
  /// Provides the implementation for operations that set a value by index.
  /// </summary>
  /// <param name="target">The target of the dynamic set index operation.</param>
  /// <param name="indexes">The indexes used in the operation.</param>
  /// <param name="value">The value to set.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public virtual DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion) =>
    this.FallbackSetIndex(target, indexes, value);

}

#endif
