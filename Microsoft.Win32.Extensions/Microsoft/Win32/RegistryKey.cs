#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;

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
    // TIP: use a stack DEPTH_FIRST_ALGORITHM
    foreach (var keyName in @this.GetSubKeyNames()) {
      var fullKeyName = subKeyName + '\\' + keyName;
      yield return baseKey.OpenSubKey(fullKeyName);
    }
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
    RegistryKey registryKey;
    switch (length == -1 ? keyName.ToUpper(CultureInfo.InvariantCulture) : keyName.Substring(0, length).ToUpper(CultureInfo.InvariantCulture)) {
      case "HKEY_CLASSES_ROOT":
        registryKey = Registry.ClassesRoot;
        break;
      case "HKEY_CURRENT_CONFIG":
        registryKey = Registry.CurrentConfig;
        break;
      case "HKEY_CURRENT_USER":
        registryKey = Registry.CurrentUser;
        break;
      case "HKEY_LOCAL_MACHINE":
        registryKey = Registry.LocalMachine;
        break;
      case "HKEY_PERFORMANCE_DATA":
        registryKey = Registry.PerformanceData;
        break;
      case "HKEY_USERS":
        registryKey = Registry.Users;
        break;
      default: throw new ArgumentException("Invalid key name!", nameof(keyName));
    }

    subKeyName = length == -1 || length == keyName.Length ? string.Empty : keyName.Substring(length + 1, keyName.Length - length - 1);
    return registryKey;
  }
  
}