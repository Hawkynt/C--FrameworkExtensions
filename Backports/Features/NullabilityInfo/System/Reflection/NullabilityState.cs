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

// NullabilityState was added in .NET 6.0
#if !SUPPORTS_NULLABILITY_INFO

namespace System.Reflection;

/// <summary>
/// Describes the nullability of a member.
/// </summary>
public enum NullabilityState {
  /// <summary>
  /// Nullability context is not enabled (oblivious).
  /// </summary>
  Unknown = 0,

  /// <summary>
  /// Type is a non-nullable reference type.
  /// </summary>
  NotNull = 1,

  /// <summary>
  /// Type is a nullable reference type.
  /// </summary>
  Nullable = 2
}

#endif
