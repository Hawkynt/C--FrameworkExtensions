#region (c)2010-2020 Hawkynt
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

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests {
  [TestClass]
  public partial class Test {

    [TestMethod]
    public void Tiger192() {
      foreach (var test in new[] {
        Tuple.Create("","3293ac630c13f0245f92bbb1766e16167a4e58492dde73f3"),
        Tuple.Create("abc","2aab1484e8c158f2bfb8c5ff41b57a525129131c957b5f93"),
        Tuple.Create("Tiger","dd00230799f5009fec6debc838bb6a27df2b9d6f110c7937"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","f71c8583902afb879edfe610f82c0d4786a3a534504486b5"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","c54034e5b43eb8005848a7e0ae6aac76e4ff590ae715fd25"),
      })
        Assert.AreEqual(test.Item2, string.Join(string.Empty, new Tiger192().ComputeHash(Encoding.ASCII.GetBytes(test.Item1)).Select(b => b.ToString("x2"))), "Hash broken");
    }

    [TestMethod]
    public void Tiger160() {
      foreach (var test in new[] {
        Tuple.Create("","3293ac630c13f0245f92bbb1766e16167a4e5849"),
        Tuple.Create("abc","2aab1484e8c158f2bfb8c5ff41b57a525129131c"),
        Tuple.Create("Tiger","dd00230799f5009fec6debc838bb6a27df2b9d6f"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","f71c8583902afb879edfe610f82c0d4786a3a534"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","c54034e5b43eb8005848a7e0ae6aac76e4ff590a"),
      })
        Assert.AreEqual(test.Item2, string.Join(string.Empty, new Tiger160().ComputeHash(Encoding.ASCII.GetBytes(test.Item1)).Select(b => b.ToString("x2"))), "Hash broken");
    }

    [TestMethod]
    public void Tiger128() {
      foreach (var test in new[] {
        Tuple.Create("","3293ac630c13f0245f92bbb1766e1616"),
        Tuple.Create("abc","2aab1484e8c158f2bfb8c5ff41b57a52"),
        Tuple.Create("Tiger","dd00230799f5009fec6debc838bb6a27"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","f71c8583902afb879edfe610f82c0d47"),
        Tuple.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-","c54034e5b43eb8005848a7e0ae6aac76"),
      })
        Assert.AreEqual(test.Item2, string.Join(string.Empty, new Tiger128().ComputeHash(Encoding.ASCII.GetBytes(test.Item1)).Select(b => b.ToString("x2"))), "Hash broken");
    }
  }
}
