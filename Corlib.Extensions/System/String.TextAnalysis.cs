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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using MethodImplOptions = Utilities.MethodImplOptions;

namespace System;

partial class StringExtensions {

  /// <summary>
  /// Creates a <see cref="TextAnalyzer"/> for the specified string using the current UI culture.
  /// </summary>
  /// <param name="this">The input text to analyze.</param>
  /// <returns>
  /// A new <see cref="TextAnalyzer"/> instance that analyzes the text using the rules of
  /// <see cref="CultureInfo.CurrentUICulture"/>.
  /// </returns>
  /// <remarks>
  /// This method is useful when performing linguistic or readability analysis in a locale-aware way,
  /// using the thread's current user interface culture to determine language-specific behaviors such as syllable counting.
  /// </remarks>
  /// <example>
  /// <code>
  /// var analyzer = "This is an example sentence.".TextAnalysis();
  /// Console.WriteLine($"Word Count: {analyzer.Words.Length}");
  /// </code>
  /// </example>
  public static TextAnalyzer TextAnalysis(this string @this) => new(@this, CultureInfo.CurrentUICulture);

  /// <summary>
  /// Creates a <see cref="TextAnalyzer"/> for the specified string using culture-specific rules.
  /// </summary>
  /// <param name="this">The input text to analyze.</param>
  /// <param name="culture">
  /// The <see cref="CultureInfo"/> to use for language-aware text analysis operations
  /// such as syllable counting, word tokenization, and normalization.
  /// </param>
  /// <returns>
  /// A new <see cref="TextAnalyzer"/> instance that encapsulates the parsed form and metrics of the text
  /// using the rules of the specified <paramref name="culture"/>.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// Thrown if <paramref name="culture"/> is <see langword="null"/>.
  /// </exception>
  /// <remarks>
  /// This method is commonly used in preparation for readability assessments, linguistic analysis,
  /// or NLP preprocessing where culture-specific rules must be respected.
  /// </remarks>
  /// <example>
  /// <code>
  /// var analyzer = "Dies ist ein Beispielsatz.".TextAnalysisFor(CultureInfo.GetCultureInfo("de-DE"));
  /// Console.WriteLine($"Syllables: {analyzer.CountSyllables()}");
  /// </code>
  /// </example>
  public static TextAnalyzer TextAnalysisFor(this string @this, CultureInfo culture) {
    Against.ArgumentIsNull(culture);

    return new(@this, culture);
  }

  /// <summary>
  /// Provides methods and properties for analyzing natural language text, including
  /// word statistics, readability metrics, and frequency analysis.
  /// </summary>
  /// <remarks>
  /// The <see cref="TextAnalyzer"/> class is intended for linguistic and readability analysis
  /// of a single text input, using language rules determined by a specific <see cref="CultureInfo"/>.
  /// It supports extraction of words, distinct words, word histograms, and is used as the underlying
  /// engine for <see cref="ReadabilityScoreCalculator"/>.
  /// </remarks>
  /// <example>
  /// <code>
  /// var analyzer = new TextAnalyzer("Dies ist ein Beispielsatz.", CultureInfo.GetCultureInfo("de-DE"));
  /// var sentenceCount = analyzer.Sentences.Length;
  /// var uniqueWords = analyzer.DistinctWords;
  /// </code>
  /// </example>
  public sealed partial class TextAnalyzer {
    private static readonly Regex _SPLIT_WORDS = new(@"[\p{L}\p{M}\p{N}']+", RegexOptions.Compiled);
    
    private static Dictionary<string, Func<string, int>> _SyllableCounters => StaticMethodLocal<Dictionary<string, Func<string, int>>>.GetOrAdd(() => new(StringComparer.OrdinalIgnoreCase) {
      ["de"] = _CountSyllablesGerman,
      ["en"] = _CountSyllablesEnglish,
      ["fr"] = _CountSyllablesRomance,
      ["es"] = _CountSyllablesRomance,
      ["it"] = _CountSyllablesRomance,
      ["pt"] = _CountSyllablesRomance
    });

    private static Dictionary<string, (string capitalLetters, string[] abbreviations)> _SentenceSplitterData => StaticMethodLocal<Dictionary<string, (string capitalLetters, string[] abbreviations)>>.GetOrAdd(() => new(StringComparer.OrdinalIgnoreCase) {
      ["de"] = ("ABCDEFGHIJKLMNOPQRSTUVWXYZAÖÜß", ["z.B.", "u.A.", "d.h.", "bzw.", "sog.", "dr.", "prof."]),
      ["en"] = ("ABCDEFGHIJKLMNOPQRSTUVWXYZ", ["e.g.", "i.e.", "vs.", "mr.", "mrs.", "ms.", "dr.", "prof."]),
      [string.Empty] = ("ABCDEFGHIJKLMNOPQRSTUVWXYZ", null)
    });

    internal readonly string text;
    internal readonly CultureInfo culture;

    internal TextAnalyzer(string text, CultureInfo culture) {
      this.text = text;
      this.culture = culture;
    }

    private string[] _words;

    /// <summary>
    /// Gets an array of all word tokens extracted from the analyzed text.
    /// </summary>
    /// <returns>
    /// An array of <see cref="string"/> representing individual words in the text,
    /// excluding punctuation and whitespace.
    /// </returns>
    /// <remarks>
    /// This property uses culture-aware word segmentation and filters out
    /// non-word characters. It is commonly used as a basis for readability metrics
    /// that depend on word counts, such as SMOG or Gunning Fog.
    /// </remarks>
    /// <example>
    /// <code>
    /// var words = "Hello world!".ReadabilityScore().Words;
    /// Console.WriteLine($"Word count: {words.Length}");
    /// </code>
    /// </example>
    public string[] Words => this._words ??= _SPLIT_WORDS
      .Matches(this.text ?? string.Empty)
      .Cast<Match>()
      .Select(m => m.Value)
      .ToArray()
    ;

    private IEnumerable<string> _distinctWords;

    /// <summary>
    /// Gets the collection of unique words used in the analyzed text, ignoring case based on the current culture.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the distinct words in the text.
    /// </returns>
    /// <remarks>
    /// This property lazily initializes and caches the set of distinct words, comparing them in a case-insensitive
    /// manner according to the set <see cref="CultureInfo"/>. Useful for vocabulary analysis or
    /// determining lexical diversity metrics.
    /// </remarks>
    /// <example>
    /// <code>
    /// var distinct = "Hello hello world!".ReadabilityScore().DistinctWords;
    /// Console.WriteLine($"Unique words: {distinct.Count()}");
    /// </code>
    /// </example>
    public IEnumerable<string> DistinctWords => this._distinctWords ??= new HashSet<string>(this.Words, StringComparer.Create(this.culture, ignoreCase: true));

    private IDictionary<string, int> _wordHistogram;

    /// <summary>
    /// Gets a histogram of word occurrences in the analyzed text.
    /// </summary>
    /// <returns>
    /// An <see cref="IDictionary{TKey, TValue}"/> mapping each word (as <see cref="string"/>) to the number of times it appears in the text.
    /// </returns>
    /// <remarks>
    /// Words are counted in a case-insensitive manner based on the current culture unless preprocessed.
    /// The histogram is lazily initialized and cached for future access.
    /// This data can be used for frequency analysis, keyword extraction, or lexical profiling.
    /// </remarks>
    /// <example>
    /// <code>
    /// var histogram = "test test one two".ReadabilityScore().WordHistogram;
    /// foreach (var entry in histogram)
    ///   Console.WriteLine($"{entry.Key}: {entry.Value}");
    /// // Output:
    /// // test: 2
    /// // one: 1
    /// // two: 1
    /// </code>
    /// </example>
    public IDictionary<string, int> WordHistogram {
      get {
        return this._wordHistogram ??= Invoke();

        Dictionary<string, int> Invoke() {
          var words = this.Words;
          var comparer = StringComparer.Create(this.culture, ignoreCase: true);
          var histogram = new Dictionary<string, int>(comparer);
          foreach (var word in words)
            if (histogram.TryGetValue(word, out var count))
              histogram[word] = ++count;
            else
              histogram[word] = 1;

          return histogram;
        }
      }
    }
    
    private string[] _sentences;

    /// <summary>
    /// Get the meaningful sentences identified in the current text.
    /// Sentences must contain at least one word-like token (letters, numbers, apostrophes).
    /// </summary>

    // TODO: The current regex-based sentence splitting fails to reliably handle abbreviations
    // such as "Dr.", "z.B.", or malformed spacing (e.g. "Dr.Max said stuff.").
    //
    // Problem:
    // - Negative lookbehinds for abbreviations are brittle, hard to scale, and limited by .NET's regex engine.
    // - Regexes fail with overlapping tokens or mixed punctuation, e.g., "z.B. das ist so." or "Dr.Max".
    // - Abbreviations without trailing whitespace or with unconventional casing break sentence detection.
    //
    // Proposed fix (pre/post-processing strategy):
    // 1. Preprocess the input by replacing all known abbreviations (plus \0 characters) with unique tokens:
    //    - Each abbreviation becomes a token like "\0N1\0", "\0N2\0", etc.
    //    - The NUL character \0 is handled specially and always replaced with the fixed token "\0N0\0".
    // 2. Build a trie (prefix tree) of all known abbreviations and \0:
    //    - Traverse the input once (O(N)) and insert tokens using the trie matcher.
    //    - Append unmatched characters and tokens directly to a preallocated StringBuilder.
    // 3. Perform sentence splitting using simplified regex logic that no longer needs to consider abbreviations.
    // 4. Postprocess the split segments by replacing all tokens back with their original values using a dictionary.
    //
    // Implementation details:
    // - Use a trie node structure with child nodes and a terminal Abbreviation field.
    // - Map each token "\0N{index}\0" to the matched abbreviation in a Dictionary<string, string>.
    // - The fixed token "\0N0\0" is always reserved for \0 and stored in the same map.
    // - The postprocessor replaces all tokens in one pass via the dictionary.
    public string[] Sentences {
      get {
        return this._sentences ??= this.text.IsNullOrWhiteSpace() ? [] : Invoke().ToArray();

        IEnumerable<string> Invoke() {
          var (letters, abbreviations) = _SentenceSplitterData.TryGetValue(this.culture.TwoLetterISOLanguageName, out var splitter)
            ? splitter
            : _SentenceSplitterData[string.Empty];

          const string NullCode = @"\0";

          // build a trie of abbreviations
          var root = new TrieNode();
          if (abbreviations != null)
            for (var i = 0; i < abbreviations.Length; ++i)
              root.Insert(abbreviations[i], i + 1);

          root.Insert(NullCode, 0);

          // tokenize all abbreviations in one scan
          var tokenMap = new Dictionary<string, string>();
          var sb = new StringBuilder(this.text.Length);
          for (var i = 0; i < this.text.Length;) {
            var node = root;
            int matchLen = 0, matchId = 0, j = i;
            // walk as far as we can
            while (j < this.text.Length && node.Children.TryGetValue(this.text[j], out var next)) {
              node = next;
              ++j;
              if (node.TokenId <= 0)
                continue;

              matchLen = j - i;
              matchId = node.TokenId;
            }

            if (matchLen > 0) {
              var token = $"{NullCode}N{matchId}{NullCode}";
              tokenMap[token] = this.text.Substring(i, matchLen);
              sb.Append(token);
              i += matchLen;
            } else {
              sb.Append(this.text[i]);
              ++i;
            }
          }

          // simple split on end‐of‐sentence punctuation + maybe whitespace + letter (capital or not)/replacement
          var pattern = $@"(?<=[\.!\?])\s*";
          var raw = Regex.Split(sb.ToString(), pattern, RegexOptions.IgnoreCase);

          // restore abbreviations, trim & filter
          var result = raw
            .Select(segment => tokenMap.Aggregate(segment, (current, kv) => current.Replace(kv.Key, kv.Value)).Trim())
            .Where(s => s.Length > 0 && _SPLIT_WORDS.IsMatch(s))
            ;

          return result;
        }

      }
    }

    private sealed class CharIgnoreCaseComparer : IEqualityComparer<char> {
      public static readonly CharIgnoreCaseComparer Instance = new();

      public bool Equals(char x, char y) => 
        char.ToUpper(x) == char.ToUpper(y)
        || char.ToLower(x) == char.ToLower(y)
        ;

      public int GetHashCode(char c) => char.ToUpperInvariant(c).GetHashCode();

    }

    private sealed class TrieNode {
      public int TokenId;
      public readonly Dictionary<char, TrieNode> Children = new(CharIgnoreCaseComparer.Instance);

      public void Insert(string abbr, int id) {
        var node = this;
        foreach (var c in abbr) {
          if (!node.Children.TryGetValue(c, out var next)) {
            next = new();
            node.Children[c] = next;
          }

          node = next;
        }

        node.TokenId = id;
      }

    }

    public int TotalSyllables => this.Words.Select(this.SyllableCounter).Sum(s => s);

    internal Func<string, int> SyllableCounter =>
      _SyllableCounters.TryGetValue(this.culture.TwoLetterISOLanguageName, out var counter)
        ? counter
        : throw new NotSupportedException($"Language '{this.culture.Name}' is not supported for syllable counting.")
    ;

    private static int _CountSyllablesGerman(string word) => _CountVowelGroups(word, "[aeiouyäöüAEIOUYÄÖÜ]+");
    private static int _CountSyllablesEnglish(string word) => _CountVowelGroups(word, "[aeiouyAEIOUY]+");
    private static int _CountSyllablesRomance(string word) => _CountVowelGroups(word, "[aeiouáéíóúàèìòùâêîôûäëïöüœæ]+", ignoreCase: true);

    private static int _CountVowelGroups(string word, string pattern, bool ignoreCase = false) {
      if (word.IsNullOrWhiteSpace())
        return 0;

      var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
      var matches = Regex.Matches(word, pattern, options);
      return Math.Max(matches.Count, 1);
    }

  }

}