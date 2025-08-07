#region (c)2010-2042 Hawkynt

/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software:
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.
    If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Security.Cryptography;

[TestFixture]
public class HashAlgorithmsTests {
  [Test]
  [TestCase(32, "", "00000001")]
  [TestCase(32, "a", "00620062")]
  [TestCase(32, "abc", "024d0127")]
  [TestCase(32, "Wikipedia", "11e60398")]
  [TestCase(32, "message digest", "29750586")]
  [TestCase(32, "abcdefghijklmnopqrstuvwxyz", "90860b20")]
  [TestCase(32, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "8adb150c")]
  [TestCase(32, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "c4062ac7")]
  public void Adler32(int numberOfBits, string input, string expected)
    => ExecuteTest(() => input.ComputeHash(new Adler(numberOfBits)), expected, null);

  [Test]
  [TestCase(8, "", "00")]
  [TestCase(8, "a", "77")]
  [TestCase(8, "abc", "19")]
  [TestCase(8, "message digest", "93")]
  [TestCase(8, "abcdefghijklmnopqrstuvwxyz", "cc")]
  [TestCase(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "c2")]
  [TestCase(8, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "90")]
  [TestCase(16, "", "0000")]
  [TestCase(16, "a", "6161")]
  [TestCase(16, "abc", "4c27")]
  [TestCase(16, "message digest", "908a")]
  [TestCase(16, "abcdefghijklmnopqrstuvwxyz", "fc2a")]
  [TestCase(16, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "0c20")]
  [TestCase(16, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "bdf0")]
  [TestCase(32, "", "00000000")]
  [TestCase(32, "a", "00610061")]
  [TestCase(32, "ab", "62616261")]
  [TestCase(32, "abc", "c52562c4")]
  [TestCase(32, "abcd", "2926c6c4")]
  [TestCase(32, "abcde", "f04fc729")]
  [TestCase(32, "abcdefgh", "ebe19591")]
  [TestCase(32, "message digest", "7c9da3e6")]
  [TestCase(32, "abcdefghijklmnopqrstuvwxyz", "d3789b8e")]
  [TestCase(32, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "e1f39f80")]
  [TestCase(32, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "f2b89957")]
  [TestCase(64, "", "0000000000000000")]
  [TestCase(64, "a", "0000006100000061")]
  [TestCase(64, "ab", "0000626100006261")]
  [TestCase(64, "abc", "0063626100636261")]
  [TestCase(64, "abcd", "6463626164636261")]
  [TestCase(64, "abcde", "c8c6c527646362c6")]
  [TestCase(64, "abcdefgh", "312e2b28cccac8c6")]
  [TestCase(64, "message digest", "f9cd1314f940aaa5")]
  [TestCase(64, "abcdefghijklmnopqrstuvwxyz", "5f44a387969104fd")]
  [TestCase(64, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "3ab328a245365a4a")]
  [TestCase(64, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "f5d1f5ebe4c2b494")]
  public void Fletcher(int numberOfBits, string input, string expected)
    => ExecuteTest(() => input.ComputeHash(new Fletcher(numberOfBits)), expected, null);

  [Test]
  [TestCase(192, "", "3293ac630c13f0245f92bbb1766e16167a4e58492dde73f3")]
  [TestCase(192, "abc", "2aab1484e8c158f2bfb8c5ff41b57a525129131c957b5f93")]
  [TestCase(192, "Tiger", "dd00230799f5009fec6debc838bb6a27df2b9d6f110c7937")]
  [TestCase(192, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "f71c8583902afb879edfe610f82c0d4786a3a534504486b5")]
  [TestCase(192, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "c54034e5b43eb8005848a7e0ae6aac76e4ff590ae715fd25")]
  [TestCase(160, "", "3293ac630c13f0245f92bbb1766e16167a4e5849")]
  [TestCase(160, "abc", "2aab1484e8c158f2bfb8c5ff41b57a525129131c")]
  [TestCase(160, "Tiger", "dd00230799f5009fec6debc838bb6a27df2b9d6f")]
  [TestCase(160, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "f71c8583902afb879edfe610f82c0d4786a3a534")]
  [TestCase(160, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "c54034e5b43eb8005848a7e0ae6aac76e4ff590a")]
  [TestCase(128, "", "3293ac630c13f0245f92bbb1766e1616")]
  [TestCase(128, "abc", "2aab1484e8c158f2bfb8c5ff41b57a52")]
  [TestCase(128, "Tiger", "dd00230799f5009fec6debc838bb6a27")]
  [TestCase(128, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "f71c8583902afb879edfe610f82c0d47")]
  [TestCase(128, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-", "c54034e5b43eb8005848a7e0ae6aac76")]
  public void Tiger(int numberOfBits, string input, string expected)
    => ExecuteTest(() => input.ComputeHash(new Tiger(numberOfBits)), expected, null);
}
