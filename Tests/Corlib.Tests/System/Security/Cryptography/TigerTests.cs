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

using System.Text;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Security.Cryptography;

using Collections.Generic;

[TestFixture]
public class TigerTests {

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
    => ExecuteTest(() => new Tiger(numberOfBits).ComputeHash(Encoding.ASCII.GetBytes(input)).Select(b => b.ToString("x2")).Join(string.Empty), expected, null);

}

