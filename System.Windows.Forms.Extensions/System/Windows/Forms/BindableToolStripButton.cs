#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.ComponentModel;
using System.Windows.Forms.Design;

namespace System.Windows.Forms;

// see https://stackoverflow.com/questions/2002170/c-sharp-how-to-implement-databinding-without-control
[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
public class BindableToolStripButton : ToolStripButton, IBindableComponent {
  #region IBindableComponent Members

  private BindingContext _bindingContext;
  private ControlBindingsCollection _dataBindings;

  [Browsable(false)]
  public BindingContext BindingContext {
    get => this._bindingContext ??= [];
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
