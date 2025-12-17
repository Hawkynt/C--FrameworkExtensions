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

#if !SUPPORTS_ENVIRONMENT_ISPRIVILEGEDPROCESS

using System.Runtime.CompilerServices;
using System.Reflection;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

public static partial class EnvironmentPolyfills {

  extension(Environment) {

    /// <summary>
    /// Gets a value indicating whether the current process is running with elevated privileges.
    /// </summary>
    /// <value><see langword="true"/> if the current process is running as administrator; otherwise, <see langword="false"/>.</value>
    // TODO: cache result for performance in repeated calls?
    public static bool IsPrivilegedProcess {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _IsWindows() ? _IsWindowsPrivileged() : _IsUnixPrivileged();
    }

    private static bool _IsWindows()
      => Environment.OSVersion.Platform == PlatformID.Win32NT
        || Environment.OSVersion.Platform == PlatformID.Win32S
        || Environment.OSVersion.Platform == PlatformID.Win32Windows
        || Environment.OSVersion.Platform == PlatformID.WinCE;

    private static bool _IsWindowsPrivileged() {
      // Use reflection to avoid compile-time dependency on Windows-only types
      var windowsIdentityType = Type.GetType("System.Security.Principal.WindowsIdentity, mscorlib")
        ?? Type.GetType("System.Security.Principal.WindowsIdentity, System.Security.Principal.Windows");

      var getCurrentMethod = windowsIdentityType?.GetMethod("GetCurrent", Type.EmptyTypes);
      var identity = getCurrentMethod?.Invoke(null, null);
      if (identity == null)
        return false;

      try {
        var windowsPrincipalType = Type.GetType("System.Security.Principal.WindowsPrincipal, mscorlib")
          ?? Type.GetType("System.Security.Principal.WindowsPrincipal, System.Security.Principal.Windows");

        if (windowsPrincipalType == null)
          return false;

        var principal = Activator.CreateInstance(windowsPrincipalType, identity);
        if (principal == null)
          return false;

        var isInRoleMethod = windowsPrincipalType.GetMethod("IsInRole", [typeof(int)]);
        if (isInRoleMethod == null)
          return false;

        // WindowsBuiltInRole.Administrator = 544
        return (bool)(isInRoleMethod.Invoke(principal, [544]) ?? false);
      } finally {
        (identity as IDisposable)?.Dispose();
      }
    }

    private static bool _IsUnixPrivileged()
      => Environment.UserName == "root"
        || (Environment.GetEnvironmentVariable("EUID") ?? Environment.GetEnvironmentVariable("UID")) == "0";

  }

}

#endif
