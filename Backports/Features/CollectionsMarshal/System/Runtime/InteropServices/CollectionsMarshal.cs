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

// Disable nullable warnings for reflection-heavy code that can't be properly annotated
#nullable disable

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
  public static Span<T> AsSpan<T>(List<T> list) => ListAccessor<T>.AsSpan(list);

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
    public static Span<T> AsSpan(List<T> list) {
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

// Handler selection:
// - NETFRAMEWORK: uses "entries" field + FindEntry method (net20-net48)
// - .NET Core 2.x-3.1: uses "_entries" field + FindEntry method (netcoreapp2.1-netcoreapp3.1)
// - .NET 5.0+: uses FindValue method which returns ref TValue directly (net5.0+)

#if NETFRAMEWORK || !SUPPORTS_COLLECTIONSMARSHAL_ASSPAN

  /// <summary>
  /// Dictionary handler for .NET Framework and .NET Core 2.x-3.1 dictionaries.
  /// Uses Dictionary's internal FindEntry method for O(1) lookup without recomputing hash codes.
  /// Uses a combined FindValuePtr delegate to minimize delegate call overhead.
  /// </summary>
  /// <remarks>
  /// .NET Framework uses "entries" field, .NET Core 2.x-3.1 uses "_entries" field.
  /// Both use FindEntry(TKey) returning int index.
  /// </remarks>
  private readonly struct DictionaryHandler<TKey, TValue> {
    // ReSharper disable StaticMemberInGenericType

    // Combined delegate: FindEntry + GetValuePtr in one call, returns IntPtr.Zero if not found
    private static readonly FindValuePtrDelegate _findValuePtr;

    // ReSharper restore StaticMemberInGenericType

    private delegate IntPtr FindValuePtrDelegate(Dictionary<TKey, TValue> dictionary, TKey key);

    static DictionaryHandler() {
      var dictType = typeof(Dictionary<TKey, TValue>);
      const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
      const BindingFlags entryFlags = flags | BindingFlags.Public;

      // .NET Framework uses "entries", .NET Core uses "_entries"
      var entriesField = (dictType.GetField("_entries", flags) ?? dictType.GetField("entries", flags)) ?? throw new InvalidOperationException("Cannot find entries field in Dictionary.");
      var entryType = entriesField.FieldType.GetElementType() ?? throw new InvalidOperationException("Cannot get Entry element type.");
      var entryValueField = entryType.GetField("value", entryFlags) ?? throw new InvalidOperationException("Cannot find 'value' field in Entry.");

      // Get the private FindEntry method - this does the hash lookup internally
      var findEntryMethod = dictType.GetMethod("FindEntry", flags) ?? throw new InvalidOperationException("Cannot find 'FindEntry' method in Dictionary.");

      _findValuePtr = CreateFindValuePtrDelegate(entriesField, entryType, entryValueField, findEntryMethod);
    }

    /// <summary>
    /// Creates a combined delegate that calls FindEntry and returns pointer to value field.
    /// IL equivalent:
    ///   int index = dictionary.FindEntry(key);
    ///   if (index &lt; 0) return IntPtr.Zero;
    ///   return (IntPtr)(&amp;dictionary.entries[index].value);
    /// </summary>
    private static FindValuePtrDelegate CreateFindValuePtrDelegate(FieldInfo entriesField, Type entryType, FieldInfo valueField, MethodInfo findEntryMethod) {
      var method = new DynamicMethod("FindValuePtr", typeof(IntPtr), [typeof(Dictionary<TKey, TValue>), typeof(TKey)], typeof(DictionaryHandler<TKey, TValue>), skipVisibility: true);
      var il = method.GetILGenerator();

      var indexLocal = il.DeclareLocal(typeof(int));
      var notFoundLabel = il.DefineLabel();

      // int index = dictionary.FindEntry(key);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Call, findEntryMethod);
      il.Emit(OpCodes.Stloc, indexLocal);

      // if (index < 0) return IntPtr.Zero;
      il.Emit(OpCodes.Ldloc, indexLocal);
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Blt, notFoundLabel);

      // return (IntPtr)(&dictionary.entries[index].value);
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldfld, entriesField);
      il.Emit(OpCodes.Ldloc, indexLocal);
      il.Emit(OpCodes.Ldelema, entryType);
      il.Emit(OpCodes.Ldflda, valueField);
      il.Emit(OpCodes.Conv_I);
      il.Emit(OpCodes.Ret);

      // notFoundLabel: return IntPtr.Zero;
      il.MarkLabel(notFoundLabel);
      il.Emit(OpCodes.Ldc_I4_0);
      il.Emit(OpCodes.Conv_I);
      il.Emit(OpCodes.Ret);

      return (FindValuePtrDelegate)method.CreateDelegate(typeof(FindValuePtrDelegate));
    }

    /// <summary>
    /// Finds the value pointer for a key in one delegate call.
    /// Returns IntPtr.Zero if key not found, otherwise pointer to value field.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr FindValuePtr(Dictionary<TKey, TValue> dictionary, TKey key)
      => _findValuePtr(dictionary, key);
  }

#else

  /// <summary>
  /// Dictionary handler for .NET 5.0+ dictionaries.
  /// Uses Dictionary's internal FindValue method which returns ref TValue directly.
  /// No IL emit needed - uses open instance delegate only.
  /// </summary>
  private readonly struct DictionaryHandler<TKey, TValue> {
    // ReSharper disable StaticMemberInGenericType

    // Open instance delegate that returns ref TValue directly - no IL emit needed
    private static readonly FindValueDelegate _findValue;

    // ReSharper restore StaticMemberInGenericType

    private delegate ref TValue FindValueDelegate(Dictionary<TKey, TValue> dictionary, TKey key);

    static DictionaryHandler() {
      var dictType = typeof(Dictionary<TKey, TValue>);
      const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

      // Get the private FindValue method - returns ref TValue directly (or null ref if not found)
      var findValueMethod = dictType.GetMethod("FindValue", flags) ?? throw new InvalidOperationException("Cannot find 'FindValue' method in Dictionary.");

      // Create open instance delegate directly - no DynamicMethod needed!
      _findValue = (FindValueDelegate)Delegate.CreateDelegate(typeof(FindValueDelegate), findValueMethod);
    }

    /// <summary>
    /// Finds the value pointer for a key using direct delegate call + unsafe pointer conversion.
    /// Returns IntPtr.Zero if key not found, otherwise pointer to value field.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IntPtr FindValuePtr(Dictionary<TKey, TValue> dictionary, TKey key) {
      ref var valueRef = ref _findValue(dictionary, key);
      if (Unsafe.IsNullRef(ref valueRef))
        return IntPtr.Zero;

      unsafe {
        return (IntPtr)Unsafe.AsPointer(ref valueRef);
      }
    }
  }

#endif

  /// <summary>
  /// Provides access to Dictionary internals using reflection and IL emit.
  /// Handler is selected at compile time based on target framework.
  /// Uses a combined FindValuePtr delegate for single-call lookup (1 hash, 1 delegate call).
  /// </summary>
  private static class DictionaryAccessor<TKey, TValue> {
    // ReSharper disable StaticMemberInGenericType
    private static readonly DictionaryHandler<TKey, TValue> _handler = new();
    // ReSharper restore StaticMemberInGenericType

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrAddDefault(Dictionary<TKey, TValue> dictionary, TKey key, out bool exists) {
      // Single delegate call: FindEntry + get value pointer combined
      var ptr = _handler.FindValuePtr(dictionary, key);
      if (ptr != IntPtr.Zero) {
        // Key exists - return ref directly (1 hash lookup, 1 delegate call)
        exists = true;
        unsafe {
          return ref *(TValue*)ptr.ToPointer();
        }
      }

      // Key not found - add it with default value
      exists = false;
      dictionary[key] = default;

      // Now get the ref to the newly added value (1 more hash lookup)
      ptr = _handler.FindValuePtr(dictionary, key);
      if (ptr == IntPtr.Zero)
        throw new InvalidOperationException("Entry not found after adding to dictionary.");

      unsafe {
        return ref *(TValue*)ptr.ToPointer();
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref TValue GetValueRefOrNullRef(Dictionary<TKey, TValue> dictionary, TKey key) {
      // Single delegate call: FindEntry + get value pointer combined
      var ptr = _handler.FindValuePtr(dictionary, key);
      if (ptr == IntPtr.Zero) {
        // Return null ref - works on all frameworks
        unsafe {
          return ref Unsafe.AsRef<TValue>(null);
        }
      }

      unsafe {
        return ref *(TValue*)ptr.ToPointer();
      }
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
