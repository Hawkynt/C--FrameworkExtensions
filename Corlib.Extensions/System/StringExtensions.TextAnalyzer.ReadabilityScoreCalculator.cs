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

using System.Globalization;
using System.Linq;

namespace System;

partial class StringExtensions {
  public sealed partial class TextAnalyzer {

    private ReadabilityScoreCalculator _readabilityScore;

    /// <summary>
    /// Gets the <see cref="ReadabilityScoreCalculator"/> associated with this text.
    /// </summary>
    /// <remarks>
    /// Lazily initializes and returns a calculator for various standardized readability scores
    /// such as SMOG, Flesch-Kincaid, LIX, ARI, and the Wiener Sachtextformel.
    /// </remarks>
    /// <example>
    /// <code>
    /// var analyzer = new TextAnalyzer("Dies ist ein Testsatz.", CultureInfo.GetCultureInfo("de-DE"));
    /// double smogScore = analyzer.ReadabilityScore.Smog;
    /// </code>
    /// </example>
    public ReadabilityScoreCalculator ReadabilityScore => this._readabilityScore ??= new(this);
    
    /// <summary>
    /// Provides methods to calculate various readability metrics for a given text.
    /// </summary>
    /// <remarks>
    /// This class computes standardized readability indices.
    /// Metrics are computed using language-aware syllable and word boundaries based on <see cref="CultureInfo"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var calculator = new ReadabilityScoreCalculator(new("Dies ist ein Beispieltext.", CultureInfo.GetCultureInfo("de-DE")));
    /// double fleschEase = calculator.FleschReadingEase;
    /// double smog = calculator.Smog;
    /// </code>
    /// </example>
    public sealed class ReadabilityScoreCalculator {

      private readonly TextAnalyzer _analyzer;
      private CultureInfo Culture => this._analyzer.culture;
      private string[] Words => this._analyzer.Words;
      private string[] Sentences => this._analyzer.Sentences;
      private int TotalSyllables => this._analyzer.TotalSyllables;
      private bool HasNoText => this._analyzer.text.IsNullOrWhiteSpace();
      private bool HasNoWords => this.Words.IsNullOrEmpty();
      private bool HasNoSentences => this.Sentences.IsNullOrEmpty();
      private int WordCount => this.Words?.Length ?? 0;
      private int SentenceCount => this.Sentences?.Length ?? 0;

      internal ReadabilityScoreCalculator(TextAnalyzer analyzer) => this._analyzer = analyzer;

      /// <summary>
      /// Gets the SMOG (Simple Measure of Gobbledygook) readability score for the associated text using the current culture.
      /// </summary>
      /// <remarks>
      /// The SMOG index estimates the years of education needed to understand a piece of writing.
      /// It is based on the number of polysyllabic words (three or more syllables) within a set of sentences.
      /// </remarks>
      /// <returns>
      /// A <see cref="double"/> representing the SMOG readability score of the analyzed text.
      /// </returns>
      /// <example>
      /// <code>
      /// var score = "Readability measurement is essential in educational research.".ReadabilityScore().Smog;
      /// Console.WriteLine($"SMOG Score: {score}");
      /// </code>
      /// </example>
      public double Smog {
        get {
          if (this.HasNoText || this.HasNoSentences)
            return 0.0;

          var polysyllables = this.Words.Select(this._analyzer.SyllableCounter).Count(s => s >= 3);

          return 1.043 * Math.Sqrt(polysyllables * (30.0 / this.SentenceCount)) + 3.1291;
        }
      }

      /// <summary>
      /// Gets the LIX (Läsbarhetsindex) readability score for the associated text.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> value representing the LIX score, which indicates the difficulty of reading a text.
      /// Higher values represent more complex texts.
      /// </returns>
      /// <remarks>
      /// The LIX formula (Lesbarhetsindex) is based on the average sentence length and the percentage of long words (more than 6 characters).
      /// It is commonly used in Scandinavian countries to assess the readability of texts in various domains such as education and media.
      /// </remarks>
      /// <example>
      /// <code>
      /// var index = "This is a simple example sentence containing a few long words.".ReadabilityScore().Lix;
      /// Console.WriteLine($"LIX Score: {index}");
      /// </code>
      /// </example>
      public double Lix {
        get {
          if (this.HasNoWords || this.HasNoSentences)
            return 0.0;

          var longWords = this.Words.Select(w => w.Length).Count(l => l > 6);
          double wordCount = this.WordCount;

          return wordCount / this.SentenceCount + 100.0 * longWords / wordCount;
        }
      }

      /// <summary>
      /// Gets the ARI (Automated Readability Index) score for the associated text.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> value representing the ARI readability score. The score approximates the U.S. grade level required to understand the text.
      /// </returns>
      /// <remarks>
      /// The ARI (Automated Readability Index) is a readability test designed to gauge the understandability of English-language texts.
      /// It uses a formula based on the number of characters per word and words per sentence.
      /// The result typically correlates with U.S. school grade levels (e.g., a score of 6.0 implies a 6th-grade reading level).
      /// </remarks>
      /// <example>
      /// <code>
      /// var ariScore = "The fox jumps over the lazy dog.".ReadabilityScore().Ari;
      /// Console.WriteLine($"ARI Score: {ariScore}");
      /// </code>
      /// </example>
      public double Ari {
        get {
          if (this.HasNoWords || this.HasNoSentences)
            return 0.0;

          var characters = this.Words.Sum(w => w.Count(char.IsLetterOrDigit));
          double wordCount = this.WordCount;

          return 4.71 * characters / wordCount + 0.5 * wordCount / this.SentenceCount - 21.43;
        }
      }

      /// <summary>
      /// Gets the Coleman–Liau Index readability score for the associated text.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> representing the Coleman–Liau Index, which estimates the U.S. grade level needed to comprehend the text.
      /// </returns>
      /// <remarks>
      /// The Coleman–Liau Index is a readability metric designed for English texts. Unlike other indices that rely on syllable counts,
      /// it uses the number of characters per word and words per sentence to determine readability.
      /// This makes it especially well-suited for computerized analysis.
      /// </remarks>
      /// <example>
      /// <code>
      /// string content = "This is an example text used to evaluate the Coleman–Liau readability score.";
      /// double score = content.ReadabilityScore().ColemanLiau;
      /// Console.WriteLine($"Coleman–Liau Index: {score}");
      /// </code>
      /// </example>
      public double ColemanLiau {
        get {
          if (this.HasNoWords)
            return 0.0;

          var letters = this.Words.Sum(w => w.Count(char.IsLetter));
          double wordCount = this.WordCount;
          var L = letters * 100 / wordCount;
          var S = this.SentenceCount * 100 / wordCount;

          return 0.0588 * L - 0.296 * S - 15.8;
        }
      }

      /// <summary>
      /// Gets the Gunning Fog Index for the associated text using the current culture.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> value representing the Gunning Fog Index, indicating how many years of formal education
      /// are required to understand the text on first reading.
      /// </returns>
      /// <remarks>
      /// The Gunning Fog Index estimates reading difficulty based on sentence length and the proportion of complex (polysyllabic) words.
      /// A score of 12 implies the text is understandable by a high school senior.
      /// </remarks>
      /// <example>
      /// <code>
      /// var calculator = "This is a sample sentence.".ReadabilityScore().GunningFog;
      /// Console.WriteLine($"Fog Index: {fog}");
      /// </code>
      /// </example>
      public double GunningFog {
        get {
          if (this.HasNoWords || this.HasNoSentences)
            return 0.0;

          var complexWords = this.Words.Select(this._analyzer.SyllableCounter).Count(s => s >= 3);
          double wordCount = this.WordCount;

          return 0.4 * (wordCount / this.SentenceCount + 100.0 * complexWords / wordCount);
        }
      }
      
      /// <summary>
      /// Gets the Flesch-Kincaid Grade Level readability score for the associated text using the current culture.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> indicating the U.S. school grade level required to comprehend the text.
      /// </returns>
      /// <remarks>
      /// The Flesch-Kincaid Grade Level is a widely used readability metric that evaluates text complexity based on
      /// sentence length and syllable count. Higher scores indicate higher reading grade levels.
      /// </remarks>
      /// <example>
      /// <code>
      /// var score = "This is a simple sentence.".ReadabilityScore().FleschKincaid;
      /// Console.WriteLine($"Flesch-Kincaid Grade Level: {score}");
      /// </code>
      /// </example>
      public double FleschKincaid {
        get {
          if (this.HasNoWords || this.HasNoSentences)
            return 0.0;

          double wordCount = this.WordCount;
          var avgWordsPerSentence = wordCount / this.SentenceCount;
          var avgSyllablesPerWord = this.TotalSyllables / wordCount;

          return 0.39 * avgWordsPerSentence + 11.8 * avgSyllablesPerWord - 15.59;
        }
      }

      /// <summary>
      /// Gets the Flesch Reading Ease score for the associated text using the current culture.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> representing the Flesch Reading Ease score, where higher values indicate easier readability.
      /// </returns>
      /// <remarks>
      /// for culture-specific syllable counting. The score reflects how easy a text is to read,
      /// with values ranging from 0 (very difficult) to 100 (very easy).
      /// </remarks>
      /// <example>
      /// <code>
      /// var score = "The quick brown fox jumps over the lazy dog.".ReadabilityScore().FleschReadingEase;
      /// Console.WriteLine($"Flesch Reading Ease: {score}");
      /// </code>
      /// </example>
      public double FleschReadingEase {
        get {
          if (this.HasNoWords || this.HasNoSentences)
            return 0.0;

          double wordCount = this.WordCount;
          var avgWordsPerSentence = wordCount / this.SentenceCount;
          var avgSyllablesPerWord = this.TotalSyllables / wordCount;

          return this.Culture.TwoLetterISOLanguageName switch {
            "de" => 180 - avgWordsPerSentence - 58.5 * avgSyllablesPerWord,
            _ => 206.835 - 1.015 * avgWordsPerSentence - 84.6 * avgSyllablesPerWord
          };
        }
      }
      
      /// <summary>
      /// Gets the Wiener Sachtextformel (WSTF 1) readability score for the associated text using the current culture.
      /// </summary>
      /// <returns>
      /// A <see cref="double"/> representing the WSTF 1 readability index. Lower values indicate simpler texts, while higher values imply greater complexity.
      /// </returns>
      /// <remarks>
      /// The Wiener Sachtextformel (WSTF) is a German-language readability formula designed to assess the comprehensibility of factual and official texts.
      /// It takes into account sentence length, word length, and the proportion of polysyllabic or complex words in the text.
      /// </remarks>
      /// <example>
      /// <code>
      /// string text = "Der Wiener Sachtextformel bewertet die Lesbarkeit von Sachtexten.";
      /// double score = text.ReadabilityScore().Wstf;
      /// Console.WriteLine($"WSTF Score: {score}");
      /// </code>
      /// </example>
      public double Wstf {
        get {
          if (this.HasNoText || this.HasNoWords || this.HasNoSentences)
            return 0.0;

          double wordCount = this.WordCount;
          var avgSentenceLength = wordCount / this.SentenceCount;
          var syllables = this.Words.Select(this._analyzer.SyllableCounter).ToArray();
          var ms = syllables.Count(s => s >= 3) / wordCount * 100;
          var iw = this.Words.Count(w => w.Length > 6) / wordCount * 100;
          var es = syllables.Count(s => s == 1) / wordCount * 100;

          return 0.1935 * ms + 0.1672 * avgSentenceLength + 0.1297 * iw - 0.0327 * es - 0.875;
        }
      }

    }
  }
}
