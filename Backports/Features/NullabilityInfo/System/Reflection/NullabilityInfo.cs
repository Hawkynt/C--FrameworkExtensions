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

// NullabilityInfo was added in .NET 6.0
#if !SUPPORTS_NULLABILITY_INFO

namespace System.Reflection;

/// <summary>
/// Represents the nullability information about a member.
/// </summary>
public sealed class NullabilityInfo {

  internal NullabilityInfo(Type type, NullabilityState readState, NullabilityState writeState, NullabilityInfo? elementType, NullabilityInfo[] genericTypeArguments) {
    this.Type = type;
    this.ReadState = readState;
    this.WriteState = writeState;
    this.ElementType = elementType;
    this.GenericTypeArguments = genericTypeArguments;
  }

  /// <summary>
  /// Gets the type of the member or parameter being described.
  /// </summary>
  public Type Type { get; }

  /// <summary>
  /// Gets the nullability read state of the member.
  /// </summary>
  public NullabilityState ReadState { get; internal set; }

  /// <summary>
  /// Gets the nullability write state of the member.
  /// </summary>
  public NullabilityState WriteState { get; internal set; }

  /// <summary>
  /// Gets the nullability info for the element type if the member type is an array or nullable type.
  /// </summary>
  public NullabilityInfo? ElementType { get; }

  /// <summary>
  /// Gets the nullability info for each of the generic type arguments if the member type is a generic type.
  /// </summary>
  public NullabilityInfo[] GenericTypeArguments { get; }

}

#endif
