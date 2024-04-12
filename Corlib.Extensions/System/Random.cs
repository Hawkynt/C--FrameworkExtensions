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

#if SUPPORTS_INLINING
using System.Runtime.CompilerServices;
#endif
using System.Text;
using System.Collections.Generic;
using Guard;
using System.Linq;
using System.Security.Cryptography;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace System;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  static partial class RandomExtensions {

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
  /// bool flipResult = random.GetBoolean();
  /// Console.WriteLine($"Coin flip result: {(flipResult ? "Heads" : "Tails")}");
  /// </code>
  /// This example simulates a coin flip and prints whether the result is heads or tails.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static bool GetBoolean(this Random @this) {
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
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static byte RollADice(this Random @this, byte count = 6) {
    Against.ThisIsNull(@this);
    Against.CountBelowOrEqualZero(count);

    return (byte)(@this.Next(count) + 1);
  }
  
  /// <summary>
  /// Generates a random value for the specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type for which to generate a random value.</typeparam>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <returns>A random value of type <typeparamref name="T"/> (this does include <see langword="null"/>).</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// string randomString = random.GetRandomValueFor&lt;string&gt;();
  /// Console.WriteLine($"Random string: {randomString}");
  /// </code>
  /// This example demonstrates generating random values for int, double, and bool types.
  /// </example>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static T GetValueFor<T>(this Random @this) => (T)TypeExtensions.GetRandomValueFor(typeof(T), true, @this);

  // When higher -> less likely; must be > 6
  private const int _SPECIAL_VALUE_LIKELIHOOD = 10;

  /// <summary>
  /// Generates a random string of a specified maximum length.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random string.</param>
  /// <param name="maxLength">The maximum length of the generated string. The actual length can be less than the maximum specified.</param>
  /// <param name="allowNull">Whether the return of <see langword="null"/> is allowed</param>
  /// <param name="allowEmpty">Whether the return of <see cref="string.Empty"/> is allowed</param>
  /// <returns>A random string of up to <paramref name="maxLength"/> characters with at least one character.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxLength"/> is less than 1.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// string randomString = random.GetRandomString(10);
  /// Console.WriteLine($"Random string: {randomString}");
  /// </code>
  /// This example generates and prints a random string of up to 10 characters in length.
  /// </example>
  /// <remarks>
  /// The method creates a string consisting of purely random characters.
  /// The exact length of the string is determined randomly and can be up to <paramref name="maxLength"/>.
  /// Note: The string may contain invalid surrogates, non-printable characters and control sequences.
  /// </remarks>
  public static string GetString(this Random @this, int maxLength, bool allowNull = false, bool allowEmpty = false) {
    Against.ThisIsNull(@this);
    Against.NegativeValuesAndZero(maxLength);

    switch (@this.Next(_SPECIAL_VALUE_LIKELIHOOD)){
      case 0 when allowNull:
        return null;
      case 1 when allowEmpty:
        return string.Empty;
      default:
        var length = maxLength <= 1 ? 1 : @this.Next(1, maxLength);
        var bytes = new byte[length * 2];
        @this.NextBytes(bytes);
        return Encoding.Unicode.GetString(bytes);
    }
  }

  /// <summary>
  /// Generates a random unsigned byte (<see cref="System.Byte"/>).
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random byte.</param>
  /// <returns>A random unsigned byte ranging from 0 to 255.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// byte randomByte = random.GetUInt8();
  /// Console.WriteLine($"Random byte: {randomByte}");
  /// </code>
  /// This example generates a random byte and prints it.
  /// </example>
  public static byte GetUInt8(this Random @this) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => byte.MaxValue,
      _ => (byte)@this.Next(byte.MaxValue)
    };
  }

  /// <summary>
  /// Generates a random unsigned short (<see cref="System.UInt16"/>).
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random ushort.</param>
  /// <returns>A random unsigned short ranging from 0 to 65535.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// ushort randomUShort = random.GetUInt16();
  /// Console.WriteLine($"Random ushort: {randomUShort}");
  /// </code>
  /// This example generates a random ushort and prints it.
  /// </example>
  public static ushort GetUInt16(this Random @this) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => ushort.MaxValue,
      _ => (ushort)@this.Next(ushort.MaxValue)
    };
  }

  /// <summary>
  /// Generates a random unsigned integer (<see cref="System.UInt32"/>).
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random uint.</param>
  /// <returns>A random unsigned integer ranging from 0 to 4294967295.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// uint randomUInt = random.GetUInt32();
  /// Console.WriteLine($"Random uint: {randomUInt}");
  /// </code>
  /// This example generates a random uint and prints it.
  /// </example>
  public static uint GetUInt32(this Random @this) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => uint.MaxValue,
      _ => (uint)@this.NextInt64(uint.MaxValue)
    };
  }

  /// <summary>
  /// Generates a random unsigned long (<see cref="System.UInt64"/>).
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random ulong.</param>
  /// <returns>A random unsigned long ranging from 0 to 18446744073709551615.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// ulong randomULong = random.GetUInt64();
  /// Console.WriteLine($"Random ulong: {randomULong}");
  /// </code>
  /// This example generates a random ulong and prints it.
  /// </example>
  public static ulong GetUInt64(this Random @this) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => ulong.MaxValue,
      _ => (ulong)(@this.NextDouble() * ulong.MaxValue)
    };
  }

  /// <summary>
  /// Generates a random signed byte (<see cref="System.SByte"/>), with an option to restrict output to only positive values.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random sbyte.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range is from 0 to 127. If <see langword="false"/>, the range is from -128 to 127.</param>
  /// <returns>A random signed byte, potentially restricted to positive values based on the <paramref name="onlyPositive"/> parameter.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// sbyte randomSByte = random.GetInt8();
  /// Console.WriteLine($"Random sbyte: {randomSByte}");
  ///
  /// sbyte positiveSByte = random.GetInt8(onlyPositive: true);
  /// Console.WriteLine($"Random positive sbyte: {positiveSByte}");
  /// </code>
  /// This example generates a random sbyte and a random positive sbyte, printing both.
  /// </example>
  public static sbyte GetInt8(this Random @this, bool onlyPositive = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => sbyte.MaxValue,
      2 when !onlyPositive=> sbyte.MinValue,
      _ when onlyPositive => (sbyte)@this.Next(sbyte.MaxValue),
      _ => (sbyte)@this.Next(byte.MaxValue + 1)
    };
  }

  /// <summary>
  /// Generates a random signed short (<see cref="System.Int16"/>), with an option to restrict output to only positive values.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random short.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range is from 0 to 32767. If <see langword="false"/>, the range is from -32768 to 32767.</param>
  /// <returns>A random signed short, potentially restricted to positive values based on the <paramref name="onlyPositive"/> parameter.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// short randomShort = random.GetInt16();
  /// Console.WriteLine($"Random short: {randomShort}");
  ///
  /// short positiveShort = random.GetInt16(onlyPositive: true);
  /// Console.WriteLine($"Random positive short: {positiveShort}");
  /// </code>
  /// This example generates a random short and a random positive short, printing both.
  /// </example>
  public static short GetInt16(this Random @this, bool onlyPositive = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => short.MaxValue,
      2 when !onlyPositive => short.MinValue,
      _ when onlyPositive => (short)@this.Next(short.MaxValue),
      _ => (short)@this.Next(ushort.MaxValue + 1)
    };
  }

  /// <summary>
  /// Generates a random signed integer (<see cref="System.Int32"/>), with an option to restrict output to only positive values.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random integer.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range is from 0 to 2147483647. If <see langword="false"/>, the range is from -2147483648 to 2147483647.</param>
  /// <returns>A random signed integer, potentially restricted to positive values based on the <paramref name="onlyPositive"/> parameter.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// int randomInt = random.GetInt32();
  /// Console.WriteLine($"Random int: {randomInt}");
  ///
  /// int positiveInt = random.GetInt32(onlyPositive: true);
  /// Console.WriteLine($"Random positive int: {positiveInt}");
  /// </code>
  /// This example generates a random integer and a random positive integer, printing both.
  /// </example>
  public static int GetInt32(this Random @this, bool onlyPositive = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => int.MaxValue,
      2 when !onlyPositive => int.MinValue,
      _ when onlyPositive => @this.Next(),
      _ => (int)@this.NextInt64(uint.MaxValue + 1L)
    };
  }

  /// <summary>
  /// Generates a random signed long (<see cref="System.Int64"/>), with an option to restrict output to only positive values.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random long.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range is from 0 to 9223372036854775807. If <see langword="false"/>, the range is from -9223372036854775808 to 9223372036854775807.</param>
  /// <returns>A random signed long, potentially restricted to positive values based on the <paramref name="onlyPositive"/> parameter.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// long randomLong = random.GetInt64();
  /// Console.WriteLine($"Random long: {randomLong}");
  ///
  /// long positiveLong = random.GetInt64(onlyPositive: true);
  /// Console.WriteLine($"Random positive long: {positiveLong}");
  /// </code>
  /// This example generates a random long and a random positive long, printing both.
  /// </example>
  public static long GetInt64(this Random @this, bool onlyPositive = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => long.MaxValue,
      2 when !onlyPositive => long.MinValue,
      _ when onlyPositive => @this.NextInt64(),
      _ => (long)(@this.NextDouble() * ((double)long.MaxValue - long.MinValue) + long.MinValue)
    };
  }

  /// <summary>
  /// Generates a random floating-point number (<see cref="System.Single"/>) with options to exclude negative values, NaN (Not a Number), and Infinity.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random float.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range excludes negative numbers.</param>
  /// <param name="noNaN">Specifies whether to exclude NaN values from the possible outputs.</param>
  /// <param name="noInfinity">Specifies whether to exclude positive and negative infinity from the possible outputs.</param>
  /// <returns>A random float potentially restricted based on the parameters provided.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// float randomFloat = random.GetFloat();
  /// Console.WriteLine($"Random float: {randomFloat}");
  ///
  /// float positiveFloat = random.GetFloat(onlyPositive: true);
  /// Console.WriteLine($"Random positive float: {positiveFloat}");
  ///
  /// float normalFloat = random.GetFloat(noNaN: true, noInfinity: true);
  /// Console.WriteLine($"Random float with no NaN or Infinity: {normalFloat}");
  /// </code>
  /// This example generates three different floats:
  /// - A completely random float.
  /// - A random positive float.
  /// - A random float that is neither NaN nor Infinity.
  /// </example>
  public static float GetFloat(this Random @this, bool onlyPositive = false, bool noNaN = false, bool noInfinity = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => float.MaxValue,
      2 when !onlyPositive => float.MinValue,
      3 when !noNaN && !onlyPositive => float.NaN,
      4 when !noInfinity && !onlyPositive => float.NegativeInfinity,
      5 when !noInfinity => float.PositiveInfinity,
      _ when onlyPositive => (float)(@this.NextDouble() * float.MaxValue),
      _ => (float)(@this.NextDouble() * (@this.GetBoolean() ? float.MaxValue : -float.MaxValue))
    };
  }

  /// <summary>
  /// Generates a random double-precision floating-point number (<see cref="System.Double"/>), with options to exclude negative values, NaN (Not a Number), and Infinity.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random double.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, negative values are excluded, making the range from 0 to <see cref="Double.MaxValue"/>.</param>
  /// <param name="noNaN">Specifies whether to prevent the generation of NaN values. This is typically only relevant in certain computational scenarios where operations could yield NaN.</param>
  /// <param name="noInfinity">Specifies whether to prevent the generation of Infinity values. This is important when operations that could produce infinity (like division by zero) are involved.</param>
  /// <returns>A random double potentially restricted based on the specified parameters.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// double randomDouble = random.GetDouble();
  /// Console.WriteLine($"Random double: {randomDouble}");
  ///
  /// double positiveDouble = random.GetDouble(onlyPositive: true);
  /// Console.WriteLine($"Random positive double: {positiveDouble}");
  ///
  /// double normalDouble = random.GetDouble(noNaN: true, noInfinity: true);
  /// Console.WriteLine($"Random double with no NaN or Infinity: {normalDouble}");
  /// </code>
  /// This example generates three different doubles:
  /// - A completely random double.
  /// - A random positive double.
  /// - A random double that is neither NaN nor Infinity.
  /// </example>
  public static double GetDouble(this Random @this, bool onlyPositive = false, bool noNaN = false, bool noInfinity = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => 0,
      1 => double.MaxValue,
      2 when !onlyPositive => double.MinValue,
      3 when !noNaN && !onlyPositive => double.NaN,
      4 when !noInfinity && !onlyPositive => double.NegativeInfinity,
      5 when !noInfinity => double.PositiveInfinity,
      _ when onlyPositive => @this.NextDouble() * double.MaxValue,
      _ => @this.NextDouble() * (@this.GetBoolean() ? double.MaxValue : -double.MaxValue)
    };
  }

  /// <summary>
  /// Generates a random decimal value (<see cref="System.Decimal"/>), with an option to exclude negative values.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random decimal.</param>
  /// <param name="onlyPositive">Specifies whether to generate only positive values. If set to <see langword="true"/>, the range is from 0 to <see cref="Decimal.MaxValue"/>.</param>
  /// <returns>A random decimal, potentially restricted to positive values based on the <paramref name="onlyPositive"/> parameter.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// decimal randomDecimal = random.GetDecimal();
  /// Console.WriteLine($"Random decimal: {randomDecimal}");
  ///
  /// decimal positiveDecimal = random.GetDecimal(onlyPositive: true);
  /// Console.WriteLine($"Random positive decimal: {positiveDecimal}");
  /// </code>
  /// This example generates a random decimal and a random positive decimal, printing both to demonstrate the output.
  /// </example>
  public static decimal GetDecimal(this Random @this, bool onlyPositive = false) {
    Against.ThisIsNull(@this);

    return @this.Next(_SPECIAL_VALUE_LIKELIHOOD) switch {
      0 => decimal.Zero,
      1 => decimal.MaxValue,
      2 when !onlyPositive => decimal.MinValue,
      3 => decimal.One,
      4 when !onlyPositive => decimal.MinusOne,
      _ => new(
        @this.Next(),
        @this.Next(),
        @this.Next(),
        onlyPositive || @this.GetBoolean(),
        (byte)@this.Next(29)) // Decimal has 28-29 significant digits.
    };
  }

  /// <summary>
  /// Generates a random character (<see cref="System.Char"/>), with options to restrict the character range based on specific criteria.
  /// </summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random character.</param>
  /// <param name="only7BitAscii">Specifies whether to generate only ASCII characters (7-bit), limiting the range to characters from 0 to 127.</param>
  /// <param name="only8Bit">Specifies whether to generate characters that fit within an 8-bit byte, limiting the range to characters from 0 to 255. This is broader than the 7-bit ASCII range.</param>
  /// <param name="noSurrogates">Specifies whether to exclude surrogate characters from the Unicode range.</param>
  /// <param name="noControlCharacters">Specifies whether to exclude control characters from the generated results.</param>
  /// <param name="noWhiteSpace">Specifies whether to exclude whitespace characters from the generated results.</param>
  /// <returns>A random character, potentially restricted based on the specified parameters.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Random random = new Random();
  /// char randomChar = random.GetChar();
  /// Console.WriteLine($"Random char: {randomChar}");
  /// 
  /// char asciiChar = random.GetChar(only7BitAscii: true);
  /// Console.WriteLine($"Random ASCII char: {asciiChar}");
  /// 
  /// char extendedChar = random.GetChar(only8Bit: true);
  /// Console.WriteLine($"Random Extended ASCII char: {extendedChar}");
  /// 
  /// char printableChar = random.GetChar(noControlCharacters: true);
  /// Console.WriteLine($"Random Printable char: {printableChar}");
  /// </code>
  /// This example generates random characters under different constraints: any character, only ASCII characters, only extended ASCII characters, and only printable characters, demonstrating the versatility of the method.
  /// </example>
  public static char GetChar(this Random @this, bool only7BitAscii = false, bool only8Bit = false, bool noSurrogates = false, bool noControlCharacters = false, bool noWhiteSpace=false) {
    Against.ThisIsNull(@this);
    switch (@this.Next(_SPECIAL_VALUE_LIKELIHOOD)) {
      case 0:
        return char.MinValue;
      case 1:
        return char.MaxValue;
      default:
        var max = only7BitAscii ? 0x80 : only8Bit ? 0x100 : 0x10000;
        for (;;) {
          var result = (char)@this.Next(max);
          if(noControlCharacters && char.IsControl(result))
            continue;
          if(noWhiteSpace && char.IsWhiteSpace(result))
            continue;
          if(noSurrogates && char.IsSurrogate(result))
            continue;

          return result;
        }
    }
  }

#if !SUPPORTS_RANDOM_NEXTINT64

  /// <summary>Returns a non-negative random integer.</summary>
  /// <param name="this">The <see cref="Random"/> instance used to generate the random value.</param>
  /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
  /// <exception cref="NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  public static long NextInt64(this Random @this, long minValue, long maxValue) {
    Against.ThisIsNull(@this);
    Against.False(minValue <= maxValue);

    return minValue + (long)(@this.NextDouble() * (maxValue - minValue));
  }

#endif

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
  
}
