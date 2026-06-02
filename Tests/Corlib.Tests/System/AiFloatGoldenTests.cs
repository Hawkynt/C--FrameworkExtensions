using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace System;

/// <summary>
/// Spec-compatibility checks against golden masters produced by the ml_dtypes reference library
/// (see TestData/generate_golden.py): E2M1 = float4_e2m1fn, E4M3 = float8_e4m3fn, Quarter = float8_e5m2,
/// E8M0 = float8_e8m0fnu. Vectors are embedded as resources.
/// </summary>
[TestFixture]
public class AiFloatGoldenTests {

  private static IEnumerable<(int raw, float value)> ReadDecode(string suffix) {
    foreach (var (a, b) in ReadRows(suffix))
      yield return (int.Parse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture), ParseFloat(b));
  }

  private static IEnumerable<(float value, int raw)> ReadEncode(string suffix) {
    foreach (var (a, b) in ReadRows(suffix))
      yield return (ParseFloat(a), int.Parse(b, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
  }

  private static IEnumerable<(string, string)> ReadRows(string suffix) {
    var asm = typeof(AiFloatGoldenTests).Assembly;
    string? name = null;
    foreach (var n in asm.GetManifestResourceNames())
      if (n.EndsWith(suffix, StringComparison.Ordinal)) {
        name = n;
        break;
      }

    Assert.That(name, Is.Not.Null, $"embedded golden resource '{suffix}' not found");
    using var stream = asm.GetManifestResourceStream(name!);
    using var reader = new StreamReader(stream!);
    string? line;
    while ((line = reader.ReadLine()) != null) {
      if (line.Length == 0 || line[0] == '#')
        continue;
      var tab = line.IndexOf('\t');
      yield return (line.Substring(0, tab), line.Substring(tab + 1));
    }
  }

  private static float ParseFloat(string s) => s switch {
    "NaN" => float.NaN,
    "Infinity" => float.PositiveInfinity,
    "-Infinity" => float.NegativeInfinity,
    _ => float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture),
  };

  private static void AssertSame(float expected, float actual, string because) {
    if (float.IsNaN(expected))
      Assert.That(float.IsNaN(actual), Is.True, because + " (expected NaN)");
    else
      Assert.That(actual, Is.EqualTo(expected), because);
  }

  // ---- decode (ToSingle) : exhaustive ----

  [Test]
  public void E2M1_Decode_MatchesMlDtypes() {
    foreach (var (raw, value) in ReadDecode("e2m1.decode.tsv"))
      AssertSame(value, E2M1.FromRaw((byte)raw).ToSingle(), $"E2M1 0x{raw:X}");
  }

  [Test]
  public void E4M3_Decode_MatchesMlDtypes() {
    foreach (var (raw, value) in ReadDecode("e4m3.decode.tsv"))
      AssertSame(value, E4M3.FromRaw((byte)raw).ToSingle(), $"E4M3 0x{raw:X2}");
  }

  [Test]
  public void Quarter_Decode_MatchesMlDtypes() {
    foreach (var (raw, value) in ReadDecode("quarter.decode.tsv"))
      AssertSame(value, Quarter.FromRaw((byte)raw).ToSingle(), $"Quarter 0x{raw:X2}");
  }

  [Test]
  public void E8M0_Decode_MatchesMlDtypes() {
    foreach (var (raw, value) in ReadDecode("e8m0.decode.tsv"))
      AssertSame(value, E8M0.FromRaw((byte)raw).ToSingle(), $"E8M0 0x{raw:X2}");
  }

  // ---- encode (FromSingle) : sampled, round-to-nearest-even ----

  [Test]
  public void E2M1_Encode_MatchesMlDtypes() {
    foreach (var (value, raw) in ReadEncode("e2m1.encode.tsv"))
      Assert.That(E2M1.FromSingle(value).RawValue, Is.EqualTo((byte)raw), $"E2M1 encode {value}");
  }

  [Test]
  public void E4M3_Encode_MatchesMlDtypes() {
    foreach (var (value, raw) in ReadEncode("e4m3.encode.tsv"))
      Assert.That(E4M3.FromSingle(value).RawValue, Is.EqualTo((byte)raw), $"E4M3 encode {value}");
  }

  [Test]
  public void Quarter_Encode_MatchesMlDtypes() {
    foreach (var (value, raw) in ReadEncode("quarter.encode.tsv"))
      Assert.That(Quarter.FromSingle(value).RawValue, Is.EqualTo((byte)raw), $"Quarter encode {value}");
  }
}
