using System.Diagnostics;
using System.IO;
using Corlib.Tests.NUnit;
using NUnit.Framework;

namespace Corlib.Tests.System.IO;

[TestFixture]
internal class PathTest {

  [Test]
  public static void GetUsableSystemTempDirectoryName_Should_Exists() {
    var arrange = PathExtensions.GetUsableSystemTempDirectoryName();
    Assert.That(Directory.Exists(arrange),Is.EqualTo(true));
  }
}
