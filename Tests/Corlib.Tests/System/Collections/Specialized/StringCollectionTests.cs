using System.Collections.Specialized;
using NUnit.Framework;

namespace System.Collections.Specialized;

[TestFixture]
public class StringCollectionTests {

  [Test]
  public void ToArray_EmptyCollection_ReturnsEmptyArray() {
    // Arrange
    var collection = new StringCollection();
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(0, result.Length);
  }

  [Test]
  public void ToArray_SingleElement_ReturnsArrayWithOneElement() {
    // Arrange
    var collection = new StringCollection { "test" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(1, result.Length);
    Assert.AreEqual("test", result[0]);
  }

  [Test]
  public void ToArray_MultipleElements_ReturnsArrayWithAllElements() {
    // Arrange
    var collection = new StringCollection { "first", "second", "third" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual("first", result[0]);
    Assert.AreEqual("second", result[1]);
    Assert.AreEqual("third", result[2]);
  }

  [Test]
  public void ToArray_WithNullValues_ReturnsArrayWithNulls() {
    // Arrange
    var collection = new StringCollection { "first", null, "third" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual("first", result[0]);
    Assert.IsNull(result[1]);
    Assert.AreEqual("third", result[2]);
  }

  [Test]
  public void ToArray_WithEmptyStrings_ReturnsArrayWithEmptyStrings() {
    // Arrange
    var collection = new StringCollection { "", "test", "" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(3, result.Length);
    Assert.AreEqual("", result[0]);
    Assert.AreEqual("test", result[1]);
    Assert.AreEqual("", result[2]);
  }

  [Test]
  public void ToArray_WithDuplicateValues_ReturnsArrayWithDuplicates() {
    // Arrange
    var collection = new StringCollection { "test", "test", "different", "test" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(4, result.Length);
    Assert.AreEqual("test", result[0]);
    Assert.AreEqual("test", result[1]);
    Assert.AreEqual("different", result[2]);
    Assert.AreEqual("test", result[3]);
  }

  [Test]
  public void ToArray_ModificationAfterCall_DoesNotAffectReturnedArray() {
    // Arrange
    var collection = new StringCollection { "original" };
    var result = collection.ToArray();
    
    // Act
    collection.Add("new");
    collection[0] = "modified";
    
    // Assert
    Assert.AreEqual(1, result.Length);
    Assert.AreEqual("original", result[0]);
  }

  [Test]
  public void ToArray_LargeCollection_ReturnsCorrectArray() {
    // Arrange - Create collection with 1000 elements
    var collection = new StringCollection();
    var expected = new string[1000];
    for (var i = 0; i < 1000; i++) {
      var value = $"item_{i:D4}";
      collection.Add(value);
      expected[i] = value;
    }
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(1000, result.Length);
    CollectionAssert.AreEqual(expected, result);
  }

  [Test]
  public void ToArray_WithUnicodeStrings_ReturnsCorrectArray() {
    // Arrange
    var collection = new StringCollection { "Hello", "世界", "🌍", "Ñiño" };
    
    // Act
    var result = collection.ToArray();
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(4, result.Length);
    Assert.AreEqual("Hello", result[0]);
    Assert.AreEqual("世界", result[1]);
    Assert.AreEqual("🌍", result[2]);
    Assert.AreEqual("Ñiño", result[3]);
  }

  [Test]
  public void ToArray_NullCollection_ThrowsArgumentNullException() {
    // Arrange
    StringCollection collection = null;
    
    // Act & Assert
    Assert.Throws<NullReferenceException>(() => collection.ToArray());
  }

}