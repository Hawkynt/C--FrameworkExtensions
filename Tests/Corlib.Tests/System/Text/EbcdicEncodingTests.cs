using NUnit.Framework;

namespace System.Text;

[TestFixture]
public class EbcdicEncodingTests {

  [Test]
  [Category("HappyPath")]
  public void RoundTrip_HelloWorld_PreservesValue() {
    const string original = "HELLO WORLD 123!";
    var encoding = EbcdicEncoding.CP037;

    var bytes = encoding.GetBytes(original);
    var decoded = encoding.GetString(bytes);

    Assert.AreEqual(original, decoded);
    Assert.AreEqual(original.Length, bytes.Length);
  }

  [Test]
  [Category("HappyPath")]
  public void GetBytes_KnownAnchors_ProduceExpectedEbcdicCodes() {
    var encoding = EbcdicEncoding.CP037;

    Assert.AreEqual(0xC1, encoding.GetBytes("A")[0], "'A' must map to EBCDIC 0xC1");
    Assert.AreEqual(0x81, encoding.GetBytes("a")[0], "'a' must map to EBCDIC 0x81");
    Assert.AreEqual(0xF0, encoding.GetBytes("0")[0], "'0' must map to EBCDIC 0xF0");
    Assert.AreEqual(0x40, encoding.GetBytes(" ")[0], "space must map to EBCDIC 0x40");
  }

  [Test]
  [Category("HappyPath")]
  public void ToEbcdic_KnownAnchors_ProduceExpectedCodes() {
    Assert.AreEqual(0xC1, EbcdicEncoding.ToEbcdic('A'));
    Assert.AreEqual(0x81, EbcdicEncoding.ToEbcdic('a'));
    Assert.AreEqual(0xF0, EbcdicEncoding.ToEbcdic('0'));
    Assert.AreEqual(0x40, EbcdicEncoding.ToEbcdic(' '));
    Assert.AreEqual(0x6F, EbcdicEncoding.ToEbcdic('?'));
  }

  [Test]
  [Category("HappyPath")]
  public void FromEbcdic_KnownAnchors_ProduceExpectedChars() {
    Assert.AreEqual('A', EbcdicEncoding.FromEbcdic(0xC1));
    Assert.AreEqual('a', EbcdicEncoding.FromEbcdic(0x81));
    Assert.AreEqual('0', EbcdicEncoding.FromEbcdic(0xF0));
    Assert.AreEqual(' ', EbcdicEncoding.FromEbcdic(0x40));
    Assert.AreEqual('?', EbcdicEncoding.FromEbcdic(0x6F));
  }

  [Test]
  [Category("HappyPath")]
  public void GetString_OfGetBytes_RoundTripsLettersDigitsPunctuation() {
    const string sample = "The Quick Brown Fox (jumps) over 99 lazy dogs! - a/b+c=d.";
    var encoding = EbcdicEncoding.CP037;

    var result = encoding.GetString(encoding.GetBytes(sample));

    Assert.AreEqual(sample, result);
  }

  [Test]
  [Category("HappyPath")]
  public void GetBytes_PunctuationAnchors_AreCorrect() {
    var encoding = EbcdicEncoding.CP037;

    Assert.AreEqual(0x4B, encoding.GetBytes(".")[0]);
    Assert.AreEqual(0x6B, encoding.GetBytes(",")[0]);
    Assert.AreEqual(0x7D, encoding.GetBytes("'")[0]);
    Assert.AreEqual(0x7E, encoding.GetBytes("=")[0]);
    Assert.AreEqual(0x50, encoding.GetBytes("&")[0]);
    Assert.AreEqual(0x5A, encoding.GetBytes("!")[0]);
    Assert.AreEqual(0x5B, encoding.GetBytes("$")[0]);
    Assert.AreEqual(0x5C, encoding.GetBytes("*")[0]);
    Assert.AreEqual(0x5D, encoding.GetBytes(")")[0]);
    Assert.AreEqual(0x4D, encoding.GetBytes("(")[0]);
    Assert.AreEqual(0x60, encoding.GetBytes("-")[0]);
    Assert.AreEqual(0x61, encoding.GetBytes("/")[0]);
    Assert.AreEqual(0x4E, encoding.GetBytes("+")[0]);
  }

  [Test]
  [Category("EquivalenceClass")]
  public void GetBytes_UnmappableCharacter_FallsBackToQuestionMark() {
    var encoding = EbcdicEncoding.CP037;

    // A CJK character is not representable in CP037; it must fall back to '?' (0x6F).
    var bytes = encoding.GetBytes("中");

    Assert.AreEqual(1, bytes.Length);
    Assert.AreEqual(0x6F, bytes[0]);
  }

  [Test]
  [Category("EquivalenceClass")]
  public void GetMaxByteCount_EqualsCharCount() {
    var encoding = EbcdicEncoding.CP037;

    Assert.AreEqual(0, encoding.GetMaxByteCount(0));
    Assert.AreEqual(1, encoding.GetMaxByteCount(1));
    Assert.AreEqual(123, encoding.GetMaxByteCount(123));
  }

  [Test]
  [Category("EquivalenceClass")]
  public void GetMaxCharCount_EqualsByteCount() {
    var encoding = EbcdicEncoding.CP037;

    Assert.AreEqual(0, encoding.GetMaxCharCount(0));
    Assert.AreEqual(1, encoding.GetMaxCharCount(1));
    Assert.AreEqual(123, encoding.GetMaxCharCount(123));
  }

  [Test]
  [Category("Boundary")]
  public void GetByteCount_And_GetCharCount_MatchInputLength() {
    var encoding = EbcdicEncoding.CP037;
    var chars = "ABC".ToCharArray();
    var bytes = encoding.GetBytes("ABC");

    Assert.AreEqual(3, encoding.GetByteCount(chars, 0, chars.Length));
    Assert.AreEqual(3, encoding.GetCharCount(bytes, 0, bytes.Length));
  }

  [Test]
  [Category("Boundary")]
  public void GetBytes_Empty_ReturnsEmpty() {
    var encoding = EbcdicEncoding.CP037;

    Assert.AreEqual(0, encoding.GetBytes(string.Empty).Length);
    Assert.AreEqual(string.Empty, encoding.GetString([]));
  }

  [Test]
  [Category("ExceptionalCase")]
  public void GetMaxByteCount_NegativeCount_Throws()
    => Assert.Throws<ArgumentOutOfRangeException>(() => EbcdicEncoding.CP037.GetMaxByteCount(-1));

  [Test]
  [Category("ExceptionalCase")]
  public void GetMaxCharCount_NegativeCount_Throws()
    => Assert.Throws<ArgumentOutOfRangeException>(() => EbcdicEncoding.CP037.GetMaxCharCount(-1));

}
