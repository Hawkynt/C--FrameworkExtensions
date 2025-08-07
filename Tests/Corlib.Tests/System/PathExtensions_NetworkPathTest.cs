using NUnit.Framework;

namespace System.IO.PathExtensionTests;

[TestFixture]
public class NetworkPath {
  private const string expectedPath = "abc";
  private const string expectedShare = "def";
  private const string expectedUser = "ghi";
  private const string expectedPassword = "jkl";
  private const string expectedServer = "mno";

  // TODO: more tests until all consts are used
  private const string shareAndPath = "\\" + expectedShare + "\\" + expectedPath;
  private const string serverAndShare = "\\\\" + expectedServer;
  private const string shareOnly = "\\" + expectedShare;
  private const string serverShareAndPath = "\\\\" + expectedServer + "\\" + expectedShare + "\\" + expectedPath;
  private const string userWithServer = expectedUser + "@\\\\" + expectedServer;

  private const string userPasswordServerAndShare =
    expectedUser + ":" + expectedPassword + "@\\" + expectedServer + "\\" + expectedShare;

  /// <summary>
  ///   Ein Test für "NetworkPath"
  /// </summary>
  [Test]
  public void NetworkPathCtorTest() {
    var a = new PathExtensions.NetworkPath();
    Assert.AreEqual(a.Username, null, "Username should be <null>");
    Assert.AreEqual(a.Password, null, "Password should be <null>");
    Assert.AreEqual(a.Server, null, "Server should be <null>");
    Assert.AreEqual(a.Share, null, "Share should be <null>");
    Assert.AreEqual(a.DirectoryAndOrFileName, null, "DirectoryAndOrFileName should be <null>");
    Assert.AreEqual(a.FullPath, null, "FullPath should be <null>");
    Assert.AreEqual(a.UncPath, null, "UncPath should be <null>");
  }

  /// <summary>
  ///   Ein Test für "NetworkPath"
  /// </summary>
  [Test]
  public void NetworkPathPathOnlyTest() {
    var a = new PathExtensions.NetworkPath(expectedPath);
    Assert.AreEqual(a.Username, null, "Username should be <null>");
    Assert.AreEqual(a.Password, null, "Password should be <null>");
    Assert.AreEqual(a.Server, null, "Server should be <null>");
    Assert.AreEqual(a.Share, null, "Share should be <null>");
    Assert.AreEqual(a.DirectoryAndOrFileName, expectedPath, "DirectoryAndOrFileName is not what it should");
    Assert.AreEqual(a.FullPath, expectedPath, "FullPath is not what it should");
    Assert.AreEqual(a.UncPath, expectedPath, "UncPath is not what it should");
  }

  /// <summary>
  ///   Ein Test für "NetworkPath"
  /// </summary>
  [Test]
  public void NetworkPathShareOnlyTest() {
    var a = new PathExtensions.NetworkPath(shareOnly);
    Assert.AreEqual(a.Username, null, "Username should be <null>");
    Assert.AreEqual(a.Password, null, "Password should be <null>");
    Assert.AreEqual(a.Server, null, "Server should be <null>");
    Assert.AreEqual(a.Share, expectedShare, "Share is not what it should");
    Assert.AreEqual(a.DirectoryAndOrFileName, null, "DirectoryAndOrFileName should be <null>");
    Assert.AreEqual(a.FullPath, shareOnly, "FullPath is not what it should");
    Assert.AreEqual(a.UncPath, shareOnly, "UncPath is not what it should");
  }
}
