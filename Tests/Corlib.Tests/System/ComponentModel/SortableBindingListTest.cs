using NUnit.Framework;

namespace System.ComponentModel;

[TestFixture]
public class SortableBindingListTest {

  #region nested types

  public class Item {
    public Item(string stringValue, int intValue, double doubleValue, bool boolValue) {
      this.StringValue = stringValue;
      this.IntValue = intValue;
      this.DoubleValue = doubleValue;
      this.BoolValue = boolValue;
    }

    public string StringValue { get; }
    public int IntValue { get; }
    public double DoubleValue { get; }
    public bool BoolValue { get; }

  }

  #endregion

  private static Item _GetItemById(int id) => new(
    id.ToString(),
    id,
    id,
    id.IsEven()
  );

  [Test]
  [TestCase(ListSortDirection.Ascending, 1, 0)]
  [TestCase(ListSortDirection.Ascending, 3, 1)]
  [TestCase(ListSortDirection.Ascending, 5, 2)]
  [TestCase(ListSortDirection.Descending, 1, 2)]
  [TestCase(ListSortDirection.Descending, 3, 1)]
  [TestCase(ListSortDirection.Descending, 5, 0)]
  public void AddTest(ListSortDirection direction, int value, int expected) {

    // Arrange
    var numbers = new[] { 2, 4 };
    var bs = new SortableBindingList<Item> {IsAutomaticallySorted = true};
    foreach (var i in numbers)
      bs.Add(_GetItemById(i));

    bs.Sort(nameof(Item.IntValue), direction);

    // Act
    var newItem = _GetItemById(value);
    bs.Add(newItem);

    // Assert
    Assert.AreEqual(bs[expected].IntValue, newItem.IntValue);
  }

  [Test]
  [TestCase(ListSortDirection.Ascending, 1, 0)]
  [TestCase(ListSortDirection.Ascending, 3, 1)]
  [TestCase(ListSortDirection.Ascending, 5, 2)]
  [TestCase(ListSortDirection.Descending, 1, 2)]
  [TestCase(ListSortDirection.Descending, 3, 1)]
  [TestCase(ListSortDirection.Descending, 5, 0)]
  public void AddRangeTest(ListSortDirection direction, int value,int expected) {
    
    // Arrange
    var numbers = new[] { 2, 4 };
    var bs = new SortableBindingList<Item> { IsAutomaticallySorted = true };
    foreach (var i in numbers)
      bs.Add(_GetItemById(i));

    bs.Sort(nameof(Item.IntValue), direction);

    // Act
    var newItem = _GetItemById(value);
    bs.AddRange(new []{newItem});

    // Assert
    Assert.AreEqual(bs[expected].IntValue, newItem.IntValue);
  }


}
