using System.Text;
using System.Threading;
using Corlib.Tests.NUnit;
using NUnit.Framework;

namespace System.IO;

using static TestUtilities;
using LineBreakMode = StringExtensions.LineBreakMode;

[TestFixture]
internal class FileInfoTest {
  [TestFixture]
  public class FileInProgressTests {
    private const string _TEST_CONTENT = "Test content";
    private const string _NEW_CONTENT = "New content";
    private FileInfo? _sourceFile;
    private string? _tempFilePattern;

    [SetUp]
    public void Setup() {
      // Use unique filename to avoid conflicts between parallel tests
      var uniqueName = $"test_{Guid.NewGuid():N}.txt";
      this._sourceFile = new(uniqueName);
      this._tempFilePattern = this._sourceFile.FullName + ".$$$";

      // Clean up any leftover files from previous runs
      _CleanupTempFiles();

      // Create a source file for testing
      this._sourceFile.WriteAllText(_TEST_CONTENT);
    }

    [Test]
    public void ShouldCreateTemporaryFile() {
      using var fileInProgress = this._sourceFile.StartWorkInProgress();
      Assert.IsTrue(File.Exists(this._tempFilePattern));
    }

    [Test]
    public void ShouldApplyChangesOnDisposeIfNotCancelled() {
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        fileInProgress.CancelChanges = false;
      }

      // Temp file should be gone after successful replace
      Assert.IsFalse(File.Exists(this._tempFilePattern));
      Assert.AreEqual(_NEW_CONTENT, this._sourceFile.ReadAllText());
    }

    [Test]
    public void ShouldNotApplyChangesOnDisposeIfCancelled() {
      using (var fileInProgress = this._sourceFile.StartWorkInProgress()) {
        fileInProgress.WriteAllText(_NEW_CONTENT);
        fileInProgress.CancelChanges = true;
      }

      // Original content should remain unchanged
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
    public void TearDown() {
      this._sourceFile.TryDelete();
      _CleanupTempFiles();
    }

    private void _CleanupTempFiles() {
      // Clean up temp file and any numbered variants
      if (this._tempFilePattern == null)
        return;

      try {
        if (File.Exists(this._tempFilePattern))
          File.Delete(this._tempFilePattern);
      } catch {
        // Ignore cleanup errors
      }
    }
  }

  [TestFixture]
  public class IsTextFileTests {
    [Test]
    public void NullFile_ThrowsNullReferenceException() {
      FileInfo? fileInfo = null;
      Assert.Throws<NullReferenceException>(() => fileInfo.IsTextFile());
    }

    [Test]
    public void NonExistentFile_ReturnsFalse() {
      var fileInfo = new FileInfo("nonexistentfile.txt");
      Assert.IsFalse(fileInfo.IsTextFile());
    }

    [Test]
    public void EmptyFile_ReturnsFalse() {
      var tempFile = Path.GetTempFileName();
      try {
        var fileInfo = new FileInfo(tempFile);
        Assert.IsFalse(fileInfo.IsTextFile());
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    [TestCase(0x00, false, TestName = "Single Control Character is no Textfile")]
    [TestCase(0x65, true, TestName = "Single Letter Character is Textfile")]
    [TestCase(0x0A, true, TestName = "Single Whitespace Character is Textfile")]
    public void SingleCharFile(byte character, bool expected) {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [character]);
        var fileInfo = new FileInfo(tempFile);
        Assert.That(fileInfo.IsTextFile(), Is.EqualTo(expected));
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    [TestCase(0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x65, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x65, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x0D, 0x0A, true, TestName = "Whitespace Characters are a Textfile")]
    [TestCase(0x65, 0x65, true, TestName = "Letter Characters are a Textfile")]
    [TestCase(0xff, 0xfe, true, TestName = "UTF-16 LE BOM is a Textfile")]
    [TestCase(0xfe, 0xff, true, TestName = "UTF-16 BE BOM is a Textfile")]
    public void DualCharacterFile(byte b0, byte b1, bool expected) {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [b0, b1]);
        var fileInfo = new FileInfo(tempFile);
        Assert.That(fileInfo.IsTextFile(), Is.EqualTo(expected));
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    [TestCase(0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x65, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x65, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x65, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x0D, 0x0A, 0x09, true, TestName = "Whitespace Characters are a Textfile")]
    [TestCase(0x65, 0x65, 0x65, true, TestName = "Letter Characters are a Textfile")]
    [TestCase(0xff, 0xfe, 0x00, true, TestName = "UTF-16 LE BOM is a Textfile")]
    [TestCase(0xfe, 0xff, 0x00, true, TestName = "UTF-16 BE BOM is a Textfile")]
    [TestCase(0x2b, 0x2f, 0x76, true, TestName = "UTF-7 BOM is a Textfile")]
    [TestCase(0xef, 0xbb, 0xbf, true, TestName = "UTF-8 BOM is a Textfile")]
    public void TripleCharacterFile(byte b0, byte b1, byte b2, bool expected) {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [b0, b1, b2]);
        var fileInfo = new FileInfo(tempFile);
        Assert.That(fileInfo.IsTextFile(), Is.EqualTo(expected));
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    [TestCase(0x00, 0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x65, 0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x65, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x65, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x00, 0x65, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x0D, 0x0A, 0x09, 0x0C, true, TestName = "Whitespace Characters are a Textfile")]
    [TestCase(0x65, 0x65, 0x65, 0x65, true, TestName = "Letter Characters are a Textfile")]
    [TestCase(0xff, 0xfe, 0x00, 0x00, true, TestName = "UTF-16 LE BOM is a Textfile")]
    [TestCase(0xfe, 0xff, 0x00, 0x00, true, TestName = "UTF-16 BE BOM is a Textfile")]
    [TestCase(0x2b, 0x2f, 0x76, 0x00, true, TestName = "UTF-7 BOM is a Textfile")]
    [TestCase(0xef, 0xbb, 0xbf, 0x00, true, TestName = "UTF-8 BOM is a Textfile")]
    [TestCase(0xff, 0xfe, 0x00, 0x00, true, TestName = "UTF-32 LE BOM is a Textfile")]
    [TestCase(0x00, 0x00, 0xfe, 0xff, true, TestName = "UTF-32 BE BOM is a Textfile")]
    public void QuadrupleCharacterFile(byte b0, byte b1, byte b2, byte b3, bool expected) {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [b0, b1, b2, b3]);
        var fileInfo = new FileInfo(tempFile);
        Assert.That(fileInfo.IsTextFile(), Is.EqualTo(expected));
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    [TestCase(0x00, 0x00, 0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x65, 0x00, 0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x65, 0x00, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x65, 0x00, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x00, 0x65, 0x00, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x00, 0x00, 0x00, 0x00, 0x65, false, TestName = "Control Characters are no Textfile")]
    [TestCase(0x0D, 0x0A, 0x09, 0x0C, 0x20, true, TestName = "Whitespace Characters are a Textfile")]
    [TestCase(0x65, 0x65, 0x65, 0x65, 0x65, true, TestName = "Letter Characters are a Textfile")]
    [TestCase(0xff, 0xfe, 0x00, 0x00, 0x00, true, TestName = "UTF-16 LE BOM is a Textfile")]
    [TestCase(0xfe, 0xff, 0x00, 0x00, 0x00, true, TestName = "UTF-16 BE BOM is a Textfile")]
    [TestCase(0x2b, 0x2f, 0x76, 0x00, 0x00, true, TestName = "UTF-7 BOM is a Textfile")]
    [TestCase(0xef, 0xbb, 0xbf, 0x00, 0x00, true, TestName = "UTF-8 BOM is a Textfile")]
    [TestCase(0xff, 0xfe, 0x00, 0x00, 0x00, true, TestName = "UTF-32 LE BOM is a Textfile")]
    [TestCase(0x00, 0x00, 0xfe, 0xff, 0x00, true, TestName = "UTF-32 BE BOM is a Textfile")]
    public void QuintupleCharacterFile(byte b0, byte b1, byte b2, byte b3, byte b4, bool expected) {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [b0, b1, b2, b3, b4]);
        var fileInfo = new FileInfo(tempFile);
        Assert.That(fileInfo.IsTextFile(), Is.EqualTo(expected));
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    public void IsTextFile_TextContentFile_ReturnsTrue() {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllText(tempFile, "This is a text file.");
        var fileInfo = new FileInfo(tempFile);
        Assert.IsTrue(fileInfo.IsTextFile());
      } finally {
        File.Delete(tempFile);
      }
    }

    [Test]
    public void IsTextFile_BinaryContentFile_ReturnsFalse() {
      var tempFile = Path.GetTempFileName();
      try {
        File.WriteAllBytes(tempFile, [0x00, 0x01, 0x02, 0x03, 0x04]);
        var fileInfo = new FileInfo(tempFile);
        Assert.IsFalse(fileInfo.IsTextFile());
      } finally {
        File.Delete(tempFile);
      }
    }
  }

  [TestFixture]
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
      WriteFileWithFlush(this._sourceFile.FullName, "Source file content");

      this._destinationFile = new(Path.Combine(this._testDirectory, "destination.txt"));
      this._backupFile = new(Path.Combine(this._testDirectory, "backup.txt"));
    }

    // Helper to ensure proper file handle release after writing
    private static void WriteFileWithFlush(string path, string content) {
      using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
      using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
        writer.Write(content);
        writer.Flush();
        stream.Flush();
      }

      // Give Windows time to fully release file handles
      GC.Collect();
      GC.WaitForPendingFinalizers();
    }

    // Helper to perform file operations with retry logic for transient Windows file system issues
    private static void ExecuteWithRetry(Action action, int maxRetries = 3) {
      for (var i = 0; i < maxRetries; ++i)
        try {
          action();
          return;
        } catch (IOException) when (i < maxRetries - 1) {
          Thread.Sleep(100);
          GC.Collect();
          GC.WaitForPendingFinalizers();
        }
    }

    [TearDown]
    public void TearDown() {
      // Cleanup the test directory after each test
      var testDirectory = this._testDirectory;
      if (testDirectory != null && Directory.Exists(testDirectory))
        Directory.Delete(testDirectory, true);
    }

    [Test]
    public void ReplaceWith_WhenDestinationDoesNotExist_ShouldMoveFile() {
      ExecuteWithRetry(() => this._destinationFile.ReplaceWith(this._sourceFile, null, false));
      this._sourceFile!.Refresh();
      this._destinationFile!.Refresh();
      this._backupFile!.Refresh();

      Assert.IsFalse(this._sourceFile.Exists);
      Assert.IsTrue(this._destinationFile.Exists);
      Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
    }

    [Test]
    public void ReplaceWith_WhenDestinationExists_ShouldReplaceFile() {
      WriteFileWithFlush(this._destinationFile!.FullName, "Original destination content");
      ExecuteWithRetry(() => this._destinationFile.ReplaceWith(this._sourceFile, null, false));
      this._sourceFile!.Refresh();
      this._destinationFile.Refresh();
      this._backupFile!.Refresh();

      Assert.IsFalse(this._sourceFile.Exists);
      Assert.IsTrue(this._destinationFile.Exists);
      Assert.AreEqual("Source file content", File.ReadAllText(this._destinationFile.FullName));
    }

    [Test]
    public void ReplaceWith_WhenBackupIsProvided_ShouldBackupDestination() {
      WriteFileWithFlush(this._destinationFile!.FullName, "Original destination content");
      ExecuteWithRetry(() => this._destinationFile.ReplaceWith(this._sourceFile, this._backupFile, false));
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

  [TestFixture]
  public class CustomTextReaderTests {
    public enum TestEncoding {
      AutoDetectFromBom = -1,
      Null = 0,
      Utf8NoBOM,
      Utf8,
      UnicodeLittleEndianNoBOM,
      UnicodeBigEndian,
      ASCII
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
    // Mixed line endings with LineBreakMode.All - exotic test cases
    // Both CRLF (\r\n) and LFCR (\n\r) are treated as SINGLE line endings (greedy 2-char match)
    // Individual \r and \n are also single line endings when not part of a 2-char sequence
    [TestCase("abc\r\ndef\nghi\rjkl", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n", TestName = "KeepFirstLines_MixedCrLfLfCr_Keep1_ReturnsCrLfTerminated")]
    [TestCase("abc\r\ndef\nghi\rjkl", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r\ndef\n", TestName = "KeepFirstLines_MixedCrLfLfCr_Keep2_ReturnsTwoLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 3, TestEncoding.ASCII, LineBreakMode.All, "abc\r\ndef\nghi\r", TestName = "KeepFirstLines_MixedCrLfLfCr_Keep3_ReturnsThreeLines")]
    // LFCR (\n\r) is ONE line break (greedy match) - same as CRLF
    // Input "abc\n\rdef" has 2 lines: "abc" and "def"
    [TestCase("abc\n\rdef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\n\r", TestName = "KeepFirstLines_LfCrAsOneBreak_Keep1_IncludesLfCrEnding")]
    [TestCase("abc\n\rdef", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\n\rdef", TestName = "KeepFirstLines_LfCrAsOneBreak_Keep2_ReturnsAll")]
    // CRLF followed by LF - two line breaks
    [TestCase("abc\r\n\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n", TestName = "KeepFirstLines_CrLfThenLf_Keep1")]
    [TestCase("abc\r\n\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n\n", TestName = "KeepFirstLines_CrLfThenLf_Keep2_IncludesEmptyLine")]
    // Multiple consecutive different line endings: \r\n (CRLF) then \n\r (LFCR) = 2 line breaks, 3 lines
    // Lines: "", "", "abc"
    [TestCase("\r\n\n\rabc", 1, TestEncoding.ASCII, LineBreakMode.All, "\r\n", TestName = "KeepFirstLines_StartsWithMixedEndings_Keep1")]
    [TestCase("\r\n\n\rabc", 2, TestEncoding.ASCII, LineBreakMode.All, "\r\n\n\r", TestName = "KeepFirstLines_StartsWithMixedEndings_Keep2_LfCrAsOne")]
    [TestCase("\r\n\n\rabc", 3, TestEncoding.ASCII, LineBreakMode.All, "\r\n\n\rabc", TestName = "KeepFirstLines_StartsWithMixedEndings_Keep3_All")]
    // Edge case: consecutive CR and LF that don't form CRLF (because they're reversed: \r\r\n)
    // \r is consumed alone, then \r\n is CRLF
    [TestCase("abc\r\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r", TestName = "KeepFirstLines_CrCrLf_Keep1_SplitsOnFirstCr")]
    [TestCase("abc\r\r\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r\r\n", TestName = "KeepFirstLines_CrCrLf_Keep2_CrLfAsSecond")]
    // CR alone followed by CRLF
    [TestCase("abc\rdef\r\nghi", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r", TestName = "KeepFirstLines_CrThenCrLf_Keep1")]
    [TestCase("abc\rdef\r\nghi", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\rdef\r\n", TestName = "KeepFirstLines_CrThenCrLf_Keep2")]
    // File ending with various combinations
    [TestCase("abc\r\n", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n", TestName = "KeepFirstLines_EndsWithCrLf_Keep1")]
    // File ending with LFCR - it's a single line ending, so there's only 1 line
    [TestCase("abc\n\r", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\n\r", TestName = "KeepFirstLines_EndsWithLfCr_Keep1_LfCrAsOneBreak")]
    public void KeepFirstLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null)
      => this._ExecuteTest(
        (f, c, e, l, o, a) => {
          if (a)
            f.KeepFirstLines(c, l);
          else
            f.KeepFirstLines(c, e, l);
        },
        input,
        count,
        testEncoding,
        newLine,
        0,
        expected,
        exception
      );

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, 0, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, 0, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, 0, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, 0, "", typeof(ArgumentException))]
    [TestCase("abc", 1, TestEncoding.ASCII, LineBreakMode.None, 0, "abc")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 0, "abc\n")]
    [TestCase("abc\ndef", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 0, "def")]
    [TestCase("abc\ndef\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 0, "def\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.CrLf, 0, "def")]
    [TestCase("abc\r\ndef\r\n", 1, TestEncoding.ASCII, LineBreakMode.CrLf, 0, "def\r\n")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "def")]
    [TestCase("abc\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.AutoDetect, 0, "def")]
    [TestCase("abc\rdef\r", 1, TestEncoding.AutoDetectFromBom, LineBreakMode.CarriageReturn, 0, "def\r")]
    [TestCase("abc\x000cdef", 1, TestEncoding.UnicodeBigEndian, LineBreakMode.FormFeed, 0, "def")]
    [TestCase("abc\x0085de\ff", 1, TestEncoding.UnicodeLittleEndianNoBOM, LineBreakMode.NextLine, 0, "de\ff")]
    [TestCase("abc\x0015de\u0085f", 1, TestEncoding.Utf8, LineBreakMode.NegativeAcknowledge, 0, "de\u0085f")]
    [TestCase("abc\x2028de\rf", 1, TestEncoding.Utf8NoBOM, LineBreakMode.LineSeparator, 0, "de\rf")]
    [TestCase("abc\x2029de\nf", 1, TestEncoding.Utf8, LineBreakMode.ParagraphSeparator, 0, "de\nf")]
    [TestCase("abc\x009Bde\nf", 1, TestEncoding.Utf8, LineBreakMode.EndOfLine, 0, "de\nf")]
    [TestCase("abc\x0076de\nf", 1, TestEncoding.Utf8, LineBreakMode.Zx, 0, "de\nf")]
    [TestCase("abc\0de\nf", 1, TestEncoding.Utf8, LineBreakMode.Null, 0, "de\nf")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, -1, "abc\n", typeof(ArgumentOutOfRangeException))]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 1, "abc\n")]
    [TestCase("abc\ndef\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 1, "abc\ndef\n")]
    [TestCase("abc\ndef\nghi\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, 1, "abc\nghi\n")]
    // Mixed line endings with LineBreakMode.All - exotic test cases
    // Both CRLF (\r\n) and LFCR (\n\r) are treated as SINGLE line endings (greedy 2-char match)
    [TestCase("abc\r\ndef\nghi\rjkl", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "jkl", TestName = "KeepLastLines_MixedCrLfLfCr_Keep1_ReturnsLastLine")]
    [TestCase("abc\r\ndef\nghi\rjkl", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "ghi\rjkl", TestName = "KeepLastLines_MixedCrLfLfCr_Keep2_ReturnsTwoLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "def\nghi\rjkl", TestName = "KeepLastLines_MixedCrLfLfCr_Keep3_ReturnsThreeLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 4, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\r\ndef\nghi\rjkl", TestName = "KeepLastLines_MixedCrLfLfCr_Keep4_ReturnsAll")]
    // LFCR (\n\r) is ONE line break - "abc\n\rdef" has 2 lines: "abc", "def"
    [TestCase("abc\n\rdef", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "def", TestName = "KeepLastLines_LfCrAsOneBreak_Keep1_ReturnsLastLine")]
    [TestCase("abc\n\rdef", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\n\rdef", TestName = "KeepLastLines_LfCrAsOneBreak_Keep2_ReturnsAll")]
    // CRLF followed by LF - two line breaks
    [TestCase("abc\r\n\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "def", TestName = "KeepLastLines_CrLfThenLf_Keep1")]
    [TestCase("abc\r\n\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "\ndef", TestName = "KeepLastLines_CrLfThenLf_Keep2_IncludesEmptyLine")]
    [TestCase("abc\r\n\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\r\n\ndef", TestName = "KeepLastLines_CrLfThenLf_Keep3_ReturnsAll")]
    // "\r\n\n\rabc": CRLF + LFCR = 2 line breaks, 3 lines: "", "", "abc"
    [TestCase("\r\n\n\rabc", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "abc", TestName = "KeepLastLines_StartsWithMixedEndings_Keep1")]
    [TestCase("\r\n\n\rabc", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "\n\rabc", TestName = "KeepLastLines_StartsWithMixedEndings_Keep2_LfCrAsOne")]
    [TestCase("\r\n\n\rabc", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "\r\n\n\rabc", TestName = "KeepLastLines_StartsWithMixedEndings_Keep3_All")]
    // Edge case: "\r\r\n" = CR + CRLF = 2 line breaks, 3 lines
    [TestCase("abc\r\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "def", TestName = "KeepLastLines_CrCrLf_Keep1_ReturnsLastLine")]
    [TestCase("abc\r\r\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "\r\ndef", TestName = "KeepLastLines_CrCrLf_Keep2_CrLfAsSecond")]
    [TestCase("abc\r\r\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\r\r\ndef", TestName = "KeepLastLines_CrCrLf_Keep3_All")]
    // CR alone followed by CRLF
    [TestCase("abc\rdef\r\nghi", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "ghi", TestName = "KeepLastLines_CrThenCrLf_Keep1")]
    [TestCase("abc\rdef\r\nghi", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "def\r\nghi", TestName = "KeepLastLines_CrThenCrLf_Keep2")]
    [TestCase("abc\rdef\r\nghi", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\rdef\r\nghi", TestName = "KeepLastLines_CrThenCrLf_Keep3_All")]
    // File ending with various combinations
    [TestCase("abc\r\n", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\r\n", TestName = "KeepLastLines_EndsWithCrLf_Keep1")]
    // "abc\n\r" - LFCR is one ending, so 1 line: "abc"
    [TestCase("abc\n\r", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "abc\n\r", TestName = "KeepLastLines_EndsWithLfCr_Keep1_LfCrAsOneBreak")]
    // "a\r\n\n\r\nb": CRLF + LFCR + LF... wait let me recalculate
    // Position 0: 'a', 1-2: '\r\n' (CRLF), 3-4: '\n\r' (LFCR), 5: '\n' (LF), 6: 'b'
    // Lines: "a", "", "", "b" = 4 lines
    [TestCase("a\r\n\n\r\nb", 1, TestEncoding.ASCII, LineBreakMode.All, 0, "b", TestName = "KeepLastLines_TripleEmptyMixed_Keep1")]
    [TestCase("a\r\n\n\r\nb", 2, TestEncoding.ASCII, LineBreakMode.All, 0, "\nb", TestName = "KeepLastLines_TripleEmptyMixed_Keep2_LfEnding")]
    [TestCase("a\r\n\n\r\nb", 3, TestEncoding.ASCII, LineBreakMode.All, 0, "\n\r\nb", TestName = "KeepLastLines_TripleEmptyMixed_Keep3")]
    public void KeepLastLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, int offset, string expected, Type? exception = null)
      => this._ExecuteTest(
        (f, c, e, l, o, a) => {
          if (o != 0) {
            if (a)
              f.KeepLastLines(c, o, l);
            else
              f.KeepLastLines(c, o, e, l);
          } else {
            if (a)
              f.KeepLastLines(c, l);
            else
              f.KeepLastLines(c, e, l);
          }
        },
        input,
        count,
        testEncoding,
        newLine,
        offset,
        expected,
        exception
      );

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, "", typeof(ArgumentException))]
    [TestCase("abc", 1, TestEncoding.ASCII, LineBreakMode.None, "")]
    [TestCase("abc\n", 1, TestEncoding.ASCII, LineBreakMode.LineFeed, "")]
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
    // Mixed line endings with LineBreakMode.All - exotic test cases
    // Both CRLF (\r\n) and LFCR (\n\r) are treated as SINGLE line endings (greedy 2-char match)
    [TestCase("abc\r\ndef\nghi\rjkl", 1, TestEncoding.ASCII, LineBreakMode.All, "def\nghi\rjkl", TestName = "RemoveFirstLines_MixedCrLfLfCr_Remove1_RemovesFirstLine")]
    [TestCase("abc\r\ndef\nghi\rjkl", 2, TestEncoding.ASCII, LineBreakMode.All, "ghi\rjkl", TestName = "RemoveFirstLines_MixedCrLfLfCr_Remove2_RemovesTwoLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 3, TestEncoding.ASCII, LineBreakMode.All, "jkl", TestName = "RemoveFirstLines_MixedCrLfLfCr_Remove3_RemovesThreeLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 4, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_MixedCrLfLfCr_Remove4_RemovesAll")]
    // LFCR (\n\r) is ONE line break - "abc\n\rdef" has 2 lines: "abc", "def"
    [TestCase("abc\n\rdef", 1, TestEncoding.ASCII, LineBreakMode.All, "def", TestName = "RemoveFirstLines_LfCrAsOneBreak_Remove1_LeavesLastLine")]
    [TestCase("abc\n\rdef", 2, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_LfCrAsOneBreak_Remove2_RemovesAll")]
    // CRLF followed by LF - two line breaks
    [TestCase("abc\r\n\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "\ndef", TestName = "RemoveFirstLines_CrLfThenLf_Remove1")]
    [TestCase("abc\r\n\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "def", TestName = "RemoveFirstLines_CrLfThenLf_Remove2_RemovesEmptyLine")]
    [TestCase("abc\r\n\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_CrLfThenLf_Remove3_RemovesAll")]
    // "\r\n\n\rabc": CRLF + LFCR = 2 line breaks, 3 lines: "", "", "abc"
    [TestCase("\r\n\n\rabc", 1, TestEncoding.ASCII, LineBreakMode.All, "\n\rabc", TestName = "RemoveFirstLines_StartsWithMixedEndings_Remove1")]
    [TestCase("\r\n\n\rabc", 2, TestEncoding.ASCII, LineBreakMode.All, "abc", TestName = "RemoveFirstLines_StartsWithMixedEndings_Remove2_LfCrAsOne")]
    [TestCase("\r\n\n\rabc", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_StartsWithMixedEndings_Remove3_All")]
    // Edge case: "\r\r\n" = CR + CRLF = 2 line breaks, 3 lines
    [TestCase("abc\r\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "\r\ndef", TestName = "RemoveFirstLines_CrCrLf_Remove1_SplitsOnFirstCr")]
    [TestCase("abc\r\r\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "def", TestName = "RemoveFirstLines_CrCrLf_Remove2_CrLfAsSecond")]
    [TestCase("abc\r\r\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_CrCrLf_Remove3_All")]
    // CR alone followed by CRLF
    [TestCase("abc\rdef\r\nghi", 1, TestEncoding.ASCII, LineBreakMode.All, "def\r\nghi", TestName = "RemoveFirstLines_CrThenCrLf_Remove1")]
    [TestCase("abc\rdef\r\nghi", 2, TestEncoding.ASCII, LineBreakMode.All, "ghi", TestName = "RemoveFirstLines_CrThenCrLf_Remove2")]
    [TestCase("abc\rdef\r\nghi", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_CrThenCrLf_Remove3_All")]
    // File ending with various combinations
    [TestCase("abc\r\n", 1, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_EndsWithCrLf_Remove1")]
    // "abc\n\r" - LFCR is one ending, so 1 line: "abc"
    [TestCase("abc\n\r", 1, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_EndsWithLfCr_Remove1_LfCrAsOneBreak")]
    // "a\r\n\n\r\nb": CRLF + LFCR + LF = 3 line breaks, 4 lines: "a", "", "", "b"
    [TestCase("a\r\n\n\r\nb", 1, TestEncoding.ASCII, LineBreakMode.All, "\n\r\nb", TestName = "RemoveFirstLines_TripleEmptyMixed_Remove1")]
    [TestCase("a\r\n\n\r\nb", 2, TestEncoding.ASCII, LineBreakMode.All, "\nb", TestName = "RemoveFirstLines_TripleEmptyMixed_Remove2_LfCrAsOne")]
    [TestCase("a\r\n\n\r\nb", 3, TestEncoding.ASCII, LineBreakMode.All, "b", TestName = "RemoveFirstLines_TripleEmptyMixed_Remove3")]
    [TestCase("a\r\n\n\r\nb", 4, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveFirstLines_TripleEmptyMixed_Remove4_All")]
    public void RemoveFirstLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null)
      => this._ExecuteTest(
        (f, c, e, l, o, a) => {
          if (a)
            f.RemoveFirstLines(c, l);
          else
            f.RemoveFirstLines(c, e, l);
        },
        input,
        count,
        testEncoding,
        newLine,
        0,
        expected,
        exception
      );

    [Test]
    [TestCase(null, 1, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(NullReferenceException))]
    [TestCase("", 0, TestEncoding.Utf8, LineBreakMode.LineFeed, null, typeof(ArgumentOutOfRangeException))]
    [TestCase("", 1, TestEncoding.Null, LineBreakMode.LineFeed, null, typeof(ArgumentNullException))]
    [TestCase("abc", 1, TestEncoding.ASCII, (LineBreakMode)short.MinValue, "", typeof(ArgumentException))]
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
    // Mixed line endings with LineBreakMode.All - exotic test cases
    // Both CRLF (\r\n) and LFCR (\n\r) are treated as SINGLE line endings (greedy 2-char match)
    [TestCase("abc\r\ndef\nghi\rjkl", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\ndef\nghi\r", TestName = "RemoveLastLines_MixedCrLfLfCr_Remove1_RemovesLastLine")]
    [TestCase("abc\r\ndef\nghi\rjkl", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r\ndef\n", TestName = "RemoveLastLines_MixedCrLfLfCr_Remove2_RemovesTwoLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 3, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n", TestName = "RemoveLastLines_MixedCrLfLfCr_Remove3_RemovesThreeLines")]
    [TestCase("abc\r\ndef\nghi\rjkl", 4, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_MixedCrLfLfCr_Remove4_RemovesAll")]
    // LFCR (\n\r) is ONE line break - "abc\n\rdef" has 2 lines: "abc", "def"
    [TestCase("abc\n\rdef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\n\r", TestName = "RemoveLastLines_LfCrAsOneBreak_Remove1_LeavesFirstLine")]
    [TestCase("abc\n\rdef", 2, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_LfCrAsOneBreak_Remove2_RemovesAll")]
    // CRLF followed by LF - two line breaks
    [TestCase("abc\r\n\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n\n", TestName = "RemoveLastLines_CrLfThenLf_Remove1")]
    [TestCase("abc\r\n\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r\n", TestName = "RemoveLastLines_CrLfThenLf_Remove2_RemovesEmptyLine")]
    [TestCase("abc\r\n\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_CrLfThenLf_Remove3_RemovesAll")]
    // "\r\n\n\rabc": CRLF + LFCR = 2 line breaks, 3 lines: "", "", "abc"
    [TestCase("\r\n\n\rabc", 1, TestEncoding.ASCII, LineBreakMode.All, "\r\n\n\r", TestName = "RemoveLastLines_StartsWithMixedEndings_Remove1")]
    [TestCase("\r\n\n\rabc", 2, TestEncoding.ASCII, LineBreakMode.All, "\r\n", TestName = "RemoveLastLines_StartsWithMixedEndings_Remove2_LfCrAsOne")]
    [TestCase("\r\n\n\rabc", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_StartsWithMixedEndings_Remove3_All")]
    // Edge case: "\r\r\n" = CR + CRLF = 2 line breaks, 3 lines
    [TestCase("abc\r\r\ndef", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\r\r\n", TestName = "RemoveLastLines_CrCrLf_Remove1_RemovesLastLine")]
    [TestCase("abc\r\r\ndef", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r", TestName = "RemoveLastLines_CrCrLf_Remove2_CrLfAsSecond")]
    [TestCase("abc\r\r\ndef", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_CrCrLf_Remove3_All")]
    // CR alone followed by CRLF
    [TestCase("abc\rdef\r\nghi", 1, TestEncoding.ASCII, LineBreakMode.All, "abc\rdef\r\n", TestName = "RemoveLastLines_CrThenCrLf_Remove1")]
    [TestCase("abc\rdef\r\nghi", 2, TestEncoding.ASCII, LineBreakMode.All, "abc\r", TestName = "RemoveLastLines_CrThenCrLf_Remove2")]
    [TestCase("abc\rdef\r\nghi", 3, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_CrThenCrLf_Remove3_All")]
    // File ending with various combinations
    [TestCase("abc\r\n", 1, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_EndsWithCrLf_Remove1")]
    // "abc\n\r" - LFCR is one ending, so 1 line: "abc"
    [TestCase("abc\n\r", 1, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_EndsWithLfCr_Remove1_LfCrAsOneBreak")]
    // "a\r\n\n\r\nb": CRLF + LFCR + LF = 3 line breaks, 4 lines: "a", "", "", "b"
    [TestCase("a\r\n\n\r\nb", 1, TestEncoding.ASCII, LineBreakMode.All, "a\r\n\n\r\n", TestName = "RemoveLastLines_TripleEmptyMixed_Remove1")]
    [TestCase("a\r\n\n\r\nb", 2, TestEncoding.ASCII, LineBreakMode.All, "a\r\n\n\r", TestName = "RemoveLastLines_TripleEmptyMixed_Remove2_LfCrAsOne")]
    [TestCase("a\r\n\n\r\nb", 3, TestEncoding.ASCII, LineBreakMode.All, "a\r\n", TestName = "RemoveLastLines_TripleEmptyMixed_Remove3")]
    [TestCase("a\r\n\n\r\nb", 4, TestEncoding.ASCII, LineBreakMode.All, "", TestName = "RemoveLastLines_TripleEmptyMixed_Remove4_All")]
    public void RemoveLastLines(string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, string expected, Type? exception = null)
      => this._ExecuteTest(
        (f, c, e, l, o, a) => {
          if (a)
            f.RemoveLastLines(c, l);
          else
            f.RemoveLastLines(c, e, l);
        },
        input,
        count,
        testEncoding,
        newLine,
        0,
        expected,
        exception
      );

    private void _ExecuteTest(Action<FileInfo?, int, Encoding?, LineBreakMode, int, bool> runner, string? input, int count, TestEncoding testEncoding, LineBreakMode newLine, int offset, string expected, Type? exception = null) {
      Encoding writeEncoding;
      Encoding? readEncoding;
      switch (testEncoding) {
        case TestEncoding.AutoDetectFromBom:
          readEncoding = null;
          writeEncoding = new UTF32Encoding(bigEndian: false, byteOrderMark: true);
          break;
        case TestEncoding.Utf8NoBOM: readEncoding = writeEncoding = new UTF8Encoding(false); break;
        case TestEncoding.Utf8: readEncoding = writeEncoding = new UTF8Encoding(true); break;
        case TestEncoding.UnicodeLittleEndianNoBOM: readEncoding = writeEncoding = new UnicodeEncoding(false, false); break;
        case TestEncoding.UnicodeBigEndian: readEncoding = writeEncoding = new UnicodeEncoding(true, true); break;
        case TestEncoding.ASCII: readEncoding = writeEncoding = Encoding.ASCII; break;
        case TestEncoding.Null:
          readEncoding = null;
          writeEncoding = Encoding.ASCII;
          break;
        default: throw new ArgumentOutOfRangeException(nameof(testEncoding), testEncoding, null);
      }

      FileInfo? file;
      if (input == null) {
        file = null;
        ExecuteTest(
          () => {
            runner(file, count, readEncoding, newLine, offset, false);
            return file.ReadAllText(writeEncoding);
          },
          expected,
          exception
        );
      } else {
        using var token = PathExtensions.GetTempFileToken();
        file = token.File;
        file.WriteAllText(input, writeEncoding);
        ExecuteTest(
          () => {
            runner(file, count, readEncoding, newLine, offset, testEncoding == TestEncoding.AutoDetectFromBom);
            return file.ReadAllText(writeEncoding);
          },
          expected,
          exception
        );
      }
    }
  }
}
