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

#if !SUPPORTS_COMPAREINFO_GETHASHCODE_COMPAREOPTIONS

using System.Runtime.CompilerServices;
using Guard;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System.Globalization;

public static partial class CompareInfoPolyfills {
  /// <param name="this">This <see cref="CompareInfo"/></param>
  extension(CompareInfo @this)
  {
    /// <summary>
    /// Gets the hash code for a string based on specified comparison options.
    /// </summary>
    /// <param name="source">The string whose hash code is to be returned.</param>
    /// <param name="options">A value that determines how strings are compared.</param>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> contains an invalid combination.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(string source, CompareOptions options) {
      Against.ThisIsNull(@this);
      ArgumentNullException.ThrowIfNull(source);
      
      // Handle ordinal comparisons directly
      if ((options & CompareOptions.Ordinal) != 0) {
        if (options != CompareOptions.Ordinal)
          AlwaysThrow.ArgumentException(nameof(options), "Ordinal cannot be combined with other options.");

        return source.GetHashCode();
      }

      if ((options & CompareOptions.OrdinalIgnoreCase) != 0) {
        if (options != CompareOptions.OrdinalIgnoreCase)
          AlwaysThrow.ArgumentException(nameof(options), "OrdinalIgnoreCase cannot be combined with other options.");

        return StringComparer.OrdinalIgnoreCase.GetHashCode(source);
      }

      // For culture-sensitive comparison, we need to produce a hash code that's consistent
      // with the CompareInfo's Compare method. We use the culture's StringComparer.
      var culture = @this.Name == string.Empty
        ? CultureInfo.InvariantCulture
        : CultureInfo.GetCultureInfo(@this.Name);

      var ignoreCase = (options & CompareOptions.IgnoreCase) != 0;

      // Create appropriate StringComparer for the culture
      StringComparer comparer;
      if (culture.Equals(CultureInfo.InvariantCulture))
        comparer = ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
      else if (culture.Equals(CultureInfo.CurrentCulture))
        comparer = ignoreCase ? StringComparer.CurrentCultureIgnoreCase : StringComparer.CurrentCulture;
      else
        // For other cultures, we use a custom approach: normalize and hash
        return _GetCultureSensitiveHashCode(source, culture, options);

      return comparer.GetHashCode(source);
    }

    private static int _GetCultureSensitiveHashCode(string source, CultureInfo culture, CompareOptions options) {
      // For cultures that don't have a direct StringComparer, we create a hash
      // by comparing against a normalized form. This is a best-effort implementation.
      // Note: This may not be 100% compatible with the native implementation's hash codes,
      // but it will be consistent within the same runtime.

      var ignoreCase = (options & CompareOptions.IgnoreCase) != 0;

      // Normalize for case if needed
      var normalized = ignoreCase ? source.ToUpper(culture) : source;

      // Combine culture name into hash for culture-specific uniqueness
      unchecked {
        var hash = 17;
        hash = hash * 31 + normalized.GetHashCode();
        hash = hash * 31 + culture.Name.GetHashCode();
        if ((options & CompareOptions.IgnoreNonSpace) != 0)
          hash = hash * 31 + 1;
        if ((options & CompareOptions.IgnoreSymbols) != 0)
          hash = hash * 31 + 2;
        if ((options & CompareOptions.IgnoreKanaType) != 0)
          hash = hash * 31 + 4;
        if ((options & CompareOptions.IgnoreWidth) != 0)
          hash = hash * 31 + 8;
        return hash;
      }
    }
  }
}

#endif
