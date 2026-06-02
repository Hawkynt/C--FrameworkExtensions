using System.Globalization;
using NUnit.Framework;

namespace System;

[TestFixture]
public class BitBenchDateTimeTests {

  // ---- FileTime ----

  [Test]
  public void FileTime_UnixEpoch_HasKnownTicks()
    => Assert.AreEqual(116444736000000000UL, FileTime.FromDateTime(new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).RawValue);

  [Test]
  public void FileTime_KnownRaw_DecodesToUnixEpoch()
    => Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), FileTime.FromRaw(116444736000000000UL).ToDateTime());

  [Test]
  public void FileTime_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, FileTime.FromDateTime(dt).ToDateTime());
  }

  // ---- UnixTime32 ----

  [Test]
  public void UnixTime32_Zero_IsUnixEpoch()
    => Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), UnixTime32.FromRaw(0).ToDateTime());

  [Test]
  public void UnixTime32_KnownValue_Decodes()
    => Assert.AreEqual(new DateTime(2001, 9, 9, 1, 46, 40, DateTimeKind.Utc), UnixTime32.FromRaw(1000000000).ToDateTime());

  [Test]
  public void UnixTime32_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, UnixTime32.FromDateTime(dt).ToDateTime());
  }

  // ---- UnixTime64 ----

  [Test]
  public void UnixTime64_Zero_IsUnixEpoch()
    => Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), UnixTime64.FromRaw(0).ToDateTime());

  [Test]
  public void UnixTime64_NegativeValue_IsBeforeEpoch()
    => Assert.AreEqual(new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc), UnixTime64.FromRaw(-1).ToDateTime());

  [Test]
  public void UnixTime64_RoundTrips() {
    var dt = new DateTime(2100, 1, 15, 8, 30, 0, DateTimeKind.Utc);
    Assert.AreEqual(dt, UnixTime64.FromDateTime(dt).ToDateTime());
  }

  // ---- NtpTimestamp ----

  [Test]
  public void NtpTimestamp_SecondsZero_IsNtpEpoch()
    => Assert.AreEqual(new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc), NtpTimestamp.FromRaw(0).ToDateTime());

  [Test]
  public void NtpTimestamp_UnixEpoch_HasKnownSeconds()
    => Assert.AreEqual(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), NtpTimestamp.FromRaw(2208988800UL << 32).ToDateTime());

  [Test]
  public void NtpTimestamp_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    var rt = NtpTimestamp.FromDateTime(dt).ToDateTime();
    Assert.That((rt - dt).Duration().TotalMilliseconds, Is.LessThan(1.0));
  }

  // ---- OleDate ----

  [Test]
  public void OleDate_ZeroDays_Is18991230() {
    var raw = (ulong)BitConverter.DoubleToInt64Bits(0.0);
    Assert.AreEqual(new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Utc), OleDate.FromRaw(raw).ToDateTime());
  }

  [Test]
  public void OleDate_DaysProperty_Reflects25569ForUnixEpoch() {
    var ole = OleDate.FromDateTime(new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    Assert.AreEqual(25569.0, ole.Days);
  }

  [Test]
  public void OleDate_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, OleDate.FromDateTime(dt).ToDateTime());
  }

  // ---- HfsPlusDate ----

  [Test]
  public void HfsPlusDate_Zero_Is19040101()
    => Assert.AreEqual(new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc), HfsPlusDate.FromRaw(0).ToDateTime());

  [Test]
  public void HfsPlusDate_UnixEpoch_HasKnownSeconds()
    => Assert.AreEqual(2082844800u, HfsPlusDate.FromDateTime(new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).RawValue);

  [Test]
  public void HfsPlusDate_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, HfsPlusDate.FromDateTime(dt).ToDateTime());
  }

  // ---- GpsTime ----

  [Test]
  public void GpsTime_Zero_IsGpsEpoch()
    => Assert.AreEqual(new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc), GpsTime.FromRaw(0).ToDateTime());

  [Test]
  public void GpsTime_OneDay_Is19800107()
    => Assert.AreEqual(new DateTime(1980, 1, 7, 0, 0, 0, DateTimeKind.Utc), GpsTime.FromRaw(86400).ToDateTime());

  [Test]
  public void GpsTime_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, GpsTime.FromDateTime(dt).ToDateTime());
  }

  // ---- WebKitTime ----

  [Test]
  public void WebKitTime_Zero_Is16010101()
    => Assert.AreEqual(new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc), WebKitTime.FromRaw(0).ToDateTime());

  [Test]
  public void WebKitTime_UnixEpoch_HasKnownMicroseconds()
    => Assert.AreEqual(11644473600000000UL, WebKitTime.FromDateTime(new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).RawValue);

  [Test]
  public void WebKitTime_RoundTrips() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, WebKitTime.FromDateTime(dt).ToDateTime());
  }

  // ---- DosDateTime ----

  [Test]
  public void DosDateTime_19800101_PacksToKnownValue()
    => Assert.AreEqual(0x00210000u, DosDateTime.FromDateTime(new(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc)).RawValue);

  [Test]
  public void DosDateTime_KnownRaw_Decodes() {
    var dos = DosDateTime.FromRaw(0x00210000u);
    Assert.AreEqual(1980, dos.Year);
    Assert.AreEqual(1, dos.Month);
    Assert.AreEqual(1, dos.Day);
    Assert.AreEqual(0, dos.Hour);
    Assert.AreEqual(0, dos.Minute);
    Assert.AreEqual(0, dos.Second);
  }

  [Test]
  public void DosDateTime_RoundTrips() {
    // Seconds must be even because DOS stores second/2.
    var dt = new DateTime(2026, 6, 2, 13, 45, 30, DateTimeKind.Utc);
    Assert.AreEqual(dt, DosDateTime.FromDateTime(dt).ToDateTime());
  }

  [Test]
  public void DosDateTime_OddSecond_TruncatesToEven() {
    var dt = new DateTime(2026, 6, 2, 13, 45, 31, DateTimeKind.Utc);
    Assert.AreEqual(30, DosDateTime.FromDateTime(dt).Second);
  }

  // ---- ToString (ISO-8601) ----

  [Test]
  public void ToString_ProducesIso8601() {
    var expected = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture);
    Assert.AreEqual(expected, UnixTime32.FromRaw(0).ToString());
  }

}
