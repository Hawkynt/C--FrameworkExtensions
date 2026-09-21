#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
//
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
//
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
//
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Hawkynt.ColorProcessing.ColorMath;
using Hawkynt.ColorProcessing.Resizing;
using Hawkynt.ColorProcessing.Resizing.Rescalers;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using Hawkynt.Drawing;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Pins the two behavioural differences xBRZ 1.9 introduced, and pins the pre-1.9 entry against
/// acquiring them by accident.
/// </summary>
/// <remarks>
/// <para>
/// 1.9 changed exactly two things: the steep/shallow direction threshold rose from 2.2 to 2.4, and
/// the weighted and 50/50 blends round to nearest rather than truncating. Both are exact and
/// integer, so both are pinned exactly rather than by eye or by tolerance.
/// </para>
/// <para>
/// The two single-change variants (<see cref="XbrzVariant.SteepThresholdOnly"/> and
/// <see cref="XbrzVariant.RoundedBlendsOnly"/>) exist so a failing test names which of the two
/// changes broke. Asserting only against the combined 1.9 variant would let either change go
/// missing without any test noticing, because the other would still produce a difference.
/// </para>
/// </remarks>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Scalers")]
public class XbrzVersionTests {

  #region helpers

  /// <summary>
  /// Builds a deterministic source image. The speckle field gives the corner pre-pass a wide spread
  /// of accumulated colour-distance ratios, so some corners land between the 2.2 and the 2.4
  /// threshold; the diagonals and the circle give the line-blend paths something to blend.
  /// </summary>
  private static Bitmap _CreateSource(int width, int height) {
    var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    var state = 0x12345678u;

    uint Next() {
      state ^= state << 13;
      state ^= state >> 17;
      state ^= state << 5;
      return state;
    }

    for (var y = 0; y < height; ++y)
      for (var x = 0; x < width; ++x) {
        int r, g, b;
        if (Math.Abs(x - y) <= 1) {
          r = 250;
          g = 20;
          b = 20;
        } else if (Math.Abs(x * 2 - y) <= 1) {
          r = 17;
          g = 200;
          b = 240;
        } else if (Math.Abs(x + y - width) <= 1) {
          r = 40;
          g = 240;
          b = 60;
        } else {
          var dx = x - width / 2.0;
          var dy = y - height / 2.0;
          if (Math.Abs(Math.Sqrt(dx * dx + dy * dy) - width / 3.0) < 1.0) {
            r = 255;
            g = 255;
            b = 30;
          } else {
            var n = Next();
            r = (int)(n & 0xFF);
            g = (int)((n >> 8) & 0xFF);
            b = (int)((n >> 16) & 0xFF);
          }
        }

        result.SetPixel(x, y, Color.FromArgb(255, r, g, b));
      }

    return result;
  }

  private static byte[] _RawPixels(Bitmap bitmap) {
    var data = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try {
      var stride = Math.Abs(data.Stride);
      var result = new byte[stride * data.Height];
      var origin = data.Scan0.ToInt64();
      for (var y = 0; y < data.Height; ++y)
        Marshal.Copy(new(origin + y * (long)data.Stride), result, y * stride, stride);

      return result;
    } finally {
      bitmap.UnlockBits(data);
    }
  }

  private static string _HashOf(Bitmap bitmap) {
    using var sha = SHA256.Create();
    var builder = new StringBuilder();
    foreach (var value in sha.ComputeHash(_RawPixels(bitmap)))
      builder.Append(value.ToString("x2"));

    return builder.ToString();
  }

  private static Bitmap _Scale(Bitmap source, int scale, XbrzVariant variant)
    => source.Upscale(new Xbrz(scale, variant), ScalerQuality.Fast);

  private static Bgra8888 _Color(byte c1, byte c2, byte c3, byte a) => new(c1, c2, c3, a);

  #endregion

  #region the pre-1.9 entry must not move

  /// <summary>
  /// The hashes below were taken from the <c>xBRZ</c> entry's output <b>before</b> 1.9 support was
  /// added. They are the whole point of the exercise: a version selector that quietly changes what
  /// the existing entry produces is a regression for every script and saved setting naming it.
  /// </summary>
  [Test]
  [Category("HappyPath")]
  [TestCase(2, "bd63fc67557bdc45e6e9fbcf2336beec6b9bb931d496ea3239bc70a7e34d441c")]
  [TestCase(3, "0e5c5cfeca7b4f75a293d0052af83a514faff8aa2776dada95ffe5740162c1eb")]
  [TestCase(4, "e5585ded852f40c0b513072c60a6d597d3b0130d2eabb2a28e7543c5b09aab74")]
  [TestCase(5, "7269fb01f984812c2df58ce1f9ae7215ce32540bee09e1fcf4ab173c03efc25d")]
  [TestCase(6, "d7372f9420f3e8bdeeedfe8cfd993fad652014ed72259f6adbff3d3d78b15a44")]
  public void Xbrz_AtEveryScale_StillProducesTheOutputItProducedBefore19Existed(int scale, string expected) {
    using var source = _CreateSource(24, 24);
    using var actual = source.Upscale(new Xbrz(scale), ScalerQuality.Fast);

    Assert.That(actual.Width, Is.EqualTo(24 * scale));
    Assert.That(_HashOf(actual), Is.EqualTo(expected), $"the xBRZ {scale}x entry changed its output");
  }

  [Test]
  [Category("HappyPath")]
  public void Xbrz_DefaultConstructed_SelectsThePre19Behaviour() {
    using var source = _CreateSource(24, 24);
    using var viaDefault = source.Upscale(new Xbrz(2), ScalerQuality.Fast);
    using var viaClassic = _Scale(source, 2, XbrzVariant.Classic);

    Assert.That(_RawPixels(viaDefault), Is.EqualTo(_RawPixels(viaClassic)));
  }

  #endregion

  #region the steep direction threshold: 2.2 before 1.9, 2.4 from 1.9

  /// <summary>
  /// Raising the threshold makes the steep/shallow test harder to satisfy, so a source whose
  /// accumulated distance ratios land between the two thresholds must classify differently. With
  /// blending held at the pre-1.9 truncating mode, any difference here is the threshold's alone.
  /// </summary>
  [Test]
  [Category("HappyPath")]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(6)]
  public void SteepThreshold_RaisedTo24_ChangesHowSomeNearDiagonalsAreClassified(int scale) {
    using var source = _CreateSource(48, 48);
    using var at22 = _Scale(source, scale, XbrzVariant.Classic);
    using var at24 = _Scale(source, scale, XbrzVariant.SteepThresholdOnly);

    Assert.That(_RawPixels(at24), Is.Not.EqualTo(_RawPixels(at22)),
      $"the {scale}x kernel classified every corner identically at 2.2 and at 2.4, so the threshold is not being read");
  }

  #endregion

  #region blends: truncated before 1.9, rounded from 1.9

  /// <summary>
  /// A weighted sum with a non-zero remainder against the total weight is exactly where truncation
  /// and rounding must part company. 0*3 + 3*1 = 3 over a total of 4: truncation floors to 0,
  /// rounding takes it to 1.
  /// </summary>
  [Test]
  [Category("HappyPath")]
  public void WeightedLerp_RemainderAgainstTotal_TruncatesButRoundedDoesNot() {
    var lerp = default(Color4BLerpInt<Bgra8888>);
    var a = _Color(0, 0, 0, 0);
    var b = _Color(3, 3, 3, 3);

    var truncated = lerp.Lerp(a, b, 3, 1);
    var rounded = lerp.LerpRounded(a, b, 3, 1);

    Assert.That(truncated.C1, Is.EqualTo(0));
    Assert.That(rounded.C1, Is.EqualTo(1));
    Assert.That(rounded.A, Is.EqualTo(1), "the alpha channel rounds like every other channel");
  }

  /// <summary>The 21/100 corner blend truncates the same way; 3 * 21 = 63 over 100.</summary>
  [Test]
  [Category("HappyPath")]
  public void WeightedLerp_AtTheCornerBlendWeights_TruncatesButRoundedDoesNot() {
    var lerp = default(Color4BLerpInt<Bgra8888>);
    var a = _Color(0, 0, 0, 0);
    var b = _Color(3, 3, 3, 3);

    Assert.That(lerp.Lerp(a, b, 79, 21).C1, Is.EqualTo(0));
    Assert.That(lerp.LerpRounded(a, b, 79, 21).C1, Is.EqualTo(1));
  }

  /// <summary>The 50/50 blend shifts rather than divides, and truncates just the same.</summary>
  [Test]
  [Category("HappyPath")]
  public void MidpointLerp_OddSum_TruncatesButRoundedDoesNot() {
    var lerp = default(Color4BLerpInt<Bgra8888>);
    var a = _Color(0, 4, 10, 0);
    var b = _Color(1, 5, 11, 1);

    var truncated = lerp.Lerp(a, b);
    var rounded = lerp.LerpRounded(a, b);

    Assert.That(truncated.C1, Is.EqualTo(0));
    Assert.That(rounded.C1, Is.EqualTo(1));
    Assert.That(truncated.C2, Is.EqualTo(4));
    Assert.That(rounded.C2, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void Lerp_WhenTheDivisionIsExact_BothModesAgree() {
    var lerp = default(Color4BLerpInt<Bgra8888>);
    var a = _Color(0, 0, 0, 0);
    var b = _Color(4, 8, 40, 4);

    Assert.That(lerp.LerpRounded(a, b, 3, 1).C1, Is.EqualTo(lerp.Lerp(a, b, 3, 1).C1));
    Assert.That(lerp.LerpRounded(a, b, 1, 1).C2, Is.EqualTo(lerp.Lerp(a, b, 1, 1).C2));
  }

  [Test]
  [Category("EdgeCase")]
  public void RoundedLerp_AtTheTopOfTheChannelRange_DoesNotOverflow() {
    var lerp = default(Color4BLerpInt<Bgra8888>);
    var a = _Color(255, 255, 255, 255);
    var b = _Color(255, 255, 255, 255);

    Assert.That(lerp.LerpRounded(a, b).C1, Is.EqualTo(255));
    Assert.That(lerp.LerpRounded(a, b, 3, 1).C1, Is.EqualTo(255));
    Assert.That(lerp.LerpRounded(a, b, 79, 21).A, Is.EqualTo(255));
  }

  [Test]
  [Category("EdgeCase")]
  public void RoundedLerp_ForAThreeComponentSpace_RoundsEveryChannel() {
    var lerp = default(Color3BLerpInt<Bgr888>);
    var a = new Bgr888(0, 0, 0);
    var b = new Bgr888(3, 3, 3);

    Assert.That(lerp.Lerp(a, b, 3, 1).R, Is.EqualTo(0));
    Assert.That(lerp.LerpRounded(a, b, 3, 1).R, Is.EqualTo(1));
    Assert.That(lerp.LerpRounded(a, b, 3, 1).B, Is.EqualTo(1));
  }

  /// <summary>
  /// Floating-point blends never truncated to a representable step, so their rounded members are
  /// deliberately the same operation. Pinning that keeps a later "fix" from making them differ.
  /// </summary>
  [Test]
  [Category("EdgeCase")]
  public void RoundedLerp_ForAFloatSpace_IsTheSameOperationAsTheTruncatingOne() {
    var lerp = default(Color4FLerp<LinearRgbaF>);
    var a = new LinearRgbaF(0f, 0f, 0f, 0f);
    var b = new LinearRgbaF(0.3f, 0.3f, 0.3f, 0.3f);

    Assert.That(lerp.LerpRounded(a, b, 3, 1).C1, Is.EqualTo(lerp.Lerp(a, b, 3, 1).C1));
    Assert.That(lerp.LerpRounded(a, b).C1, Is.EqualTo(lerp.Lerp(a, b).C1));
  }

  /// <summary>
  /// With the threshold held at 2.2, any difference the kernels produce is the rounding's alone.
  /// </summary>
  [Test]
  [Category("HappyPath")]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(6)]
  public void RoundedBlends_InTheKernels_ChangeBlendedPixels(int scale) {
    using var source = _CreateSource(48, 48);
    using var truncating = _Scale(source, scale, XbrzVariant.Classic);
    using var rounding = _Scale(source, scale, XbrzVariant.RoundedBlendsOnly);

    Assert.That(_RawPixels(rounding), Is.Not.EqualTo(_RawPixels(truncating)),
      $"the {scale}x kernel produced identical pixels truncating and rounding, so it is not using the rounded blend");
  }

  #endregion

  #region the 1.9 entry

  [Test]
  [Category("HappyPath")]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(6)]
  public void Xbrz19_AtEveryScale_DiffersFromThePre19Entry(int scale) {
    using var source = _CreateSource(48, 48);
    using var classic = source.Upscale(new Xbrz(scale), ScalerQuality.Fast);
    using var v19 = source.Upscale(new Xbrz19(scale), ScalerQuality.Fast);

    Assert.That(_RawPixels(v19), Is.Not.EqualTo(_RawPixels(classic)));
  }

  [Test]
  [Category("HappyPath")]
  public void Xbrz19_CarriesBothOf19sChanges() {
    using var source = _CreateSource(48, 48);
    using var v19 = source.Upscale(new Xbrz19(2), ScalerQuality.Fast);
    using var steepOnly = _Scale(source, 2, XbrzVariant.SteepThresholdOnly);
    using var roundOnly = _Scale(source, 2, XbrzVariant.RoundedBlendsOnly);

    Assert.That(_RawPixels(v19), Is.Not.EqualTo(_RawPixels(steepOnly)), "1.9 must round its blends, not only raise the threshold");
    Assert.That(_RawPixels(v19), Is.Not.EqualTo(_RawPixels(roundOnly)), "1.9 must raise the threshold, not only round its blends");
  }

  [Test]
  [Category("HappyPath")]
  [TestCase(2)]
  [TestCase(6)]
  public void Xbrz19_OutputDimensions_MatchTheScaleFactor(int scale) {
    using var source = TestUtilities.CreateTestPattern(10, 10);
    using var result = source.Upscale(new Xbrz19(scale), ScalerQuality.Fast);

    Assert.That(result.Width, Is.EqualTo(10 * scale));
    Assert.That(result.Height, Is.EqualTo(10 * scale));
  }

  [Test]
  [Category("EdgeCase")]
  [TestCase(1)]
  [TestCase(7)]
  public void Xbrz19_WithAnUnsupportedScale_Throws(int scale)
    => Assert.That(() => new Xbrz19(scale), Throws.InstanceOf<ArgumentOutOfRangeException>());

  [Test]
  [Category("HappyPath")]
  public void Xbrz19_SupportedScales_AreTwoThroughSix() {
    Assert.That(Xbrz19.SupportedScales.Length, Is.EqualTo(5));
    Assert.That(Xbrz19.SupportsScale(new(6, 6)), Is.True);
    Assert.That(Xbrz19.SupportsScale(new(7, 7)), Is.False);
  }

  /// <summary>
  /// The registry is what the GUI and the command line enumerate, so the pre-1.9 entry keeping its
  /// exact name is what keeps existing scripts and saved settings working.
  /// </summary>
  [Test]
  [Category("HappyPath")]
  public void ScalerRegistry_ListsBothEntries_WithThePre19NameUnchanged() {
    var names = ScalerRegistry.Rescalers.Select(s => s.Name).ToArray();

    Assert.That(names, Does.Contain("xBRZ"));
    Assert.That(names, Does.Contain("xBRZ 1.9"));
  }

  #endregion

}
