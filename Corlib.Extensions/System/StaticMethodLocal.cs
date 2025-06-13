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
using System.Runtime.CompilerServices;

namespace System;

file static class StaticLocalStorage {
  
  // A thread-safe, global cache for static local variables.
  // The key is a ValueTuple, which avoids heap allocation on every call, making it highly performant.
  // The string part of the key is the interned file path provided by the compiler.
  private static readonly ConcurrentDictionary<(string, int), object> _locals = new();

  public static StaticMethodLocal.Holder<T> GetOrAdd<T>(Func<StaticMethodLocal.Holder<T>> valueFactory, string path, int line) {
    // Using a ValueTuple as the key is allocation-free and highly performant.
    var cacheKey = (path, line);
    var value = _locals.GetOrAdd(cacheKey, _ => valueFactory());
    return (StaticMethodLocal.Holder<T>)value;
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
  public sealed class Holder<T>: IFormattable {
    private T _reference;

    /// <summary>
    /// The constructor is private, forcing creation through StaticLocal.GetOrAdd.
    /// </summary>
    private Holder(T value) => this._reference = value;

    /// <summary>
    /// Provides direct, modifiable reference access to the underlying value.
    /// C# 7.0+ feature. This is the intended way to modify the stored value.
    /// </summary>
    public ref T Ref => ref this._reference;

    /// <summary>
    /// Allows the holder to be used for read-only access as if it were the value itself.
    /// Example: int myInt = myHolder;
    /// </summary>
    public static implicit operator T(Holder<T> holder) => holder._reference;

    /// <summary>
    /// Creates a Holder<T> from a value of type T. This operator can access the private
    /// constructor and is the key to allowing the factory lambda to create instances.
    /// </summary>
    public static implicit operator Holder<T>(T value) => new(value);

    /// <inheritdoc />
    public override string ToString() => this._reference?.ToString() ?? "<null>";

    /// <summary>
    /// Formats the value of the current instance using the specified format.
    /// </summary>
    public string ToString(string format, IFormatProvider formatProvider) =>
      this._reference is IFormattable formattable
        // If the underlying type supports IFormattable, pass the format down.
        ? formattable.ToString(format, formatProvider)
        // Otherwise, fall back to the default ToString().
        : this.ToString()
      ;

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
  /// Gets a persistent holder for a method-scoped static value type using a factory for initialization.
  /// This overload automatically wraps the result of the value factory in a Holder.
  /// </summary>
  public static StaticMethodLocal.Holder<TValue> GetOrAdd(Func<TValue> valueFactory, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => valueFactory(), path, line);

  /// <summary>
  /// Gets a persistent holder for a method-scoped static value type, initializing it with a default value.
  /// </summary>
  public static StaticMethodLocal.Holder<TValue> GetOrAdd(TValue defaultValue, [CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => defaultValue, path, line);

  /// <summary>
  /// Gets a persistent holder for a method-scoped static value type, initializing it with default(T).
  /// </summary>
  public static StaticMethodLocal.Holder<TValue> GetOrAdd([CallerFilePath] string path = null, [CallerLineNumber] int line = 0) => StaticLocalStorage.GetOrAdd<TValue>(() => default(TValue), path, line);

}
