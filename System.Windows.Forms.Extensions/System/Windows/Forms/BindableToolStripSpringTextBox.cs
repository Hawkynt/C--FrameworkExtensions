using System.ComponentModel;

namespace System.Windows.Forms;

#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
#endif
  class BindableToolStripSpringTextBox : ToolStripSpringTextBox, IBindableComponent {
  #region IBindableComponent Members

  private BindingContext _bindingContext;
  private ControlBindingsCollection _dataBindings;

  [Browsable(false)]
  public BindingContext BindingContext {
    get => this._bindingContext ??= new();
    set => this._bindingContext = value;
  }

  [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
  [Category("Data")]
  [Description("Bindings")]
  [RefreshProperties(RefreshProperties.All)]
  [ParenthesizePropertyName(true)]
  public ControlBindingsCollection DataBindings => this._dataBindings ??= new(this);

  #endregion
}
