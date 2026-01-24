using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class AnsiZTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNormalString_PreservesValue() {
    var az = new AnsiZ("Hello");
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithEmbeddedNull_TruncatesAtNull() {
    var az = new AnsiZ("Hello\0World");
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var az = new AnsiZ(bytes);
    Assert.AreEqual("Hello", az.ToString());
    Assert.AreEqual(5, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArrayWithNull_Truncates() {
    var bytes = new byte[] { 72, 105, 0, 33 }; // "Hi\0!"
    var az = new AnsiZ(bytes);
    Assert.AreEqual("Hi", az.ToString());
    Assert.AreEqual(2, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullString_ReturnsEmpty() {
    var az = new AnsiZ((string)null);
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyString_ReturnsEmpty() {
    var az = new AnsiZ(string.Empty);
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithLeadingNull_ReturnsEmpty() {
    var az = new AnsiZ("\0Hello");
    Assert.IsTrue(az.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithExtendedLatin_PreservesValue() {
    var az = new AnsiZ("Héllo Wörld");
    Assert.AreEqual("Héllo Wörld", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithWindows1252SpecialChars_PreservesValue() {
    // Euro sign (0x80), trademark (0x99), bullets (0x95)
    var az = new AnsiZ("€™•");
    Assert.AreEqual("€™•", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithHighBytes_Converts() {
    // Test bytes in the 0x80-0xFF range
    var bytes = new byte[] { 0x80, 0x99, 0x95 }; // €, ™, •
    var az = new AnsiZ(bytes);
    Assert.AreEqual("€™•", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithUnrepresentableChar_ReplacesWithQuestion() {
    // Japanese characters cannot be represented in Windows-1252
    var az = new AnsiZ("日本");
    Assert.AreEqual("??", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var az = new AnsiZ("Hello");
    Assert.AreEqual((byte)'H', az[0]);
    Assert.AreEqual((byte)'e', az[1]);
    Assert.AreEqual((byte)'o', az[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var az = new AnsiZ("Hello");
    Assert.AreEqual((byte)'o', az[^1]);
    Assert.AreEqual((byte)'l', az[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var az = new AnsiZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = az[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithTooLargeIndex_Throws() {
    var az = new AnsiZ("Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = az[5]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var az = new AnsiZ("Hello World");
    var sub = az[0..5];
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var az = new AnsiZ("Hello World");
    var sub = az.Substring(6);
    Assert.AreEqual("World", sub.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var az = new AnsiZ("Hello World");
    var sub = az.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithNegativeStart_Throws() {
    var az = new AnsiZ("Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => az.Substring(-1));
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_FromString_Works() {
    AnsiZ az = "Test";
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var az = new AnsiZ("Test");
    string s = az;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var az1 = new AnsiZ("Test");
    var az2 = new AnsiZ("Test");
    Assert.IsTrue(az1.Equals(az2));
    Assert.IsTrue(az1 == az2);
    Assert.IsFalse(az1 != az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var az1 = new AnsiZ("Test1");
    var az2 = new AnsiZ("Test2");
    Assert.IsFalse(az1.Equals(az2));
    Assert.IsFalse(az1 == az2);
    Assert.IsTrue(az1 != az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var az1 = new AnsiZ("A");
    var az2 = new AnsiZ("B");
    Assert.IsTrue(az1 < az2);
    Assert.IsTrue(az1 <= az2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var az1 = new AnsiZ("B");
    var az2 = new AnsiZ("A");
    Assert.IsTrue(az1 > az2);
    Assert.IsTrue(az1 >= az2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var az1 = new AnsiZ("Test");
    var az2 = new AnsiZ("Test");
    Assert.AreEqual(az1.GetHashCode(), az2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsCorrectSpan() {
    var az = new AnsiZ("Hello");
    var span = az.AsSpan();
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual((byte)'H', span[0]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var az = new AnsiZ("Hi");
    var arr = az.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var az = new AnsiZ("Hi");
    var arr = az.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void Concatenation_CombinesStrings() {
    var az1 = new AnsiZ("Hello");
    var az2 = new AnsiZ(" World");
    var result = az1 + az2;
    Assert.AreEqual("Hello World", result.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Empty_IsEmptyAndZeroLength() {
    var az = AnsiZ.Empty;
    Assert.IsTrue(az.IsEmpty);
    Assert.AreEqual(0, az.Length);
  }

  [Test]
  [Category("Regression")]
  public void ZeroTerminatedBehavior_WithEmbeddedNul_CutsCorrectly() {
    var bytes = new byte[] { 65, 66, 0, 67, 68 }; // "AB\0CD"
    var az = new AnsiZ(bytes);
    Assert.AreEqual("AB", az.ToString());
    Assert.AreEqual(2, az.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var az = new AnsiZ("Test");
    Assert.Greater(((IComparable)az).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var az = new AnsiZ("Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)az).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteSpan_Works() {
    ReadOnlySpan<byte> span = new byte[] { 84, 101, 115, 116 }; // "Test"
    var az = new AnsiZ(span);
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var az = new AnsiZ(span);
    Assert.AreEqual("Test", az.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullByteArray_ReturnsEmpty() {
    var az = new AnsiZ((byte[])null);
    Assert.IsTrue(az.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var az = new AnsiZ("Test");
    unsafe {
      fixed (byte* ptr = az)
        Assert.AreEqual((byte)'T', *ptr);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void RoundTrip_WithLatin1Chars_PreservesValue() {
    var original = "Café résumé naïve";
    var az = new AnsiZ(original);
    Assert.AreEqual(original, az.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Windows1252SpecificCharacters_AreCorrectlyMapped() {
    // Test characters that differ between ISO-8859-1 and Windows-1252
    // 0x80 = € (Euro), 0x93 = " (left double quotation mark)
    var bytes = new byte[] { 0x80, 0x93 };
    var az = new AnsiZ(bytes);
    Assert.AreEqual("\u20AC\u201C", az.ToString());
  }

}
