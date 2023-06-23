namespace Corlib.Tests.System.Diagnostics;

using global::NUnit.Framework;
using global::System.Diagnostics;
using global::System.Linq;
using static Corlib.Tests.NUnit.TestUtilities;

[TestFixture]
public class ProcessTests {

  [Test]
  public static void GetParentProcess() => Assert.That(ProcessExtensions.GetParentProcess()!=null);

  [Test]
  public static void Parent() => Assert.That(ProcessExtensions.GetParentProcess().Id == Process.GetCurrentProcess().Parent().Id);

  [Test]
  public static void ParentOfChildren() {
    var myPid=Process.GetCurrentProcess().Id;
    int childParentId;

    Process? childProcess = null;
    try {
      childProcess = Process.Start(new ProcessStartInfo("cmd.exe") { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
      childParentId = childProcess.Parent().Id;
    } finally {
      childProcess?.Kill();
    }
    
    Assert.That(myPid == childParentId);
  }

  [Test]
  public static void Children() {
    var myself = Process.GetCurrentProcess();
    int childId;
    int? childId2;

    Process? childProcess=null;
    try {
      childProcess = Process.Start(new ProcessStartInfo("cmd.exe"){CreateNoWindow = true,WindowStyle = ProcessWindowStyle.Hidden} );
      // TODO: FIXME: test doesn't want to run somehow
      childId = childProcess.Id;
      var children = myself.Children().ToArray();
      childId2 = children.FirstOrDefault()?.Id;
    } finally {
      childProcess?.Kill();
    }
    
    Assert.That(childId2,Is.EqualTo(childId));
  }

}