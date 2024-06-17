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
using System.Collections.Generic;
using System.Globalization;
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Microsoft.Win32;

#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static partial class RegistryKeyExtensions {
  /// <summary>
  /// </summary>
  /// <param name="this"></param>
  /// <returns></returns>
  public static string GetSubKeyPath(this RegistryKey @this) {
    var result = @this.Name;
    var index = result.IndexOf('\\');
    return index >= 0 ? result[(index + 1)..] : result;
  }

  /// <summary>
  /// </summary>
  /// <param name="this"></param>
  /// <returns></returns>
  public static string GetSubKeyName(this RegistryKey @this) {
    var result = @this.Name;
    var index = result.LastIndexOf('\\');
    return index >= 0 ? result[(index + 1)..] : result;
  }

  /// <summary>
  /// </summary>
  /// <param name="this"></param>
  /// <param name="recursive"></param>
  /// <returns></returns>

  // TODO: 
  public static IEnumerable<RegistryKey> GetSubKeys(this RegistryKey @this, bool recursive = false) {
    var baseKey = GetBaseKeyFromKeyName(@this.Name, out var subKeyName);
    var stack = new Stack<string>();
    stack.Push(subKeyName);

    do {
      var currentSubKey = stack.Pop();

      using var currentKey = baseKey.OpenSubKey(currentSubKey);
      if (currentKey == null)
        continue;

      foreach (var keyName in currentKey.GetSubKeyNames()) {
        var fullKeyName = $"{currentSubKey}\\{keyName}";
        yield return baseKey.OpenSubKey(fullKeyName);

        if (recursive)
          stack.Push(fullKeyName);
      }
    } while (stack.Count > 0);
  }

  /// <summary>
  /// </summary>
  /// <param name="keyName"></param>
  /// <param name="subKeyName"></param>
  /// c#
  /// <returns></returns>
  public static RegistryKey GetBaseKeyFromKeyName(string keyName, out string subKeyName) {
    if (keyName == null)
      throw new ArgumentNullException(nameof(keyName));

    var length = keyName.IndexOf('\\');
    var registryKey = (length == -1 ? keyName.ToUpper(CultureInfo.InvariantCulture) : keyName[..length].ToUpper(CultureInfo.InvariantCulture)) switch {
      "HKEY_CLASSES_ROOT" or "HKCM" => Registry.ClassesRoot,
      "HKEY_CURRENT_CONFIG" or "HKCC" => Registry.CurrentConfig,
      "HKEY_CURRENT_USER" or "HKCU" => Registry.CurrentUser,
      "HKEY_LOCAL_MACHINE" or "HKLM" => Registry.LocalMachine,
      "HKEY_PERFORMANCE_DATA" or "HKPD" => Registry.PerformanceData,
      "HKEY_USERS" or "HKU" => Registry.Users,
      _ => throw new ArgumentException("Invalid key name!", nameof(keyName))
    };

    subKeyName = length == -1 || length == keyName.Length ? string.Empty : keyName.Substring(length + 1, keyName.Length - length - 1);
    return registryKey;
  }
}
