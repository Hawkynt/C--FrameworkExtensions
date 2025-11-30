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

#if !SUPPORTS_VALUE_TUPLE && !OFFICIAL_VALUE_TUPLE

using Guard;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

/// <summary>
///   The ValueTuple types (from arity 0 to 8) comprise the runtime implementation that underlies tuples in C# and struct
///   tuples in F#.
///   Aside from created via language syntax, they are most easily created via the ValueTuple.Create factory methods.
///   The System.ValueTuple types differ from the System.Tuple types in that:
///   - they are structs rather than classes,
///   - they are mutable rather than readonly, and
///   - their members (such as Item1, Item2, etc) are fields rather than properties.
/// </summary>
public struct ValueTuple : IEquatable<ValueTuple>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple>, IValueTupleInternal {
  int IValueTupleInternal.Size => 0;

  /// <summary>Creates a new struct 0-tuple.</summary>
  /// <returns>A 0-tuple.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple Create() => default;

  /// <summary>Creates a new struct 1-tuple, or singleton.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <returns>A 1-tuple (singleton) whose value is (item1).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1> Create<T1>(T1 item1) => new(item1);

  /// <summary>Creates a new struct 2-tuple, or pair.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <returns>A 2-tuple (pair) whose value is (item1, item2).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new(item1, item2);

  /// <summary>Creates a new struct 3-tuple, or triple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <returns>A 3-tuple (triple) whose value is (item1, item2, item3).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new(item1, item2, item3);

  /// <summary>Creates a new struct 4-tuple, or quadruple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <param name="item4">The value of the fourth component of the tuple.</param>
  /// <returns>A 4-tuple (quadruple) whose value is (item1, item2, item3, item4).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new(item1, item2, item3, item4);

  /// <summary>Creates a new struct 5-tuple, or quintuple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
  /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <param name="item4">The value of the fourth component of the tuple.</param>
  /// <param name="item5">The value of the fifth component of the tuple.</param>
  /// <returns>A 5-tuple (quintuple) whose value is (item1, item2, item3, item4, item5).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3, T4, T5> Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) => new(item1, item2, item3, item4, item5);

  /// <summary>Creates a new struct 6-tuple, or sextuple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
  /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
  /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <param name="item4">The value of the fourth component of the tuple.</param>
  /// <param name="item5">The value of the fifth component of the tuple.</param>
  /// <param name="item6">The value of the sixth component of the tuple.</param>
  /// <returns>A 6-tuple (sextuple) whose value is (item1, item2, item3, item4, item5, item6).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) => new(item1, item2, item3, item4, item5, item6);

  /// <summary>Creates a new struct 7-tuple, or septuple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
  /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
  /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
  /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <param name="item4">The value of the fourth component of the tuple.</param>
  /// <param name="item5">The value of the fifth component of the tuple.</param>
  /// <param name="item6">The value of the sixth component of the tuple.</param>
  /// <param name="item7">The value of the seventh component of the tuple.</param>
  /// <returns>A 7-tuple (septuple) whose value is (item1, item2, item3, item4, item5, item6, item7).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) => new(item1, item2, item3, item4, item5, item6, item7);

  /// <summary>Creates a new struct 8-tuple, or octuple.</summary>
  /// <typeparam name="T1">The type of the first component of the tuple.</typeparam>
  /// <typeparam name="T2">The type of the second component of the tuple.</typeparam>
  /// <typeparam name="T3">The type of the third component of the tuple.</typeparam>
  /// <typeparam name="T4">The type of the fourth component of the tuple.</typeparam>
  /// <typeparam name="T5">The type of the fifth component of the tuple.</typeparam>
  /// <typeparam name="T6">The type of the sixth component of the tuple.</typeparam>
  /// <typeparam name="T7">The type of the seventh component of the tuple.</typeparam>
  /// <typeparam name="T8">The type of the eighth component of the tuple.</typeparam>
  /// <param name="item1">The value of the first component of the tuple.</param>
  /// <param name="item2">The value of the second component of the tuple.</param>
  /// <param name="item3">The value of the third component of the tuple.</param>
  /// <param name="item4">The value of the fourth component of the tuple.</param>
  /// <param name="item5">The value of the fifth component of the tuple.</param>
  /// <param name="item6">The value of the sixth component of the tuple.</param>
  /// <param name="item7">The value of the seventh component of the tuple.</param>
  /// <param name="item8">The value of the eighth component of the tuple.</param>
  /// <returns>An 8-tuple (octuple) whose value is (item1, item2, item3, item4, item5, item6, item7, item8).</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static ValueTuple<T1, T2, T3, T4, T5, T6, T7, ValueTuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8) => new(item1, item2, item3, item4, item5, item6, item7, Create(item8));

  /// <inheritdoc />
  /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
  /// <param name="other">An instance to compare.</param>
  /// <returns>
  ///   A signed number indicating the relative values of this instance and <paramref name="other" />.
  ///   Returns less than zero if this instance is less than <paramref name="other" />, zero if this
  ///   instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
  ///   than <paramref name="other" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(ValueTuple other) => 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) {
    switch (obj) {
      case null: return 1;
      case ValueTuple: return 0;
      default:
        AlwaysThrow.ArgumentException(nameof(obj), "The parameter should be a ValueTuple type of appropriate arity.");
        return 0;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null: return 1;
      case ValueTuple: return 0;
      default:
        AlwaysThrow.ArgumentException(nameof(other),"The parameter should be a ValueTuple type of appropriate arity.");
        return 0;
    }
  }

  /// <summary>
  ///   Returns a value that indicates whether the current <see cref="ValueTuple" /> instance is equal to a specified object.
  /// </summary>
  /// <param name="obj">The object to compare with this instance.</param>
  /// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="ValueTuple" />.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is ValueTuple;

  /// <inheritdoc />
  /// <summary>Returns a value indicating whether this instance is equal to a specified value.</summary>
  /// <param name="other">An instance to compare to this instance.</param>
  /// <returns>true if <paramref name="other" /> has the same value as this instance; otherwise, false.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ValueTuple other) => true;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is ValueTuple;

  /// <summary>Returns the hash code for this instance.</summary>
  /// <returns>A 32-bit signed integer hash code.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => 0;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IValueTupleInternal.GetHashCode(IEqualityComparer comparer) => 0;

  /// <summary>
  ///   Returns a string that represents the value of this <see cref="ValueTuple" /> instance.
  /// </summary>
  /// <returns>The string representation of this <see cref="ValueTuple" /> instance.</returns>
  /// <remarks>
  ///   The string returned by this method takes the form <c>()</c>.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => "()";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  string IValueTupleInternal.ToStringEnd() => ")";

}

/// <summary>
///   Represents an 8-tuple, or octuple, as a value type.
/// </summary>
/// <typeparam name="T1">The type of the tuple's first component.</typeparam>
/// <typeparam name="T2">The type of the tuple's second component.</typeparam>
/// <typeparam name="T3">The type of the tuple's third component.</typeparam>
/// <typeparam name="T4">The type of the tuple's fourth component.</typeparam>
/// <typeparam name="T5">The type of the tuple's fifth component.</typeparam>
/// <typeparam name="T6">The type of the tuple's sixth component.</typeparam>
/// <typeparam name="T7">The type of the tuple's seventh component.</typeparam>
/// <typeparam name="TRest">The type of the tuple's eighth component.</typeparam>
[StructLayout(LayoutKind.Auto)]
public struct ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IEquatable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>>, IValueTupleInternal
  where TRest : struct {
  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's first component.
  /// </summary>
  public T1 Item1;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's second component.
  /// </summary>
  public T2 Item2;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's third component.
  /// </summary>
  public T3 Item3;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's fourth component.
  /// </summary>
  public T4 Item4;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's fifth component.
  /// </summary>
  public T5 Item5;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's sixth component.
  /// </summary>
  public T6 Item6;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's seventh component.
  /// </summary>
  public T7 Item7;

  /// <summary>
  ///   The current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance's eighth component.
  /// </summary>
  public TRest Rest;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> value type.
  /// </summary>
  /// <param name="item1">The value of the tuple's first component.</param>
  /// <param name="item2">The value of the tuple's second component.</param>
  /// <param name="item3">The value of the tuple's third component.</param>
  /// <param name="item4">The value of the tuple's fourth component.</param>
  /// <param name="item5">The value of the tuple's fifth component.</param>
  /// <param name="item6">The value of the tuple's sixth component.</param>
  /// <param name="item7">The value of the tuple's seventh component.</param>
  /// <param name="rest">The value of the tuple's eight component.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest) {
    if (rest is not IValueTupleInternal)
      AlwaysThrow.ArgumentException(nameof(rest), "The TRest type argument of ValueTuple`8 must be a ValueTuple.");

    this.Item1 = item1;
    this.Item2 = item2;
    this.Item3 = item3;
    this.Item4 = item4;
    this.Item5 = item5;
    this.Item6 = item6;
    this.Item7 = item7;
    this.Rest = rest;
  }

  int IValueTupleInternal.Size {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    get => this.Rest is not IValueTupleInternal rest ? 8 : 7 + rest.Size;
  }

  /// <inheritdoc />
  /// <summary>Compares this instance to a specified instance and returns an indication of their relative values.</summary>
  /// <param name="other">An instance to compare.</param>
  /// <returns>
  ///   A signed number indicating the relative values of this instance and <paramref name="other" />.
  ///   Returns less than zero if this instance is less than <paramref name="other" />, zero if this
  ///   instance is equal to <paramref name="other" />, and greater than zero if this instance is greater
  ///   than <paramref name="other" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other) {
    var c = Comparer<T1>.Default.Compare(this.Item1, other.Item1);
    if (c != 0)
      return c;

    c = Comparer<T2>.Default.Compare(this.Item2, other.Item2);
    if (c != 0)
      return c;

    c = Comparer<T3>.Default.Compare(this.Item3, other.Item3);
    if (c != 0)
      return c;

    c = Comparer<T4>.Default.Compare(this.Item4, other.Item4);
    if (c != 0)
      return c;

    c = Comparer<T5>.Default.Compare(this.Item5, other.Item5);
    if (c != 0)
      return c;

    c = Comparer<T6>.Default.Compare(this.Item6, other.Item6);
    if (c != 0)
      return c;

    c = Comparer<T7>.Default.Compare(this.Item7, other.Item7);
    return c != 0 ? c : Comparer<TRest>.Default.Compare(this.Rest, other.Rest);
  }

  /// <summary>
  ///   Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />
  ///   instance is equal to a specified object.
  /// </summary>
  /// <param name="obj">The object to compare with this instance.</param>
  /// <returns>
  ///   <see langword="true" /> if the current instance is equal to the specified object; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  /// <remarks>
  ///   The <paramref name="obj" /> parameter is considered to be equal to the current instance under the following
  ///   conditions:
  ///   <list type="bullet">
  ///     <item>
  ///       <description>It is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> value type.</description>
  ///     </item>
  ///     <item>
  ///       <description>Its components are of the same types as those of the current instance.</description>
  ///     </item>
  ///     <item>
  ///       <description>
  ///         Its components are equal to those of the current instance. Equality is determined by the default
  ///         object equality comparer for each component.
  ///       </description>
  ///     </item>
  ///   </list>
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => obj is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple && this.Equals(tuple);

  /// <inheritdoc />
  /// <summary>
  ///   Returns a value that indicates whether the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />
  ///   instance is equal to a specified <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />.
  /// </summary>
  /// <param name="other">The tuple to compare with this instance.</param>
  /// <returns>
  ///   <see langword="true" /> if the current instance is equal to the specified tuple; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  /// <remarks>
  ///   The <paramref name="other" /> parameter is considered to be equal to the current instance if each of its fields
  ///   are equal to that of the current instance, using the default comparer for that field's type.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> other) =>
    EqualityComparer<T1>.Default.Equals(this.Item1, other.Item1)
    && EqualityComparer<T2>.Default.Equals(this.Item2, other.Item2)
    && EqualityComparer<T3>.Default.Equals(this.Item3, other.Item3)
    && EqualityComparer<T4>.Default.Equals(this.Item4, other.Item4)
    && EqualityComparer<T5>.Default.Equals(this.Item5, other.Item5)
    && EqualityComparer<T6>.Default.Equals(this.Item6, other.Item6)
    && EqualityComparer<T7>.Default.Equals(this.Item7, other.Item7)
    && EqualityComparer<TRest>.Default.Equals(this.Rest, other.Rest);

  /// <summary>
  ///   Returns the hash code for the current <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance.
  /// </summary>
  /// <returns>A 32-bit signed integer hash code.</returns>
  public override int GetHashCode() {
    // We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
    if (this.Rest is not IValueTupleInternal rest)
      return NumericsHelpers.CombineHash(
        EqualityComparer<T1>.Default.GetHashCode(this.Item1),
        EqualityComparer<T2>.Default.GetHashCode(this.Item2),
        EqualityComparer<T3>.Default.GetHashCode(this.Item3),
        EqualityComparer<T4>.Default.GetHashCode(this.Item4),
        EqualityComparer<T5>.Default.GetHashCode(this.Item5),
        EqualityComparer<T6>.Default.GetHashCode(this.Item6),
        EqualityComparer<T7>.Default.GetHashCode(this.Item7)
      );

    var size = rest.Size;
    if (size >= 8)
      return rest.GetHashCode();

    // In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
    switch (8 - size) {
      case 1:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 2:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 3:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T5>.Default.GetHashCode(this.Item5),
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 4:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T4>.Default.GetHashCode(this.Item4),
          EqualityComparer<T5>.Default.GetHashCode(this.Item5),
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 5:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T3>.Default.GetHashCode(this.Item3),
          EqualityComparer<T4>.Default.GetHashCode(this.Item4),
          EqualityComparer<T5>.Default.GetHashCode(this.Item5),
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 6:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T2>.Default.GetHashCode(this.Item2),
          EqualityComparer<T3>.Default.GetHashCode(this.Item3),
          EqualityComparer<T4>.Default.GetHashCode(this.Item4),
          EqualityComparer<T5>.Default.GetHashCode(this.Item5),
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      case 7:
      case 8:
        return NumericsHelpers.CombineHash(
          EqualityComparer<T1>.Default.GetHashCode(this.Item1),
          EqualityComparer<T2>.Default.GetHashCode(this.Item2),
          EqualityComparer<T3>.Default.GetHashCode(this.Item3),
          EqualityComparer<T4>.Default.GetHashCode(this.Item4),
          EqualityComparer<T5>.Default.GetHashCode(this.Item5),
          EqualityComparer<T6>.Default.GetHashCode(this.Item6),
          EqualityComparer<T7>.Default.GetHashCode(this.Item7),
          rest.GetHashCode()
        );

      default:
        Debug.Fail("Missed all cases for computing ValueTuple hash code");
        return -1;
    }
  }

  /// <summary>
  ///   Returns a string that represents the value of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" />
  ///   instance.
  /// </summary>
  /// <returns>The string representation of this <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}" /> instance.</returns>
  /// <remarks>
  ///   The string returned by this method takes the form <c>(Item1, Item2, Item3, Item4, Item5, Item6, Item7, Rest)</c>.
  ///   If any field value is <see langword="null" />, it is represented as <see cref="string.Empty" />.
  /// </remarks>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => "(" + this.ToStringEnd();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(object obj) {
    switch (obj) {
      case null:
        return 1;
      case ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple:
        return this.CompareTo(tuple);
      default:
        AlwaysThrow.ArgumentException(nameof(obj),"The parameter should be a ValueTuple type of appropriate arity.");
        return 0;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple: {
        var c = comparer.Compare(this.Item1, tuple.Item1);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item2, tuple.Item2);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item3, tuple.Item3);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item4, tuple.Item4);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item5, tuple.Item5);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item6, tuple.Item6);
        if (c != 0)
          return c;

        c = comparer.Compare(this.Item7, tuple.Item7);
        return c != 0 ? c : comparer.Compare(this.Rest, tuple.Rest);
      }
      default:
        AlwaysThrow.ArgumentException(nameof(other),"The parameter should be a ValueTuple type of appropriate arity.");
        return 0;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(object other, IEqualityComparer comparer) => other is ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple
                                                                  && comparer.Equals(this.Item1, tuple.Item1)
                                                                  && comparer.Equals(this.Item2, tuple.Item2)
                                                                  && comparer.Equals(this.Item3, tuple.Item3)
                                                                  && comparer.Equals(this.Item4, tuple.Item4)
                                                                  && comparer.Equals(this.Item5, tuple.Item5)
                                                                  && comparer.Equals(this.Item6, tuple.Item6)
                                                                  && comparer.Equals(this.Item7, tuple.Item7)
                                                                  && comparer.Equals(this.Rest, tuple.Rest);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public int GetHashCode(IEqualityComparer comparer) => this.GetHashCodeCore(comparer);

  private int GetHashCodeCore(IEqualityComparer comparer) {
    // We want to have a limited hash in this case.  We'll use the last 8 elements of the tuple
    if (this.Rest is not IValueTupleInternal rest)
      return NumericsHelpers.CombineHash(
        comparer.GetHashCode(this.Item1),
        comparer.GetHashCode(this.Item2),
        comparer.GetHashCode(this.Item3),
        comparer.GetHashCode(this.Item4),
        comparer.GetHashCode(this.Item5),
        comparer.GetHashCode(this.Item6),
        comparer.GetHashCode(this.Item7)
      );

    var size = rest.Size;
    if (size >= 8)
      return rest.GetHashCode(comparer);

    // In this case, the rest member has less than 8 elements so we need to combine some our elements with the elements in rest
    switch (8 - size) {
      case 1: return NumericsHelpers.CombineHash(comparer.GetHashCode(this.Item7), rest.GetHashCode(comparer));

      case 2: return NumericsHelpers.CombineHash(comparer.GetHashCode(this.Item6), comparer.GetHashCode(this.Item7), rest.GetHashCode(comparer));

      case 3:
        return NumericsHelpers.CombineHash(
          comparer.GetHashCode(this.Item5),
          comparer.GetHashCode(this.Item6),
          comparer.GetHashCode(this.Item7),
          rest.GetHashCode(comparer)
        );

      case 4:
        return NumericsHelpers.CombineHash(
          comparer.GetHashCode(this.Item4),
          comparer.GetHashCode(this.Item5),
          comparer.GetHashCode(this.Item6),
          comparer.GetHashCode(this.Item7),
          rest.GetHashCode(comparer)
        );

      case 5:
        return NumericsHelpers.CombineHash(
          comparer.GetHashCode(this.Item3),
          comparer.GetHashCode(this.Item4),
          comparer.GetHashCode(this.Item5),
          comparer.GetHashCode(this.Item6),
          comparer.GetHashCode(this.Item7),
          rest.GetHashCode(comparer)
        );

      case 6:
        return NumericsHelpers.CombineHash(
          comparer.GetHashCode(this.Item2),
          comparer.GetHashCode(this.Item3),
          comparer.GetHashCode(this.Item4),
          comparer.GetHashCode(this.Item5),
          comparer.GetHashCode(this.Item6),
          comparer.GetHashCode(this.Item7),
          rest.GetHashCode(comparer)
        );

      case 7:
      case 8:
        return NumericsHelpers.CombineHash(
          comparer.GetHashCode(this.Item1),
          comparer.GetHashCode(this.Item2),
          comparer.GetHashCode(this.Item3),
          comparer.GetHashCode(this.Item4),
          comparer.GetHashCode(this.Item5),
          comparer.GetHashCode(this.Item6),
          comparer.GetHashCode(this.Item7),
          rest.GetHashCode(comparer)
        );

      default:
        Debug.Fail("Missed all cases for computing ValueTuple hash code");
        return -1;
    }
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToStringEnd() => this.Rest is not IValueTupleInternal rest 
    ? $"{this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5}, {this.Item6}, {this.Item7}, {this.Rest})" 
    : $"{this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5}, {this.Item6}, {this.Item7}, {rest.ToStringEnd()}"
  ;

}

/// <summary>
///   Provides extension methods for <see cref="Tuple" /> instances to interop with C# tuples features (deconstruction
///   syntax, converting from and to <see cref="ValueTuple" />).
/// </summary>
public static partial class TupleExtensions {
  
  private static ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> CreateLong<T1, T2, T3, T4, T5, T6, T7, TRest>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
    where TRest : struct
    => new(item1, item2, item3, item4, item5, item6, item7, rest);

  private static Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> CreateLongRef<T1, T2, T3, T4, T5, T6, T7, TRest>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
    => new(item1, item2, item3, item4, item5, item6, item7, rest);
}

#endif
