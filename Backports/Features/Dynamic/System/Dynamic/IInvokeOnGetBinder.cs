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
/// Represents a binder that, when used to bind a get member operation,
/// will invoke the target if the result is a callable delegate or callable member.
/// </summary>
/// <remarks>
/// This interface is typically implemented by <see cref="GetMemberBinder"/>
/// when the language semantics require that getting a member automatically
/// invokes it if it is callable (like in some scripting languages).
/// </remarks>
public interface IInvokeOnGetBinder {

  /// <summary>
  /// Gets a value indicating whether the result of the get member operation
  /// should be invoked if it is callable.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if the result should be invoked;
  /// otherwise, <see langword="false"/>.
  /// </value>
  bool InvokeOnGet { get; }

}

#endif
