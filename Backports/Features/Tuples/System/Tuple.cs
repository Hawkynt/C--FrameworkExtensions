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

// Copyright (c) Microsoft. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !SUPPORTS_TUPLES

#pragma warning disable CA1036 // Override methods on comparable types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1066 // Implement IEquatable when overriding Object.Equals
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CC0074 // Make field readonly
#pragma warning disable RCS1212 // Remove redundant assignment.
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'

using Guard;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static class Tuple {

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1> Create<T1>(T1 item1) => new(item1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new(item1, item2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new(item1, item2, item3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new(item1, item2, item3, item4);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3, T4, T5>
    Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
    => new(item1, item2, item3, item4, item5);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6
  )
    => new(item1, item2, item3, item4, item5, item6);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6,
    T7 item7
  )
    => new(item1, item2, item3, item4, item5, item6, item7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
    T1 item1,
    T2 item2,
    T3 item3,
    T4 item4,
    T5 item5,
    T6 item6,
    T7 item7,
    T8 item8
  )
    => new(
      item1,
      item2,
      item3,
      item4,
      item5,
      item6,
      item7,
      new(item8)
    );
}

[Serializable]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1>(T1 item1) : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1> tuple && comparer.Equals(this.Item1, tuple.Item1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(this.Item1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    if (other == null)
      return 1;

    if (other is Tuple<T1> tuple)
      return comparer.Compare(this.Item1, tuple.Item1);
    
    AlwaysThrow.ArgumentException(nameof(other), string.Empty);
    return 0;
  }
}

[Serializable]
[method:MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2>(T1 item1, T2 item2) : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  
  public T1 Item1 { get; } = item1;

  public T2 Item2 { get; } = item2;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException(nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
[method:MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2, T3>(T1 item1, T2 item2, T3 item3) : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;
  public T2 Item2 { get; } = item2;
  public T3 Item3 { get; } = item3;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2}, {this.Item3})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException(nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
[method:MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4)
  : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;
  public T2 Item2 { get; } = item2;
  public T3 Item3 { get; } = item3;
  public T4 Item4 { get; } = item4;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3, T4> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3)
                                                                                && comparer.Equals(this.Item4, tuple.Item4);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3, T4> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        if (result == 0)
          result = comparer.Compare(this.Item4, tuple.Item4);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException( nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
  : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;
  public T2 Item2 { get; } = item2;
  public T3 Item3 { get; } = item3;
  public T4 Item4 { get; } = item4;
  public T5 Item5 { get; } = item5;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3, T4, T5> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3)
                                                                                && comparer.Equals(this.Item4, tuple.Item4)
                                                                                && comparer.Equals(this.Item5, tuple.Item5);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3, T4, T5> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        if (result == 0)
          result = comparer.Compare(this.Item4, tuple.Item4);

        if (result == 0)
          result = comparer.Compare(this.Item5, tuple.Item5);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException(nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
[method:MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
  : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;
  public T2 Item2 { get; } = item2;
  public T3 Item3 { get; } = item3;
  public T4 Item4 { get; } = item4;
  public T5 Item5 { get; } = item5;
  public T6 Item6 { get; } = item6;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3, T4, T5, T6> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3)
                                                                                && comparer.Equals(this.Item4, tuple.Item4)
                                                                                && comparer.Equals(this.Item5, tuple.Item5)
                                                                                && comparer.Equals(this.Item6, tuple.Item6);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item6);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5}, {this.Item6})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3, T4, T5, T6> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        if (result == 0)
          result = comparer.Compare(this.Item4, tuple.Item4);

        if (result == 0)
          result = comparer.Compare(this.Item5, tuple.Item5);

        if (result == 0)
          result = comparer.Compare(this.Item6, tuple.Item6);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException( nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public class Tuple<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
  : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  public T1 Item1 { get; } = item1;
  public T2 Item2 { get; } = item2;
  public T3 Item3 { get; } = item3;
  public T4 Item4 { get; } = item4;
  public T5 Item5 { get; } = item5;
  public T6 Item6 { get; } = item6;
  public T7 Item7 { get; } = item7;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3, T4, T5, T6, T7> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3)
                                                                                && comparer.Equals(this.Item4, tuple.Item4)
                                                                                && comparer.Equals(this.Item5, tuple.Item5)
                                                                                && comparer.Equals(this.Item6, tuple.Item6)
                                                                                && comparer.Equals(this.Item7, tuple.Item7);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item6);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item7);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5}, {this.Item6}, {this.Item7})";

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3, T4, T5, T6, T7> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        if (result == 0)
          result = comparer.Compare(this.Item4, tuple.Item4);

        if (result == 0)
          result = comparer.Compare(this.Item5, tuple.Item5);

        if (result == 0)
          result = comparer.Compare(this.Item6, tuple.Item6);

        if (result == 0)
          result = comparer.Compare(this.Item7, tuple.Item7);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException( nameof(other),string.Empty);
        return 0;
    }
  }
}

[Serializable]
public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal {
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest) {
    if(rest is not ITupleInternal)
      AlwaysThrow.ArgumentException(nameof(rest), "The last element of an eight element tuple must be a Tuple.");

    this.Item1 = item1;
    this.Item2 = item2;
    this.Item3 = item3;
    this.Item4 = item4;
    this.Item5 = item5;
    this.Item6 = item6;
    this.Item7 = item7;
    this.Rest = rest;
  }

  public T1 Item1 { get; }
  public T2 Item2 { get; }
  public T3 Item3 { get; }
  public T4 Item4 { get; }
  public T5 Item5 { get; }
  public T6 Item6 { get; }
  public T7 Item7 { get; }
  public TRest Rest { get; }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple
                                                                                && comparer.Equals(this.Item1, tuple.Item1)
                                                                                && comparer.Equals(this.Item2, tuple.Item2)
                                                                                && comparer.Equals(this.Item3, tuple.Item3)
                                                                                && comparer.Equals(this.Item4, tuple.Item4)
                                                                                && comparer.Equals(this.Item5, tuple.Item5)
                                                                                && comparer.Equals(this.Item6, tuple.Item6)
                                                                                && comparer.Equals(this.Item7, tuple.Item7)
                                                                                && comparer.Equals(this.Rest, tuple.Rest);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
    var hash = comparer.GetHashCode(this.Item1);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item6);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Item7);
    hash = (hash << 5) - hash + comparer.GetHashCode(this.Rest);
    return hash;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() {
    var restString = this.Rest?.ToString() ?? string.Empty;
    return $"({this.Item1}, {this.Item2}, {this.Item3}, {this.Item4}, {this.Item5}, {this.Item6}, {this.Item7}, {restString[1..^1]})";
  }
  
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private int CompareTo(object other, IComparer comparer) {
    switch (other) {
      case null:
        return 1;
      case Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple: {
        var result = comparer.Compare(this.Item1, tuple.Item1);
        if (result == 0)
          result = comparer.Compare(this.Item2, tuple.Item2);

        if (result == 0)
          result = comparer.Compare(this.Item3, tuple.Item3);

        if (result == 0)
          result = comparer.Compare(this.Item4, tuple.Item4);

        if (result == 0)
          result = comparer.Compare(this.Item5, tuple.Item5);

        if (result == 0)
          result = comparer.Compare(this.Item6, tuple.Item6);

        if (result == 0)
          result = comparer.Compare(this.Item7, tuple.Item7);

        if (result == 0)
          result = comparer.Compare(this.Item7, tuple.Item7);

        return result;
      }
      default:
        AlwaysThrow.ArgumentException( nameof(other),string.Empty);
        return 0;
    }
  }
}

#endif
