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

// UnsafeAccessorKind was added in .NET 8.0
#if !SUPPORTS_UNSAFE_ACCESSOR_ATTRIBUTE

namespace System.Runtime.CompilerServices;

/// <summary>
/// Specifies the kind of member being accessed by an unsafe accessor.
/// </summary>
public enum UnsafeAccessorKind {
  /// <summary>
  /// Specifies accessing a constructor.
  /// </summary>
  Constructor,

  /// <summary>
  /// Specifies accessing an instance method.
  /// </summary>
  Method,

  /// <summary>
  /// Specifies accessing a static method.
  /// </summary>
  StaticMethod,

  /// <summary>
  /// Specifies accessing an instance field.
  /// </summary>
  Field,

  /// <summary>
  /// Specifies accessing a static field.
  /// </summary>
  StaticField
}

#endif
