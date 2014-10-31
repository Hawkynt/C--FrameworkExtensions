#region (c)2010-2020 Hawkynt
/*
  This file is part of Hawkynt's .NET Framework extensions.

    Hawkynt's .NET Framework extensions are free software: 
    you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Hawkynt's .NET Framework extensions is distributed in the hope that 
    it will be useful, but WITHOUT ANY WARRANTY; without even the implied 
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
    the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Hawkynt's .NET Framework extensions.  
    If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace System.Windows.Forms {
  internal static partial class ControlExtensions {

    /// <summary>
    /// Executes the given action with the current control after a period of time.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <param name="dueTime">The time to wait until execution.</param>
    /// <param name="action">The action.</param>
    public static void SetTimeout<TControl>(this TControl This, TimeSpan dueTime, Action<TControl> action) where TControl : Control {
      Contract.Requires(This != null);
      Contract.Requires(action != null);
      This.Async(() => {
        Thread.Sleep(dueTime);
        This.SafelyInvoke(() => action(This));
      });
    }

    /// <summary>
    /// Gets the position of a given control relative to its form.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <returns>The position of it relative to it's form.</returns>
    public static Drawing.Point GetLocationOnForm(this Control This) {
      Contract.Requires(This != null);
      var result = This.Location;
      var c = This;
      for (; (c as Form) == null; c = c.Parent)
        result.Offset(c.Location);
      return (result);
    }

    public static Drawing.Point GetLocationOnClient(this Control This) {
      Contract.Requires(This != null);
      var result = Drawing.Point.Empty;
      var c = This;
      for (; c.Parent != null; c = c.Parent)
        result.Offset(c.Location);
      return (result);
    }

    /// <summary>
    /// Safely invokes the given code on the control's thread.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <param name="task">The task to perform in its thread.</param>
    /// <returns><c>true</c> when no thread switch was needed; otherwise, <c>false</c>.</returns>
    public static bool SafelyInvoke(this Control This, Action task, bool @async = true) {
      Contract.Requires(This != null);

      if (This.InvokeRequired) {

        // switch to gui thread if needed
        if (@async)
          This.BeginInvoke(task);
        else
          This.Invoke(task);
        return (false);
      }

      // already on gui thread
      if (!This.IsHandleCreated) {

        // handle has not yet been created - puuuh
        Action action = () => {
          var tries = 300;
          while (!This.IsHandleCreated && --tries > 0)
            Thread.Sleep(10);
          if (tries <= 0)
            throw new NotSupportedException("Timed out waiting for the handle of the control to be created.");

          // try again because it seems that the handle has yet been created
          This.SafelyInvoke(task);
        };

        // wait on another thread, so this gui thread can go on
        action.BeginInvoke(action.EndInvoke, null);


        return (false);
      }

      if (This.IsDisposed)
        throw new ObjectDisposedException("Control already disposed.");

      task();
      return (true);
    }


    /// <summary>
    /// Safelies executes an action and returns the result..
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="function">The function.</param>
    /// <returns>Whatever the method returned.</returns>
    public static TResult SafelyInvoke<TResult>(this Control This, Func<TResult> function) {
      return (This.InvokeRequired ? (TResult)This.Invoke(function) : function());
    }

    /// <summary>
    /// Safelies executes an action and returns the result..
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="function">The function.</param>
    /// <returns>
    /// Whatever the method returned.
    /// </returns>
    public static TResult SafelyInvoke<TControl, TResult>(this TControl This, Func<TControl, TResult> function) where TControl : Control {
      return (This.InvokeRequired ? (TResult)This.Invoke(function, This) : function(This));
    }

    /// <summary>
    /// Safely invokes the given code on the control's thread.
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="This">This Control.</param>
    /// <param name="task">The task to perform in its thread.</param>
    /// <returns><c>true</c> when no thread switch was needed; otherwise, <c>false</c>.</returns>
    public static bool SafelyInvoke<TControl>(this TControl This, Action<TControl> task, bool @async = true) where TControl : Control {
      Contract.Requires(This != null);

      if (This.InvokeRequired) {

        // switch to gui thread if needed
        if (@async)
          This.BeginInvoke(task, This);
        else
          This.Invoke(task, This);
        return (false);
      }

      // already on gui thread
      if (!This.IsHandleCreated) {

        // handle has not yet been created - puuuh
        Action action = () => {
          var tries = 300;
          while (!This.IsHandleCreated && --tries > 0)
            Thread.Sleep(10);
          if (tries <= 0)
            throw new NotSupportedException("Timed out waiting for the handle of the control to be created.");

          // try again because it seems that the handle has yet been created
          This.SafelyInvoke(task);
        };

        // wait on another thread, so this gui thread can go on
        action.BeginInvoke(action.EndInvoke, null);

        return (false);
      }

      if (This.IsDisposed)
        throw new ObjectDisposedException("Control already disposed.");

      task(This);
      return (true);
    }

    /// <summary>
    /// Executes a task in a thread that is definitely not the GUI thread.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <param name="task">The task.</param>
    /// <returns><c>true</c> when a thread switch was needed; otherwise, <c>false</c>.</returns>
    public static bool Async(this Control This, Action task) {
      Contract.Requires(This != null);
      Contract.Requires(task != null);
      if (This.InvokeRequired) {
        task();
        return (false);
      }
      task.BeginInvoke(task.EndInvoke, null);
      return (true);
    }

    /// <summary>
    /// Runs something in the gui thread before executing a task in a different thread.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="This">The this.</param>
    /// <param name="syncPreAction">The sync pre action.</param>
    /// <param name="task">The task.</param>
    /// <param name="syncPostAction">The sync post action.</param>
    public static void Async<TType>(this TType This, Action<TType> syncPreAction, Action task, Action<TType> syncPostAction = null) where TType : Control {
      Contract.Requires(This != null);
      Contract.Requires(syncPreAction != null);
      Contract.Requires(task != null);
      This.SafelyInvoke(() => syncPreAction(This), false);
      This.Async(() => {
        try {
          task();
        } finally {
          if (syncPostAction != null)
            This.SafelyInvoke(() => syncPostAction(This), false);
        }
      });
    }

    /// <summary>
    /// Gets the text property, and converts empty values automatically to <c>null</c>.
    /// </summary>
    /// <param name="This">This control.</param>
    /// <returns>A string with text or <c>null</c>.</returns>
    public static string GetTextProperty(this Control This) {
      Contract.Requires(This != null);
      var r = Contract.Result<string>();
      Contract.Ensures(r == null || r.Length > 0);
      var text = This.Text;
      if (string.IsNullOrWhiteSpace(text))
        return null;
      Contract.Assume(text.Trim().Length > 0);
      return text.Trim();
    }

    /// <summary>
    /// Clears the children of a controls and disposes them.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <param name="predicate">The predicate, default to <c>null</c>.</param>
    public static void ClearChildren(this Control This, Predicate<Control> predicate = null) {
      Contract.Requires(This != null);
      var children = This.Controls;
      foreach (var child in children.Cast<Control>().Where(c => c != null && (predicate == null || predicate(c))).ToArray()) {
        children.Remove(child);
        child.Dispose();
      }
    }

    /// <summary>
    /// Returns all child controls, including their child controls.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <returns>An enumeration of child controls.</returns>
    public static IEnumerable<Control> AllControls(this Control This) {
      Contract.Requires(This != null);
      Contract.Ensures(Contract.Result<IEnumerable<Control>>() != null);
      foreach (Control child in This.Controls) {
        yield return child;
        foreach (var subchild in child.AllControls())
          yield return subchild;
      }
    }

    /// <summary>
    /// Tries to duplicate the given control.
    /// </summary>
    /// <param name="This">This Control.</param>
    /// <param name="newName">The name for the new control; defaults to auto-name.</param>
    /// <returns>A new control with the same properties as the given one.</returns>
    public static Control Duplicate(this Control This, string newName = null) {
      Contract.Requires(This != null);
      if (newName == null)
        newName = new Guid().ToString();
      if (This is Label) {
        var label = (new Label {
          AllowDrop = This.AllowDrop,
          Anchor = This.Anchor,
          AutoEllipsis = ((Label)This).AutoEllipsis,
          AutoSize = This.AutoSize,
          BackColor = This.BackColor,
          BackgroundImage = This.BackgroundImage,
          AutoScrollOffset = This.AutoScrollOffset,
          BackgroundImageLayout = This.BackgroundImageLayout,
          Bounds = This.Bounds,
          Capture = This.Capture,
          Text = This.Text,
          Tag = This.Tag,
          BorderStyle = ((Label)This).BorderStyle,
          CausesValidation = This.CausesValidation,
          ClientSize = This.ClientSize,
          ContextMenu = This.ContextMenu,
          Cursor = This.Cursor,
          Enabled = This.Enabled,
          Visible = This.Visible,
          Dock = This.Dock,
          FlatStyle = ((Label)This).FlatStyle,
          Font = This.Font,
          ForeColor = This.ForeColor,
          ContextMenuStrip = This.ContextMenuStrip,
          Location = This.Location,
          Size = This.Size,
          Image = ((Label)This).Image,
          ImageAlign = ((Label)This).ImageAlign,
          ImageKey = ((Label)This).ImageKey,
          ImageIndex = ((Label)This).ImageIndex,
          ImageList = ((Label)This).ImageList,
          TextAlign = ((Label)This).TextAlign,
          Padding = This.Padding,
          Margin = This.Margin,
          UseWaitCursor = This.UseWaitCursor,
          UseMnemonic = ((Label)This).UseMnemonic,
          Name = newName,
          RightToLeft = This.RightToLeft,
          MinimumSize = This.MinimumSize,
          MaximumSize = This.MaximumSize,
        });
        return label;
      }
      newName = new Guid().ToString();
      if (This is Button) {
        return (new Button {
          AllowDrop = This.AllowDrop,
          Anchor = This.Anchor,
          AutoEllipsis = ((Button)This).AutoEllipsis,
          AutoSize = This.AutoSize,
          BackColor = This.BackColor,
          BackgroundImage = This.BackgroundImage,
          AutoScrollOffset = This.AutoScrollOffset,
          BackgroundImageLayout = This.BackgroundImageLayout,
          Bounds = This.Bounds,
          Capture = This.Capture,
          Text = This.Text,
          Tag = This.Tag,
          CausesValidation = This.CausesValidation,
          ClientSize = This.ClientSize,
          ContextMenu = This.ContextMenu,
          Cursor = This.Cursor,
          Enabled = This.Enabled,
          Visible = This.Visible,
          Dock = This.Dock,
          FlatStyle = ((Button)This).FlatStyle,
          Font = This.Font,
          ForeColor = This.ForeColor,
          ContextMenuStrip = This.ContextMenuStrip,
          Location = This.Location,
          Size = This.Size,
          Image = ((Button)This).Image,
          ImageAlign = ((Button)This).ImageAlign,
          ImageKey = ((Button)This).ImageKey,
          ImageIndex = ((Button)This).ImageIndex,
          ImageList = ((Button)This).ImageList,
          TextAlign = ((Button)This).TextAlign,
          Padding = This.Padding,
          Margin = This.Margin,
          UseWaitCursor = This.UseWaitCursor,
          UseMnemonic = ((Button)This).UseMnemonic,
          Name = newName,
          RightToLeft = This.RightToLeft,
          MinimumSize = This.MinimumSize,
          MaximumSize = This.MaximumSize,
          TextImageRelation = ((Button)This).TextImageRelation,
          UseVisualStyleBackColor = ((Button)This).UseVisualStyleBackColor,
          UseCompatibleTextRendering = ((Button)This).UseCompatibleTextRendering,
          AutoSizeMode = ((Button)This).AutoSizeMode,
          DialogResult = ((Button)This).DialogResult,
        });
      }
      if (This is CheckBox) {
        return (new CheckBox {
          AllowDrop = This.AllowDrop,
          Anchor = This.Anchor,
          AutoEllipsis = ((CheckBox)This).AutoEllipsis,
          AutoSize = This.AutoSize,
          BackColor = This.BackColor,
          BackgroundImage = This.BackgroundImage,
          AutoScrollOffset = This.AutoScrollOffset,
          BackgroundImageLayout = This.BackgroundImageLayout,
          Bounds = This.Bounds,
          Capture = This.Capture,
          Text = This.Text,
          Tag = This.Tag,
          CausesValidation = This.CausesValidation,
          ClientSize = This.ClientSize,
          ContextMenu = This.ContextMenu,
          Cursor = This.Cursor,
          Enabled = This.Enabled,
          Visible = This.Visible,
          Dock = This.Dock,
          FlatStyle = ((CheckBox)This).FlatStyle,
          Font = This.Font,
          ForeColor = This.ForeColor,
          ContextMenuStrip = This.ContextMenuStrip,
          Location = This.Location,
          Size = This.Size,
          Image = ((CheckBox)This).Image,
          ImageAlign = ((CheckBox)This).ImageAlign,
          ImageKey = ((CheckBox)This).ImageKey,
          ImageIndex = ((CheckBox)This).ImageIndex,
          ImageList = ((CheckBox)This).ImageList,
          TextAlign = ((CheckBox)This).TextAlign,
          Padding = This.Padding,
          Margin = This.Margin,
          UseWaitCursor = This.UseWaitCursor,
          UseMnemonic = ((CheckBox)This).UseMnemonic,
          Name = newName,
          RightToLeft = This.RightToLeft,
          MinimumSize = This.MinimumSize,
          MaximumSize = This.MaximumSize,
          CheckAlign = ((CheckBox)This).CheckAlign,
          CheckState = ((CheckBox)This).CheckState,
          Checked = ((CheckBox)This).Checked,
          AutoCheck = ((CheckBox)This).AutoCheck,
          ThreeState = ((CheckBox)This).ThreeState,
          TextImageRelation = ((CheckBox)This).TextImageRelation,
          Appearance = ((CheckBox)This).Appearance,
        });

      }
      if (This is NumericUpDown) {
        return (new NumericUpDown {
          AllowDrop = This.AllowDrop,
          Anchor = This.Anchor,
          AutoSize = This.AutoSize,
          BackColor = This.BackColor,
          BackgroundImage = This.BackgroundImage,
          AutoScrollOffset = This.AutoScrollOffset,
          BackgroundImageLayout = This.BackgroundImageLayout,
          Bounds = This.Bounds,
          Capture = This.Capture,
          Text = This.Text,
          Tag = This.Tag,
          CausesValidation = This.CausesValidation,
          ClientSize = This.ClientSize,
          ContextMenu = This.ContextMenu,
          Cursor = This.Cursor,
          Enabled = This.Enabled,
          Visible = This.Visible,
          Dock = This.Dock,
          Font = This.Font,
          ForeColor = This.ForeColor,
          ContextMenuStrip = This.ContextMenuStrip,
          Location = This.Location,
          Size = This.Size,
          TextAlign = ((NumericUpDown)This).TextAlign,
          Padding = This.Padding,
          Margin = This.Margin,
          UseWaitCursor = This.UseWaitCursor,
          Name = newName,
          RightToLeft = This.RightToLeft,
          MinimumSize = This.MinimumSize,
          MaximumSize = This.MaximumSize,
          Minimum = ((NumericUpDown)This).Minimum,
          Maximum = ((NumericUpDown)This).Maximum,
          Value = ((NumericUpDown)This).Value,
          UpDownAlign = ((NumericUpDown)This).UpDownAlign,
          ThousandsSeparator = ((NumericUpDown)This).ThousandsSeparator,
          ReadOnly = ((NumericUpDown)This).ReadOnly,
          InterceptArrowKeys = ((NumericUpDown)This).InterceptArrowKeys,
          Increment = ((NumericUpDown)This).Increment,
          Hexadecimal = ((NumericUpDown)This).Hexadecimal,
          DecimalPlaces = ((NumericUpDown)This).DecimalPlaces,
          BorderStyle = ((NumericUpDown)This).BorderStyle,
          AutoValidate = ((NumericUpDown)This).AutoValidate,
          AutoScroll = ((NumericUpDown)This).AutoScroll,
          AutoScrollMargin = ((NumericUpDown)This).AutoScrollMargin,
          AutoScrollMinSize = ((NumericUpDown)This).AutoScrollMinSize,
          AutoScrollPosition = ((NumericUpDown)This).AutoScrollPosition
        });

      }

      if (This is ComboBox) {
        var comboBox = new ComboBox() {
          AllowDrop = This.AllowDrop,
          Anchor = This.Anchor,
          AutoSize = This.AutoSize,
          BackColor = This.BackColor,
          BackgroundImage = This.BackgroundImage,
          AutoScrollOffset = This.AutoScrollOffset,
          BackgroundImageLayout = This.BackgroundImageLayout,
          Bounds = This.Bounds,
          Capture = This.Capture,
          Text = This.Text,
          Tag = This.Tag,
          CausesValidation = This.CausesValidation,
          ClientSize = This.ClientSize,
          ContextMenu = This.ContextMenu,
          Cursor = This.Cursor,
          Enabled = This.Enabled,
          Visible = This.Visible,
          Dock = This.Dock,
          FlatStyle = ((ComboBox)This).FlatStyle,
          Font = This.Font,
          ForeColor = This.ForeColor,
          ContextMenuStrip = This.ContextMenuStrip,
          Location = This.Location,
          Size = This.Size,
          Padding = This.Padding,
          Margin = This.Margin,
          UseWaitCursor = This.UseWaitCursor,
          Name = newName,
          RightToLeft = This.RightToLeft,
          MinimumSize = This.MinimumSize,
          MaximumSize = This.MaximumSize,
          DisplayMember = ((ComboBox)This).DisplayMember,
          ValueMember = ((ComboBox)This).ValueMember,
          SelectedText = ((ComboBox)This).SelectedText,
          MaxLength = ((ComboBox)This).MaxLength,
          MaxDropDownItems = ((ComboBox)This).MaxDropDownItems,
          DropDownHeight = ((ComboBox)This).DropDownHeight,
          DropDownStyle = ((ComboBox)This).DropDownStyle,
          DropDownWidth = ((ComboBox)This).DropDownWidth,
          DroppedDown = ((ComboBox)This).DroppedDown,
        };
        if (((ComboBox)This).DataSource == null) {
          var items = ((ComboBox)This).Items;
          if (items.Count > 0) {
            comboBox.Items.AddRange(items.Cast<object>().ToArray());
            comboBox.SelectedItem = ((ComboBox)This).SelectedItem;
            comboBox.SelectedIndex = ((ComboBox)This).SelectedIndex;
          }
        } else {
          comboBox.DataSource = ((ComboBox)This).DataSource;
          comboBox.SelectedValue = ((ComboBox)This).SelectedValue;
        }
        return comboBox;
      }
      throw new NotSupportedException();

    }
  }
}