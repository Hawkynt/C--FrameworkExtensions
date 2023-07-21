namespace Corlib.Tests.System.Diagnostics;

using global::NUnit.Framework;
using global::System.Diagnostics;
using global::System.Linq;
using static Corlib.Tests.NUnit.TestUtilities;

[TestFixture]
public class ProcessTests {

  [Test]
  public static void GetParentProcess() => Assert.That(ProcessExtensions.GetParentProcess() != null);

  [Test]
  public static void Parent() => Assert.That(ProcessExtensions.GetParentProcess().Id == Process.GetCurrentProcess().Parent().Id);

  private static readonly string _EXECUTABLE_TO_TEST = IsWindowsPlatform() ? "cmd.exe" : "bash";

  [Test]
  public static void ParentOfChildren() {
    if(!IsWindowsPlatform())
      Assert.Pass("[Skipped]Not supported on non windows platforms");

    var myPid = Process.GetCurrentProcess().Id;
    int childParentId;

    Process? childProcess = null;
    try {
      childProcess = Process.Start(new ProcessStartInfo(_EXECUTABLE_TO_TEST) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
      childParentId = childProcess.Parent().Id;
    } finally {
      childProcess?.Kill();
    }

    Assert.That(myPid == childParentId);
  }

  [Test]
  public static void Children() {
    if (!IsWindowsPlatform())
      Assert.Pass("[Skipped]Not supported on non windows platforms");
    
    var myself = Process.GetCurrentProcess();
    int? childId;
    Process? child;
    Process? childProcess = null;
    try {
      childProcess = Process.Start(new ProcessStartInfo(_EXECUTABLE_TO_TEST) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
      childId = childProcess?.Id;
      var children = myself.Children().ToArray();
      child = children.FirstOrDefault(p => p.Id == childId);
    } finally {
      childProcess?.Kill();
    }

    Assert.That(child, Is.Not.Null);
  }

  [Test]
  public static void AllChildren() {
    if (!IsWindowsPlatform())
      Assert.Pass("[Skipped]Not supported on non windows platforms");
    
    var myself = Process.GetCurrentProcess();
    int? childId;
    Process? child;
    Process? childProcess = null;
    try {
      childProcess = Process.Start(new ProcessStartInfo(_EXECUTABLE_TO_TEST) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, Arguments = "/k cmd.exe" });
      childId = childProcess?.Id;
      var children = myself.AllChildren().ToArray();
      child = children.FirstOrDefault(p => p.Parent()?.Id == childId);
    } finally {
      childProcess?.Kill();
    }

    Assert.That(child, Is.Not.Null);
  }

}