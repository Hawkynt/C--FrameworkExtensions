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

namespace System;

/// <summary>
///   Used to force the compiler to chose a method-overload with a class constraint on a generic type.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once ConvertToStaticClass
public sealed class __ClassForcingTag<T> where T : class {
  private __ClassForcingTag() { }
}

/// <summary>
///   Used to force the compiler to chose a method-overload with a struct constraint on a generic type.
/// </summary>
/// <typeparam name="T"></typeparam>
// ReSharper disable once ConvertToStaticClass
public sealed class __StructForcingTag<T> where T : struct {
  private __StructForcingTag() { }
}
