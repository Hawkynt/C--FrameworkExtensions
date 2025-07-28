using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace System.Collections;

[TestFixture]
public class EnumerableExtensionsTests
{
    [Test]
    public void Count_Null_Throws()
    {
        IEnumerable source = null!;
        Assert.That(() => source.Count(), Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void Count_ReturnsNumberOfElements()
    {
        IEnumerable source = new[] {1,2,3};
        Assert.That(source.Count(), Is.EqualTo(3));
    }

    [Test]
    public void ForEach_InvokesActionForEachItem()
    {
        IEnumerable source = new[] {1,2,3};
        var list = new List<int>();
        source.ForEach<int>(list.Add);
        Assert.That(list, Is.EqualTo(new[]{1,2,3}));
    }

    [Test]
    public void ForEach_NullAction_Throws()
    {
        IEnumerable source = new[] {1};
        Assert.That(() => source.ForEach<int>(null!), Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ConvertAll_ConvertsElements()
    {
        IEnumerable source = new[] {1,2,3};
        var result = source.ConvertAll<int,string>(i => $"#{i}").Cast<string>().ToArray();
        Assert.That(result, Is.EqualTo(new[]{"#1","#2","#3"}));
    }

    [Test]
    public void ConvertAll_NullConverter_Throws()
    {
        IEnumerable source = new[] {1};
        Assert.That(
            () => source.ConvertAll<int, string>(null!).Cast<string>().ToArray(),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ToObjectArray_ReturnsObjects()
    {
        IEnumerable source = new[]{1,2};
        var result = source.ToObjectArray();
        Assert.That(result, Is.EqualTo(new object[]{1,2}));
    }

    [Test]
    public void ToObjectArray_Null_ReturnsNull()
    {
        IEnumerable source = null!;
        Assert.That(source.ToObjectArray(), Is.Null);
    }

    [Test]
    public void Any_WithElements_ReturnsTrue()
    {
        IEnumerable source = new[]{1};
        Assert.That(source.Any(), Is.True);
    }

    [Test]
    public void Any_Empty_ReturnsFalse()
    {
        IEnumerable source = Array.Empty<int>();
        Assert.That(source.Any(), Is.False);
    }

    [Test]
    public void Any_Null_Throws()
    {
        IEnumerable source = null!;
        Assert.That(() => source.Any(), Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void ToCache_CachesEnumeration()
    {
        var counter = 0;
        IEnumerable<int> Enumerate()
        {
            for(int i=0;i<3;i++)
            {
                counter++;
                yield return i;
            }
        }

        var cached = Enumerate().ToCache();
        Assert.That(counter, Is.EqualTo(0));
        var first = cached.ToArray();
        Assert.That(counter, Is.EqualTo(3));
        var second = cached.ToArray();
        Assert.That(counter, Is.EqualTo(3));
        Assert.That(second, Is.EqualTo(first));
    }

    [Test]
    public void ToCache_Null_Throws()
    {
        IEnumerable<int> source = null!;
        Assert.That(() => source.ToCache(), Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void ToBiDictionary_CreatesDictionary()
    {
        var data = new[]{("a",1),("b",2)};
        var dict = data.ToBiDictionary(t => t.Item1, t => t.Item2);
        Assert.That(dict.Count, Is.EqualTo(2));
        Assert.That(dict["a"], Is.EqualTo(1));
        Assert.That(dict.Reverse[1], Is.EqualTo("a"));
    }

    [Test]
    public void ToBiDictionary_NullEnumerable_Throws()
    {
        IEnumerable<(string,int)> source = null!;
        Assert.That(() => source.ToBiDictionary(t=>t.Item1,t=>t.Item2), Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void ToBiDictionary_NullSelectors_Throw()
    {
        var data = new[]{("a",1)};
        Assert.That(
            () => data.ToBiDictionary<(string, int), string, int>(null!, t => t.Item2),
            Throws.TypeOf<ArgumentNullException>());
        Assert.That(
            () => data.ToBiDictionary<(string, int), string, int>(t => t.Item1, null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void ToSortableBindingList_ReturnsList()
    {
        var data = new[]{1,2,3};
        var list = data.ToSortableBindingList();
        Assert.That(list, Is.InstanceOf<SortableBindingList<int>>());
        Assert.That(list, Is.EqualTo(data));
    }

    [Test]
    public void ToSortableBindingList_Null_Throws()
    {
        IEnumerable<int> source = null!;
        Assert.That(() => source.ToSortableBindingList(), Throws.TypeOf<NullReferenceException>());
    }

    [Test]
    public void ToNullIfEmpty_ReturnsNullForEmptyArray()
    {
        var result = Array.Empty<int>().ToNullIfEmpty();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ToNullIfEmpty_ReturnsSameForNonEmpty()
    {
        var input = new[]{1};
        var result = input.ToNullIfEmpty();
        Assert.That(ReferenceEquals(result, input));
    }

    [Test]
    public void ToNullIfEmpty_Null_ReturnsNull()
    {
        IEnumerable<int> source = null!;
        Assert.That(source.ToNullIfEmpty(), Is.Null);
    }
}
