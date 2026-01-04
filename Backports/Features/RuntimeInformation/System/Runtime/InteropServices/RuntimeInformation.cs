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

// RuntimeInformation was added in .NET Standard 1.1 / .NET Core 1.0
// Available via NuGet package for net45+
#if !SUPPORTS_RUNTIME_INFORMATION && !OFFICIAL_RUNTIME_INFORMATION

namespace System.Runtime.InteropServices;

/// <summary>
/// Provides information about the .NET runtime installation.
/// </summary>
public static class RuntimeInformation {

  /// <summary>
  /// Gets the name of the .NET installation on which an app is running.
  /// </summary>
  public static string FrameworkDescription => Environment.Version.ToString();

  /// <summary>
  /// Gets the platform architecture on which the current app is running.
  /// </summary>
  public static Architecture ProcessArchitecture => IntPtr.Size == 8 ? Architecture.X64 : Architecture.X86;

  /// <summary>
  /// Gets the platform architecture of the operating system.
  /// </summary>
  public static Architecture OSArchitecture => _cachedOSArchitecture ??= _GetOSArchitecture();

  private static Architecture? _cachedOSArchitecture;

  private static Architecture _GetOSArchitecture() {
    // If process is 64-bit, OS must be 64-bit
    if (IntPtr.Size == 8)
      return Architecture.X64;

    // Process is 32-bit, but OS could be 64-bit (WoW64 on Windows, multilib on Linux)
    Architecture result;
    if (OperatingSystem.IsWindows())
      // PROCESSOR_ARCHITEW6432 is set when running 32-bit process on 64-bit Windows
      result = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432") != null
        ? Architecture.X64
        : Architecture.X86;
    else
      // On Unix, check for 64-bit indicators
      result = _IsUnix64BitOS() ? Architecture.X64 : Architecture.X86;

    return result;
  }

  private static bool _IsUnix64BitOS() {
    try {
      // Check /proc/cpuinfo for x86_64 or similar on Linux
      if (IO.File.Exists("/proc/cpuinfo")) {
        var cpuInfo = IO.File.ReadAllText("/proc/cpuinfo");
        if (cpuInfo.Contains("x86_64") || cpuInfo.Contains("aarch64") || cpuInfo.Contains("ppc64"))
          return true;
      }

      // Check /usr/lib64 existence (common on 64-bit Linux)
      if (IO.Directory.Exists("/usr/lib64"))
        return true;

      // macOS: check for x86_64 in hw.machine or hw.optional.x86_64
      if (OperatingSystem.IsMacOS() && IO.Directory.Exists("/usr/lib"))
        // On 64-bit macOS, /usr/lib exists and system is always 64-bit on modern hardware
        return true;
    } catch {
      // Ignore filesystem errors
    }

    return false;
  }

  /// <summary>
  /// Gets a string that describes the operating system on which the app is running.
  /// </summary>
  public static string OSDescription => Environment.OSVersion.ToString();

  /// <summary>
  /// Gets the platform identifier and version number on which the runtime is running.
  /// </summary>
  public static string RuntimeIdentifier {
    get {
      var arch = IntPtr.Size == 8 ? "x64" : "x86";
      return OperatingSystem.IsWindows() 
        ? $"win-{arch}" 
        : OperatingSystem.IsLinux() 
          ? $"linux-{arch}" 
          : OperatingSystem.IsMacOS() 
            ? $"osx-{arch}" 
            : OperatingSystem.IsFreeBSD() 
              ? $"freebsd-{arch}" 
              : $"unknown-{arch}"
      ;
    }
  }

  /// <summary>
  /// Indicates whether the current application is running on the specified platform.
  /// </summary>
  /// <param name="osPlatform">A platform.</param>
  /// <returns><see langword="true"/> if the current app is running on the specified platform; otherwise, <see langword="false"/>.</returns>
  public static bool IsOSPlatform(OSPlatform osPlatform) => 
    osPlatform == OSPlatform.Windows 
      ? OperatingSystem.IsWindows() 
      : osPlatform == OSPlatform.Linux 
        ? OperatingSystem.IsLinux() 
        : osPlatform == OSPlatform.OSX 
          ? OperatingSystem.IsMacOS() 
          : osPlatform == OSPlatform.FreeBSD && OperatingSystem.IsFreeBSD()
          ;
}

#endif
