using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class AccordionPanelTests {
  private AccordionPanel _accordion;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._accordion = new AccordionPanel();
    this._form = new Form();
    this._form.Controls.Add(this._accordion);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._accordion?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._accordion.Sections, Is.Empty);
    Assert.That(this._accordion.AllowMultipleExpanded, Is.False);
    Assert.That(this._accordion.AnimateExpansion, Is.True);
    Assert.That(this._accordion.SectionHeight, Is.EqualTo(150));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSection_CreatesSection() {
    var section = this._accordion.AddSection("Test Section");

    Assert.That(section, Is.Not.Null);
    Assert.That(section.HeaderText, Is.EqualTo("Test Section"));
    Assert.That(this._accordion.Sections.Length, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AddSection_MultipleSections() {
    this._accordion.AddSection("Section 1");
    this._accordion.AddSection("Section 2");
    this._accordion.AddSection("Section 3");

    Assert.That(this._accordion.Sections.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveSection_RemovesSection() {
    var section = this._accordion.AddSection("Test Section");
    this._accordion.RemoveSection(section);

    Assert.That(this._accordion.Sections.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ClearSections_RemovesAllSections() {
    this._accordion.AddSection("Section 1");
    this._accordion.AddSection("Section 2");
    this._accordion.ClearSections();

    Assert.That(this._accordion.Sections.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowMultipleExpanded_CanBeSetAndRetrieved() {
    this._accordion.AllowMultipleExpanded = true;
    Assert.That(this._accordion.AllowMultipleExpanded, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void AnimateExpansion_CanBeSetAndRetrieved() {
    this._accordion.AnimateExpansion = false;
    Assert.That(this._accordion.AnimateExpansion, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SectionHeight_CanBeSetAndRetrieved() {
    this._accordion.SectionHeight = 200;
    Assert.That(this._accordion.SectionHeight, Is.EqualTo(200));
  }

  [Test]
  [Category("EdgeCase")]
  public void SectionHeight_MinimumIsFifty() {
    this._accordion.SectionHeight = 20;
    Assert.That(this._accordion.SectionHeight, Is.EqualTo(50));
  }

  [Test]
  [Category("HappyPath")]
  public void Section_IsExpanded_CanBeSetAndRetrieved() {
    this._accordion.AnimateExpansion = false;
    var section = this._accordion.AddSection("Test");

    section.IsExpanded = true;
    Assert.That(section.IsExpanded, Is.True);

    section.IsExpanded = false;
    Assert.That(section.IsExpanded, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void Section_HeaderText_CanBeSetAndRetrieved() {
    var section = this._accordion.AddSection("Initial");
    section.HeaderText = "Updated";
    Assert.That(section.HeaderText, Is.EqualTo("Updated"));
  }

  [Test]
  [Category("HappyPath")]
  public void Section_ContentPanel_IsAccessible() {
    var section = this._accordion.AddSection("Test");
    var label = new Label { Text = "Content" };
    section.ContentPanel.Controls.Add(label);

    Assert.That(section.ContentPanel.Controls.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SectionExpanded_IsRaised() {
    this._accordion.AnimateExpansion = false;
    var section = this._accordion.AddSection("Test");
    section.IsExpanded = false;

    AccordionSectionEventArgs raisedArgs = null;
    this._accordion.SectionExpanded += (s, e) => raisedArgs = e;

    section.IsExpanded = true;

    Assert.That(raisedArgs, Is.Not.Null);
    Assert.That(raisedArgs.Section, Is.EqualTo(section));
    Assert.That(raisedArgs.Index, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SectionCollapsed_IsRaised() {
    this._accordion.AnimateExpansion = false;
    var section = this._accordion.AddSection("Test");
    section.IsExpanded = true;

    AccordionSectionEventArgs raisedArgs = null;
    this._accordion.SectionCollapsed += (s, e) => raisedArgs = e;

    section.IsExpanded = false;

    Assert.That(raisedArgs, Is.Not.Null);
    Assert.That(raisedArgs.Section, Is.EqualTo(section));
  }

  [Test]
  [Category("HappyPath")]
  public void ExpandAll_ExpandsAllSections() {
    this._accordion.AnimateExpansion = false;
    this._accordion.AllowMultipleExpanded = true;
    var section1 = this._accordion.AddSection("Section 1");
    var section2 = this._accordion.AddSection("Section 2");

    this._accordion.ExpandAll();

    Assert.That(section1.IsExpanded, Is.True);
    Assert.That(section2.IsExpanded, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void CollapseAll_CollapsesAllSections() {
    this._accordion.AnimateExpansion = false;
    this._accordion.AllowMultipleExpanded = true;
    var section1 = this._accordion.AddSection("Section 1");
    var section2 = this._accordion.AddSection("Section 2");
    section1.IsExpanded = true;
    section2.IsExpanded = true;

    this._accordion.CollapseAll();

    Assert.That(section1.IsExpanded, Is.False);
    Assert.That(section2.IsExpanded, Is.False);
  }
}
