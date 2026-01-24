using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class AsciiStringTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNormalString_PreservesValue() {
    var s = new AsciiString("Hello");
    Assert.AreEqual("Hello", s.ToString());
    Assert.AreEqual(5, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEmbeddedNull_PreservesNull() {
    var s = new AsciiString("Hello\0World");
    Assert.AreEqual("Hello\0World", s.ToString());
    Assert.AreEqual(11, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var s = new AsciiString(bytes);
    Assert.AreEqual("Hello", s.ToString());
    Assert.AreEqual(5, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArrayWithNull_PreservesNull() {
    var bytes = new byte[] { 72, 105, 0, 33 }; // "Hi\0!"
    var s = new AsciiString(bytes);
    Assert.AreEqual("Hi\0!", s.ToString());
    Assert.AreEqual(4, s.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullString_ReturnsEmpty() {
    var s = new AsciiString((string)null);
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyString_ReturnsEmpty() {
    var s = new AsciiString(string.Empty);
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNonAsciiChar_Throws() {
    Assert.Throws<ArgumentException>(() => new AsciiString("Héllo"));
    Assert.Throws<ArgumentException>(() => new AsciiString("日本語"));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNonAsciiByte_Throws() {
    var bytes = new byte[] { 72, 200, 108, 108, 111 }; // byte > 127
    Assert.Throws<ArgumentException>(() => new AsciiString(bytes));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithReplaceMode_ReplacesInvalidChars() {
    var s = new AsciiString("Héllo", InvalidCharBehavior.Replace);
    Assert.AreEqual("H?llo", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithSkipMode_SkipsInvalidChars() {
    var s = new AsciiString("Héllo", InvalidCharBehavior.Skip);
    Assert.AreEqual("Hllo", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var s = new AsciiString("Hello");
    Assert.AreEqual((byte)'H', s[0]);
    Assert.AreEqual((byte)'e', s[1]);
    Assert.AreEqual((byte)'o', s[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var s = new AsciiString("Hello");
    Assert.AreEqual((byte)'o', s[^1]);
    Assert.AreEqual((byte)'l', s[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var s = new AsciiString("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = s[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithTooLargeIndex_Throws() {
    var s = new AsciiString("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = s[5]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var s = new AsciiString("Hello World");
    var sub = s[0..5];
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var s = new AsciiString("Hello World");
    var sub = s.Substring(6);
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var s = new AsciiString("Hello World");
    var sub = s.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithNegativeStart_Throws() {
    var s = new AsciiString("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_FromString_Works() {
    AsciiString s = "Test";
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var s = new AsciiString("Test");
    string result = s;
    Assert.AreEqual("Test", result);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var s1 = new AsciiString("Test");
    var s2 = new AsciiString("Test");
    Assert.IsTrue(s1.Equals(s2));
    Assert.IsTrue(s1 == s2);
    Assert.IsFalse(s1 != s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var s1 = new AsciiString("Test1");
    var s2 = new AsciiString("Test2");
    Assert.IsFalse(s1.Equals(s2));
    Assert.IsFalse(s1 == s2);
    Assert.IsTrue(s1 != s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var s1 = new AsciiString("A");
    var s2 = new AsciiString("B");
    Assert.IsTrue(s1 < s2);
    Assert.IsTrue(s1 <= s2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var s1 = new AsciiString("B");
    var s2 = new AsciiString("A");
    Assert.IsTrue(s1 > s2);
    Assert.IsTrue(s1 >= s2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var s1 = new AsciiString("Test");
    var s2 = new AsciiString("Test");
    Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsPackedSpan() {
    var s = new AsciiString("Hello");
    var span = s.AsSpan();
    // AsSpan returns packed 7-bit data (no allocation)
    // 5 chars * 7 bits = 35 bits = 5 packed bytes
    Assert.AreEqual(5, span.Length);
    // Verify packed data is accessible (not unpacked characters)
    Assert.IsTrue(span.Length > 0);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var s = new AsciiString("Hi");
    var arr = s.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var s = new AsciiString("Hi");
    var arr = s.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void Concatenation_CombinesStrings() {
    var s1 = new AsciiString("Hello");
    var s2 = new AsciiString(" World");
    var result = s1 + s2;
    Assert.AreEqual("Hello World", result.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmptyAndZeroLength() {
    var s = AsciiString.Empty;
    Assert.IsTrue(s.IsEmpty);
    Assert.AreEqual(0, s.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var s = new AsciiString("Test");
    Assert.Greater(((IComparable)s).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var s = new AsciiString("Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)s).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteSpan_Works() {
    ReadOnlySpan<byte> span = new byte[] { 84, 101, 115, 116 }; // "Test"
    var s = new AsciiString(span);
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var s = new AsciiString(span);
    Assert.AreEqual("Test", s.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullByteArray_ReturnsEmpty() {
    var s = new AsciiString((byte[])null);
    Assert.IsTrue(s.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var s = new AsciiString("Test");
    unsafe {
      fixed (byte* ptr = s) {
        // GetPinnableReference returns packed data, so verify pointer is valid
        // and that we can dereference it without crashing
        Assert.IsTrue(ptr != null);
        _ = *ptr; // Should not throw
      }
    }
    // Verify the indexer still returns correct unpacked values
    Assert.AreEqual((byte)'T', s[0]);
  }

  [Test]
  [Category("Regression")]
  public void DifferenceFromAsciiZ_PreservesNulls() {
    var bytes = new byte[] { 65, 0, 66, 0, 67 }; // "A\0B\0C"
    var s = new AsciiString(bytes);
    Assert.AreEqual(5, s.Length);
    Assert.AreEqual(0, s[1]);
    Assert.AreEqual((byte)'B', s[2]);
  }

}
