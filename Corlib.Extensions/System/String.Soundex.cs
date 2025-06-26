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
//

using Guard;
using System.Globalization;

namespace System;

partial class StringExtensions {

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentation(this string @this)
    => GetSoundexRepresentation(@this, CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "de" ? 5 : 4, CultureInfo.CurrentCulture);

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="maxLength">The length of the soundex string</param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentation(this string @this, int maxLength)
    => GetSoundexRepresentation(@this, maxLength, CultureInfo.CurrentCulture);

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentationInvariant(this string @this)
    => GetSoundexRepresentation(@this, 4, CultureInfo.InvariantCulture);

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="maxLength">The length of the soundex string</param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentationInvariant(this string @this, int maxLength)
    => GetSoundexRepresentation(@this, maxLength, CultureInfo.InvariantCulture);

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="culture">The culture to use for phonetic matchings</param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentation(this string @this, CultureInfo culture)
    => GetSoundexRepresentation(@this, culture.TwoLetterISOLanguageName == "de" ? 5 : 4, culture);

  /// <summary>
  ///   Gets the soundex representation of a given string (doesn't split words, assumes everthing is one word)
  /// </summary>
  /// <param name="this">This <see cref="string" /></param>
  /// <param name="maxLength">The length of the soundex string</param>
  /// <param name="culture">The culture to use for phonetic matchings</param>
  /// <returns>The soundex result (phonetic representation)</returns>
  public static string GetSoundexRepresentation(this string @this, int maxLength, CultureInfo culture) {
    Against.ThisIsNull(@this);
    Against.ValuesBelow(maxLength, 4);
    Against.ArgumentIsNull(culture);

    const char ZERO = '0';

    if (@this.Length == 0)
      return new(ZERO, maxLength);

    var isGermanCulture = culture.TwoLetterISOLanguageName == "de";
    Func<char, char, char> GetSoundexCode = isGermanCulture ? GetGermanSoundexCode : GetEnglishSoundexCode;
    Func<char, char> DiacriticsReplacer = isGermanCulture ? GetGermanReplacer : GetEnglishReplacer;

    var firstChar = char.MinValue;

    var nextUnseenIndex = _INDEX_NOT_FOUND;
    for (var i = 0; i < @this.Length; ++i) {
      var currentChar = @this[i];
      if (!char.IsLetter(currentChar))
        continue;

      nextUnseenIndex = i + 1;
      currentChar = char.ToUpper(currentChar, culture);

      // when first character already known, calculate soundex
      if (firstChar != char.MinValue)
        return CalculateSoundexRepresentation(@this, maxLength, firstChar, currentChar, nextUnseenIndex, GetSoundexCode, culture);

      // assign the first letter and continue searching a second one
      firstChar = DiacriticsReplacer(currentChar);
    }

    // no letters found
    if (nextUnseenIndex < 0)
      return new(ZERO, maxLength);

    // only one letter found
    return firstChar + new string(ZERO, maxLength - 1);

    static string CalculateSoundexRepresentation(string text, int maxLength, char firstCharacter, char previousCharacter, int lastLetterSeenPlusOne, Func<char, char, char> getSoundexCode, CultureInfo cultureInfo) {
      var result = new char[maxLength];
      result[0] = firstCharacter;

      var resultIndex = 1;
      var previousResultChar = char.MinValue;

      for (var i = lastLetterSeenPlusOne; i < text.Length; ++i) {
        var currentCharacter = text[i];

        // only take letters into account
        if (!char.IsLetter(currentCharacter))
          continue;

        currentCharacter = char.ToUpper(currentCharacter, cultureInfo);

        var soundexCode = getSoundexCode(previousCharacter, currentCharacter);
        previousCharacter = currentCharacter;

        // no duplicate characters in soundex
        if (soundexCode == previousResultChar)
          continue;

        // no inside zeroes
        if (soundexCode == ZERO)
          continue;

        result[resultIndex++] = previousResultChar = soundexCode;

        // if we already got enough soundex letters, return
        if (resultIndex >= maxLength)
          return new(result);
      }

      // last soundex letter
      var lastSoundex = getSoundexCode(previousCharacter, char.MinValue);

      // still no duplicate characters in soundex
      if (lastSoundex != previousResultChar)
        result[resultIndex++] = lastSoundex;

      // fill rest with zeroes
      for (var i = resultIndex; i < maxLength; ++i)
        result[i] = ZERO;

      return new(result);
    }

    static char GetGermanSoundexCode(char letter, char next)
      => letter switch {
        'C' when next == 'H' => '7',
        'B' or 'P' or 'F' or 'V' or 'W' => '1',
        'C' or 'G' or 'K' or 'Q' or 'X' or 'S' or 'Z' or 'ß' => '2',
        'D' or 'T' => '3',
        'L' => '4',
        'M' or 'N' => '5',
        'R' => '6',
        _ => '0'
      };

    static char GetGermanReplacer(char letter)
      => letter switch {
        '\u00c4' or '\u00e4' => 'A',
        '\u00d6' or '\u00f6' => 'O',
        '\u00dc' or '\u00fc' => 'U',
        '\u00df' => 'S',
        _ => letter
      };

    static char GetEnglishSoundexCode(char letter, char next)
      => letter switch {
        'B' or 'F' or 'P' or 'V' => '1',
        'C' or 'G' or 'J' or 'K' or 'Q' or 'S' or 'X' or 'Z' => '2',
        'D' or 'T' => '3',
        'L' => '4',
        'M' or 'N' => '5',
        'R' => '6',
        _ => '0'
      };

    static char GetEnglishReplacer(char letter) => letter;

  }
}
