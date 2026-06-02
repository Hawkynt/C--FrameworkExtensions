using System.Globalization;
using System.IO;
using NUnit.Framework;

namespace System;

/// <summary>
/// Spec-compatibility of <see cref="Posit8"/>/<see cref="Posit16"/>/<see cref="Posit32"/> decode against the
/// from-spec posit oracle in TestData/generate_golden.py (validated against published anchors). posit8 is
/// exhaustive (256 codes); posit16/32 are sampled. Vectors embedded as resources.
/// </summary>
[TestFixture]
public class PositGoldenTests {

  private static void Check(string suffix, Func<uint, double> decode) {
    var asm = typeof(PositGoldenTests).Assembly;
    string? name = null;
    foreach (var n in asm.GetManifestResourceNames())
      if (n.EndsWith(suffix, StringComparison.Ordinal)) {
        name = n;
        break;
      }

    Assert.That(name, Is.Not.Null, $"resource '{suffix}' not found");
    using var reader = new StreamReader(asm.GetManifestResourceStream(name!)!);
    string? line;
    while ((line = reader.ReadLine()) != null) {
      if (line.Length == 0 || line[0] == '#')
        continue;
      var tab = line.IndexOf('\t');
      var raw = uint.Parse(line.Substring(0, tab), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      var token = line.Substring(tab + 1);
      var actual = decode(raw);
      if (token == "NaN")
        Assert.That(double.IsNaN(actual), Is.True, $"{suffix} 0x{raw:X} expected NaR/NaN");
      else
        Assert.That(actual, Is.EqualTo(double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture)), $"{suffix} 0x{raw:X}");
    }
  }

  [Test]
  public void Posit8_Decode_MatchesSpecOracle() => Check("posit8.decode.tsv", r => Posit8.FromRaw((byte)r).ToDouble());

  [Test]
  public void Posit16_Decode_MatchesSpecOracle() => Check("posit16.decode.tsv", r => Posit16.FromRaw((ushort)r).ToDouble());

  [Test]
  public void Posit32_Decode_MatchesSpecOracle() => Check("posit32.decode.tsv", r => Posit32.FromRaw(r).ToDouble());

  // ---- encode: FromDouble must match the nearest-even oracle (round-trip identity + rounding direction) ----

  private static void CheckEncode(string suffix, Func<double, uint> encode) {
    var asm = typeof(PositGoldenTests).Assembly;
    string? name = null;
    foreach (var n in asm.GetManifestResourceNames())
      if (n.EndsWith(suffix, StringComparison.Ordinal)) {
        name = n;
        break;
      }

    Assert.That(name, Is.Not.Null, $"resource '{suffix}' not found");
    using var reader = new StreamReader(asm.GetManifestResourceStream(name!)!);
    string? line;
    while ((line = reader.ReadLine()) != null) {
      if (line.Length == 0 || line[0] == '#')
        continue;
      var tab = line.IndexOf('\t');
      var token = line.Substring(0, tab);
      var expected = uint.Parse(line.Substring(tab + 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
      var value = token == "NaN" ? double.NaN
        : token == "Infinity" ? double.PositiveInfinity
        : token == "-Infinity" ? double.NegativeInfinity
        : double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture);
      Assert.That(encode(value), Is.EqualTo(expected), $"{suffix} encode {token}");
    }
  }

  [Test]
  public void Posit8_Encode_MatchesSpecOracle() => CheckEncode("posit8.encode.tsv", v => Posit8.FromDouble(v).RawValue);

  [Test]
  public void Posit16_Encode_MatchesSpecOracle() => CheckEncode("posit16.encode.tsv", v => Posit16.FromDouble(v).RawValue);

  [Test]
  public void Posit32_Encode_MatchesSpecOracle() => CheckEncode("posit32.encode.tsv", v => Posit32.FromDouble(v).RawValue);
}
