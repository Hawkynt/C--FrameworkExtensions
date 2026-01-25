using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class WizardControlTests {
  private WizardControl _wizard;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._wizard = new WizardControl();
    this._form = new Form();
    this._form.Controls.Add(this._wizard);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._wizard?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._wizard.Pages, Is.Empty);
    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
    Assert.That(this._wizard.CurrentPage, Is.Null);
    Assert.That(this._wizard.ShowStepIndicator, Is.True);
    Assert.That(this._wizard.ShowNavigationButtons, Is.True);
    Assert.That(this._wizard.NextButtonText, Is.EqualTo("Next >"));
    Assert.That(this._wizard.BackButtonText, Is.EqualTo("< Back"));
    Assert.That(this._wizard.FinishButtonText, Is.EqualTo("Finish"));
  }

  [Test]
  [Category("HappyPath")]
  public void AddPage_CreatesPage() {
    var page = this._wizard.AddPage("Test Page", "Description");

    Assert.That(page, Is.Not.Null);
    Assert.That(page.Title, Is.EqualTo("Test Page"));
    Assert.That(page.Description, Is.EqualTo("Description"));
    Assert.That(this._wizard.Pages.Length, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AddPage_MultiplePages() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");
    this._wizard.AddPage("Page 3");

    Assert.That(this._wizard.Pages.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CurrentPage_ReturnsFirstPageAfterAdding() {
    var page = this._wizard.AddPage("Test Page");

    Assert.That(this._wizard.CurrentPage, Is.EqualTo(page));
    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void RemovePage_RemovesPage() {
    var page = this._wizard.AddPage("Test Page");
    this._wizard.RemovePage(page);

    Assert.That(this._wizard.Pages.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void Next_MovesToNextPage() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");

    this._wizard.Next();

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("EdgeCase")]
  public void Next_DoesNotExceedLastPage() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");
    this._wizard.GoToPage(1);

    this._wizard.Next();

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Back_MovesToPreviousPage() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");
    this._wizard.GoToPage(1);

    this._wizard.Back();

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void Back_DoesNotGoBelowZero() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");

    this._wizard.Back();

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GoToPage_NavigatesToSpecificPage() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");
    this._wizard.AddPage("Page 3");

    this._wizard.GoToPage(2);

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(2));
  }

  [Test]
  [Category("EdgeCase")]
  public void GoToPage_InvalidIndex_DoesNothing() {
    this._wizard.AddPage("Page 1");

    this._wizard.GoToPage(5);

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PageChanging_IsRaised() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");

    WizardPageChangingEventArgs raisedArgs = null;
    this._wizard.PageChanging += (s, e) => raisedArgs = e;

    this._wizard.Next();

    Assert.That(raisedArgs, Is.Not.Null);
    Assert.That(raisedArgs.CurrentIndex, Is.EqualTo(0));
    Assert.That(raisedArgs.NewIndex, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void PageChanging_CanBeCancelled() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");

    this._wizard.PageChanging += (s, e) => e.Cancel = true;

    this._wizard.Next();

    Assert.That(this._wizard.CurrentPageIndex, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void PageChanged_IsRaised() {
    this._wizard.AddPage("Page 1");
    this._wizard.AddPage("Page 2");

    WizardPageEventArgs raisedArgs = null;
    this._wizard.PageChanged += (s, e) => raisedArgs = e;

    this._wizard.Next();

    Assert.That(raisedArgs, Is.Not.Null);
    Assert.That(raisedArgs.Index, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void ShowStepIndicator_CanBeSetAndRetrieved() {
    this._wizard.ShowStepIndicator = false;
    Assert.That(this._wizard.ShowStepIndicator, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ShowNavigationButtons_CanBeSetAndRetrieved() {
    this._wizard.ShowNavigationButtons = false;
    Assert.That(this._wizard.ShowNavigationButtons, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NextButtonText_CanBeSetAndRetrieved() {
    this._wizard.NextButtonText = "Continue";
    Assert.That(this._wizard.NextButtonText, Is.EqualTo("Continue"));
  }

  [Test]
  [Category("EdgeCase")]
  public void NextButtonText_NullValue_BecomesDefault() {
    this._wizard.NextButtonText = null;
    Assert.That(this._wizard.NextButtonText, Is.EqualTo("Next >"));
  }

  [Test]
  [Category("HappyPath")]
  public void BackButtonText_CanBeSetAndRetrieved() {
    this._wizard.BackButtonText = "Previous";
    Assert.That(this._wizard.BackButtonText, Is.EqualTo("Previous"));
  }

  [Test]
  [Category("EdgeCase")]
  public void BackButtonText_NullValue_BecomesDefault() {
    this._wizard.BackButtonText = null;
    Assert.That(this._wizard.BackButtonText, Is.EqualTo("< Back"));
  }

  [Test]
  [Category("HappyPath")]
  public void FinishButtonText_CanBeSetAndRetrieved() {
    this._wizard.FinishButtonText = "Done";
    Assert.That(this._wizard.FinishButtonText, Is.EqualTo("Done"));
  }

  [Test]
  [Category("EdgeCase")]
  public void FinishButtonText_NullValue_BecomesDefault() {
    this._wizard.FinishButtonText = null;
    Assert.That(this._wizard.FinishButtonText, Is.EqualTo("Finish"));
  }

  [Test]
  [Category("HappyPath")]
  public void Page_CanMoveNext_DefaultIsTrue() {
    var page = this._wizard.AddPage("Test");
    Assert.That(page.CanMoveNext, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Page_CanMoveBack_DefaultIsTrue() {
    var page = this._wizard.AddPage("Test");
    Assert.That(page.CanMoveBack, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void Page_ContentPanel_IsAccessible() {
    var page = this._wizard.AddPage("Test");
    var label = new Label { Text = "Content" };
    page.ContentPanel.Controls.Add(label);

    Assert.That(page.ContentPanel.Controls.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void Page_Tag_CanBeSetAndRetrieved() {
    var page = this._wizard.AddPage("Test");
    var tag = new object();
    page.Tag = tag;

    Assert.That(page.Tag, Is.EqualTo(tag));
  }
}
