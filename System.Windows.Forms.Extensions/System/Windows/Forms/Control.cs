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

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Guard;

namespace System.Windows.Forms;

public static partial class ControlExtensions {
  /// <summary>
  ///   Stops the layout and returns a token which will continue layoutint on disposal.
  /// </summary>
  /// <param name="this">The this.</param>
  /// <returns></returns>
  public static ISuspendedLayoutToken PauseLayout(this Control @this) {
    Against.ThisIsNull(@this);

    return new SuspendedLayoutToken(@this);
  }

  public static ISuspendedRedrawToken PauseRedraw(this Control @this) {
    Against.ThisIsNull(@this);

    return new SuspendedRedrawToken(@this);
  }

  /// <summary>
  ///   Executes the given action with the current control after a period of time.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="dueTime">The time to wait until execution.</param>
  /// <param name="action">The action.</param>
  public static void SetTimeout<TControl>(this TControl @this, TimeSpan dueTime, Action<TControl> action) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(action);

    Async(
      @this,
      t => {
        Thread.Sleep(dueTime);
        SafelyInvoke(t, action);
      }
    );
  }

  /// <summary>
  ///   Gets the position of a given control relative to its form.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <returns>The position of it relative to it's form.</returns>
  public static Point GetLocationOnForm(this Control @this) {
    Against.ThisIsNull(@this);

    var result = @this.Location;
    var c = @this;
    for (; c is not Form; c = c.Parent)
      result.Offset(c.Location);

    return result;
  }

  /// <summary>
  ///   Gets the position of a given control relative to the screen.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <returns>The position of it relative to it's screen.</returns>
  public static Point GetLocationOnScreen(this Control @this) {
    Against.ThisIsNull(@this);

    return @this.PointToScreen(Point.Empty);
  }

  public static Point GetLocationOnClient(this Control @this) {
    Against.ThisIsNull(@this);

    var result = Point.Empty;
    var c = @this;
    for (; c.Parent != null; c = c.Parent)
      result.Offset(c.Location);

    return result;
  }

  /// <summary>
  ///   Safely invokes the given code on the control's thread.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="task">The task to perform in its thread.</param>
  /// <param name="async">if set to <c>true</c> calls asynchronous.</param>
  /// <returns>
  ///   <c>true</c> when no thread switch was needed; otherwise, <c>false</c>.
  /// </returns>
  /// <exception cref="System.ObjectDisposedException">Control already disposed.</exception>
  public static bool SafelyInvoke(this Control @this, Action task, bool async = true) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(task);

    return SafelyInvoke(@this, new __ActionWithDummyParameterWrapper { method = task }.Invoke, async);
  }


  /// <summary>
  ///   Safelies executes an action and returns the result..
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="function">The function.</param>
  /// <returns>Whatever the method returned.</returns>
  public static TResult SafelyInvoke<TResult>(this Control @this, Func<TResult> function) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(function);

    return SafelyInvoke(@this, new __FunctionWithDummyParameterWrapper<TResult> { function = function }.Invoke);
  }

  /// <summary>
  ///   Safelies executes an action and returns the result..
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="function">The function.</param>
  /// <returns>
  ///   Whatever the method returned.
  /// </returns>
  public static TResult SafelyInvoke<TControl, TResult>(this TControl @this, Func<TControl, TResult> function) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(function);

    if (@this.IsDisposed)
      throw new ObjectDisposedException(nameof(@this));

    if (@this.IsHandleCreated)
      return @this.InvokeRequired ? (TResult)@this.Invoke(function, @this) : function(@this);

    var context = SynchronizationContext.Current;
    if (context != null) {
      var wrapper = new __ReturnValueWithDummyParameterWrapper<TControl, TResult> { control = @this, function = function };
      context.Send(wrapper.Invoke, null);
      return wrapper.result;
    }

    if (Application.MessageLoop)
      return function(@this);

    throw new InvalidOperationException("Handle not yet created");
  }

  /// <summary>
  ///   Safely invokes the given code on the control's thread.
  /// </summary>
  /// <typeparam name="TControl">The type of the control.</typeparam>
  /// <param name="this">This Control.</param>
  /// <param name="task">The task to perform in its thread.</param>
  /// <param name="async">if set to <c>true</c>, do not wait when passed to the gui thread.</param>
  /// <returns>
  ///   <c>true</c> when no thread switch was needed; otherwise, <c>false</c>.
  /// </returns>
  public static bool SafelyInvoke<TControl>(this TControl @this, Action<TControl> task, bool async = true) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(task);

    if (@this.IsDisposed)
      throw new ObjectDisposedException(nameof(@this));

    if (@this.IsHandleCreated)
      if (@this.InvokeRequired) {
        if (async)
          @this.BeginInvoke(task, @this);
        else
          @this.Invoke(task, @this);

        return false;
      } else {
        task(@this);
        return true;
      }

    var context = SynchronizationContext.Current;
    if (context != null) {
      var wrapper = new __ControlActionWithDummyParameterWrapper<TControl> { method = task, control = @this };
      if (async)
        context.Post(wrapper.Invoke, null);
      else
        context.Send(wrapper.Invoke, null);

      return false;
    }

    if (Application.MessageLoop) {
      task(@this);
      return true;
    }

    if (async)
      @this.HandleCreated += new __HandleCallback<TControl> { method = task }.Invoke;
    else {
      using var eventWaiter = new ManualResetEventSlim(false);
      @this.HandleCreated += new __HandleCallback<TControl> { method = task, resetEvent = eventWaiter }.Invoke;
      eventWaiter.Wait();
    }

    return false;
  }

  /// <summary>
  ///   Executes a task in a thread that is definitely not the GUI thread.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="task">The task.</param>
  /// <returns><c>true</c> when a thread switch was needed; otherwise, <c>false</c>.</returns>
  public static bool Async(this Control @this, Action task) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(task);

    if (@this.InvokeRequired) {
      task();
      return false;
    }

    task.BeginInvoke(task.EndInvoke, null);
    return true;
  }

  /// <summary>
  ///   Executes a task in a thread that is definitely not the GUI thread.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="task">The task.</param>
  /// <returns><c>true</c> when a thread switch was needed; otherwise, <c>false</c>.</returns>
  public static bool Async<TControl>(this TControl @this, Action<TControl> task) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(task);

    if (@this.InvokeRequired) {
      task(@this);
      return false;
    }

    task.BeginInvoke(@this, task.EndInvoke, null);
    return true;
  }

  /// <summary>
  ///   Runs something in the gui thread before executing a task in a different thread.
  /// </summary>
  /// <typeparam name="TControl">The type of the type.</typeparam>
  /// <param name="this">The this.</param>
  /// <param name="syncPreAction">The sync pre action.</param>
  /// <param name="task">The task.</param>
  /// <param name="syncPostAction">The sync post action.</param>
  public static void Async<TControl>(this TControl @this, Action<TControl> syncPreAction, Action task, Action<TControl> syncPostAction = null) where TControl : Control {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(syncPreAction);
    Against.ArgumentIsNull(task);

    SafelyInvoke(@this, syncPreAction, false);
    Async(
      @this,
      () => {
        try {
          task();
        } finally {
          if (syncPostAction != null)
            SafelyInvoke(@this, syncPostAction, false);
        }
      }
    );
  }

  /// <summary>
  ///   Gets the trimmed text property, and converts empty values automatically to <c>null</c>.
  /// </summary>
  /// <param name="this">This control.</param>
  /// <returns>A string with text or <c>null</c>.</returns>
  public static string GetTextProperty(this Control @this) {
    Against.ThisIsNull(@this);

    var text = @this.Text;
    if (text == null)
      return null;

    text = text.Trim();
    return text.Length < 1 ? null : text;
  }

  /// <summary>
  ///   Clears the children of a controls and disposes them.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="predicate">The predicate, default to <c>null</c>.</param>
  public static void ClearChildren(this Control @this, Predicate<Control> predicate = null) {
    Against.ThisIsNull(@this);

    var children = @this.Controls;
    if (predicate == null)
      for (var i = children.Count - 1; i >= 0; --i) {
        var child = children[i];
        children.RemoveAt(i);
        child.Dispose();
      }
    else
      for (var i = children.Count - 1; i >= 0; --i) {
        var child = children[i];
        // Check for null before calling a delegate
        if (!predicate(child))
          continue;
        
        // Check for null before calling a delegate
        children.RemoveAt(i);
        child.Dispose();
      }
  }

  /// <summary>
  ///   Returns all child controls, including their child controls.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <returns>An enumeration of child controls.</returns>
  public static IEnumerable<Control> AllControls(this Control @this) {
    Against.ThisIsNull(@this);

    foreach (Control child in @this.Controls) {
      yield return child;
      foreach (var subchild in child.AllControls())
        yield return subchild;
    }
  }

  /// <summary>
  ///   Checks whether the given control reference is <c>null</c> or Disposed.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <returns><c>true</c> if the control reference is <c>null</c> or the control is disposed; otherwise, <c>false</c>.</returns>
  public static bool IsNullOrDisposed(this Control @this) => @this == null || @this.IsDisposed;

  /// <summary>
  ///   enables double buffering on the given control.
  /// </summary>
  /// <param name="this">The control</param>
  public static void EnableDoubleBuffering(this Control @this) {
    Against.ThisIsNull(@this);

    if (SystemInformation.TerminalServerSession)
      return;

    var controlType = @this.GetType();
    var pi = controlType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    if (pi == null)
      return;

    pi.SetValue(@this, true, null);
  }

  /// <summary>
  ///   Tries to duplicate the given control.
  /// </summary>
  /// <param name="this">This Control.</param>
  /// <param name="newName">The name for the new control; defaults to auto-name.</param>
  /// <returns>A new control with the same properties as the given one.</returns>
  public static Control Duplicate(this Control @this, string newName = null) {
    Against.ThisIsNull(@this);

    newName ??= new Guid().ToString();

    switch (@this) {
      case Label label: {
        return new Label {
          AllowDrop = label.AllowDrop,
          Anchor = label.Anchor,
          AutoEllipsis = label.AutoEllipsis,
          AutoSize = label.AutoSize,
          BackColor = label.BackColor,
          BackgroundImage = label.BackgroundImage,
          AutoScrollOffset = label.AutoScrollOffset,
          BackgroundImageLayout = label.BackgroundImageLayout,
          Bounds = label.Bounds,
          Capture = label.Capture,
          Text = label.Text,
          Tag = label.Tag,
          BorderStyle = label.BorderStyle,
          CausesValidation = label.CausesValidation,
          ClientSize = label.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = label.ContextMenu,
#endif
          Cursor = label.Cursor,
          Enabled = label.Enabled,
          Visible = label.Visible,
          Dock = label.Dock,
          FlatStyle = label.FlatStyle,
          Font = label.Font,
          ForeColor = label.ForeColor,
          ContextMenuStrip = label.ContextMenuStrip,
          Location = label.Location,
          Size = label.Size,
          Image = label.Image,
          ImageAlign = label.ImageAlign,
          ImageKey = label.ImageKey,
          ImageIndex = label.ImageIndex,
          ImageList = label.ImageList,
          TextAlign = label.TextAlign,
          Padding = label.Padding,
          Margin = label.Margin,
          UseWaitCursor = label.UseWaitCursor,
          UseMnemonic = label.UseMnemonic,
          Name = newName,
          RightToLeft = label.RightToLeft,
          MinimumSize = label.MinimumSize,
          MaximumSize = label.MaximumSize,
        };
      }
      case Button button:
        return new Button {
          AllowDrop = button.AllowDrop,
          Anchor = button.Anchor,
          AutoEllipsis = button.AutoEllipsis,
          AutoSize = button.AutoSize,
          BackColor = button.BackColor,
          BackgroundImage = button.BackgroundImage,
          AutoScrollOffset = button.AutoScrollOffset,
          BackgroundImageLayout = button.BackgroundImageLayout,
          Bounds = button.Bounds,
          Capture = button.Capture,
          Text = button.Text,
          Tag = button.Tag,
          CausesValidation = button.CausesValidation,
          ClientSize = button.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = button.ContextMenu,
#endif
          Cursor = button.Cursor,
          Enabled = button.Enabled,
          Visible = button.Visible,
          Dock = button.Dock,
          FlatStyle = button.FlatStyle,
          Font = button.Font,
          ForeColor = button.ForeColor,
          ContextMenuStrip = button.ContextMenuStrip,
          Location = button.Location,
          Size = button.Size,
          Image = button.Image,
          ImageAlign = button.ImageAlign,
          ImageKey = button.ImageKey,
          ImageIndex = button.ImageIndex,
          ImageList = button.ImageList,
          TextAlign = button.TextAlign,
          Padding = button.Padding,
          Margin = button.Margin,
          UseWaitCursor = button.UseWaitCursor,
          UseMnemonic = button.UseMnemonic,
          Name = newName,
          RightToLeft = button.RightToLeft,
          MinimumSize = button.MinimumSize,
          MaximumSize = button.MaximumSize,
          TextImageRelation = button.TextImageRelation,
          UseVisualStyleBackColor = button.UseVisualStyleBackColor,
          UseCompatibleTextRendering = button.UseCompatibleTextRendering,
          AutoSizeMode = button.AutoSizeMode,
          DialogResult = button.DialogResult,
        };
      case CheckBox box:
        return new CheckBox {
          AllowDrop = box.AllowDrop,
          Anchor = box.Anchor,
          AutoEllipsis = box.AutoEllipsis,
          AutoSize = box.AutoSize,
          BackColor = box.BackColor,
          BackgroundImage = box.BackgroundImage,
          AutoScrollOffset = box.AutoScrollOffset,
          BackgroundImageLayout = box.BackgroundImageLayout,
          Bounds = box.Bounds,
          Capture = box.Capture,
          Text = box.Text,
          Tag = box.Tag,
          CausesValidation = box.CausesValidation,
          ClientSize = box.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = box.ContextMenu,
#endif
          Cursor = box.Cursor,
          Enabled = box.Enabled,
          Visible = box.Visible,
          Dock = box.Dock,
          FlatStyle = box.FlatStyle,
          Font = box.Font,
          ForeColor = box.ForeColor,
          ContextMenuStrip = box.ContextMenuStrip,
          Location = box.Location,
          Size = box.Size,
          Image = box.Image,
          ImageAlign = box.ImageAlign,
          ImageKey = box.ImageKey,
          ImageIndex = box.ImageIndex,
          ImageList = box.ImageList,
          TextAlign = box.TextAlign,
          Padding = box.Padding,
          Margin = box.Margin,
          UseWaitCursor = box.UseWaitCursor,
          UseMnemonic = box.UseMnemonic,
          Name = newName,
          RightToLeft = box.RightToLeft,
          MinimumSize = box.MinimumSize,
          MaximumSize = box.MaximumSize,
          CheckAlign = box.CheckAlign,
          CheckState = box.CheckState,
          Checked = box.Checked,
          AutoCheck = box.AutoCheck,
          ThreeState = box.ThreeState,
          TextImageRelation = box.TextImageRelation,
          Appearance = box.Appearance,
        };
      case NumericUpDown down:
        return new NumericUpDown {
          AllowDrop = down.AllowDrop,
          Anchor = down.Anchor,
          AutoSize = down.AutoSize,
          BackColor = down.BackColor,
          BackgroundImage = down.BackgroundImage,
          AutoScrollOffset = down.AutoScrollOffset,
          BackgroundImageLayout = down.BackgroundImageLayout,
          Bounds = down.Bounds,
          Capture = down.Capture,
          Text = down.Text,
          Tag = down.Tag,
          CausesValidation = down.CausesValidation,
          ClientSize = down.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = down.ContextMenu,
#endif
          Cursor = down.Cursor,
          Enabled = down.Enabled,
          Visible = down.Visible,
          Dock = down.Dock,
          Font = down.Font,
          ForeColor = down.ForeColor,
          ContextMenuStrip = down.ContextMenuStrip,
          Location = down.Location,
          Size = down.Size,
          TextAlign = down.TextAlign,
          Padding = @this.Padding,
          Margin = down.Margin,
          UseWaitCursor = down.UseWaitCursor,
          Name = newName,
          RightToLeft = down.RightToLeft,
          MinimumSize = down.MinimumSize,
          MaximumSize = down.MaximumSize,
          Minimum = down.Minimum,
          Maximum = down.Maximum,
          Value = down.Value,
          UpDownAlign = down.UpDownAlign,
          ThousandsSeparator = down.ThousandsSeparator,
          ReadOnly = down.ReadOnly,
          InterceptArrowKeys = down.InterceptArrowKeys,
          Increment = down.Increment,
          Hexadecimal = down.Hexadecimal,
          DecimalPlaces = down.DecimalPlaces,
          BorderStyle = down.BorderStyle,
          AutoValidate = down.AutoValidate,
          AutoScroll = down.AutoScroll,
          AutoScrollMargin = down.AutoScrollMargin,
          AutoScrollMinSize = down.AutoScrollMinSize,
          AutoScrollPosition = down.AutoScrollPosition
        };
      case ComboBox box: {
        var comboBox = new ComboBox {
          AllowDrop = box.AllowDrop,
          Anchor = box.Anchor,
          AutoSize = box.AutoSize,
          BackColor = box.BackColor,
          BackgroundImage = box.BackgroundImage,
          AutoScrollOffset = box.AutoScrollOffset,
          BackgroundImageLayout = box.BackgroundImageLayout,
          Bounds = box.Bounds,
          Capture = box.Capture,
          Text = box.Text,
          Tag = box.Tag,
          CausesValidation = box.CausesValidation,
          ClientSize = box.ClientSize,
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = box.ContextMenu,
#endif
          Cursor = box.Cursor,
          Enabled = box.Enabled,
          Visible = box.Visible,
          Dock = box.Dock,
          FlatStyle = box.FlatStyle,
          Font = box.Font,
          ForeColor = box.ForeColor,
          ContextMenuStrip = box.ContextMenuStrip,
          Location = box.Location,
          Size = box.Size,
          Padding = @this.Padding,
          Margin = box.Margin,
          UseWaitCursor = box.UseWaitCursor,
          Name = newName,
          RightToLeft = box.RightToLeft,
          MinimumSize = box.MinimumSize,
          MaximumSize = box.MaximumSize,
          DisplayMember = box.DisplayMember,
          ValueMember = box.ValueMember,
          SelectedText = box.SelectedText,
          MaxLength = box.MaxLength,
          MaxDropDownItems = box.MaxDropDownItems,
          DropDownHeight = box.DropDownHeight,
          DropDownStyle = box.DropDownStyle,
          DropDownWidth = box.DropDownWidth,
          DroppedDown = box.DroppedDown,
        };
        if (box.DataSource == null) {
          var items = box.Items;
          if (items.Count <= 0)
            return comboBox;

          comboBox.Items.AddRange(items.Cast<object>().ToArray());
          comboBox.SelectedItem = box.SelectedItem;
          comboBox.SelectedIndex = box.SelectedIndex;
        } else {
          comboBox.DataSource = box.DataSource;
          comboBox.SelectedValue = box.SelectedValue;
        }

        return comboBox;
      }
      default: throw new NotSupportedException();
    }
  }

  private static void _AddBinding<TControl, TSource>(this TControl @this, object bindingSource, Expression<Func<TControl, TSource, bool>> expression, Type controlType, Type sourceType, DataSourceUpdateMode mode, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control {
    const string excMessage = @"Must be an expression like :
(control, source) => control.propertyName == source.dataMember
(control, source) => control.propertyName == (type)source.dataMember
(control, source) => control.propertyName == source.dataMember.ToString()
(control, source) => control.propertyName == source.subMember.dataMember
(control, source) => control.propertyName == (type)source.subMember.dataMember
(control, source) => control.propertyName == source.subMember.dataMember.ToString()
(control, source) => control.propertyName == source.....dataMember
(control, source) => control.propertyName == (type)source.....dataMember
(control, source) => control.propertyName == source.....dataMember.ToString()
";

    if (expression.Body is not BinaryExpression { NodeType: ExpressionType.Equal } body)
      throw new ArgumentException(excMessage);

    var propertyName = GetControlPropertyName(body.Left) ?? throw new ArgumentException(excMessage);
    var dataMember = GetBindingSourcePropertyName(body.Right) ?? throw new ArgumentException(excMessage);

    var binding = new Binding(propertyName, bindingSource, dataMember, true) { DataSourceUpdateMode = mode };
    if (customConversionHandler != null)
      binding.Format += customConversionHandler;

    if (bindingCompleteCallback != null)
      binding.BindingComplete += bindingCompleteCallback;

    @this.DataBindings.Add(binding);
    return;

    string GetBindingSourcePropertyName(Expression e)
      => e switch {
        MemberExpression member => // controlProperty = bs.PropertyName
          GetPropertyName(member),
        UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression member } => GetPropertyName(member),
        MethodCallExpression { Method.Name: nameof(ToString), Arguments.Count: 0, Object: MemberExpression member } => GetPropertyName(member),
        _ => null
      }
    ;

    string GetControlPropertyName(Expression e) 
      => e switch {
        MemberExpression { Expression: ParameterExpression parameter } member when parameter.Type == controlType => member.Member.Name,
        UnaryExpression {
          NodeType: ExpressionType.Convert,
          Operand: MemberExpression { Expression: ParameterExpression parameter } member
        } when parameter.Type == controlType => member.Member.Name,
        _ => null
      }
    ;

    string GetPropertyName(MemberExpression member) {
      if (
        member.Expression is ParameterExpression parameter
        && parameter.Type == sourceType
      )
        return member.Member.Name;

      if (member.Expression is not MemberExpression parent)
        return null;

      // walk the expression
      var propertyNameChain = new Stack<string>();
      do {
        propertyNameChain.Push(parent.Member.Name);
        parent = parent.Expression as MemberExpression;
      } while (parent != null);

      // create sources for members
      do {
        var propName = propertyNameChain.Pop();
        bindingSource = new BindingSource(bindingSource, propName);
      } while (propertyNameChain.Count > 0);

      return member.Member.Name;
    }
  }

  public static void AddBinding<TControl, TSource>(this TControl @this, TSource source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
    => _AddBinding(@this, source, expression, @this.GetType(), source.GetType(), mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TControl, TSource>(this TControl @this, object source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
    => _AddBinding(@this, source, expression, typeof(TControl), typeof(TSource), mode, customConversionHandler, bindingCompleteCallback);

  #region to make life easier

  public static void AddBinding<TSource>(
    this Label @this,
    object source,
    Expression<Func<Label, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<Label, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this CheckBox @this,
    object source,
    Expression<Func<CheckBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<CheckBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this TextBox @this,
    object source,
    Expression<Func<TextBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<TextBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this NumericUpDown @this,
    object source,
    Expression<Func<NumericUpDown, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<NumericUpDown, TSource>(
      @this,
      source,
      expression,
      mode,
      customConversionHandler,
      bindingCompleteCallback
    );

  public static void AddBinding<TSource>(
    this RadioButton @this,
    object source,
    Expression<Func<RadioButton, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<RadioButton, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this Button @this,
    object source,
    Expression<Func<Button, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<Button, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this GroupBox @this,
    object source,
    Expression<Func<GroupBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<GroupBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this ComboBox @this,
    object source,
    Expression<Func<ComboBox, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<ComboBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

  public static void AddBinding<TSource>(
    this DateTimePicker @this,
    object source,
    Expression<Func<DateTimePicker, TSource, bool>> expression,
    DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged,
    ConvertEventHandler customConversionHandler = null,
    BindingCompleteEventHandler bindingCompleteCallback = null
  ) =>
    AddBinding<DateTimePicker, TSource>(
      @this,
      source,
      expression,
      mode,
      customConversionHandler,
      bindingCompleteCallback
    );

  #endregion

  /// <summary>
  ///   Checks if this or parents are in DesignMode.
  ///   Use this instead because <see cref="System.ComponentModel.Component.DesignMode">Component.DesignMode</see> is only
  ///   set after
  ///   <see cref="Form.Load" /> was invoked.
  /// </summary>
  /// <param name="this">The control on which to check if it's in DesignMode.</param>
  /// <returns>True if DesignMode.</returns>
  public static bool IsDesignMode(this Control @this)
    => LicenseManager.UsageMode == LicenseUsageMode.Designtime
       || IsDesignModeDetector.Instance.GetDesignModePropertyValue(@this);
}
