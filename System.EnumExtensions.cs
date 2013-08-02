using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace System {
  internal static partial class EnumExtensions {

    /// <summary>
    /// Gets the field description.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The content of the description attribute or <c>null</c>.</returns>
    public static string GetFieldDescription<TEnum>(TEnum field) where TEnum : struct {
      var result = GetFieldAttribute<TEnum, DescriptionAttribute>(field);
      return (result == null ? null : result.Description);
    }

    /// <summary>
    /// Gets the field description.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The content of the description attribute or <c>null</c>.</returns>
    public static string GetFieldDisplayName<TEnum>(TEnum field) where TEnum : struct {
      var result = GetFieldAttribute<TEnum, DisplayNameAttribute>(field);
      return (result == null ? null : result.DisplayName);
    }

    /// <summary>
    /// Gets the attribute of an enumeration field.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The attribute or <c>null</c>.</returns>
    public static TAttribute GetFieldAttribute<TEnum, TAttribute>(TEnum field)
      where TEnum : struct
      where TAttribute : Attribute {

      var type = field.GetType();
      Contract.Assert(type.IsEnum, "Only supported on enumerations");

      var name = field.ToString();

      var fieldInfo = type.GetField(name);
      Contract.Assert(fieldInfo != null, "Can not find field");

      var attribute = (TAttribute)fieldInfo.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault();
      return (attribute);
    }
  }
}
