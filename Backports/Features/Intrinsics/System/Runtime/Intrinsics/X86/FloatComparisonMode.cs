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

#if !SUPPORTS_INTRINSICS

namespace System.Runtime.Intrinsics.X86;

/// <summary>
/// Specifies the comparison operation for floating-point comparison intrinsics.
/// </summary>
public enum FloatComparisonMode : byte {
  /// <summary>Compares for equality (ordered, non-signaling). Returns true if x == y.</summary>
  OrderedEqualNonSignaling = 0,
  /// <summary>Compares for less than (ordered, signaling). Returns true if x &lt; y.</summary>
  OrderedLessThanSignaling = 1,
  /// <summary>Compares for less than or equal (ordered, signaling). Returns true if x &lt;= y.</summary>
  OrderedLessThanOrEqualSignaling = 2,
  /// <summary>Returns true if either operand is NaN (unordered, non-signaling).</summary>
  UnorderedNonSignaling = 3,
  /// <summary>Compares for not equal (unordered, non-signaling). Returns true if x != y or either is NaN.</summary>
  UnorderedNotEqualNonSignaling = 4,
  /// <summary>Compares for not less than (unordered, signaling). Returns true if !(x &lt; y).</summary>
  UnorderedNotLessThanSignaling = 5,
  /// <summary>Compares for not less than or equal (unordered, signaling). Returns true if !(x &lt;= y).</summary>
  UnorderedNotLessThanOrEqualSignaling = 6,
  /// <summary>Returns true if both operands are not NaN (ordered, non-signaling).</summary>
  OrderedNonSignaling = 7,
  /// <summary>Compares for equality (unordered, non-signaling). Returns true if x == y or either is NaN.</summary>
  UnorderedEqualNonSignaling = 8,
  /// <summary>Compares for not greater than or equal (unordered, signaling). Returns true if !(x &gt;= y).</summary>
  UnorderedNotGreaterThanOrEqualSignaling = 9,
  /// <summary>Compares for not greater than (unordered, signaling). Returns true if !(x &gt; y).</summary>
  UnorderedNotGreaterThanSignaling = 10,
  /// <summary>Returns false unconditionally.</summary>
  FalseNonSignaling = 11,
  /// <summary>Compares for not equal (ordered, non-signaling). Returns true if x != y and neither is NaN.</summary>
  OrderedNotEqualNonSignaling = 12,
  /// <summary>Compares for greater than or equal (ordered, signaling). Returns true if x &gt;= y.</summary>
  OrderedGreaterThanOrEqualSignaling = 13,
  /// <summary>Compares for greater than (ordered, signaling). Returns true if x &gt; y.</summary>
  OrderedGreaterThanSignaling = 14,
  /// <summary>Returns true unconditionally.</summary>
  TrueNonSignaling = 15,
  /// <summary>Compares for equality (ordered, signaling).</summary>
  OrderedEqualSignaling = 16,
  /// <summary>Compares for less than (ordered, non-signaling).</summary>
  OrderedLessThanNonSignaling = 17,
  /// <summary>Compares for less than or equal (ordered, non-signaling).</summary>
  OrderedLessThanOrEqualNonSignaling = 18,
  /// <summary>Returns true if either operand is NaN (unordered, signaling).</summary>
  UnorderedSignaling = 19,
  /// <summary>Compares for not equal (unordered, signaling).</summary>
  UnorderedNotEqualSignaling = 20,
  /// <summary>Compares for not less than (unordered, non-signaling).</summary>
  UnorderedNotLessThanNonSignaling = 21,
  /// <summary>Compares for not less than or equal (unordered, non-signaling).</summary>
  UnorderedNotLessThanOrEqualNonSignaling = 22,
  /// <summary>Returns true if both operands are not NaN (ordered, signaling).</summary>
  OrderedSignaling = 23,
  /// <summary>Compares for equality (unordered, signaling).</summary>
  UnorderedEqualSignaling = 24,
  /// <summary>Compares for not greater than or equal (unordered, non-signaling).</summary>
  UnorderedNotGreaterThanOrEqualNonSignaling = 25,
  /// <summary>Compares for not greater than (unordered, non-signaling).</summary>
  UnorderedNotGreaterThanNonSignaling = 26,
  /// <summary>Returns false unconditionally (signaling).</summary>
  FalseSignaling = 27,
  /// <summary>Compares for not equal (ordered, signaling).</summary>
  OrderedNotEqualSignaling = 28,
  /// <summary>Compares for greater than or equal (ordered, non-signaling).</summary>
  OrderedGreaterThanOrEqualNonSignaling = 29,
  /// <summary>Compares for greater than (ordered, non-signaling).</summary>
  OrderedGreaterThanNonSignaling = 30,
  /// <summary>Returns true unconditionally (signaling).</summary>
  TrueSignaling = 31
}

#endif
