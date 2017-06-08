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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using qword = System.UInt64;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System {
  internal static partial class RandomExtensions {
    /// <summary>
    /// Creates a random number between the given limits.
    /// </summary>
    /// <param name="This">This Random.</param>
    /// <param name="minValue">The min value.</param>
    /// <param name="maxValue">The max value.</param>
    /// <returns>A value between the given boundaries</returns>
#if NETFX_45
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static double NextDouble(this Random This, double minValue, double maxValue) => This.NextDouble() * (maxValue - minValue) + minValue;

    /// <summary>
    /// Generates a random password.
    /// </summary>
    /// <param name="This">The this.</param>
    /// <param name="minimumLength">The minimum length.</param>
    /// <param name="maximumLength">The maximum length.</param>
    /// <param name="useLetters">if set to <c>true</c> [use letters].</param>
    /// <param name="allowCaseSensitive">if set to <c>true</c> [allow case sensitive].</param>
    /// <param name="allowNumbers">if set to <c>true</c> [allow numbers].</param>
    /// <param name="allowSpecialChars">if set to <c>true</c> [allow special chars].</param>
    /// <param name="allowedCharset">The allowed charset.</param>
    /// <returns></returns>
    public static string GeneratePassword(this Random This, qword minimumLength = 8, qword maximumLength = 14, bool useLetters = true, bool allowCaseSensitive = true, bool allowNumbers = true, bool allowSpecialChars = true, string allowedCharset = null) {
      Contract.Requires(This != null);

      // generate a length
      if (maximumLength < minimumLength)
        maximumLength = minimumLength;

      var length =
        maximumLength == minimumLength
        ? minimumLength
        : (qword)This.Next((int)(maximumLength - minimumLength)) + minimumLength
        ;

      // find out which chars are allowed
      var allowedChars = string.Empty;
      if (string.IsNullOrEmpty(allowedCharset)) {
        if (useLetters)
          allowedChars += allowCaseSensitive ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        if (allowNumbers)
          allowedChars += "0123456789";

        if (allowSpecialChars)
          allowedChars += @"!""$%&/()=?{[]}\#+*~-_.:,;<>@";

      } else {
        allowedChars = allowedCharset;
      }

      // build a password
      var result = new StringBuilder((int)length);
      var charCount = allowedChars.Length;
      for (var i = length; i > 0; --i) {
        var index = This.Next(charCount);
        Contract.Assume(index < charCount);
        result.Append(allowedChars[index]);
      }

      return (result.ToString());
    }
  }
}
