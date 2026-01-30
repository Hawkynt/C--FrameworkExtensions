using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class CarouselControlTests {
  private CarouselControl _carousel;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._carousel = new CarouselControl();
    this._form = new Form();
    this._form.Controls.Add(this._carousel);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._carousel?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(0));
    Assert.That(this._carousel.AutoRotate, Is.False);
    Assert.That(this._carousel.AutoRotateInterval, Is.EqualTo(5000));
    Assert.That(this._carousel.ShowNavigation, Is.True);
    Assert.That(this._carousel.ShowIndicators, Is.True);
    Assert.That(this._carousel.Transition, Is.EqualTo(CarouselTransition.Slide));
    Assert.That(this._carousel.TransitionDuration, Is.EqualTo(300));
    Assert.That(this._carousel.EnableSwipe, Is.True);
    Assert.That(this._carousel.Loop, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Items_CanBeAdded() {
    var item = new CarouselItem { Title = "Test" };
    this._carousel.Items.Add(item);

    Assert.That(this._carousel.Items.Count, Is.EqualTo(1));
    Assert.That(this._carousel.CurrentItem, Is.EqualTo(item));
  }

  [Test]
  [Category("HappyPath")]
  public void CurrentIndex_CanBeChanged() {
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 3" });

    this._carousel.CurrentIndex = 1;

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(1));
    Assert.That(this._carousel.CurrentItem.Title, Is.EqualTo("Item 2"));
  }

  [Test]
  [Category("HappyPath")]
  public void Next_AdvancesToNextItem() {
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });

    this._carousel.Next();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Previous_GoesToPreviousItem() {
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });
    this._carousel.CurrentIndex = 1;

    this._carousel.Previous();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Next_AtLastItem_LoopsToFirst_WhenLoopEnabled() {
    this._carousel.Loop = true;
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });
    this._carousel.CurrentIndex = 1;

    this._carousel.Next();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Next_AtLastItem_StaysAtLast_WhenLoopDisabled() {
    this._carousel.Loop = false;
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });
    this._carousel.CurrentIndex = 1;

    this._carousel.Next();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Previous_AtFirstItem_LoopsToLast_WhenLoopEnabled() {
    this._carousel.Loop = true;
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });

    this._carousel.Previous();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Previous_AtFirstItem_StaysAtFirst_WhenLoopDisabled() {
    this._carousel.Loop = false;
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });

    this._carousel.Previous();

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GoTo_NavigatesToSpecificIndex() {
    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 3" });

    this._carousel.GoTo(2);

    Assert.That(this._carousel.CurrentIndex, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void AutoRotateInterval_CanBeSetAndRetrieved() {
    this._carousel.AutoRotateInterval = 3000;
    Assert.That(this._carousel.AutoRotateInterval, Is.EqualTo(3000));
  }

  [Test]
  [Category("EdgeCase")]
  public void AutoRotateInterval_HasMinimumValue() {
    this._carousel.AutoRotateInterval = 100;
    Assert.That(this._carousel.AutoRotateInterval, Is.EqualTo(500));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowNavigation_CanBeSetAndRetrieved() {
    this._carousel.ShowNavigation = false;
    Assert.That(this._carousel.ShowNavigation, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowIndicators_CanBeSetAndRetrieved() {
    this._carousel.ShowIndicators = false;
    Assert.That(this._carousel.ShowIndicators, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Transition_CanBeSetAndRetrieved() {
    this._carousel.Transition = CarouselTransition.Fade;
    Assert.That(this._carousel.Transition, Is.EqualTo(CarouselTransition.Fade));
  }

  [Test]
  [Category("HappyPath")]
  public void TransitionDuration_CanBeSetAndRetrieved() {
    this._carousel.TransitionDuration = 500;
    Assert.That(this._carousel.TransitionDuration, Is.EqualTo(500));
  }

  [Test]
  [Category("EdgeCase")]
  public void TransitionDuration_CannotBeNegative() {
    this._carousel.TransitionDuration = -100;
    Assert.That(this._carousel.TransitionDuration, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void EnableSwipe_CanBeSetAndRetrieved() {
    this._carousel.EnableSwipe = false;
    Assert.That(this._carousel.EnableSwipe, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Loop_CanBeSetAndRetrieved() {
    this._carousel.Loop = false;
    Assert.That(this._carousel.Loop, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void CurrentItem_ReturnsNull_WhenNoItems() {
    Assert.That(this._carousel.CurrentItem, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void ItemChanged_IsRaisedWhenIndexChanges() {
    var eventRaised = false;
    CarouselItem receivedItem = null;

    this._carousel.Items.Add(new CarouselItem { Title = "Item 1" });
    this._carousel.Items.Add(new CarouselItem { Title = "Item 2" });

    this._carousel.ItemChanged += (s, e) => {
      eventRaised = true;
      receivedItem = e.Item;
    };

    this._carousel.CurrentIndex = 1;
    Application.DoEvents();

    Assert.That(eventRaised, Is.True);
    Assert.That(receivedItem.Title, Is.EqualTo("Item 2"));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._carousel.Width, Is.EqualTo(400));
    Assert.That(this._carousel.Height, Is.EqualTo(250));
  }

  [Test]
  [Category("HappyPath")]
  public void CarouselItem_PropertiesWork() {
    using var image = new Bitmap(100, 100);
    var item = new CarouselItem {
      Image = image,
      Title = "Test Title",
      Description = "Test Description",
      Tag = "Custom Tag"
    };

    Assert.That(item.Image, Is.EqualTo(image));
    Assert.That(item.Title, Is.EqualTo("Test Title"));
    Assert.That(item.Description, Is.EqualTo("Test Description"));
    Assert.That(item.Tag, Is.EqualTo("Custom Tag"));
  }
}
