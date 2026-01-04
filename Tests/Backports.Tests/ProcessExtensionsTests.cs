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

using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Process")]
public class ProcessExtensionsTests {

  private static ProcessStartInfo _CreateQuickExitStartInfo() => new() {
    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
    Arguments = OperatingSystem.IsWindows() ? "/c exit 0" : "-c \"exit 0\"",
    UseShellExecute = false,
    CreateNoWindow = true
  };

  private static ProcessStartInfo _CreateLongRunningStartInfo() => new() {
    FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "/bin/sh",
    Arguments = OperatingSystem.IsWindows() ? "/c ping -n 10 127.0.0.1" : "-c \"sleep 10\"",
    UseShellExecute = false,
    CreateNoWindow = true
  };

  #region WaitForExitAsync

  [Test]
  [Category("HappyPath")]
  public void WaitForExitAsync_AlreadyExitedProcess_ReturnsCompletedTask() {
    using var process = new Process { StartInfo = _CreateQuickExitStartInfo() };
    process.Start();
    process.WaitForExit();
    var task = process.WaitForExitAsync();
    Assert.That(task.IsCompleted, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Timeout(5000)]
  public void WaitForExitAsync_ShortProcess_Completes() {
    using var process = new Process { StartInfo = _CreateQuickExitStartInfo() };
    process.Start();
    var task = process.WaitForExitAsync();
    task.Wait();
    Assert.That(process.HasExited, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Timeout(5000)]
  public void WaitForExitAsync_WithCancellation_NotCanceled_Completes() {
    using var cts = new CancellationTokenSource();
    using var process = new Process { StartInfo = _CreateQuickExitStartInfo() };
    process.Start();
    var task = process.WaitForExitAsync(cts.Token);
    task.Wait();
    Assert.That(process.HasExited, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void WaitForExitAsync_AlreadyCanceledToken_ReturnsImmediately() {
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    using var process = new Process { StartInfo = _CreateLongRunningStartInfo() };
    process.Start();
    try {
      var task = process.WaitForExitAsync(cts.Token);
      Assert.That(task.IsCanceled, Is.True);
    } finally {
      try { process.Kill(); } catch { }
    }
  }

  #endregion

  #region Kill(bool)

  [Test]
  [Category("HappyPath")]
  [Timeout(5000)]
  public void Kill_WithFalse_KillsProcessOnly() {
    using var process = new Process { StartInfo = _CreateLongRunningStartInfo() };
    process.Start();
    Thread.Sleep(100);
    Assert.That(process.HasExited, Is.False);
    process.Kill(false);
    process.WaitForExit(2000);
    Assert.That(process.HasExited, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  [Timeout(5000)]
  public void Kill_WithTrue_KillsProcess() {
    using var process = new Process { StartInfo = _CreateLongRunningStartInfo() };
    process.Start();
    Thread.Sleep(100);
    Assert.That(process.HasExited, Is.False);
    process.Kill(true);
    process.WaitForExit(2000);
    Assert.That(process.HasExited, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void Kill_AlreadyExitedProcess_DoesNotThrow() {
    // Note: Unlike Kill(), Kill(bool) does NOT throw for already-exited processes.
    // This matches the behavior of the native .NET 5+ Kill(bool) method.
    using var process = new Process { StartInfo = _CreateQuickExitStartInfo() };
    process.Start();
    process.WaitForExit();
    Assert.DoesNotThrow(() => process.Kill(false));
  }

  #endregion

}
