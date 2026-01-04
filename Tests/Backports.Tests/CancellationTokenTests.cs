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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("CancellationToken")]
public class CancellationTokenTests {

  #region CancellationTokenSource.TryReset

  [Test]
  [Category("HappyPath")]
  public void TryReset_NotCanceled_ReturnsTrue() {
    using var cts = new CancellationTokenSource();
    var result = cts.TryReset();
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TryReset_AfterCancel_ReturnsFalse() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var result = cts.TryReset();
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void TryReset_NotCanceled_TokenRemainsValid() {
    using var cts = new CancellationTokenSource();
    var token = cts.Token;
    cts.TryReset();
    Assert.That(token.IsCancellationRequested, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void TryReset_MultipleCalls_ReturnsTrueIfNotCanceled() {
    using var cts = new CancellationTokenSource();
    Assert.That(cts.TryReset(), Is.True);
    Assert.That(cts.TryReset(), Is.True);
    Assert.That(cts.TryReset(), Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void TryReset_AfterCancel_MultipleCalls_ReturnsFalse() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    Assert.That(cts.TryReset(), Is.False);
    Assert.That(cts.TryReset(), Is.False);
  }

  #endregion

  #region CancellationTokenSource.CancelAsync

  [Test]
  [Category("HappyPath")]
  public void CancelAsync_NotCanceled_CancelsToken() {
    using var cts = new CancellationTokenSource();
    var task = cts.CancelAsync();
    task.Wait();
    Assert.That(cts.IsCancellationRequested, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CancelAsync_ReturnsCompletedTask() {
    using var cts = new CancellationTokenSource();
    var task = cts.CancelAsync();
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CancelAsync_TokenIsCancellationRequested_IsTrue() {
    using var cts = new CancellationTokenSource();
    var token = cts.Token;
    cts.CancelAsync().Wait();
    Assert.That(token.IsCancellationRequested, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void CancelAsync_AlreadyCanceled_Succeeds() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    var task = cts.CancelAsync();
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CancelAsync_CallbackIsInvoked() {
    using var cts = new CancellationTokenSource();
    var callbackInvoked = false;
    cts.Token.Register(() => callbackInvoked = true);
    cts.CancelAsync().Wait();
    Assert.That(callbackInvoked, Is.True);
  }

  #endregion

}
