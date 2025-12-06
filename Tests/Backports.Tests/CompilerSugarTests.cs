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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Backports.Tests;

/// <summary>
/// Tests for compiler sugar features.
/// These tests verify that compiler features work correctly - the test passes if the code compiles and runs.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("CompilerSugar")]
public class CompilerSugarTests {

  #region ValueTuple Syntax

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_TupleLiteralSyntax_Compiles() {
    var tuple = (1, "hello", 3.14);
    Assert.That(tuple.Item1, Is.EqualTo(1));
    Assert.That(tuple.Item2, Is.EqualTo("hello"));
    Assert.That(tuple.Item3, Is.EqualTo(3.14));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_NamedElements_Compiles() {
    var tuple = (Id: 42, Name: "Test");
    Assert.That(tuple.Id, Is.EqualTo(42));
    Assert.That(tuple.Name, Is.EqualTo("Test"));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_Deconstruction_Compiles() {
    var tuple = (10, 20, 30);
    var (a, b, c) = tuple;
    Assert.That(a, Is.EqualTo(10));
    Assert.That(b, Is.EqualTo(20));
    Assert.That(c, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_DeconstructionWithDiscard_Compiles() {
    var tuple = (1, 2, 3, 4);
    var (first, _, _, last) = tuple;
    Assert.That(first, Is.EqualTo(1));
    Assert.That(last, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_MethodReturn_Compiles() {
    var result = GetMinMax(new[] { 5, 2, 8, 1, 9 });
    Assert.That(result.Min, Is.EqualTo(1));
    Assert.That(result.Max, Is.EqualTo(9));
  }

  private static (int Min, int Max) GetMinMax(int[] values) {
    var min = values[0];
    var max = values[0];
    for (var i = 1; i < values.Length; ++i) {
      if (values[i] < min)
        min = values[i];
      if (values[i] > max)
        max = values[i];
    }

    return (min, max);
  }

  [Test]
  [Category("HappyPath")]
  public void ValueTuple_Equality_Compiles() {
    var a = (1, 2);
    var b = (1, 2);
    var c = (1, 3);
    Assert.That(a == b, Is.True);
    Assert.That(a != c, Is.True);
  }

  #endregion

  #region Index and Range Syntax

  [Test]
  [Category("HappyPath")]
  public void Index_FromEnd_Compiles() {
    var index = ^1;
    Assert.That(index.IsFromEnd, Is.True);
    Assert.That(index.Value, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Index_FromStart_Compiles() {
    Index index = 5;
    Assert.That(index.IsFromEnd, Is.False);
    Assert.That(index.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_StartEnd_Compiles() {
    var range = 1..5;
    Assert.That(range.Start.Value, Is.EqualTo(1));
    Assert.That(range.End.Value, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void Range_FromEndSyntax_Compiles() {
    var range = ^3..^1;
    Assert.That(range.Start.IsFromEnd, Is.True);
    Assert.That(range.End.IsFromEnd, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Range_FullRange_Compiles() {
    var range = ..;
    Assert.That(range.Start.Value, Is.EqualTo(0));
    Assert.That(range.End.IsFromEnd, Is.True);
  }

  #endregion

  #region CallerArgumentExpression

  [Test]
  [Category("HappyPath")]
  public void CallerArgumentExpression_CapturesExpression() {
    var value = 42;
    var result = CaptureExpression(value);
    Assert.That(result, Is.EqualTo("value"));
  }

  [Test]
  [Category("HappyPath")]
  public void CallerArgumentExpression_CapturesComplexExpression() {
    var result = CaptureExpression(1 + 2 + 3);
    Assert.That(result, Is.EqualTo("1 + 2 + 3"));
  }

  private static string CaptureExpression(int value, [CallerArgumentExpression(nameof(value))] string expression = null) => expression;

  #endregion

  #region CallerMemberName / CallerFilePath / CallerLineNumber

  [Test]
  [Category("HappyPath")]
  public void CallerMemberName_CapturesMethodName() {
    var memberName = GetCallerMemberName();
    Assert.That(memberName, Is.EqualTo(nameof(CallerMemberName_CapturesMethodName)));
  }

  [Test]
  [Category("HappyPath")]
  public void CallerFilePath_CapturesFilePath() {
    var filePath = GetCallerFilePath();
    Assert.That(filePath, Does.EndWith("CompilerSugarTests.cs"));
  }

  [Test]
  [Category("HappyPath")]
  public void CallerLineNumber_CapturesLineNumber() {
    var lineNumber = GetCallerLineNumber();
    Assert.That(lineNumber, Is.GreaterThan(0));
  }

  private static string GetCallerMemberName([CallerMemberName] string memberName = null) => memberName;

  private static string GetCallerFilePath([CallerFilePath] string filePath = null) => filePath;

  private static int GetCallerLineNumber([CallerLineNumber] int lineNumber = 0) => lineNumber;

  #endregion

  #region Yield Return

  [Test]
  [Category("HappyPath")]
  public void YieldReturn_GeneratesSequence() {
    var result = new List<int>();
    foreach (var item in GenerateNumbers(5))
      result.Add(item);

    Assert.That(result, Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
  }

  [Test]
  [Category("HappyPath")]
  public void YieldReturn_SupportsBreak() {
    var result = new List<int>();
    foreach (var item in GenerateNumbers(100)) {
      if (item >= 3)
        break;
      result.Add(item);
    }

    Assert.That(result, Is.EqualTo(new[] { 0, 1, 2 }));
  }

  [Test]
  [Category("HappyPath")]
  public void YieldReturn_IsLazy() {
    var evaluationCount = 0;
    IEnumerable<int> GetNumbersWithSideEffect() {
      for (var i = 0; i < 10; ++i) {
        ++evaluationCount;
        yield return i;
      }
    }

    var enumerable = GetNumbersWithSideEffect();
    Assert.That(evaluationCount, Is.EqualTo(0));

    using var enumerator = enumerable.GetEnumerator();
    enumerator.MoveNext();
    Assert.That(evaluationCount, Is.EqualTo(1));
  }

  private static IEnumerable<int> GenerateNumbers(int count) {
    for (var i = 0; i < count; ++i)
      yield return i;
  }

  #endregion

  #region Extension Methods

  [Test]
  [Category("HappyPath")]
  public void ExtensionMethod_OnString_Compiles() {
    var result = "hello".Reverse();
    Assert.That(result, Is.EqualTo("olleh"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExtensionMethod_OnGenericType_Compiles() {
    var list = new List<int> { 1, 2, 3 };
    var doubled = list.DoubleAll();
    Assert.That(doubled, Is.EqualTo(new[] { 2, 4, 6 }));
  }

  #endregion

  #region Nullable Reference Types Annotations

  [Test]
  [Category("HappyPath")]
  public void NullableAnnotation_NotNull_Compiles() {
    var result = EnsureNotNull("test");
    Assert.That(result, Is.Not.Null);
  }

  private static string EnsureNotNull([System.Diagnostics.CodeAnalysis.NotNull] string value) {
    if (value == null)
      throw new ArgumentNullException(nameof(value));
    return value;
  }

  #endregion

}

/// <summary>
/// Extension methods for testing extension method compiler support.
/// </summary>
internal static class CompilerSugarTestExtensions {

  public static string Reverse(this string str) {
    var chars = str.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
  }

  public static IEnumerable<int> DoubleAll(this IEnumerable<int> source) {
    foreach (var item in source)
      yield return item * 2;
  }

}
