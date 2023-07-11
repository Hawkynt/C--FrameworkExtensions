using System;
using System.Collections.Generic;

using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
namespace Corlib.Tests.System.Collections.Generic;

[TestFixture]
public class EnumerableTests {

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsNoSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsNoMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(s => s == search), expected, exception);

}