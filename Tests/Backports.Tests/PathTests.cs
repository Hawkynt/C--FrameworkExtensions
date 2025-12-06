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
using System.IO;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Path")]
public class PathTests {

  #region Path.Join

  [Test]
  [Category("HappyPath")]
  public void PathJoin_TwoPaths_JoinsThem() {
    var result = Path.Join("folder", "file.txt");
    var expected = "folder" + Path.DirectorySeparatorChar + "file.txt";
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_FirstPathEndsWithSeparator_DoesNotAddExtra() {
    var result = Path.Join("folder" + Path.DirectorySeparatorChar, "file.txt");
    var expected = "folder" + Path.DirectorySeparatorChar + "file.txt";
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_EmptyFirstPath_ReturnsSecondPath() {
    var result = Path.Join("", "file.txt");
    Assert.That(result, Is.EqualTo("file.txt"));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_EmptySecondPath_ReturnsFirstPath() {
    var result = Path.Join("folder", "");
    Assert.That(result, Is.EqualTo("folder"));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_NullFirstPath_ReturnsSecondPath() {
    var result = Path.Join(null, "file.txt");
    Assert.That(result, Is.EqualTo("file.txt"));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_ThreePaths_JoinsThem() {
    var result = Path.Join("a", "b", "c");
    var expected = "a" + Path.DirectorySeparatorChar + "b" + Path.DirectorySeparatorChar + "c";
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_FourPaths_JoinsThem() {
    var result = Path.Join("a", "b", "c", "d");
    var expected = "a" + Path.DirectorySeparatorChar + "b" + Path.DirectorySeparatorChar + "c" + Path.DirectorySeparatorChar + "d";
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void PathJoin_ParamsArray_JoinsThem() {
    var result = Path.Join(new[] { "a", "b", "c", "d", "e" });
    var sep = Path.DirectorySeparatorChar.ToString();
    Assert.That(result, Is.EqualTo("a" + sep + "b" + sep + "c" + sep + "d" + sep + "e"));
  }

  [Test]
  [Category("EdgeCase")]
  public void PathJoin_EmptyArray_ReturnsEmpty() {
    var result = Path.Join(new string[0]);
    Assert.That(result, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("Exception")]
  public void PathJoin_NullArray_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Path.Join((string[])null));
  }

  #endregion

  #region Path.GetRelativePath

  [Test]
  [Category("HappyPath")]
  public void GetRelativePath_SubDirectory_ReturnsRelativePath() {
    var relativeTo = Path.Combine(Path.GetTempPath(), "base");
    var path = Path.Combine(Path.Combine(Path.Combine(Path.GetTempPath(), "base"), "sub"), "file.txt");
    var result = Path.GetRelativePath(relativeTo, path);
    var expected = "sub" + Path.DirectorySeparatorChar + "file.txt";
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void GetRelativePath_ParentDirectory_ReturnsParentNotation() {
    var relativeTo = Path.Combine(Path.Combine(Path.GetTempPath(), "base"), "sub");
    var path = Path.Combine(Path.Combine(Path.GetTempPath(), "base"), "file.txt");
    var result = Path.GetRelativePath(relativeTo, path);
    Assert.That(result, Does.Contain(".."));
  }

  [Test]
  [Category("HappyPath")]
  public void GetRelativePath_SamePath_ReturnsDot() {
    var path = Path.Combine(Path.GetTempPath(), "base");
    var result = Path.GetRelativePath(path, path);
    Assert.That(result, Is.EqualTo("."));
  }

  [Test]
  [Category("Exception")]
  public void GetRelativePath_NullRelativeTo_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Path.GetRelativePath(null, "path"));
  }

  [Test]
  [Category("Exception")]
  public void GetRelativePath_NullPath_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => Path.GetRelativePath("base", null));
  }

  [Test]
  [Category("Exception")]
  public void GetRelativePath_EmptyRelativeTo_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Path.GetRelativePath("", "path"));
  }

  [Test]
  [Category("Exception")]
  public void GetRelativePath_EmptyPath_ThrowsArgumentException() {
    Assert.Throws<ArgumentException>(() => Path.GetRelativePath("base", ""));
  }

  #endregion

}
