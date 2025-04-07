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
//

using System.Linq;
using NUnit.Framework;

namespace System.Buffers.ArrayPoolTests;

[TestFixture]
public class ArrayPoolPolyfillTests {
  [Test]
  public void Shared_ReturnsInstanceForValueType() {
    // Act
    var pool = ArrayPool<int>.Shared;

    // Assert
    Assert.IsNotNull(pool);
    Assert.IsInstanceOf<ArrayPool<int>>(pool);
    Assert.That(() => pool.Rent(1)[0] = 42, Throws.Nothing);
  }

  [Test]
  public void Shared_ReturnsInstanceForReferenceType() {
    // Act
    var pool = ArrayPool<string>.Shared;

    // Assert
    Assert.IsNotNull(pool);
    Assert.IsInstanceOf<ArrayPool<string>>(pool);
    Assert.That(() => pool.Rent(1)[0] = "test", Throws.Nothing);
  }

  [Test]
  public void Create_ReturnsConfigurableArrayPool() {
    // Act
    var pool = ArrayPool<int>.Create();

    // Assert
    Assert.IsNotNull(pool);
    Assert.IsInstanceOf<ArrayPool<int>>(pool);
  }

  [Test]
  public void Create_WithParameters_ReturnsConfigurableArrayPool() {
    // Act
    var pool = ArrayPool<int>.Create(1024, 16);

    // Assert
    Assert.IsNotNull(pool);
    Assert.IsInstanceOf<ArrayPool<int>>(pool);
  }

  [Test]
  public void Create_WithInvalidParameters_ThrowsArgumentOutOfRangeException() {
    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<int>.Create(0, 10));
    Assert.Throws<ArgumentOutOfRangeException>(() => ArrayPool<int>.Create(100, 0));
  }

  [Test]
  public void Rent_ReturnsArrayOfAtLeastRequestedSize() {
    // Arrange
    var pool = ArrayPool<int>.Create();

    // Act
    var array = pool.Rent(100);

    // Assert
    Assert.IsNotNull(array);
    Assert.GreaterOrEqual(array.Length, 100);
  }

  [Test]
  public void Rent_WithZeroLength_ReturnsEmptyArray() {
    // Arrange
    var pool = ArrayPool<int>.Create();

    // Act
    var array = pool.Rent(0);

    // Assert
    Assert.IsNotNull(array);
    Assert.AreEqual(0, array.Length);
  }

  [Test]
  public void Rent_WithNegativeLength_ThrowsArgumentOutOfRangeException() {
    // Arrange
    var pool = ArrayPool<int>.Create();

    // Act & Assert
    Assert.Throws<ArgumentOutOfRangeException>(() => pool.Rent(-1));
  }

  [Test]
  public void Return_WithNull_DoesThrow() {
    // Arrange
    var pool = ArrayPool<int>.Create();

    // Act & Assert
    Assert.That(() => pool.Return(null!), Throws.TypeOf<ArgumentNullException>());
  }

  [Test]
  public void RentAndReturn_ReusesSameArray() {
    // Arrange
    var pool = ArrayPool<int>.Create(1024, 10);

    // Act
    var array1 = pool.Rent(100);
    pool.Return(array1);
    var array2 = pool.Rent(100);

    // Assert
    Assert.AreSame(array1, array2);
  }

  [Test]
  public void Return_WithClearArray_ClearsArrayContents() {
    // Arrange
    var pool = ArrayPool<int>.Create();
    var array = pool.Rent(10);

    // Fill the array with values
    for (int i = 0; i < array.Length; i++)
      array[i] = 42;

    // Act
    pool.Return(array, true);
    var newArray = pool.Rent(10);

    // Assert
    Assert.AreSame(array, newArray);
    Assert.AreEqual(0, newArray.Sum()); // All values should be cleared to 0
  }

  [Test]
  public void Return_WithoutClearArray_PreservesArrayContents() {
    // Arrange
    var pool = ArrayPool<int>.Create();
    var array = pool.Rent(10);

    // Fill the array with values
    for (int i = 0; i < array.Length; i++)
      array[i] = 42;

    // Act
    pool.Return(array, false);
    var newArray = pool.Rent(10);

    // Assert
    Assert.AreSame(array, newArray);
    Assert.AreEqual(42 * array.Length, newArray.Sum()); // Values should be preserved
  }

  [Test]
  public void Rent_MultipleTimes_ReturnsDifferentArrays() {
    // Arrange
    var pool = ArrayPool<int>.Create(1024, 1); // Only 1 array per bucket

    // Act
    var array1 = pool.Rent(100);
    var array2 = pool.Rent(100);

    // Assert
    Assert.AreNotSame(array1, array2);
  }

  [Test]
  public void Rent_ExceedingMaxArrayLength_AllocatesNewArray() {
    // Arrange
    var pool = ArrayPool<int>.Create(100, 10); // Max array length = 100

    // Act
    var array = pool.Rent(200);

    // Assert
    Assert.IsNotNull(array);
    Assert.GreaterOrEqual(array.Length, 200);
  }

  [Test]
  public void Return_NonPowerOfTwoSizedArray_ThrowsException() {
    // Arrange
    var pool = ArrayPool<int>.Create();
    var array = new int[123]; // Not a power of 2

    Assert.That(()=> pool.Return(array),Throws.TypeOf<ArgumentException>());
  }

  [Test]
  public void SharedPool_ReturnsSameInstanceForSameType() {
    // Act
    var pool1 = ArrayPool<int>.Shared;
    var pool2 = ArrayPool<int>.Shared;

    // Assert
    Assert.AreSame(pool1, pool2);
  }

  [Test]
  public void SharedPool_ReturnsDifferentInstanceForDifferentTypes() {
    // Act
    var intPool = ArrayPool<int>.Shared;
    var stringPool = ArrayPool<string>.Shared;

    // Assert
    Assert.AreNotSame(intPool, stringPool);
  }

}