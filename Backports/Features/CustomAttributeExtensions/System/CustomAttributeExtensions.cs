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

#if !SUPPORTS_CUSTOM_ATTRIBUTE_EXTENSIONS

using System.Collections.Generic;

#nullable enable
namespace System.Reflection;

/// <summary>Contains static methods for retrieving custom attributes.</summary>
public static class CustomAttributeExtensions {
  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(this Assembly element, Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(this Module element, Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(this MemberInfo element, Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(this ParameterInfo element, Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this Assembly element) where T : Attribute => (T)element.GetCustomAttribute(typeof(T));

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this Module element) where T : Attribute => (T)element.GetCustomAttribute(typeof(T));

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this MemberInfo element) where T : Attribute => (T)element.GetCustomAttribute(typeof(T));

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this ParameterInfo element) where T : Attribute => (T)element.GetCustomAttribute(typeof(T));

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(
    this MemberInfo element,
    Type attributeType,
    bool inherit)
    => Attribute.GetCustomAttribute(element, attributeType, inherit);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute matching <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static Attribute? GetCustomAttribute(
    this ParameterInfo element,
    Type attributeType,
    bool inherit)
    => Attribute.GetCustomAttribute(element, attributeType, inherit);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute => (T)element.GetCustomAttribute(typeof(T), inherit);

  /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
  public static T? GetCustomAttribute<T>(this ParameterInfo element, bool inherit) where T : Attribute => (T)element.GetCustomAttribute(typeof(T), inherit);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(this Assembly element) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(this Module element) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(this ParameterInfo element) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> that match the specified criteria, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(this MemberInfo element, bool inherit) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, inherit);

  /// <summary>Retrieves a collection of custom attributes that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this ParameterInfo element,
    bool inherit)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, inherit);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this Assembly element,
    Type attributeType)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this Module element,
    Type attributeType)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this MemberInfo element,
    Type attributeType)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this ParameterInfo element,
    Type attributeType)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this Assembly element) where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this Module element) where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element) where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element) where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this MemberInfo element,
    Type attributeType,
    bool inherit)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType, inherit);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<Attribute> GetCustomAttributes(
    this ParameterInfo element,
    Type attributeType,
    bool inherit)
    => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType, inherit);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo element, bool inherit) where T : Attribute => (IEnumerable<T>)CustomAttributeExtensions.GetCustomAttributes(element, typeof(T), inherit);

  /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <typeparam name="T">The type of attribute to search for.</typeparam>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
  /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
  public static IEnumerable<T> GetCustomAttributes<T>(this ParameterInfo element, bool inherit) where T : Attribute => (IEnumerable<T>)CustomAttributeExtensions.GetCustomAttributes(element, typeof(T), inherit);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified assembly.</summary>
  /// <param name="element">The assembly to inspect.</param>
  /// <param name="attributeType">The type of the attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this Assembly element, Type attributeType) => Attribute.IsDefined(element, attributeType);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified module.</summary>
  /// <param name="element">The module to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this Module element, Type attributeType) => Attribute.IsDefined(element, attributeType);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified member.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this MemberInfo element, Type attributeType) => Attribute.IsDefined(element, attributeType);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified parameter.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this ParameterInfo element, Type attributeType) => Attribute.IsDefined(element, attributeType);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally, applied to its ancestors.</summary>
  /// <param name="element">The member to inspect.</param>
  /// <param name="attributeType">The type of the attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <exception cref="T:System.NotSupportedException">
  /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this MemberInfo element, Type attributeType, bool inherit) => Attribute.IsDefined(element, attributeType, inherit);

  /// <summary>Indicates whether custom attributes of a specified type are applied to a specified parameter, and, optionally, applied to its ancestors.</summary>
  /// <param name="element">The parameter to inspect.</param>
  /// <param name="attributeType">The type of attribute to search for.</param>
  /// <param name="inherit">
  /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
  /// <exception cref="T:System.ArgumentNullException">
  /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
  /// <exception cref="T:System.ArgumentException">
  /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
  /// <returns>
  /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
  public static bool IsDefined(this ParameterInfo element, Type attributeType, bool inherit) => Attribute.IsDefined(element, attributeType, inherit);
}

#endif