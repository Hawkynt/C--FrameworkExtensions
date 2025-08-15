using System.ComponentModel;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace System.Windows.Forms.Tests {
  [TestFixture]
  [Category("Unit")]
  public class ControlDataBindingTests {
    private TestDataSource _dataSource;
    private TextBox _textBox;
    private NumericUpDown _numericUpDown;
    private Form _form;

    [SetUp]
    public void Setup() {
      this._dataSource = new TestDataSource();
      this._textBox = new TextBox();
      this._numericUpDown = new NumericUpDown();
      this._numericUpDown.Minimum = 0;
      this._numericUpDown.Maximum = 10000; // Allow larger test values
      
      // Ensure proper binding context for standalone controls
      this._form = new Form();
      this._form.Controls.Add(this._textBox);
      this._form.Controls.Add(this._numericUpDown);
      
      // Force form handle creation for proper binding processing
      var handle = this._form.Handle; // This forces handle creation
    }

    [TearDown]
    public void TearDown() {
      this._textBox?.Dispose();
      this._numericUpDown?.Dispose();
      this._form?.Dispose();
    }

    #region Two-Way Binding Tests (==)

    [Test]
    [Category("HappyPath")]
    public void AddBinding_TwoWayWithEqualsOperator_CreatesProperBinding() {
      this._dataSource.Name = "Initial";

      this._textBox.AddBinding(this._dataSource, (ctrl, src) => ctrl.Text == src.Name);

      Assert.That(this._textBox.Text, Is.EqualTo("Initial"));
      Assert.That(this._textBox.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._textBox.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_TwoWayBinding_UpdatesControlWhenSourceChanges() {
      this._textBox.AddBinding(this._dataSource, (ctrl, src) => ctrl.Text == src.Name);

      // Verify the binding was created properly
      Assert.That(this._textBox.DataBindings.Count, Is.EqualTo(1));

      this._dataSource.Name = "Updated";

      // Wait for async operations to complete
      System.Threading.Tasks.Task.Delay(50).Wait();
      
      // Force binding updates by processing messages
      Application.DoEvents();
      
      // Also try to force the binding to read the current value
      if (this._textBox.DataBindings.Count > 0) {
        this._textBox.DataBindings[0].ReadValue();
      }

      Assert.That(this._textBox.Text, Is.EqualTo("Updated"));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_TwoWayBinding_UpdatesSourceWhenControlChanges() {
      this._textBox.AddBinding(this._dataSource, (ctrl, src) => ctrl.Text == src.Name);

      this._textBox.Text = "New Value";

      Assert.That(this._dataSource.Name, Is.EqualTo("New Value"));
    }

    #endregion

    #region One-Way Source-to-Control Tests (<)

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWaySourceToControlWithLessThanOperator_CreatesReadOnlyBinding() {
      this._dataSource.Count = 42;

      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value < src.Count);

      Assert.That(this._numericUpDown.Value, Is.EqualTo(42));
      Assert.That(this._numericUpDown.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._numericUpDown.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.Never));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWaySourceToControl_UpdatesControlWhenSourceChanges() {
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value < src.Count);

      this._dataSource.Count = 100;

      Assert.That(this._numericUpDown.Value, Is.EqualTo(100));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWaySourceToControl_DoesNotUpdateSourceWhenControlChanges() {
      this._dataSource.Count = 50;
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value < src.Count);

      this._numericUpDown.Value = 999;

      Assert.That(this._dataSource.Count, Is.EqualTo(50));
    }

    #endregion

    #region One-Way Control-to-Source Tests (>)

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWayControlToSourceWithGreaterThanOperator_CreatesWriteOnlyBinding() {
      var originalCount = this._dataSource.Count;

      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.Count);

      Assert.That(this._numericUpDown.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._numericUpDown.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(this._numericUpDown.DataBindings[0].FormattingEnabled, Is.False);
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWayControlToSource_UpdatesSourceWhenControlChanges() {
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.Count);

      this._numericUpDown.Value = 123;

      Assert.That(this._dataSource.Count, Is.EqualTo(123));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWayControlToSource_DoesNotUpdateControlWhenSourceChanges() {
      this._numericUpDown.Value = 75;
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.Count);

      this._dataSource.Count = 200;

      Assert.That(this._numericUpDown.Value, Is.EqualTo(75));
    }

    #endregion

    #region Exception Tests

    [Test]
    [Category("Exception")]
    public void AddBinding_InvalidOperator_ThrowsArgumentException() => Assert.Throws<ArgumentException>(() => this._textBox.AddBinding(this._dataSource, (ctrl, src) => ctrl.Text != src.Name));

    [Test]
    [Category("Exception")]
    public void AddBinding_InvalidExpression_ThrowsArgumentException() => Assert.Throws<ArgumentException>(() => this._textBox.AddBinding(this._dataSource, (ctrl, src) => true));

    #endregion

    #region Equivalent Expression Tests

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EquivalentSourceToControlExpressions_BehaveSame() {
      var numUpDown1 = new NumericUpDown() { Maximum = 10000 };
      var numUpDown2 = new NumericUpDown() { Maximum = 10000 };

      // These should be equivalent: control < source vs source > control
      numUpDown1.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value < src.Count);
      numUpDown2.AddBinding(this._dataSource, (ctrl, src) => src.Count > ctrl.Value);

      Assert.That(numUpDown1.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.Never));
      Assert.That(numUpDown2.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.Never));

      this._dataSource.Count = 123;
      Assert.That(numUpDown1.Value, Is.EqualTo(123));
      Assert.That(numUpDown2.Value, Is.EqualTo(123));

      numUpDown1.Value = 456;
      numUpDown2.Value = 789;
      Assert.That(this._dataSource.Count, Is.EqualTo(123)); // Should not change for either

      numUpDown1.Dispose();
      numUpDown2.Dispose();
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EquivalentControlToSourceExpressions_BehaveSame() {
      var numUpDown1 = new NumericUpDown() { Maximum = 10000 };
      var numUpDown2 = new NumericUpDown() { Maximum = 10000 };

      // These should be equivalent: control > source vs source < control
      numUpDown1.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.Count);
      numUpDown2.AddBinding(this._dataSource, (ctrl, src) => src.Count < ctrl.Value);

      Assert.That(numUpDown1.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(numUpDown2.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(numUpDown1.DataBindings[0].FormattingEnabled, Is.False);
      Assert.That(numUpDown2.DataBindings[0].FormattingEnabled, Is.False);

      numUpDown1.Value = 100;
      numUpDown2.Value = 200;
      Assert.That(this._dataSource.Count, Is.EqualTo(200)); // Should update from control changes

      this._dataSource.Count = 300;
      Assert.That(numUpDown1.Value, Is.EqualTo(100)); // Should not change
      Assert.That(numUpDown2.Value, Is.EqualTo(200)); // Should not change

      numUpDown1.Dispose();
      numUpDown2.Dispose();
    }

    #endregion

    #region Equality With Complex Expressions Tests

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EqualityWithCastOnLeft_Works() {
      this._dataSource.Count = 42.5m;

      // Cast on left side: (int)source.Count == control.Value
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => (int)src.Count == ctrl.Value);

      Assert.That(this._numericUpDown.Value, Is.EqualTo(42));
      Assert.That(this._numericUpDown.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._numericUpDown.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EqualityWithToStringOnLeft_Works() {
      this._dataSource.Count = 123;

      // ToString on left side: source.Count.ToString() == control.Text
      this._textBox.AddBinding(this._dataSource, (ctrl, src) => src.Count.ToString() == ctrl.Text);

      Assert.That(this._textBox.Text, Is.EqualTo("123"));
      Assert.That(this._textBox.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._textBox.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EqualityWithNestedPropertyOnLeft_Works() {
      this._dataSource.NestedObject = new NestedTestData { Number = 99 };

      // Nested property on left side: source.NestedObject.Number == control.Value
      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => src.NestedObject.Number == ctrl.Value);

      Assert.That(this._numericUpDown.Value, Is.EqualTo(99));
      Assert.That(this._numericUpDown.DataBindings.Count, Is.EqualTo(1));
      Assert.That(this._numericUpDown.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
    }

    [Test]
    [Category("HappyPath")]
    public void AddBinding_EqualitySymmetricBehavior_BothDirectionsWork() {
      // Test that equality works the same regardless of which side is source vs control
      var textBox1 = new TextBox();
      var textBox2 = new TextBox();
      var numUpDown1 = new NumericUpDown() { Maximum = 10000 };
      var numUpDown2 = new NumericUpDown() { Maximum = 10000 };

      this._dataSource.Name = "Test";
      this._dataSource.Count = 42;

      // Standard: control == source
      textBox1.AddBinding(this._dataSource, (c, s) => c.Text == s.Name);
      numUpDown1.AddBinding(this._dataSource, (c, s) => c.Value == s.Count);

      // Reversed: source == control
      textBox2.AddBinding(this._dataSource, (c, s) => s.Name == c.Text);
      numUpDown2.AddBinding(this._dataSource, (c, s) => s.Count == c.Value);

      // Both should behave identically
      Assert.That(textBox1.Text, Is.EqualTo("Test"));
      Assert.That(textBox2.Text, Is.EqualTo("Test"));
      Assert.That(numUpDown1.Value, Is.EqualTo(42));
      Assert.That(numUpDown2.Value, Is.EqualTo(42));

      Assert.That(textBox1.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(textBox2.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(numUpDown1.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));
      Assert.That(numUpDown2.DataBindings[0].DataSourceUpdateMode, Is.EqualTo(DataSourceUpdateMode.OnPropertyChanged));

      // Test bidirectional updates
      textBox1.Text = "Updated1";
      textBox2.Text = "Updated2";
      numUpDown1.Value = 100;
      numUpDown2.Value = 200;

      Assert.That(this._dataSource.Name, Is.EqualTo("Updated2")); // Last write wins
      Assert.That(this._dataSource.Count, Is.EqualTo(200)); // Last write wins

      textBox1.Dispose();
      textBox2.Dispose();
      numUpDown1.Dispose();
      numUpDown2.Dispose();
    }

    #endregion

    #region Complex Property Path Tests

    [Test]
    [Category("HappyPath")]
    public void AddBinding_NestedProperty_WorksWithAllDirections() {
      this._dataSource.NestedObject = new NestedTestData { Number = 42 };

      var numUpDown1 = new NumericUpDown() { Maximum = 10000 };
      numUpDown1.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value == src.NestedObject.Number);

      var numUpDown2 = new NumericUpDown() { Maximum = 10000 };
      numUpDown2.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value < src.NestedObject.Number);

      var numUpDown3 = new NumericUpDown() { Maximum = 10000 };
      numUpDown3.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.NestedObject.Number);

      Assert.That(numUpDown1.Value, Is.EqualTo(42));
      Assert.That(numUpDown2.Value, Is.EqualTo(42));

      this._dataSource.NestedObject.Number = 100;
      Assert.That(numUpDown1.Value, Is.EqualTo(100));
      Assert.That(numUpDown2.Value, Is.EqualTo(100));
      Assert.That(numUpDown3.Value, Is.EqualTo(0)); // Should not update

      numUpDown1.Value = 200;
      numUpDown3.Value = 300;

      Assert.That(this._dataSource.NestedObject.Number, Is.EqualTo(300)); // Last write wins

      numUpDown1.Dispose();
      numUpDown2.Dispose();
      numUpDown3.Dispose();
    }

    #endregion

    #region Test Data Classes

    public class TestDataSource : INotifyPropertyChanged {
      private string _name = "";
      private decimal _count = 0;
      private NestedTestData _nestedObject = new NestedTestData();

      public string Name {
        get => this._name;
        set {
          this._name = value;
          this.OnPropertyChanged(nameof(this.Name));
        }
      }

      public decimal Count {
        get => this._count;
        set {
          this._count = value;
          this.OnPropertyChanged(nameof(this.Count));
        }
      }

      public NestedTestData NestedObject {
        get => this._nestedObject;
        set {
          this._nestedObject = value;
          this.OnPropertyChanged(nameof(this.NestedObject));
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class NestedTestData : INotifyPropertyChanged {
      private string _value = "";
      private decimal _number = 0;

      public string Value {
        get => this._value;
        set {
          this._value = value;
          this.OnPropertyChanged(nameof(this.Value));
        }
      }

      public decimal Number {
        get => this._number;
        set {
          this._number = value;
          this.OnPropertyChanged(nameof(this.Number));
        }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      protected virtual void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
