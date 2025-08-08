using Corlib.Tests.NUnit;
using NUnit.Framework;

namespace System.IO;

using static TestUtilities;

[TestFixture]
internal class FileSystemInfoTest {
  /// <summary>
  ///   Tests the RelativeTo routine.
  /// </summary>
  [TestCase("a/b", "a", ExpectedResult = "b", TestName = "WhenBaseIsParent_ShouldReturnChild")]
  [TestCase("a", "a/b", ExpectedResult = "..", TestName = "WhenBaseIsChild_ShouldReturnParent")]
  [TestCase("a/b", "a/b", ExpectedResult = "", TestName = "WhenBaseIsSame_ShouldReturnEmpty")]
  [TestCase("c", "a", ExpectedResult = "c", TestName = "WhenNoCommonPath_ShouldReturnOriginal")]
  [TestCase("a/b", "a\\b", ExpectedResult = "", TestName = "WhenBaseDiffersBySlashes_ShouldReturnEmpty")]
  public string? TestRelativeTo(string path, string baseDir) => typeof(FileSystemInfoExtensions).NonPublic<string>("_RelativeTo")([path, baseDir]);
}
