using System;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace Utilities;

[TestFixture]
internal class ArrayTests {
  private static T[] Empty<T>() {
    var assembly = typeof(ArrayExtensions).Assembly;
    var @class = assembly.NonPublic("Utilities.Array");
    var method = @class!.NonPublic<T[]>("Empty");
    return method(null)!;
  }

  [Test]
  public void EmptyArrayOfTypeIntShouldBeEmptyAndCorrectType() {
    var result = Empty<int>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<int[]>(result);
  }

  [Test]
  public void EmptyArrayOfTypeStringShouldBeEmptyAndCorrectType() {
    var result = Empty<string>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<string[]>(result);
  }

  [Test]
  public void EmptyArrayOfTypeObjectShouldBeEmptyAndCorrectType() {
    var result = Empty<object>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<object[]>(result);
  }

  [Test]
  public void EmptyArraysShouldBeSingletons() {
    var array1 = Empty<int>();
    var array2 = Empty<int>();
    Assert.AreSame(array1, array2, "Expected single instance for same type");
  }

  [Test]
  public void EmptyArraysOfDifferentTypesAreNotSame() {
    var intArray = Empty<int>();
    var doubleArray = Empty<double>();
    Assert.AreNotSame(intArray, doubleArray, "Expected different instances for different types");
  }

  [Test]
  public void EmptyArrayShouldHaveNoElements() {
    var result = Empty<double>();
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, result.Count(), "LINQ Count should also be 0");
  }
}
