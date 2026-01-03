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

using System.Linq.Expressions;

namespace System.Dynamic;

/// <summary>
/// Represents a dynamic object that allows runtime binding of operations.
/// </summary>
/// <remarks>
/// Classes that implement <see cref="IDynamicMetaObjectProvider"/> can fully participate in the dynamic
/// runtime binding process. This interface allows implementing types to dispatch operations to a
/// <see cref="DynamicMetaObject"/> which handles the binding logic.
/// </remarks>
public interface IDynamicMetaObjectProvider {

  /// <summary>
  /// Returns the <see cref="DynamicMetaObject"/> responsible for binding operations performed on this object.
  /// </summary>
  /// <param name="parameter">The expression that represents this object during the binding process.</param>
  /// <returns>
  /// The <see cref="DynamicMetaObject"/> that should be used to bind operations on this object.
  /// </returns>
  /// <remarks>
  /// The returned <see cref="DynamicMetaObject"/> is responsible for performing the binding of operations
  /// such as method calls, property access, indexing, and operator invocations on this object.
  /// </remarks>
  DynamicMetaObject GetMetaObject(Expression parameter);

}

#endif
