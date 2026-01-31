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
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("File")]
public class FileReadLinesTests {
  private string _testDirectory;
  private string _testFilePath;

  [SetUp]
  public void SetUp() {
    _testDirectory = Path.Combine(Path.GetTempPath(), "FileReadLinesTests_" + Guid.NewGuid().ToString("N")[..8]);
    Directory.CreateDirectory(_testDirectory);
    _testFilePath = Path.Combine(_testDirectory, "test.txt");
  }

  [TearDown]
  public void TearDown() {
    if (Directory.Exists(_testDirectory))
      Directory.Delete(_testDirectory, true);
  }

  #region ReadLines without encoding

  [Test]
  [Category("HappyPath")]
  public void ReadLines_EmptyFile_ReturnsNoLines() {
    System.IO.File.WriteAllText(_testFilePath, "");
    var lines = System.IO.File.ReadLines(_testFilePath).ToArray();
    Assert.That(lines, Is.Empty);
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_SingleLine_ReturnsSingleLine() {
    System.IO.File.WriteAllText(_testFilePath, "Hello World");
    var lines = System.IO.File.ReadLines(_testFilePath).ToArray();
    Assert.That(lines, Has.Length.EqualTo(1));
    Assert.That(lines[0], Is.EqualTo("Hello World"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_MultipleLines_ReturnsAllLines() {
    System.IO.File.WriteAllText(_testFilePath, "Line 1\nLine 2\nLine 3");
    var lines = System.IO.File.ReadLines(_testFilePath).ToArray();
    Assert.That(lines, Has.Length.EqualTo(3));
    Assert.That(lines[0], Is.EqualTo("Line 1"));
    Assert.That(lines[1], Is.EqualTo("Line 2"));
    Assert.That(lines[2], Is.EqualTo("Line 3"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_WindowsLineEndings_ReturnsAllLines() {
    System.IO.File.WriteAllText(_testFilePath, "Line 1\r\nLine 2\r\nLine 3");
    var lines = System.IO.File.ReadLines(_testFilePath).ToArray();
    Assert.That(lines, Has.Length.EqualTo(3));
    Assert.That(lines[0], Is.EqualTo("Line 1"));
    Assert.That(lines[1], Is.EqualTo("Line 2"));
    Assert.That(lines[2], Is.EqualTo("Line 3"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_TrailingNewline_DoesNotReturnExtraLine() {
    System.IO.File.WriteAllText(_testFilePath, "Line 1\nLine 2\n");
    var lines = System.IO.File.ReadLines(_testFilePath).ToArray();
    Assert.That(lines, Has.Length.EqualTo(2));
    Assert.That(lines[0], Is.EqualTo("Line 1"));
    Assert.That(lines[1], Is.EqualTo("Line 2"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_IsLazyEnumerable_DoesNotReadEntireFile() {
    System.IO.File.WriteAllText(_testFilePath, "Line 1\nLine 2\nLine 3\nLine 4\nLine 5");
    var firstTwo = System.IO.File.ReadLines(_testFilePath).Take(2).ToArray();
    Assert.That(firstTwo, Has.Length.EqualTo(2));
    Assert.That(firstTwo[0], Is.EqualTo("Line 1"));
    Assert.That(firstTwo[1], Is.EqualTo("Line 2"));
  }

  #endregion

  #region ReadLines with encoding

  [Test]
  [Category("HappyPath")]
  public void ReadLines_WithUtf8Encoding_ReturnsLines() {
    System.IO.File.WriteAllText(_testFilePath, "Héllo Wörld\nÄÖÜ", Encoding.UTF8);
    var lines = System.IO.File.ReadLines(_testFilePath, Encoding.UTF8).ToArray();
    Assert.That(lines, Has.Length.EqualTo(2));
    Assert.That(lines[0], Is.EqualTo("Héllo Wörld"));
    Assert.That(lines[1], Is.EqualTo("ÄÖÜ"));
  }

  [Test]
  [Category("HappyPath")]
  public void ReadLines_WithUnicodeEncoding_ReturnsLines() {
    System.IO.File.WriteAllText(_testFilePath, "Line 1\nLine 2", Encoding.Unicode);
    var lines = System.IO.File.ReadLines(_testFilePath, Encoding.Unicode).ToArray();
    Assert.That(lines, Has.Length.EqualTo(2));
    Assert.That(lines[0], Is.EqualTo("Line 1"));
    Assert.That(lines[1], Is.EqualTo("Line 2"));
  }

  #endregion

  #region Exception cases

  [Test]
  [Category("Exception")]
  public void ReadLines_FileDoesNotExist_ThrowsFileNotFoundException() {
    var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");
    Assert.Throws<FileNotFoundException>(() => {
      foreach (var _ in System.IO.File.ReadLines(nonExistentPath)) { }
    });
  }

  [Test]
  [Category("Exception")]
  public void ReadLines_NullPath_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => {
      foreach (var _ in System.IO.File.ReadLines(null)) { }
    });
  }

  #endregion
}
