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
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Environment")]
public class EnvironmentTests {

  #region Environment.TickCount64

  [Test]
  [Category("HappyPath")]
  public void TickCount64_ReturnsPositiveValue() {
    var ticks = Environment.TickCount64;
    Assert.That(ticks, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void TickCount64_Increases_OverTime() {
    var ticks1 = Environment.TickCount64;
    Thread.Sleep(50);
    var ticks2 = Environment.TickCount64;
    Assert.That(ticks2, Is.GreaterThan(ticks1));
  }

  [Test]
  [Category("HappyPath")]
  public void TickCount64_ReturnsMilliseconds() {
    var ticks1 = Environment.TickCount64;
    Thread.Sleep(100);
    var ticks2 = Environment.TickCount64;
    var elapsed = ticks2 - ticks1;
    Assert.That(elapsed, Is.GreaterThanOrEqualTo(80));
    Assert.That(elapsed, Is.LessThan(300));
  }

  #endregion

  #region Environment.ProcessId

  [Test]
  [Category("HappyPath")]
  public void ProcessId_ReturnsPositiveValue() {
    var id = Environment.ProcessId;
    Assert.That(id, Is.GreaterThan(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ProcessId_ReturnsConsistentValue() {
    var id1 = Environment.ProcessId;
    var id2 = Environment.ProcessId;
    Assert.That(id1, Is.EqualTo(id2));
  }

  [Test]
  [Category("HappyPath")]
  public void ProcessId_MatchesDiagnostics() {
    using var process = System.Diagnostics.Process.GetCurrentProcess();
    var expected = process.Id;
    var actual = Environment.ProcessId;
    Assert.That(actual, Is.EqualTo(expected));
  }

  #endregion

  #region Environment.ProcessPath

  [Test]
  [Category("HappyPath")]
  public void ProcessPath_ReturnsNonEmptyString() {
    var path = Environment.ProcessPath;
    Assert.That(path, Is.Not.Null.And.Not.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void ProcessPath_ReturnsExistingFile() {
    var path = Environment.ProcessPath;
    Assert.That(System.IO.File.Exists(path), Is.True, $"Path does not exist: {path}");
  }

  [Test]
  [Category("HappyPath")]
  public void ProcessPath_ReturnsConsistentValue() {
    var path1 = Environment.ProcessPath;
    var path2 = Environment.ProcessPath;
    Assert.That(path1, Is.EqualTo(path2));
  }

  #endregion

}
