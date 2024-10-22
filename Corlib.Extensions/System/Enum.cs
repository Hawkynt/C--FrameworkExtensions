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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;
using Guard;

namespace System;

public static partial class EnumExtensions {
  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDescription<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DescriptionAttribute>(field)?.Description;

  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayName<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName;

  /// <summary>
  ///   Gets the field description.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The content of the description attribute or <c>null</c>.</returns>
  public static string GetFieldDisplayNameOrDefault<TEnum>(this TEnum field) where TEnum : Enum
    => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName ?? field.ToString();

  /// <summary>
  ///   Gets the attribute of an enumeration field.
  /// </summary>
  /// <typeparam name="TEnum">The type of the enum.</typeparam>
  /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
  /// <param name="field">The field.</param>
  /// <returns>The attribute or <c>null</c>.</returns>
  public static TAttribute GetFieldAttribute<TEnum, TAttribute>(this TEnum field)
    where TEnum : Enum
    where TAttribute : Attribute
    => (TAttribute)field
      .GetType()
      .GetField(field.ToString())
      ?
      .GetCustomAttributes(typeof(TAttribute), false)
      .FirstOrDefault();

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">The converter function to use with the custom attribute.</param>
  /// <returns>A string representation of the enum value based on the custom attribute, or the enum's default <see cref="object.ToString"/> if no matching attribute is found.</returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum ExampleEnum {
  ///     [Description("Custom Value 1")]
  ///     Value1,
  ///     [Description("Custom Value 2")]
  ///     Value2
  /// }
  ///
  /// var result = ExampleEnum.Value1.ToString&lt;ExampleEnum, DescriptionAttribute&gt;(desc => desc.Description);
  /// Console.WriteLine(result); // Output: "Custom Value 1"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter) where TEnum : struct, Enum where TAttribute : Attribute
    => TryToString(@this, converter, out var result) ? result : @this.ToString()
  ;

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function.
  /// If no matching attribute is found, returns the provided default value.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">The converter function to use with the custom attribute.</param>
  /// <param name="defaultValue">The default value to return if no matching attribute is found.</param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the provided <paramref name="defaultValue"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     "Unknown status");
  /// Console.WriteLine(result); // Output: "Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     "Unknown status");
  /// Console.WriteLine(result); // Output: "Unknown status"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, string defaultValue) where TEnum : struct, Enum where TAttribute : Attribute
    => TryToString(@this, converter, out var result) ? result : defaultValue
  ;

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function. 
  /// If no matching attribute is found, returns a value generated by the provided default value generator function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">The converter function to use with the custom attribute.</param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return if no matching attribute is found.
  /// </param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the value generated by <paramref name="defaultValueGenerator"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     () => "Unknown status");
  /// Console.WriteLine(result); // Output: "Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     () => "Unknown status");
  /// Console.WriteLine(result); // Output: "Unknown status"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, Func<string> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryToString(@this, converter, out var result) ? result : defaultValueGenerator();
  }

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function. 
  /// If no matching attribute is found, returns a value generated by the provided default value generator function, 
  /// which takes the enum value as a parameter.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">The converter function to use with the custom attribute.</param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return, taking the enum value as a parameter, 
  /// if no matching attribute is found.
  /// </param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the value generated by <paramref name="defaultValueGenerator"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> or <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     s => $"Enum value: {s}");
  /// Console.WriteLine(result); // Output: "Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     attr => attr.Description, 
  ///     s => $"Enum value: {s}");
  /// Console.WriteLine(result); // Output: "Enum value: Pending"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, Func<TEnum, string> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryToString(@this, converter, out var result) ? result : defaultValueGenerator(@this);
  }

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function.
  /// If no matching attribute is found, the enum's default <see cref="object.ToString"/> method is used.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the enum value, and returns the string representation.
  /// </param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the result of the enum's <see cref="object.ToString"/> method if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToString&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}");
  /// Console.WriteLine(result); // Output: "Active: Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToString&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}");
  /// Console.WriteLine(result); // Output: "Pending"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter) where TEnum : struct, Enum where TAttribute : Attribute
    => TryToString(@this, converter, out var result) ? result : @this.ToString()
    ;

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function. 
  /// If no matching attribute is found, returns the provided default value.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the enum value, and returns the string representation.
  /// </param>
  /// <param name="defaultValue">The default value to return if no matching attribute is found.</param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the provided <paramref name="defaultValue"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     "Unknown status");
  /// Console.WriteLine(result); // Output: "Active: Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     "Unknown status");
  /// Console.WriteLine(result); // Output: "Unknown status"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, string defaultValue) where TEnum : struct, Enum where TAttribute : Attribute
    => TryToString(@this, converter, out var result) ? result : defaultValue
  ;

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function. 
  /// If no matching attribute is found, returns a value generated by the provided default value generator function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the enum value, and returns the string representation.
  /// </param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return if no matching attribute is found.
  /// </param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the value generated by <paramref name="defaultValueGenerator"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> or <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     () => "Unknown status");
  /// Console.WriteLine(result); // Output: "Active: Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     () => "Unknown status");
  /// Console.WriteLine(result); // Output: "Unknown status"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, Func<string> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryToString(@this, converter, out var result) ? result : defaultValueGenerator();
  }

  /// <summary>
  /// Converts an enum value to a string using a custom attribute and a converter function. 
  /// If no matching attribute is found, returns a value generated by the provided default value generator function, 
  /// which takes the enum value as a parameter.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the enum value, and returns the string representation.
  /// </param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return, taking the enum value as a parameter, 
  /// if no matching attribute is found.
  /// </param>
  /// <returns>
  /// A string representation of the enum value based on the custom attribute, 
  /// or the value generated by <paramref name="defaultValueGenerator"/> if no matching attribute is found.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> or <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: When attribute is present
  /// var status = Status.Active;
  /// var result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     s => $"Enum value: {s}");
  /// Console.WriteLine(result); // Output: "Active: Status is active"
  ///
  /// // Usage Example 2: When no attribute is present
  /// status = Status.Pending;
  /// result = status.ToStringOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", 
  ///     s => $"Enum value: {s}");
  /// Console.WriteLine(result); // Output: "Enum value: Pending"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static string ToStringOrDefault<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, Func<TEnum, string> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryToString(@this, converter, out var result) ? result : defaultValueGenerator(@this);
  }

  /// <summary>
  /// Tries to convert an enum value to a string using a custom attribute and a converter function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">The converter function that takes the custom attribute and returns the string representation.</param>
  /// <param name="result">
  /// When this method returns, contains the string representation of the enum value if the conversion succeeded, 
  /// or <see langword="null"/> if the conversion failed.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the conversion succeeded and a matching attribute was found; otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: When attribute is present
  /// var status = Status.Active;
  /// var success = status.TryToString&lt;Status, DescriptionAttribute&gt;(attr => attr.Description, out var result);
  /// Console.WriteLine(success); // Output: True
  /// Console.WriteLine(result);  // Output: "Status is active"
  ///
  /// // Example 2: When no attribute is present
  /// status = Status.Pending;
  /// success = status.TryToString&lt;Status, DescriptionAttribute&gt;(attr => attr.Description, out result);
  /// Console.WriteLine(success); // Output: False
  /// Console.WriteLine(result == null);  // Output: True
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool TryToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, string> converter, out string result) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(converter);

    return TryToString<TEnum, TAttribute>(@this, (a, _) => converter(a), out result);
  }

  /// <summary>
  /// Tries to convert an enum value to a string using a custom attribute and a converter function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The enum value to convert.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the enum value, 
  /// and returns the string representation.
  /// </param>
  /// <param name="result">
  /// When this method returns, contains the string representation of the enum value if the conversion succeeded, 
  /// or <see langword="null"/> if the conversion failed.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the conversion succeeded and a matching attribute was found; otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: When attribute is present
  /// var status = Status.Active;
  /// var success = status.TryToString&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", out var result);
  /// Console.WriteLine(success); // Output: True
  /// Console.WriteLine(result);  // Output: "Active: Status is active"
  ///
  /// // Example 2: When no attribute is present
  /// status = Status.Pending;
  /// success = status.TryToString&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, enumValue) => $"{enumValue}: {attr.Description}", out result);
  /// Console.WriteLine(success); // Output: False
  /// Console.WriteLine(result == null);  // Output: True
  /// </code>
  /// </example>
  public static bool TryToString<TEnum, TAttribute>(this TEnum @this, Func<TAttribute, TEnum, string> converter, out string result) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(converter);

    var type = typeof(TEnum);
    var valueText = @this.ToString();
    var value = type.GetField(valueText);
    if (value == null) {
      result = default;
      return false;
    }

    foreach (var attribute in value.GetCustomAttributes(true).OfType<TAttribute>()) {
      result = converter(attribute, @this);
      if (result != null)
        return true;
    }

    result = default;
    return false;
  }

  /// <summary>
  /// Parses a string to an enum value using a custom attribute and a converter function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <returns>The enum value parsed from the string.</returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="System.ArgumentException">
  /// Thrown if the string cannot be parsed into the enum value.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Usage Example 1: Successful parsing
  /// string input = "Status is active";
  /// var status = input.Parse&lt;Status, DescriptionAttribute&gt;((attr, val) => 
  ///     attr.Description == val ? Status.Active : (Status?)null);
  /// Console.WriteLine(status); // Output: Active
  ///
  /// // Usage Example 2: Parsing fails, throws ArgumentException
  /// input = "Unknown status";
  /// status = input.Parse&lt;Status, DescriptionAttribute&gt;((attr, val) => 
  ///     attr.Description == val ? Status.Active : (Status?)null);
  /// // Throws ArgumentException: "Unknown value for Status:Unknown status"
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TEnum Parse<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter) where TEnum : struct, Enum where TAttribute : Attribute 
    => TryParse(@this, converter, out var result) ? result : throw new ArgumentException($"Unknown value for {typeof(TEnum).Name}:{@this}", nameof(@this))
  ;

  /// <summary>
  /// Parses a string to an enum value using a custom attribute and a converter function.
  /// If the string cannot be parsed, returns the provided default enum value.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <param name="defaultValue">The default enum value to return if the string cannot be parsed.</param>
  /// <returns>
  /// The parsed enum value if the string matches the attribute, or the provided <paramref name="defaultValue"/> if the parsing fails.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: Successful parsing
  /// string input = "Status is active";
  /// var status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     Status.Pending);
  /// Console.WriteLine(status); // Output: Active
  ///
  /// // Example 2: Parsing fails, returns default value
  /// input = "Unknown status";
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     Status.Pending);
  /// Console.WriteLine(status); // Output: Pending
  ///
  /// // Example 3: Converter is null, throws ArgumentNullException
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(null, Status.Pending);
  /// // Throws ArgumentNullException
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TEnum ParseOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter, TEnum defaultValue) where TEnum : struct, Enum where TAttribute : Attribute
    => TryParse(@this, converter, out var result) ? result : defaultValue
  ;

  /// <summary>
  /// Parses a string to an enum value using a custom attribute and a converter function. 
  /// If the string cannot be parsed, returns a value generated by the provided default value generator function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return if the string cannot be parsed.
  /// </param>
  /// <returns>
  /// The parsed enum value if the string matches the attribute, or the value generated by 
  /// <paramref name="defaultValueGenerator"/> if parsing fails.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> or <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: Successful parsing
  /// string input = "Status is active";
  /// var status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     () => Status.Pending);
  /// Console.WriteLine(status); // Output: Active
  ///
  /// // Example 2: Parsing fails, default value is generated
  /// input = "Unknown status";
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     () => Status.Pending);
  /// Console.WriteLine(status); // Output: Pending
  ///
  /// // Example 3: Converter is null, throws ArgumentNullException
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(null, () => Status.Pending);
  /// // Throws ArgumentNullException
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TEnum ParseOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter, Func<TEnum> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryParse(@this, converter, out var result) ? result : defaultValueGenerator();
  }

  /// <summary>
  /// Parses a string to an enum value using a custom attribute and a converter function. 
  /// If the string cannot be parsed, returns a value generated by the provided default value generator function, 
  /// which takes the input string as a parameter.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <param name="defaultValueGenerator">
  /// A function that generates a default value to return if the string cannot be parsed, 
  /// taking the input string as a parameter.
  /// </param>
  /// <returns>
  /// The parsed enum value if the string matches the attribute, or the value generated by 
  /// <paramref name="defaultValueGenerator"/> if parsing fails.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> or <paramref name="defaultValueGenerator"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: Successful parsing
  /// string input = "Status is active";
  /// var status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     s => Status.Pending);
  /// Console.WriteLine(status); // Output: Active
  ///
  /// // Example 2: Parsing fails, default value is generated based on the input string
  /// input = "Unknown status";
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     s => Status.Pending);
  /// Console.WriteLine(status); // Output: Pending
  ///
  /// // Example 3: Converter is null, throws ArgumentNullException
  /// status = input.ParseOrDefault&lt;Status, DescriptionAttribute&gt;(null, s => Status.Pending);
  /// // Throws ArgumentNullException
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TEnum ParseOrDefault<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter, Func<string, TEnum> defaultValueGenerator) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ArgumentIsNull(defaultValueGenerator);

    return TryParse(@this, converter, out var result) ? result : defaultValueGenerator(@this);
  }

  /// <summary>
  /// Parses a string to an enum value using a custom attribute and a converter function. 
  /// If the string cannot be parsed, returns <see langword="null"/>.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <returns>
  /// The parsed enum value if the string matches the attribute, or <see langword="null"/> if parsing fails.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: Successful parsing
  /// string input = "Status is active";
  /// var status = input.ParseOrNull&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null);
  /// Console.WriteLine(status); // Output: Active
  ///
  /// // Example 2: Parsing fails, returns null
  /// input = "Unknown status";
  /// status = input.ParseOrNull&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null);
  /// Console.WriteLine(status == null); // Output: True
  ///
  /// // Example 3: Converter is null, throws ArgumentNullException
  /// status = input.ParseOrNull&lt;Status, DescriptionAttribute&gt;(null);
  /// // Throws ArgumentNullException
  /// </code>
  /// </example>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static TEnum? ParseOrNull<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter) where TEnum : struct, Enum where TAttribute : Attribute
    => TryParse(@this, converter, out var result) ? result : null
  ;

  /// <summary>
  /// Tries to parse a string to an enum value using a custom attribute and a converter function.
  /// </summary>
  /// <typeparam name="TEnum">The enum type to parse into.</typeparam>
  /// <typeparam name="TAttribute">The attribute type used for conversion.</typeparam>
  /// <param name="this">The string to parse.</param>
  /// <param name="converter">
  /// The converter function that takes both the custom attribute and the input string, 
  /// and returns a nullable enum value if the conversion is successful, or <see langword="null"/> if it fails.
  /// </param>
  /// <param name="result">
  /// When this method returns, contains the enum value if the string was successfully parsed; 
  /// otherwise, contains the default value of <typeparamref name="TEnum"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the string was successfully parsed to an enum value; otherwise, <see langword="false"/>.
  /// </returns>
  /// <exception cref="System.ArgumentNullException">
  /// Thrown if <paramref name="this"/> or <paramref name="converter"/> is <see langword="null"/>.
  /// </exception>
  /// <example>
  /// <code>
  /// public enum Status {
  ///     [Description("Status is active")]
  ///     Active,
  ///     [Description("Status is inactive")]
  ///     Inactive,
  ///     Pending // No attribute here
  /// }
  ///
  /// // Example 1: Successful parsing
  /// string input = "Status is active";
  /// var success = input.TryParse&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     out var status);
  /// Console.WriteLine(success); // Output: True
  /// Console.WriteLine(status);  // Output: Active
  ///
  /// // Example 2: Parsing fails, returns false
  /// input = "Unknown status";
  /// success = input.TryParse&lt;Status, DescriptionAttribute&gt;(
  ///     (attr, val) => attr.Description == val ? Status.Active : (Status?)null, 
  ///     out status);
  /// Console.WriteLine(success); // Output: False
  /// Console.WriteLine(status);  // Output: Pending (default value)
  ///
  /// // Example 3: Converter is null, throws ArgumentNullException
  /// success = input.TryParse&lt;Status, DescriptionAttribute&gt;(null, out status);
  /// // Throws ArgumentNullException
  /// </code>
  /// </example>
  public static bool TryParse<TEnum, TAttribute>(this string @this, Func<TAttribute, string, TEnum?> converter, out TEnum result) where TEnum : struct, Enum where TAttribute : Attribute {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(converter);

    var type = typeof(TEnum);
    var fields = type.GetFields();
    foreach (var field in fields)
    foreach (var attribute in field.GetCustomAttributes(true).OfType<TAttribute>()) {
      var maybe = converter(attribute, @this);
      if (!maybe.HasValue)
        continue;

      result = maybe.Value;
      return true;
    }

    result = default;
    return false;
  }

}
