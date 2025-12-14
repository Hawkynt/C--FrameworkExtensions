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
  /// <param name="element">The assembly to inspect.</param>
  extension(Assembly element)
  {
    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified assembly.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
    public Attribute? GetCustomAttribute(Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified assembly.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
    public T? GetCustomAttribute<T>() where T : Attribute => (T?)element.GetCustomAttribute(typeof(T));

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified assembly.</summary>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes() => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified assembly.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(Type attributeType)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified assembly.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified assembly.</summary>
    /// <param name="attributeType">The type of the attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>
    /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
    public bool IsDefined(Type attributeType) => Attribute.IsDefined(element, attributeType);
  }

  /// <param name="element">The module to inspect.</param>
  extension(Module element)
  {
    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified module.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
    public Attribute? GetCustomAttribute(Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified module.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
    public T? GetCustomAttribute<T>() where T : Attribute => (T?)element.GetCustomAttribute(typeof(T));

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified module.</summary>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes() => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified module.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(Type attributeType)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified module.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified module.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>
    /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
    public bool IsDefined(Type attributeType) => Attribute.IsDefined(element, attributeType);
  }

  /// <param name="element">The member to inspect.</param>
  extension(MemberInfo element)
  {
    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member.</summary>
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
    public Attribute? GetCustomAttribute(Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
    public T? GetCustomAttribute<T>() where T : Attribute => (T?)element.GetCustomAttribute(typeof(T));

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.</summary>
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
    public Attribute? GetCustomAttribute(
      Type attributeType,
      bool inherit)
      => Attribute.GetCustomAttribute(element, attributeType, inherit);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.</summary>
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
    public T? GetCustomAttribute<T>(bool inherit) where T : Attribute => (T?)element.GetCustomAttribute(typeof(T), inherit);

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified member.</summary>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes() => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
    /// <param name="inherit">
    /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> that match the specified criteria, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(bool inherit) => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, inherit);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(Type attributeType)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
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
    public IEnumerable<Attribute> GetCustomAttributes(
      Type attributeType,
      bool inherit)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType, inherit);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified member, and optionally inspects the ancestors of that member.</summary>
    /// <param name="inherit">
    /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute => (IEnumerable<T>)CustomAttributeExtensions.GetCustomAttributes(element, typeof(T), inherit);

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified member.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <returns>
    /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
    public bool IsDefined(Type attributeType) => Attribute.IsDefined(element, attributeType);

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified member, and, optionally, applied to its ancestors.</summary>
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
    public bool IsDefined(Type attributeType, bool inherit) => Attribute.IsDefined(element, attributeType, inherit);
  }

  /// <param name="element">The parameter to inspect.</param>
  extension(ParameterInfo element)
  {
    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A custom attribute that matches <paramref name="attributeType" />, or <see langword="null" /> if no such attribute is found.</returns>
    public Attribute? GetCustomAttribute(Type attributeType) => Attribute.GetCustomAttribute(element, attributeType);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A custom attribute that matches <paramref name="T" />, or <see langword="null" /> if no such attribute is found.</returns>
    public T? GetCustomAttribute<T>() where T : Attribute => (T?)element.GetCustomAttribute(typeof(T));

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
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
    public Attribute? GetCustomAttribute(
      Type attributeType,
      bool inherit)
      => Attribute.GetCustomAttribute(element, attributeType, inherit);

    /// <summary>Retrieves a custom attribute of a specified type that is applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
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
    public T? GetCustomAttribute<T>(bool inherit) where T : Attribute => (T?)element.GetCustomAttribute(typeof(T), inherit);

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified parameter.</summary>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes() => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element);

    /// <summary>Retrieves a collection of custom attributes that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
    /// <param name="inherit">
    /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(bool inherit)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, inherit);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="attributeType" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<Attribute> GetCustomAttributes(Type attributeType)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter.</summary>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>() where T : Attribute => (IEnumerable<T>)element.GetCustomAttributes(typeof(T));

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
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
    public IEnumerable<Attribute> GetCustomAttributes(
      Type attributeType,
      bool inherit)
      => (IEnumerable<Attribute>)Attribute.GetCustomAttributes(element, attributeType, inherit);

    /// <summary>Retrieves a collection of custom attributes of a specified type that are applied to a specified parameter, and optionally inspects the ancestors of that parameter.</summary>
    /// <param name="inherit">
    /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
    /// <typeparam name="T">The type of attribute to search for.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">
    /// <paramref name="element" /> is not a constructor, method, property, event, type, or field.</exception>
    /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
    /// <returns>A collection of the custom attributes that are applied to <paramref name="element" /> and that match <paramref name="T" />, or an empty collection if no such attributes exist.</returns>
    public IEnumerable<T> GetCustomAttributes<T>(bool inherit) where T : Attribute => (IEnumerable<T>)CustomAttributeExtensions.GetCustomAttributes(element, typeof(T), inherit);

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified parameter.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>
    /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
    public bool IsDefined(Type attributeType) => Attribute.IsDefined(element, attributeType);

    /// <summary>Indicates whether custom attributes of a specified type are applied to a specified parameter, and, optionally, applied to its ancestors.</summary>
    /// <param name="attributeType">The type of attribute to search for.</param>
    /// <param name="inherit">
    /// <see langword="true" /> to inspect the ancestors of <paramref name="element" />; otherwise, <see langword="false" />.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="element" /> or <paramref name="attributeType" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="attributeType" /> is not derived from <see cref="T:System.Attribute" />.</exception>
    /// <returns>
    /// <see langword="true" /> if an attribute of the specified type is applied to <paramref name="element" />; otherwise, <see langword="false" />.</returns>
    public bool IsDefined(Type attributeType, bool inherit) => Attribute.IsDefined(element, attributeType, inherit);
  }
}

#endif