using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace System;

[TestFixture]
internal class EnumTests {

  private sealed class MyConversionAttribute(string value) : Attribute {
    public string Value => value;
  }

  public enum TestType {
    Unspecified,
    [MyConversion("kn")]
    Known,
    [MyConversion("un kn")]
    Unknown,
  }

  [Test]
  [TestCase("kn2",false,TestType.Unspecified)]
  [TestCase("kn", true, TestType.Known)]
  [TestCase("un kn", true, TestType.Unknown)]
  public void TryParseEnum_ShouldReturnValue(string value,bool couldParse, TestType result) {
    var cp = value.TryParseEnum<TestType, MyConversionAttribute>((a, v) => a.Value == v, out var res);
    Assert.That(cp,Is.EqualTo(couldParse));
    Assert.That(res,Is.EqualTo(result));
  }

  [Test]
  [TestCase("kn2", false, TestType.Unspecified)]
  [TestCase("kn", true, TestType.Known)]
  [TestCase("un kn", true, TestType.Unknown)]
  public void TryParseEnum_ShouldReturnValue2(string value, bool couldParse, TestType result) {
    var cp = value.TryParseEnum<TestType, MyConversionAttribute>(a => a.Value, out var res);
    Assert.That(cp, Is.EqualTo(couldParse));
    Assert.That(res, Is.EqualTo(result));
  }
}
