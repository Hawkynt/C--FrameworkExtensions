﻿using System.Linq;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Collections.Generic;

[TestFixture]
public class ListTests {
  
  [Test]
  [TestCase(null,false,null,typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "a")]
  public void TryGetFirst(string? input,bool result,string? expected,Type? exception=null) 
    => ExecuteTest(()=> {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetFirst(out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetFirst(out v);
      return (r,v);
    },(result,expected),exception)
    ;

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "b")]
  public void TryGetLast(string? input, bool result, string? expected, Type? exception = null) 
    => ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetLast(out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetLast(out v);
      return (r, v);
    }, (result, expected), exception)
    ;

  [Test]
  [TestCase(null, 0, false, null, typeof(NullReferenceException))]
  [TestCase("", -1, false, null, typeof(IndexOutOfRangeException))]
  [TestCase("a", 1, false, null)]
  [TestCase("a|b|c", 1, true, "b")]
  public void TryGetItem(string? input, int index, bool result, string? expected, Type? exception = null) 
    => ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetItem(index, out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetItem(index, out v);
      return (r, v);
    }, (result, expected), exception)
    ;

  [Test]
  [TestCase(null, false, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", true)]
  public void Any(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => input == null ? ((List<string?>)null!).Any() : ConvertFromStringToTestArray(input)?.ToList().Any(), expected, exception)
    ;

  [Test]
  [TestCase(null,null,false,null,typeof(NullReferenceException))]
  [TestCase("",null,false,-1)]
  [TestCase("!", null, false, 0)]
  [TestCase("1", "1", false, 0)]
  [TestCase("1|2", "2", false, 1)]
  [TestCase("1|3", "2", false, -1)]
  [TestCase("1|3", "2", true, 1)]
  [TestCase("1|3", null, true, 0)]
  public void BinarySearchIndex(string? input,string? value,bool allowGreater, int expected,Type? exception=null) 
    => ExecuteTest(()=> input == null ? ((List<string?>)null!).BinarySearchIndex(value,allowGreater) : ConvertFromStringToTestArray(input)?.ToList().BinarySearchIndex(value, allowGreater), expected,exception)
    ;
}
