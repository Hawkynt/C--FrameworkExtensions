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

#if !SUPPORTS_MEMBER_NOT_NULL_WHEN_ATTRIBUTE

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that the method or property will ensure that the listed field and property members have not-null values when returning with the specified return value condition.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
public sealed class MemberNotNullWhenAttribute : Attribute {
  /// <summary>
  /// Gets the return value condition.
  /// </summary>
  public bool ReturnValue { get; }

  /// <summary>
  /// Gets field or property member names.
  /// </summary>
  public string[] Members { get; }

  /// <summary>
  /// Initializes the attribute with the specified return value condition and a field or property member.
  /// </summary>
  /// <param name="returnValue">The return value condition. If the method returns this value, the associated parameter will not be null.</param>
  /// <param name="member">The field or property member that is promised to be not-null.</param>
  public MemberNotNullWhenAttribute(bool returnValue, string member) {
    this.ReturnValue = returnValue;
    this.Members = new[] { member };
  }

  /// <summary>
  /// Initializes the attribute with the specified return value condition and list of field and property members.
  /// </summary>
  /// <param name="returnValue">The return value condition. If the method returns this value, the associated parameter will not be null.</param>
  /// <param name="members">The list of field and property members that are promised to be not-null.</param>
  public MemberNotNullWhenAttribute(bool returnValue, params string[] members) {
    this.ReturnValue = returnValue;
    this.Members = members;
  }
}

#endif
