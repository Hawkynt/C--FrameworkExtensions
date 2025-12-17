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

#if !SUPPORTS_CANCELLATIONTOKENSOURCE_TRYRESET

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading;

public static partial class CancellationTokenSourcePolyfills {

  extension(CancellationTokenSource @this) {

    /// <summary>
    /// Attempts to reset the <see cref="CancellationTokenSource"/> to be used for an unrelated operation.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="CancellationTokenSource"/> has not had cancellation requested and could be reset;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// A <see cref="CancellationTokenSource"/> can only be reset if it has not been canceled.
    /// This polyfill always returns <see langword="false"/> as the .NET Framework does not support resetting.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReset() {
      Against.ThisIsNull(@this);
      // In older .NET versions, CancellationTokenSource cannot be reset
      // Return false to indicate reset is not possible
      // TODO: is there a way to implement this properly?
      return !@this.IsCancellationRequested;
    }

  }

}

#endif
