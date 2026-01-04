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
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("OperatingSystem")]
public class OperatingSystemTests {

  #region Platform Detection - Basic

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWindows_ReturnsTrueOnWindows() {
    var isWindows = OperatingSystem.IsWindows();
    var expected = Environment.OSVersion.Platform == PlatformID.Win32NT
                   || Environment.OSVersion.Platform == PlatformID.Win32S
                   || Environment.OSVersion.Platform == PlatformID.Win32Windows
                   || Environment.OSVersion.Platform == PlatformID.WinCE;
    Assert.That(isWindows, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsLinux_ConsistentWithPlatform() {
    var isLinux = OperatingSystem.IsLinux();
    // On Windows, IsLinux should always return false
    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
      Assert.That(isLinux, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsMacOS_ConsistentWithPlatform() {
    var isMacOS = OperatingSystem.IsMacOS();
    // If platform reports MacOSX, IsMacOS should return true
    if (Environment.OSVersion.Platform == PlatformID.MacOSX)
      Assert.That(isMacOS, Is.True);
    // On Windows, IsMacOS should always return false
    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
      Assert.That(isMacOS, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsFreeBSD_ConsistentWithPlatform() {
    var isFreeBSD = OperatingSystem.IsFreeBSD();
    // On Windows, IsFreeBSD should always return false
    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
      Assert.That(isFreeBSD, Is.False);
  }

  #endregion

  #region Non-Desktop Platforms

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsAndroid_ReturnsFalseOnDesktop() {
    // On .NET Framework / desktop .NET, Android is never running
    var isAndroid = OperatingSystem.IsAndroid();
    Assert.That(isAndroid, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsBrowser_ReturnsFalseOnDesktop() {
    // On .NET Framework / desktop .NET, Browser is never running
    var isBrowser = OperatingSystem.IsBrowser();
    Assert.That(isBrowser, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsIOS_ReturnsFalseOnDesktop() {
    // On .NET Framework / desktop .NET, iOS is never running
    var isIOS = OperatingSystem.IsIOS();
    Assert.That(isIOS, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsMacCatalyst_ReturnsFalseOnDesktop() {
    var isMacCatalyst = OperatingSystem.IsMacCatalyst();
    Assert.That(isMacCatalyst, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsTvOS_ReturnsFalseOnDesktop() {
    var isTvOS = OperatingSystem.IsTvOS();
    Assert.That(isTvOS, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWasi_ReturnsFalseOnDesktop() {
    var isWasi = OperatingSystem.IsWasi();
    Assert.That(isWasi, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWatchOS_ReturnsFalseOnDesktop() {
    var isWatchOS = OperatingSystem.IsWatchOS();
    Assert.That(isWatchOS, Is.False);
  }

  #endregion

  #region IsOSPlatform

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatform_Windows_MatchesIsWindows() {
    var isWindows = OperatingSystem.IsWindows();
    var isOSPlatform = OperatingSystem.IsOSPlatform("Windows");
    Assert.That(isOSPlatform, Is.EqualTo(isWindows));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatform_Linux_MatchesIsLinux() {
    var isLinux = OperatingSystem.IsLinux();
    var isOSPlatform = OperatingSystem.IsOSPlatform("Linux");
    Assert.That(isOSPlatform, Is.EqualTo(isLinux));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatform_CaseVariants_Work() {
    var isWindowsLower = OperatingSystem.IsOSPlatform("windows");
    var isWindowsUpper = OperatingSystem.IsOSPlatform("Windows");
    Assert.That(isWindowsLower, Is.EqualTo(isWindowsUpper));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatform_MacOS_MatchesIsMacOS() {
    var isMacOS = OperatingSystem.IsMacOS();
    Assert.That(OperatingSystem.IsOSPlatform("macOS"), Is.EqualTo(isMacOS));
    Assert.That(OperatingSystem.IsOSPlatform("macos"), Is.EqualTo(isMacOS));
    Assert.That(OperatingSystem.IsOSPlatform("OSX"), Is.EqualTo(isMacOS));
    Assert.That(OperatingSystem.IsOSPlatform("osx"), Is.EqualTo(isMacOS));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatform_Unknown_ReturnsFalse() {
    var isUnknown = OperatingSystem.IsOSPlatform("UnknownPlatform");
    Assert.That(isUnknown, Is.False);
  }

  #endregion

  #region Version At Least

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWindowsVersionAtLeast_ReturnsFalseOnNonWindows() {
    if (!OperatingSystem.IsWindows()) {
      var result = OperatingSystem.IsWindowsVersionAtLeast(1);
      Assert.That(result, Is.False);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWindowsVersionAtLeast_WithVersion1_ReturnsTrueOnWindows() {
    if (OperatingSystem.IsWindows()) {
      // Any Windows version should be at least version 1
      var result = OperatingSystem.IsWindowsVersionAtLeast(1);
      Assert.That(result, Is.True);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsWindowsVersionAtLeast_WithVeryHighVersion_ReturnsFalseOnWindows() {
    if (OperatingSystem.IsWindows()) {
      // No Windows version should be 999+
      var result = OperatingSystem.IsWindowsVersionAtLeast(999);
      Assert.That(result, Is.False);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsAndroidVersionAtLeast_AlwaysReturnsFalseOnDesktop() {
    var result = OperatingSystem.IsAndroidVersionAtLeast(1);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsIOSVersionAtLeast_AlwaysReturnsFalseOnDesktop() {
    var result = OperatingSystem.IsIOSVersionAtLeast(1);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsMacOSVersionAtLeast_ReturnsFalseOnNonMacOS() {
    if (!OperatingSystem.IsMacOS()) {
      var result = OperatingSystem.IsMacOSVersionAtLeast(1);
      Assert.That(result, Is.False);
    }
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsFreeBSDVersionAtLeast_ReturnsFalseOnNonFreeBSD() {
    if (!OperatingSystem.IsFreeBSD()) {
      var result = OperatingSystem.IsFreeBSDVersionAtLeast(1);
      Assert.That(result, Is.False);
    }
  }

  #endregion

  #region IsOSPlatformVersionAtLeast

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatformVersionAtLeast_Windows_MatchesIsWindowsVersionAtLeast() {
    var direct = OperatingSystem.IsWindowsVersionAtLeast(6, 1);
    var viaPlatform = OperatingSystem.IsOSPlatformVersionAtLeast("Windows", 6, 1);
    Assert.That(viaPlatform, Is.EqualTo(direct));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_IsOSPlatformVersionAtLeast_Unknown_ReturnsFalse() {
    var result = OperatingSystem.IsOSPlatformVersionAtLeast("UnknownPlatform", 1);
    Assert.That(result, Is.False);
  }

  #endregion

  #region Mutual Exclusivity

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_OnlyOneDesktopPlatformIsTrue() {
    // At most one of Windows, Linux, macOS, FreeBSD should be true
    var count = 0;
    if (OperatingSystem.IsWindows()) ++count;
    if (OperatingSystem.IsLinux()) ++count;
    if (OperatingSystem.IsMacOS()) ++count;
    if (OperatingSystem.IsFreeBSD()) ++count;

    Assert.That(count, Is.LessThanOrEqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void OperatingSystem_AtLeastOneDesktopPlatformIsTrue() {
    // At least one platform should be detected
    var anyPlatform = OperatingSystem.IsWindows()
                      || OperatingSystem.IsLinux()
                      || OperatingSystem.IsMacOS()
                      || OperatingSystem.IsFreeBSD();

    // On desktop .NET, this should always be true
    Assert.That(anyPlatform, Is.True);
  }

  #endregion

}
