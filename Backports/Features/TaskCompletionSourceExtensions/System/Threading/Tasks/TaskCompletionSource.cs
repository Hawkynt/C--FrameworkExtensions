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

// TrySetCanceled(CancellationToken) was added in .NET 4.6
#if !SUPPORTS_TASKCOMPLETIONSOURCE_TRYSETCANCELED_TOKEN

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Threading.Tasks;

public static partial class TaskCompletionSourcePolyfills {

  extension<TResult>(TaskCompletionSource<TResult> @this) {

    /// <summary>
    /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Canceled"/> state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token with which to cancel the <see cref="Task{TResult}"/>.</param>
    /// <returns><see langword="true"/> if the operation was successful; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetCanceled(CancellationToken cancellationToken) {
      Against.ThisIsNull(@this);

      // On older frameworks, just delegate to the parameterless version
      // The cancellation token is not actually used in the underlying implementation
      return @this.TrySetCanceled();
    }

  }

}

#endif
