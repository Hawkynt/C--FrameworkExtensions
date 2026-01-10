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

// HttpContent.ReadAsStream was added in .NET 5.0
// System.Net.Http is built-in starting from .NET Core (needs package on .NET Framework)
#if !SUPPORTS_HTTPCONTENT_READASSTREAM && NETCOREAPP

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Net.Http;

public static partial class HttpContentPolyfills {

  extension(HttpContent @this) {

    /// <summary>
    /// Serializes the HTTP content and returns a stream that represents the content.
    /// </summary>
    /// <returns>The stream that represents the HTTP content.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stream ReadAsStream()
      => @this.ReadAsStreamAsync().GetAwaiter().GetResult();

    /// <summary>
    /// Serializes the HTTP content and returns a stream that represents the content.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The stream that represents the HTTP content.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Stream ReadAsStream(CancellationToken cancellationToken) {
      cancellationToken.ThrowIfCancellationRequested();
      return @this.ReadAsStreamAsync().GetAwaiter().GetResult();
    }

  }

}

#endif
