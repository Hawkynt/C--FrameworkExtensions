#if !NET40_OR_GREATER && !NETCOREAPP && !NETSTANDARD

// Aggregated and sourced from https://github.com/theraot/Theraot

// Copyright (c) Microsoft. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA1036 // Override methods on comparable types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1066 // Implement IEquatable when overriding Object.Equals
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CC0074 // Make field readonly
#pragma warning disable RCS1212 // Remove redundant assignment.
#pragma warning disable RECS0017 // Possible compare of value type with 'null'
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System {
  namespace Collections {
    public interface IStructuralComparable {
      int CompareTo(object other, IComparer comparer);
    }

    public interface IStructuralEquatable {
      bool Equals(object other, IEqualityComparer comparer);

      int GetHashCode(IEqualityComparer comparer);
    }
  }

  public static class Tuple {
    public static Tuple<T1> Create<T1>(T1 item1) => new(item1);

    public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => new(item1, item2);

    public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => new(item1, item2, item3);

    public static Tuple<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 item1, T2 item2, T3 item3, T4 item4) => new(item1, item2, item3, item4);

    public static Tuple<T1, T2, T3, T4, T5>
      Create<T1, T2, T3, T4, T5>(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) =>
      new(item1, item2, item3, item4, item5);

    public static Tuple<T1, T2, T3, T4, T5, T6> Create<T1, T2, T3, T4, T5, T6>(T1 item1, T2 item2, T3 item3, T4 item4,
      T5 item5, T6 item6) =>
      new(item1, item2, item3, item4, item5, item6);

    public static Tuple<T1, T2, T3, T4, T5, T6, T7> Create<T1, T2, T3, T4, T5, T6, T7>(T1 item1, T2 item2, T3 item3,
      T4 item4, T5 item5, T6 item6, T7 item7) =>
      new(item1, item2, item3, item4, item5, item6, item7);

    public static Tuple<T1, T2, T3, T4, T5, T6, T7, Tuple<T8>> Create<T1, T2, T3, T4, T5, T6, T7, T8>(T1 item1,
      T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8) =>
      new(item1, item2, item3, item4, item5, item6, item7,
        new(item8));
  }

  [Serializable]

  public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1) => this.Item1 = item1;

    public T1 Item1 { get; }

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) => other is Tuple<T1> tuple && comparer.Equals(this.Item1, tuple.Item1);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(this.Item1);

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0})", this.Item1);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

      return comparer.Compare(this.Item1, tuple.Item1);
    }
  }

  [Serializable]

  public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2) {
      this.Item1 = item1;
      this.Item2 = item2;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
      var hash = comparer.GetHashCode(this.Item1);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
      return hash;
    }

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0}, {1})", this.Item1, this.Item2);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

      var result = comparer.Compare(this.Item1, tuple.Item1);
      if (result == 0)
        result = comparer.Compare(this.Item2, tuple.Item2);

      return result;
    }
  }

  [Serializable]

  public class Tuple<T1, T2, T3> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3) {
      this.Item1 = item1;
      this.Item2 = item2;
      this.Item3 = item3;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    public T3 Item3 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
      var hash = comparer.GetHashCode(this.Item1);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
      return hash;
    }

    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2})", this.Item1, this.Item2, this.Item3);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

      var result = comparer.Compare(this.Item1, tuple.Item1);
      if (result == 0)
        result = comparer.Compare(this.Item2, tuple.Item2);

      if (result == 0)
        result = comparer.Compare(this.Item3, tuple.Item3);

      return result;
    }
  }

  [Serializable]

  public class Tuple<T1, T2, T3, T4> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4) {
      this.Item1 = item1;
      this.Item2 = item2;
      this.Item3 = item3;
      this.Item4 = item4;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    public T3 Item3 { get; }

    public T4 Item4 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3, T4> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3)
             && comparer.Equals(this.Item4, tuple.Item4);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
      var hash = comparer.GetHashCode(this.Item1);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
      return hash;
    }

    public override string ToString() =>
      string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3})", this.Item1, this.Item2, this.Item3,
        this.Item4);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3, T4> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

      var result = comparer.Compare(this.Item1, tuple.Item1);
      if (result == 0)
        result = comparer.Compare(this.Item2, tuple.Item2);

      if (result == 0)
        result = comparer.Compare(this.Item3, tuple.Item3);

      if (result == 0)
        result = comparer.Compare(this.Item4, tuple.Item4);

      return result;
    }
  }

  [Serializable]

  public class Tuple<T1, T2, T3, T4, T5> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5) {
      this.Item1 = item1;
      this.Item2 = item2;
      this.Item3 = item3;
      this.Item4 = item4;
      this.Item5 = item5;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    public T3 Item3 { get; }

    public T4 Item4 { get; }

    public T5 Item5 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3, T4, T5> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3)
             && comparer.Equals(this.Item4, tuple.Item4)
             && comparer.Equals(this.Item5, tuple.Item5);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
      var hash = comparer.GetHashCode(this.Item1);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
      return hash;
    }

    public override string ToString() =>
      string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4})", this.Item1, this.Item2,
        this.Item3, this.Item4, this.Item5);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3, T4, T5> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

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
  }

  [Serializable]

  public class Tuple<T1, T2, T3, T4, T5, T6> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6) {
      this.Item1 = item1;
      this.Item2 = item2;
      this.Item3 = item3;
      this.Item4 = item4;
      this.Item5 = item5;
      this.Item6 = item6;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    public T3 Item3 { get; }

    public T4 Item4 { get; }

    public T5 Item5 { get; }

    public T6 Item6 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3, T4, T5, T6> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3)
             && comparer.Equals(this.Item4, tuple.Item4)
             && comparer.Equals(this.Item5, tuple.Item5)
             && comparer.Equals(this.Item6, tuple.Item6);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

    int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) {
      var hash = comparer.GetHashCode(this.Item1);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item2);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item3);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item4);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item5);
      hash = (hash << 5) - hash + comparer.GetHashCode(this.Item6);
      return hash;
    }

    public override string ToString() =>
      string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5})", this.Item1, this.Item2,
        this.Item3, this.Item4, this.Item5, this.Item6);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3, T4, T5, T6> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

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
  }

  [Serializable]

  public class Tuple<T1, T2, T3, T4, T5, T6, T7> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7) {
      this.Item1 = item1;
      this.Item2 = item2;
      this.Item3 = item3;
      this.Item4 = item4;
      this.Item5 = item5;
      this.Item6 = item6;
      this.Item7 = item7;
    }

    public T1 Item1 { get; }

    public T2 Item2 { get; }

    public T3 Item3 { get; }

    public T4 Item4 { get; }

    public T5 Item5 { get; }

    public T6 Item6 { get; }

    public T7 Item7 { get; }

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3)
             && comparer.Equals(this.Item4, tuple.Item4)
             && comparer.Equals(this.Item5, tuple.Item5)
             && comparer.Equals(this.Item6, tuple.Item6)
             && comparer.Equals(this.Item7, tuple.Item7);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

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

    public override string ToString() =>
      string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5}, {6})", this.Item1, this.Item2,
        this.Item3, this.Item4, this.Item5, this.Item6, this.Item7);

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

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
  }

  [Serializable]
  public class Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> : IStructuralEquatable, IStructuralComparable, IComparable {
    public Tuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest) {
      CheckType(rest);
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

    int IStructuralComparable.CompareTo(object other, IComparer comparer) => this.CompareTo(other, comparer);

    int IComparable.CompareTo(object obj) => this.CompareTo(obj, Comparer<object>.Default);

    public override bool Equals(object obj) => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);

    bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer) {
      if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple)
        return false;

      return comparer.Equals(this.Item1, tuple.Item1)
             && comparer.Equals(this.Item2, tuple.Item2)
             && comparer.Equals(this.Item3, tuple.Item3)
             && comparer.Equals(this.Item4, tuple.Item4)
             && comparer.Equals(this.Item5, tuple.Item5)
             && comparer.Equals(this.Item6, tuple.Item6)
             && comparer.Equals(this.Item7, tuple.Item7)
             && comparer.Equals(this.Rest, tuple.Rest);
    }

    public override int GetHashCode() => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

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

    public override string ToString() {
      var restString = this.Rest.ToString();
      return string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})", this.Item1,
        this.Item2, this.Item3, this.Item4, this.Item5, this.Item6, this.Item7,
        restString.Substring(1, restString.Length - 2));
    }

    private static void CheckType(TRest rest) {
      if (rest == null || !typeof(TRest).IsGenericType)
        throw new ArgumentException("The last element of an eight element tuple must be a Tuple.", nameof(rest));

      var type = typeof(TRest).GetGenericTypeDefinition();
      if
      (
        type == typeof(Tuple<>)
        || type == typeof(Tuple<,>)
        || type == typeof(Tuple<,,>)
        || type == typeof(Tuple<,,,>)
        || type == typeof(Tuple<,,,,>)
        || type == typeof(Tuple<,,,,,>)
        || type == typeof(Tuple<,,,,,,>)
        || type == typeof(Tuple<,,,,,,,>)
      )
        return;

      throw new ArgumentException("The last element of an eight element tuple must be a Tuple.", nameof(rest));
    }

    private int CompareTo(object other, IComparer comparer) {
      if (other == null)
        return 1;

      if (other is not Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple)
        throw new ArgumentException(string.Empty, nameof(other));

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
  }
}
#endif
