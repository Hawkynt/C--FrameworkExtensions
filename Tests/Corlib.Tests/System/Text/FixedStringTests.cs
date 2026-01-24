using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class FixedStringTests {

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityAndValue_Works() {
    var fs = new FixedString(32, "Hello");
    Assert.AreEqual(32, fs.Capacity);
    Assert.AreEqual(5, fs.Length);
    Assert.AreEqual("Hello", fs.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_WithCapacityOnly_CreatesEmpty() {
    var fs = new FixedString(16);
    Assert.AreEqual(16, fs.Capacity);
    Assert.AreEqual(0, fs.Length);
    Assert.IsTrue(fs.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueLongerThanCapacity_Truncates() {
    var fs = new FixedString(5, "Hello World");
    Assert.AreEqual(5, fs.Capacity);
    Assert.AreEqual(5, fs.Length);
    Assert.AreEqual("Hello", fs.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_ValueExactlyCapacity_FitsExactly() {
    var fs = new FixedString(5, "Hello");
    Assert.AreEqual(5, fs.Capacity);
    Assert.AreEqual(5, fs.Length);
    Assert.AreEqual("Hello", fs.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void Constructor_FromCharSpan_Works() {
    ReadOnlySpan<char> span = "Test".AsSpan();
    var fs = new FixedString(10, span);
    Assert.AreEqual("Test", fs.ToString());
    Assert.AreEqual(10, fs.Capacity);
    Assert.AreEqual(4, fs.Length);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithNullValue_CreatesEmpty() {
    var fs = new FixedString(10, (string)null);
    Assert.AreEqual(10, fs.Capacity);
    Assert.AreEqual(0, fs.Length);
    Assert.IsTrue(fs.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithEmptyValue_CreatesEmpty() {
    var fs = new FixedString(10, string.Empty);
    Assert.AreEqual(10, fs.Capacity);
    Assert.AreEqual(0, fs.Length);
    Assert.IsTrue(fs.IsEmpty);
  }

  [Test]
  [Category("EdgeCase")]
  public void Constructor_WithZeroCapacity_Works() {
    var fs = new FixedString(0);
    Assert.AreEqual(0, fs.Capacity);
    Assert.AreEqual(0, fs.Length);
    Assert.IsTrue(fs.IsEmpty);
  }

  [Test]
  [Category("Exception")]
  public void Constructor_WithNegativeCapacity_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedString(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => new FixedString(-1, "test"));
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithValidIndex_ReturnsCorrectChar() {
    var fs = new FixedString(10, "Hello");
    Assert.AreEqual('H', fs[0]);
    Assert.AreEqual('e', fs[1]);
    Assert.AreEqual('o', fs[4]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithFromEndIndex_ReturnsCorrectChar() {
    var fs = new FixedString(10, "Hello");
    Assert.AreEqual('o', fs[^1]);
    Assert.AreEqual('l', fs[^2]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_WithNegativeIndex_Throws() {
    var fs = new FixedString(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fs[-1]);
  }

  [Test]
  [Category("Exception")]
  public void Indexer_BeyondLength_Throws() {
    var fs = new FixedString(10, "Hello");
    Assert.Throws<IndexOutOfRangeException>(() => _ = fs[5]);
    Assert.Throws<IndexOutOfRangeException>(() => _ = fs[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void Indexer_WithRange_ReturnsSubstring() {
    var fs = new FixedString(20, "Hello World");
    var sub = fs[0..5];
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartIndex_ReturnsRemainder() {
    var fs = new FixedString(20, "Hello World");
    var sub = fs.Substring(6);
    Assert.AreEqual("World", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("HappyPath")]
  public void Substring_WithStartAndLength_ReturnsCorrectPortion() {
    var fs = new FixedString(20, "Hello World");
    var sub = fs.Substring(0, 5);
    Assert.AreEqual("Hello", sub.ToString());
    Assert.AreEqual(20, sub.Capacity);
  }

  [Test]
  [Category("Exception")]
  public void Substring_WithInvalidStart_Throws() {
    var fs = new FixedString(10, "Hello");
    Assert.Throws<ArgumentOutOfRangeException>(() => fs.Substring(-1));
    Assert.Throws<ArgumentOutOfRangeException>(() => fs.Substring(6));
  }

  [Test]
  [Category("HappyPath")]
  public void PadRight_FillsToCapacity() {
    var fs = new FixedString(10, "Hi");
    var padded = fs.PadRight('\0');
    Assert.AreEqual(10, padded.Length);
    Assert.AreEqual('H', padded[0]);
    Assert.AreEqual('i', padded[1]);
    Assert.AreEqual('\0', padded[2]);
    Assert.AreEqual('\0', padded[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void PadRight_WithCustomChar_Works() {
    var fs = new FixedString(8, "Test");
    var padded = fs.PadRight('X');
    Assert.AreEqual("TestXXXX", padded.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void PadLeft_FillsToCapacity() {
    var fs = new FixedString(10, "Hi");
    var padded = fs.PadLeft(' ');
    Assert.AreEqual(10, padded.Length);
    Assert.AreEqual(' ', padded[0]);
    Assert.AreEqual(' ', padded[7]);
    Assert.AreEqual('H', padded[8]);
    Assert.AreEqual('i', padded[9]);
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_RemovesTrailingNulsAndWhitespace() {
    var fs = new FixedString(10, "Hi");
    var padded = fs.PadRight('\0');
    var trimmed = padded.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void TrimEnd_RemovesWhitespace() {
    var fs = new FixedString(10, "Hi   ");
    var trimmed = fs.TrimEnd();
    Assert.AreEqual(2, trimmed.Length);
    Assert.AreEqual("Hi", trimmed.ToString());
  }

  [Test]
  [Category("EdgeCase")]
  public void TrimEnd_OnAllWhitespace_ReturnsEmpty() {
    var fs = new FixedString(5, "   ");
    var trimmed = fs.TrimEnd();
    Assert.AreEqual(0, trimmed.Length);
    Assert.IsTrue(trimmed.IsEmpty);
  }

  [Test]
  [Category("HappyPath")]
  public void ImplicitConversion_ToString_Works() {
    var fs = new FixedString(10, "Test");
    string s = fs;
    Assert.AreEqual("Test", s);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_SameContent_ReturnsTrue() {
    var fs1 = new FixedString(10, "Test");
    var fs2 = new FixedString(20, "Test");
    Assert.IsTrue(fs1.Equals(fs2));
    Assert.IsTrue(fs1 == fs2);
    Assert.IsFalse(fs1 != fs2);
  }

  [Test]
  [Category("HappyPath")]
  public void Equality_DifferentContent_ReturnsFalse() {
    var fs1 = new FixedString(10, "Test1");
    var fs2 = new FixedString(10, "Test2");
    Assert.IsFalse(fs1.Equals(fs2));
    Assert.IsFalse(fs1 == fs2);
    Assert.IsTrue(fs1 != fs2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_LessThan_Works() {
    var fs1 = new FixedString(10, "A");
    var fs2 = new FixedString(10, "B");
    Assert.IsTrue(fs1 < fs2);
    Assert.IsTrue(fs1 <= fs2);
  }

  [Test]
  [Category("HappyPath")]
  public void Comparison_GreaterThan_Works() {
    var fs1 = new FixedString(10, "B");
    var fs2 = new FixedString(10, "A");
    Assert.IsTrue(fs1 > fs2);
    Assert.IsTrue(fs1 >= fs2);
  }

  [Test]
  [Category("HappyPath")]
  public void GetHashCode_SameContent_ReturnsSameHash() {
    var fs1 = new FixedString(10, "Test");
    var fs2 = new FixedString(20, "Test");
    Assert.AreEqual(fs1.GetHashCode(), fs2.GetHashCode());
  }

  [Test]
  [Category("HappyPath")]
  public void AsSpan_ReturnsContentOnly() {
    var fs = new FixedString(20, "Hello");
    var span = fs.AsSpan();
    Assert.AreEqual(5, span.Length);
    Assert.AreEqual("Hello", span.ToString());
  }

  [Test]
  [Category("HappyPath")]
  public void AsFullSpan_ReturnsFullCapacity() {
    var fs = new FixedString(10, "Hi");
    var span = fs.AsFullSpan();
    Assert.AreEqual(10, span.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void ToNullTerminatedArray_ReturnsCorrectArray() {
    var fs = new FixedString(10, "Hi");
    var arr = fs.ToNullTerminatedArray();
    Assert.AreEqual(3, arr.Length);
    Assert.AreEqual('H', arr[0]);
    Assert.AreEqual('i', arr[1]);
    Assert.AreEqual('\0', arr[2]);
  }

  [Test]
  [Category("HappyPath")]
  public void CompareTo_WithNull_ReturnsPositive() {
    var fs = new FixedString(10, "Test");
    Assert.Greater(((IComparable)fs).CompareTo(null), 0);
  }

  [Test]
  [Category("Exception")]
  public void CompareTo_WithInvalidType_Throws() {
    var fs = new FixedString(10, "Test");
    Assert.Throws<ArgumentException>(() => ((IComparable)fs).CompareTo("string"));
  }

  [Test]
  [Category("HappyPath")]
  public void GetPinnableReference_AllowsFixed() {
    var fs = new FixedString(10, "Test");
    unsafe {
      fixed (char* ptr = fs)
        Assert.AreEqual('T', *ptr);
    }
  }

  [Test]
  [Category("EdgeCase")]
  public void DefaultStruct_IsEmptyWithZeroCapacity() {
    FixedString fs = default;
    Assert.AreEqual(0, fs.Capacity);
    Assert.AreEqual(0, fs.Length);
    Assert.IsTrue(fs.IsEmpty);
    Assert.AreEqual(string.Empty, fs.ToString());
  }

}
