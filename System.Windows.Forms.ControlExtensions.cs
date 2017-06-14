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
using System.Linq;
using System.Threading;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.Windows.Forms {
  internal static partial class ControlExtensions {

    /// <summary>
    /// The token that resumes layout on disposal.
    /// </summary>
    public interface ISuspendedLayoutToken : IDisposable { }

    private class SuspendedLayoutToken : ISuspendedLayoutToken {
      private readonly Control _targetControl;
      public SuspendedLayoutToken(Control targetControl) {
        targetControl.SuspendLayout();
        this._targetControl = targetControl;
      }

      ~SuspendedLayoutToken() {
        this._Dispose(false);
      }

      private void _Dispose(bool isManagedDisposal) {
        this._targetControl.ResumeLayout(true);
        if (isManagedDisposal)
          GC.SuppressFinalize(this);
      }

      public void Dispose() => this._Dispose(true);
    }

    /// <summary>
    /// Stops the layout and returns a token which will continue layoutint on disposal.
    /// </summary>
    /// <param name="this">The this.</param>
    /// <returns></returns>
    public static ISuspendedLayoutToken PauseLayout(this Control @this) {
      if (@this == null) throw new NullReferenceException();

      return new SuspendedLayoutToken(@this);
    }

    /// <summary>
    /// Executes the given action with the current control after a period of time.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="dueTime">The time to wait until execution.</param>
    /// <param name="action">The action.</param>
    public static void SetTimeout<TControl>(this TControl @this, TimeSpan dueTime, Action<TControl> action) where TControl : Control {
      if (@this == null) throw new NullReferenceException();
      if (action == null) throw new ArgumentNullException(nameof(action));

      @this.Async(() => {
        Thread.Sleep(dueTime);
        @this.SafelyInvoke(() => action(@this));
      });
    }

    /// <summary>
    /// Gets the position of a given control relative to its form.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns>The position of it relative to it's form.</returns>
    public static Drawing.Point GetLocationOnForm(this Control @this) {
      if (@this == null) throw new NullReferenceException();

      var result = @this.Location;
      var c = @this;
      for (; c as Form == null; c = c.Parent)
        result.Offset(c.Location);
      return result;
    }

    public static Drawing.Point GetLocationOnClient(this Control @this) {
      if (@this == null) throw new NullReferenceException();

      var result = Drawing.Point.Empty;
      var c = @this;
      for (; c.Parent != null; c = c.Parent)
        result.Offset(c.Location);

      return result;
    }

    /// <summary>
    /// Safely invokes the given code on the control's thread.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="task">The task to perform in its thread.</param>
    /// <param name="async">if set to <c>true</c> calls asynchronous.</param>
    /// <returns>
    ///   <c>true</c> when no thread switch was needed; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ObjectDisposedException">Control already disposed.</exception>
    public static bool SafelyInvoke(this Control @this, Action task, bool @async = true) {
      if (@this == null) throw new NullReferenceException();
      if (task == null) throw new ArgumentNullException(nameof(task));

      if (@this.InvokeRequired) {

        // switch to gui thread if needed
        if (@async)
          @this.BeginInvoke(task);
        else
          @this.Invoke(task);

        return false;
      }

      // already on gui thread
      if (!@this.IsHandleCreated) {

        // handle has not yet been created - puuuh
        Action action = () => {
          var tries = 1000;
          while (!@this.IsHandleCreated && --tries > 0)
            Thread.Sleep(10);

          if (tries <= 0)
            throw new NotSupportedException("Timed out waiting for the handle of the control to be created.");

          // try again because it seems that the handle has yet been created
          SafelyInvoke(@this, task);
        };

        // wait on another thread, so this gui thread can go on
        if (@async)
          action.BeginInvoke(action.EndInvoke, null);
        else
          action.Invoke();

        return false;
      }

      if (@this.IsDisposed)
        throw new ObjectDisposedException("Control already disposed.");

      task();
      return true;
    }


    /// <summary>
    /// Safelies executes an action and returns the result..
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="function">The function.</param>
    /// <returns>Whatever the method returned.</returns>
    public static TResult SafelyInvoke<TResult>(this Control @this, Func<TResult> function) {
      if (@this == null) throw new NullReferenceException();
      if (function == null) throw new ArgumentNullException(nameof(function));

      if (@this.IsDisposed)
        return default(TResult);

      return @this.InvokeRequired ? (TResult)@this.Invoke(function) : function();
    }

    /// <summary>
    /// Safelies executes an action and returns the result..
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="function">The function.</param>
    /// <returns>
    /// Whatever the method returned.
    /// </returns>
    public static TResult SafelyInvoke<TControl, TResult>(this TControl @this, Func<TControl, TResult> function) where TControl : Control {
      if (@this == null) throw new NullReferenceException();
      if (function == null) throw new ArgumentNullException(nameof(function));

      if (@this.IsDisposed)
        return default(TResult);

      return @this.InvokeRequired ? (TResult)@this.Invoke(function, @this) : function(@this);
    }

    /// <summary>
    /// Safely invokes the given code on the control's thread.
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="this">This Control.</param>
    /// <param name="task">The task to perform in its thread.</param>
    /// <param name="async">if set to <c>true</c> [@async].</param>
    /// <returns>
    ///   <c>true</c> when no thread switch was needed; otherwise, <c>false</c>.
    /// </returns>
    public static bool SafelyInvoke<TControl>(this TControl @this, Action<TControl> task, bool @async = true) where TControl : Control {
      if (@this == null) throw new NullReferenceException();
      if (task == null) throw new ArgumentNullException(nameof(task));

      if (@this.InvokeRequired) {

        // switch to gui thread if needed
        if (@async) {
          @this.BeginInvoke(task, @this);
        } else
          @this.Invoke(task, @this);
        return false;
      }

      // already on gui thread
      if (!@this.IsHandleCreated) {

        // handle has not yet been created - puuuh
        Action action = () => {
          var tries = 1000;
          while (!@this.IsHandleCreated && --tries > 0)
            Thread.Sleep(10);
          if (tries <= 0)
            throw new NotSupportedException("Timed out waiting for the handle of the control to be created.");

          // try again because it seems that the handle has yet been created
          SafelyInvoke(@this, task);
        };

        // wait on another thread, so this gui thread can go on
        action.BeginInvoke(action.EndInvoke, null);

        return false;
      }

      if (@this.IsDisposed)
        throw new ObjectDisposedException("Control already disposed.");

      task(@this);
      return true;
    }

    /// <summary>
    /// Executes a task in a thread that is definitely not the GUI thread.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="task">The task.</param>
    /// <returns><c>true</c> when a thread switch was needed; otherwise, <c>false</c>.</returns>
#pragma warning disable CC0072 // Remove Async termination when method is not asynchronous.
    public static bool Async(this Control @this, Action task) {
#pragma warning restore CC0072 // Remove Async termination when method is not asynchronous.
      if (@this == null) throw new NullReferenceException();
      if (task == null) throw new ArgumentNullException(nameof(task));

      if (@this.InvokeRequired) {
        task();
        return false;
      }

      task.BeginInvoke(task.EndInvoke, null);
      return true;
    }

    /// <summary>
    /// Runs something in the gui thread before executing a task in a different thread.
    /// </summary>
    /// <typeparam name="TType">The type of the type.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="syncPreAction">The sync pre action.</param>
    /// <param name="task">The task.</param>
    /// <param name="syncPostAction">The sync post action.</param>
#pragma warning disable CC0072 // Remove Async termination when method is not asynchronous.
    public static void Async<TType>(this TType @this, Action<TType> syncPreAction, Action task, Action<TType> syncPostAction = null) where TType : Control {
#pragma warning restore CC0072 // Remove Async termination when method is not asynchronous.
      if (@this == null) throw new NullReferenceException();
      if (syncPreAction == null) throw new ArgumentNullException(nameof(syncPreAction));
      if (task == null) throw new ArgumentNullException(nameof(task));

      SafelyInvoke(@this, () => syncPreAction(@this), false);
      Async(@this, () => {
        try {
          task();
        } finally {
          if (syncPostAction != null)
            @this.SafelyInvoke(() => syncPostAction(@this), false);
        }
      });
    }

    /// <summary>
    /// Gets the trimmed text property, and converts empty values automatically to <c>null</c>.
    /// </summary>
    /// <param name="this">This control.</param>
    /// <returns>A string with text or <c>null</c>.</returns>
    public static string GetTextProperty(this Control @this) {
      if (@this == null) throw new NullReferenceException();

      var text = @this.Text;
      if (text == null)
        return null;

      text = text.Trim();
      return text.Length < 1 ? null : text;
    }

    /// <summary>
    /// Clears the children of a controls and disposes them.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="predicate">The predicate, default to <c>null</c>.</param>
    public static void ClearChildren(this Control @this, Predicate<Control> predicate = null) {
      if (@this == null) throw new NullReferenceException();

      var children = @this.Controls;
      foreach (var child in children.Cast<Control>().Where(c => c != null && (predicate == null || predicate(c))).ToArray()) {
        children.Remove(child);
        child.Dispose();
      }
    }

    /// <summary>
    /// Returns all child controls, including their child controls.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns>An enumeration of child controls.</returns>
    public static IEnumerable<Control> AllControls(this Control @this) {
      if (@this == null) throw new NullReferenceException();

      foreach (Control child in @this.Controls) {
        yield return child;
        foreach (var subchild in child.AllControls())
          yield return subchild;
      }
    }

    /// <summary>
    /// Checks whether the given control reference is <c>null</c> or Disposed.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns><c>true</c> if the control reference is <c>null</c> or the control is disposed; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrDisposed(this Control @this) => @this == null || @this.IsDisposed;

    /// <summary>
    /// Tries to duplicate the given control.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="newName">The name for the new control; defaults to auto-name.</param>
    /// <returns>A new control with the same properties as the given one.</returns>
    public static Control Duplicate(this Control @this, string newName = null) {
      if (@this == null) throw new NullReferenceException();

      if (newName == null)
        newName = new Guid().ToString();

      if (@this is Label) {
        var label = new Label {
          AllowDrop = @this.AllowDrop,
          Anchor = @this.Anchor,
          AutoEllipsis = ((Label)@this).AutoEllipsis,
          AutoSize = @this.AutoSize,
          BackColor = @this.BackColor,
          BackgroundImage = @this.BackgroundImage,
          AutoScrollOffset = @this.AutoScrollOffset,
          BackgroundImageLayout = @this.BackgroundImageLayout,
          Bounds = @this.Bounds,
          Capture = @this.Capture,
          Text = @this.Text,
          Tag = @this.Tag,
          BorderStyle = ((Label)@this).BorderStyle,
          CausesValidation = @this.CausesValidation,
          ClientSize = @this.ClientSize,
          ContextMenu = @this.ContextMenu,
          Cursor = @this.Cursor,
          Enabled = @this.Enabled,
          Visible = @this.Visible,
          Dock = @this.Dock,
          FlatStyle = ((Label)@this).FlatStyle,
          Font = @this.Font,
          ForeColor = @this.ForeColor,
          ContextMenuStrip = @this.ContextMenuStrip,
          Location = @this.Location,
          Size = @this.Size,
          Image = ((Label)@this).Image,
          ImageAlign = ((Label)@this).ImageAlign,
          ImageKey = ((Label)@this).ImageKey,
          ImageIndex = ((Label)@this).ImageIndex,
          ImageList = ((Label)@this).ImageList,
          TextAlign = ((Label)@this).TextAlign,
          Padding = @this.Padding,
          Margin = @this.Margin,
          UseWaitCursor = @this.UseWaitCursor,
          UseMnemonic = ((Label)@this).UseMnemonic,
          Name = newName,
          RightToLeft = @this.RightToLeft,
          MinimumSize = @this.MinimumSize,
          MaximumSize = @this.MaximumSize,
        };
        return label;
      }

      newName = new Guid().ToString();
      if (@this is Button) {
        return new Button {
          AllowDrop = @this.AllowDrop,
          Anchor = @this.Anchor,
          AutoEllipsis = ((Button)@this).AutoEllipsis,
          AutoSize = @this.AutoSize,
          BackColor = @this.BackColor,
          BackgroundImage = @this.BackgroundImage,
          AutoScrollOffset = @this.AutoScrollOffset,
          BackgroundImageLayout = @this.BackgroundImageLayout,
          Bounds = @this.Bounds,
          Capture = @this.Capture,
          Text = @this.Text,
          Tag = @this.Tag,
          CausesValidation = @this.CausesValidation,
          ClientSize = @this.ClientSize,
          ContextMenu = @this.ContextMenu,
          Cursor = @this.Cursor,
          Enabled = @this.Enabled,
          Visible = @this.Visible,
          Dock = @this.Dock,
          FlatStyle = ((Button)@this).FlatStyle,
          Font = @this.Font,
          ForeColor = @this.ForeColor,
          ContextMenuStrip = @this.ContextMenuStrip,
          Location = @this.Location,
          Size = @this.Size,
          Image = ((Button)@this).Image,
          ImageAlign = ((Button)@this).ImageAlign,
          ImageKey = ((Button)@this).ImageKey,
          ImageIndex = ((Button)@this).ImageIndex,
          ImageList = ((Button)@this).ImageList,
          TextAlign = ((Button)@this).TextAlign,
          Padding = @this.Padding,
          Margin = @this.Margin,
          UseWaitCursor = @this.UseWaitCursor,
          UseMnemonic = ((Button)@this).UseMnemonic,
          Name = newName,
          RightToLeft = @this.RightToLeft,
          MinimumSize = @this.MinimumSize,
          MaximumSize = @this.MaximumSize,
          TextImageRelation = ((Button)@this).TextImageRelation,
          UseVisualStyleBackColor = ((Button)@this).UseVisualStyleBackColor,
          UseCompatibleTextRendering = ((Button)@this).UseCompatibleTextRendering,
          AutoSizeMode = ((Button)@this).AutoSizeMode,
          DialogResult = ((Button)@this).DialogResult,
        };
      }
      if (@this is CheckBox) {
        return new CheckBox {
          AllowDrop = @this.AllowDrop,
          Anchor = @this.Anchor,
          AutoEllipsis = ((CheckBox)@this).AutoEllipsis,
          AutoSize = @this.AutoSize,
          BackColor = @this.BackColor,
          BackgroundImage = @this.BackgroundImage,
          AutoScrollOffset = @this.AutoScrollOffset,
          BackgroundImageLayout = @this.BackgroundImageLayout,
          Bounds = @this.Bounds,
          Capture = @this.Capture,
          Text = @this.Text,
          Tag = @this.Tag,
          CausesValidation = @this.CausesValidation,
          ClientSize = @this.ClientSize,
          ContextMenu = @this.ContextMenu,
          Cursor = @this.Cursor,
          Enabled = @this.Enabled,
          Visible = @this.Visible,
          Dock = @this.Dock,
          FlatStyle = ((CheckBox)@this).FlatStyle,
          Font = @this.Font,
          ForeColor = @this.ForeColor,
          ContextMenuStrip = @this.ContextMenuStrip,
          Location = @this.Location,
          Size = @this.Size,
          Image = ((CheckBox)@this).Image,
          ImageAlign = ((CheckBox)@this).ImageAlign,
          ImageKey = ((CheckBox)@this).ImageKey,
          ImageIndex = ((CheckBox)@this).ImageIndex,
          ImageList = ((CheckBox)@this).ImageList,
          TextAlign = ((CheckBox)@this).TextAlign,
          Padding = @this.Padding,
          Margin = @this.Margin,
          UseWaitCursor = @this.UseWaitCursor,
          UseMnemonic = ((CheckBox)@this).UseMnemonic,
          Name = newName,
          RightToLeft = @this.RightToLeft,
          MinimumSize = @this.MinimumSize,
          MaximumSize = @this.MaximumSize,
          CheckAlign = ((CheckBox)@this).CheckAlign,
          CheckState = ((CheckBox)@this).CheckState,
          Checked = ((CheckBox)@this).Checked,
          AutoCheck = ((CheckBox)@this).AutoCheck,
          ThreeState = ((CheckBox)@this).ThreeState,
          TextImageRelation = ((CheckBox)@this).TextImageRelation,
          Appearance = ((CheckBox)@this).Appearance,
        };

      }
      if (@this is NumericUpDown) {
        return new NumericUpDown {
          AllowDrop = @this.AllowDrop,
          Anchor = @this.Anchor,
          AutoSize = @this.AutoSize,
          BackColor = @this.BackColor,
          BackgroundImage = @this.BackgroundImage,
          AutoScrollOffset = @this.AutoScrollOffset,
          BackgroundImageLayout = @this.BackgroundImageLayout,
          Bounds = @this.Bounds,
          Capture = @this.Capture,
          Text = @this.Text,
          Tag = @this.Tag,
          CausesValidation = @this.CausesValidation,
          ClientSize = @this.ClientSize,
          ContextMenu = @this.ContextMenu,
          Cursor = @this.Cursor,
          Enabled = @this.Enabled,
          Visible = @this.Visible,
          Dock = @this.Dock,
          Font = @this.Font,
          ForeColor = @this.ForeColor,
          ContextMenuStrip = @this.ContextMenuStrip,
          Location = @this.Location,
          Size = @this.Size,
          TextAlign = ((NumericUpDown)@this).TextAlign,
          Padding = @this.Padding,
          Margin = @this.Margin,
          UseWaitCursor = @this.UseWaitCursor,
          Name = newName,
          RightToLeft = @this.RightToLeft,
          MinimumSize = @this.MinimumSize,
          MaximumSize = @this.MaximumSize,
          Minimum = ((NumericUpDown)@this).Minimum,
          Maximum = ((NumericUpDown)@this).Maximum,
          Value = ((NumericUpDown)@this).Value,
          UpDownAlign = ((NumericUpDown)@this).UpDownAlign,
          ThousandsSeparator = ((NumericUpDown)@this).ThousandsSeparator,
          ReadOnly = ((NumericUpDown)@this).ReadOnly,
          InterceptArrowKeys = ((NumericUpDown)@this).InterceptArrowKeys,
          Increment = ((NumericUpDown)@this).Increment,
          Hexadecimal = ((NumericUpDown)@this).Hexadecimal,
          DecimalPlaces = ((NumericUpDown)@this).DecimalPlaces,
          BorderStyle = ((NumericUpDown)@this).BorderStyle,
          AutoValidate = ((NumericUpDown)@this).AutoValidate,
          AutoScroll = ((NumericUpDown)@this).AutoScroll,
          AutoScrollMargin = ((NumericUpDown)@this).AutoScrollMargin,
          AutoScrollMinSize = ((NumericUpDown)@this).AutoScrollMinSize,
          AutoScrollPosition = ((NumericUpDown)@this).AutoScrollPosition
        };

      }

      if (@this is ComboBox) {
        var comboBox = new ComboBox {
          AllowDrop = @this.AllowDrop,
          Anchor = @this.Anchor,
          AutoSize = @this.AutoSize,
          BackColor = @this.BackColor,
          BackgroundImage = @this.BackgroundImage,
          AutoScrollOffset = @this.AutoScrollOffset,
          BackgroundImageLayout = @this.BackgroundImageLayout,
          Bounds = @this.Bounds,
          Capture = @this.Capture,
          Text = @this.Text,
          Tag = @this.Tag,
          CausesValidation = @this.CausesValidation,
          ClientSize = @this.ClientSize,
          ContextMenu = @this.ContextMenu,
          Cursor = @this.Cursor,
          Enabled = @this.Enabled,
          Visible = @this.Visible,
          Dock = @this.Dock,
          FlatStyle = ((ComboBox)@this).FlatStyle,
          Font = @this.Font,
          ForeColor = @this.ForeColor,
          ContextMenuStrip = @this.ContextMenuStrip,
          Location = @this.Location,
          Size = @this.Size,
          Padding = @this.Padding,
          Margin = @this.Margin,
          UseWaitCursor = @this.UseWaitCursor,
          Name = newName,
          RightToLeft = @this.RightToLeft,
          MinimumSize = @this.MinimumSize,
          MaximumSize = @this.MaximumSize,
          DisplayMember = ((ComboBox)@this).DisplayMember,
          ValueMember = ((ComboBox)@this).ValueMember,
          SelectedText = ((ComboBox)@this).SelectedText,
          MaxLength = ((ComboBox)@this).MaxLength,
          MaxDropDownItems = ((ComboBox)@this).MaxDropDownItems,
          DropDownHeight = ((ComboBox)@this).DropDownHeight,
          DropDownStyle = ((ComboBox)@this).DropDownStyle,
          DropDownWidth = ((ComboBox)@this).DropDownWidth,
          DroppedDown = ((ComboBox)@this).DroppedDown,
        };
        if (((ComboBox)@this).DataSource == null) {
          var items = ((ComboBox)@this).Items;
          if (items.Count > 0) {
            comboBox.Items.AddRange(items.Cast<object>().ToArray());
            comboBox.SelectedItem = ((ComboBox)@this).SelectedItem;
            comboBox.SelectedIndex = ((ComboBox)@this).SelectedIndex;
          }
        } else {
          comboBox.DataSource = ((ComboBox)@this).DataSource;
          comboBox.SelectedValue = ((ComboBox)@this).SelectedValue;
        }
        return comboBox;
      }
      throw new NotSupportedException();

    }
  }
}