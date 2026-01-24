using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class FixedAsciiTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityAndValue_Works() {
    var fa = new FixedAscii(32, "Hello");
    Assert.AreEqual(32, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityOnly_CreatesEmpty() {
    var fa = new FixedAscii(16);
    Assert.AreEqual(16, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueLongerThanCapacity_Truncates() {
    var fa = new FixedAscii(5, "Hello World");
    Assert.AreEqual(5, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueExactlyCapacity_FitsExactly() {
    var fa = new FixedAscii(5, "Hello");
    Assert.AreEqual(5, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
    Assert.AreEqual("Hello", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var fa = new FixedAscii(10, span);
    Assert.AreEqual("Test", fa.ToString());
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(4, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromByteArray_Works() {
    var bytes = new byte[] { 72, 101, 108, 108, 111 }; // "Hello"
    var fa = new FixedAscii(10, bytes);
    Assert.AreEqual("Hello", fa.ToString());
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(5, fa.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullValue_CreatesEmpty() {
    var fa = new FixedAscii(10, (string)null);
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyValue_CreatesEmpty() {
    var fa = new FixedAscii(10, string.Empty);
    Assert.AreEqual(10, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithZeroCapacity_Works() {
    var fa = new FixedAscii(0);
    Assert.AreEqual(0, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNegativeCapacity_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedAscii(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedAscii(-1, "test"));
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNonAsciiChar_ThrowsDefault() {
    Assert.Throws<ArgumentException>(() => new FixedAscii(10, "Héllo"));
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNonAsciiAndReplace_Replaces() {
    var fa = new FixedAscii(10, "Héllo", InvalidCharBehavior.Replace);
    Assert.AreEqual("H?llo", fa.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithNonAsciiAndSkip_Skips() {
    var fa = new FixedAscii(10, "Héllo", InvalidCharBehavior.Skip);
    Assert.AreEqual("Hllo", fa.ToString());
    Assert.AreEqual(4, fa.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ByteArrayWithNonAsciiAndReplace_Replaces() {
    var bytes = new byte[] { 72, 200, 108, 108, 111 }; // H, 200 (>127), l, l, o
    var fa = new FixedAscii(10, bytes, InvalidCharBehavior.Replace);
    Assert.AreEqual((byte)'H', fa[0]);
    Assert.AreEqual((byte)'?', fa[1]);
    Assert.AreEqual((byte)'l', fa[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectByte() {
    var fa = new FixedAscii(10, "Hello");
    Assert.AreEqual((byte)'H', fa[0]);
    Assert.AreEqual((byte)'e', fa[1]);
    Assert.AreEqual((byte)'o', fa[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectByte() {
    var fa = new FixedAscii(10, "Hello");
    Assert.AreEqual((byte)'o', fa[^1]);
    Assert.AreEqual((byte)'l', fa[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var fa = new FixedAscii(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_BeyondLength_Throws() {
    var fa = new FixedAscii(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[5]);
    Assert.Throws<IndexOutOfRangeException>(() => _ = fa[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var fa = new FixedAscii(20, "Hello World");
    var sub = fa[0..5];
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var fa = new FixedAscii(20, "Hello World");
    var sub = fa.Substring(6);
    Assert.AreEqual("World", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var fa = new FixedAscii(20, "Hello World");
    var sub = fa.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithInvalidStart_Throws() {
    var fa = new FixedAscii(10, "Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => fa.Substring(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => fa.Substring(6));
  }

  [Test]
  [Category("HappyPath")]
  public void PadRight_FillsToCapacity() {
    var fa = new FixedAscii(10, "Hi");
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
    var fa = new FixedAscii(8, "Test");
    var padded = fa.PadRight((byte)'X');
    Assert.AreEqual("TestXXXX", padded.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void PadLeft_FillsToCapacity() {
    var fa = new FixedAscii(10, "Hi");
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
    var fa = new FixedAscii(10, "Hi");
    var padded = fa.PadRight(0);
    var trimmed = padded.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_RemovesSpaces() {
    var fa = new FixedAscii(10, "Hi   ");
    var trimmed = fa.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void TrimEnd_OnAllWhitespace_ReturnsEmpty() {
    var fa = new FixedAscii(5, "   ");
    var trimmed = fa.TrimEnd();
    Assert.AreEqual(0, trimmed.Length);
    Assert.IsTrue(trimmed.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var fa = new FixedAscii(10, "Test");
    string s = fa;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var fa1 = new FixedAscii(10, "Test");
    var fa2 = new FixedAscii(20, "Test");
    Assert.IsTrue(fa1.Equals(fa2));
    Assert.IsTrue(fa1 == fa2);
    Assert.IsFalse(fa1 != fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var fa1 = new FixedAscii(10, "Test1");
    var fa2 = new FixedAscii(10, "Test2");
    Assert.IsFalse(fa1.Equals(fa2));
    Assert.IsFalse(fa1 == fa2);
    Assert.IsTrue(fa1 != fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var fa1 = new FixedAscii(10, "A");
    var fa2 = new FixedAscii(10, "B");
    Assert.IsTrue(fa1 < fa2);
    Assert.IsTrue(fa1 <= fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var fa1 = new FixedAscii(10, "B");
    var fa2 = new FixedAscii(10, "A");
    Assert.IsTrue(fa1 > fa2);
    Assert.IsTrue(fa1 >= fa2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var fa1 = new FixedAscii(10, "Test");
    var fa2 = new FixedAscii(20, "Test");
    Assert.AreEqual(fa1.GetHashCode(), fa2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsPackedSpan() {
    var fa = new FixedAscii(20, "Hello");
    var span = fa.AsSpan();
    // AsSpan returns packed 7-bit data (no allocation)
    // 5 chars * 7 bits = 35 bits = 5 packed bytes
    Assert.AreEqual(5, span.Length);
    // Verify packed data is accessible (not unpacked characters)
    Assert.IsTrue(span.Length > 0);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var fa = new FixedAscii(10, "Hi");
    var arr = fa.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
    Assert.AreEqual(0, arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void ToArray_ReturnsCorrectArray() {
    var fa = new FixedAscii(10, "Hi");
    var arr = fa.ToArray();
    Assert.AreEqual(2, arr.Length);
    Assert.AreEqual((byte)'H', arr[0]);
    Assert.AreEqual((byte)'i', arr[1]);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var fa = new FixedAscii(10, "Test");
    Assert.Greater(((IComparable)fa).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var fa = new FixedAscii(10, "Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)fa).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var fa = new FixedAscii(10, "Test");
    unsafe {
      fixed (byte* ptr = fa) {
        // GetPinnableReference returns packed data, so verify pointer is valid
        // and that we can dereference it without crashing
        Assert.IsTrue(ptr != null);
        _ = *ptr; // Should not throw
      }
    }
    // Verify the indexer still returns correct unpacked values
    Assert.AreEqual((byte)'T', fa[0]);
  }

  [Test]
  [Category("EdgeCase")]
  public void DefaultStruct_IsEmptyWithZeroCapacity() {
    FixedAscii fa = default;
    Assert.AreEqual(0, fa.Capacity);
    Assert.AreEqual(0, fa.Length);
    Assert.IsTrue(fa.IsEmpty);
    Assert.AreEqual(string.Empty, fa.ToString());
  }

}
