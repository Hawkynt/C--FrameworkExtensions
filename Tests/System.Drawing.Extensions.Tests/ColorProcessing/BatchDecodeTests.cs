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

using Hawkynt.ColorProcessing.Codecs;
using Hawkynt.ColorProcessing.Storage;
using Hawkynt.ColorProcessing.Working;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Bit-exactness verification for native <see cref="IBatchDecode{TPixel,TWork}"/> implementations.
/// Each codec MUST produce element-wise identical output between its scalar
/// <c>Decode(in TPixel)</c> and span-based <c>DecodeBatch(...)</c> paths.
/// </summary>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("Codecs")]
public class BatchDecodeTests {

  private const int _SAMPLE_COUNT = 1024;

  [Test]
  public void Srgb32ToLinearRgbaF_BatchEqualsScalar() {
    var codec = new Srgb32ToLinearRgbaF();
    var rng = new Random(0xC0DEC);
    var src = new Bgra8888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      src[i] = new Bgra8888((uint)rng.Next());

    // Scalar reference
    var scalar = new LinearRgbaF[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      scalar[i] = codec.Decode(src[i]);

    // Batch
    var batch = new LinearRgbaF[_SAMPLE_COUNT];
    codec.DecodeBatch(src, batch);

    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      Assert.That(_BitwiseEqual(scalar[i], batch[i]), Is.True,
        $"DecodeBatch differs from Decode at i={i}: scalar={scalar[i]}, batch={batch[i]}");
  }

  [Test]
  public void Rgb24ToLinearRgbF_BatchEqualsScalar() {
    var codec = new Rgb24ToLinearRgbF();
    var rng = new Random(0xC0DEC);
    var src = new Bgr888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      src[i] = new Bgr888((byte)rng.Next(256), (byte)rng.Next(256), (byte)rng.Next(256));

    var scalar = new LinearRgbF[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      scalar[i] = codec.Decode(src[i]);

    var batch = new LinearRgbF[_SAMPLE_COUNT];
    codec.DecodeBatch(src, batch);

    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      Assert.That(_BitwiseEqual(scalar[i], batch[i]), Is.True,
        $"DecodeBatch differs from Decode at i={i}: scalar={scalar[i]}, batch={batch[i]}");
  }

  [Test]
  public void Srgb32ToOklabaF_BatchEqualsScalar() {
    var codec = new Srgb32ToOklabaF();
    var rng = new Random(0xC0DEC);
    var src = new Bgra8888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      src[i] = new Bgra8888((uint)rng.Next());

    var scalar = new OklabaF[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      scalar[i] = codec.Decode(src[i]);

    var batch = new OklabaF[_SAMPLE_COUNT];
    codec.DecodeBatch(src, batch);

    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      Assert.That(_BitwiseEqual(scalar[i], batch[i]), Is.True,
        $"DecodeBatch differs from Decode at i={i}: scalar={scalar[i]}, batch={batch[i]}");
  }

  [Test]
  public void IdentityDecode_BatchEqualsScalar() {
    var codec = new IdentityDecode<Bgra8888>();
    var rng = new Random(0xC0DEC);
    var src = new Bgra8888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      src[i] = new Bgra8888((uint)rng.Next());

    var scalar = new Bgra8888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      scalar[i] = codec.Decode(src[i]);

    var batch = new Bgra8888[_SAMPLE_COUNT];
    codec.DecodeBatch(src, batch);

    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      Assert.That(scalar[i].Packed, Is.EqualTo(batch[i].Packed),
        $"DecodeBatch differs from Decode at i={i}");
  }

  [Test]
  public void BatchDecodeAdapter_RoundTripsThroughScalar() {
    // Adapter wraps a per-pixel decoder; output MUST equal the underlying
    // Decode loop element-wise (this is the definition of the fallback).
    var inner = new Srgb32ToLinearRgbaF();
    var adapter = new BatchDecodeAdapter<Srgb32ToLinearRgbaF, Bgra8888, LinearRgbaF>(inner);
    var rng = new Random(0xADAD7E);
    var src = new Bgra8888[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      src[i] = new Bgra8888((uint)rng.Next());

    var direct = new LinearRgbaF[_SAMPLE_COUNT];
    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      direct[i] = inner.Decode(src[i]);

    var viaAdapter = new LinearRgbaF[_SAMPLE_COUNT];
    adapter.DecodeBatch(src, viaAdapter);

    for (var i = 0; i < _SAMPLE_COUNT; ++i)
      Assert.That(_BitwiseEqual(direct[i], viaAdapter[i]), Is.True,
        $"BatchDecodeAdapter differs from inner.Decode at i={i}");
  }

  // Bit-exact float comparison via re-interpret to int. Decode and DecodeBatch must
  // produce literally identical bit patterns since they use the same constants.
  private static unsafe int _Bits(float f) => *(int*)&f;

  private static bool _BitwiseEqual(LinearRgbaF a, LinearRgbaF b)
    => _Bits(a.R) == _Bits(b.R)
    && _Bits(a.G) == _Bits(b.G)
    && _Bits(a.B) == _Bits(b.B)
    && _Bits(a.A) == _Bits(b.A);

  private static bool _BitwiseEqual(LinearRgbF a, LinearRgbF b)
    => _Bits(a.R) == _Bits(b.R)
    && _Bits(a.G) == _Bits(b.G)
    && _Bits(a.B) == _Bits(b.B);

  private static bool _BitwiseEqual(OklabaF a, OklabaF b)
    => _Bits(a.L) == _Bits(b.L)
    && _Bits(a.A) == _Bits(b.A)
    && _Bits(a.B) == _Bits(b.B)
    && _Bits(a.Alpha) == _Bits(b.Alpha);
}
