﻿using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
namespace Corlib.Tests.System.Collections.Generic;

using global::System;
using global::System.Collections.Generic;

[TestFixture]
public class EnumerableTests {

  private static IEnumerable<string>? _ConvertFromString(string? input) => input?.Split('|');
  private static IEnumerable<string>? _ConvertFromStringAllowEmpty(string? input) => input == null ? null : input == string.Empty ? new string[0] : input.Split('|');


  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromStringAllowEmpty(input).IsSingle(), expected, exception)
  ;

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsNoSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromStringAllowEmpty(input).IsNoSingle(), expected, exception)
  ;

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromString(input).IsMultiple(), expected, exception)
  ;

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsNoMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromString(input).IsNoMultiple(), expected, exception)
  ;

  [Test]
  [TestCase(null,null,false,typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSingle(string? input,string? search,bool expected,Type? exception=null) 
    => ExecuteTest(()=>_ConvertFromString(input).HasSingle(search),expected,exception)
    ;

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromString(input).HasNoSingle(search), expected, exception)
  ;

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromString(input).HasMultiple(search), expected, exception)
  ;

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => _ConvertFromString(input).HasNoMultiple(search), expected, exception)
  ;

}