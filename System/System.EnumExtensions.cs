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
    public static string GetFieldDescription<TEnum>(this TEnum field) where TEnum : struct
      => GetFieldAttribute<TEnum, DescriptionAttribute>(field)?.Description
      ;

    /// <summary>
    /// Gets the field description.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The content of the description attribute or <c>null</c>.</returns>
    public static string GetFieldDisplayName<TEnum>(this TEnum field) where TEnum : struct
      => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName
      ;

    /// <summary>
    /// Gets the field description.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The content of the description attribute or <c>null</c>.</returns>
    public static string GetFieldDisplayNameOrDefault<TEnum>(this TEnum field) where TEnum : struct
      => GetFieldAttribute<TEnum, DisplayNameAttribute>(field)?.DisplayName ?? field.ToString()
      ;
      
    /// <summary>
    /// Gets the attribute of an enumeration field.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enum.</typeparam>
    /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
    /// <param name="field">The field.</param>
    /// <returns>The attribute or <c>null</c>.</returns>
    public static TAttribute GetFieldAttribute<TEnum, TAttribute>(this TEnum field)
      where TEnum : struct
      where TAttribute : Attribute {

      var type = field.GetType();
      Contract.Assert(type.IsEnum, "Only supported on enumerations");

      return
        (TAttribute)type
        .GetField(field.ToString())?
        .GetCustomAttributes(typeof(TAttribute), false)
        .FirstOrDefault()
        ;
    }
  }
}
