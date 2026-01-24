using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class AnsiStringTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNormalString_PreservesValue() {
    var s = new AnsiString("Hello");
    Assert.AreEqual("Hello", s.ToString());
    Assert.AreEqual(5, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEmbeddedNull_PreservesNull() {
    var s = new AnsiString("Hello\0World");
    Assert.AreEqual("Hello\0World", s.ToString());
    Assert.AreEqual(11, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var s = new AnsiString(bytes);
    Assert.AreEqual("Hello", s.ToString());
    Assert.AreEqual(5, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArrayWithNull_PreservesNull() {
    var bytes = new byte[] { 72, 105, 0, 33 }; // "Hi\0!"
    var s = new AnsiString(bytes);
    Assert.AreEqual("Hi\0!", s.ToString());
    Assert.AreEqual(4, s.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullString_ReturnsEmpty() {
    var s = new AnsiString((string)null);
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyString_ReturnsEmpty() {
    var s = new AnsiString(string.Empty);
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithExtendedLatin_PreservesValue() {
    var s = new AnsiString("Héllo Wörld");
    Assert.AreEqual("Héllo Wörld", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithWindows1252SpecialChars_PreservesValue() {
    var s = new AnsiString("€™•");
    Assert.AreEqual("€™•", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithHighBytes_Converts() {
    var bytes = new byte[] { 0x80, 0x99, 0x95 }; // €, ™, •
    var s = new AnsiString(bytes);
    Assert.AreEqual("€™•", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithUnrepresentableChar_ReplacesWithQuestion() {
    var s = new AnsiString("日本");
    Assert.AreEqual("??", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var s = new AnsiString("Hello");
    Assert.AreEqual((byte)'H', s[0]);
    Assert.AreEqual((byte)'e', s[1]);
    Assert.AreEqual((byte)'o', s[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var s = new AnsiString("Hello");
    Assert.AreEqual((byte)'o', s[^1]);
    Assert.AreEqual((byte)'l', s[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var s = new AnsiString("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = s[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithTooLargeIndex_Throws() {
    var s = new AnsiString("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = s[5]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var s = new AnsiString("Hello World");
    var sub = s[0..5];
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var s = new AnsiString("Hello World");
    var sub = s.Substring(6);
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var s = new AnsiString("Hello World");
    var sub = s.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithNegativeStart_Throws() {
    var s = new AnsiString("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_FromString_Works() {
    AnsiString s = "Test";
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var s = new AnsiString("Test");
    string result = s;
    Assert.AreEqual("Test", result);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var s1 = new AnsiString("Test");
    var s2 = new AnsiString("Test");
    Assert.IsTrue(s1.Equals(s2));
    Assert.IsTrue(s1 == s2);
    Assert.IsFalse(s1 != s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var s1 = new AnsiString("Test1");
    var s2 = new AnsiString("Test2");
    Assert.IsFalse(s1.Equals(s2));
    Assert.IsFalse(s1 == s2);
    Assert.IsTrue(s1 != s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var s1 = new AnsiString("A");
    var s2 = new AnsiString("B");
    Assert.IsTrue(s1 < s2);
    Assert.IsTrue(s1 <= s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var s1 = new AnsiString("B");
    var s2 = new AnsiString("A");
    Assert.IsTrue(s1 > s2);
    Assert.IsTrue(s1 >= s2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var s1 = new AnsiString("Test");
    var s2 = new AnsiString("Test");
    Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsCorrectSpan() {
    var s = new AnsiString("Hello");
    var span = s.AsSpan();
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual((byte)'H', span[0]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var s = new AnsiString("Hi");
    var arr = s.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var s = new AnsiString("Hi");
    var arr = s.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void Concatenation_CombinesStrings() {
    var s1 = new AnsiString("Hello");
    var s2 = new AnsiString(" World");
    var result = s1 + s2;
    Assert.AreEqual("Hello World", result.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmptyAndZeroLength() {
    var s = AnsiString.Empty;
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var s = new AnsiString("Test");
    Assert.Greater(((IComparable)s).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var s = new AnsiString("Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)s).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteSpan_Works() {
    ReadOnlySpan<byte> span = new byte[] { 84, 101, 115, 116 }; // "Test"
    var s = new AnsiString(span);
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var s = new AnsiString(span);
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullByteArray_ReturnsEmpty() {
    var s = new AnsiString((byte[])null);
    Assert.IsTrue(s.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var s = new AnsiString("Test");
    unsafe {
      fixed (byte* ptr = s)
        Assert.AreEqual((byte)'T', *ptr);
    }
  }

  [Test]
  [Category("Regression")]
  public void DifferenceFromAnsiZ_PreservesNulls() {
    var bytes = new byte[] { 65, 0, 66, 0, 67 }; // "A\0B\0C"
    var s = new AnsiString(bytes);
    Assert.AreEqual(5, s.Length);
    Assert.AreEqual(0, s[1]);
    Assert.AreEqual((byte)'B', s[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void RoundTrip_WithLatin1Chars_PreservesValue() {
    var original = "Café résumé naïve";
    var s = new AnsiString(original);
    Assert.AreEqual(original, s.ToString());
  }

}
