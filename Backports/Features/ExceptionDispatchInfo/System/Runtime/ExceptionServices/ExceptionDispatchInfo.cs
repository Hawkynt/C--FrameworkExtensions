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

// ExceptionDispatchInfo was added in .NET Framework 4.5
// Allows capturing and re-throwing exceptions while preserving stack trace
// On older frameworks, we provide a polyfill implementation
using System;
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.ExceptionServices;

#if !SUPPORTS_EXCEPTION_DISPATCH_INFO

#nullable disable

// Use a different internal namespace to avoid CLR namespace conflicts
// The type is aliased back to System.Runtime.ExceptionServices via global using
internal sealed class ExceptionDispatchInfoCore(Exception exception) {
  public Exception SourceException { get; } = exception ?? throw new ArgumentNullException(nameof(exception));
  public void Throw() => throw this.SourceException;
}

public sealed class ExceptionDispatchInfo {
  private readonly ExceptionDispatchInfoCore _core;

  public Exception SourceException => this._core.SourceException;

  private ExceptionDispatchInfo(Exception exception) => this._core = new(exception);

  public static ExceptionDispatchInfo Capture(Exception source) => new(source);

  public void Throw() => this._core.Throw();
}

#endif

// ExceptionDispatchInfo.Throw(Exception) static method was added in .NET Core 2.1
// This polyfills it for frameworks where ExceptionDispatchInfo exists but static Throw doesn't
#if !SUPPORTS_EXCEPTION_DISPATCH_INFO_THROW_STATIC

public static class ExceptionDispatchInfoPolyfills {

  extension(ExceptionDispatchInfo) {

    /// <summary>
    /// Throws the source exception, maintaining the original stack trace information.
    /// </summary>
    /// <param name="source">The exception whose state is captured, then thrown.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    [DoesNotReturn]
    public static void Throw(Exception source) {
      ArgumentNullException.ThrowIfNull(source);
      ExceptionDispatchInfo.Capture(source).Throw();
    }

  }

}

#endif
