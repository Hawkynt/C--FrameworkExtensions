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

namespace System.Runtime.CompilerServices;

/// <summary>
/// Represents a dynamic call site in the Dynamic Language Runtime.
/// </summary>
/// <remarks>
/// <para>
/// A call site is a location in a program where a dynamic operation occurs.
/// The call site caches the results of dynamic binding operations to improve
/// performance on subsequent calls with the same types.
/// </para>
/// <para>
/// Use the generic <see cref="CallSite{T}"/> class to create call sites with
/// a specific delegate type, or use <see cref="Create"/> to create a call site
/// dynamically.
/// </para>
/// </remarks>
public class CallSite {

  /// <summary>
  /// The binder that performs dynamic binding for this call site.
  /// </summary>
  private readonly CallSiteBinder _binder;

  /// <summary>
  /// Initializes a new instance of the <see cref="CallSite"/> class.
  /// </summary>
  /// <param name="binder">The <see cref="CallSiteBinder"/> responsible for binding operations at this call site.</param>
  internal CallSite(CallSiteBinder binder) => this._binder = binder;

  /// <summary>
  /// Gets the <see cref="CallSiteBinder"/> responsible for binding operations at this call site.
  /// </summary>
  public CallSiteBinder Binder => this._binder;

  /// <summary>
  /// Creates a call site with the specified delegate type and binder.
  /// </summary>
  /// <param name="delegateType">The type of the delegate used by this call site.</param>
  /// <param name="binder">The <see cref="CallSiteBinder"/> responsible for binding operations.</param>
  /// <returns>A new <see cref="CallSite"/> instance.</returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="delegateType"/> or <paramref name="binder"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// <paramref name="delegateType"/> is not a delegate type.
  /// </exception>
  public static CallSite Create(Type delegateType, CallSiteBinder binder) {
    if (delegateType == null)
      throw new ArgumentNullException(nameof(delegateType));
    if (binder == null)
      throw new ArgumentNullException(nameof(binder));
    if (!typeof(Delegate).IsAssignableFrom(delegateType))
      throw new ArgumentException("Type must be a delegate type.", nameof(delegateType));

    // Create a CallSite<T> where T is the delegate type
    var genericType = typeof(CallSite<>).MakeGenericType(delegateType);
    return (CallSite)Activator.CreateInstance(genericType, binder);
  }

}

#endif
