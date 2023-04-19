using System.ComponentModel;
using System.Windows.Forms.Design;

namespace System.Windows.Forms;

// see https://stackoverflow.com/questions/2002170/c-sharp-how-to-implement-databinding-without-control
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
#if COMPILE_TO_EXTENSION_DLL
  public
#else
internal
#endif
class BindableToolStripButton : ToolStripButton, IBindableComponent {

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
