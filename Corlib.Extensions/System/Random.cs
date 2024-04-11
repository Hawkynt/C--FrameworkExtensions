#region (c)2010-2042 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Text;
using qword = System.UInt64;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

using Collections.Generic;
using Guard;
using Linq;
using Security.Cryptography;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  static partial class RandomExtensions {

  /// <summary>
  /// Creates a random number between the given limits.
  /// </summary>
  /// <param name="this">This Random.</param>
  /// <param name="minimumInclusive">The min value.</param>
  /// <param name="maximumExclusive">The max value.</param>
  /// <returns>A value between the given boundaries</returns>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static double NextDouble(this Random @this, double minimumInclusive, double maximumExclusive) => @this.NextDouble() * (maximumExclusive - minimumInclusive) + minimumInclusive;

  /// <summary>
  /// Represents settings for generating passwords.
  /// </summary>
  public readonly struct PasswordSettings {

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordSettings"/> struct with default values.
    /// </summary>
    public PasswordSettings() { }

    /// <summary>
    /// Indicates whether lowercase letters are allowed in the password.
    /// </summary>
    public bool AllowLowerCaseLetters { get; init; } = true;

    /// <summary>
    /// Indicates whether uppercase letters are allowed in the password.
    /// </summary>
    public bool AllowUpperCaseLetters { get; init; } = true;

    /// <summary>
    /// Indicates whether numbers are allowed in the password.
    /// </summary>
    public bool AllowNumbers { get; init; } = true;

    /// <summary>
    /// Indicates whether special characters are allowed in the password.
    /// </summary>
    public bool AllowSpecialCharacters { get; init; } = true;

    /// <summary>
    /// Indicates whether duplicate characters are to be avoided in the password.
    /// </summary>
    public bool AvoidDuplicates { get; init; } = false;

    /// <summary>
    /// Indicates whether visually similar characters are to be avoided in the password.
    /// </summary>
    public bool AvoidVisuallySimilarCharacters { get; init; } = false;

    /// <summary>
    /// Indicates whether to prefer pronounceable patterns in the password.
    /// </summary>
    public bool PreferPronouncable { get; init; } = false;

    /// <summary>
    /// Specifies the minimum length of the generated password.
    /// </summary>
    public byte MinimumLength { get; init; } = 8;

    /// <summary>
    /// Specifies the maximum length of the generated password.
    /// </summary>
    public byte MaximumLength { get; init; } = 14;

    /// <summary>
    /// Specifies a custom set of allowed characters for the password. If null or empty, default character sets are used.
    /// </summary>
    /// <remarks>Overrides the other settings regarding used characters</remarks>
    public string AllowedCharacterSet { get; init; } = null;

    internal const string VOWELS = "aA4eE3iI1!yYoO0uU";
    internal const string CONSONANTS = "bBcCdDfFgGhHjJkKlL1mMnNpPqQrRsS5$tTvVwWxXyYzZ";
    internal const string NUMBERS = "0123456789";
    internal const string LOWER_LETTERS = "abcdefghijklmnopqrstuvwxyz";
    internal const string UPPER_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    internal const string SPECIAL = "!@#$%^&*()-_=+[]{}|;:'\",.<>?~";
    internal const string HARD_TO_DISTINGUISH = "il1|!0OQ:;.,";
  }

  /// <summary>
  /// Generates a password based on provided settings.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance to use for random number generation.</param>
  /// <param name="settings">The <see cref="PasswordSettings"/> to use for password generation.</param>
  /// <param name="useStrongRandomization">Indicates whether to use a strong random number generator (if set to <see langword="true"/> the <paramref name="this"/> is ignored, entirely).</param>
  /// <returns>A randomly generated password string.</returns>
  /// <exception cref="InvalidOperationException">Thrown when no characters are available to select for the generated password.</exception>
  /// <example>
  /// <code>
  /// // Example usage with custom settings:
  /// var random = new Random();
  /// var settings = new PasswordSettings
  /// {
  ///     AllowLowerCaseLetters = true,
  ///     AllowUpperCaseLetters = true,
  ///     AllowNumbers = true,
  ///     AllowSpecialCharacters = true,
  ///     AvoidDuplicates = true,
  ///     AvoidVisuallySimilarCharacters = true,
  ///     PreferPronouncable = true,
  ///     MinimumLength = 10,
  ///     MaximumLength = 16
  /// };
  /// string customPassword = random.GeneratePassword(settings, true);
  /// Console.WriteLine(customPassword);
  ///
  /// // Example usage without specifying settings (using defaults):
  /// string defaultPassword = random.GeneratePassword();
  /// Console.WriteLine(defaultPassword);
  /// </code>
  /// </example>
  public static string GeneratePassword(this Random @this, PasswordSettings? settings = null, bool useStrongRandomization = false) {
    var localSettings = settings ?? new();
    
    List<char> pool = new(128);

    if (localSettings.AllowedCharacterSet.IsNotNullOrEmpty())
      pool.AddRange(localSettings.AllowedCharacterSet);
    else {
      if (localSettings.AllowNumbers)
        pool.AddRange(PasswordSettings.NUMBERS);
      if (localSettings.AllowLowerCaseLetters)
        pool.AddRange(PasswordSettings.LOWER_LETTERS);
      if (localSettings.AllowUpperCaseLetters)
        pool.AddRange(PasswordSettings.UPPER_LETTERS);
      if (localSettings.AllowSpecialCharacters)
        pool.AddRange(PasswordSettings.SPECIAL);
      if (localSettings.AvoidVisuallySimilarCharacters)
        pool.RemoveRange(PasswordSettings.HARD_TO_DISTINGUISH);
    }

    Func<int, int> getNext = useStrongRandomization ? new RNGCryptoServiceProvider().Next : @this.Next;
    var length = localSettings.MinimumLength >= localSettings.MaximumLength 
      ? localSettings.MinimumLength 
      : localSettings.MinimumLength + getNext(localSettings.MaximumLength - localSettings.MinimumLength + 1)
      ;

    var result = new char[length];

    if (localSettings.PreferPronouncable) {
      var vowels = pool.Where(c => PasswordSettings.VOWELS.Contains(c)).ToList();
      var consonants = pool.Where(c => PasswordSettings.CONSONANTS.Contains(c)).ToList();
      var special = pool.Except(vowels).Except(consonants).ToList();
      GeneratePronounceable(localSettings.AvoidDuplicates, getNext, vowels, consonants, special, result);
    } else
      GenerateNormal(localSettings.AvoidDuplicates, getNext, pool, result);

    return result.ToStringInstance();

    static void RemoveFromPoolIfNotEmpty(IList<char> pool, char entry) {
      if (pool.Count > 1)
        pool.RemoveEvery(entry);
    }

    static void GeneratePronounceable(bool avoidDuplicates, Func<int, int> next, List<char> vowels, List<char> consonants, List<char> special, char[] result) {
      Func<IList<char>, char> func = avoidDuplicates switch {
        true => p => {
          var c = p[next(p.Count)];
          RemoveFromPoolIfNotEmpty(vowels, c);
          RemoveFromPoolIfNotEmpty(consonants, c);
          RemoveFromPoolIfNotEmpty(special, c);
          return c;
        },
        _ => p => p[next(p.Count)]
      };

      // Randomly decide if starting with vowel or consonant
      var isVowel = next(2) < 1;
      for (var i = 0; i < result.Length; isVowel = !isVowel, ++i )
        result[i] = func(i switch {
          _ when i == result.Length - 1 && special.Any() => special,
          _ when isVowel && vowels.Any() => vowels,
          _ when !isVowel && consonants.Any() => consonants,
          _ when vowels.Any() => vowels,
          _ when consonants.Any() => consonants,
          _ when special.Any() => special,
          _ => throw new InvalidOperationException("No characters available to select.")
        });
    }

    static void GenerateNormal(bool avoidDuplicates, Func<int, int> next, List<char> pool, char[] result) {
      Func<char> func = avoidDuplicates switch {
        true => () => {
          var c = pool[next(pool.Count)];
          RemoveFromPoolIfNotEmpty(pool, c);
          return c;
        }
        ,
        _ => () => pool[next(pool.Count)]
      };

      for (var i = 0; i < result.Length; ++i)
        result[i] = func();
    }
  }

  /// <summary>
  /// Simulates flipping a coin using the provided <see cref="Random"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the coin flip.</param>
  /// <returns><see langword="true"/> for heads and <see langword="false"/> for tails.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// bool flipResult = random.FlipACoin();
  /// Console.WriteLine($"Coin flip result: {(flipResult ? "Heads" : "Tails")}");
  /// </code>
  /// This example simulates a coin flip and prints whether the result is heads or tails.
  /// </example>
  public static bool FlipACoin(this Random @this) {
    Against.ThisIsNull(@this);

    return @this.Next(2) <= 0;
  }

  /// <summary>
  /// Rolls a dice of a specified number of sides using the provided <see cref="Random"/> instance.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the dice roll.</param>
  /// <param name="count">The number of sides of the dice (default is 6).</param>
  /// <returns>A <see cref="byte"/> representing the result of the dice roll, ranging from 1 to the number of sides inclusive.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of sides is less than 1.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// byte rollResult = random.RollADice();
  /// Console.WriteLine($"Result of rolling a 6-sided dice: {rollResult}");
  /// 
  /// rollResult = random.RollADice(20);
  /// Console.WriteLine($"Result of rolling a 20-sided dice: {rollResult}");
  /// </code>
  /// This example demonstrates rolling a standard 6-sided dice and a 20-sided dice, printing the results to the console.
  /// </example>
  public static byte RollADice(this Random @this, byte count = 6) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);

    return (byte)(@this.Next(count) + 1);
  }

#if !SUPPORTS_RANDOM_NEXTINT64

  /// <summary>Returns a non-negative random integer.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
  public static long NextInt64(this Random @this) {
    Against.ThisIsNull(@this);

    return (long)(@this.NextDouble() * ulong.MaxValue);
  }

  /// <summary>Returns a non-negative random integer that is less than the specified maximum.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
  /// <returns>
  /// A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
  /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
  public static long NextInt64(this Random @this, long maxValue) {
    Against.ThisIsNull(@this);
    Against.NegativeValues(maxValue);

    return (long)(@this.NextDouble() * maxValue);
  }

  /// <summary>Returns a random integer that is within a specified range.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
  /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
  /// <returns>
  /// A 64-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
  /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
  public static long NextInt64(this Random @this, long minValue, long maxValue) {
    Against.ThisIsNull(@this);
    Against.False(minValue <= maxValue);

    return minValue + (long)(@this.NextDouble() * (maxValue - minValue));
  }

#endif

}
