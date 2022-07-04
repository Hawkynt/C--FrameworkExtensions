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

namespace System.ComponentModel {
  /// <summary>
  /// Tells the propertygrid what the minimum value for this number is.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class MinValueAttribute : Attribute {
    public decimal Value { get; }

    public MinValueAttribute(decimal value) {
      this.Value = value;
    }
    public MinValueAttribute(int value)
      : this((decimal)value) {
    }
  }
  /// <summary>
  /// Tells the propertygrid what the maximum value for this number is.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class MaxValueAttribute : Attribute {
    public decimal Value { get; }

    public MaxValueAttribute(decimal value) {
      this.Value = value;
    }
    public MaxValueAttribute(int value)
      : this((decimal)value) {
    }
  }
  /// <summary>
  /// Tells the propertygrid what the maximum value for this number is.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field)]
  public class EnumDisplayNameAttribute : DisplayNameAttribute {
    public override string DisplayName { get; }

    public EnumDisplayNameAttribute(string displayName) {
      this.DisplayName = displayName;
    }

    public static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct => GetDisplayName(typeof(TEnum), value);
    public static string GetDisplayName(Type type, object value) => (type.GetField(value.ToString())?.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).FirstOrDefault() as EnumDisplayNameAttribute)?.DisplayName;
    public static string GetDisplayNameOrDefault<TEnum>(TEnum value) where TEnum : struct => GetDisplayName(value) ?? value.ToString();
    public static string GetDisplayNameOrDefault(Type type, object value) => GetDisplayName(type, value) ?? value.ToString();
  }
}
