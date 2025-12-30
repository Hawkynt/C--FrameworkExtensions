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

// Polyfill for Assert.ThrowsAsync when NUnit's native implementation isn't available.
// This uses an extension block to add ThrowsAsync to NUnit's Assert class.

using System;
using System.Threading.Tasks;

namespace NUnit.Framework;

/// <summary>
/// Provides Assert.ThrowsAsync polyfill for frameworks where NUnit's native implementation isn't available.
/// </summary>
public static partial class AssertPolyfills {

  extension(Assert) {

    /// <summary>
    /// Asserts that an async delegate throws an exception of the specified type.
    /// </summary>
    /// <typeparam name="TException">The expected exception type.</typeparam>
    /// <param name="asyncAction">The async action to test.</param>
    /// <returns>The caught exception.</returns>
    public static TException ThrowsAsync<TException>(Func<Task> asyncAction) where TException : Exception {
      try {
        var task = asyncAction();
        task.Wait();
        Assert.Fail($"Expected exception of type {typeof(TException).Name} was not thrown.");
        return null!;
      } catch (AggregateException ae) {
        var innerException = ae.InnerException;
        if (innerException is TException expected)
          return expected;

        if (innerException is AggregateException { InnerException: TException innerExpected })
          return innerExpected;

        Assert.Fail($"Expected exception of type {typeof(TException).Name} but got {innerException?.GetType().Name ?? "null"}.");
        return null!;
      } catch (TException ex) {
        return ex;
      } catch (Exception ex) {
        Assert.Fail($"Expected exception of type {typeof(TException).Name} but got {ex.GetType().Name}.");
        return null!;
      }
    }

  }

}
