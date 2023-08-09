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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MethodLocalStaticValue;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
static class MethodLocal {

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
  
  private static readonly Dictionary<MethodKey, object> _VALUES = new();

  public static TValue Get<TOwner, TValue>(TValue startValue = default, [CallerMemberName] string memberName = null)
    => _Get(typeof(TOwner), memberName, startValue);

  public static TValue Get<TValue>(Type owner, TValue startValue = default, [CallerMemberName] string memberName = null)
    => _Get(owner, memberName, startValue);

  public static TValue Get<TValue>(TValue startValue = default, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null)
    => _Get(null, filePath + memberName, startValue);
  
  private static TValue _Get<TValue>(Type owner, string methodName, TValue startValue) {
    var key = new MethodKey(owner,null, methodName);
    lock (MethodLocal._VALUES)
      if (MethodLocal._VALUES.TryGetValue(key, out var result))
        return (TValue)result;

    return startValue;
  }

  private static void _Set<TValue>(Type owner, string methodName, TValue value) {
    var key = new MethodKey(owner, null, methodName);
    lock (MethodLocal._VALUES)
      MethodLocal._VALUES[key] = value;
  }

  public static void Set<TOwner, TValue>(TValue value, [CallerMemberName] string memberName = null)
    => _Set(typeof(TOwner), memberName, value);

  public static void Set<TValue>(Type owner, TValue value, [CallerMemberName] string memberName = null)
    => _Set(owner, memberName, value);

  public static void Set<TValue>(TValue value, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null)
    => _Set(null, filePath + memberName, value);

}