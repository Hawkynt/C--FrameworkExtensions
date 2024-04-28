namespace System.Windows.Forms;

public partial class BoundDataGridViewComboBoxColumn : DataGridViewColumn {
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
  
}
