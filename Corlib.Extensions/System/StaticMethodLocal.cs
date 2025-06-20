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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Guard;

namespace System;

file static class StaticLocalStorage {
  
  // A thread-safe, global cache for static local variables.
  // The key is a ValueTuple, which avoids heap allocation on every call, making it highly performant.
  // The string part of the key is the interned file path provided by the compiler.
  private static readonly ConcurrentDictionary<(string, int), object> _locals = new();

  public static StaticMethodLocal.Storage<T> GetOrAdd<T>(Func<StaticMethodLocal.Storage<T>> valueFactory, string path, int line) {
    // Using a ValueTuple as the key is allocation-free and highly performant.
    var cacheKey = (path, line);
    var value = _locals.GetOrAdd(cacheKey, _ => valueFactory());
    return (StaticMethodLocal.Storage<T>)value;
  }

}

/// <summary>
/// The internal value holder for static method-local values.
/// </summary>
public static class StaticMethodLocal {

  /// <summary>
  /// A generic, reusable container to hold a value.
  /// This class is sealed and its constructor is private to ensure it can only be created
  /// and used by the parent StaticLocal class.
  /// </summary>
  public sealed class Storage<T>: IFormattable, IEquatable<Storage<T>>, IEquatable<T>, IComparable<Storage<T>>, IComparable<T> {
    private T _reference;

    /// <summary>
    /// The constructor is private, forcing creation through StaticLocal.GetOrAdd.
    /// </summary>
    private Storage(T value) => this._reference = value;

    /// <summary>
    /// Provides direct, modifiable reference access to the underlying value.
    /// C# 7.0+ feature. This is the intended way to modify the stored value.
    /// </summary>
    public ref T Ref => ref this._reference;

    /// <summary>
    /// Allows the holder to be used for read-only access as if it were the value itself.
    /// Example: int myInt = myHolder;
    /// </summary>
    public static implicit operator T(Storage<T> storage) => storage._reference;

    /// <summary>
    /// Creates a Holder{T} from a value of type T. This operator can access the private
    /// constructor and is the key to allowing the factory lambda to create instances.
    /// </summary>
    public static implicit operator Storage<T>(T value) => new(value);

    #region Overrides and Interface Implementations
    
    /// <inheritdoc/>
    public override string ToString() => this._reference?.ToString() ?? "<null>";

    /// <summary>
    /// Formats the value of the current instance using the specified format.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) =>
        this._reference is IFormattable formattable
            ? formattable.ToString(format, formatProvider)
            : this.ToString();

    /// <inheritdoc/>
    public override int GetHashCode() => this._reference?.GetHashCode() ?? 0;

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
        obj is Storage<T> holder && this.Equals(holder) ||
        obj is T value && this.Equals(value);

    /// <inheritdoc/>
    public bool Equals(Storage<T> other) => other is not null && (ReferenceEquals(this, other) || EqualityComparer<T>.Default.Equals(this._reference, other._reference));

    /// <inheritdoc/>
    public bool Equals(T other) => EqualityComparer<T>.Default.Equals(this._reference, other);

    /// <inheritdoc/>
    public int CompareTo(Storage<T> other) => other is null ? 1 : this.CompareTo(other._reference);

    /// <inheritdoc/>
    public int CompareTo(T other) => this._reference switch {
      IComparable<T> comparable => comparable.CompareTo(other),
      IComparable legacyComparable => legacyComparable.CompareTo(other),
      _ => AlwaysThrow.InvalidOperationException<int>($"The underlying type {typeof(T)} does not implement IComparable.")
    };

    #endregion

    #region Operator Overloads

    // Holder vs Holder

    /// <summary>
    /// Determines whether two <see cref="Storage{T}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if both instances are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Storage<T> left, Storage<T> right) =>
      left?.Equals(right) ?? right is null;

    /// <summary>
    /// Determines whether two <see cref="Storage{T}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if both instances are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Storage<T> left, Storage<T> right) => !(left == right);

    /// <summary>
    /// Determines whether one <see cref="Storage{T}"/> instance is less than another.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Storage<T> left, Storage<T> right) =>
      left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one <see cref="Storage{T}"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Storage<T> left, Storage<T> right) =>
      left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one <see cref="Storage{T}"/> instance is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Storage<T> left, Storage<T> right) =>
      left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one <see cref="Storage{T}"/> instance is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The second <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Storage<T> left, Storage<T> right) =>
      left is null ? right is null : left.CompareTo(right) >= 0;

    // Holder vs T

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is equal to a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Storage<T> left, T right) => left is not null && left.Equals(right);

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is not equal to a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(Storage<T> left, T right) => !(left == right);

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is less than a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if less than; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(Storage<T> left, T right) => left is not null && left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is less than or equal to a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if less than or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(Storage<T> left, T right) => left is not null && left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is greater than a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if greater than; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(Storage<T> left, T right) => left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether a <see cref="Storage{T}"/> instance is greater than or equal to a raw value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="left">The <see cref="Storage{T}"/> instance.</param>
    /// <param name="right">The raw value to compare.</param>
    /// <returns><see langword="true"/> if greater than or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(Storage<T> left, T right) => left is not null && left.CompareTo(right) >= 0;

    // T vs Holder

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is equal to a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(T left, Storage<T> right) => right is not null && right.Equals(left);

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is not equal to a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(T left, Storage<T> right) => !(left == right);

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is less than a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if less than; otherwise, <see langword="false"/>.</returns>
    public static bool operator <(T left, Storage<T> right) => right is not null && right.CompareTo(left) > 0;

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is less than or equal to a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if less than or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator <=(T left, Storage<T> right) => right is not null && right.CompareTo(left) >= 0;

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is greater than a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if greater than; otherwise, <see langword="false"/>.</returns>
    public static bool operator >(T left, Storage<T> right) => right is not null && right.CompareTo(left) < 0;

    /// <summary>
    /// Determines whether a raw value of type <typeparamref name="T"/> is greater than or equal to a <see cref="Storage{T}"/> instance.
    /// </summary>
    /// <param name="left">The raw value.</param>
    /// <param name="right">The <see cref="Storage{T}"/> instance.</param>
    /// <returns><see langword="true"/> if greater than or equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator >=(T left, Storage<T> right) => right is not null && right.CompareTo(left) <= 0;

    #endregion


  }
}

/// <summary>
///   Allows methods to have a private local value that is kept during method invocations
/// </summary>
/// <remarks>
///   All methods within the same source file and the same name share the same value.
///   To make overloads with the same name of different source files share the same name, use the owner parameter.
///   If a source file implements several classes with the same method name, values are shared across classes.
/// </remarks>
/// <typeparam name="TValue">The type of value to keep</typeparam>
public static class StaticMethodLocal<TValue> {

  /// <summary>
  /// Gets a persistent holder for a method-scoped static value, initializing it with default(TValue) on its first use.
  /// The value is unique to the source code location where this method is called.
  /// </summary>
  /// <param name="path">The source code location filename where the value is defined; filled by compiler - leave empty!</param>
  /// <param name="line">The source code location line number where the value is defined; filled by compiler - leave empty!</param>
  /// <example>
  /// This example shows a simple counter that increments on each call.
  /// <code>
  /// public void MyMethod() {
  ///     var callCounter = StaticMethodLocal&lt;int&gt;.GetOrAdd();
  ///     callCounter.Ref++;
  ///     Console.WriteLine($"This method has been called {callCounter.Ref} times.");
  /// }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAdd([CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => default(TValue), path, line);

  /// <summary>
  /// Gets a persistent holder for a method-scoped static value, initializing it with a specified default value on its first use.
  /// The value is unique to the source code location where this method is called.
  /// </summary>
  /// <param name="defaultValue">The value to use if the static local has not yet been initialized.</param>
  /// <param name="path">The source code location filename where the value is defined; filled by compiler - leave empty!</param>
  /// <param name="line">The source code location line number where the value is defined; filled by compiler - leave empty!</param>
  /// <example>
  /// <code>
  /// public void StartCountingFrom100() {
  ///     var counter = StaticMethodLocal&lt;int&gt;.GetOrAdd(100);
  ///     Console.WriteLine($"Current counter {callCounter.Ref++}.");
  /// }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAdd(TValue defaultValue, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => defaultValue, path, line);

  /// <summary>
  /// Gets a persistent holder for a method-scoped static value, using a factory function for initialization on its first use.
  /// The factory is only executed once. The value is unique to the source code location where this method is called.
  /// </summary>
  /// <param name="valueFactory">A function that creates the initial value.</param>
  /// <param name="path">The source code location filename where the value is defined; filled by compiler - leave empty!</param>
  /// <param name="line">The source code location line number where the value is defined; filled by compiler - leave empty!</param>
  /// <example>
  /// This is useful for lazy initialization of expensive objects.
  /// <code>
  /// public void LogTimestamp() {
  ///     // The DateTime.Now is only evaluated the very first time LogTimestamp is called.
  ///     var creationTime = StaticMethodLocal&lt;DateTime&gt;.GetOrAdd(() => DateTime.Now);
  ///     Console.WriteLine($"This method was first called at {creationTime}.");
  /// }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAdd(Func<TValue> valueFactory, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => valueFactory(), path, line);

  /// <summary>
  /// Gets a persistent holder for a static value identified by a specific name, initializing it with default(TValue).
  /// </summary>
  /// <remarks>
  /// This is useful for sharing a static value between different method overloads or locations without relying on source position.
  /// All calls to GetOrAddByName with the same name will access the same value.
  /// </remarks>
  /// <param name="name">The unique key to identify the static value.</param>
  /// <example>
  /// <code>
  /// public void MethodA() { StaticMethodLocal&lt;int&gt;.GetOrAddByName(nameof(MethodA)).Ref++; }
  /// public void MethodB() { Console.WriteLine(StaticMethodLocal&lt;int&gt;.GetOrAddByName(nameof(MethodA))); }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAddByName(string name) => StaticLocalStorage.GetOrAdd<TValue>(() => default(TValue), name, -1);

  /// <summary>
  /// Gets a persistent holder for a static value identified by a specific name, initializing it with a given value.
  /// </summary>
  /// <remarks>
  /// This is useful for sharing a static value between different method overloads or locations without relying on source position.
  /// All calls to GetOrAddByName with the same name will access the same value.
  /// </remarks>
  /// <param name="name">The unique key to identify the static value.</param>
  /// <param name="defaultValue">The initialization value to use</param>
  /// <example>
  /// <code>
  /// public void MethodA() { StaticMethodLocal&lt;int&gt;.GetOrAddByName(nameof(MethodA)).Ref++; }
  /// public void MethodB() { Console.WriteLine(StaticMethodLocal&lt;int&gt;.GetOrAddByName(nameof(MethodA))); }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAddByName(string name, TValue defaultValue) => StaticLocalStorage.GetOrAdd<TValue>(() => defaultValue, name, -1);

  /// <summary>
  /// Gets a persistent holder for a static value identified by a specific name, using a factory function for initialization.
  /// </summary>
  /// <remarks>
  /// This is useful for sharing a static value between different methods or locations without relying on source position.
  /// All calls to GetOrAddByName with the same name will access the same value. The factory is only executed once per name.
  /// </remarks>
  /// <param name="name">The unique key to identify the static value.</param>
  /// <param name="valueFactory">A function that creates the initial value. It will only be executed once per name.</param>
  /// <example>
  /// This pattern is more powerful than a simple `Lazy&lt;T&gt;` because it allows different methods to access the same lazily-initialized instance without needing access to a shared static field.
  /// <code>
  /// int ReadCounterFromFile() {
  ///     // In a real scenario, you would read and parse the file.
  ///     // This is just a placeholder for the example.
  ///     Console.WriteLine($"--- Reading initial counter value from file... ---");
  ///     return 100;
  /// }
  ///
  /// // This method might be in one part of the application.
  /// public void IncrementSharedCounter() {
  ///     // The counter's initial value is read from the file only on the very first call,
  ///     // either here or in another method that uses the same name "SessionCounter".
  ///     var counter = StaticMethodLocal&lt;int&gt;.GetOrAddByName("SessionCounter", ReadCounterFromFile);
  ///     counter.Ref++;
  ///     Console.WriteLine($"Counter is now: {counter}");
  /// }
  ///
  /// // This method could be in a completely different class.
  /// public void DisplaySharedCounter() {
  ///     // It accesses the *same* counter instance. If IncrementSharedCounter() was called first,
  ///     // this will not re-read the file.
  ///     var counter = StaticMethodLocal&lt;int&gt;.GetOrAddByName("SessionCounter", ReadCounterFromFile);
  ///     Console.WriteLine($"Reading counter from another method. Current value: {counter}");
  /// }
  /// </code>
  /// </example>
  public static StaticMethodLocal.Storage<TValue> GetOrAddByName(string name, Func<TValue> valueFactory) => StaticLocalStorage.GetOrAdd<TValue>(() => valueFactory(), name, -1);

}
