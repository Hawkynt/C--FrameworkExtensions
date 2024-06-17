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

using System.Collections.Generic;
using Guard;

namespace System.Net;

public static partial class WebHeaderCollectionExntenions {
  /// <summary>
  ///   Adds a bunch of headers.
  /// </summary>
  /// <param name="this">This WebHeaderCollection.</param>
  /// <param name="headers">The headers.</param>
  public static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<string, string>> headers) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(headers);

    foreach (var kvp in headers)
      @this.Add(kvp.Key, kvp.Value);
  }

  /// <summary>
  ///   Adds a bunch of headers.
  /// </summary>
  /// <param name="this">This WebHeaderCollection.</param>
  /// <param name="headers">The headers.</param>
  public static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(headers);

    foreach (var kvp in headers)
      @this.Add(kvp.Key, kvp.Value);
  }

  /// <summary>
  ///   Adds a bunch of headers.
  /// </summary>
  /// <param name="this">This WebHeaderCollection.</param>
  /// <param name="headers">The headers.</param>
  public static void AddRange(this WebHeaderCollection @this, IEnumerable<KeyValuePair<HttpResponseHeader, string>> headers) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(headers);

    foreach (var kvp in headers)
      @this.Add(kvp.Key, kvp.Value);
  }
}
