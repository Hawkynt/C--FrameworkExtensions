using System;
using System.Collections.Generic;
using System.Linq;

using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
namespace Corlib.Tests.System.Collections.Generic;

[TestFixture]
public class ListTests {
  
  [Test]
  [TestCase(null,false,null,typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "a")]
  public void TryGetFirst(string? input,bool result,string? expected,Type? exception=null) {
    ExecuteTest(()=> {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetFirst(out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetFirst(out v);
      return (r,v);
    },(result,expected),exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "b")]
  public void TryGetLast(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetLast(out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetLast(out v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, 0, false, null, typeof(NullReferenceException))]
  [TestCase("", -1, false, null, typeof(IndexOutOfRangeException))]
  [TestCase("a", 1, false, null)]
  [TestCase("a|b|c", 1, true, "b")]
  public void TryGetItem(string? input, int index, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      string? v = null;
      var r = input == null ? ((List<string?>)null!).TryGetItem(index, out v) : ConvertFromStringToTestArray(input)?.ToList().TryGetItem(index, out v);
      return (r, v);
    }, (result, expected), exception);
  }

}

