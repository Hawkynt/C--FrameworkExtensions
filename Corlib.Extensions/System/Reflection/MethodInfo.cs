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

using System.Linq;
using System.Text;
using Guard;

namespace System.Reflection;

public static partial class MethodInfoExtensions {
  #region specific type

  private static readonly Type _tpVoid = typeof(void);

  private static readonly Type _tpBool = typeof(bool);
  private static readonly Type _tpChar = typeof(char);
  private static readonly Type _tpByte = typeof(byte);
  private static readonly Type _tpSByte = typeof(sbyte);
  private static readonly Type _tpShort = typeof(short);
  private static readonly Type _tpWord = typeof(ushort);
  private static readonly Type _tpInt = typeof(int);
  private static readonly Type _tpDWord = typeof(uint);
  private static readonly Type _tpLong = typeof(long);
  private static readonly Type _tpQWord = typeof(ulong);
  private static readonly Type _tpFloat = typeof(float);
  private static readonly Type _tpDouble = typeof(double);
  private static readonly Type _tpDecimal = typeof(decimal);
  private static readonly Type _tpString = typeof(string);
  private static readonly Type _tpObject = typeof(object);

  #endregion

  /// <summary>
  ///   Tidies the name of the type.
  /// </summary>
  /// <param name="type">The type.</param>
  /// <returns>A tidied version of the type name.</returns>
  private static string _TidyTypeName(Type type) {
    Against.ArgumentIsNull(type);

    Type elementType;
    if (type.IsByRef && (elementType = type.GetElementType()) != null)
      return _TidyTypeName(elementType);

    if (type.IsArray && (elementType = type.GetElementType()) != null)
      return _TidyTypeName(elementType) + "[]";

    if (type == _tpVoid)
      return "void";
    if (type == _tpBool)
      return "bool";
    if (type == _tpChar)
      return "char";
    if (type == _tpByte)
      return "byte";
    if (type == _tpSByte)
      return "sbyte";
    if (type == _tpShort)
      return "short";
    if (type == _tpWord)
      return "word";
    if (type == _tpInt)
      return "int";
    if (type == _tpDWord)
      return "dword";
    if (type == _tpLong)
      return "long";
    if (type == _tpQWord)
      return "qword";
    if (type == _tpFloat)
      return "float";
    if (type == _tpDouble)
      return "double";
    if (type == _tpDecimal)
      return "decimal";
    if (type == _tpString)
      return "string";
    if (type == _tpObject)
      return "object";

    var ntype = Nullable.GetUnderlyingType(type);
    if (ntype != null)
      return _TidyTypeName(ntype) + "?";

    var fullName = type.IsGenericType || type.IsGenericParameter ? type.Name : type.FullName;

    if (fullName != null) {
      if (type.IsGenericType) {
        var i = fullName.IndexOf('`');
        if (i > 0)
          fullName = fullName[..i];
      }

      if (fullName.StartsWith("System."))
        fullName = fullName[7..];
    }

    if (type.IsGenericType)
      return fullName + "<" + string.Join(", ", type.GetGenericArguments().Select(_TidyTypeName).ToArray()) + ">";

    return fullName;
  }

  /// <summary>
  ///   Gets the full signature.
  /// </summary>
  /// <param name="this">This MethodInfo.</param>
  /// <returns>The full signature of the method.</returns>
  public static string GetFullSignature(this MethodInfo @this) {
    Against.ThisIsNull(@this);

    StringBuilder sb = new();

    if (@this.IsPublic)
      sb.Append("public ");
    else if (@this.IsPrivate)
      sb.Append("private ");
    else if (@this.IsAssembly)
      sb.Append("internal ");
    else if (@this.IsFamily)
      sb.Append("protected ");

    if (@this.IsStatic)
      sb.Append("static ");
    if (@this.IsFinal)
      sb.Append("final ");
    if (@this.IsVirtual)
      sb.Append("virtual ");
    if (@this.IsAbstract)
      sb.Append("abstract ");

    sb.Append(_TidyTypeName(@this.ReturnType));
    sb.Append(' ');
    sb.Append(@this.DeclaringType);
    sb.Append('.');
    sb.Append(@this.Name);
    var types = @this.IsGenericMethod ? @this.GetGenericArguments() : null;
    if (types != null) {
      sb.Append('<');
      for (var j = 0; j < types.Length; ++j) {
        if (j > 0)
          sb.Append(", ");

        sb.Append(_TidyTypeName(types[j]));
      }

      sb.Append('>');
    }

    sb.Append('(');

    var pars = @this.GetParameters();
    var lastI = pars.Length - 1;
    for (var i = 0; i < pars.Length; ++i) {
      if (i > 0)
        sb.Append(", ");

      var p = pars[i];
      /*if (p.IsOptional)
        sb.Append("optional ");
      */

      var paramType = p.ParameterType;

      if (paramType.IsByRef)
        sb.Append("ref ");
      else if (p.IsOut)
        sb.Append("out ");

      if (i == lastI && paramType.IsArray)
        sb.Append("params ");

      sb.Append(_TidyTypeName(paramType) + ' ');

      sb.Append(p.Name);
      if (p.IsOptional)
        sb.Append(" = " + (p.DefaultValue ?? "null"));
    }

    sb.Append(')');
    return sb.ToString();
  }
}
