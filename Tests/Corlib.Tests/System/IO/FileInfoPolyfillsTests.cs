using System.Text;
using System.Threading;
using NUnit.Framework;

namespace System.IO;

[TestFixture]
public class FileInfoPolyfillsTests {
  private string _testDirectory;
  private string _sourceDirectory;
  private string _targetDirectory;
  private const string TestContent = "This is test content";
  private const string TestContent2 = "This is different test content";

  [SetUp]
  public void SetUp() {
    // Erstelle ein Testverzeichnis
    this._testDirectory = Path.Combine(Path.GetTempPath(), "MoveToOverwriteTests_" + Guid.NewGuid().ToString("N"));
    this._sourceDirectory = Path.Combine(this._testDirectory, "Source");
    this._targetDirectory = Path.Combine(this._testDirectory, "Target");

    Directory.CreateDirectory(this._sourceDirectory);
    Directory.CreateDirectory(this._targetDirectory);
  }

  [TearDown]
  public void TearDown() {
    // Bereinige Testverzeichnisse
    try {
      if (Directory.Exists(this._testDirectory))
        Directory.Delete(this._testDirectory, true);
    } catch {
      // Ignoriere Fehler beim Aufräumen
    }
  }

  [Test]
  public void MoveTo_DestinationDoesNotExist_MovesFile() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Dateiinhalt sollte identisch sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");
  }

  [Test]
  public void MoveTo_DestinationExists_OverwriteTrue_OverwritesFile() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");
    this.CreateTestFile(this._targetDirectory, "dest.txt", TestContent2); // Zieldatei erstellen

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Zieldatei sollte überschrieben worden sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");
  }

  [Test]
  public void MoveTo_DestinationExists_OverwriteFalse_ThrowsIOException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");
    this.CreateTestFile(this._targetDirectory, "dest.txt", TestContent2); // Zieldatei erstellen

    // Act & Assert
    var ex = Assert.Throws<IOException>(() => sourceFile.MoveTo(destPath, false));
    Assert.IsTrue(File.Exists(originalPath), "Quelldatei sollte noch existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Quelldatei sollte unverändert sein");
    Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Zieldatei sollte unverändert sein");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  [Test]
  public void MoveTo_SameDirectoryOverwrite_CorrectlyRenames() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._sourceDirectory, "dest.txt");
    this.CreateTestFile(this._sourceDirectory, "dest.txt", TestContent2); // Zieldatei erstellen

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Zieldatei sollte überschrieben worden sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");
  }

  [Test]
  public void MoveTo_TargetDirectoryDoesNotExist_ThrowsDirectoryNotFoundException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var nonExistentDir = Path.Combine(this._testDirectory, "NonExistent");
    var destPath = Path.Combine(nonExistentDir, "dest.txt");

    // Act & Assert
    Assert.Throws<DirectoryNotFoundException>(() => sourceFile.MoveTo(destPath, true));
    Assert.IsTrue(File.Exists(originalPath), "Quelldatei sollte noch existieren");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  [Test]
  public void MoveTo_LargeFile_CorrectlyMoves() {
    // Arrange
    var largeContent = new string('A', 10 * 1024 * 1024); // 10 MB Datei
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "large.txt", largeContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "large_dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(largeContent.Length, new FileInfo(destPath).Length, "Dateigröße sollte identisch sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");
  }

  [Test]
  public void MoveTo_DestinationIsHidden_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "hidden_dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "hidden_dest.txt", TestContent2);

    // Zieldatei als versteckt markieren
    File.SetAttributes(destPath, FileAttributes.Hidden);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Zieldatei sollte überschrieben worden sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");

    // Prüfen, ob das Hidden-Attribut erhalten bleibt oder nicht
    // Je nach Implementierung kann das Attribut erhalten bleiben oder verloren gehen
    // Das ist hauptsächlich eine Frage der Implementierungsdetails
    Assert.IsFalse((File.GetAttributes(destPath) & FileAttributes.Hidden) == FileAttributes.Hidden, "Zieldatei sollte nicht mehr versteckt sein");
  }

  [Test]
  public void MoveTo_DestinationIsSystem_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "system_dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "system_dest.txt", TestContent2);

    // Zieldatei als Systemdatei markieren
    File.SetAttributes(destPath, FileAttributes.System);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Zieldatei sollte überschrieben worden sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");

    // Prüfen, ob das System-Attribut erhalten bleibt oder nicht
    // Je nach Implementierung kann das Attribut erhalten bleiben oder verloren gehen
    Assert.IsFalse((File.GetAttributes(destPath) & FileAttributes.System) == FileAttributes.System, "Zieldatei sollte keine Systemdatei mehr sein");
  }

  [Test]
  public void MoveTo_DestinationWithHiddenAndSystem_OverwriteTrue_SuccessfullyOverwrites() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "multi_attr_dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "multi_attr_dest.txt", TestContent2);

    // Zieldatei mit Hidden und System Attributen markieren (aber nicht ReadOnly)
    File.SetAttributes(destPath, FileAttributes.Hidden | FileAttributes.System);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(destPath), "Zieldatei sollte überschrieben worden sein");
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert sein");
  }

  [Test]
  public void MoveTo_DestinationIsReadOnly_OverwriteTrue_DoesNotOverwrite() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "readonly_dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "readonly_dest.txt", TestContent2);

    // Zieldatei als schreibgeschützt markieren
    File.SetAttributes(destPath, FileAttributes.ReadOnly);

    // Act
    Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsTrue(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Quelldatei sollte nicht überschrieben worden sein");
    Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Zieldatei sollte nicht überschrieben worden sein");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
    Assert.IsTrue((File.GetAttributes(destPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                  "Zieldatei sollte immer noch schreibgeschützt sein");
  }

  [Test]
  public void MoveTo_DestinationWithMultipleAttributes_OverwriteTrue_DoesNotOverwrite() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "multi_attr_dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "multi_attr_dest.txt", TestContent2);

    // Zieldatei mit mehreren Attributen markieren
    File.SetAttributes(destPath, FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System);

    // Act
    Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));

    // Assert
    Assert.IsTrue(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Quelldatei sollte nicht überschrieben worden sein");
    Assert.AreEqual(TestContent2, File.ReadAllText(destPath), "Zieldatei sollte nicht überschrieben worden sein");
    Assert.AreEqual(destPath, destFile.FullName, "FileInfo.FullName sollte nicht aktualisiert sein");

    var attributes = File.GetAttributes(destPath);
    Assert.IsTrue((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                  "Zieldatei sollte immer noch schreibgeschützt sein");
    Assert.IsTrue((attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                  "Zieldatei sollte immer noch versteckt sein");
    Assert.IsTrue((attributes & FileAttributes.System) == FileAttributes.System,
                  "Zieldatei sollte immer noch eine Systemdatei sein");
  }

  [Test]
  public void MoveTo_FileInUse_HandlesAppropriately() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");
    var destFile = this.CreateTestFile(this._targetDirectory, "dest.txt", TestContent2);

    // Öffne die Zieldatei mit FileShare.None, um sie zu sperren
    using var fileStream = new FileStream(destPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

    // Act & Assert
    Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destPath, true));

    // Stelle sicher, dass die Quelldatei noch existiert
    Assert.IsTrue(File.Exists(originalPath), "Quelldatei sollte noch existieren");
    Assert.AreEqual(TestContent, File.ReadAllText(originalPath), "Quelldatei sollte unverändert sein");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  [Test]
  public void MoveTo_FileInfoBehavior_UpdatesSourceFileInfo() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalFullName = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.AreEqual(destPath, sourceFile.FullName, "FileInfo.FullName sollte aktualisiert werden");
    Assert.AreEqual("dest.txt", sourceFile.Name, "FileInfo.Name sollte aktualisiert werden");
    Assert.AreEqual(this._targetDirectory, sourceFile.DirectoryName, "FileInfo.DirectoryName sollte aktualisiert werden");
    Assert.IsTrue(sourceFile.Exists, "FileInfo.Exists sollte true sein");
    Assert.IsFalse(File.Exists(originalFullName), "Originaldatei sollte nicht mehr existieren");
  }

  [Test]
  public void MoveTo_MultipleConcurrentOperations_WorksCorrectly() {
    // Arrange
    const int fileCount = 5;
    var sourceFiles = new FileInfo[fileCount];
    var originalPaths = new string[fileCount];
    var destPaths = new string[fileCount];

    for (int i = 0; i < fileCount; i++) {
      sourceFiles[i] = this.CreateTestFile(this._sourceDirectory, $"source{i}.txt", TestContent + i);
      originalPaths[i] = sourceFiles[i].FullName;
      destPaths[i] = Path.Combine(this._targetDirectory, $"dest{i}.txt");
      this.CreateTestFile(this._targetDirectory, $"dest{i}.txt", TestContent2 + i);
    }

    // Act
    var threads = new Thread[fileCount];
    var exceptions = new Exception[fileCount];

    for (int i = 0; i < fileCount; i++) {
      int index = i; // Lokale Kopie für Lambda-Ausdruck
      threads[i] = new(() => {
        try {
          sourceFiles[index].MoveTo(destPaths[index], true);
        } catch (Exception ex) {
          exceptions[index] = ex;
        }
      });
      threads[i].Start();
    }

    foreach (var thread in threads) {
      thread.Join();
    }

    // Assert
    for (int i = 0; i < fileCount; i++) {
      Assert.IsNull(exceptions[i], $"Operation für Datei {i} sollte erfolgreich sein");
      Assert.IsFalse(File.Exists(originalPaths[i]),
        $"Ursprünglicher Dateipfad für Quelldatei {i} sollte nicht mehr existieren");
      Assert.IsTrue(File.Exists(destPaths[i]), $"Zieldatei {i} sollte existieren");
      Assert.AreEqual(TestContent + i, File.ReadAllText(destPaths[i]),
        $"Zieldatei {i} sollte den korrekten Inhalt haben");
      Assert.AreEqual(destPaths[i], sourceFiles[i].FullName,
        $"FileInfo.FullName für Datei {i} sollte aktualisiert sein");
    }
  }

  [Test]
  public void MoveTo_NonExistentSourceFile_ThrowsFileNotFoundException() {
    // Arrange
    var nonExistentFile = new FileInfo(Path.Combine(this._sourceDirectory, "nonexistent.txt"));
    var originalPath = nonExistentFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Act & Assert
    var ex = Assert.Throws<FileNotFoundException>(() => nonExistentFile.MoveTo(destPath, true));
    Assert.AreEqual(originalPath, nonExistentFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  [Test]
  public void MoveTo_DestinationIsDirectory_ThrowsUnauthorizedAccessException() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destDirectory = this._targetDirectory; // Zielpfad ist ein Verzeichnis

    // Act & Assert
    var ex = Assert.Throws<UnauthorizedAccessException>(() => sourceFile.MoveTo(destDirectory, true));
    Assert.IsTrue(File.Exists(originalPath), "Quelldatei sollte noch existieren");
    Assert.AreEqual(originalPath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  [Test]
  public void MoveTo_SameFileOverwrite_NoOperation() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var sourceContent = File.ReadAllText(sourceFile.FullName);
    var sourceFilePath = sourceFile.FullName;

    // Act - Verschieben an denselben Ort
    sourceFile.MoveTo(sourceFilePath, true);

    // Assert
    Assert.IsTrue(File.Exists(sourceFilePath), "Datei sollte weiterhin existieren");
    Assert.AreEqual(sourceContent, File.ReadAllText(sourceFilePath), "Inhalt sollte unverändert sein");
    Assert.AreEqual(sourceFilePath, sourceFile.FullName, "FileInfo.FullName sollte unverändert sein");
  }

  // Hilfsmethode zum Erstellen von Testdateien
  private FileInfo CreateTestFile(string directory, string fileName, string content) {
    var filePath = Path.Combine(directory, fileName);
    File.WriteAllText(filePath, content, new UTF8Encoding(false));
    return new(filePath);
  }
  [Test]
  public void MoveTo_SourceHasAttributes_AttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Setze verschiedene Attribute auf die Quelldatei
    File.SetAttributes(originalPath, FileAttributes.Archive | FileAttributes.Hidden);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");

    var targetAttributes = File.GetAttributes(destPath);
    Assert.IsTrue((targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
                "Archive-Attribut sollte auf die Zieldatei übertragen werden");
    Assert.IsTrue((targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                "Hidden-Attribut sollte auf die Zieldatei übertragen werden");
  }

  [Test]
  public void MoveTo_SourceHasAttributes_DestinationExists_OverwriteTrue_AttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");
    this.CreateTestFile(this._targetDirectory, "dest.txt", TestContent2); // Zieldatei erstellen

    // Setze verschiedene Attribute auf die Quelldatei
    File.SetAttributes(originalPath, FileAttributes.Archive | FileAttributes.System);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");

    var targetAttributes = File.GetAttributes(destPath);
    Assert.IsTrue((targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
                "Archive-Attribut sollte auf die Zieldatei übertragen werden");
    Assert.IsTrue((targetAttributes & FileAttributes.System) == FileAttributes.System,
                "System-Attribut sollte auf die Zieldatei übertragen werden");
  }

  [Test]
  public void MoveTo_SourceAndDestinationHaveAttributes_OverwriteTrue_SourceAttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");
    this.CreateTestFile(this._targetDirectory, "dest.txt", TestContent2);

    // Setze unterschiedliche Attribute auf Quell- und Zieldatei
    File.SetAttributes(originalPath, FileAttributes.Archive | FileAttributes.Hidden);
    File.SetAttributes(destPath, FileAttributes.System | FileAttributes.Temporary);

    // Act
    sourceFile.MoveTo(destPath, true);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");

    var targetAttributes = File.GetAttributes(destPath);

    // Die Attribute der Quelldatei sollten erhalten bleiben
    Assert.IsTrue((targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
                "Archive-Attribut der Quelldatei sollte erhalten bleiben");
    Assert.IsTrue((targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                "Hidden-Attribut der Quelldatei sollte erhalten bleiben");

    // Die Attribute der Zieldatei sollten nicht mehr vorhanden sein
    Assert.IsFalse((targetAttributes & FileAttributes.System) == FileAttributes.System,
                "System-Attribut der Zieldatei sollte nicht erhalten bleiben");
    Assert.IsFalse((targetAttributes & FileAttributes.Temporary) == FileAttributes.Temporary,
                "Temporary-Attribut der Zieldatei sollte nicht erhalten bleiben");
  }

  [Test]
  public void MoveTo_SourceIsReadOnly_AttributeIsPreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Setze ReadOnly-Attribut auf die Quelldatei
    File.SetAttributes(originalPath, FileAttributes.ReadOnly);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");

    var targetAttributes = File.GetAttributes(destPath);
    Assert.IsTrue((targetAttributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly,
                "ReadOnly-Attribut sollte auf die Zieldatei übertragen werden");
  }

  [Test]
  public void MoveTo_SourceHasAllAttributes_AllAttributesArePreserved() {
    // Arrange
    var sourceFile = this.CreateTestFile(this._sourceDirectory, "source.txt", TestContent);
    var originalPath = sourceFile.FullName;
    var destPath = Path.Combine(this._targetDirectory, "dest.txt");

    // Setze mehrere Attribute auf die Quelldatei
    var sourceAttributes = FileAttributes.Archive | FileAttributes.Hidden |
                           FileAttributes.System | FileAttributes.Temporary;
    File.SetAttributes(originalPath, sourceAttributes);

    // Act
    sourceFile.MoveTo(destPath, false);

    // Assert
    Assert.IsFalse(File.Exists(originalPath), "Ursprünglicher Dateipfad sollte nicht mehr existieren");
    Assert.IsTrue(File.Exists(destPath), "Zieldatei sollte existieren");

    var targetAttributes = File.GetAttributes(destPath);

    // Überprüfe alle Attribute einzeln
    Assert.IsTrue((targetAttributes & FileAttributes.Archive) == FileAttributes.Archive,
                "Archive-Attribut sollte erhalten bleiben");
    Assert.IsTrue((targetAttributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                "Hidden-Attribut sollte erhalten bleiben");
    Assert.IsTrue((targetAttributes & FileAttributes.System) == FileAttributes.System,
                "System-Attribut sollte erhalten bleiben");
    Assert.IsTrue((targetAttributes & FileAttributes.Temporary) == FileAttributes.Temporary,
                "Temporary-Attribut sollte erhalten bleiben");
  }
}