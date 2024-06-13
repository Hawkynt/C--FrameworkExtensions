using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Corlib.Tests.NUnit;

internal static class TestUtilities {
  
  /// <summary>
  /// Executes a given lambda function for testing.
  /// </summary>
  /// <typeparam name="TResult">The type of the result</typeparam>
  /// <param name="resultProvider">The system under test</param>
  /// <param name="expected">The expected result, if any applicable</param>
  /// <param name="exception">The exception type is expected to be thrown, if applicable</param>
  public static void ExecuteTest<TResult>(Func<TResult> resultProvider, TResult expected, Type? exception) {
    if (exception == null)
      Assert.That(resultProvider(), Is.EqualTo(expected));
    else
      Assert.That(resultProvider, Throws.TypeOf(exception));
  }

  /// <summary>
  /// Executes a given lambda function for testing.
  /// </summary>
  /// <param name="resultGenerator">The system under test</param>
  /// <param name="assertion">The assertion to check the results.</param>
  /// <param name="exception">The exception type is expected to be thrown, if applicable</param>
  public static void ExecuteTest(Action resultGenerator, Action assertion, Type? exception) {
    if (exception == null) {
      resultGenerator();
      assertion();
    } else
      Assert.That(resultGenerator, Throws.TypeOf(exception));
  }

  // ReSharper disable once UseArrayEmptyMethod
#pragma warning disable CA1825
  private static readonly string?[] _EMPTY_ARRAY= [];
#pragma warning restore CA1825

  /// <summary>
  /// Splits a <see cref="String"/> in the format a|b|!|d||e into an <see cref="IEnumerable{T}"/>.
  /// Note: ! gets replaced by a <see langword="null"/>-reference.
  /// </summary>
  /// <param name="input">The values to split</param>
  /// <returns></returns>
  public static IEnumerable<string?>? ConvertFromStringToTestArray(string? input)
    => input == null ? null : input == string.Empty ? _EMPTY_ARRAY : input.Split('|').Select(c => c == "!" ? null : c)
    ;

  /// <summary>
  /// Whether we're currently running under something Windows-like
  /// </summary>
  /// <returns></returns>
  public static bool IsWindowsPlatform() 
    => Environment.OSVersion.Platform is PlatformID.Win32Windows or PlatformID.Win32NT or PlatformID.Win32S or PlatformID.WinCE or PlatformID.Xbox
    ;

  /// <summary>
  /// Gets a <see cref="StringComparer"/> from a <see cref="StringComparison"/>.
  /// </summary>
  /// <param name="comparison">The comparison mode</param>
  /// <returns>The comparer for the requested mode</returns>
  /// <exception cref="NotSupportedException">When the mode is unknown</exception>
  public static StringComparer FromComparison(StringComparison comparison) 
    => comparison switch {
      StringComparison.InvariantCulture => StringComparer.InvariantCulture,
      StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
      StringComparison.Ordinal => StringComparer.Ordinal,
      StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
      StringComparison.CurrentCulture => StringComparer.CurrentCulture,
      StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
      _ => throw new NotSupportedException($"Unknown comparison mode {comparison}")
    };

  /// <summary>
  /// Creates a delegate that represents a non-public instance method within a specified object's type.
  /// </summary>
  /// <typeparam name="TObject">The type of the object that contains the non-public method.</typeparam>
  /// <typeparam name="TResult">The return type of the non-public method.</typeparam>
  /// <param name="this">The instance of the class containing the non-public method.</param>
  /// <param name="methodName">The name of the non-public method to be invoked.</param>
  /// <returns>
  /// A delegate <see cref="Func{T, TResult}"/> that can be used to invoke the non-public method, 
  /// where the delegate takes an array of objects as parameters and returns a value of type <typeparamref name="TResult"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/>, or <paramref name="methodName"/> is <see langword="null"/> or an empty string.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown if the method specified by <paramref name="methodName"/> does not exist on <typeparamref name="TObject"/>.
  /// </exception>
  /// <exception cref="TargetInvocationException">
  /// Thrown if the invoked method throws an exception.
  /// </exception>
  /// <remarks>
  /// This method uses reflection to find a non-public instance method of the specified object. It then creates
  /// a delegate that can invoke that method. The method can handle methods with any number of parameters
  /// by accepting an array of objects as parameters for the method invocation.
  ///
  /// If <paramref name="args"/> is <see langword="null"/> or empty, it invokes the method with no parameters.
  ///
  /// Ensure the parameter types of the non-public method match the types in <paramref name="args"/> to avoid runtime exceptions.
  /// </remarks>
  public static Func<object?[]?, TResult?> NonPublic<TObject, TResult>(this TObject @this, string methodName) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this), "The instance cannot be null.");

    if (string.IsNullOrEmpty(methodName))
      throw new ArgumentNullException(nameof(methodName), "The method name cannot be null or empty.");

    var privateMethod = typeof(TObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

    if (privateMethod == null)
      throw new ArgumentException($"The method '{methodName}' is not found on type '{typeof(TObject).FullName}'.", nameof(methodName));

    if (privateMethod.IsGenericMethod) {
      var resultType = typeof(TResult);
      privateMethod = privateMethod.MakeGenericMethod(resultType.IsArray ? resultType.GetElementType()! : resultType);
    }

    return args => (TResult?)privateMethod.Invoke(@this, args ?? []);
  }

  /// <summary>
  /// Creates a delegate that represents a non-public static method within a specified type.
  /// </summary>
  /// <typeparam name="TType">The type that contains the non-public static method.</typeparam>
  /// <typeparam name="TResult">The return type of the non-public static method.</typeparam>
  /// <param name="methodName">The name of the non-public static method to be invoked.</param>
  /// <returns>
  /// A delegate <see cref="Func{T, TResult}"/> that can be used to invoke the non-public static method,
  /// where the delegate takes an array of objects as parameters and returns a value of type <typeparamref name="TResult"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="methodName"/> is <see langword="null"/> or an empty string.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown if the method specified by <paramref name="methodName"/> does not exist on <typeparamref name="TType"/>.
  /// </exception>
  /// <remarks>
  /// This method uses reflection to find a non-public static method of the specified type. It then creates
  /// a delegate that can invoke that method. The method can handle methods with any number of parameters
  /// by accepting an array of objects as parameters for the method invocation.
  ///
  /// If <paramref name="args"/> is <see langword="null"/> or empty, it invokes the method with no parameters.
  ///
  /// Ensure the parameter types of the non-public static method match the types in <paramref name="args"/> to avoid runtime exceptions.
  /// </remarks>
  public static Func<object?[]?, TResult?> NonPublic<TType, TResult>(string methodName) => NonPublic<TResult>(typeof(TType), methodName);
  
  /// <summary>
  /// Creates a delegate that represents a non-public static method within the specified <see cref="Type"/>.
  /// </summary>
  /// <typeparam name="TResult">The return type of the non-public static method.</typeparam>
  /// <param name="this">The <see cref="Type"/> that contains the non-public static method.</param>
  /// <param name="methodName">The name of the non-public static method to be invoked.</param>
  /// <returns>
  /// A delegate <see cref="Func{T, TResult}"/> that can be used to invoke the non-public static method,
  /// where the delegate takes an array of objects as parameters and returns a value of type <typeparamref name="TResult"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="this"/> is <see langword="null"/> or if <paramref name="methodName"/> is <see langword="null"/> or an empty string.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown if the method specified by <paramref name="methodName"/> does not exist on the <paramref name="this"/> type.
  /// </exception>
  /// <remarks>
  /// This extension method uses reflection to find a non-public static method of the specified type. It then creates
  /// a delegate that can invoke that method. The method can handle methods with any number of parameters
  /// by accepting an array of objects as parameters for the method invocation.
  ///
  /// If <paramref name="args"/> is <see langword="null"/> or empty, it invokes the method with no parameters.
  ///
  /// Ensure the parameter types of the non-public static method match the types in <paramref name="args"/> to avoid runtime exceptions.
  /// </remarks>
  public static Func<object?[]?, TResult?> NonPublic<TResult>(this Type @this, string methodName) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this), "The type cannot be null.");

    if (string.IsNullOrEmpty(methodName))
      throw new ArgumentNullException(nameof(methodName), "The method name cannot be null or empty.");

    var privateMethod = @this.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

    if (privateMethod == null)
      throw new ArgumentException($"The method '{methodName}' is not found on type '{@this.FullName}'.", nameof(methodName));

    if (privateMethod.IsGenericMethod) {
      var resultType = typeof(TResult);
      privateMethod = privateMethod.MakeGenericMethod(resultType.IsArray ? resultType.GetElementType()! : resultType);
    }

    return args => (TResult?)privateMethod.Invoke(null, args ?? []);
  }

  /// <summary>
  /// Retrieves a non-public (private or internal) type from the specified assembly by its name.
  /// </summary>
  /// <param name="this">The assembly from which to retrieve the non-public type.</param>
  /// <param name="typeName">The full name of the type to retrieve.</param>
  /// <returns>
  /// The type if found; otherwise, null. This method returns a <see cref="Type"/> object representing
  /// the non-public type, or null if the type is not found.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="this"/> is null, indicating that the method was called on a null assembly reference,
  /// or if <paramref name="typeName"/> is null or an empty string, indicating that the type name was not provided.
  /// </exception>
  /// <remarks>
  /// This method searches for types that are not public within the given assembly. It does not search
  /// for public types, and it does not throw an exception if the type is not found (instead, it returns null).
  ///
  /// <para>This method is useful for accessing types that are typically hidden from external assemblies,
  /// such as internal or private classes, structures, interfaces, or enums defined within the assembly.</para>
  ///
  /// <para>Example usage:</para>
  /// <code>
  /// var assembly = Assembly.Load("YourAssemblyName");
  /// var privateType = assembly.NonPublic("Namespace.SubNamespace.PrivateClassName");
  ///
  /// if (privateType != null)
  /// {
  ///     Console.WriteLine("Private type found: " + privateType.FullName);
  /// }
  /// else
  /// {
  ///     Console.WriteLine("Private type not found.");
  /// }
  /// </code>
  ///
  /// Note that the <paramref name="typeName"/> should include the full namespace if applicable.
  /// </remarks>
  public static Type? NonPublic(this Assembly @this, string typeName) {
    if (@this == null)
      throw new ArgumentNullException(nameof(@this), "The assembly cannot be null.");

    if (string.IsNullOrEmpty(typeName))
      throw new ArgumentNullException(nameof(typeName), "The type name cannot be null or empty.");

    return @this.GetType(typeName, false, false);
  }


}