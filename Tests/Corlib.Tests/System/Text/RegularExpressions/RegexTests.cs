using NUnit.Framework;
using static Corlib.Tests.NUnit.TestUtilities;

namespace System.Text.RegularExpressions;

[TestFixture]
internal class RegexTests {
  [Test]
  [TestCase(null, RegexOptions.None, null, RegexOptions.None, "", false, typeof(NullReferenceException))]
  [TestCase("abc", RegexOptions.None, null, RegexOptions.None, "", false, typeof(ArgumentNullException))]
  [TestCase("abc", RegexOptions.ECMAScript, "def", RegexOptions.None, "", false, typeof(InvalidOperationException))]
  [TestCase("abc", RegexOptions.None, "def", RegexOptions.CultureInvariant, "", false, typeof(InvalidOperationException))]
  [TestCase("abc", RegexOptions.RightToLeft, "def", RegexOptions.None, "", false, typeof(InvalidOperationException))]
  [TestCase("abc", RegexOptions.RightToLeft, "def", RegexOptions.RightToLeft, "abc", true)]
  [TestCase("abc", RegexOptions.None, "def", RegexOptions.None, "def", true)]
  [TestCase("abc", RegexOptions.None, "def", RegexOptions.None, "123", false)]
  [TestCase("abc", RegexOptions.IgnoreCase, "def", RegexOptions.None, "ABC", true)]
  [TestCase("abc", RegexOptions.IgnoreCase, "def", RegexOptions.None, "DEF", false)]
  [TestCase("abc", RegexOptions.None, "def", RegexOptions.IgnoreCase, "DEF", true)]
  [TestCase("abc", RegexOptions.None, "def", RegexOptions.IgnoreCase, "ABC", false)]
  public static void Or(string? first, RegexOptions firstOptions, string? second, RegexOptions secondOptions, string test, bool expected, Type? exception = null)
    => ExecuteTest(() => (first == null ? null : new Regex(first, firstOptions)).Or(second == null ? null : new Regex(second, secondOptions)).IsMatch(test), expected, exception);
}
