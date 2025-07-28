using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace System.Collections.Concurrent;

[TestFixture]
internal class ConcurrentDictionaryTests {
  [Test]
  public void AddOrUpdate_Adds_And_Updates_Value() {
    var dict = new ConcurrentDictionary<int, string>();
    dict.AddOrUpdate(1, "first");
    dict.AddOrUpdate(1, "second");

    Assert.That(dict[1], Is.EqualTo("second"));
  }

  [Test]
  public void AddOrUpdate_Null_Throws() {
    ConcurrentDictionary<int, int>? dict = null;

    Assert.Throws<NullReferenceException>(() => dict!.AddOrUpdate(1, 1));
  }

  [Test]
  public void Add_With_Function_Returns_Unique_Keys() {
    var dict = new ConcurrentDictionary<int, string>();
    int counter = 0;
    var key1 = dict.Add("a", () => ++counter);
    var key2 = dict.Add("b", () => ++counter);

    Assert.That(key1, Is.Not.EqualTo(key2));
    Assert.That(dict[key1], Is.EqualTo("a"));
    Assert.That(dict[key2], Is.EqualTo("b"));
  }

  [Test]
  public void Add_With_Function_Null_Throws() {
    var dict = new ConcurrentDictionary<int, string>();

    Assert.Throws<ArgumentNullException>(() => dict.Add("a", (Func<int>)null!));
  }

  [Test]
  public void Add_With_Enumerator_Uses_First_Free_Key() {
    var dict = new ConcurrentDictionary<int, string>();
    dict[1] = "x";
    var keys = new[] { 1, 2, 3 };

    var resultKey = dict.Add("y", keys.AsEnumerable().GetEnumerator());

    Assert.That(resultKey, Is.EqualTo(2));
    Assert.That(dict[2], Is.EqualTo("y"));
  }

  [Test]
  public void Add_With_Enumerator_Null_Throws() {
    var dict = new ConcurrentDictionary<int, string>();

    Assert.Throws<ArgumentNullException>(() => dict.Add("a", (IEnumerator<int>)null!));
  }

  [Test]
  public void Add_With_Enumerable_Uses_First_Key() {
    var dict = new ConcurrentDictionary<int, string>();
    dict[3] = "c";
    var keys = new[] { 3, 4, 5 };

    var resultKey = dict.Add("d", keys);

    Assert.That(resultKey, Is.EqualTo(4));
    Assert.That(dict[4], Is.EqualTo("d"));
  }

  [Test]
  public void Add_With_Enumerable_Null_Throws() {
    var dict = new ConcurrentDictionary<int, string>();

    Assert.Throws<NullReferenceException>(() => dict.Add("a", (IEnumerable<int>)null!));
  }

  [Test]
  public void TryGetKey_Finds_Value() {
    var dict = new ConcurrentDictionary<int, string>();
    dict[4] = "four";
    dict[5] = "five";

    var found = dict.TryGetKey("five", out var key);

    Assert.That(found, Is.True);
    Assert.That(key, Is.EqualTo(5));
  }

  [Test]
  public void TryGetKey_Null_Dictionary_Throws() {
    ConcurrentDictionary<int, string>? dict = null;

    Assert.Throws<NullReferenceException>(() => dict!.TryGetKey("x", out _));
  }

  [Test]
  public void Remove_Removes_Key() {
    var dict = new ConcurrentDictionary<int, int>();
    dict[2] = 20;

    var removed = dict.Remove(2);

    Assert.That(removed, Is.True);
    Assert.That(dict.ContainsKey(2), Is.False);
  }

  [Test]
  public void Remove_Null_Dictionary_Throws() {
    ConcurrentDictionary<int, int>? dict = null;

    Assert.Throws<NullReferenceException>(() => dict!.Remove(1));
  }

  [Test]
  public void GetOrAdd_ReturnsExistingOrAdds() {
    var dict = new ConcurrentDictionary<string, string>();
    var first = dict.GetOrAdd("z");
    var second = dict.GetOrAdd("z");

    Assert.That(first, Is.EqualTo("z"));
    Assert.That(second, Is.EqualTo("z"));
    Assert.That(dict.ContainsKey("z"), Is.True);
  }

  [Test]
  public void GetOrAdd_Null_Dictionary_Throws() {
    ConcurrentDictionary<string, string>? dict = null;

    Assert.Throws<NullReferenceException>(() => dict!.GetOrAdd("x"));
  }
}
