#region (c)2010-2042 Hawkynt
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

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif

using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Collections.Generic {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ListExtensions {

    /// <summary>
    /// Removes the given items from the list.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    /// <param name="This">This enumerable.</param>
    /// <param name="items">The items.</param>
    public static void RemoveAll<TItem>(this IList<TItem> This, IEnumerable<TItem> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(This != null);
      Contract.Requires(items != null);
#endif

      var removeables = new List<TItem>(items);
      foreach (var item in removeables)
        This.Remove(item);

    }

    // return part 
    public static T[] Splice<T>(this IList<T> @this, int start, int count) {
      var result = new T[count];
      for (var i = count - 1; i >= 0; i--)
        result[i] = @this[i + start];

      return result;
    }

    // swap two elements
    public static void Swap<T>(this IList<T> @this, int i, int j) {
      var tmp = @this[i];
      @this[i] = @this[j];
      @this[j] = tmp;
    }

    // fisher-yates shuffle array
    public static void Shuffle<T>(this IList<T> @this) {
      var i = @this.Count;
      var random = new Random();
      while (i-- > 1)
        @this.Swap(random.Next(i + 1), i);

    }

    public static TOutput[] ConvertAll<TInput, TOutput>(this IList<TInput> @this, Converter<TInput, TOutput> converter)
      => Array.ConvertAll(@this.ToArray(), converter)
    ;

    public static void ForEach<TInput>(this IList<TInput> @this, Action<TInput> action) {
      Array.ForEach(@this.ToArray(), action);
    }

    /// <summary>
    /// Removes items at the given position.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="start">The start.</param>
    /// <param name="count">The count.</param>
    public static void RemoveRange<TInput>(this IList<TInput> @this, int start, int count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif

      // special case I - return when nothing to remove
      if (count < 1)
        return;

      // special case II - only one item removed
      if (count == 1) {
        @this.RemoveAt(start);
        return;
      }

      // special case, given a real List<T>
      var realList = @this as List<TInput>;
      if (realList != null) {
        realList.RemoveRange(start, count);
        return;
      }

      // remove every single item, starting backwards to avoid broken indexes
      for (var i = Math.Min(@this.Count - 1, start + count - 1); i >= start; --i)
        @this.RemoveAt(i);
    }

    /// <summary>
    /// Adds the items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="items">The items.</param>
    public static void AddRange<TInput>(this IList<TInput> @this, IEnumerable<TInput> items) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
      Contract.Requires(items != null);
#endif

      // special case, given a real List<T>
      var realList = @this as List<TInput>;
      if (realList != null) {
        realList.AddRange(items);
        return;
      }

      foreach (var item in items)
        @this.Add(item);
    }

    /// <summary>
    /// Adds if not null.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="item">The item.</param>
    public static void AddIfNotNull<TInput>(this IList<TInput> @this, TInput item) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif

      if (item != null)
        @this.Add(item);
    }

    /// <summary>
    /// Keeps the first n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="count">The count.</param>
    public static void KeepFirst<TInput>(this IList<TInput> @this, int count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif

      // special case: keep nothing
      if (count < 1) {
        @this.Clear();
        return;
      }

      // special case: keep the first element
      if (count == 1) {
        var item = @this[0];
        @this.Clear();
        @this.Add(item);
        return;
      }

      // special case: keep all elements
      var len = @this.Count;
      if (count >= len)
        return;

      var index = count;
      count = len - index;

      if (count > index) {

        // more to remove than to keep, copy the items and clear the list, then re-add
        var copy = new TInput[index];
        for (var i = 0; i < index; ++i)
          copy[i] = @this[i];
        @this.Clear();
        @this.AddRange(copy);
      } else

        // only few elements to remove
        @this.RemoveRange(index, count);
    }

    /// <summary>
    /// Keeps the last n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="count">The count.</param>
    public static void KeepLast<TInput>(this IList<TInput> @this, int count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif

      // special case: remove all items
      if (count < 1) {
        @this.Clear();
        return;
      }

      // special case: keep all elements
      var len = @this.Count;
      if (count > len)
        return;

      // special case: keep last item
      if (count == 1) {
        var item = @this[len - 1];
        @this.Clear();
        @this.Add(item);
        return;
      }

      var index = len - count;
      if (count > index) {

        // more to remove than to keep
        var copy = new TInput[count];
        for (int i = count - 1, j = len - 1; i >= 0; --i, --j)
          copy[i] = @this[j];
        @this.Clear();
        @this.AddRange(copy);
      } else

        // only few elements to remove
        @this.RemoveRange(0, index);
    }

    /// <summary>
    /// Removes the first n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="count">The count.</param>
    public static void RemoveFirst<TInput>(this IList<TInput> @this, int count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      var remaining = @this.Count - count;
      @this.KeepLast(remaining);
    }

    /// <summary>
    /// Removes the last n items.
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <param name="this">This IList.</param>
    /// <param name="count">The count.</param>
    public static void RemoveLast<TInput>(this IList<TInput> @this, int count) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      var remaining = @this.Count - count;
      @this.KeepFirst(remaining);
    }

    /// <summary>
    /// Returns all permutations of the specified items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">The items.</param>
    /// <param name="separateArrays">if set to <c>true</c> returns separate arrays; otherwise, returns the same array changed over and over again.</param>
    /// <returns></returns>
    public static IEnumerable<T[]> Permutate<T>(this IList<T> @this, bool separateArrays = false) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      var length = @this.Count;
      if (length < 1)
        yield break;

      var current = new T[length];
      var state = new int[length];
      for (var i = 0; i < length; ++i)
        current[i] = @this[state[i] = i];

      var lastIndex = length - 1;
      while (true) {

        // return copy or working array
        if (separateArrays) {
          var result = new T[length];
          current.CopyTo(result, 0);
          yield return result;
        } else
          yield return current;

        // increment the 2nd last digit
        var index = lastIndex - 1;
        while (true) {

          // increment as long as there are matching slots
          var slotsBefore = new HashSet<int>(state.Take(index));
          do {
            ++state[index];
          } while (slotsBefore.Contains(state[index]));

          // if we did not ran out of digits
          if (state[index] <= lastIndex)
            break;

          // otherwise, try incrementing the left next slot
          --index;

          // no more slots, all permutations done
          if (index < 0)
            yield break;
        }

        // fill content by digit
        current[index] = @this[state[index]];

        // fill all slots after the incremented one
        for (var i = index + 1; i < length; ++i) {
          state[i] = 0;

          var slotsBefore = new HashSet<int>(state.Take(i));
          while (slotsBefore.Contains(state[i]))
            ++state[i];

          current[i] = @this[state[i]];
        }

      }

    }

    /// <summary>
    /// Returns all permutations of the specified items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this">The items.</param>
    /// <param name="length">The length of each permutation.</param>
    /// <param name="separateArrays">if set to <c>true</c> returns separate arrays; otherwise, returns the same array changed over and over again.</param>
    /// <returns></returns>
    public static IEnumerable<T[]> Permutate<T>(this IList<T> @this, int length, bool separateArrays = false) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
      if (length < 1)
        yield break;

      var itemLastIndex = @this.Count - 1;
      if (itemLastIndex < 0)
        yield break;

      var current = new T[length];
      for (var i = 0; i < length; ++i)
        current[i] = @this[0];

      var states = new int[length];
      --length;

      // this version creates a new array for each permutations and returns it
      if (separateArrays) {
        while (true) {
          var result = new T[length + 1];
          current.CopyTo(result, 0);
          yield return result;

          if (!_ArePermutationsLeft(@this, length, states, itemLastIndex, current))
            yield break;
        }
      }

      while (true) {
        yield return current;

        if (!_ArePermutationsLeft(@this, length, states, itemLastIndex, current))
          yield break;
      }

    }

    /// <summary>
    /// The permutation core.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">The items.</param>
    /// <param name="length">The length.</param>
    /// <param name="states">The states.</param>
    /// <param name="itemLastIndex">Last index of the item.</param>
    /// <param name="current">The current.</param>
    /// <returns><c>true</c> if there are more permutations available; otherwise, <c>false</c>.</returns>
    private static bool _ArePermutationsLeft<T>(IList<T> items, int length, int[] states, int itemLastIndex, T[] current) {

      // set counter position back to last index
      var index = length;
      while (states[index] >= itemLastIndex) {
        states[index] = 0;
        current[index] = items[0];
        --index;

        // all permutations done
        if (index < 0)
          return false;
      }

      // create next permutation
      current[index] = items[++states[index]];

      return true;
    }

#if !NET5_0_OR_GREATER
    public static List<T> DeepClone<T>(this List<T> list) 
    {
      object objResult = null;
      using (var  ms = new MemoryStream())
      {
        var  bf =   new BinaryFormatter();
        bf.Serialize(ms, list);

        ms.Position = 0;
        objResult = bf.Deserialize(ms);
      }
      return (List<T>)objResult;
    }
#endif

  }
}
