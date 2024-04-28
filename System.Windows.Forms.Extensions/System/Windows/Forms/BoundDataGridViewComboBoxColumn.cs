using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

public class BoundDataGridViewComboBoxColumn : DataGridViewColumn {
  public string DataSourcePropertyName { get; }
  public string EnabledWhenPropertyName { get; }
  public string ValueMember { get; }
  public string DisplayMember { get; }

  public BoundDataGridViewComboBoxColumn(string dataSourcePropertyName, string enabledWhenPropertyName,
    string valueMember, string displayMember) {
    var cell = new BoundDataGridViewComboBoxCell(dataSourcePropertyName, enabledWhenPropertyName, valueMember,
      displayMember);

    this.DataSourcePropertyName = dataSourcePropertyName;
    this.EnabledWhenPropertyName = enabledWhenPropertyName;
    this.ValueMember = valueMember;
    this.DisplayMember = displayMember;

    // ReSharper disable once VirtualMemberCallInConstructor
    this.CellTemplate = cell;
  }

  #region Overrides of DataGridViewColumn

  public override object Clone() {
    var result = new BoundDataGridViewComboBoxColumn(this.DataSourcePropertyName, this.EnabledWhenPropertyName,
      this.ValueMember, this.DisplayMember) {
      Name = this.Name,
      DisplayIndex = this.DisplayIndex,
      HeaderText = this.HeaderText,
      DataPropertyName = this.DataPropertyName,
      AutoSizeMode = this.AutoSizeMode,
      SortMode = this.SortMode,
      FillWeight = this.FillWeight
    };
    return result;
  }

  #endregion

  #region Overrides of DataGridViewBand

  public override DataGridViewCell CellTemplate {
    get => base.CellTemplate;
    set {
      // Ensure that the cell used for the template is a BoundDataGridViewComboBoxCell.
      if (value != null && !value.GetType().IsAssignableFrom(typeof(BoundDataGridViewComboBoxCell)))
        throw new InvalidCastException(nameof(BoundDataGridViewComboBoxCell));

      base.CellTemplate = value;
    }
  }

  #endregion

  internal class DataGridViewComboBoxEditingControl : ComboBox, IDataGridViewEditingControl {
    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle) { }
    public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey) => false;
    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => this.SelectedValue;
    public void PrepareEditingControlForEdit(bool selectAll) { }

    public DataGridView EditingControlDataGridView { get; set; }

    public object EditingControlFormattedValue {
      get => this.GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
      set => this.SelectedValue = value;
    }

    public int EditingControlRowIndex { get; set; }

    public bool EditingControlValueChanged { get; set; }

    public Cursor EditingPanelCursor => Cursors.Default;

    public bool RepositionEditingControlOnValueChange => false;

    protected override void OnSelectedValueChanged(EventArgs e) {
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);

      base.OnSelectedValueChanged(e);
    }
  }

  internal class BoundDataGridViewComboBoxCell : DataGridViewTextBoxCell {
    public string DataSourcePropertyName { get; set; }
    public string EnabledWhenPropertyName { get; set; }
    public string ValueMember { get; set; }
    public string DisplayMember { get; set; }

    public BoundDataGridViewComboBoxCell() { }

    public BoundDataGridViewComboBoxCell(string dataSourcePropertyName, string enabledWhenPropertyName,
      string valueMember, string displayMember) {
      this.DataSourcePropertyName = dataSourcePropertyName;
      this.EnabledWhenPropertyName = enabledWhenPropertyName;
      this.ValueMember = valueMember;
      this.DisplayMember = displayMember;
    }

    public override Type EditType => typeof(DataGridViewComboBoxEditingControl);

    public override Type ValueType { get; set; }

    public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle,
      TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter) =>
      Convert.ChangeType(formattedValue, this.ValueType);

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue,
      DataGridViewCellStyle dataGridViewCellStyle) {
      base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
      if (this.DataGridView.EditingControl is not ComboBox comboBox)
        return;

      var owningRowDataBoundItem = this.OwningRow.DataBoundItem;
      var source = DataGridViewExtensions.GetPropertyValueOrDefault<IEnumerable>(owningRowDataBoundItem,
        this.DataSourcePropertyName, null, null, null, null);
      comboBox.DataSource = source;


      if (this.EnabledWhenPropertyName != null)
        comboBox.Enabled = DataGridViewExtensions.GetPropertyValueOrDefault(owningRowDataBoundItem,
          this.EnabledWhenPropertyName, true, true, true, true);

      if (source == null)
        return;

      if (this.DisplayMember != null)
        comboBox.DisplayMember = this.DisplayMember;

      if (this.ValueMember != null)
        comboBox.ValueMember = this.ValueMember;


      comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBox.SelectedIndex = 0;

      this.ValueType = source.GetType().GetElementType();
    }

    public override object Clone() {
      var cell = (BoundDataGridViewComboBoxCell)base.Clone();
      cell.DataSourcePropertyName = this.DataSourcePropertyName;
      cell.EnabledWhenPropertyName = this.EnabledWhenPropertyName;
      cell.ValueMember = this.ValueMember;
      cell.DisplayMember = this.DisplayMember;
      return cell;
    }
  }
}
