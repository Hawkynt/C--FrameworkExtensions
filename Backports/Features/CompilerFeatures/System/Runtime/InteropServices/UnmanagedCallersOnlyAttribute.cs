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

#if !SUPPORTS_UNMANAGED_CALLERS_ONLY_ATTRIBUTE

namespace System.Runtime.InteropServices;

/// <summary>
/// This attribute is used to indicate that a method should be directly callable from native code.
/// Methods marked with this attribute can be passed to unmanaged code as a function pointer.
/// </summary>
/// <remarks>
/// <para>
/// Methods marked with this attribute have the following restrictions:
/// </para>
/// <list type="bullet">
///   <item><description>Must be static</description></item>
///   <item><description>Must not be called from managed code</description></item>
///   <item><description>Must only have blittable parameter types</description></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class UnmanagedCallersOnlyAttribute : Attribute {

  /// <summary>
  /// Initializes a new instance of the <see cref="UnmanagedCallersOnlyAttribute"/> class.
  /// </summary>
  public UnmanagedCallersOnlyAttribute() { }

  /// <summary>
  /// Optional. Types indicating the calling convention of the entry point.
  /// If not specified, the platform default calling convention is used.
  /// </summary>
  public Type[]? CallConvs;

  /// <summary>
  /// Optional. If specified, the name of the entry point in the generated export table.
  /// If not specified, the method name is used.
  /// </summary>
  public string? EntryPoint;

}

#endif
