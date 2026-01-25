using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class RangeSliderTests {
  private RangeSlider _slider;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._slider = new RangeSlider();
    this._form = new Form();
    this._form.Controls.Add(this._slider);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._slider?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._slider.Minimum, Is.EqualTo(0));
    Assert.That(this._slider.Maximum, Is.EqualTo(100));
    Assert.That(this._slider.LowerValue, Is.EqualTo(0));
    Assert.That(this._slider.UpperValue, Is.EqualTo(100));
    Assert.That(this._slider.SmallChange, Is.EqualTo(1));
    Assert.That(this._slider.LargeChange, Is.EqualTo(10));
    Assert.That(this._slider.SnapToTicks, Is.False);
    Assert.That(this._slider.TickFrequency, Is.EqualTo(10));
    Assert.That(this._slider.Orientation, Is.EqualTo(Orientation.Horizontal));
    Assert.That(this._slider.TrackColor, Is.EqualTo(Color.LightGray));
    Assert.That(this._slider.RangeColor, Is.EqualTo(Color.DodgerBlue));
    Assert.That(this._slider.ThumbColor, Is.EqualTo(Color.White));
  }

  [Test]
  [Category("HappyPath")]
  public void LowerValue_CanBeSetAndRetrieved() {
    this._slider.LowerValue = 25;
    Assert.That(this._slider.LowerValue, Is.EqualTo(25));
  }

  [Test]
  [Category("HappyPath")]
  public void UpperValue_CanBeSetAndRetrieved() {
    this._slider.UpperValue = 75;
    Assert.That(this._slider.UpperValue, Is.EqualTo(75));
  }

  [Test]
  [Category("HappyPath")]
  public void LowerValueChanged_IsRaisedWhenValueChanges() {
    var eventRaised = false;
    this._slider.LowerValueChanged += (s, e) => eventRaised = true;

    this._slider.LowerValue = 30;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void UpperValueChanged_IsRaisedWhenValueChanges() {
    var eventRaised = false;
    this._slider.UpperValueChanged += (s, e) => eventRaised = true;

    this._slider.UpperValue = 70;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void RangeChanged_IsRaisedWhenLowerValueChanges() {
    var eventRaised = false;
    this._slider.RangeChanged += (s, e) => eventRaised = true;

    this._slider.LowerValue = 20;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void RangeChanged_IsRaisedWhenUpperValueChanges() {
    var eventRaised = false;
    this._slider.RangeChanged += (s, e) => eventRaised = true;

    this._slider.UpperValue = 80;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void LowerValue_ClampedToMinimum() {
    this._slider.LowerValue = -50;
    Assert.That(this._slider.LowerValue, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void LowerValue_ClampedToUpperValue() {
    this._slider.UpperValue = 50;
    this._slider.LowerValue = 75;
    Assert.That(this._slider.LowerValue, Is.EqualTo(50));
  }

  [Test]
  [Category("EdgeCase")]
  public void UpperValue_ClampedToMaximum() {
    this._slider.UpperValue = 150;
    Assert.That(this._slider.UpperValue, Is.EqualTo(100));
  }

  [Test]
  [Category("EdgeCase")]
  public void UpperValue_ClampedToLowerValue() {
    this._slider.LowerValue = 50;
    this._slider.UpperValue = 25;
    Assert.That(this._slider.UpperValue, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Minimum_CanBeSetAndRetrieved() {
    this._slider.Minimum = 10;
    Assert.That(this._slider.Minimum, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Minimum_AdjustsLowerValueWhenIncreased() {
    this._slider.LowerValue = 20;
    this._slider.Minimum = 30;
    Assert.That(this._slider.LowerValue, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void Maximum_CanBeSetAndRetrieved() {
    this._slider.Maximum = 200;
    Assert.That(this._slider.Maximum, Is.EqualTo(200));
  }

  [Test]
  [Category("EdgeCase")]
  public void Maximum_AdjustsUpperValueWhenDecreased() {
    this._slider.UpperValue = 80;
    this._slider.Maximum = 50;
    Assert.That(this._slider.UpperValue, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void SmallChange_CanBeSetAndRetrieved() {
    this._slider.SmallChange = 5;
    Assert.That(this._slider.SmallChange, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void SmallChange_NegativeValue_BecomesZero() {
    this._slider.SmallChange = -5;
    Assert.That(this._slider.SmallChange, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void LargeChange_CanBeSetAndRetrieved() {
    this._slider.LargeChange = 20;
    Assert.That(this._slider.LargeChange, Is.EqualTo(20));
  }

  [Test]
  [Category("EdgeCase")]
  public void LargeChange_NegativeValue_BecomesZero() {
    this._slider.LargeChange = -10;
    Assert.That(this._slider.LargeChange, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SnapToTicks_CanBeSetAndRetrieved() {
    this._slider.SnapToTicks = true;
    Assert.That(this._slider.SnapToTicks, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void TickFrequency_CanBeSetAndRetrieved() {
    this._slider.TickFrequency = 5;
    Assert.That(this._slider.TickFrequency, Is.EqualTo(5));
  }

  [Test]
  [Category("EdgeCase")]
  public void TickFrequency_MinimumValue() {
    this._slider.TickFrequency = 0;
    Assert.That(this._slider.TickFrequency, Is.EqualTo(0.001).Within(0.0001));
  }

  [Test]
  [Category("HappyPath")]
  public void Orientation_CanBeSetAndRetrieved() {
    this._slider.Orientation = Orientation.Vertical;
    Assert.That(this._slider.Orientation, Is.EqualTo(Orientation.Vertical));
  }

  [Test]
  [Category("HappyPath")]
  public void TrackColor_CanBeSetAndRetrieved() {
    var expected = Color.DarkGray;
    this._slider.TrackColor = expected;
    Assert.That(this._slider.TrackColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void RangeColor_CanBeSetAndRetrieved() {
    var expected = Color.Green;
    this._slider.RangeColor = expected;
    Assert.That(this._slider.RangeColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void ThumbColor_CanBeSetAndRetrieved() {
    var expected = Color.Yellow;
    this._slider.ThumbColor = expected;
    Assert.That(this._slider.ThumbColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._slider.Width, Is.EqualTo(200));
    Assert.That(this._slider.Height, Is.EqualTo(30));
  }

  [Test]
  [Category("HappyPath")]
  public void SnapToTicks_AffectsValueSetting() {
    this._slider.TickFrequency = 10;
    this._slider.SnapToTicks = true;
    this._slider.LowerValue = 23;
    Assert.That(this._slider.LowerValue, Is.EqualTo(20));
  }
}
