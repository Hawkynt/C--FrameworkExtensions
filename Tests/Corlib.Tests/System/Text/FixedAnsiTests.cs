using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class FixedAnsiTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityAndValue_Works() {
    var fa = new FixedAnsi(32, "Hello");
    Assert.AreEqual(32, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityOnly_CreatesEmpty() {
    var fa = new FixedAnsi(16);
    Assert.AreEqual(16, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueLongerThanCapacity_Truncates() {
    var fa = new FixedAnsi(5, "Hello World");
    Assert.AreEqual(5, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueExactlyCapacity_FitsExactly() {
    var fa = new FixedAnsi(5, "Hello");
    Assert.AreEqual(5, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var fa = new FixedAnsi(10, span);
    Assert.AreEqual("Test", fa.ToString());
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(4, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var fa = new FixedAnsi(10, bytes);
    Assert.AreEqual("Hello", fa.ToString());
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteSpan_Works() {
    ReadOnlySpan<byte> span = new byte[] { 84, 101, 115, 116 }; // "Test"
    var fa = new FixedAnsi(10, span);
    Assert.AreEqual("Test", fa.ToString());
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(4, fa.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullValue_CreatesEmpty() {
    var fa = new FixedAnsi(10, (string)null);
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyValue_CreatesEmpty() {
    var fa = new FixedAnsi(10, string.Empty);
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithZeroCapacity_Works() {
    var fa = new FixedAnsi(0);
    Assert.AreEqual(0, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNegativeCapacity_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedAnsi(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedAnsi(-1, "test"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithExtendedLatin_PreservesValue() {
    var fa = new FixedAnsi(20, "Héllo Wörld");
    Assert.AreEqual("Héllo Wörld", fa.ToString());
    Assert.AreEqual(11, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithWindows1252SpecialChars_PreservesValue() {
    var fa = new FixedAnsi(10, "€™•");
    Assert.AreEqual("€™•", fa.ToString());
    Assert.AreEqual(3, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithHighBytes_Converts() {
    var bytes = new byte[] { 0x80, 0x99, 0x95 }; // €, ™, •
    var fa = new FixedAnsi(10, bytes);
    Assert.AreEqual("€™•", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithUnrepresentableChar_ReplacesWithQuestion() {
    var fa = new FixedAnsi(10, "日本");
    Assert.AreEqual("??", fa.ToString());
    Assert.AreEqual(2, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var fa = new FixedAnsi(10, "Hello");
    Assert.AreEqual((byte)'H', fa[0]);
    Assert.AreEqual((byte)'e', fa[1]);
    Assert.AreEqual((byte)'o', fa[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var fa = new FixedAnsi(10, "Hello");
    Assert.AreEqual((byte)'o', fa[^1]);
    Assert.AreEqual((byte)'l', fa[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var fa = new FixedAnsi(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_BeyondLength_Throws() {
    var fa = new FixedAnsi(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[5]);
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var fa = new FixedAnsi(20, "Hello World");
    var sub = fa[0..5];
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var fa = new FixedAnsi(20, "Hello World");
    var sub = fa.Substring(6);
    Assert.AreEqual("World", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var fa = new FixedAnsi(20, "Hello World");
    var sub = fa.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithInvalidStart_Throws() {
    var fa = new FixedAnsi(10, "Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => fa.Substring(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => fa.Substring(6));
  }

  [Test]
  [Category("HappyPath")]
  public void PadRight_FillsToCapacity() {
    var fa = new FixedAnsi(10, "Hi");
    var padded = fa.PadRight(0);
    Assert.AreEqual(10, padded.Length);
    Assert.AreEqual((byte)'H', padded[0]);
    Assert.AreEqual((byte)'i', padded[1]);
    Assert.AreEqual((byte)0, padded[2]);
    Assert.AreEqual((byte)0, padded[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void PadRight_WithCustomByte_Works() {
    var fa = new FixedAnsi(8, "Test");
    var padded = fa.PadRight((byte)'X');
    Assert.AreEqual("TestXXXX", padded.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void PadLeft_FillsToCapacity() {
    var fa = new FixedAnsi(10, "Hi");
    var padded = fa.PadLeft((byte)' ');
    Assert.AreEqual(10, padded.Length);
    Assert.AreEqual((byte)' ', padded[0]);
    Assert.AreEqual((byte)' ', padded[7]);
    Assert.AreEqual((byte)'H', padded[8]);
    Assert.AreEqual((byte)'i', padded[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_RemovesTrailingNulsAndSpaces() {
    var fa = new FixedAnsi(10, "Hi");
    var padded = fa.PadRight(0);
    var trimmed = padded.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_RemovesSpaces() {
    var fa = new FixedAnsi(10, "Hi   ");
    var trimmed = fa.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void TrimEnd_OnAllWhitespace_ReturnsEmpty() {
    var fa = new FixedAnsi(5, "   ");
    var trimmed = fa.TrimEnd();
    Assert.AreEqual(0, trimmed.Length);
    Assert.IsTrue(trimmed.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var fa = new FixedAnsi(10, "Test");
    string s = fa;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var fa1 = new FixedAnsi(10, "Test");
    var fa2 = new FixedAnsi(20, "Test");
    Assert.IsTrue(fa1.Equals(fa2));
    Assert.IsTrue(fa1 == fa2);
    Assert.IsFalse(fa1 != fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var fa1 = new FixedAnsi(10, "Test1");
    var fa2 = new FixedAnsi(10, "Test2");
    Assert.IsFalse(fa1.Equals(fa2));
    Assert.IsFalse(fa1 == fa2);
    Assert.IsTrue(fa1 != fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var fa1 = new FixedAnsi(10, "A");
    var fa2 = new FixedAnsi(10, "B");
    Assert.IsTrue(fa1 < fa2);
    Assert.IsTrue(fa1 <= fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var fa1 = new FixedAnsi(10, "B");
    var fa2 = new FixedAnsi(10, "A");
    Assert.IsTrue(fa1 > fa2);
    Assert.IsTrue(fa1 >= fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var fa1 = new FixedAnsi(10, "Test");
    var fa2 = new FixedAnsi(20, "Test");
    Assert.AreEqual(fa1.GetHashCode(), fa2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsContentOnly() {
    var fa = new FixedAnsi(20, "Hello");
    var span = fa.AsSpan();
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual((byte)'H', span[0]);
  }

  [Test]
  [Category("HappyPath")]
  public void AsFullSpan_ReturnsFullCapacity() {
    var fa = new FixedAnsi(10, "Hi");
    var span = fa.AsFullSpan();
    Assert.AreEqual(10, span.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var fa = new FixedAnsi(10, "Hi");
    var arr = fa.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var fa = new FixedAnsi(10, "Hi");
    var arr = fa.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var fa = new FixedAnsi(10, "Test");
    Assert.Greater(((IComparable)fa).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var fa = new FixedAnsi(10, "Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)fa).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var fa = new FixedAnsi(10, "Test");
    unsafe {
      fixed (byte* ptr = fa)
        Assert.AreEqual((byte)'T', *ptr);
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void DefaultStruct_IsEmptyWithZeroCapacity() {
    FixedAnsi fa = default;
    Assert.AreEqual(0, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
    Assert.AreEqual(string.Empty, fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void RoundTrip_WithLatin1Chars_PreservesValue() {
    var original = "Café résumé";
    var fa = new FixedAnsi(20, original);
    Assert.AreEqual(original, fa.ToString());
  }

}
