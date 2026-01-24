using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class AsciiZTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNormalString_PreservesValue() {
    var az = new AsciiZ("Hello");
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEmbeddedNull_TruncatesAtNull() {
    var az = new AsciiZ("Hello\0World");
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var az = new AsciiZ(bytes);
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArrayWithNull_Truncates() {
    var bytes = new byte[] { 72, 105, 0, 33 }; // "Hi\0!"
    var az = new AsciiZ(bytes);
    Assert.AreEqual("Hi", az.ToString());
    Assert.AreEqual(2, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullString_ReturnsEmpty() {
    var az = new AsciiZ((string)null);
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyString_ReturnsEmpty() {
    var az = new AsciiZ(string.Empty);
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithLeadingNull_ReturnsEmpty() {
    var az = new AsciiZ("\0Hello");
    Assert.IsTrue(az.IsEmpty);
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNonAsciiChar_Throws() {
    Assert.Throws<ArgumentException>(() => new AsciiZ("Héllo"));
    Assert.Throws<ArgumentException>(() => new AsciiZ("日本語"));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNonAsciiByte_Throws() {
    var bytes = new byte[] { 72, 200, 108, 108, 111 }; // byte > 127
    Assert.Throws<ArgumentException>(() => new AsciiZ(bytes));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithReplaceMode_ReplacesInvalidChars() {
    var az = new AsciiZ("Héllo", InvalidCharBehavior.Replace);
    Assert.AreEqual("H?llo", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithSkipMode_SkipsInvalidChars() {
    var az = new AsciiZ("Héllo", InvalidCharBehavior.Skip);
    Assert.AreEqual("Hllo", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ByteArrayWithReplaceMode_ReplacesInvalidBytes() {
    var bytes = new byte[] { 72, 200, 108, 108, 111 };
    var az = new AsciiZ(bytes, InvalidCharBehavior.Replace);
    Assert.AreEqual("H?llo", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ByteArrayWithSkipMode_SkipsInvalidBytes() {
    var bytes = new byte[] { 72, 200, 108, 108, 111 };
    var az = new AsciiZ(bytes, InvalidCharBehavior.Skip);
    Assert.AreEqual("Hllo", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var az = new AsciiZ("Hello");
    Assert.AreEqual((byte)'H', az[0]);
    Assert.AreEqual((byte)'e', az[1]);
    Assert.AreEqual((byte)'o', az[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var az = new AsciiZ("Hello");
    Assert.AreEqual((byte)'o', az[^1]);
    Assert.AreEqual((byte)'l', az[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var az = new AsciiZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = az[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithTooLargeIndex_Throws() {
    var az = new AsciiZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = az[5]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var az = new AsciiZ("Hello World");
    var sub = az[0..5];
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var az = new AsciiZ("Hello World");
    var sub = az.Substring(6);
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var az = new AsciiZ("Hello World");
    var sub = az.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithNegativeStart_Throws() {
    var az = new AsciiZ("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => az.Substring(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_FromString_Works() {
    AsciiZ az = "Test";
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var az = new AsciiZ("Test");
    string s = az;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var az1 = new AsciiZ("Test");
    var az2 = new AsciiZ("Test");
    Assert.IsTrue(az1.Equals(az2));
    Assert.IsTrue(az1 == az2);
    Assert.IsFalse(az1 != az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var az1 = new AsciiZ("Test1");
    var az2 = new AsciiZ("Test2");
    Assert.IsFalse(az1.Equals(az2));
    Assert.IsFalse(az1 == az2);
    Assert.IsTrue(az1 != az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var az1 = new AsciiZ("A");
    var az2 = new AsciiZ("B");
    Assert.IsTrue(az1 < az2);
    Assert.IsTrue(az1 <= az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var az1 = new AsciiZ("B");
    var az2 = new AsciiZ("A");
    Assert.IsTrue(az1 > az2);
    Assert.IsTrue(az1 >= az2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var az1 = new AsciiZ("Test");
    var az2 = new AsciiZ("Test");
    Assert.AreEqual(az1.GetHashCode(), az2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsPackedSpan() {
    var az = new AsciiZ("Hello");
    var span = az.AsSpan();
    // AsSpan returns packed 7-bit data (no allocation)
    // 5 chars * 7 bits = 35 bits = 5 packed bytes
    Assert.AreEqual(5, span.Length);
    // Verify packed data is accessible (not unpacked characters)
    Assert.IsTrue(span.Length > 0);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var az = new AsciiZ("Hi");
    var arr = az.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var az = new AsciiZ("Hi");
    var arr = az.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void Concatenation_CombinesStrings() {
    var az1 = new AsciiZ("Hello");
    var az2 = new AsciiZ(" World");
    var result = az1 + az2;
    Assert.AreEqual("Hello World", result.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmptyAndZeroLength() {
    var az = AsciiZ.Empty;
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("Regression")]
  public void ZeroTerminatedBehavior_WithEmbeddedNul_CutsCorrectly() {
    var bytes = new byte[] { 65, 66, 0, 67, 68 }; // "AB\0CD"
    var az = new AsciiZ(bytes);
    Assert.AreEqual("AB", az.ToString());
    Assert.AreEqual(2, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var az = new AsciiZ("Test");
    Assert.Greater(((IComparable)az).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var az = new AsciiZ("Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)az).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteSpan_Works() {
    ReadOnlySpan<byte> span = new byte[] { 84, 101, 115, 116 }; // "Test"
    var az = new AsciiZ(span);
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var az = new AsciiZ(span);
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullByteArray_ReturnsEmpty() {
    var az = new AsciiZ((byte[])null);
    Assert.IsTrue(az.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var az = new AsciiZ("Test");
    unsafe {
      fixed (byte* ptr = az) {
        // GetPinnableReference returns packed data, so verify pointer is valid
        // and that we can dereference it without crashing
        Assert.IsTrue(ptr != null);
        _ = *ptr; // Should not throw
      }
    }
    // Verify the indexer still returns correct unpacked values
    Assert.AreEqual((byte)'T', az[0]);
  }

}
