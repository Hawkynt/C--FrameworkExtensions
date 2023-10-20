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

using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Text;
using System.Text.RegularExpressions;
// TODO: qp is also in string extensions
namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ConvertExtensions {
    private const string _QP_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<\>?@[]^_-{|}~""";
    private static readonly byte[] _QP_ENCODING_TABLE = new byte[256];
    private static readonly Dictionary<char, byte> _QP_DECODING_TABLE = new();

    private const string _BASE91_CHARS = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_'{|}~""";
    private static readonly char[] _BASE91_ENCODING_TABLE;
    private static readonly Dictionary<char, byte> _BASE91_DECODING_TABLE = new();

    static ConvertExtensions() {
      #region quoted printable
      for (var i = _QP_CHARS.Length; i > 0; ) {
        --i;
        var c = (byte)_QP_CHARS[i];
        _QP_ENCODING_TABLE[c] = c;
      }
      for (var i = 0; i < 16; i++)
        _QP_DECODING_TABLE.Add(i.ToString("X1")[0], (byte)i);
      #endregion

      #region base 91
      _BASE91_ENCODING_TABLE = new char[_BASE91_CHARS.Length];
      for (var i = _BASE91_CHARS.Length; i > 0; ) {
        --i;
        var c = _BASE91_CHARS[i];
        _BASE91_ENCODING_TABLE[i] = c;
        _BASE91_DECODING_TABLE.Add(c, (byte)i);
      }
      #endregion

    }

    #region Quoted Printable
    public static string ToQuotedPrintableString(byte[] data) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(data != null);
#else
      Diagnostics.Debug.Assert(data!=null);
#endif
      StringBuilder result = new();
      foreach (var b in data) {
        var c = _QP_ENCODING_TABLE[b];
        if (c > 0)
          result.Append((char)c);
        else
          result.Append("=" + b.ToString("X2"));
      }
      return (result.ToString());
    }

    public static byte[] FromQuotedPrintableString(string data) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(data != null);
#else
      Diagnostics.Debug.Assert(data!=null);
#endif
      List<byte> result = new();
      for (var i = 0; i < data.Length; i++) {
        var c = data[i];
        if (c == '=') {
          result.Add((byte)(_QP_DECODING_TABLE[data[++i]] * 16 + _QP_DECODING_TABLE[data[++i]]));
        } else {
          result.Add((byte)c);
        }
      }
      return (result.ToArray());
    }
    #endregion

    #region BASE 91
    /// <summary>
    /// Converts a byte array to a base91-encoding string.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>The encoded string.</returns>
    public static string ToBase91String(byte[] data) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(data != null);
#else
      Diagnostics.Debug.Assert(data!=null);
#endif
      StringBuilder result = new();
      int b;
      var n = b = 0;
      foreach (var t in data) {
        b |= t << n;
        n += 8;
        if (n <= 13) continue;
        var v = b & 8191;
        if (v > 88) {
          b >>= 13;
          n -= 13;
        } else {
          v = b & 16383;
          b >>= 14;
          n -= 14;
        }
        result.Append(_BASE91_ENCODING_TABLE[v % 91]);
        result.Append(_BASE91_ENCODING_TABLE[v / 91]);
      }
      // are there still bits left ?
      if (n > 0) {
        result.Append(_BASE91_ENCODING_TABLE[b % 91]);
        if (n > 7 || b > 90)
          result.Append(_BASE91_ENCODING_TABLE[b / 91]);
      }
      return (result.ToString());
    }

    /// <summary>
    /// Converts a base 91 encoded string back to a byte array.
    /// </summary>
    /// <param name="encoded">The encoded string.</param>
    /// <returns>The data.</returns>
    public static byte[] FromBase91String(string encoded) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(encoded != null);
#else
      Diagnostics.Debug.Assert(encoded!=null);
#endif
      List<byte> result = new();
      int b, n;
      var v = (b = n = 0) - 1;
      foreach (var i in encoded) {
        byte c;
        if (!_BASE91_DECODING_TABLE.TryGetValue(i, out c))
          continue;
        if (v < 0) {
          v = c;
          continue;
        }
        v += c * 91;
        b |= v << n;
        n += ((v & 8191) > 88) ? 13 : 14;
        do {
          result.Add((byte)b);
          b >>= 8;
          n -= 8;
        } while (n > 7);
        v = -1;
      }
      if (v + 1 > 0)
        result.Add((byte)(b | v << n));
      return (result.ToArray());
    }
    #endregion

  }
}