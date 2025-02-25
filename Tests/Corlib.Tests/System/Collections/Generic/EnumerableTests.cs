using static Corlib.Tests.NUnit.TestUtilities;
using NUnit.Framework;
using System.Linq;

namespace System.Collections.Generic;

[TestFixture]
public class EnumerableTests {

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsNoSingle(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoSingle(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", false)]
  [TestCase("a", false)]
  [TestCase("a|a", true)]
  public void IsMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(NullReferenceException))]
  [TestCase("", true)]
  [TestCase("a", true)]
  [TestCase("a|a", false)]
  public void IsNoMultiple(string? input, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).IsNoMultiple(), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSingle(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiple(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", false)]
  public void HasSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", true)]
  public void HasNoSinglePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoSingle(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", false)]
  [TestCase("a", "b", false)]
  [TestCase("a|a", "a", true)]
  public void HasMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasMultiple(s => s == search), expected, exception);

  [Test]
  [TestCase(null, null, false, typeof(NullReferenceException))]
  [TestCase("a", "a", true)]
  [TestCase("a", "b", true)]
  [TestCase("a|a", "a", false)]
  public void HasNoMultiplePredicate(string? input, string? search, bool expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).HasNoMultiple(s => s == search), expected, exception);

  [Test]
  [TestCase(null, false, null, typeof(ArgumentNullException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "a")]
  public void TryGetFirst(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetFirst(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(ArgumentNullException))]
  [TestCase("", false, null)]
  [TestCase("a", true, "a")]
  [TestCase("a|b", true, "b")]
  public void TryGetLast(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetLast(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, 0, false, null, typeof(NullReferenceException))]
  [TestCase("", -1, false, null, typeof(IndexOutOfRangeException))]
  [TestCase("a", 1, false, null)]
  [TestCase("a|b|c", 1, true, "b")]
  public void TryGetItem(string? input, int index, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetItem(index, out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a|b", true, "b")]
  [TestCase("b|a", true, "b")]
  public void TryGetMax(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMax(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("a|b", true, "a")]
  [TestCase("b|a", true, "a")]
  public void TryGetMin(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMin(out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("ab|ba", true, "ab")]
  [TestCase("ba|ab", true, "ab")]
  public void TryGetMaxBy(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMaxBy(i => i![1], out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, false, null, typeof(NullReferenceException))]
  [TestCase("", false, null)]
  [TestCase("ab|ba", true, "ba")]
  [TestCase("ba|ab", true, "ba")]
  public void TryGetMinBy(string? input, bool result, string? expected, Type? exception = null) {
    ExecuteTest(() => {
      var r = ConvertFromStringToTestArray(input).TryGetMinBy(i => i![1], out var v);
      return (r, v);
    }, (result, expected), exception);
  }

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("a", "b", "b|a")]
  public void Prepend(string? input, string data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(data).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("a", "b", "a|b")]
  public void Append(string? input, string data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(data).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "b|a")]
  public void PrependArray(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(ConvertFromStringToTestArray(data)?.ToArray()).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "a|b")]
  public void AppendArray(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(ConvertFromStringToTestArray(data)?.ToArray()).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "b|a")]
  public void PrependEnumerable(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Prepend(ConvertFromStringToTestArray(data)).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", null, "", typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("", "a|b", "a|b")]
  [TestCase("a", "b", "a|b")]
  public void AppendEnumerable(string? input, string? data, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input)!.Append(ConvertFromStringToTestArray(data)).Join("|"), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(NullReferenceException))]
  [TestCase("", "", null)]
  [TestCase("", "", "")]
  [TestCase("a", "a", null)]
  [TestCase("a|b", "ab", null)]
  [TestCase("a|b", "ab", "")]
  [TestCase("a|b", "a,b", ",")]
  public void Join(string? input, string? expected, string delimiter = ",", Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).Join(delimiter), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase(null, null, StringComparison.OrdinalIgnoreCase, typeof(ArgumentNullException))]
  [TestCase("", "")]
  [TestCase("", "", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a", "a")]
  [TestCase("a", "a", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|b", "a|b")]
  [TestCase("a|b", "a|b", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|a", "a")]
  [TestCase("a|a", "a", StringComparison.OrdinalIgnoreCase)]
  [TestCase("a|A", "a|A")]
  [TestCase("a|A", "a", StringComparison.OrdinalIgnoreCase)]
  public void ToHashSet(string? input, string? expected, StringComparison? comparison = null, Type? exception = null)
    => ExecuteTest(() =>
        (
          comparison == null
            ? ConvertFromStringToTestArray(input)!
              .ToHashSet()
            : ConvertFromStringToTestArray(input)!
              .ToHashSet(FromComparison(comparison.Value))
        )
        .OrderBy()
        .Join("|"),
      expected,
      exception
    );

  [Test]
  [TestCase(null, null, typeof(ArgumentNullException))]
  [TestCase("", null)]
  [TestCase("a", "a")]
  [TestCase("a|b", "a")]
  [TestCase("!|b", null)]
  public void FirstOrNull(string? input, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).FirstOrNull(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(ArgumentNullException))]
  [TestCase("", null)]
  [TestCase("a", "a")]
  [TestCase("a|b", "b")]
  [TestCase("a|!", null)]
  public void LastOrNull(string? input, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).LastOrNull(), expected, exception);

  [Test]
  [TestCase(null, null, typeof(ArgumentNullException))]
  [TestCase("", null)]
  [TestCase("a", "a")]
  [TestCase("a|b", null)]
  [TestCase("!", null)]
  public void SingleOrNull(string? input, string? expected, Type? exception = null)
    => ExecuteTest(() => ConvertFromStringToTestArray(input).SingleOrNull(), expected, exception);

  [Test]
  [TestCase(null, null, null, typeof(ArgumentNullException))]
  [TestCase("", "a", "a")]
  [TestCase("b", "a", "b")]
  [TestCase("b|c", "a", "a",typeof(InvalidOperationException))]
  [TestCase("!", "a", null)]
  public void SingleOrDefault(string? input, string? @default, string? expected, Type? exception = null) {
    var self = ConvertFromStringToTestArray(input);
    ExecuteTest(() => self!.SingleOrDefault(@default), expected, exception);
    ExecuteTest(() => self.SingleOrDefault(() => @default), expected, exception);
    ExecuteTest(() => self.SingleOrDefault(_ => @default), expected, exception);
    ExecuteTest(() => self.SingleOrDefault((Func<string?>)null!), expected, typeof(ArgumentNullException));
    ExecuteTest(() => self.SingleOrDefault((Func<IEnumerable<string?>, string?>)null!), expected, typeof(ArgumentNullException));
  }
  
  private class Dummy {
    public Dummy(string data) => this.Data = data;

    public string Data { get; }
    public string DataReversed => new(this.Data.Reverse().ToArray());
  }

  [Test]
  [TestCase(null, null, false, null, typeof(NullReferenceException))]
  [TestCase("", null, false, "")]
  [TestCase("a", null, false, "a")]
  [TestCase("a|b", null, false, "a|b")]
  [TestCase("a|b", "a", false, "a")]
  [TestCase("a|b", "c", false, "")]
  [TestCase("a b|b c|c d", "b", false, "a b|b c")]
  [TestCase("a b|b c|c d", "b a", false, "a b")]
  [TestCase("a|b", "A", false, "")]
  [TestCase("a|b", "A", true, "a")]
  public void FilterIfNeeded(string? input, string? filter, bool ignoreCase, string? expected, Type? exception = null)
    => ExecuteTest(
        () => (input == null ? null : ConvertFromStringToTestArray(input)?.Select(s => s == null ? null : new Dummy(s))).FilterIfNeeded(d => d.Data, filter, ignoreCase).Select(d => d.Data),
        ConvertFromStringToTestArray(expected),
        exception
      )
    ;

  [Test]
  [TestCase(null, null, false, null, typeof(NullReferenceException))]
  [TestCase("", null, false, "")]
  [TestCase("a", null, false, "a")]
  [TestCase("a|b", null, false, "a|b")]
  [TestCase("a|b", "a", false, "a")]
  [TestCase("a|b", "c", false, "")]
  [TestCase("a b|b c|c d", "b", false, "a b|b c")]
  [TestCase("a b|b c|c d", "b a", false, "a b")]
  [TestCase("a|b", "A", false, "")]
  [TestCase("a|b", "A", true, "a")]
  [TestCase("ab|bc", "BA", false, "")]
  [TestCase("ab|bc", "ba", false, "ab")]
  [TestCase("ab|bc", "BA", true, "ab")]
  public void FilterIfNeeded2(string? input, string? filter, bool ignoreCase, string? expected, Type? exception = null)
    => ExecuteTest(
      () => (input == null ? null : ConvertFromStringToTestArray(input)?.Select(s => s == null ? null : new Dummy(s))).FilterIfNeeded(filter, ignoreCase, d => d.Data, d => d.DataReversed).Select(d => d.Data),
      ConvertFromStringToTestArray(expected),
      exception
    )
  ;

  [Test]
  [TestCase(-1,1337)]
  [TestCase(0, 1337)]
  [TestCase(1, 1337)]
  [TestCase(16, 1337)]
  public void ShuffleShouldShuffle(int elementCount, int seed) {
    var input = elementCount < 0 ? null : elementCount == 0 ? [] : Enumerable.Range(0, elementCount).ToArray();

    // null should throw
    if (input == null) {
      Assert.That(() => input.Shuffled(), Throws.TypeOf<NullReferenceException>());
      return;
    }

    // shuffling using the stream should match length and not modify the source
    var outputEnumeration = Enumerable.ToArray(input.Shuffled(new(seed)));
    Assert.That(outputEnumeration.Length, Is.EqualTo(elementCount));
    Assert.That(input, Is.EqualTo(Enumerable.Range(0, elementCount)),"Should not modify source array");

    // shuffling using ToArray should match length and not modify the source
    var outputArray = input.Shuffled(new(seed)).ToArray();
    Assert.That(outputArray.Length, Is.EqualTo(elementCount));
    Assert.That(input, Is.EqualTo(Enumerable.Range(0, elementCount)), "Should not modify source array");

    // shuffling using ToList should match length and not modify the source
    var outputList = input.Shuffled(new(seed)).ToList();
    Assert.That(outputList.Count, Is.EqualTo(elementCount));
    Assert.That(input, Is.EqualTo(Enumerable.Range(0, elementCount)), "Should not modify source array");

    switch (input.Length) {
      case <= 0:
        return;
      case 1:
        Assert.That(outputEnumeration[0], Is.EqualTo(input[0]), "One element alone can't be shuffled");
        Assert.That(outputArray[0], Is.EqualTo(input[0]), "One element alone can't be shuffled");
        Assert.That(outputList[0], Is.EqualTo(input[0]), "One element alone can't be shuffled");
        return;
      default:
        Assert.That(outputEnumeration, Is.Not.EqualTo(input),"Shuffling should yield a different element order");
        Assert.That(outputEnumeration, Is.EquivalentTo(input),"Shuffling should return all source elements");

        Assert.That(outputArray, Is.Not.EqualTo(input), "Shuffling should yield a different element order");
        Assert.That(outputArray, Is.EquivalentTo(input), "Shuffling should return all source elements");

        Assert.That(outputList, Is.Not.EqualTo(input), "Shuffling should yield a different element order");
        Assert.That(outputList, Is.EquivalentTo(input), "Shuffling should return all source elements");
        break;
    }

    var shuffled = input.Shuffled(new(seed));
    Assert.That(Enumerable.ToArray(shuffled), Is.Not.EqualTo(shuffled),"Multiple enumerations should yield different ordering");
    Assert.That(shuffled.ToArray(),Is.Not.EqualTo(shuffled.ToArray()), "Multiple materialization should yield different ordering");
    Assert.That(shuffled.ToList(), Is.Not.EqualTo(shuffled.ToList()), "Multiple materialization should yield different ordering");
  }

}