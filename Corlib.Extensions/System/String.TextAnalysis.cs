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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    private static Dictionary<string, string[]> _Abbreviations => StaticMethodLocal<Dictionary<string, string[]>>.GetOrAdd(() => new(StringComparer.OrdinalIgnoreCase) {
      ["de"] = ["z.B.", "u.A.", "d.h.", "bzw.", "sog.", "dr.", "prof.", "usw.", "vgl.", "Nr.", "ca.", "Hr.", "Fr.", "Abs.", "ggf.", "etc.", "i.d.R.", "i.A."],
      ["en"] = ["e.g.", "i.e.", "vs.", "mr.", "mrs.", "ms.", "dr.", "prof.", "etc.", "cf.", "ca.", "est.", "p.m.", "a.m.", "Inc.", "Ltd.", "Co.", "Jr.", "Sr."],
      [string.Empty] = null
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
    public IEnumerable<string> DistinctWords {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => this._distinctWords ??= new HashSet<string>(this.Words, StringComparer.Create(this.culture, ignoreCase: true));
    }

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
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    public string[] Sentences {
      get {
        return this._sentences ??= this.text.IsNullOrWhiteSpace() ? [] : Invoke().ToArray();

        IEnumerable<string> Invoke() {
          var abbreviations = _Abbreviations.TryGetValue(this.culture.TwoLetterISOLanguageName, out var splitter)
            ? splitter
            : _Abbreviations[string.Empty];

          const char TokenIdentifier = '\0';
          var root = BuildTrie(this.culture, abbreviations, TokenIdentifier);
          var tokenized = TokenizeAbbreviations(this.text, root, TokenIdentifier, out var tokenMap);
          var raw = Split(tokenized, TokenIdentifier);
          var result = raw.Select(sentence => sentence.MultipleReplace(tokenMap));

          return result;
        }

        static TrieNode BuildTrie(CultureInfo culture, string[] strings, char tokenIdentifier) {
          var trieNode = new TrieNode(new(culture));
          if (strings != null)
            for (var i = 0; i < strings.Length; ++i)
              trieNode.Insert(strings[i], i + 1);

          trieNode.Insert(tokenIdentifier.ToString(), 0);
          return trieNode;
        }

        static string TokenizeAbbreviations(string text, TrieNode root, char tokenIdentifier, out Dictionary<string, string> tokenMap) {
          tokenMap = new();
          var sb = new StringBuilder(text.Length);
          for (var i = 0; i < text.Length;) {
            var node = root;
            int matchLen = 0, matchId = 0, j = i;
            // walk as far as we can
            while (j < text.Length && node.Children.TryGetValue(text[j], out var next)) {
              node = next;
              ++j;
              if (node.TokenId <= 0)
                continue;

              matchLen = j - i;
              matchId = node.TokenId;
            }

            if (matchLen > 0) {
              var token = $"{tokenIdentifier}N{matchId}{tokenIdentifier}";
              tokenMap[token] = text.Substring(i, matchLen);
              sb.Append(token);
              i += matchLen;
            } else {
              sb.Append(text[i]);
              ++i;
            }
          }

          return sb.ToString();
        }

        static IEnumerable<string> Split(string text, char tokenIdentifier) {
          var start = 0;
          var i = 0;
          var len = text.Length;

          while (i < len) {
            var c = text[i];

            if (IsTerminator(c)) {

              // step forward through any consecutive terminators
              while (i + 1 < len && IsTerminator(text[i + 1]))
                ++i;

              // step forward through whitespace
              var sentenceEnd = i + 1;
              while (sentenceEnd < len && char.IsWhiteSpace(text[sentenceEnd]))
                ++sentenceEnd;

              // if next char is likely a sentence start (letter or digit), split here
              if (sentenceEnd >= len || text[sentenceEnd] == tokenIdentifier || char.IsLetterOrDigit(text[sentenceEnd])) {
                var sentence = text[start..sentenceEnd].Trim();
                if (sentence.Length > 0)
                  yield return sentence;

                start = sentenceEnd;
                i = sentenceEnd;
                continue;
              }
            }

            ++i;
          }

          // yield remainder if any
          if (start < len) {
            var tail = text[start..].Trim();
            if (tail.Length > 0)
              yield return tail;
          }

          yield break;

          [MethodImpl(MethodImplOptions.AggressiveInlining)] 
          static bool IsTerminator(char chr) => chr is '.' or '!' or '?';
        }
      }
    }

    private sealed class CharIgnoreCaseComparer(CultureInfo culture) : IEqualityComparer<char> {
      
      public bool Equals(char x, char y) => 
        x == y 
        || char.ToUpper(x, culture) == char.ToUpper(y, culture)
        || char.ToLower(x, culture) == char.ToLower(y, culture)
        ;

      public int GetHashCode(char c) => char.ToUpper(c, culture).GetHashCode();

    }

    private sealed class TrieNode(CharIgnoreCaseComparer comparer) {
      public int TokenId;
      public readonly Dictionary<char, TrieNode> Children = new(comparer);

      public void Insert(string abbr, int id) {
        var node = this;
        foreach (var c in abbr) {
          if (!node.Children.TryGetValue(c, out var next)) {
            next = new(comparer);
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