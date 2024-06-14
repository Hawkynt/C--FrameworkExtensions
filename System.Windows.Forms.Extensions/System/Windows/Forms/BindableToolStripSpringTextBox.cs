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

namespace System.Windows.Forms;

public class BindableToolStripSpringTextBox : ToolStripSpringTextBox, IBindableComponent {
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
