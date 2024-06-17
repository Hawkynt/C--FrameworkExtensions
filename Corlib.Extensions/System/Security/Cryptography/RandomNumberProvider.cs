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

namespace System.Security.Cryptography;

public static partial class RandomNumberGeneratorExtenions {
  /// <summary>
  ///   Gets a new random number.
  /// </summary>
  /// <param name="this">This RandomNumberGenerator.</param>
  /// <param name="maxValue">The maximum exclusive value.</param>
  /// <returns></returns>
  public static int Next(this RandomNumberGenerator @this, int maxValue) {
    var data = new byte[4];
    @this.GetBytes(data);
    var result = BitConverter.ToInt32(data, 0);
    return Math.Abs(result) % maxValue;
  }
}
