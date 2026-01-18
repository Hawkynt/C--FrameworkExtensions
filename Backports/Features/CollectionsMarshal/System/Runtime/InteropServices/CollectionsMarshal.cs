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

// CollectionsMarshal API evolution:
// - net5.0: AsSpan (SUPPORTS_COLLECTIONSMARSHAL_ASSPAN)
// - net6.0: GetValueRefOrAddDefault, GetValueRefOrNullRef (SUPPORTS_COLLECTIONSMARSHAL_GETVALUEREFORADDDEFAULT)
// - net8.0: SetCount (SUPPORTS_COLLECTIONSMARSHAL_SETCOUNT)
//
// Wave architecture (no nesting):
// - Wave 1: Minimal polyfill class with ONLY AsSpan (!SUPPORTS_COLLECTIONSMARSHAL_ASSPAN)
// - Wave 2: Extension block for GetValueRefOrAddDefault/GetValueRefOrNullRef (!SUPPORTS_COLLECTIONSMARSHAL_GETVALUEREFORADDDEFAULT)
// - Wave 3: Extension block for SetCount (!SUPPORTS_COLLECTIONSMARSHAL_SETCOUNT)
//
// Extension blocks work on BOTH polyfill and native CollectionsMarshal classes.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Runtime.InteropServices;

// Wave 1: Minimal CollectionsMarshal polyfill with ONLY AsSpan (for net20-netcoreapp3.1)
#if !SUPPORTS_COLLECTIONSMARSHAL_ASSPAN

/// <summary>
/// Provides methods for interoperating with collection types.
/// </summary>
/// <remarks>
/// This is a polyfill for <see cref="CollectionsMarshal"/> which was introduced in .NET 5.
/// </remarks>
public static class CollectionsMarshal {

  /// <summary>
  /// Gets a <see cref="Span{T}"/> view over the data in a list.
  /// Items should not be added or removed from the <see cref="List{T}"/> while the <see cref="Span{T}"/> is in use.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the list.</typeparam>
  /// <param name="list">The list to get the data view over.</param>
  /// <returns>A <see cref="Span{T}"/> instance over the <see cref="List{T}"/>.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Span<T> AsSpan<T>(List<T>? list) => ListAccessor<T>.AsSpan(list);

  /// <summary>
  /// Provides access to List internals using reflection.
  /// </summary>
  private static class ListAccessor<T> {
    // ReSharper disable StaticMemberInGenericType
    private static readonly FieldInfo _itemsField;
    private static readonly FieldInfo _sizeField;
    // ReSharper restore StaticMemberInGenericType

    static ListAccessor() {
      var listType = typeof(List<T>);
      const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

      // Get the internal items array field (handle different naming conventions)
      _itemsField = (listType.GetField("_items", flags) ?? listType.GetField("items", flags)) ?? throw new InvalidOperationException("Cannot find List internal items field.");
      _sizeField = (listType.GetField("_size", flags) ?? listType.GetField("size", flags)) ?? throw new InvalidOperationException("Cannot find List internal size field.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan(List<T>? list) {
      if (list == null)
        return Span<T>.Empty;

      var items = (T[])_itemsField.GetValue(list);
      var count = (int)_sizeField.GetValue(list);
      return new(items, 0, count);
    }
  }
}

#endif

// Wave 2: Extension block for GetValueRefOrAddDefault/GetValueRefOrNullRef
// Applies to ALL frameworks without native support (net20-net5.0)
// Works on both polyfill CollectionsMarshal and native net5.0 CollectionsMarshal
#if !SUPPORTS_COLLECTIONSMARSHAL_GETVALUEREFORADDDEFAULT

public static partial class CollectionsMarshalPolyfills {

  extension(CollectionsMarshal) {
    /// <summary>
    /// Gets a reference to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey,TValue}"/>,
    /// adding a new entry with a default value if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used to get the ref to <typeparamref name="TValue"/>.</param>
    /// <param name="exists">Whether the key existed in the dictionary.</param>
    /// <returns>A reference to a <typeparamref name="TValue"/> in the dictionary.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrAddDefault<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, out bool exists)
      => ref DictionaryAccessor<TKey, TValue>.GetValueRefOrAddDefault(dictionary, key, out exists);

    /// <summary>
    /// Gets a reference to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey,TValue}"/>,
    /// or a null reference if the key does not exist.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used to get the ref to <typeparamref name="TValue"/>.</param>
    /// <returns>A reference to a <typeparamref name="TValue"/> in the dictionary, or a null reference if not found.</returns>
    /// <remarks>
    /// Items should not be added or removed from the <see cref="Dictionary{TKey,TValue}"/> while the ref <typeparamref name="TValue"/> is in use.
    /// Use <see cref="Unsafe.IsNullRef{T}(ref T)"/> to determine if the returned reference is null.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key)
      => ref DictionaryAccessor<TKey, TValue>.GetValueRefOrNullRef(dictionary, key);
  }

  /// <summary>
  /// Provides access to Dictionary internals using reflection and IL emit.
  /// </summary>
  private static class DictionaryAccessor<TKey, TValue> {
    // ReSharper disable StaticMemberInGenericType
    private static readonly FieldInfo _entriesField;
    private static readonly Type _entryType;
    private static readonly FieldInfo _entryHashCodeField;
    private static readonly FieldInfo _entryKeyField;
    private static readonly FieldInfo _entryValueField;
    private static readonly IEqualityComparer<TKey> _comparer;
    // ReSharper restore StaticMemberInGenericType

    // Delegate that returns pointer to value field in entries array
    private delegate IntPtr GetValuePtrDelegate(Array entries, int index);
    private static readonly GetValuePtrDelegate _getValuePtr;

    static DictionaryAccessor() {
      var dictType = typeof(Dictionary<TKey, TValue>);
      const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

      // Get dictionary fields (handle both .NET Framework and .NET Core naming)
      _entriesField = (dictType.GetField("entries", flags) ?? dictType.GetField("_entries", flags)) ?? throw new InvalidOperationException();

      // Get Entry type and its fields
      _entryType = _entriesField.FieldType.GetElementType() ?? throw new InvalidOperationException();
      _entryHashCodeField = (_entryType.GetField("hashCode", flags | BindingFlags.Public) ?? _entryType.GetField("_hashCode", flags | BindingFlags.Public)) ?? throw new InvalidOperationException();
      _entryKeyField = (_entryType.GetField("key", flags | BindingFlags.Public) ?? _entryType.GetField("_key", flags | BindingFlags.Public)) ?? throw new InvalidOperationException();
      _entryValueField = (_entryType.GetField("value", flags | BindingFlags.Public) ?? _entryType.GetField("_value", flags | BindingFlags.Public)) ?? throw new InvalidOperationException();

      // Cache default comparer
      _comparer = EqualityComparer<TKey>.Default;

      // Create dynamic method to get pointer to value field
      _getValuePtr = CreateGetValuePtrDelegate();
    }

    private static GetValuePtrDelegate CreateGetValuePtrDelegate() {
      // DynamicMethod can return IntPtr (pointer), which we then convert to ref
      var entriesArrayType = _entriesField.FieldType; // Entry[]

      var method = new DynamicMethod(
        "GetValuePtr",
        typeof(IntPtr),
        [typeof(Array), typeof(int)],
        typeof(DictionaryAccessor<TKey, TValue>),
        skipVisibility: true
      );

      var il = method.GetILGenerator();

      // Load and cast the array to Entry[]
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Castclass, entriesArrayType);

      // Load the index
      il.Emit(OpCodes.Ldarg_1);

      // ldelema - load element address (gets pointer to Entry at index)
      il.Emit(OpCodes.Ldelema, _entryType);

      // ldflda - load field address (gets pointer to value field within Entry)
      il.Emit(OpCodes.Ldflda, _entryValueField);

      // conv.i - convert managed pointer to native int (IntPtr)
      il.Emit(OpCodes.Conv_I);

      // Return the IntPtr
      il.Emit(OpCodes.Ret);

      return (GetValuePtrDelegate)method.CreateDelegate(typeof(GetValuePtrDelegate));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrAddDefault(Dictionary<TKey, TValue> dictionary, TKey key, out bool exists) {
      // Ensure key exists in dictionary
      exists = dictionary.ContainsKey(key);
      if (!exists)
        dictionary[key] = default;

      // Get the entries array
      var entries = (Array)_entriesField.GetValue(dictionary);

      // Find the entry index
      var index = FindEntryIndex(key, entries);
      if (index < 0)
        throw new InvalidOperationException("Entry not found after adding to dictionary.");

      // Get pointer to value field and convert to ref
      // Note: The returned ref must be used before any dictionary modification
      // that could cause a resize (which reallocates the entries array)
      var ptr = _getValuePtr(entries, index);

      unsafe {
        return ref *(TValue*)ptr.ToPointer();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrNullRef(Dictionary<TKey, TValue> dictionary, TKey key) {
      // Get the entries array
      var entries = (Array)_entriesField.GetValue(dictionary);

      // Find the entry index
      var index = FindEntryIndex(key, entries);
      if (index < 0) {
        // Return null ref - works on all frameworks
        unsafe {
          return ref Unsafe.AsRef<TValue>(null);
        }
      }

      // Get pointer to value field and convert to ref
      var ptr = _getValuePtr(entries, index);

      unsafe {
        return ref *(TValue*)ptr.ToPointer();
      }
    }

    private static int FindEntryIndex(TKey key, Array entries) {
      // Simple linear search through entries - more robust than bucket lookup
      // since dictionary internal structure may vary across .NET versions
      var count = entries.Length;
      for (var i = 0; i < count; ++i) {
        var entry = entries.GetValue(i);
        if (entry == null)
          continue;

        // Check if this entry has a valid hashCode (non-negative means occupied)
        var hashCode = (int)_entryHashCodeField.GetValue(entry);
        if (hashCode < 0)
          continue;

        // Check if the key matches
        var entryKey = (TKey)_entryKeyField.GetValue(entry);
        if (_comparer.Equals(entryKey, key))
          return i;
      }

      return -1;
    }
  }
}

#endif

// Wave 3: Extension block for SetCount
// Applies to ALL frameworks without native support (net20-net7.0)
// Works on both polyfill CollectionsMarshal and native net5.0/net6.0/net7.0 CollectionsMarshal
#if !SUPPORTS_COLLECTIONSMARSHAL_SETCOUNT

public static partial class CollectionsMarshalPolyfills {

  extension(CollectionsMarshal) {
    /// <summary>
    /// Sets the count of the <see cref="List{T}"/> to the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    /// <param name="list">The list to set the count of.</param>
    /// <param name="count">The value to set the count to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative or greater than the capacity of the list.</exception>
    /// <remarks>
    /// This method does not clear elements when shrinking. If the list contains references, this may prevent garbage collection of those objects.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCount<T>(List<T> list, int count)
      => ListAccessor<T>.SetCount(list, count);
  }

  /// <summary>
  /// Provides access to List internals using reflection.
  /// </summary>
  private static class ListAccessor<T> {
    // ReSharper disable StaticMemberInGenericType
    private static readonly FieldInfo _sizeField;
    // ReSharper restore StaticMemberInGenericType

    static ListAccessor() {
      var listType = typeof(List<T>);
      const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

      _sizeField = (listType.GetField("_size", flags) ?? listType.GetField("size", flags)) ?? throw new InvalidOperationException("Cannot find List internal size field.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCount(List<T> list, int count) {
      // Note: We don't explicitly check for null - we let NullReferenceException propagate
      // to match native CollectionsMarshal behavior
      ArgumentOutOfRangeException.ThrowIfNegative(count);

      // Expand capacity if needed (matches native behavior)
      if (count > list.Capacity)
        list.Capacity = count;

      _sizeField.SetValue(list, count);
    }
  }
}

#endif
