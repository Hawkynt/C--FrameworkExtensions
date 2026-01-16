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

using System;
using System.Runtime.CompilerServices;
#if !SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
using System.Linq.Expressions;
using System.Reflection;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
#endif
using Hawkynt.ColorProcessing.Metrics;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace Hawkynt.ColorProcessing;

/// <summary>
/// Factory for creating color space instances across all target frameworks.
/// </summary>
/// <remarks>
/// On .NET 7+, forwards directly to the static abstract Create method.
/// On earlier frameworks, uses compiled expression trees for efficient delegate invocation.
/// </remarks>
public static class ColorFactory {

  /// <summary>
  /// Creates a 4-component byte color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create4B<T>(byte c1, byte c2, byte c3, byte a)
    where T : unmanaged, IColorSpace4B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3, a);
#else
    => ColorFactory4B<T>.Create(c1, c2, c3, a);
#endif

  /// <summary>
  /// Creates a 3-component byte color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create3B<T>(byte c1, byte c2, byte c3)
    where T : unmanaged, IColorSpace3B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3);
#else
    => ColorFactory3B<T>.Create(c1, c2, c3);
#endif

  /// <summary>
  /// Creates a 4-component float color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create4F<T>(float c1, float c2, float c3, float a)
    where T : unmanaged, IColorSpace4F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3, a);
#else
    => ColorFactory4F<T>.Create(c1, c2, c3, a);
#endif

  /// <summary>
  /// Creates a 3-component float color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create3F<T>(float c1, float c2, float c3)
    where T : unmanaged, IColorSpace3F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3);
#else
    => ColorFactory3F<T>.Create(c1, c2, c3);
#endif

  /// <summary>
  /// Creates a 5-component byte color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create5B<T>(byte c1, byte c2, byte c3, byte c4, byte a)
    where T : unmanaged, IColorSpace5B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3, c4, a);
#else
    => ColorFactory5B<T>.Create(c1, c2, c3, c4, a);
#endif

  /// <summary>
  /// Creates a 5-component float color instance.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create5F<T>(float c1, float c2, float c3, float c4, float a)
    where T : unmanaged, IColorSpace5F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.Create(c1, c2, c3, c4, a);
#else
    => ColorFactory5F<T>.Create(c1, c2, c3, c4, a);
#endif

  /// <summary>
  /// Creates a 3-component byte color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_3B<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3)
    where T : unmanaged, IColorSpace3B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3);
#else
    => ColorFactory3B<T>.FromNormalized(c1, c2, c3);
#endif

  /// <summary>
  /// Creates a 4-component byte color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_4B<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a)
    where T : unmanaged, IColorSpace4B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3, a);
#else
    => ColorFactory4B<T>.FromNormalized(c1, c2, c3, a);
#endif

  /// <summary>
  /// Creates a 5-component byte color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_5B<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a)
    where T : unmanaged, IColorSpace5B<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3, c4, a);
#else
    => ColorFactory5B<T>.FromNormalized(c1, c2, c3, c4, a);
#endif

  /// <summary>
  /// Creates a 3-component float color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_3F<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3)
    where T : unmanaged, IColorSpace3F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3);
#else
    => ColorFactory3F<T>.FromNormalized(c1, c2, c3);
#endif

  /// <summary>
  /// Creates a 4-component float color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_4F<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a)
    where T : unmanaged, IColorSpace4F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3, a);
#else
    => ColorFactory4F<T>.FromNormalized(c1, c2, c3, a);
#endif

  /// <summary>
  /// Creates a 5-component float color from normalized values.
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_5F<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a)
    where T : unmanaged, IColorSpace5F<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3, c4, a);
#else
    => ColorFactory5F<T>.FromNormalized(c1, c2, c3, c4, a);
#endif

  /// <summary>
  /// Creates a 4-component color from normalized values (base interface).
  /// </summary>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized_4<T>(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a)
    where T : unmanaged, IColorSpace4<T>
#if SUPPORTS_ABSTRACT_INTERFACE_MEMBERS
    => T.FromNormalized(c1, c2, c3, a);
#else
    => ColorFactory4<T>.FromNormalized(c1, c2, c3, a);
#endif
}

#if !SUPPORTS_ABSTRACT_INTERFACE_MEMBERS

/// <summary>
/// Helper for building compiled delegates via expression trees.
/// </summary>
internal static class ColorFactoryHelper {

  /// <summary>
  /// Builds a compiled delegate that invokes a static method of the target type.
  /// </summary>
  /// <typeparam name="TDelegate">The delegate type to compile.</typeparam>
  /// <typeparam name="TTarget">The target type containing the static method.</typeparam>
  /// <param name="methodName">The name of the static method to invoke.</param>
  /// <param name="paramTypes">The parameter types for the method.</param>
  /// <returns>A compiled delegate that invokes the method.</returns>
  public static TDelegate BuildCreateDelegate<TDelegate, TTarget>(string methodName, params Type[] paramTypes)
    where TDelegate : Delegate {
    var method = typeof(TTarget).GetMethod(
      methodName,
      BindingFlags.Public | BindingFlags.Static,
      null,
      paramTypes,
      null
    ) ?? throw new InvalidOperationException(
      $"Type {typeof(TTarget).Name} does not have a public static {methodName}({string.Join(", ", Array.ConvertAll(paramTypes, t => t.Name))}) method."
    );

    var parameters = new ParameterExpression[paramTypes.Length];
    for (var i = 0; i < paramTypes.Length; ++i)
      parameters[i] = Expression.Parameter(paramTypes[i], $"p{i}");

    var call = Expression.Call(method, parameters);
    return Expression.Lambda<TDelegate>(call, parameters).Compile();
  }
}

/// <summary>
/// Cached delegate factory for IColorSpace3B types (pre-.NET 7).
/// </summary>
internal static class ColorFactory3B<T> where T : unmanaged, IColorSpace3B<T> {

  private delegate T CreateDelegate(byte c1, byte c2, byte c3);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>( nameof(Bgr888.Create), typeof(byte), typeof(byte), typeof(byte));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(Bgr888.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(byte c1, byte c2, byte c3) => _create(c1, c2, c3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => _fromNormalized(c1, c2, c3);
}

/// <summary>
/// Cached delegate factory for IColorSpace4B types (pre-.NET 7).
/// </summary>
internal static class ColorFactory4B<T> where T : unmanaged, IColorSpace4B<T> {

  private delegate T CreateDelegate(byte c1, byte c2, byte c3, byte a);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>(nameof(Bgra8888.Create), typeof(byte), typeof(byte), typeof(byte), typeof(byte));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(Bgra8888.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(byte c1, byte c2, byte c3, byte a) => _create(c1, c2, c3, a);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => _fromNormalized(c1, c2, c3, a);
}

/// <summary>
/// Cached delegate factory for IColorSpace5B types (pre-.NET 7).
/// </summary>
internal static class ColorFactory5B<T> where T : unmanaged, IColorSpace5B<T> {

  private delegate T CreateDelegate(byte c1, byte c2, byte c3, byte c4, byte a);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>(nameof(Cmyka88888.Create), typeof(byte), typeof(byte), typeof(byte), typeof(byte), typeof(byte));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(Cmyka88888.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(byte c1, byte c2, byte c3, byte c4, byte a) => _create(c1, c2, c3, c4, a);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a) => _fromNormalized(c1, c2, c3, c4, a);
}

/// <summary>
/// Cached delegate factory for IColorSpace3F types (pre-.NET 7).
/// </summary>
internal static class ColorFactory3F<T> where T : unmanaged, IColorSpace3F<T> {

  private delegate T CreateDelegate(float c1, float c2, float c3);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>(nameof(LinearRgbF.Create), typeof(float), typeof(float), typeof(float));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(LinearRgbF.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(float c1, float c2, float c3) => _create(c1, c2, c3);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3) => _fromNormalized(c1, c2, c3);
}

/// <summary>
/// Cached delegate factory for IColorSpace4F types (pre-.NET 7).
/// </summary>
internal static class ColorFactory4F<T> where T : unmanaged, IColorSpace4F<T> {

  private delegate T CreateDelegate(float c1, float c2, float c3, float a);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>(nameof(LinearRgbaF.Create), typeof(float), typeof(float), typeof(float), typeof(float));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(LinearRgbaF.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(float c1, float c2, float c3, float a) => _create(c1, c2, c3, a);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => _fromNormalized(c1, c2, c3, a);
}

/// <summary>
/// Cached delegate factory for IColorSpace5F types (pre-.NET 7).
/// </summary>
internal static class ColorFactory5F<T> where T : unmanaged, IColorSpace5F<T> {

  private delegate T CreateDelegate(float c1, float c2, float c3, float c4, float a);
  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a);

  private static readonly CreateDelegate _create =
    ColorFactoryHelper.BuildCreateDelegate<CreateDelegate, T>(nameof(CmykaF.Create), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float));

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(CmykaF.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T Create(float c1, float c2, float c3, float c4, float a) => _create(c1, c2, c3, c4, a);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 c4, UNorm32 a) => _fromNormalized(c1, c2, c3, c4, a);
}

/// <summary>
/// Cached delegate factory for IColorSpace4 types (base interface, pre-.NET 7).
/// </summary>
internal static class ColorFactory4<T> where T : unmanaged, IColorSpace4<T> {

  private delegate T FromNormalizedDelegate(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a);

  private static readonly FromNormalizedDelegate _fromNormalized =
    ColorFactoryHelper.BuildCreateDelegate<FromNormalizedDelegate, T>(nameof(Bgra8888.FromNormalized), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32), typeof(UNorm32));

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static T FromNormalized(UNorm32 c1, UNorm32 c2, UNorm32 c3, UNorm32 a) => _fromNormalized(c1, c2, c3, a);
}

#endif
