using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace System.Text;

/// <summary>
/// Verifies <see cref="PetsciiEncoding"/> against golden vectors generated from the authoritative cbmcodecs2
/// reference (TestData/generate_retro_encodings.py): exhaustive byte->Unicode for all four variants.
/// </summary>
[TestFixture]
public class PetsciiGoldenTests {

  private static void CheckDecode(string suffix, PetsciiEncoding enc) {
    var asm = typeof(PetsciiGoldenTests).Assembly;
    string? name = null;
    foreach (var n in asm.GetManifestResourceNames())
      if (n.EndsWith(suffix, StringComparison.Ordinal)) {
        name = n;
        break;
      }

    Assert.That(name, Is.Not.Null, $"resource '{suffix}' not found");
    using var reader = new StreamReader(asm.GetManifestResourceStream(name!)!);
    string? line;
    var seen = 0;
    while ((line = reader.ReadLine()) != null) {
      if (line.Length == 0 || line[0] == '#')
        continue;
      var tab = line.IndexOf('\t');
      var b = byte.Parse(line.Substring(0, tab), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      var expected = (char)int.Parse(line.Substring(tab + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      Assert.That(enc.ToChar(b), Is.EqualTo(expected), $"{suffix} byte 0x{b:X2}");
      ++seen;
    }

    Assert.That(seen, Is.EqualTo(256), $"{suffix} should have 256 rows");
  }

  [Test]
  public void C64Uppercase_Decode_MatchesCbmcodecs2() => CheckDecode("petscii_c64Uppercase.tsv", PetsciiEncoding.C64Uppercase);

  [Test]
  public void C64Lowercase_Decode_MatchesCbmcodecs2() => CheckDecode("petscii_c64Lowercase.tsv", PetsciiEncoding.C64Lowercase);

  [Test]
  public void Vic20Uppercase_Decode_MatchesCbmcodecs2() => CheckDecode("petscii_vic20Uppercase.tsv", PetsciiEncoding.Vic20Uppercase);

  [Test]
  public void Vic20Lowercase_Decode_MatchesCbmcodecs2() => CheckDecode("petscii_vic20Lowercase.tsv", PetsciiEncoding.Vic20Lowercase);

  [Test]
  public void Ascii_Range_RoundTrips() {
    // Letters/digits/space share ASCII positions; uppercase text round-trips in uppercase mode.
    const string text = "HELLO WORLD 123!";
    var enc = PetsciiEncoding.C64Uppercase;
    Assert.AreEqual(text, enc.GetString(enc.GetBytes(text)));
    // 'A' is 0x41, '0' is 0x30, space 0x20
    Assert.AreEqual(0x41, enc.ToByte('A'));
    Assert.AreEqual(0x30, enc.ToByte('0'));
  }

  [Test]
  public void Lowercase_Mode_HasLowercaseLetters() {
    var enc = PetsciiEncoding.C64Lowercase;
    Assert.AreEqual("hello", enc.GetString(enc.GetBytes("hello")));
    Assert.AreNotEqual('?', enc.ToChar(enc.ToByte('a'))); // 'a' is representable in lowercase mode
  }

  [Test]
  public void GraphicsGlyph_UsesPrivateUseArea() {
    // C64 byte 0xD0 decodes to a PUA block-graphics glyph (per the cbmcodecs2/Walleij mapping).
    var ch = PetsciiEncoding.C64Uppercase.ToChar(0xD0);
    Assert.That(ch, Is.EqualTo((char)0xF12C));  // 0xD0 -> U+F12C (PUA block graphic)
  }
}
