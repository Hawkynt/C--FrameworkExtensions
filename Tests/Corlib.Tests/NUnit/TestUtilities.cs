using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Corlib.Tests.NUnit; 

internal class TestUtilities {
  
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
  private static readonly string?[] _EMPTY_ARRAY= new string?[0];
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
}