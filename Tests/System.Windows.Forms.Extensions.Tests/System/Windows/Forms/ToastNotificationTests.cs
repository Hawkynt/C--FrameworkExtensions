using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ToastNotificationTests {
  private ToastNotification _toast;

  [SetUp]
  public void Setup() {
    this._toast = new ToastNotification();
  }

  [TearDown]
  public void TearDown() {
    this._toast?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._toast.Message, Is.EqualTo(string.Empty));
    Assert.That(this._toast.Title, Is.EqualTo(string.Empty));
    Assert.That(this._toast.Type, Is.EqualTo(ToastType.Info));
    Assert.That(this._toast.Duration, Is.EqualTo(3000));
    Assert.That(this._toast.ShowCloseButton, Is.True);
    Assert.That(this._toast.Icon, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void Message_CanBeSetAndRetrieved() {
    const string expected = "Test message";
    this._toast.Message = expected;
    Assert.That(this._toast.Message, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void Message_NullValue_BecomesEmpty() {
    this._toast.Message = null;
    Assert.That(this._toast.Message, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Title_CanBeSetAndRetrieved() {
    const string expected = "Test title";
    this._toast.Title = expected;
    Assert.That(this._toast.Title, Is.EqualTo(expected));
  }

  [Test]
  [Category("EdgeCase")]
  public void Title_NullValue_BecomesEmpty() {
    this._toast.Title = null;
    Assert.That(this._toast.Title, Is.EqualTo(string.Empty));
  }

  [Test]
  [Category("HappyPath")]
  public void Type_CanBeSetAndRetrieved() {
    this._toast.Type = ToastType.Success;
    Assert.That(this._toast.Type, Is.EqualTo(ToastType.Success));

    this._toast.Type = ToastType.Warning;
    Assert.That(this._toast.Type, Is.EqualTo(ToastType.Warning));

    this._toast.Type = ToastType.Error;
    Assert.That(this._toast.Type, Is.EqualTo(ToastType.Error));
  }

  [Test]
  [Category("HappyPath")]
  public void Duration_CanBeSetAndRetrieved() {
    this._toast.Duration = 5000;
    Assert.That(this._toast.Duration, Is.EqualTo(5000));
  }

  [Test]
  [Category("EdgeCase")]
  public void Duration_NegativeValue_BecomesZero() {
    this._toast.Duration = -1000;
    Assert.That(this._toast.Duration, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowCloseButton_CanBeSetAndRetrieved() {
    this._toast.ShowCloseButton = false;
    Assert.That(this._toast.ShowCloseButton, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._toast.Width, Is.EqualTo(300));
    Assert.That(this._toast.Height, Is.EqualTo(80));
  }

  [Test]
  [Category("HappyPath")]
  public void ToastManager_DefaultPosition_CanBeSetAndRetrieved() {
    ToastManager.DefaultPosition = ToastPosition.TopRight;
    Assert.That(ToastManager.DefaultPosition, Is.EqualTo(ToastPosition.TopRight));

    ToastManager.DefaultPosition = ToastPosition.BottomRight;
  }

  [Test]
  [Category("HappyPath")]
  public void ToastManager_MaxVisible_CanBeSetAndRetrieved() {
    var original = ToastManager.MaxVisible;

    ToastManager.MaxVisible = 3;
    Assert.That(ToastManager.MaxVisible, Is.EqualTo(3));

    ToastManager.MaxVisible = original;
  }

  [Test]
  [Category("EdgeCase")]
  public void ToastManager_MaxVisible_MinimumIsOne() {
    var original = ToastManager.MaxVisible;

    ToastManager.MaxVisible = 0;
    Assert.That(ToastManager.MaxVisible, Is.EqualTo(1));

    ToastManager.MaxVisible = original;
  }

  [Test]
  [Category("HappyPath")]
  public void ToastOptions_Properties() {
    var options = new ToastOptions {
      Message = "Test",
      Title = "Title",
      Type = ToastType.Success,
      Duration = 5000,
      ShowCloseButton = false,
      Position = ToastPosition.TopLeft
    };

    Assert.That(options.Message, Is.EqualTo("Test"));
    Assert.That(options.Title, Is.EqualTo("Title"));
    Assert.That(options.Type, Is.EqualTo(ToastType.Success));
    Assert.That(options.Duration, Is.EqualTo(5000));
    Assert.That(options.ShowCloseButton, Is.False);
    Assert.That(options.Position, Is.EqualTo(ToastPosition.TopLeft));
  }
}
