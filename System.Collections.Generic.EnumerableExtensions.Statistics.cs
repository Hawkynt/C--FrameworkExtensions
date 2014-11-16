#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Linq;
using dword = System.UInt32;
using qword = System.UInt64;
using word = System.UInt16;
#if !NET35
  using System.Diagnostics.Contracts;
#endif

// This file holds statistical routines on enumerations.

namespace System.Collections.Generic {
  internal static partial class EnumerableExtensions {
    #region Sum
    #region TimeSpan
    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static TimeSpan Sum(this IEnumerable<TimeSpan> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (TimeSpan.FromMilliseconds(This.Select(i => i.TotalMilliseconds).Sum()));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static TimeSpan Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, TimeSpan> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Sum());
    }
    #endregion
    #region word
    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static word Sum(this IEnumerable<word> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once RedundantCast
      return (This.Aggregate((word)0, (current, i) => (word)(current + i)));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static word Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, word> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Sum());
    }
    #endregion
    #region dword
    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static dword Sum(this IEnumerable<dword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once RedundantCast
      return (This.Aggregate((dword)0, (current, i) => (dword)(current + i)));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static dword Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, dword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Sum());
    }
    #endregion
    #region qword
    /// <summary>
    /// Sums the specified elements in an enumeration.
    /// </summary>
    /// <param name="This">This enumeration.</param>
    /// <returns>The sum of all given elements.</returns>
    public static qword Sum(this IEnumerable<qword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once RedundantCast
      return (This.Aggregate((qword)0, (current, i) => (qword)(current + i)));
    }

    /// <summary>
    /// Sums the specified elements in an enumeration using a selector.
    /// </summary>
    /// <typeparam name="TIn">The type of the items in the enumeration.</typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns>The sum of all given elements.</returns>
    public static qword Sum<TIn>(this IEnumerable<TIn> This, Func<TIn, qword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Sum());
    }
    #endregion

    #endregion
    #region Min
    #region sbyte
    public static sbyte Min(this IEnumerable<sbyte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      sbyte result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static sbyte Min<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #region byte
    public static byte Min(this IEnumerable<byte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      byte result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static byte Min<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #region short
    public static short Min(this IEnumerable<short> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      short result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static short Min<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #region word
    public static word Min(this IEnumerable<word> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      word result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static word Min<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #region dword
    public static dword Min(this IEnumerable<dword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      dword result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static dword Min<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #region qword
    public static qword Min(this IEnumerable<qword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      qword result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current < result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static qword Min<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Min());
    }
    #endregion
    #endregion
    #region MinOrDefault
    #region sbyte
    public static sbyte MinOrDefault(this IEnumerable<sbyte> This, sbyte defaultValue = default(sbyte)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<sbyte> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static sbyte MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector, sbyte defaultValue = default(sbyte)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region sbyte?
    public static sbyte? MinOrDefault(this IEnumerable<sbyte?> This, sbyte? defaultValue = default(sbyte?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region byte
    public static byte MinOrDefault(this IEnumerable<byte> This, byte defaultValue = default(byte)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<byte> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static byte MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector, byte defaultValue = default(byte)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region byte?
    public static byte? MinOrDefault(this IEnumerable<byte?> This, byte? defaultValue = default(byte?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region short
    public static short MinOrDefault(this IEnumerable<short> This, short defaultValue = default(short)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<short> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static short MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector, short defaultValue = default(short)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region short?
    public static short? MinOrDefault(this IEnumerable<short?> This, short? defaultValue = default(short?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region word
    public static word MinOrDefault(this IEnumerable<word> This, word defaultValue = default(word)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<word> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static word MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector, word defaultValue = default(word)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region word?
    public static word? MinOrDefault(this IEnumerable<word?> This, word? defaultValue = default(word?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region int
    public static int MinOrDefault(this IEnumerable<int> This, int defaultValue = default(int)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<int> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static int MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, int> selector, int defaultValue = default(int)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region int?
    public static int? MinOrDefault(this IEnumerable<int?> This, int? defaultValue = default(int?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region dword
    public static dword MinOrDefault(this IEnumerable<dword> This, dword defaultValue = default(dword)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<dword> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static dword MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector, dword defaultValue = default(dword)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region dword?
    public static dword? MinOrDefault(this IEnumerable<dword?> This, dword? defaultValue = default(dword?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region long
    public static long MinOrDefault(this IEnumerable<long> This, long defaultValue = default(long)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<long> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static long MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, long> selector, long defaultValue = default(long)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region long?
    public static long? MinOrDefault(this IEnumerable<long?> This, long? defaultValue = default(long?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region qword
    public static qword MinOrDefault(this IEnumerable<qword> This, qword defaultValue = default(qword)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<qword> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static qword MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector, qword defaultValue = default(qword)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region qword?
    public static qword? MinOrDefault(this IEnumerable<qword?> This, qword? defaultValue = default(qword?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region float
    public static float MinOrDefault(this IEnumerable<float> This, float defaultValue = default(float)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<float> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static float MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, float> selector, float defaultValue = default(float)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region float?
    public static float? MinOrDefault(this IEnumerable<float?> This, float? defaultValue = default(float?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region double
    public static double MinOrDefault(this IEnumerable<double> This, double defaultValue = default(double)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<double> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static double MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, double> selector, double defaultValue = default(double)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region double?
    public static double? MinOrDefault(this IEnumerable<double?> This, double? defaultValue = default(double?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #region decimal
    public static decimal MinOrDefault(this IEnumerable<decimal> This, decimal defaultValue = default(decimal)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<decimal> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Min());
    }

    public static decimal MinOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, decimal> selector, decimal defaultValue = default(decimal)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MinOrDefault());
    }
    #endregion
    #region decimal?
    public static decimal? MinOrDefault(this IEnumerable<decimal?> This, decimal? defaultValue = default(decimal?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Min(x=>x.Value));
    }
    #endregion
    #endregion
    #region Max
    #region sbyte
    public static sbyte Max(this IEnumerable<sbyte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      sbyte result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static sbyte Max<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #region byte
    public static byte Max(this IEnumerable<byte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      byte result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static byte Max<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #region short
    public static short Max(this IEnumerable<short> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      short result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static short Max<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #region word
    public static word Max(this IEnumerable<word> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      word result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static word Max<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #region dword
    public static dword Max(this IEnumerable<dword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      dword result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static dword Max<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #region qword
    public static qword Max(this IEnumerable<qword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      qword result;
      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        result=enumerator.Current;
        while(enumerator.MoveNext())
          if (enumerator.Current > result)
            result = enumerator.Current;
      }
      
      return (result);
    }

    public static qword Max<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).Max());
    }
    #endregion
    #endregion
    #region MaxOrDefault
    #region sbyte
    public static sbyte MaxOrDefault(this IEnumerable<sbyte> This, sbyte defaultValue = default(sbyte)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<sbyte> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static sbyte MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, sbyte> selector, sbyte defaultValue = default(sbyte)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region sbyte?
    public static sbyte? MaxOrDefault(this IEnumerable<sbyte?> This, sbyte? defaultValue = default(sbyte?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region byte
    public static byte MaxOrDefault(this IEnumerable<byte> This, byte defaultValue = default(byte)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<byte> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static byte MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, byte> selector, byte defaultValue = default(byte)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region byte?
    public static byte? MaxOrDefault(this IEnumerable<byte?> This, byte? defaultValue = default(byte?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region short
    public static short MaxOrDefault(this IEnumerable<short> This, short defaultValue = default(short)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<short> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static short MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, short> selector, short defaultValue = default(short)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region short?
    public static short? MaxOrDefault(this IEnumerable<short?> This, short? defaultValue = default(short?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region word
    public static word MaxOrDefault(this IEnumerable<word> This, word defaultValue = default(word)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<word> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static word MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, word> selector, word defaultValue = default(word)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region word?
    public static word? MaxOrDefault(this IEnumerable<word?> This, word? defaultValue = default(word?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region int
    public static int MaxOrDefault(this IEnumerable<int> This, int defaultValue = default(int)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<int> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static int MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, int> selector, int defaultValue = default(int)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region int?
    public static int? MaxOrDefault(this IEnumerable<int?> This, int? defaultValue = default(int?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region dword
    public static dword MaxOrDefault(this IEnumerable<dword> This, dword defaultValue = default(dword)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<dword> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static dword MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, dword> selector, dword defaultValue = default(dword)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region dword?
    public static dword? MaxOrDefault(this IEnumerable<dword?> This, dword? defaultValue = default(dword?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region long
    public static long MaxOrDefault(this IEnumerable<long> This, long defaultValue = default(long)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<long> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static long MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, long> selector, long defaultValue = default(long)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region long?
    public static long? MaxOrDefault(this IEnumerable<long?> This, long? defaultValue = default(long?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region qword
    public static qword MaxOrDefault(this IEnumerable<qword> This, qword defaultValue = default(qword)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<qword> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static qword MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, qword> selector, qword defaultValue = default(qword)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region qword?
    public static qword? MaxOrDefault(this IEnumerable<qword?> This, qword? defaultValue = default(qword?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region float
    public static float MaxOrDefault(this IEnumerable<float> This, float defaultValue = default(float)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<float> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static float MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, float> selector, float defaultValue = default(float)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region float?
    public static float? MaxOrDefault(this IEnumerable<float?> This, float? defaultValue = default(float?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region double
    public static double MaxOrDefault(this IEnumerable<double> This, double defaultValue = default(double)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<double> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static double MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, double> selector, double defaultValue = default(double)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region double?
    public static double? MaxOrDefault(this IEnumerable<double?> This, double? defaultValue = default(double?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #region decimal
    public static decimal MaxOrDefault(this IEnumerable<decimal> This, decimal defaultValue = default(decimal)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This as ICollection<decimal> ?? This.ToList();
      return (items.Count == 0 ? defaultValue : items.Max());
    }

    public static decimal MaxOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem, decimal> selector, decimal defaultValue = default(decimal)) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return (This.Select(selector).MaxOrDefault());
    }
    #endregion
    #region decimal?
    public static decimal? MaxOrDefault(this IEnumerable<decimal?> This, decimal? defaultValue = default(decimal?)) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var items = This.Where(x=>x.HasValue).ToList();
      // ReSharper disable once PossibleInvalidOperationException
      return (items.Count == 0 ? defaultValue : items.Max(x=>x.Value));
    }
    #endregion
    #endregion
    #region AverageOrDefault
    #region float
    public static float AverageOrDefault(this IEnumerable<float> This, float defaultValue = default(float)) {
      if (This == null)
        return (defaultValue);

      float result = 0;
      var count = 0;
      foreach (var item in This) {
        result += item;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }

    public static float AverageOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem,float> selector, float defaultValue = default(float)) {
#if NET35
      Debug.Assert(selector != null);
#else
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).AverageOrDefault(defaultValue));
    }
    #endregion
    #region float?
    public static float? AverageOrDefault(this IEnumerable<float?> This, float? defaultValue = default(float?)) {
      if (This == null)
        return (defaultValue);

      float result = 0;
      var count = 0;
      foreach (var item in This.Where(x=>x.HasValue)) {
        result += item.Value;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }
    #endregion
    #region double
    public static double AverageOrDefault(this IEnumerable<double> This, double defaultValue = default(double)) {
      if (This == null)
        return (defaultValue);

      double result = 0;
      var count = 0;
      foreach (var item in This) {
        result += item;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }

    public static double AverageOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem,double> selector, double defaultValue = default(double)) {
#if NET35
      Debug.Assert(selector != null);
#else
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).AverageOrDefault(defaultValue));
    }
    #endregion
    #region double?
    public static double? AverageOrDefault(this IEnumerable<double?> This, double? defaultValue = default(double?)) {
      if (This == null)
        return (defaultValue);

      double result = 0;
      var count = 0;
      foreach (var item in This.Where(x=>x.HasValue)) {
        result += item.Value;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }
    #endregion
    #region decimal
    public static decimal AverageOrDefault(this IEnumerable<decimal> This, decimal defaultValue = default(decimal)) {
      if (This == null)
        return (defaultValue);

      decimal result = 0;
      var count = 0;
      foreach (var item in This) {
        result += item;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }

    public static decimal AverageOrDefault<TItem>(this IEnumerable<TItem> This, Func<TItem,decimal> selector, decimal defaultValue = default(decimal)) {
#if NET35
      Debug.Assert(selector != null);
#else
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).AverageOrDefault(defaultValue));
    }
    #endregion
    #region decimal?
    public static decimal? AverageOrDefault(this IEnumerable<decimal?> This, decimal? defaultValue = default(decimal?)) {
      if (This == null)
        return (defaultValue);

      decimal result = 0;
      var count = 0;
      foreach (var item in This.Where(x=>x.HasValue)) {
        result += item.Value;
        count++;
      }

      return (count == 0 ? defaultValue : result / count);
    }
    #endregion
    #endregion
    #region Variance
    #region float
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Variance(this IEnumerable<float> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var collection=This as ICollection<float> ?? This.ToList();
      var count = collection.Count;
      var mean = collection.Aggregate((float)0, (r, x) => r + x) / count;
      var sum = collection.Aggregate((float)0, (r, x) => r + (x - mean) * (x - mean));
      var result = sum / (count - 1);
      return(result);
    }
    
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float Variance<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Variance());
    }
    #endregion
    #region float?
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Variance(this IEnumerable<float?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Variance(x=>x.Value));
    }
    #endregion
    #region double
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Variance(this IEnumerable<double> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var collection=This as ICollection<double> ?? This.ToList();
      var count = collection.Count;
      var mean = collection.Aggregate((double)0, (r, x) => r + x) / count;
      var sum = collection.Aggregate((double)0, (r, x) => r + (x - mean) * (x - mean));
      var result = sum / (count - 1);
      return(result);
    }
    
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double Variance<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Variance());
    }
    #endregion
    #region double?
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Variance(this IEnumerable<double?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Variance(x=>x.Value));
    }
    #endregion
    #region decimal
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Variance(this IEnumerable<decimal> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var collection=This as ICollection<decimal> ?? This.ToList();
      var count = collection.Count;
      var mean = collection.Aggregate((decimal)0, (r, x) => r + x) / count;
      var sum = collection.Aggregate((decimal)0, (r, x) => r + (x - mean) * (x - mean));
      var result = sum / (count - 1);
      return(result);
    }
    
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal Variance<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Variance());
    }
    #endregion
    #region decimal?
    /// <summary>
    /// Variance is the measure of the amount of variation of all the scores for a variable (not just the extremes which give the range).
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Variance(this IEnumerable<decimal?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Variance(x=>x.Value));
    }
    #endregion
    #endregion
    #region Standard Deviation
    #region float
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float StdDev(this IEnumerable<float> This) {
      var result = This.Variance();
      result = _Sqrt(result);
      return (result);
    }
    
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float StdDev<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).StdDev());
    }
    #endregion
    #region float?
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float StdDev(this IEnumerable<float?> This) {
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).StdDev(x=>x.Value));
    }
    #endregion
    #region double
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double StdDev(this IEnumerable<double> This) {
      var result = This.Variance();
      result = _Sqrt(result);
      return (result);
    }
    
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double StdDev<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).StdDev());
    }
    #endregion
    #region double?
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double StdDev(this IEnumerable<double?> This) {
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).StdDev(x=>x.Value));
    }
    #endregion
    #region decimal
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal StdDev(this IEnumerable<decimal> This) {
      var result = This.Variance();
      result = _Sqrt(result);
      return (result);
    }
    
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal StdDev<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).StdDev());
    }
    #endregion
    #region decimal?
    /// <summary>
    /// The Standard Deviation of a statistical population, a data set, or a probability distribution is the square root of its variance.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal StdDev(this IEnumerable<decimal?> This) {
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).StdDev(x=>x.Value));
    }
    #endregion
    #endregion
    #region Center
    #region float
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Center(this IEnumerable<float> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var sortedList = This.OrderBy(v => v).ToList();
      var index = sortedList.Count >> 1;
      return (sortedList.Count & 1) == 0 ? (sortedList[index] + sortedList[index - 1]) / 2 : sortedList[index];
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float Center<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Center());
    }
    #endregion
    #region float?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Center(this IEnumerable<float?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Center(x=>x.Value));
    }
    #endregion
    #region double
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Center(this IEnumerable<double> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var sortedList = This.OrderBy(v => v).ToList();
      var index = sortedList.Count >> 1;
      return (sortedList.Count & 1) == 0 ? (sortedList[index] + sortedList[index - 1]) / 2 : sortedList[index];
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double Center<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Center());
    }
    #endregion
    #region double?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Center(this IEnumerable<double?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Center(x=>x.Value));
    }
    #endregion
    #region decimal
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Center(this IEnumerable<decimal> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      var sortedList = This.OrderBy(v => v).ToList();
      var index = sortedList.Count >> 1;
      return (sortedList.Count & 1) == 0 ? (sortedList[index] + sortedList[index - 1]) / 2 : sortedList[index];
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal Center<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Center());
    }
    #endregion
    #region decimal?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Center(this IEnumerable<decimal?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Center(x=>x.Value));
    }
    #endregion
    #endregion
    #region Median
    #region float
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Median(this IEnumerable<float> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return(This.Min()+This.Range()/2);
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float Median<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Median());
    }
    #endregion
    #region float?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Median(this IEnumerable<float?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Median(x=>x.Value));
    }
    #endregion
    #region double
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Median(this IEnumerable<double> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return(This.Min()+This.Range()/2);
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double Median<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Median());
    }
    #endregion
    #region double?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Median(this IEnumerable<double?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Median(x=>x.Value));
    }
    #endregion
    #region decimal
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Median(this IEnumerable<decimal> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return(This.Min()+This.Range()/2);
    }
    
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal Median<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Median());
    }
    #endregion
    #region decimal?
    /// <summary>
    /// Median is the number separating the higher half of a sample, a population, or a probability distribution, from the lower half.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Median(this IEnumerable<decimal?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return (This.Where(x=>x.HasValue).Median(x=>x.Value));
    }
    #endregion
    #endregion
    #region Mode
    /// <summary>
    /// Mode is the value that occurs the most frequently in a data set or a probability distribution.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static T Mode<T>(this IEnumerable<T> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      return (
        from v in This
        group v by v
          into i
          select new { i.Key, Count = i.Count() } into j
          orderby j.Count descending
          select j.Key
        ).First();
    }
    #endregion
    #region Range
    #region sbyte
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static sbyte Range(this IEnumerable<sbyte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      sbyte min;
      sbyte max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (sbyte)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static sbyte Range<TItem>(this IEnumerable<TItem> This,Func<TItem,sbyte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region sbyte?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static sbyte Range(this IEnumerable<sbyte?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region byte
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static byte Range(this IEnumerable<byte> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      byte min;
      byte max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (byte)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static byte Range<TItem>(this IEnumerable<TItem> This,Func<TItem,byte> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region byte?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static byte Range(this IEnumerable<byte?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region short
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static short Range(this IEnumerable<short> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      short min;
      short max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (short)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static short Range<TItem>(this IEnumerable<TItem> This,Func<TItem,short> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region short?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static short Range(this IEnumerable<short?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region word
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static word Range(this IEnumerable<word> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      word min;
      word max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (word)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static word Range<TItem>(this IEnumerable<TItem> This,Func<TItem,word> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region word?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static word Range(this IEnumerable<word?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region int
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static int Range(this IEnumerable<int> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      int min;
      int max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (int)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static int Range<TItem>(this IEnumerable<TItem> This,Func<TItem,int> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region int?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static int Range(this IEnumerable<int?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region dword
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static dword Range(this IEnumerable<dword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      dword min;
      dword max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (dword)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static dword Range<TItem>(this IEnumerable<TItem> This,Func<TItem,dword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region dword?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static dword Range(this IEnumerable<dword?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region long
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static long Range(this IEnumerable<long> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      long min;
      long max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (long)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static long Range<TItem>(this IEnumerable<TItem> This,Func<TItem,long> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region long?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static long Range(this IEnumerable<long?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region qword
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static qword Range(this IEnumerable<qword> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      qword min;
      qword max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (qword)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static qword Range<TItem>(this IEnumerable<TItem> This,Func<TItem,qword> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region qword?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static qword Range(this IEnumerable<qword?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region float
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Range(this IEnumerable<float> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      float min;
      float max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (float)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float Range<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region float?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static float Range(this IEnumerable<float?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region double
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Range(this IEnumerable<double> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      double min;
      double max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (double)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double Range<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region double?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static double Range(this IEnumerable<double?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
    #region decimal
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Range(this IEnumerable<decimal> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      decimal min;
      decimal max;

      using(var enumerator=This.GetEnumerator()) {
        if(!enumerator.MoveNext())
          throw new InvalidOperationException("Enumeration is empty.");
        
        min=max=enumerator.Current;
        while(enumerator.MoveNext()){
          var x=enumerator.Current;
          if (x < min) {
            min = x;
            continue;
          }
          if (x > max)
            max = x;
        }
      }
      
      // ReSharper disable once RedundantCast
      return (decimal)(max - min);
    }
    
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal Range<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Range());
    }
    #endregion
    #region decimal?
    /// <summary>
    /// Range is the length of the smallest interval which contains all the data.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <returns></returns>
    public static decimal Range(this IEnumerable<decimal?> This) {
#if NET35
      Debug.Assert(This != null);
#else
      Contract.Requires(This != null);
#endif
      // ReSharper disable once PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Range(x=>x.Value));
    }
    #endregion
 
    #endregion
    #region Covariance
    #region float
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static float Covariance(this IEnumerable<float> This, IEnumerable<float> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      var a = This as IList<float> ?? This.ToList();
      var b = other as IList<float> ?? other.ToList();
      var avga = a.Average();
      var avgb = b.Average();
      var result = (float)0;
      var length = Math.Min(a.Count, b.Count);
      for (var i = length - 1; i >= 0; --i)
        result += (a[i] - avga) * (b[i] - avgb);

      return (result / length);
    }

    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static float Covariance<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> other,Func<TItem,float> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Covariance(other.Select(selector)));
    }
    
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector1">The selector.</param>
    /// <param name="selector2">The selector for the other enumeration.</param>
    /// <returns></returns>
    public static float Covariance<TItem>(this IEnumerable<TItem> This,Func<TItem,float> selector1, IEnumerable<TItem> other,Func<TItem,float> selector2) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector1 != null);
      Debug.Assert(selector2 != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector1 != null);
      Contract.Requires(selector2 != null);
#endif
      return(This.Select(selector1).Covariance(other.Select(selector2)));
    }
    #endregion
    #region float?
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static float Covariance(this IEnumerable<float?> This, IEnumerable<float?> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      // ReSharper disable PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Covariance(x=>x.Value,other.Where(x=>x.HasValue),x=>x.Value));
      // ReSharper restore PossibleInvalidOperationException
    }
    #endregion
    #region double
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static double Covariance(this IEnumerable<double> This, IEnumerable<double> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      var a = This as IList<double> ?? This.ToList();
      var b = other as IList<double> ?? other.ToList();
      var avga = a.Average();
      var avgb = b.Average();
      var result = (double)0;
      var length = Math.Min(a.Count, b.Count);
      for (var i = length - 1; i >= 0; --i)
        result += (a[i] - avga) * (b[i] - avgb);

      return (result / length);
    }

    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static double Covariance<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> other,Func<TItem,double> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Covariance(other.Select(selector)));
    }
    
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector1">The selector.</param>
    /// <param name="selector2">The selector for the other enumeration.</param>
    /// <returns></returns>
    public static double Covariance<TItem>(this IEnumerable<TItem> This,Func<TItem,double> selector1, IEnumerable<TItem> other,Func<TItem,double> selector2) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector1 != null);
      Debug.Assert(selector2 != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector1 != null);
      Contract.Requires(selector2 != null);
#endif
      return(This.Select(selector1).Covariance(other.Select(selector2)));
    }
    #endregion
    #region double?
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static double Covariance(this IEnumerable<double?> This, IEnumerable<double?> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      // ReSharper disable PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Covariance(x=>x.Value,other.Where(x=>x.HasValue),x=>x.Value));
      // ReSharper restore PossibleInvalidOperationException
    }
    #endregion
    #region decimal
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static decimal Covariance(this IEnumerable<decimal> This, IEnumerable<decimal> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      var a = This as IList<decimal> ?? This.ToList();
      var b = other as IList<decimal> ?? other.ToList();
      var avga = a.Average();
      var avgb = b.Average();
      var result = (decimal)0;
      var length = Math.Min(a.Count, b.Count);
      for (var i = length - 1; i >= 0; --i)
        result += (a[i] - avga) * (b[i] - avgb);

      return (result / length);
    }

    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector">The selector.</param>
    /// <returns></returns>
    public static decimal Covariance<TItem>(this IEnumerable<TItem> This, IEnumerable<TItem> other,Func<TItem,decimal> selector) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector != null);
#endif
      return(This.Select(selector).Covariance(other.Select(selector)));
    }
    
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <param name="selector1">The selector.</param>
    /// <param name="selector2">The selector for the other enumeration.</param>
    /// <returns></returns>
    public static decimal Covariance<TItem>(this IEnumerable<TItem> This,Func<TItem,decimal> selector1, IEnumerable<TItem> other,Func<TItem,decimal> selector2) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
      Debug.Assert(selector1 != null);
      Debug.Assert(selector2 != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
      Contract.Requires(selector1 != null);
      Contract.Requires(selector2 != null);
#endif
      return(This.Select(selector1).Covariance(other.Select(selector2)));
    }
    #endregion
    #region decimal?
    /// <summary>
    /// Covariance is a measure of how much two variables change together.
    /// </summary>
    /// <param name="This">This Enumeration.</param>
    /// <param name="other">The other enumeration.</param>
    /// <returns></returns>
    public static decimal Covariance(this IEnumerable<decimal?> This, IEnumerable<decimal?> other) {
#if NET35
      Debug.Assert(This != null);
      Debug.Assert(other != null);
#else
      Contract.Requires(This != null);
      Contract.Requires(other != null);
#endif
      // ReSharper disable PossibleInvalidOperationException
      return(This.Where(x=>x.HasValue).Covariance(x=>x.Value,other.Where(x=>x.HasValue),x=>x.Value));
      // ReSharper restore PossibleInvalidOperationException
    }
    #endregion
    #endregion
    #region Math Utils
    /// <summary>
    /// Calculate a less accurate square root.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <returns>
    /// The square root of x
    /// </returns>
    private static float _Sqrt(float x) {
      return ((float)Math.Sqrt(x));
    }
    
    /// <summary>
    /// Just a wrapper for math.sqrt,
    /// </summary>
    /// <param name="x">The x.</param>
    /// <returns>The square root of x</returns>
    private static double _Sqrt(double x) {
      return (Math.Sqrt(x));
    }
    
    /// <summary>
    /// Calculate a more accurate square root, see http://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="epsilon">The epsilon.</param>
    /// <returns>The square root of x</returns>
    private static decimal _Sqrt(decimal x, decimal epsilon = 0) {
      if (x < 0) throw new OverflowException("Cannot calculate square root from a negative number");

      decimal current = (decimal)Math.Sqrt((double)x), previous;
      const decimal factor = 2m;
      const decimal zero = decimal.Zero;

      do {
        previous = current;
        if (previous == zero) return zero;
        current = (previous + x / previous) / factor;
      }
      while (Math.Abs(previous - current) > epsilon);
      return(current);
    }
    #endregion
  }
}