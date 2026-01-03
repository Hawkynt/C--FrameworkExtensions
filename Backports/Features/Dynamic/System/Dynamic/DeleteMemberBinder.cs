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
/// Represents the dynamic delete member operation at the call site, providing the binding semantics
/// for dynamic delete member operations.
/// </summary>
public abstract class DeleteMemberBinder : DynamicMetaObjectBinder {

  /// <summary>
  /// Initializes a new instance of the <see cref="DeleteMemberBinder"/> class.
  /// </summary>
  /// <param name="name">The name of the member to delete.</param>
  /// <param name="ignoreCase">
  /// <see langword="true"/> if the name should be matched ignoring case; otherwise, <see langword="false"/>.
  /// </param>
  /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
  protected DeleteMemberBinder(string name, bool ignoreCase) {
    this.Name = name ?? throw new ArgumentNullException(nameof(name));
    this.IgnoreCase = ignoreCase;
  }

  /// <summary>
  /// Gets the name of the member to delete.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// Gets a value indicating whether the member name should be matched ignoring case.
  /// </summary>
  public bool IgnoreCase { get; }

  /// <summary>
  /// Gets the return type of the operation.
  /// </summary>
  public override Type ReturnType => typeof(void);

  /// <summary>
  /// Performs the binding of the dynamic delete member operation.
  /// </summary>
  /// <param name="target">The target of the dynamic delete member operation.</param>
  /// <param name="args">An array of arguments (should be empty).</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args) {
    if (target == null)
      throw new ArgumentNullException(nameof(target));
    return target.BindDeleteMember(this);
  }

  /// <summary>
  /// Provides the implementation for operations that delete a member.
  /// </summary>
  /// <param name="target">The target of the dynamic delete member operation.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  /// <remarks>
  /// This method is called when the target does not have its own implementation for the delete member operation.
  /// </remarks>
  public abstract DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target);

  /// <summary>
  /// Provides the implementation for operations that delete a member.
  /// </summary>
  /// <param name="target">The target of the dynamic delete member operation.</param>
  /// <param name="errorSuggestion">The binding to use if binding fails, or null.</param>
  /// <returns>The <see cref="DynamicMetaObject"/> representing the result of the binding.</returns>
  /// <remarks>
  /// This method is called when the target does not have its own implementation for the delete member operation.
  /// </remarks>
  public virtual DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) =>
    this.FallbackDeleteMember(target);

}

#endif
