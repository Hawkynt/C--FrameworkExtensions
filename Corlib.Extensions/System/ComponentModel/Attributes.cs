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

namespace System.ComponentModel;

/// <summary>
///   Tells the propertygrid what the minimum value for this number is.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MinValueAttribute(decimal value) : Attribute {
  public decimal Value { get; } = value;
  public MinValueAttribute(int value) : this((decimal)value) { }
}

/// <summary>
///   Tells the propertygrid what the maximum value for this number is.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class MaxValueAttribute(decimal value) : Attribute {
  public decimal Value { get; } = value;
  public MaxValueAttribute(int value) : this((decimal)value) { }
}

/// <summary>
///   Tells the propertygrid what the maximum value for this number is.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class EnumDisplayNameAttribute(string displayName) : DisplayNameAttribute {
  public override string DisplayName { get; } = displayName;
  public static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct => GetDisplayName(typeof(TEnum), value);
  public static string GetDisplayName(Type type, object value) => (type.GetField(value.ToString())?.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).FirstOrDefault() as EnumDisplayNameAttribute)?.DisplayName;
  public static string GetDisplayNameOrDefault<TEnum>(TEnum value) where TEnum : struct => GetDisplayName(value) ?? value.ToString();
  public static string GetDisplayNameOrDefault(Type type, object value) => GetDisplayName(type, value) ?? value.ToString();
}
