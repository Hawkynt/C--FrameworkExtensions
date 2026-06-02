using NUnit.Framework;

namespace System.Collections;

[TestFixture]
public class PackedBitBufferTests {

  [Test]
  [TestCase(0, 4, 0)]
  [TestCase(1, 4, 1)]
  [TestCase(2, 4, 1)]
  [TestCase(3, 4, 2)]
  [TestCase(8, 7, 7)]   // matches Ascii7BitPacking: 8 chars -> 7 bytes
  [TestCase(10, 3, 4)]  // 30 bits -> 4 bytes
  [TestCase(1, 64, 8)]
  [TestCase(5, 13, 9)]  // 65 bits -> 9 bytes
  public void GetPackedByteCount_IsCorrect(int count, int bits, int expected)
    => Assert.AreEqual(expected, PackedBitBuffer.GetPackedByteCount(count, bits));

  [Test]
  [TestCase(1)]
  [TestCase(2)]
  [TestCase(3)]
  [TestCase(4)]
  [TestCase(5)]
  [TestCase(7)]
  [TestCase(8)]
  [TestCase(11)]
  [TestCase(12)]
  [TestCase(13)]
  [TestCase(16)]
  [TestCase(17)]
  [TestCase(31)]
  [TestCase(32)]
  [TestCase(33)]
  [TestCase(63)]
  [TestCase(64)]
  public void RoundTrip_AllWidths_BothOrders(int bits) {
    RoundTripCore<LsbFirst>(bits);
    RoundTripCore<MsbFirst>(bits);
  }

  private static void RoundTripCore<TBitOrder>(int bits) where TBitOrder : struct, IBitOrder {
    const int count = 37;
    var buffer = new PackedBitBuffer<TBitOrder>(count, bits);
    var mask = bits == 64 ? ulong.MaxValue : (1UL << bits) - 1;

    // a deterministic spread of values, including the extremes that exercise straddling
    var values = new ulong[count];
    for (var i = 0; i < count; ++i)
      values[i] = unchecked((ulong)i * 0x9E3779B97F4A7C15UL + 0x1234567UL) & mask;
    values[0] = 0;
    values[1] = mask;
    values[2] = mask ^ 1;

    for (var i = 0; i < count; ++i)
      buffer.SetBits(i, values[i]);

    for (var i = 0; i < count; ++i)
      Assert.AreEqual(values[i], buffer.GetBits(i), $"width={bits}, order={typeof(TBitOrder).Name}, index={i}");
  }

  [Test]
  public void OverWideValue_IsTruncatedToWidth() {
    var buffer = new PackedBitBuffer<LsbFirst>(1, 4);
    buffer.SetBits(0, 0xABCD);
    Assert.AreEqual(0xDUL, buffer.GetBits(0)); // only low 4 bits retained
  }

  [Test]
  public void NeighboringElements_AreNotCorrupted() {
    const int bits = 5;
    var buffer = new PackedBitBuffer<LsbFirst>(4, bits);
    buffer.SetBits(0, 0x1F);
    buffer.SetBits(1, 0x00);
    buffer.SetBits(2, 0x1F);
    buffer.SetBits(3, 0x0A);
    // overwrite the middle and ensure others stand
    buffer.SetBits(2, 0x15);
    Assert.AreEqual(0x1FUL, buffer.GetBits(0));
    Assert.AreEqual(0x00UL, buffer.GetBits(1));
    Assert.AreEqual(0x15UL, buffer.GetBits(2));
    Assert.AreEqual(0x0AUL, buffer.GetBits(3));
  }

  [Test]
  public void LsbFirst_ProducesExpectedByteLayout() {
    // two 4-bit codes 0x3 then 0xC, LSB-first -> low nibble first => byte 0xC3
    var buffer = new PackedBitBuffer<LsbFirst>(2, 4);
    buffer.SetBits(0, 0x3);
    buffer.SetBits(1, 0xC);
    Assert.AreEqual((byte)0xC3, buffer.PackedData[0]);
  }

  [Test]
  public void MsbFirst_ProducesExpectedByteLayout() {
    // two 4-bit codes 0x3 then 0xC, MSB-first -> high nibble first => byte 0x3C
    var buffer = new PackedBitBuffer<MsbFirst>(2, 4);
    buffer.SetBits(0, 0x3);
    buffer.SetBits(1, 0xC);
    Assert.AreEqual((byte)0x3C, buffer.PackedData[0]);
  }

  [Test]
  public void PackUnpack_RoundTrips() {
    const int bits = 12;
    const int count = 20;
    var buffer = new PackedBitBuffer<LsbFirst>(count, bits);
    var codes = new ulong[count];
    for (var i = 0; i < count; ++i)
      codes[i] = (ulong)((i * 173 + 7) & 0xFFF);

    buffer.Pack(codes);
    var roundTripped = new ulong[count];
    buffer.Unpack(roundTripped);
    Assert.AreEqual(codes, roundTripped);
  }

  [Test]
  public void FromPacked_TooSmall_Throws()
    => Assert.Throws<ArgumentOutOfRangeException>(() => PackedBitBuffer<LsbFirst>.FromPacked(new byte[1], 4, 4));

  [Test]
  public void GetBits_OutOfRange_Throws() {
    var buffer = new PackedBitBuffer<LsbFirst>(2, 4);
    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetBits(2));
    Assert.Throws<ArgumentOutOfRangeException>(() => buffer.GetBits(-1));
  }

  // ---- typed view: PackedBuffer<T, TBitOrder> (interface codec) ----

  [Test]
  public void PackedBuffer_Unsigned_RoundTrips() {
    var buffer = new PackedBuffer<ulong, LsbFirst>(8, new UnsignedBitCodec(5));
    for (var i = 0; i < 8; ++i)
      buffer[i] = (ulong)(i * 3) & 0x1F;
    for (var i = 0; i < 8; ++i)
      Assert.AreEqual((ulong)(i * 3) & 0x1F, buffer[i]);
  }

  [Test]
  public void PackedBuffer_Signed_SignExtends() {
    var buffer = new PackedBuffer<long, LsbFirst>(4, new SignedBitCodec(4)); // range -8..7
    buffer[0] = -1;
    buffer[1] = -8;
    buffer[2] = 7;
    buffer[3] = 0;
    Assert.AreEqual(-1L, buffer[0]);
    Assert.AreEqual(-8L, buffer[1]);
    Assert.AreEqual(7L, buffer[2]);
    Assert.AreEqual(0L, buffer[3]);
  }

  [Test]
  public void PackedBuffer_DecodeTo_And_ToArray_Agree() {
    var buffer = new PackedBuffer<ulong, LsbFirst>(6, new UnsignedBitCodec(7));
    var source = new ulong[] { 1, 2, 64, 127, 0, 100 };
    buffer.EncodeFrom(source);
    var destination = new ulong[6];
    buffer.DecodeTo(destination);
    Assert.AreEqual(source, destination);
    Assert.AreEqual(source, buffer.ToArray());
  }

  [Test]
  public void PackedBuffer_WidthMismatch_Throws() {
    var storage = new PackedBitBuffer<LsbFirst>(4, 5);
    Assert.Throws<ArgumentException>(() => new PackedBuffer<ulong, LsbFirst>(storage, new UnsignedBitCodec(6)));
  }

  // ---- typed view: PackedBuffer<T, TCodec, TBitOrder> (fully zero-cost) ----

  [Test]
  public void PackedBufferGeneric_MatchesInterfaceVariant() {
    var generic = new PackedBuffer<ulong, UnsignedBitCodec, LsbFirst>(8, new UnsignedBitCodec(6));
    var iface = new PackedBuffer<ulong, LsbFirst>(8, new UnsignedBitCodec(6));
    for (var i = 0; i < 8; ++i) {
      var v = (ulong)(i * 9) & 0x3F;
      generic[i] = v;
      iface[i] = v;
    }
    Assert.AreEqual(iface.ToArray(), generic.ToArray());
  }

  [Test]
  public void PackedBufferGeneric_MsbFirst_RoundTrips() {
    var buffer = new PackedBuffer<ulong, UnsignedBitCodec, MsbFirst>(8, new UnsignedBitCodec(6));
    for (var i = 0; i < 8; ++i)
      buffer[i] = (ulong)(i * 7) & 0x3F;
    for (var i = 0; i < 8; ++i)
      Assert.AreEqual((ulong)(i * 7) & 0x3F, buffer[i]);
  }
}
