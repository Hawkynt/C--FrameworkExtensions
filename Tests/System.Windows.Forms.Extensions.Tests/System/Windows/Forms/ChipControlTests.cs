using System.Drawing;
using NUnit.Framework;

namespace System.Windows.Forms.Tests;

[TestFixture]
[Category("Unit")]
public class ChipControlTests {
  private ChipControl _chipControl;
  private Form _form;

  [SetUp]
  public void Setup() {
    this._chipControl = new ChipControl();
    this._form = new Form();
    this._form.Controls.Add(this._chipControl);
    var _ = this._form.Handle;
  }

  [TearDown]
  public void TearDown() {
    this._chipControl?.Dispose();
    this._form?.Dispose();
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultValues_AreCorrect() {
    Assert.That(this._chipControl.Chips, Is.Empty);
    Assert.That(this._chipControl.AllowAdd, Is.True);
    Assert.That(this._chipControl.AllowRemove, Is.True);
    Assert.That(this._chipControl.AllowSelection, Is.False);
    Assert.That(this._chipControl.SelectionMode, Is.EqualTo(SelectionMode.None));
    Assert.That(this._chipControl.ChipSpacing, Is.EqualTo(4));
  }

  [Test]
  [Category("HappyPath")]
  public void AddChip_CreatesChip() {
    var chip = this._chipControl.AddChip("Test");

    Assert.That(chip, Is.Not.Null);
    Assert.That(chip.Text, Is.EqualTo("Test"));
    Assert.That(this._chipControl.Chips.Length, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void AddChip_WithColor() {
    var chip = this._chipControl.AddChip("Test", Color.Blue);

    Assert.That(chip.BackColor, Is.EqualTo(Color.Blue));
  }

  [Test]
  [Category("HappyPath")]
  public void AddChip_MultipleChips() {
    this._chipControl.AddChip("Chip 1");
    this._chipControl.AddChip("Chip 2");
    this._chipControl.AddChip("Chip 3");

    Assert.That(this._chipControl.Chips.Length, Is.EqualTo(3));
  }

  [Test]
  [Category("EdgeCase")]
  public void AddChip_WhenNotAllowed_ReturnsNull() {
    this._chipControl.AllowAdd = false;
    var chip = this._chipControl.AddChip("Test");

    Assert.That(chip, Is.Null);
    Assert.That(this._chipControl.Chips.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void RemoveChip_RemovesChip() {
    var chip = this._chipControl.AddChip("Test");
    this._chipControl.RemoveChip(chip);

    Assert.That(this._chipControl.Chips.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ClearChips_RemovesAllChips() {
    this._chipControl.AddChip("Chip 1");
    this._chipControl.AddChip("Chip 2");
    this._chipControl.ClearChips();

    Assert.That(this._chipControl.Chips.Length, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ChipAdded_IsRaised() {
    Chip addedChip = null;
    this._chipControl.ChipAdded += (s, e) => addedChip = e.Chip;

    var chip = this._chipControl.AddChip("Test");

    Assert.That(addedChip, Is.EqualTo(chip));
  }

  [Test]
  [Category("HappyPath")]
  public void ChipRemoved_IsRaised() {
    Chip removedChip = null;
    this._chipControl.ChipRemoved += (s, e) => removedChip = e.Chip;

    var chip = this._chipControl.AddChip("Test");
    this._chipControl.RemoveChip(chip);

    Assert.That(removedChip, Is.EqualTo(chip));
  }

  [Test]
  [Category("HappyPath")]
  public void AllowAdd_CanBeSetAndRetrieved() {
    this._chipControl.AllowAdd = false;
    Assert.That(this._chipControl.AllowAdd, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowRemove_CanBeSetAndRetrieved() {
    this._chipControl.AllowRemove = false;
    Assert.That(this._chipControl.AllowRemove, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void AllowSelection_CanBeSetAndRetrieved() {
    this._chipControl.AllowSelection = true;
    Assert.That(this._chipControl.AllowSelection, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SelectionMode_CanBeSetAndRetrieved() {
    this._chipControl.SelectionMode = SelectionMode.One;
    Assert.That(this._chipControl.SelectionMode, Is.EqualTo(SelectionMode.One));
  }

  [Test]
  [Category("HappyPath")]
  public void ChipSpacing_CanBeSetAndRetrieved() {
    this._chipControl.ChipSpacing = 8;
    Assert.That(this._chipControl.ChipSpacing, Is.EqualTo(8));
  }

  [Test]
  [Category("EdgeCase")]
  public void ChipSpacing_NegativeValue_BecomesZero() {
    this._chipControl.ChipSpacing = -5;
    Assert.That(this._chipControl.ChipSpacing, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void GetSelectedChips_ReturnsSelectedChips() {
    this._chipControl.AllowSelection = true;
    this._chipControl.SelectionMode = SelectionMode.MultiSimple;

    var chip1 = this._chipControl.AddChip("Chip 1");
    var chip2 = this._chipControl.AddChip("Chip 2");
    this._chipControl.AddChip("Chip 3");

    chip1.IsSelected = true;
    chip2.IsSelected = true;

    var selected = this._chipControl.GetSelectedChips();
    Assert.That(selected.Length, Is.EqualTo(2));
  }

  [Test]
  [Category("HappyPath")]
  public void Chip_Properties() {
    var chip = new Chip("Test", Color.Red) {
      ForeColor = Color.White,
      CanRemove = false,
      Tag = "tag"
    };

    Assert.That(chip.Text, Is.EqualTo("Test"));
    Assert.That(chip.BackColor, Is.EqualTo(Color.Red));
    Assert.That(chip.ForeColor, Is.EqualTo(Color.White));
    Assert.That(chip.CanRemove, Is.False);
    Assert.That(chip.Tag, Is.EqualTo("tag"));
    Assert.That(chip.IsSelected, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void DefaultSize_IsReasonable() {
    Assert.That(this._chipControl.Width, Is.EqualTo(300));
    Assert.That(this._chipControl.Height, Is.EqualTo(60));
  }
}
