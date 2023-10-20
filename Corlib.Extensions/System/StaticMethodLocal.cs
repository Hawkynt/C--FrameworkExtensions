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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Guard;

namespace System;

/// <summary>
/// Allows methods to have a private local value that is kept during method invocations
/// </summary>
/// <remarks>
/// All methods within the same source file and the same name share the same value.
/// To make overloads with the same name of different source files share the same name, use the owner parameter.
/// If a source file implements several classes with the same method name, values are shared across classes.
/// </remarks>
/// <typeparam name="TValue">The type of value to keep</typeparam>
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static class StaticMethodLocal<TValue> {

  /// <summary>
  /// The internal key to distinguish values.
  /// </summary>
  private readonly struct MethodKey : IEquatable<MethodKey> {
    private readonly Type _owner;
    private readonly string _filePath;
    private readonly string _methodName;

    public MethodKey(Type owner, string filePath, string methodName) {
      this._owner = owner;
      this._filePath = filePath;
      this._methodName = methodName;
    }

    public bool Equals(MethodKey other)
      => this._owner == other._owner 
        && this._filePath == other._filePath 
        && this._methodName == other._methodName
      ;

    public override bool Equals(object obj) 
      => obj is MethodKey key && this.Equals(key)
      ;
    
    public static bool operator ==(MethodKey left, MethodKey right) => left.Equals(right);

    public static bool operator !=(MethodKey left, MethodKey right) => !left.Equals(right);
    
    /// <inheritdoc />
    public override int GetHashCode() {
      unchecked {
        var hashCode = (this._owner != null ? this._owner.GetHashCode() : 0);
        hashCode = (hashCode * 127) ^ (this._filePath != null ? this._filePath.GetHashCode() : 0);
        hashCode = (hashCode * 257) ^ (this._methodName != null ? this._methodName.GetHashCode() : 0);
        return hashCode;
      }
    }
    
  }
  
  private static readonly Dictionary<MethodKey, TValue> _VALUES = new();

  private static TValue _Get(Type owner, string methodName, TValue startValue) {
    MethodKey key = new(owner, null, methodName);
    lock (_VALUES)
      if (_VALUES.TryGetValue(key, out var result))
        return result;

    return startValue;
  }

  private static TValue _Get(Type owner, string methodName, Func<TValue> startValueFactory) {
    MethodKey key = new(owner, null, methodName);
    lock (_VALUES) {
      if (_VALUES.TryGetValue(key, out var result))
        return result;

      _VALUES.Add(key, result = startValueFactory());
      return result;
    }
  }

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <typeparam name="TOwner">The method owner</typeparam>
  /// <param name="startValue">The initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get<TOwner>(TValue startValue = default, [CallerMemberName] string memberName = null)
    => _Get(typeof(TOwner), memberName, startValue);

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <param name="owner">The method owner</param>
  /// <param name="startValue">The initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get(Type owner, TValue startValue = default, [CallerMemberName] string memberName = null)
    => _Get(owner, memberName, startValue);

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <param name="startValue">The initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <param name="filePath">The source filename of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get(TValue startValue = default, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null)
    => _Get(null, filePath + memberName, startValue);

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <typeparam name="TOwner">The method owner</typeparam>
  /// <param name="startValueFactory">The factory that generates the initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get<TOwner>(Func<TValue> startValueFactory, [CallerMemberName] string memberName = null) {
    Against.ArgumentIsNull(startValueFactory);

    return _Get(typeof(TOwner), memberName, startValueFactory);
  }

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <param name="owner">The method owner</param>
  /// <param name="startValueFactory">The factory that generates the initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get(Type owner, Func<TValue> startValueFactory, [CallerMemberName] string memberName = null) {
    Against.ArgumentIsNull(startValueFactory);

    return _Get(owner, memberName, startValueFactory);
  }

  /// <summary>
  /// Gets the method local static value
  /// </summary>
  /// <param name="startValueFactory">The factory that generates the initial value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <param name="filePath">The source filename of the method; let the compiler fill this</param>
  /// <returns>The current/initial value</returns>
  public static TValue Get(
    Func<TValue> startValueFactory,
    [CallerMemberName] string memberName = null,
    [CallerFilePath] string filePath = null
  ) {
    Against.ArgumentIsNull(startValueFactory);
    
    return _Get(null, filePath + memberName, startValueFactory);
  }

  private static void _Set(Type owner, string methodName, TValue value) {
    MethodKey key = new(owner, null, methodName);
    lock (_VALUES)
      _VALUES[key] = value;
  }

  /// <summary>
  /// Sets the method local static to a new value.
  /// </summary>
  /// <typeparam name="TOwner">The method owner</typeparam>
  /// <param name="value">The new value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  public static void Set<TOwner>(TValue value, [CallerMemberName] string memberName = null)
    => _Set(typeof(TOwner), memberName, value);

  /// <summary>
  /// Sets the method local static to a new value.
  /// </summary>
  /// <param name="owner">The method owner</param>
  /// <param name="value">The new value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  public static void Set(Type owner, TValue value, [CallerMemberName] string memberName = null)
    => _Set(owner, memberName, value);

  /// <summary>
  /// Sets the method local static to a new value.
  /// </summary>
  /// <param name="value">The new value</param>
  /// <param name="memberName">The name of the method; let the compiler fill this</param>
  /// <param name="filePath">The source filename of the method; let the compiler fill this</param>
  public static void Set(TValue value, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null)
    => _Set(null, filePath + memberName, value);

}