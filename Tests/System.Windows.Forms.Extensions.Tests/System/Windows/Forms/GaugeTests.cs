using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class GaugeTests {
  private Gauge _gauge;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._gauge = new Gauge();
    this._form = new Form();
    this._form.Controls.Add(this._gauge);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._gauge?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._gauge.Value, Is.EqualTo(0));
    Assert.That(this._gauge.Minimum, Is.EqualTo(0));
    Assert.That(this._gauge.Maximum, Is.EqualTo(100));
    Assert.That(this._gauge.StartAngle, Is.EqualTo(225));
    Assert.That(this._gauge.SweepAngle, Is.EqualTo(270));
    Assert.That(this._gauge.ShowTicks, Is.True);
    Assert.That(this._gauge.MajorTickCount, Is.EqualTo(5));
    Assert.That(this._gauge.MinorTickCount, Is.EqualTo(4));
    Assert.That(this._gauge.ShowValue, Is.True);
    Assert.That(this._gauge.ValueFormat, Is.EqualTo("{0:0}"));
    Assert.That(this._gauge.Unit, Is.EqualTo(string.Empty));
    Assert.That(this._gauge.NeedleColor, Is.EqualTo(Color.Red));
    Assert.That(this._gauge.DialColor, Is.EqualTo(Color.WhiteSmoke));
    Assert.That(this._gauge.Zones, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Value_CanBeSetAndRetrieved() {
    this._gauge.Value = 50;
    Assert.That(this._gauge.Value, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void ValueChanged_IsRaisedWhenValueChanges() {
    var eventRaised = false;
    this._gauge.ValueChanged += (s, e) => eventRaised = true;

    this._gauge.Value = 75;

    Assert.That(eventRaised, Is.True);
  }

  [Test]
  [Category("EdgeCase")]
  public void ValueChanged_NotRaisedWhenSameValue() {
    this._gauge.Value = 50;
    var eventRaised = false;
    this._gauge.ValueChanged += (s, e) => eventRaised = true;

    this._gauge.Value = 50;

    Assert.That(eventRaised, Is.False);
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToMinimum() {
    this._gauge.Minimum = 10;
    this._gauge.Value = 5;
    Assert.That(this._gauge.Value, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void Value_ClampedToMaximum() {
    this._gauge.Maximum = 100;
    this._gauge.Value = 150;
    Assert.That(this._gauge.Value, Is.EqualTo(100));
  }

  [Test]
  [Category("HappyPath")]
  public void Minimum_CanBeSetAndRetrieved() {
    this._gauge.Minimum = 10;
    Assert.That(this._gauge.Minimum, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void Maximum_CanBeSetAndRetrieved() {
    this._gauge.Maximum = 200;
    Assert.That(this._gauge.Maximum, Is.EqualTo(200));
  }

  [Test]
  [Category("HappyPath")]
  public void StartAngle_CanBeSetAndRetrieved() {
    this._gauge.StartAngle = 180;
    Assert.That(this._gauge.StartAngle, Is.EqualTo(180));
  }

  [Test]
  [Category("HappyPath")]
  public void SweepAngle_CanBeSetAndRetrieved() {
    this._gauge.SweepAngle = 180;
    Assert.That(this._gauge.SweepAngle, Is.EqualTo(180));
  }

  [Test]
  [Category("HappyPath")]
  public void Zones_CanBeSetAndRetrieved() {
    var zones = new[] {
      new GaugeZone(0, 30, Color.Green),
      new GaugeZone(30, 70, Color.Yellow),
      new GaugeZone(70, 100, Color.Red)
    };

    this._gauge.Zones = zones;

    Assert.That(this._gauge.Zones, Is.EqualTo(zones));
    Assert.That(this._gauge.Zones.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowTicks_CanBeSetAndRetrieved() {
    this._gauge.ShowTicks = false;
    Assert.That(this._gauge.ShowTicks, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void MajorTickCount_CanBeSetAndRetrieved() {
    this._gauge.MajorTickCount = 10;
    Assert.That(this._gauge.MajorTickCount, Is.EqualTo(10));
  }

  [Test]
  [Category("EdgeCase")]
  public void MajorTickCount_MinimumIsTwo() {
    this._gauge.MajorTickCount = 1;
    Assert.That(this._gauge.MajorTickCount, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void MinorTickCount_CanBeSetAndRetrieved() {
    this._gauge.MinorTickCount = 9;
    Assert.That(this._gauge.MinorTickCount, Is.EqualTo(9));
  }

  [Test]
  [Category("EdgeCase")]
  public void MinorTickCount_MinimumIsZero() {
    this._gauge.MinorTickCount = -1;
    Assert.That(this._gauge.MinorTickCount, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowValue_CanBeSetAndRetrieved() {
    this._gauge.ShowValue = false;
    Assert.That(this._gauge.ShowValue, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ValueFormat_CanBeSetAndRetrieved() {
    const string expected = "{0:0.0}";
    this._gauge.ValueFormat = expected;
    Assert.That(this._gauge.ValueFormat, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void ValueFormat_NullValue_BecomesDefault() {
    this._gauge.ValueFormat = null;
    Assert.That(this._gauge.ValueFormat, Is.EqualTo("{0:0}"));
  }

  [Test]
  [Category("HappyPath")]
  public void Unit_CanBeSetAndRetrieved() {
    const string expected = "km/h";
    this._gauge.Unit = expected;
    Assert.That(this._gauge.Unit, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void Unit_NullValue_BecomesEmpty() {
    this._gauge.Unit = null;
    Assert.That(this._gauge.Unit, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void NeedleColor_CanBeSetAndRetrieved() {
    var expected = Color.Blue;
    this._gauge.NeedleColor = expected;
    Assert.That(this._gauge.NeedleColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void DialColor_CanBeSetAndRetrieved() {
    var expected = Color.LightBlue;
    this._gauge.DialColor = expected;
    Assert.That(this._gauge.DialColor, Is.EqualTo(expected));
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._gauge.Width, Is.EqualTo(150));
    Assert.That(this._gauge.Height, Is.EqualTo(150));
  }

  [Test]
  [Category("HappyPath")]
  public void GaugeZone_DefaultConstructor_WorksCorrectly() {
    var zone = new GaugeZone();
    Assert.That(zone.Start, Is.EqualTo(0));
    Assert.That(zone.End, Is.EqualTo(0));
    Assert.That(zone.Color, Is.EqualTo(Color.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void GaugeZone_ParameterizedConstructor_WorksCorrectly() {
    var zone = new GaugeZone(10, 50, Color.Green);
    Assert.That(zone.Start, Is.EqualTo(10));
    Assert.That(zone.End, Is.EqualTo(50));
    Assert.That(zone.Color, Is.EqualTo(Color.Green));
  }
}
