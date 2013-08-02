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
namespace System.Runtime.Serialization {
  internal static partial class SerializationInfoExtensions {
    /// <summary>
    /// Reads a value of the given type from the serialization stream.
    /// </summary>
    /// <typeparam name="TType">The type of object to read.</typeparam>
    /// <param name="This">This SerializationInfo.</param>
    /// <param name="name">The name of the key to get the value from.</param>
    /// <returns>The value.</returns>
    public static TType GetValue<TType>(this SerializationInfo This, string name) {
      return ((TType)This.GetValue(name, typeof(TType)));
    }

    /// <summary>
    /// Determines whether the specified key exists.
    /// </summary>
    /// <param name="This">This SerializationInfo.</param>
    /// <param name="name">The name of the key.</param>
    /// <returns>
    ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.
    /// </returns>
    public static bool ContainsKey(this SerializationInfo This, string name) {
      foreach (var entry in This) {
        if (entry.Name == name)
          return (true);
      }
      return (false);
    }

    #region get key or default value
    #region signed ints
    public static sbyte GetValueOrDefault(this SerializationInfo This, string name, sbyte defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetSByte(name) : defaultValue);
    }

    public static Int16 GetValueOrDefault(this SerializationInfo This, string name, Int16 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetInt16(name) : defaultValue);
    }

    public static Int32 GetValueOrDefault(this SerializationInfo This, string name, Int32 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetInt32(name) : defaultValue);
    }

    public static Int64 GetValueOrDefault(this SerializationInfo This, string name, Int64 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetInt64(name) : defaultValue);
    }
    #endregion
    #region unsigned ints
    public static byte GetValueOrDefault(this SerializationInfo This, string name, byte defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetByte(name) : defaultValue);
    }

    public static UInt16 GetValueOrDefault(this SerializationInfo This, string name, UInt16 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetUInt16(name) : defaultValue);
    }

    public static UInt32 GetValueOrDefault(this SerializationInfo This, string name, UInt32 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetUInt32(name) : defaultValue);
    }

    public static UInt64 GetValueOrDefault(this SerializationInfo This, string name, UInt64 defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetUInt64(name) : defaultValue);
    }
    #endregion
    #region numbers
    public static float GetValueOrDefault(this SerializationInfo This, string name, float defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetSingle(name) : defaultValue);
    }

    public static double GetValueOrDefault(this SerializationInfo This, string name, double defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetDouble(name) : defaultValue);
    }

    public static decimal GetValueOrDefault(this SerializationInfo This, string name, decimal defaultValue = 0) {
      return (This.ContainsKey(name) ? This.GetDecimal(name) : defaultValue);
    }
    #endregion
    #region bools
    public static bool GetValueOrDefault(this SerializationInfo This, string name, bool defaultValue = false) {
      return (This.ContainsKey(name) ? This.GetBoolean(name) : defaultValue);
    }
    #endregion
    #region generics
    public static TType GetValueOrDefault<TType>(this SerializationInfo This, string name, TType defaultValue) {
      return (This.ContainsKey(name) ? This.GetValue<TType>(name) : defaultValue);
    }
    public static TType GetValueOrDefault<TType>(this SerializationInfo This, string name) {
      return (This.GetValueOrDefault(name, default(TType)));
    }
    #endregion
    #endregion

  }
}
