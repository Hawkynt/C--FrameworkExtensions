using System;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace Utilities;

[TestFixture]
internal class ArrayTests {

  private static T[] Empty<T>() {
    var assembly = typeof(ArrayExtensions).Assembly;
    var @class=assembly.NonPublic("Utilities.Array");
    var method = @class!.NonPublic<T[]>("Empty");
    return method(null)!;
  }

  [Test]
  public void EmptyArrayOfTypeIntShouldBeEmptyAndCorrectType() {
    var result = ArrayTests.Empty<int>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<int[]>(result);
  }

  [Test]
  public void EmptyArrayOfTypeStringShouldBeEmptyAndCorrectType() {
    var result = ArrayTests.Empty<string>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<string[]>(result);
  }

  [Test]
  public void EmptyArrayOfTypeObjectShouldBeEmptyAndCorrectType() {
    var result = ArrayTests.Empty<object>();
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
    Assert.IsInstanceOf<object[]>(result);
  }

  [Test]
  public void EmptyArraysShouldBeSingletons() {
    var array1 = ArrayTests.Empty<int>();
    var array2 = ArrayTests.Empty<int>();
    Assert.AreSame(array1, array2, "Expected single instance for same type");
  }

  [Test]
  public void EmptyArraysOfDifferentTypesAreNotSame() {
    var intArray = ArrayTests.Empty<int>();
    var doubleArray = ArrayTests.Empty<double>();
    Assert.AreNotSame(intArray, doubleArray, "Expected different instances for different types");
  }

  [Test]
  public void EmptyArrayShouldHaveNoElements() {
    var result = ArrayTests.Empty<double>();
    Assert.AreEqual(0, result.Length);
    Assert.AreEqual(0, result.Count(), "LINQ Count should also be 0");
  }

}