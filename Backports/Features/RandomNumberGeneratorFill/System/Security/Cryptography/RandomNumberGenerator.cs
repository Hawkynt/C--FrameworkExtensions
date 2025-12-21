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

#if !SUPPORTS_RNG_FILL

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Security.Cryptography;

public static partial class RandomNumberGeneratorPolyfills {
  extension(RandomNumberGenerator) {
    /// <summary>
    /// Fills a span with cryptographically strong random bytes.
    /// </summary>
    /// <param name="data">The span to fill with cryptographically strong random bytes.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(Span<byte> data) {
      if (data.IsEmpty)
        return;

      var array = new byte[data.Length];
#if NEEDS_RNG_DISPOSE
      using var rng = new RNGCryptoServiceProvider();
      rng.GetBytes(array);
#else
      new RNGCryptoServiceProvider().GetBytes(array);
#endif
      for (var i = 0; i < array.Length; ++i)
        data[i] = array[i];
    }
  }
}

#endif
