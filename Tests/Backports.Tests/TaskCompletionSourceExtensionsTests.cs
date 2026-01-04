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

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("TaskCompletionSource")]
public class TaskCompletionSourceExtensionsTests {

  #region TrySetCanceled(CancellationToken)

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_WithToken_ReturnsTrue() {
    var tcs = new TaskCompletionSource<int>();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = tcs.TrySetCanceled(cts.Token);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_WithToken_TaskIsCanceled() {
    var tcs = new TaskCompletionSource<int>();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    tcs.TrySetCanceled(cts.Token);
    Assert.That(tcs.Task.IsCanceled, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_WithDefaultToken_ReturnsTrue() {
    var tcs = new TaskCompletionSource<int>();
    var result = tcs.TrySetCanceled(default);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_WithDefaultToken_TaskIsCanceled() {
    var tcs = new TaskCompletionSource<int>();
    tcs.TrySetCanceled(default);
    Assert.That(tcs.Task.IsCanceled, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void TrySetCanceled_AlreadyCompleted_ReturnsFalse() {
    var tcs = new TaskCompletionSource<int>();
    tcs.SetResult(42);
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = tcs.TrySetCanceled(cts.Token);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void TrySetCanceled_AlreadyCanceled_ReturnsFalse() {
    var tcs = new TaskCompletionSource<int>();
    tcs.TrySetCanceled();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = tcs.TrySetCanceled(cts.Token);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void TrySetCanceled_MultipleCalls_OnlyFirstSucceeds() {
    var tcs = new TaskCompletionSource<int>();
    using var cts1 = new CancellationTokenSource();
    using var cts2 = new CancellationTokenSource();
    cts1.Cancel();
    cts2.Cancel();
    var result1 = tcs.TrySetCanceled(cts1.Token);
    var result2 = tcs.TrySetCanceled(cts2.Token);
    Assert.That(result1, Is.True);
    Assert.That(result2, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_StringType_Works() {
    var tcs = new TaskCompletionSource<string>();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = tcs.TrySetCanceled(cts.Token);
    Assert.That(result, Is.True);
    Assert.That(tcs.Task.IsCanceled, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TrySetCanceled_ObjectType_Works() {
    var tcs = new TaskCompletionSource<object>();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = tcs.TrySetCanceled(cts.Token);
    Assert.That(result, Is.True);
    Assert.That(tcs.Task.IsCanceled, Is.True);
  }

  #endregion

}
