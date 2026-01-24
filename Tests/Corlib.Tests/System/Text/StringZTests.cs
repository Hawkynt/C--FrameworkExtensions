using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class StringZTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNormalString_PreservesValue() {
    var sz = new StringZ("Hello");
    Assert.AreEqual("Hello", sz.ToString());
    Assert.AreEqual(5, sz.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEmbeddedNull_TruncatesAtNull() {
    var sz = new StringZ("Hello\0World");
    Assert.AreEqual("Hello", sz.ToString());
    Assert.AreEqual(5, sz.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithMultipleNulls_TruncatesAtFirstNull() {
    var sz = new StringZ("A\0B\0C");
    Assert.AreEqual("A", sz.ToString());
    Assert.AreEqual(1, sz.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_TruncatesAtNull() {
    ReadOnlySpan<char> span = "Test\0Data".AsSpan();
    var sz = new StringZ(span);
    Assert.AreEqual("Test", sz.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharArray_TruncatesAtNull() {
    var chars = new[] { 'H', 'i', '\0', '!' };
    var sz = new StringZ(chars);
    Assert.AreEqual("Hi", sz.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullString_ReturnsEmpty() {
    var sz = new StringZ((string)null);
    Assert.AreEqual(string.Empty, sz.ToString());
    Assert.IsTrue(sz.IsEmpty);
    Assert.AreEqual(0, sz.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyString_ReturnsEmpty() {
    var sz = new StringZ(string.Empty);
    Assert.IsTrue(sz.IsEmpty);
    Assert.AreEqual(0, sz.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithLeadingNull_ReturnsEmpty() {
    var sz = new StringZ("\0Hello");
    Assert.IsTrue(sz.IsEmpty);
    Assert.AreEqual(0, sz.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithOnlyNull_ReturnsEmpty() {
    var sz = new StringZ("\0");
    Assert.IsTrue(sz.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectChar() {
    var sz = new StringZ("Hello");
    Assert.AreEqual('H', sz[0]);
    Assert.AreEqual('e', sz[1]);
    Assert.AreEqual('o', sz[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectChar() {
    var sz = new StringZ("Hello");
    Assert.AreEqual('o', sz[^1]);
    Assert.AreEqual('l', sz[^2]);
    Assert.AreEqual('H', sz[^5]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_ThrowsIndexOutOfRange() {
    var sz = new StringZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = sz[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithTooLargeIndex_ThrowsIndexOutOfRange() {
    var sz = new StringZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = sz[5]);
    Assert.Throws<IndexOutOfRangeException>(() => _ = sz[100]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var sz = new StringZ("Hello World");
    var sub = sz[0..5];
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRangeFromEnd_ReturnsSubstring() {
    var sz = new StringZ("Hello World");
    var sub = sz[6..^0];
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var sz = new StringZ("Hello World");
    var sub = sz.Substring(6);
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var sz = new StringZ("Hello World");
    var sub = sz.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Substring_AtEnd_ReturnsEmpty() {
    var sz = new StringZ("Hello");
    var sub = sz.Substring(5);
    Assert.IsTrue(sub.IsEmpty);
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithNegativeStart_Throws() {
    var sz = new StringZ("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => sz.Substring(-1));
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithStartBeyondLength_Throws() {
    var sz = new StringZ("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => sz.Substring(6));
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithLengthBeyondEnd_Throws() {
    var sz = new StringZ("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => sz.Substring(3, 5));
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_FromString_Works() {
    StringZ sz = "Test";
    Assert.AreEqual("Test", sz.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var sz = new StringZ("Test");
    string s = sz;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_RoundTrip_PreservesValue() {
    StringZ sz = "Original";
    string s = sz;
    StringZ sz2 = s;
    Assert.AreEqual(sz, sz2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var sz1 = new StringZ("Test");
    var sz2 = new StringZ("Test");
    Assert.IsTrue(sz1.Equals(sz2));
    Assert.IsTrue(sz1 == sz2);
    Assert.IsFalse(sz1 != sz2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var sz1 = new StringZ("Test1");
    var sz2 = new StringZ("Test2");
    Assert.IsFalse(sz1.Equals(sz2));
    Assert.IsFalse(sz1 == sz2);
    Assert.IsTrue(sz1 != sz2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var sz1 = new StringZ("A");
    var sz2 = new StringZ("B");
    Assert.IsTrue(sz1 < sz2);
    Assert.IsTrue(sz1 <= sz2);
    Assert.IsFalse(sz1 > sz2);
    Assert.IsFalse(sz1 >= sz2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var sz1 = new StringZ("B");
    var sz2 = new StringZ("A");
    Assert.IsTrue(sz1 > sz2);
    Assert.IsTrue(sz1 >= sz2);
    Assert.IsFalse(sz1 < sz2);
    Assert.IsFalse(sz1 <= sz2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_Equal_Works() {
    var sz1 = new StringZ("Test");
    var sz2 = new StringZ("Test");
    Assert.IsTrue(sz1 <= sz2);
    Assert.IsTrue(sz1 >= sz2);
    Assert.AreEqual(0, sz1.CompareTo(sz2));
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var sz1 = new StringZ("Test");
    var sz2 = new StringZ("Test");
    Assert.AreEqual(sz1.GetHashCode(), sz2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsCorrectSpan() {
    var sz = new StringZ("Hello");
    var span = sz.AsSpan();
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual('H', span[0]);
    Assert.AreEqual('o', span[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_WithRange_ReturnsCorrectSpan() {
    var sz = new StringZ("Hello World");
    var span = sz.AsSpan(6, 5);
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual("World", span.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var sz = new StringZ("Hi");
    var arr = sz.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual('H', arr[0]);
    Assert.AreEqual('i', arr[1]);
    Assert.AreEqual('\0', arr[2]);
  }

  [Test]
  [Category("EdgeCase")]
  public void ToNullTerminatedArray_EmptyString_ReturnsNullOnly() {
    var sz = new StringZ(string.Empty);
    var arr = sz.ToNullTerminatedArray();
    Assert.AreEqual(1, arr.Length);
    Assert.AreEqual('\0', arr[0]);
  }

  [Test]
  [Category("HappyPath")]
  public void Concatenation_CombinesStrings() {
    var sz1 = new StringZ("Hello");
    var sz2 = new StringZ(" World");
    var result = sz1 + sz2;
    Assert.AreEqual("Hello World", result.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmptyAndZeroLength() {
    var sz = StringZ.Empty;
    Assert.IsTrue(sz.IsEmpty);
    Assert.AreEqual(0, sz.Length);
    Assert.AreEqual(string.Empty, sz.ToString());
  }

  [Test]
  [Category("Regression")]
  public void ZeroTerminatedBehavior_WithEmbeddedNul_CutsCorrectly() {
    var original = "First\0Second\0Third";
    var sz = new StringZ(original);
    Assert.AreEqual("First", sz.ToString());
    Assert.AreEqual(5, sz.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var sz = new StringZ("Test");
    Assert.Greater(((IComparable)sz).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var sz = new StringZ("Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)sz).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var sz = new StringZ("Test");
    unsafe {
      fixed (char* ptr = sz)
        Assert.AreEqual('T', *ptr);
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullCharArray_ReturnsEmpty() {
    var sz = new StringZ((char[])null);
    Assert.IsTrue(sz.IsEmpty);
  }

}
