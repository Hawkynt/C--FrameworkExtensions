using System;
using System.IO;
using System.Text;
using Corlib.Tests.NUnit;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;
using static TestUtilities;
using LineBreakMode=StringExtensions.LineBreakMode;

internal class FileInfoTest {

  [TestFixture]
  public class FileInProgressTests {
    private const string _TEST_CONTENT = "Test content";
    private const string _NEW_CONTENT = "New content";
    private FileInfo? _sourceFile;
    
    [SetUp]
    public void Setup() {
      // Create a source file for testing
      this._sourceFile = new("test.txt");
      this._sourceFile.WriteAllText(_TEST_CONTENT);
    }

    [Test]
    public void ShouldCreateTemporaryFile() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress();
      Assert.IsTrue(File.Exists(this._sourceFile!.FullName+".$$$"));
    }

    [Test]
    public void ShouldApplyChangesOnDisposeIfNotCancelled() {
      string? tempFileName;
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        tempFileName = this._sourceFile!.FullName + ".$$$";
        fileInProgress.CancelChanges = false;
      }

      // Assuming ReplaceWith correctly replaces the source file with temporary file
      Assert.IsFalse(File.Exists(tempFileName));
      Assert.AreEqual(_NEW_CONTENT,this._sourceFile.ReadAllText());
    }

    [Test]
    public void ShouldNotApplyChangesOnDisposeIfCancelled() {
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        fileInProgress.CancelChanges = true;
      }

      // Assuming ReplaceWith correctly replaces the source file with temporary file
      Assert.AreEqual(_TEST_CONTENT, this._sourceFile.ReadAllText());
    }

    [Test]
    public void ShouldCopyContentsIfRequested() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress(true);
      Assert.AreEqual(_TEST_CONTENT, fileInProgress.ReadAllText());
    }

    [Test]
    public void ShouldNotCopyContentsIfRequested() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress(false);
      Assert.AreEqual(string.Empty, fileInProgress.ReadAllText());
    }

    [TearDown]
    public void TearDown() => this._sourceFile.TryDelete();
  }


  public class ReplaceWithTest {
    private string? _testDirectory;
    private FileInfo? _sourceFile;
    private FileInfo? _destinationFile;
    private FileInfo? _backupFile;

    [SetUp]
    public void Setup() {
      // Create a temporary directory for testing
      this._testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
      Directory.CreateDirectory(this._testDirectory);

      // Setup initial file states for each test
      this._sourceFile = new(Path.Combine(this._testDirectory, "source.txt"));
      File.WriteAllText(this._sourceFile.FullName, "Source file content");

      this._destinationFile = new(Path.Combine(this._testDirectory, "destination.txt"));
      this._backupFile = new(Path.Combine(this._testDirectory, "backup.txt"));
    }

    [TearDown]
    public void TearDown() {
      // Cleanup the test directory after each test
      if (Directory.Exists(this._testDirectory))
        Directory.Delete(this._testDirectory, true);
    }

    [Test]
    public void ReplaceWith_WhenDestinationDoesNotExist_ShouldMoveFile() {
      this._destinationFile.ReplaceWith(this._sourceFile, null, false);
      this._sourceFile!.Refresh();
      this._destinationFile!.Refresh();
      this._backupFile!.Refresh();

      Assert.IsFalse(this._sourceFile.Exists);
      Assert.IsTrue(this._destinationFile.Exists);
      Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
    }

    [Test]
    public void ReplaceWith_WhenDestinationExists_ShouldReplaceFile() {
      File.WriteAllText(this._destinationFile!.FullName, "Original destination content");
      this._destinationFile.ReplaceWith(this._sourceFile, null, false);
      this._sourceFile!.Refresh();
      this._destinationFile.Refresh();
      this._backupFile!.Refresh();

      Assert.IsFalse(this._sourceFile.Exists);
      Assert.IsTrue(this._destinationFile.Exists);
      Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
    }

    [Test]
    public void ReplaceWith_WhenBackupIsProvided_ShouldBackupDestination() {
      File.WriteAllText(this._destinationFile!.FullName, "Original destination content");
      this._destinationFile.ReplaceWith(this._sourceFile, this._backupFile, false);
      this._sourceFile!.Refresh();
      this._destinationFile.Refresh();
      this._backupFile!.Refresh();

      Assert.IsTrue(this._destinationFile.Exists);
      Assert.IsFalse(this._sourceFile.Exists);
      Assert.IsTrue(this._backupFile.Exists);
      Assert.AreEqual("Original destination content", File.ReadAllText(this._backupFile.FullName));
      Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
    }

  }

  private class CustomTextReaderTests {

    public enum TestEncoding {
      AutoDetectFromBom = -1,
      Null=0,
      Utf8NoBOM,
      Utf8,
      UnicodeLittleEndianNoBOM,
      UnicodeBigEndian,
      ASCII,
    }

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, "", typeof(ArgumentException))]
    [TestCase("abc", 1, TestEncoding.ASCII, LineBreakMode.None, "abc")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\ndef", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\ndef\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "abc\r\n")]
    [TestCase("abc\r\ndef\r\n", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "abc\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.AutoDetect, "abc\r\n")]
    [TestCase("abc\rdef\r", 1, TestEncoding.AutoDetectFromBom, LineBreakMode.CarriageReturn, "abc\r")]
    [TestCase("abc\x000cdef", 1, TestEncoding.UnicodeBigEndian, LineBreakMode.FormFeed, "abc\x0c")]
    [TestCase("ab\fc\x0085def", 1, TestEncoding.UnicodeLittleEndianNoBOM, LineBreakMode.NextLine, "ab\fc\x85")]
    [TestCase("ab\u0085c\x0015def", 1, TestEncoding.Utf8, LineBreakMode.NegativeAcknowledge, "ab\u0085c\x15")]
    [TestCase("ab\rc\x2028def", 1, TestEncoding.Utf8NoBOM, LineBreakMode.LineSeparator, "ab\rc\x2028")]
    [TestCase("ab\nc\x2029def", 1, TestEncoding.Utf8, LineBreakMode.ParagraphSeparator, "ab\nc\x2029")]
    [TestCase("ab\nc\x009Bdef", 1, TestEncoding.Utf8, LineBreakMode.EndOfLine, "ab\nc\x9B")]
    [TestCase("ab\nc\x0076def", 1, TestEncoding.Utf8, LineBreakMode.Zx, "ab\nc\x76")]
    [TestCase("ab\nc\0def", 1, TestEncoding.Utf8, LineBreakMode.Null, "ab\nc\0")]
    public void KeepFirstLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null)
      => this._ExecuteTest((f, c, e, l, a) => {
        if (a)
          f.KeepFirstLines(c, l);
        else
          f.KeepFirstLines(c, e, l);
      }, input, count, testEncoding, newLine, expected, exception)
    ;

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, "", typeof(ArgumentException))]
    [TestCase("abc", 1, TestEncoding.ASCII, LineBreakMode.None, "abc")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\ndef", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "def")]
    [TestCase("abc\ndef\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "def\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "def")]
    [TestCase("abc\r\ndef\r\n", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "def\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "def")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.AutoDetect, "def")]
    [TestCase("abc\rdef\r", 1, TestEncoding.AutoDetectFromBom, LineBreakMode.CarriageReturn, "def\r")]
    [TestCase("abc\x000cdef", 1, TestEncoding.UnicodeBigEndian, LineBreakMode.FormFeed, "def")]
    [TestCase("abc\x0085de\ff", 1, TestEncoding.UnicodeLittleEndianNoBOM, LineBreakMode.NextLine, "de\ff")]
    [TestCase("abc\x0015de\u0085f", 1, TestEncoding.Utf8, LineBreakMode.NegativeAcknowledge, "de\u0085f")]
    [TestCase("abc\x2028de\rf", 1, TestEncoding.Utf8NoBOM, LineBreakMode.LineSeparator, "de\rf")]
    [TestCase("abc\x2029de\nf", 1, TestEncoding.Utf8, LineBreakMode.ParagraphSeparator, "de\nf")]
    [TestCase("abc\x009Bde\nf", 1, TestEncoding.Utf8, LineBreakMode.EndOfLine, "de\nf")]
    [TestCase("abc\x0076de\nf", 1, TestEncoding.Utf8, LineBreakMode.Zx, "de\nf")]
    [TestCase("abc\0de\nf", 1, TestEncoding.Utf8, LineBreakMode.Null, "de\nf")]
    public void KeepLastLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null)
      => this._ExecuteTest((f, c, e, l, a) => {
        if (a)
          f.KeepLastLines(c, l);
        else
          f.KeepLastLines(c, e, l);
      }, input, count, testEncoding, newLine, expected, exception)
    ;

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, "",typeof(ArgumentException))]
    [TestCase("abc", 1, TestEncoding.ASCII, LineBreakMode.None, "")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "")]
    [TestCase("abc\ndef", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\ndef\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "abc\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "abc\r\n")]
    [TestCase("abc\r\ndef\r\n", 1, TestEncoding.ASCII, LineBreakMode.CrLf, "abc\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.AutoDetect, "abc\r\n")]
    [TestCase("abc\rdef\r", 1, TestEncoding.AutoDetectFromBom, LineBreakMode.CarriageReturn, "abc\r")]
    [TestCase("abc\x000cdef", 1, TestEncoding.UnicodeBigEndian, LineBreakMode.FormFeed, "abc\x0c")]
    [TestCase("ab\fc\x0085def", 1, TestEncoding.UnicodeLittleEndianNoBOM, LineBreakMode.NextLine, "ab\fc\x85")]
    [TestCase("ab\u0085c\x0015def", 1, TestEncoding.Utf8, LineBreakMode.NegativeAcknowledge, "ab\u0085c\x15")]
    [TestCase("ab\rc\x2028def", 1, TestEncoding.Utf8NoBOM, LineBreakMode.LineSeparator, "ab\rc\x2028")]
    [TestCase("ab\nc\x2029def", 1, TestEncoding.Utf8, LineBreakMode.ParagraphSeparator, "ab\nc\x2029")]
    [TestCase("ab\nc\x009Bdef", 1, TestEncoding.Utf8, LineBreakMode.EndOfLine, "ab\nc\x9B")]
    [TestCase("ab\nc\x0076def", 1, TestEncoding.Utf8, LineBreakMode.Zx, "ab\nc\x76")]
    [TestCase("ab\nc\0def", 1, TestEncoding.Utf8, LineBreakMode.Null, "ab\nc\0")]
    public void RemoveLastLines(string? input,int count, TestEncoding testEncoding, LineBreakMode newLine,string expected, Type? exception=null) 
      => this._ExecuteTest((f, c, e, l, a) => {
        if (a)
          f.RemoveLastLines(c, l);
        else
          f.RemoveLastLines(c, e, l);
      }, input,count,testEncoding,newLine,expected,exception)
      ;

    private void _ExecuteTest(Action<FileInfo?,int,Encoding?,LineBreakMode,bool> runner, string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null) {
      Encoding writeEncoding;
      Encoding? readEncoding;
      switch (testEncoding) {
        case TestEncoding.AutoDetectFromBom:
          readEncoding = null;
          writeEncoding = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
          break;
        case TestEncoding.Utf8NoBOM:
          readEncoding = writeEncoding = new UTF8Encoding(false);
          break;
        case TestEncoding.Utf8:
          readEncoding = writeEncoding = new UTF8Encoding(true);
          break;
        case TestEncoding.UnicodeLittleEndianNoBOM:
          readEncoding = writeEncoding = new UnicodeEncoding(false, false);
          break;
        case TestEncoding.UnicodeBigEndian:
          readEncoding = writeEncoding = new UnicodeEncoding(true, true);
          break;
        case TestEncoding.ASCII:
          readEncoding = writeEncoding = Encoding.ASCII;
          break;
        case TestEncoding.Null:
          readEncoding = null;
          writeEncoding = Encoding.ASCII;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(testEncoding), testEncoding, null);
      }

      FileInfo? file;
      if (input == null) {
        file = null;
        ExecuteTest(() => {
          runner(file, count, readEncoding, newLine,false);
          return file.ReadAllText(writeEncoding);
        }, expected, exception);
      } else {
        using var token = PathExtensions.GetTempFileToken();
        file = token.File;
        file.WriteAllText(input, writeEncoding);
        ExecuteTest(() => {
          runner(file, count, readEncoding, newLine,testEncoding==TestEncoding.AutoDetectFromBom);
          return file.ReadAllText(writeEncoding);
        }, expected, exception);
      }
    }


  }

}