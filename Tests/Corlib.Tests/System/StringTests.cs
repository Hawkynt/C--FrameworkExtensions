using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace Corlib.Tests.System;

using global::System.Globalization;

[TestFixture]
public class StringTests {

  public readonly record struct SplitParametersTestData(
    string? Input,
    string? Splitter,
    ulong? Max,
    IEnumerable<string>? Expected,
    Type? Exception = null
  );

  public readonly record struct FormatWithExParametersTestData(
    string? Input,
    Func<string, object?>? FieldGetter,
    bool PassFieldFormatToGetter,
    string? Expected,
    Type? Exception = null
  );

  public readonly record struct StringComparisonTestData(
    string? Haystack,
    char Needle,
    StringComparison Comparison,
    bool ExpectedResult
  );

  public readonly record struct StringComparerTestData(
    string? Haystack,
    char Needle,
    StringComparer Comparer,
    bool ExpectedResult
  );

  public readonly record struct FormatWithParametersTestData(
    string? Input,
    string? Expected,
    Type? Exception = null,
    params object[]? Parameters
  );

  public readonly record struct MatchGroupsTestData(
    string? Input,
    string? Regex,
    string[]? Expected,
    RegexOptions RegexOptions,
    Type? Exception = null
  );

  public readonly record struct MatchesTestData(
    string? Input,
    string? Regex,
    string[]? Expected,
    RegexOptions RegexOptions,
    Type? Exception = null
  );

  public readonly record struct MultipleReplaceTestData(
    string? Input,
    IEnumerable<KeyValuePair<string, object?>>? Replacements,
    string? Expected,
    Type? Exception = null
  );

  private static IEnumerable<SplitParametersTestData> _TestSplitTestData() {
    yield return new(null, null, null, null, typeof(NullReferenceException));
    yield return new(string.Empty, string.Empty, null, new[] { string.Empty });
    yield return new("abc", null, null, new[] { "abc" });
    yield return new("abc", string.Empty, null, new[] { "abc" });
    yield return new("abc", "b", null, new[] { "a", "c" });
    yield return new("abc", "d", null, new[] { "abc" });
    yield return new("abcbdbe", "b", null, new[] { "a", "c", "d", "e" });
    yield return new("abcbdbe", "b", 2, new[] { "a", "c", "dbe" });
    yield return new("abcbdbe", "b", 1, new[] { "a", "cbdbe" });
  }

  private static IEnumerable<MultipleReplaceTestData> _TestMultipleReplaceTestData() {
    yield return new(null, new Dictionary<string, object?> { { "ha", "Riesen" }, { "llo", "zahn" } }, null);
    yield return new("", new Dictionary<string, object?> { { "ha", "Riesen" }, { "llo", "zahn" } }, "");
    yield return new("", null, "");
    yield return new("hallo", new Dictionary<string, object?> { { "ha", "Riesen" }, { "llo", "zahn" } }, "Riesenzahn");
    yield return new("hallo", new Dictionary<string, object?> { { "hal", "Riesen" }, { "llo", "zahn" } }, "Riesenlo");
    yield return new("hallo", new Dictionary<string, object?> { { "ha", "Riesen" }, { "lloo", "zahn" } }, "Riesenllo");
    yield return new("hallo", new Dictionary<string, object?> { { "hallo", "Riesen" }, { "llo", "zahn" } }, "Riesen");
    yield return new("hallo", new Dictionary<string, object?> { { "Katze", "Riesen" }, { "llo", "zahn" } }, "hazahn");
    yield return new("hallo", new Dictionary<string, object?> { { "ha", "Riesen" }, { "ll", "zahn" }, { "o", 5 } }, "Riesenzahn5");
  }

  private static IEnumerable<MatchesTestData> _TestMatchesTestData() {
    yield return new(null, @"^.*mp3$", null, RegexOptions.None, typeof(NullReferenceException));
    yield return new("", null, null, RegexOptions.None, typeof(ArgumentNullException));
    yield return new("sAid sheD seE spear sprEad Super", @"s\w+d", new[] { "sAid", "sprEad" }, RegexOptions.None);
    yield return new("sAid sheD seE spear sprEad Super", @"s\w+d", new[] { "sAid", "sheD", "sprEad" }, RegexOptions.IgnoreCase);
  }

  private static IEnumerable<MatchGroupsTestData> _TestMatchGroupsTestData() {
    yield return new(null, @"^.*mp3$", null, RegexOptions.None, typeof(NullReferenceException));
    yield return new("", null, null, RegexOptions.None, typeof(ArgumentNullException));
    yield return new("said shed see spear spread super", @"s\w+d", new[] { "said" }, RegexOptions.None);
    yield return new("saiD shed see spear spread super", @"s\w+d", new[] { "shed" }, RegexOptions.None);
    yield return new("saiD sheD see spear spread super", @"s\w+d", new[] { "spread" }, RegexOptions.None);
    yield return new("SAid sheD seE spear sprEaD Super", @"s\w+d", new[] { "SAid" }, RegexOptions.IgnoreCase);
  }

  private static IEnumerable<FormatWithParametersTestData> _TestFormatWithParameters() {
    yield return new("Money is: {0:c} and tomorrow it is: {1:c}", "Money is: 21,80 € and tomorrow it is: 24,10 €", null, new object[] { 21.8, 24.1 });
    yield return new(null, null, typeof(NullReferenceException), new object[] { 21.8, 24.1 });
    yield return new("", null, typeof(ArgumentNullException), null);
    yield return new("Money is: {0:c} degrees and tomorrow it is: {2:c} degrees", null, typeof(FormatException), new object[] { 21.8, 24.1 });
  }

  private static IEnumerable<FormatWithExParametersTestData> _TestFormatWithExParameters() {
    yield return new(null, field => field, false, null, typeof(NullReferenceException));
    yield return new(string.Empty, null, false, null, typeof(ArgumentNullException));
    yield return new("Hallo {name}", field => field, false, "Hallo name");
    yield return new("Hallo {name}", _ => null, false, "Hallo ");
    yield return new("Hallo {name:0.0}", _ => 5, false, "Hallo 5,0");
    yield return new("Hallo {name:0.0}", field => field, true, "Hallo name:0.0");
  }

  private static IEnumerable<StringComparisonTestData> _TestStringsComparison() {
    //TESTS: Matching Strings ignore Case
    yield return new("Test", 't', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparison.OrdinalIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparison.OrdinalIgnoreCase, true);

    yield return new("test", 't', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("test", 't', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("test", 't', StringComparison.OrdinalIgnoreCase, true);
    yield return new("test", 'T', StringComparison.CurrentCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparison.InvariantCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparison.OrdinalIgnoreCase, true);
    //TESTS: Matching Strings no ignore Case
    yield return new("Test", 't', StringComparison.CurrentCulture, false);
    yield return new("Test", 't', StringComparison.InvariantCulture, false);
    yield return new("Test", 't', StringComparison.Ordinal, false);
    yield return new("Test", 'T', StringComparison.CurrentCulture, true);
    yield return new("Test", 'T', StringComparison.InvariantCulture, true);
    yield return new("Test", 'T', StringComparison.Ordinal, true);

    yield return new("test", 't', StringComparison.CurrentCulture, true);
    yield return new("test", 't', StringComparison.InvariantCulture, true);
    yield return new("test", 't', StringComparison.Ordinal, true);
    yield return new("test", 'T', StringComparison.CurrentCulture, false);
    yield return new("test", 'T', StringComparison.InvariantCulture, false);
    yield return new("test", 'T', StringComparison.Ordinal, false);
    //TESTS: Not Matching Strings ignore Case
    yield return new("Test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparison.OrdinalIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparison.OrdinalIgnoreCase, false);

    yield return new("test", 'c', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparison.OrdinalIgnoreCase, false);
    yield return new("test", 'C', StringComparison.CurrentCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparison.InvariantCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparison.OrdinalIgnoreCase, false);
    //TESTS: Not Matching Strings not ignore Case
    yield return new("Test", 'c', StringComparison.CurrentCulture, false);
    yield return new("Test", 'c', StringComparison.InvariantCulture, false);
    yield return new("Test", 'c', StringComparison.Ordinal, false);
    yield return new("Test", 'C', StringComparison.CurrentCulture, false);
    yield return new("Test", 'C', StringComparison.InvariantCulture, false);
    yield return new("Test", 'C', StringComparison.Ordinal, false);

    yield return new("test", 'c', StringComparison.CurrentCulture, false);
    yield return new("test", 'c', StringComparison.InvariantCulture, false);
    yield return new("test", 'c', StringComparison.Ordinal, false);
    yield return new("test", 'C', StringComparison.CurrentCulture, false);
    yield return new("test", 'C', StringComparison.InvariantCulture, false);
    yield return new("test", 'C', StringComparison.Ordinal, false);
  }

  private static IEnumerable<StringComparerTestData> _TestStringsComparer() {
    //TESTS: Matching Strings ignore Case
    yield return new("Test", 't', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("Test", 't', StringComparer.OrdinalIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("Test", 'T', StringComparer.OrdinalIgnoreCase, true);

    yield return new("test", 't', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("test", 't', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("test", 't', StringComparer.OrdinalIgnoreCase, true);
    yield return new("test", 'T', StringComparer.CurrentCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparer.InvariantCultureIgnoreCase, true);
    yield return new("test", 'T', StringComparer.OrdinalIgnoreCase, true);
    //TESTS: Matching Strings no ignore Case
    yield return new("Test", 't', StringComparer.CurrentCulture, false);
    yield return new("Test", 't', StringComparer.InvariantCulture, false);
    yield return new("Test", 't', StringComparer.Ordinal, false);
    yield return new("Test", 'T', StringComparer.CurrentCulture, true);
    yield return new("Test", 'T', StringComparer.InvariantCulture, true);
    yield return new("Test", 'T', StringComparer.Ordinal, true);

    yield return new("test", 't', StringComparer.CurrentCulture, true);
    yield return new("test", 't', StringComparer.InvariantCulture, true);
    yield return new("test", 't', StringComparer.Ordinal, true);
    yield return new("test", 'T', StringComparer.CurrentCulture, false);
    yield return new("test", 'T', StringComparer.InvariantCulture, false);
    yield return new("test", 'T', StringComparer.Ordinal, false);
    //TESTS: Not Matching Strings ignore Case
    yield return new("Test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'c', StringComparer.OrdinalIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("Test", 'C', StringComparer.OrdinalIgnoreCase, false);

    yield return new("test", 'c', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("test", 'c', StringComparer.OrdinalIgnoreCase, false);
    yield return new("test", 'C', StringComparer.CurrentCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparer.InvariantCultureIgnoreCase, false);
    yield return new("test", 'C', StringComparer.OrdinalIgnoreCase, false);
    //TESTS: Not Matching Strings not ignore Case
    yield return new("test", 'c', StringComparer.CurrentCulture, false);
    yield return new("test", 'c', StringComparer.InvariantCulture, false);
    yield return new("test", 'c', StringComparer.Ordinal, false);
    yield return new("test", 'C', StringComparer.CurrentCulture, false);
    yield return new("test", 'C', StringComparer.InvariantCulture, false);
    yield return new("test", 'C', StringComparer.Ordinal, false);
  }

  [Test]
  [TestCaseSource(nameof(_TestStringsComparer))]
  public void StartsWith_CharacterComparer(StringComparerTestData data)
    => ExecuteTest(() => data.Haystack.StartsWith(data.Needle, data.Comparer), data.ExpectedResult, null);

  [Test]
  [TestCaseSource(nameof(_TestStringsComparison))]
  public void StartsWith_CharacterComparison(StringComparisonTestData data)
    => ExecuteTest(() => data.Haystack.StartsWith(data.Needle, data.Comparison), data.ExpectedResult, null);

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase(" ", false)]
  [TestCase("abc", false)]
  public void IsNullOrEmpty(string? toTest, bool expected)
    => ExecuteTest(toTest.IsNullOrEmpty, expected, null);

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase(" ", true)]
  [TestCase("abc", false)]
  public void IsNullOrWhitespace(string? toTest, bool expected)
    => ExecuteTest(toTest.IsNullOrWhiteSpace, expected, null);

  [Test]
  [TestCase("", "")]
  [TestCase("abc", "abc")]
  [TestCase("a\nb", "a=0Ab")]
  [TestCase("a=b", "a=3Db")]
  public void QuotedPrintable(string input, string expected) {
    var result = input.ToQuotedPrintable();
    Assert.AreEqual(result, expected, "Error during quoted-printable encoding");
    Assert.AreEqual(result.FromQuotedPrintable(), input, "Error during quoted-printable decoding");
  }

  [Test]
  [TestCase(null, 0, null, null, typeof(NullReferenceException))]
  [TestCase("", 1, "a", "a")]
  [TestCase("ABC", -1, "a", "a", typeof(IndexOutOfRangeException))]
  [TestCase("ABC", 0, "a", "a", typeof(ArgumentOutOfRangeException))]
  [TestCase("ABC", 1, null, "A")]
  [TestCase("ABC", 1, "a", "Aa")]
  [TestCase("ABC", 10, "a", "ABCa")]
  public void ExchangeAt_WithFullStringReplacement(string input, int index, string replacement, string expected, Type? exception = null)
    => ExecuteTest(() => input.ExchangeAt(index, replacement), expected, exception);

  [Test]
  [TestCase(null, 0, 'a', "a", typeof(NullReferenceException))]
  [TestCase("ABC", -1, 'a', "aABC", typeof(IndexOutOfRangeException))]
  [TestCase("ABC", 0, 'a', "aBC")]
  [TestCase("", 0, 'a', "a")]
  [TestCase("A", 0, 'a', "a")]
  [TestCase("A", 1, 'a', "Aa")]
  [TestCase("ABC", 1, 'a', "AaC")]
  [TestCase("ABC", 2, 'a', "ABa")]
  [TestCase("ABC", 10, 'a', "ABCa")]
  public void ExchangeAt_WithSingleCharacterReplacement(string input, int index, char replacement, string expected, Type? exception = null)
    => ExecuteTest(() => input.ExchangeAt(index, replacement), expected, exception);

  [Test]
  [TestCase(null, 0, 0, null, null, typeof(NullReferenceException))]
  [TestCase("A", -1, 0, null, null, typeof(IndexOutOfRangeException))]
  [TestCase("A", 0, 0, null, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("", 0, 1, "a", "a")]
  [TestCase("ABC", 0, 1, "a", "aBC")]
  [TestCase("ABC", 0, 1, "aa", "aaBC")]
  [TestCase("ABC", 0, 2, "a", "aC")]
  [TestCase("ABC", 2, 1, "a", "ABa")]
  [TestCase("ABC", 2, 2, "a", "ABa")]
  [TestCase("ABC", 3, 1, "a", "ABCa")]
  [TestCase("ABC", 3, 1, "aa", "ABCaa")]
  public void ExchangeAt_WithStringCountReplacement(string input, int index, int length, string replacement, string expected, Type? exception = null)
    => ExecuteTest(() => input.ExchangeAt(index, length, replacement), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", null, false, typeof(ArgumentNullException))]
  [TestCase("a", "", false)]
  [TestCase("a", "a,b", true)]
  [TestCase("a", "b,c", false)]
  public void IsIn(string needle, string? haystack, bool expected, Type? exception = null) {
    var hay = haystack?.Split(',').Select(i => i.Trim());
    ExecuteTest(() => needle.IsIn(hay), expected, exception);
  }

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", null, false, typeof(ArgumentNullException))]
  [TestCase("a", "", true)]
  [TestCase("a", "a,b", false)]
  [TestCase("a", "b,c", true)]
  public void IsNotIn(string needle, string? haystack, bool expected, Type? exception = null) {
    var hay = haystack?.Split(',').Select(i => i.Trim());
    ExecuteTest(() => needle.IsNotIn(hay), expected, exception);
  }

  [Test]
  [TestCase(null, 0, null, typeof(NullReferenceException))]
  [TestCase("a", -1, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("a", 0, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("a", 1, null, typeof(ArgumentOutOfRangeException))]
  [TestCase("", 10, null, typeof(ArgumentException))]
  [TestCase("a", 2, "aa")]
  [TestCase("ab", 2, "abab")]
  [TestCase("ab", 3, "ababab")]
  [TestCase("ab", 4, "abababab")]
  [TestCase("ab", 5, "ababababab")]
  public void Repeat(string input, int count, string expected, Type? exception = null)
    => ExecuteTest(() => input.Repeat(count), expected, exception);

  [Test]
  [TestCase(null, 1, null, typeof(NullReferenceException))]
  [TestCase("", -1, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("", 0, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("", 1, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("ab", 2, "")]
  [TestCase("ab", 1, "a")]
  public void RemoveLast(string input, int count, string expected, Type? exception = null)
    => ExecuteTest(() => input.RemoveLast(count), expected, exception);

  [Test]
  [TestCase(null, 1, null, typeof(NullReferenceException))]
  [TestCase("", -1, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("", 0, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("", 1, "", typeof(ArgumentOutOfRangeException))]
  [TestCase("ab", 2, "")]
  [TestCase("ab", 1, "b")]
  public void RemoveFirst(string input, int count, string expected, Type? exception = null)
    => ExecuteTest(() => input.RemoveFirst(count), expected, exception);

  [Test]
  [TestCase(null, 0, 0, null, typeof(NullReferenceException))]
  [TestCase("", 0, 0, "")]
  [TestCase("abc", 0, 1, "a")]
  [TestCase("abc", 0, -1, "ab")]
  [TestCase("abc", -2, 1, "b")]
  [TestCase("abc", -2, 100, "bc")]
  [TestCase("abc", -100, 3, "abc")]
  [TestCase("abc", -100, -99, "")]
  public void SubString(string input, int start, int end, string expected, Type? exception = null)
    => ExecuteTest(() => input.SubString(start, end), expected, exception);

  [Test]
  [TestCase(null, 0, null, typeof(NullReferenceException))]
  [TestCase("", 0, "")]
  [TestCase("abc", 0, "")]
  [TestCase("abc", 2, "ab")]
  [TestCase("abc", 3, "abc")]
  [TestCase("abc", 100, "abc")]
  [TestCase("abc", -100, "", typeof(ArgumentOutOfRangeException))]

  public void Left(string input, int count, string expected, Type? exception = null)
    => ExecuteTest(() => input.Left(count), expected, exception);

  [Test]
  [TestCase(null, 0, null, typeof(NullReferenceException))]
  [TestCase("", 0, "")]
  [TestCase("abc", 0, "")]
  [TestCase("abc", 2, "bc")]
  [TestCase("abc", 3, "abc")]
  [TestCase("abc", 100, "abc")]
  [TestCase("abc", -100, "", typeof(ArgumentOutOfRangeException))]

  public void Right(string input, int count, string expected, Type? exception = null)
    => ExecuteTest(() => input.Right(count), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(NullReferenceException))]
  [TestCase("", "", null, typeof(ArgumentException))]
  [TestCase("a", "a")]
  [TestCase(":", "_")]
  [TestCase("abc", "abc")]
  [TestCase("C\\Users\\user\\", "C_Users_user_")]
  [TestCase("C:Usersuser", "C_Usersuser")]
  [TestCase("\"<>|\0\u0001\u0002\u0003\u0004\u0005\u0006", "___________")]
  [TestCase("C\"Usersuser", "C_Usersuser")]
  [TestCase("C<Usersuser", "C_Usersuser")]
  [TestCase("C>Usersuser", "C_Usersuser")]
  [TestCase("C\0Usersuser", "C_Usersuser")]
  [TestCase("C\u0001Usersuser", "C_Usersuser")]
  [TestCase("C\aUsersuser", "C_Usersuser")]
  [TestCase("C\u001FUsersuser", "C_Usersuser")]
  [TestCase("C*Usersuser", "C_Usersuser")]
  [TestCase("C?Usersuser", "C_Usersuser")]
  [TestCase("C/Usersuser", "C_Usersuser")]
  [TestCase(
    "12345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689:",
    "12345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689012345689_")]
  [TestCase("C:\\test\\demo?.tx*", "CXXtestXdemoX.txX", 'X')]
  public void SanitizeForFileName(string input, string expected, char? sanitation = null, Type? exception = null)
    => ExecuteTest(() => sanitation == null ? input.SanitizeForFileName() : input.SanitizeForFileName(sanitation.Value), expected, exception);

  [Test]
  [TestCase(null, "", null, typeof(NullReferenceException))]
  [TestCase("", null, null, typeof(ArgumentNullException))]
  [TestCase("", "", null, typeof(ArgumentException))]
  [TestCase("", "[/]", null, typeof(ArgumentException))]
  [TestCase("", "[<]", null, typeof(ArgumentException))]
  [TestCase("", "[\"]", null, typeof(ArgumentException))]
  [TestCase(".mp3", "?mp3", true)]
  [TestCase("abc.mp3", "*mp3", true)]
  [TestCase("a.abc.mp3", "*?mp3", true)]
  [TestCase("a.mp3", "a.mp3", true)]
  [TestCase("a.mp3abc", "a.mp3", true)]
  [TestCase("abc.mp", "*.mp", true)]
  [TestCase("abc.mp3", "?mp3", false)]
  [TestCase("abc.mp3", "*.mp", false)]

  public void MatchesFilePattern(string input, string pattern, bool expected, Type? exception = null)
    => ExecuteTest(() => input.MatchesFilePattern(pattern), expected, exception);

  [Test]
  [TestCase(null, @"^.*mp3$", false)]
  [TestCase("abc.mp3", @"^.*mp3$", true)]
  [TestCase("abc.mp", @"^.*mp3$", false)]

  public void IsMatch(string input, string regex, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsMatch(new(regex)), expected, exception);

  [Test]
  [TestCase(null, @"^.*mp3$", true)]
  [TestCase("abc.mp3", @"^.*mp3$", false)]
  [TestCase("abc.mp", @"^.*mp3$", true)]

  public void IsNotMatch(string input, string regex, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsNotMatch(new(regex)), expected, exception);

  [Test]
  [TestCase(null, @"^.*mp3$", RegexOptions.None, false)]
  [TestCase("abc.MP3", @"^.*mp3$", RegexOptions.IgnoreCase, true)]
  [TestCase("abc.mp", @"^.*mp3$", RegexOptions.None, false)]

  public void IsMatch(string input, string regex, RegexOptions regexOptions, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsMatch(new(regex, regexOptions)), expected, exception);

  [Test]
  [TestCase(null, @"^.*mp3$", RegexOptions.None, true)]
  [TestCase("abc.MP3", @"^.*mp3$", RegexOptions.IgnoreCase, false)]
  [TestCase("abc.mp", @"^.*mp3$", RegexOptions.None, true)]

  public void IsNotMatch(string input, string regex, RegexOptions regexOptions, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsNotMatch(new(regex, regexOptions)), expected, exception);

  [Test]
  [TestCaseSource(nameof(_TestMatchesTestData))]

  public void Matches(MatchesTestData data) {
    var resultProvider = () => data.Input.Matches(data.Regex, data.RegexOptions);

    if (data.Exception == null) {
      var resultMatchCollection = resultProvider.Invoke();
      for (var i = 0; i < resultMatchCollection.Count; ++i) {
        Assert.True(resultMatchCollection[i].ToString().Equals(data.Expected![i]));
      }
    } else {
      Assert.That(resultProvider, Throws.TypeOf(data.Exception));
    }
  }

  [Test]
  [TestCaseSource(nameof(_TestMatchGroupsTestData))]

  public void MatchGroups(MatchGroupsTestData data) {
    var resultProvider = () => data.Input.MatchGroups(data.Regex, data.RegexOptions);

    if (data.Exception == null) {
      var resultGroupCollection = resultProvider.Invoke();
      for (var i = 0; i < resultGroupCollection.Count; ++i) {
        Assert.True(resultGroupCollection[i].ToString().Equals(data.Expected![i]));
      }
    } else {
      Assert.That(resultProvider, Throws.TypeOf(data.Exception));
    }
  }

  [Test]
  [TestCaseSource(nameof(_TestFormatWithParameters))]

  public void FormatWith(FormatWithParametersTestData data)
    => ExecuteTest(() => data.Input.FormatWith(data.Parameters), data.Expected, data.Exception);

  [Test]
  [TestCaseSource(nameof(_TestFormatWithExParameters))]
  public void FormatWithEx(FormatWithExParametersTestData data)
    => ExecuteTest(() => data.Input.FormatWithEx(data.FieldGetter, data.PassFieldFormatToGetter), data.Expected, data.Exception);

  [Test]
  [TestCaseSource(nameof(_TestMultipleReplaceTestData))]
  public void MultipleReplace(MultipleReplaceTestData data)
    => ExecuteTest(() => data.Input.MultipleReplace(data.Replacements), data.Expected, data.Exception);

  [Test]
  [TestCase(null, @"^.*mp3$", RegexOptions.None, "", null, typeof(NullReferenceException))]
  [TestCase("abc.MP3", null, RegexOptions.None, "", null, typeof(ArgumentNullException))]
  [TestCase("abc.mp3", @"^.*mp3$", RegexOptions.None, null, "")]
  [TestCase("abc.MP", @"^.*mp3$", RegexOptions.IgnoreCase, null, "abc.MP")]
  [TestCase("abc.MP3", @"^.*mp3$", RegexOptions.IgnoreCase, "", "")]
  [TestCase("Bit and Bat", "B.t", RegexOptions.None, "BAT", "BAT and BAT")]
  [TestCase("BiT and BaT", "B.t", RegexOptions.IgnoreCase, "BAT", "BAT and BAT")]
  public void ReplaceRegex(string input, string regex, RegexOptions regexOptions, string newValue, string expected, Type? exception = null)
    => ExecuteTest(() => input.ReplaceRegex(regex, newValue, regexOptions), expected, exception);

  [Test]
  [TestCase(null, @"^.*mp3$", "", null, typeof(NullReferenceException))]
  [TestCase("abc.MP3", null, "", null, typeof(ArgumentNullException))]
  [TestCase("abc.mp3", @"^.*mp3$", "", "")]
  [TestCase("Bit and Bat", "B.t", "BAT", "BAT and BAT")]
  [TestCase("BiT and BaT", "B.t", "BAT", "BiT and BaT")]
  public void Replace(string input, string regex, string newValue, string expected, Type? exception = null)
    => ExecuteTest(() => input.Replace(new Regex(regex), newValue), expected, exception);

  [Test]
  [TestCase(null, "", "", 1, StringComparison.CurrentCulture, null)]
  [TestCase("", null, "", 1, StringComparison.CurrentCulture, "")]
  [TestCase("", "", "", 0, StringComparison.CurrentCulture, "")]
  public void Replace(string input, string oldValue, string newValue, int count, StringComparison comparison, string expected, Type? exception = null)
    => ExecuteTest(() => input.Replace(oldValue, newValue, count, comparison), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [TestCase("ı", "I")]
  [TestCase("AB", "AB")]
  [TestCase("ab", "Ab")]
  public void UpperFirst(string input, string expected, Type? exception = null) => ExecuteTest(() => input.UpperFirst(CultureInfo.GetCultureInfo("de-DE")), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("a", "A")]
  [TestCase("ı", "ı")]
  [TestCase("AB", "AB")]
  [TestCase("ab", "Ab")]
  public void UpperFirstInvariant(string input, string expected, Type? exception = null) => ExecuteTest(input.UpperFirstInvariant, expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("A", "a")]
  [TestCase("ϴ", "θ")]
  [TestCase("ab", "ab")]
  [TestCase("AB", "aB")]
  public void LowerFirst(string input, string expected, Type? exception = null) => ExecuteTest(() => input.LowerFirst(CultureInfo.GetCultureInfo("de-DE")), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("A", "a")]
  [TestCase("ϒ", "ϒ")]
  [TestCase("ab", "ab")]
  [TestCase("AB", "aB")]
  public void LowerFirstInvariant(string input, string expected, Type? exception = null) => ExecuteTest(input.LowerFirstInvariant, expected, exception);

  [Test]
  [TestCaseSource(nameof(_TestSplitTestData))]
  public void Split(SplitParametersTestData test) {
    IEnumerable<string> Execute() => test.Max == null ? test.Input.Split(test.Splitter) : test.Input.Split(test.Splitter, test.Max.Value);

    if (test.Exception == null)
      Assert.That(Execute(), Is.EquivalentTo(test.Expected!));
    else
      Assert.That(Execute, Throws.TypeOf(test.Exception!));
  }

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("CamelCase", "camelCase")]
  [TestCase("camelCase", "camelCase")]
  [TestCase("cAMelCase", "cAmelCase")]
  [TestCase("camel-case", "camelCase")]
  [TestCase("  camel-case", "camelCase")]
  [TestCase("camel-case  ", "camelCase")]
  [TestCase("camel1case", "camel1Case")]
  public void CamelCase(string? input, string? expected, Type? exception = null)
    => ExecuteTest(() => input.ToCamelCase(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", "")]
  [TestCase("PascalCase", "PascalCase")]
  [TestCase("pascalCase", "PascalCase")]
  [TestCase("pAScalCase", "PaScalCase")]
  [TestCase("pascal-case", "PascalCase")]
  [TestCase("  pascal-case", "PascalCase")]
  [TestCase("pascal-case  ", "PascalCase")]
  [TestCase("pascal1case", "Pascal1Case")]
  public void PascalCase(string? input, string? expected, Type? exception = null)
    => ExecuteTest(() => input.ToPascalCase(), expected, exception);

  [Test]
  [TestCase(null, null, null, StringComparison.Ordinal, null, typeof(NullReferenceException))]
  [TestCase("", null, null, StringComparison.Ordinal, "", typeof(ArgumentNullException))]
  [TestCase("", "", null, StringComparison.Ordinal, "")]
  [TestCase("", "", "", StringComparison.Ordinal, "")]
  [TestCase("abc", "a", null, StringComparison.Ordinal, "bc")]
  [TestCase("abc", "A", null, StringComparison.Ordinal, "abc")]
  [TestCase("abc", "A", null, StringComparison.OrdinalIgnoreCase, "bc")]
  [TestCase("abc", "A", "d", StringComparison.OrdinalIgnoreCase, "dbc")]
  [TestCase("abc", "A", "def", StringComparison.OrdinalIgnoreCase, "defbc")]
  [TestCase("abc", "ABCD", "def", StringComparison.OrdinalIgnoreCase, "abc")]
  public void ReplaceAtStart(string? input, string? what, string? replacement, StringComparison comparison, string? expected, Type? exception = null)
    => ExecuteTest(() => input.ReplaceAtStart(what, replacement, comparison), expected, exception);

  [Test]
  [TestCase(null, null, null, StringComparison.Ordinal, null, typeof(NullReferenceException))]
  [TestCase("", null, null, StringComparison.Ordinal, "", typeof(ArgumentNullException))]
  [TestCase("", "", null, StringComparison.Ordinal, "")]
  [TestCase("", "", "", StringComparison.Ordinal, "")]
  [TestCase("abc", "c", null, StringComparison.Ordinal, "ab")]
  [TestCase("abc", "C", null, StringComparison.Ordinal, "abc")]
  [TestCase("abc", "C", null, StringComparison.OrdinalIgnoreCase, "ab")]
  [TestCase("abc", "C", "d", StringComparison.OrdinalIgnoreCase, "abd")]
  [TestCase("abc", "C", "def", StringComparison.OrdinalIgnoreCase, "abdef")]
  [TestCase("abc", "ABCD", "def", StringComparison.OrdinalIgnoreCase, "abc")]
  public void ReplaceAtEnd(string? input, string? what, string? replacement, StringComparison comparison, string? expected, Type? exception = null)
    => ExecuteTest(() => input.ReplaceAtEnd(what, replacement, comparison), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(NullReferenceException))]
  [TestCase("", null, null, typeof(ArgumentNullException))]
  [TestCase("abc", "bc", "a")]
  [TestCase("abc", "ab", "abc")]
  public void TrimEnd(string? input, string? what, string? expected, Type? exception = null)
    => ExecuteTest(() => input.TrimEnd(what), expected, exception);

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase("a", false)]
  public void NullOrEmpty(string? input, bool expected) {
    Assert.That(input.IsNullOrEmpty(), Is.EqualTo(expected));
    Assert.That(input.IsNotNullOrEmpty(), Is.EqualTo(!expected));
  }

  [Test]
  [TestCase(null, true)]
  [TestCase("", true)]
  [TestCase(" \t\n\r\u0085", true)]
  [TestCase("    ", true)]
  [TestCase("a", false)]
  public void NullOrWhiteSpace(string? input, bool expected) {
    Assert.That(input.IsNullOrWhiteSpace(), Is.EqualTo(expected));
    Assert.That(input.IsNotNullOrWhiteSpace(), Is.EqualTo(!expected));
  }

  [Test]
  [TestCase(null,null,StringComparison.Ordinal,false,typeof(NullReferenceException))]
  [TestCase("", null, StringComparison.Ordinal, false, typeof(ArgumentNullException))]
  [TestCase("abc", "bc", StringComparison.Ordinal, true)]
  [TestCase("abc", "BC", StringComparison.Ordinal, false)]
  [TestCase("abc", "BC", StringComparison.OrdinalIgnoreCase, true)]
  [TestCase("a", "bc", StringComparison.Ordinal, false)]
  [TestCase("a", "", StringComparison.Ordinal, true)]
  [TestCase("a", "abc", StringComparison.Ordinal, false)]
  public void Contains(string? input, string? what, StringComparison comparison, bool expected, Type? exception = null)
    => ExecuteTest(() => input.Contains(what, comparison), expected, exception)
    ;

  [Test]
  [TestCase("", null, false, typeof(ArgumentNullException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "a|b", true)]
  [TestCase("a", "b|a", true)]
  [TestCase("a", "A|B", false)]
  [TestCase("", "x||z", true)]
  [TestCase(null, "-", true)]
  public void IsAnyOf(string? input, string? needlesSeparatedByPipes, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsAnyOf(needlesSeparatedByPipes?.Split('|').Select(i=>i=="-"?null:i)), expected, exception)
  ;

  [Test]
  [TestCase("", null, StringComparison.Ordinal, false, typeof(ArgumentNullException))]
  [TestCase("a", "a", StringComparison.Ordinal, true)]
  [TestCase("a", "a|b", StringComparison.Ordinal, true)]
  [TestCase("a", "b|a", StringComparison.Ordinal, true)]
  [TestCase("a", "A|B", StringComparison.Ordinal, false)]
  [TestCase("a", "A|B", StringComparison.OrdinalIgnoreCase, true)]
  [TestCase("", "x||z", StringComparison.Ordinal, true)]
  [TestCase(null, "-", StringComparison.Ordinal, true)]
  public void IsAnyOfWithComparer(string? input, string? needlesSeparatedByPipes, StringComparison comparison, bool expected, Type? exception = null)
    => ExecuteTest(() => input.IsAnyOf(needlesSeparatedByPipes?.Split('|').Select(i => i == "-" ? null : i), comparison), expected, exception)
    ;

}