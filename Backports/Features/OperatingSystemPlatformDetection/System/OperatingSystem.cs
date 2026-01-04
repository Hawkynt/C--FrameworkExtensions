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

using System.Runtime.CompilerServices;
using MethodImplOptions = Utilities.MethodImplOptions;

#if !SUPPORTS_OPERATING_SYSTEM_PLATFORM_DETECTION_WAVE1

namespace System {

/// <summary>
/// Provides polyfill extension members for platform detection on the <see cref="OperatingSystem"/> class.
/// </summary>
/// <remarks>
/// These static methods were added in .NET 5.0 to provide a consistent API for checking the current operating system.
/// </remarks>
public static partial class OperatingSystemPolyfills {

  extension(OperatingSystem) {

    /// <summary>
    /// Indicates whether the current application is running on Android.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on Android; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAndroid() => _GetOSPlatform() == OSPlatformType.Android;

    /// <summary>
    /// Indicates whether the current application is running as WebAssembly in a browser.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running as WebAssembly in a browser; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBrowser() => _GetOSPlatform() == OSPlatformType.Browser;

    /// <summary>
    /// Indicates whether the current application is running on FreeBSD.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on FreeBSD; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFreeBSD() => _GetOSPlatform() == OSPlatformType.FreeBSD;

    /// <summary>
    /// Indicates whether the current application is running on iOS.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on iOS; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIOS() => _GetOSPlatform() == OSPlatformType.iOS;

    /// <summary>
    /// Indicates whether the current application is running on Linux.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on Linux; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLinux() => _GetOSPlatform() == OSPlatformType.Linux;

    /// <summary>
    /// Indicates whether the current application is running on macOS.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on macOS; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMacOS() => _GetOSPlatform() == OSPlatformType.MacOS;

    /// <summary>
    /// Indicates whether the current application is running on tvOS.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on tvOS; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsTvOS() => _GetOSPlatform() == OSPlatformType.tvOS;

    /// <summary>
    /// Indicates whether the current application is running on watchOS.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on watchOS; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWatchOS() => _GetOSPlatform() == OSPlatformType.watchOS;

    /// <summary>
    /// Indicates whether the current application is running on Windows.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on Windows; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWindows() => _GetOSPlatform() == OSPlatformType.Windows;

    /// <summary>
    /// Indicates whether the current application is running on the specified platform.
    /// </summary>
    /// <param name="platform">The platform to check (e.g., "Windows", "Linux", "macOS", "FreeBSD", "Android", "iOS", "Browser").</param>
    /// <returns><see langword="true"/> if the current application is running on the specified platform; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOSPlatform(string platform) =>
      platform switch {
        "Windows" or "windows" => IsWindows(),
        "Linux" or "linux" => IsLinux(),
        "macOS" or "macos" or "OSX" or "osx" => IsMacOS(),
        "FreeBSD" or "freebsd" => IsFreeBSD(),
        "Android" or "android" => IsAndroid(),
        "iOS" or "ios" => IsIOS(),
        "Browser" or "browser" => IsBrowser(),
        "MacCatalyst" or "maccatalyst" => OperatingSystem.IsMacCatalyst(), // Provided by Wave 2
        "tvOS" or "tvos" => IsTvOS(),
        "watchOS" or "watchos" => IsWatchOS(),
        "WASI" or "wasi" => OperatingSystem.IsWasi(), // Provided by Wave 3
        _ => false
      };

    /// <summary>
    /// Checks whether the current Android operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <param name="revision">The revision number (default is 0).</param>
    /// <returns><see langword="true"/> if the current Android version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsAndroidVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0) {
      if (!IsAndroid())
        return false;

      var version = _GetAndroidVersion();
      return version != null && _IsVersionAtLeast(version, major, minor, build, revision);
    }

    /// <summary>
    /// Checks whether the current FreeBSD operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <param name="revision">The revision number (default is 0).</param>
    /// <returns><see langword="true"/> if the current FreeBSD version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsFreeBSDVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
      => IsFreeBSD() && _IsVersionAtLeast(Environment.OSVersion.Version, major, minor, build, revision);

    /// <summary>
    /// Checks whether the current iOS operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <returns><see langword="true"/> if the current iOS version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsIOSVersionAtLeast(int major, int minor = 0, int build = 0) {
      if (!IsIOS())
        return false;

      var version = _GetIOSVersion();
      return version != null && _IsVersionAtLeast(version, major, minor, build, 0);
    }

    /// <summary>
    /// Checks whether the current macOS operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <returns><see langword="true"/> if the current macOS version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsMacOSVersionAtLeast(int major, int minor = 0, int build = 0) {
      if (!IsMacOS())
        return false;

      var version = _GetDarwinVersion() ?? Environment.OSVersion.Version;
      return _IsVersionAtLeast(version, major, minor, build, 0);
    }

    /// <summary>
    /// Checks whether the current operating system version is at least the specified version for the specified platform.
    /// </summary>
    /// <param name="platform">The platform to check (e.g., "Windows", "Linux", "macOS").</param>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <param name="revision">The revision number (default is 0).</param>
    /// <returns><see langword="true"/> if the current OS version is at least the specified version for the platform; otherwise, <see langword="false"/>.</returns>
    public static bool IsOSPlatformVersionAtLeast(string platform, int major, int minor = 0, int build = 0, int revision = 0) =>
      platform switch {
        "Windows" or "windows" => IsWindowsVersionAtLeast(major, minor, build, revision),
        "Linux" or "linux" => IsLinux() && _IsVersionAtLeast(Environment.OSVersion.Version, major, minor, build, revision),
        "macOS" or "macos" or "OSX" or "osx" => IsMacOSVersionAtLeast(major, minor, build),
        "FreeBSD" or "freebsd" => IsFreeBSDVersionAtLeast(major, minor, build, revision),
        "Android" or "android" => IsAndroidVersionAtLeast(major, minor, build, revision),
        "iOS" or "ios" => IsIOSVersionAtLeast(major, minor, build),
        "MacCatalyst" or "maccatalyst" => OperatingSystem.IsMacCatalystVersionAtLeast(major, minor, build), // Provided by Wave 2
        "tvOS" or "tvos" => IsTvOSVersionAtLeast(major, minor, build),
        "watchOS" or "watchos" => IsWatchOSVersionAtLeast(major, minor, build),
        _ => false
      };

    /// <summary>
    /// Checks whether the current tvOS operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <returns><see langword="true"/> if the current tvOS version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsTvOSVersionAtLeast(int major, int minor = 0, int build = 0) {
      if (!IsTvOS())
        return false;

      var version = _GetDarwinVersion();
      return version != null && _IsVersionAtLeast(version, major, minor, build, 0);
    }

    /// <summary>
    /// Checks whether the current watchOS operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <returns><see langword="true"/> if the current watchOS version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsWatchOSVersionAtLeast(int major, int minor = 0, int build = 0) {
      if (!IsWatchOS())
        return false;

      var version = _GetDarwinVersion();
      return version != null && _IsVersionAtLeast(version, major, minor, build, 0);
    }

    /// <summary>
    /// Checks whether the current Windows operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <param name="revision">The revision number (default is 0).</param>
    /// <returns><see langword="true"/> if the current Windows version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsWindowsVersionAtLeast(int major, int minor = 0, int build = 0, int revision = 0)
      => IsWindows() && _IsVersionAtLeast(Environment.OSVersion.Version, major, minor, build, revision);

  }

  private enum OSPlatformType {
    Unknown,
    Windows,
    Linux,
    MacOS,
    FreeBSD,
    Android,
    iOS,
    MacCatalyst,
    tvOS,
    watchOS,
    Browser,
    Wasi
  }

  private static OSPlatformType? _cachedPlatform;

  private static OSPlatformType _GetOSPlatform() {
    if (_cachedPlatform.HasValue)
      return _cachedPlatform.Value;

    var platform = Environment.OSVersion.Platform;
    var result = platform switch {
      PlatformID.Win32NT or PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.WinCE => OSPlatformType.Windows,
      PlatformID.Unix => _DetectUnixPlatform(),
      PlatformID.MacOSX => OSPlatformType.MacOS,
      _ => OSPlatformType.Unknown
    };

    _cachedPlatform = result;
    return result;
  }

  private static OSPlatformType _DetectUnixPlatform() {
    // On Unix-like systems, we need to distinguish between various platforms
    // Each platform has specific filesystem markers or environment variables

    try {
      // Check for WebAssembly environments first (no filesystem or limited access)
      // Browser environment: Emscripten sets specific environment variables
      if (Environment.GetEnvironmentVariable("BROWSER") == "1" || Environment.GetEnvironmentVariable("EMSCRIPTEN") != null)
        return OSPlatformType.Browser;

      // WASI environment: Check for WASI-specific markers
      if (Environment.GetEnvironmentVariable("WASI") == "1" || Environment.GetEnvironmentVariable("__WASI__") != null)
        return OSPlatformType.Wasi;

      // Check for Android (Android is Linux-based but has specific markers)
      // Android has /system/build.prop and ANDROID_ROOT environment variable
      if (IO.File.Exists("/system/build.prop") || Environment.GetEnvironmentVariable("ANDROID_ROOT") != null)
        return OSPlatformType.Android;

      // Check Darwin-based systems by reading SystemVersion.plist
      // This file exists on macOS, iOS, tvOS, watchOS and identifies the exact platform
      var darwinPlatform = _DetectDarwinPlatform();
      if (darwinPlatform != OSPlatformType.Unknown)
        return darwinPlatform;

      // Check for /proc/version (Linux-specific, not Android which we already checked)
      if (IO.File.Exists("/proc/version"))
        return OSPlatformType.Linux;

      // Check for FreeBSD-specific paths
      if (IO.File.Exists("/etc/freebsd-update.conf") || IO.File.Exists("/etc/rc.conf") && IO.File.ReadAllText("/etc/rc.conf").Contains("freebsd"))
        return OSPlatformType.FreeBSD;

      // Default to Linux for unknown Unix platforms
      return OSPlatformType.Linux;
    } catch {
      // If file system checks fail, check for WebAssembly (limited FS access is a hint)
      // But default to Linux as the safest fallback
      return OSPlatformType.Linux;
    }
  }

  private static OSPlatformType _DetectDarwinPlatform() {
    const string systemVersionPath = "/System/Library/CoreServices/SystemVersion.plist";

    // Alternative paths for different Darwin platforms
    string[] possiblePaths = [
      systemVersionPath,
      "/System/Library/CoreServices/ServerVersion.plist",
      "/private/var/mobile/Library/Caches/com.apple.MobileGestalt.plist"
    ];

    foreach (var path in possiblePaths) {
      if (!IO.File.Exists(path))
        continue;

      try {
        var content = IO.File.ReadAllText(path);
        var productName = _ExtractPlistValue(content, "ProductName");

        if (productName != null)
          return _ParseDarwinProductName(productName);
      } catch {
        // Continue to next path
      }
    }

    // iOS/tvOS/watchOS devices have /var/mobile (sandboxed)
    if (IO.Directory.Exists("/var/mobile") || IO.Directory.Exists("/private/var/mobile")) {
      // Distinguish between iOS variants by checking device-specific paths
      if (IO.Directory.Exists("/private/var/db/appletv") || IO.File.Exists("/AppleTV"))
        return OSPlatformType.tvOS;

      if (IO.Directory.Exists("/private/var/mobile/Library/Caches/com.apple.watchkit"))
        return OSPlatformType.watchOS;

      return OSPlatformType.iOS;
    }

    // macOS has CoreServices but not mobile paths
    if (IO.Directory.Exists("/System/Library/CoreServices"))
      return OSPlatformType.MacOS;

    return OSPlatformType.Unknown;
  }

  private static OSPlatformType _ParseDarwinProductName(string productName) {
    // ProductName values from SystemVersion.plist:
    // - macOS: "macOS" or "Mac OS X"
    // - iOS: "iPhone OS"
    // - tvOS: "Apple TVOS" or "tvOS"
    // - watchOS: "Watch OS" or "watchOS"
    // - Mac Catalyst: iOS app on macOS, uses "macOS" but with iOS bundle markers

    if (productName.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
        productName.Equals("iOS", StringComparison.OrdinalIgnoreCase))
      return OSPlatformType.iOS;

    if (productName.Contains("TVOS", StringComparison.OrdinalIgnoreCase) ||
        productName.Contains("Apple TV", StringComparison.OrdinalIgnoreCase))
      return OSPlatformType.tvOS;

    if (productName.Contains("Watch", StringComparison.OrdinalIgnoreCase))
      return OSPlatformType.watchOS;

    if (productName.Contains("Mac", StringComparison.OrdinalIgnoreCase)) {
      // Check for Mac Catalyst (iOS app running on macOS)
      // Mac Catalyst apps have __MACCATALYST__ env or specific bundle markers
      if (Environment.GetEnvironmentVariable("__MACCATALYST__") != null ||
          _IsMacCatalystApp())
        return OSPlatformType.MacCatalyst;

      return OSPlatformType.MacOS;
    }

    return OSPlatformType.Unknown;
  }

  private static bool _IsMacCatalystApp() {
    // Mac Catalyst apps are iOS apps running on macOS
    // They have specific markers in their bundle or Info.plist
    try {
      // Check for UIKit framework presence (iOS/Catalyst apps use UIKit, not AppKit)
      var executablePath = Environment.GetCommandLineArgs().Length > 0
        ? Environment.GetCommandLineArgs()[0]
        : null;

      if (executablePath == null)
        return false;

      // Mac Catalyst apps typically have their bundle in a specific structure
      // and their Info.plist contains LSRequiresIPhoneOS or UIDeviceFamily
      var bundlePath = IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(executablePath));
      if (bundlePath == null)
        return false;

      var infoPlistPath = IO.Path.Combine(bundlePath, "Info.plist");
      if (!IO.File.Exists(infoPlistPath))
        return false;

      var infoPlist = IO.File.ReadAllText(infoPlistPath);
      // UIDeviceFamily key with value 2 or [2] indicates macOS Catalyst
      // LSRequiresIPhoneOS indicates an iOS app (Catalyst or regular)
      return infoPlist.Contains("UIDeviceFamily") || infoPlist.Contains("LSRequiresIPhoneOS");
    } catch {
      return false;
    }
  }

  private static string? _ExtractPlistValue(string plistContent, string key) {
    var keyPattern = $"<key>{key}</key>";
    var keyIndex = plistContent.IndexOf(keyPattern, StringComparison.Ordinal);
    if (keyIndex < 0)
      return null;

    var stringStart = plistContent.IndexOf("<string>", keyIndex, StringComparison.Ordinal);
    if (stringStart < 0)
      return null;

    stringStart += "<string>".Length;
    var stringEnd = plistContent.IndexOf("</string>", stringStart, StringComparison.Ordinal);
    if (stringEnd < 0)
      return null;

    return plistContent.Substring(stringStart, stringEnd - stringStart).Trim();
  }

  private static bool _IsVersionAtLeast(Version current, int major, int minor, int build, int revision) {
    if (current.Major != major)
      return current.Major > major;

    if (current.Minor != minor)
      return current.Minor > minor;

    if (current.Build != build)
      return current.Build > build;

    return current.Revision >= revision;
  }

  private static Version? _cachedAndroidVersion;
  private static bool _androidVersionChecked;

  private static Version? _GetAndroidVersion() {
    if (_androidVersionChecked)
      return _cachedAndroidVersion;

    _androidVersionChecked = true;

    try {
      // Try to read Android version from /system/build.prop
      // Format: ro.build.version.release=13 or ro.build.version.release=13.0.0
      const string buildPropPath = "/system/build.prop";
      if (!IO.File.Exists(buildPropPath))
        return null;

      foreach (var line in IO.File.ReadAllLines(buildPropPath)) {
        if (!line.StartsWith("ro.build.version.release=", StringComparison.Ordinal))
          continue;

        var versionString = line.Substring("ro.build.version.release=".Length).Trim();
        if (Version.TryParse(versionString, out var version)) {
          _cachedAndroidVersion = version;
          return version;
        }

        // Handle single number versions like "13" -> "13.0"
        if (int.TryParse(versionString, out var majorVersion)) {
          _cachedAndroidVersion = new(majorVersion, 0);
          return _cachedAndroidVersion;
        }

        break;
      }
    } catch {
      // Ignore read errors
    }

    return null;
  }

  private static Version? _cachedIOSVersion;
  private static bool _iosVersionChecked;

  private static Version? _GetIOSVersion() {
    if (_iosVersionChecked)
      return _cachedIOSVersion;

    _iosVersionChecked = true;
    _cachedIOSVersion = _GetDarwinVersion();
    return _cachedIOSVersion;
  }

  private static Version? _cachedDarwinVersion;
  private static bool _darwinVersionChecked;

  private static Version? _GetDarwinVersion() {
    if (_darwinVersionChecked)
      return _cachedDarwinVersion;

    _darwinVersionChecked = true;

    try {
      // Try to read version from SystemVersion.plist
      // This file exists on all Darwin platforms (macOS, iOS, tvOS, watchOS)
      const string systemVersionPath = "/System/Library/CoreServices/SystemVersion.plist";

      if (!IO.File.Exists(systemVersionPath))
        return null;

      var content = IO.File.ReadAllText(systemVersionPath);
      var versionString = _ExtractPlistValue(content, "ProductVersion");

      if (versionString != null && Version.TryParse(versionString, out var version)) {
        _cachedDarwinVersion = version;
        return version;
      }
    } catch {
      // Ignore read errors
    }

    return null;
  }

}

} // namespace System

#endif

// Wave 2: IsMacCatalyst was added in .NET 6.0
// This wave provides IsMacCatalyst and IsMacCatalystVersionAtLeast for frameworks where the base OperatingSystem class exists but lacks these methods
#if !SUPPORTS_OPERATING_SYSTEM_PLATFORM_DETECTION_WAVE2

namespace System {

/// <summary>
/// Provides the IsMacCatalyst polyfill extension.
/// </summary>
public static partial class OperatingSystemIsMacCatalystPolyfill {

  private static bool? _isMacCatalyst;

  private static bool _DetectMacCatalyst() {
    if (_isMacCatalyst.HasValue)
      return _isMacCatalyst.Value;

    // Mac Catalyst detection: iOS app running on macOS
    // Check for __MACCATALYST__ environment variable
    if (Environment.GetEnvironmentVariable("__MACCATALYST__") != null) {
      _isMacCatalyst = true;
      return true;
    }

    // Check if running on macOS with iOS bundle markers
    if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
      try {
        // Mac Catalyst apps have specific Info.plist markers
        var executablePath = Environment.GetCommandLineArgs().Length > 0
          ? Environment.GetCommandLineArgs()[0]
          : null;

        if (executablePath != null) {
          var bundlePath = IO.Path.GetDirectoryName(IO.Path.GetDirectoryName(executablePath));
          if (bundlePath != null) {
            var infoPlistPath = IO.Path.Combine(bundlePath, "Info.plist");
            if (IO.File.Exists(infoPlistPath)) {
              var infoPlist = IO.File.ReadAllText(infoPlistPath);
              // UIDeviceFamily or LSRequiresIPhoneOS indicates iOS/Catalyst app
              if (infoPlist.Contains("UIDeviceFamily") || infoPlist.Contains("LSRequiresIPhoneOS")) {
                _isMacCatalyst = true;
                return true;
              }
            }
          }
        }
      } catch {
        // Ignore filesystem errors
      }
    }

    _isMacCatalyst = false;
    return false;
  }

  private static Version? _GetDarwinVersionForCatalyst() {
    try {
      const string systemVersionPath = "/System/Library/CoreServices/SystemVersion.plist";
      if (!IO.File.Exists(systemVersionPath))
        return null;

      var content = IO.File.ReadAllText(systemVersionPath);
      var keyIndex = content.IndexOf("<key>ProductVersion</key>", StringComparison.Ordinal);
      if (keyIndex < 0)
        return null;

      var stringStart = content.IndexOf("<string>", keyIndex, StringComparison.Ordinal);
      if (stringStart < 0)
        return null;

      stringStart += "<string>".Length;
      var stringEnd = content.IndexOf("</string>", stringStart, StringComparison.Ordinal);
      if (stringEnd < 0)
        return null;

      var versionString = content.Substring(stringStart, stringEnd - stringStart).Trim();
      return Version.TryParse(versionString, out var version) ? version : null;
    } catch {
      return null;
    }
  }

  extension(OperatingSystem) {

    /// <summary>
    /// Indicates whether the current application is running on Mac Catalyst.
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on Mac Catalyst; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMacCatalyst() => _DetectMacCatalyst();

    /// <summary>
    /// Checks whether the current Mac Catalyst operating system version is at least the specified version.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number (default is 0).</param>
    /// <param name="build">The build number (default is 0).</param>
    /// <returns><see langword="true"/> if the current Mac Catalyst version is at least the specified version; otherwise, <see langword="false"/>.</returns>
    public static bool IsMacCatalystVersionAtLeast(int major, int minor = 0, int build = 0) {
      if (!IsMacCatalyst())
        return false;

      var version = _GetDarwinVersionForCatalyst();
      if (version == null)
        return false;

      if (version.Major != major)
        return version.Major > major;
      if (version.Minor != minor)
        return version.Minor > minor;
      return version.Build >= build;
    }

  }

}

} // namespace System

#endif

// Wave 3: IsWasi was added in .NET 8.0
// This wave provides IsWasi for frameworks where the OperatingSystem class exists but lacks this method
#if !SUPPORTS_OPERATING_SYSTEM_PLATFORM_DETECTION_WAVE3

namespace System {

/// <summary>
/// Provides the IsWasi polyfill extension.
/// </summary>
public static partial class OperatingSystemIsWasiPolyfill {

  private static bool? _isWasi;

  private static bool _DetectWasi() {
    if (_isWasi.HasValue)
      return _isWasi.Value;

    // WASI detection: WebAssembly System Interface environment
    // Check for WASI-specific environment variables
    if (Environment.GetEnvironmentVariable("WASI") == "1" ||
        Environment.GetEnvironmentVariable("__WASI__") != null ||
        Environment.GetEnvironmentVariable("WASI_SDK_PATH") != null) {
      _isWasi = true;
      return true;
    }

    // Check if running in a WASI runtime by examining architecture and runtime info
    // WASI runs on wasm32 architecture
    try {
      var runtimeId = Environment.GetEnvironmentVariable("DOTNET_RUNTIME_ID");
      if (runtimeId != null && runtimeId.Contains("wasi", StringComparison.OrdinalIgnoreCase)) {
        _isWasi = true;
        return true;
      }
    } catch {
      // Ignore errors
    }

    _isWasi = false;
    return false;
  }

  extension(OperatingSystem) {

    /// <summary>
    /// Indicates whether the current application is running on WASI (WebAssembly System Interface).
    /// </summary>
    /// <returns><see langword="true"/> if the current application is running on WASI; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWasi() => _DetectWasi();

  }

}

} // namespace System

#endif
