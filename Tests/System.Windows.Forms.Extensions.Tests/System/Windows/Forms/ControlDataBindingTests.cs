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
      this._numericUpDown.Minimum = -10000;
      this._numericUpDown.Maximum = 10000; // Allow larger test values
      
      // Ensure proper binding context for standalone controls
      this._form = new Form();
      this._form.Controls.Add(this._textBox);
      this._form.Controls.Add(this._numericUpDown);

      // This forces handle creation for proper binding processing
      var handle = this._form.Handle; 
      handle=this._textBox.Handle;
      handle=this._numericUpDown.Handle;
    }

    private void _UpdateUI() {
      // Wait for async operations to complete
      System.Threading.Tasks.Task.Delay(50).Wait();

      // Force binding updates by processing messages
      Application.DoEvents();
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

    #endregion

    #region One-Way Control-to-Source Tests (>)

    [Test]
    [Category("HappyPath")]
    public void AddBinding_OneWayControlToSourceWithGreaterThanOperator_CreatesWriteOnlyBinding() {
      var originalCount = this._dataSource.Count;

      this._numericUpDown.AddBinding(this._dataSource, (ctrl, src) => ctrl.Value > src.Count);

      // For control-to-source only bindings, no WinForms binding is created - everything is handled manually
      Assert.That(this._numericUpDown.DataBindings.Count, Is.EqualTo(0));
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

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWork() {
      var ctrl = this._textBox;

      var data = new TestDataSource { Name = "Start" };
      ctrl.AddBinding(data, (c, d) => c.Text == d.Name);
      this._UpdateUI();
      Assert.That(data.Name, Is.EqualTo("Start"));
      Assert.That(ctrl.Text, Is.EqualTo("Start"));

      data.Name = "SourceChanged";
      this._UpdateUI();
      Assert.That(data.Name, Is.EqualTo("SourceChanged"));
      Assert.That(ctrl.Text, Is.EqualTo("SourceChanged"));

      ctrl.Text= "ControlChanged";
      this._UpdateUI();
      Assert.That(data.Name, Is.EqualTo("ControlChanged"));
      Assert.That(ctrl.Text, Is.EqualTo("ControlChanged"));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWork() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 42 };
      ctrl.AddBinding(data, (c, d) => c.Value < d.Count);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(42));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.Count= 1337;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWorkReversed() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 42 };
      ctrl.AddBinding(data, (c, d) => d.Count > c.Value);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(42));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.Count = 1337;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWork() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { Count = 42 };
      ctrl.AddBinding(data, (c, d) => c.Value > d.Count);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.Count = 1337;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWorkReversed() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { Count = 42 };
      ctrl.AddBinding(data, (c, d) => d.Count < c.Value);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.Count = 1337;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    #region Nested Property Integration Tests

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWorkWithNestedProperties() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 123 } };
      ctrl.AddBinding(data, (c, d) => c.Value == d.NestedObject.Number);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(123));
      Assert.That(ctrl.Value, Is.EqualTo(123));

      data.NestedObject.Number = 456;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(456));
      Assert.That(ctrl.Value, Is.EqualTo(456));

      ctrl.Value = 789;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(789));
      Assert.That(ctrl.Value, Is.EqualTo(789));
    }

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWorkWithNestedPropertiesReversed() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 123 } };
      ctrl.AddBinding(data, (c, d) => d.NestedObject.Number == c.Value);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(123));
      Assert.That(ctrl.Value, Is.EqualTo(123));

      data.NestedObject.Number = 456;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(456));
      Assert.That(ctrl.Value, Is.EqualTo(456));

      ctrl.Value = 789;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(789));
      Assert.That(ctrl.Value, Is.EqualTo(789));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWorkWithNestedProperties() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 42 } };
      ctrl.AddBinding(data, (c, d) => c.Value < d.NestedObject.Number);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(42));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.NestedObject.Number = 1337;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWorkWithNestedPropertiesReversed() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 42 } };
      ctrl.AddBinding(data, (c, d) => d.NestedObject.Number > c.Value);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(42));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.NestedObject.Number = 1337;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWorkWithNestedProperties() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 42 } };
      ctrl.AddBinding(data, (c, d) => c.Value > d.NestedObject.Number);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.NestedObject.Number = 1337;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }
    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWorkWithNestedPropertiesReversed() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 42 } };
      ctrl.AddBinding(data, (c, d) => d.NestedObject.Number < c.Value);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.NestedObject.Number = 1337;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(1337));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWorkWhenNestedObjectIsReplaced() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { NestedObject = new NestedTestData { Number = 123 } };
      ctrl.AddBinding(data, (c, d) => c.Value == d.NestedObject.Number);
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(123));
      Assert.That(ctrl.Value, Is.EqualTo(123));

      // Replace the entire nested object - this should trigger NestedObject PropertyChanged
      data.NestedObject = new NestedTestData { Number = 999 };
      this._UpdateUI();
      
      // Question: Should the binding still work after nested object replacement?
      // Current expectation: binding should pick up the new nested object's value
      Assert.That(data.NestedObject.Number, Is.EqualTo(999));
      Assert.That(ctrl.Value, Is.EqualTo(999), "Control should reflect new nested object's value");

      // Test that changes to the NEW nested object still propagate
      data.NestedObject.Number = 777;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(777));
      Assert.That(ctrl.Value, Is.EqualTo(777), "Control should reflect changes to new nested object");

      // Test that control-to-source still works with new nested object
      ctrl.Value = 555;
      this._UpdateUI();
      Assert.That(data.NestedObject.Number, Is.EqualTo(555));
      Assert.That(ctrl.Value, Is.EqualTo(555));
    }

    #endregion

    #region Casted Property Integration Tests

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWorkWithCastedProperties() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 123.7m };
      ctrl.AddBinding(data, (c, d) => c.Value == (int)d.Count);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(123));
      Assert.That(ctrl.Value, Is.EqualTo(123));

      data.Count = 456.9m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(456));
      Assert.That(ctrl.Value, Is.EqualTo(456));

      ctrl.Value = 789;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(789));
      Assert.That(ctrl.Value, Is.EqualTo(789));
    }

    [Test]
    [Category("Integration")]
    public void TwoWayBindingsShouldWorkWithCastedPropertiesReversed() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 123.7m };
      ctrl.AddBinding(data, (c, d) => (int)d.Count == c.Value);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(123));
      Assert.That(ctrl.Value, Is.EqualTo(123));

      data.Count = 456.9m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(456));
      Assert.That(ctrl.Value, Is.EqualTo(456));

      ctrl.Value = 789;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(789));
      Assert.That(ctrl.Value, Is.EqualTo(789));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWorkWithCastedProperties() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 42.8m };
      ctrl.AddBinding(data, (c, d) => c.Value < (int)d.Count);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(42.8m));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.Count = 1337.3m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.3m));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.3m));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }


    [Test]
    [Category("Integration")]
    public void OneWayToTargetBindingsShouldWorkWithCastedPropertiesReversed() {
      var ctrl = this._numericUpDown;

      var data = new TestDataSource { Count = 42.8m };
      ctrl.AddBinding(data, (c, d) => (int)d.Count > c.Value);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(42.8m));
      Assert.That(ctrl.Value, Is.EqualTo(42));

      data.Count = 1337.3m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.3m));
      Assert.That(ctrl.Value, Is.EqualTo(1337));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.3m));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWorkWithCastedProperties() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { Count = 42.7m };
      ctrl.AddBinding(data, (c, d) => c.Value > (int)d.Count);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.Count = 1337.9m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.9m));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
    }

    [Test]
    [Category("Integration")]
    public void OneWayToSourceBindingsShouldWorkWithCastedPropertiesReversed() {
      var ctrl = this._numericUpDown;
      ctrl.Value = 0;

      var data = new TestDataSource { Count = 42.7m };
      ctrl.AddBinding(data, (c, d) => (int)d.Count < c.Value);
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(0));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      data.Count = 1337.9m;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(1337.9m));
      Assert.That(ctrl.Value, Is.EqualTo(0));

      ctrl.Value = -815;
      this._UpdateUI();
      Assert.That(data.Count, Is.EqualTo(-815));
      Assert.That(ctrl.Value, Is.EqualTo(-815));
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
