#region (c)2010-2042 Hawkynt
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
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace System.Windows.Forms {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif
  static partial class ControlExtensions {

    #region nested types


    [CompilerGenerated]
    // ReSharper disable once InconsistentNaming
    private sealed class __ActionWithDummyParameterWrapper {
#pragma warning disable CC0074 // Make field readonly
      public Action method;
#pragma warning restore CC0074 // Make field readonly

#pragma warning disable CC0057 // Unused parameters
      public void Invoke(object _) => this.method();
#pragma warning restore CC0057 // Unused parameters

    }

    [CompilerGenerated]
    // ReSharper disable once InconsistentNaming
    private sealed class __ControlActionWithDummyParameterWrapper<TControl> where TControl : Control {
#pragma warning disable CC0074 // Make field readonly
      public Action<TControl> method;
      public TControl control;
#pragma warning restore CC0074 // Make field readonly

#pragma warning disable CC0057 // Unused parameters
      public void Invoke(object _) => this.method(this.control);
#pragma warning restore CC0057 // Unused parameters

    }

    [CompilerGenerated]
    // ReSharper disable once InconsistentNaming
    private sealed class __FunctionWithDummyParameterWrapper<TResult> {
#pragma warning disable CC0074 // Make field readonly
      public Func<TResult> function;
#pragma warning restore CC0074 // Make field readonly

#pragma warning disable CC0057 // Unused parameters
      public TResult Invoke(object _) => this.function();
#pragma warning restore CC0057 // Unused parameters

    }

    [CompilerGenerated]
    // ReSharper disable once InconsistentNaming
    private sealed class __ReturnValueWithDummyParameterWrapper<TControl, TResult> where TControl : Control {
#pragma warning disable CC0074 // Make field readonly
      public TControl control;
      public Func<TControl, TResult> function;
      public TResult result;
#pragma warning restore CC0074 // Make field readonly

#pragma warning disable CC0057 // Unused parameters
      public void Invoke(object _) => this.result = this.function(this.control);
#pragma warning restore CC0057 // Unused parameters

    }

    [CompilerGenerated]
    // ReSharper disable once InconsistentNaming
    private sealed class __HandleCallback<TControl> where TControl : Control {
#pragma warning disable CC0074 // Make field readonly
      public Action<TControl> method;
      public ManualResetEventSlim resetEvent;
#pragma warning restore CC0074 // Make field readonly

      public void Invoke(object sender, EventArgs _) {
        var control = (TControl)sender;
        control.HandleCreated -= this.Invoke;
        try {
          this.method(control);
        } finally {
          this.resetEvent?.Set();
        }
      }
    }

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

    public interface ISuspendedRedrawToken : IDisposable { }

    private class SuspendedRedrawToken : ISuspendedRedrawToken {
      private readonly IntPtr _targetControl;
      [DllImport("user32.dll")]
      private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
      private const int WM_SETREDRAW = 11;

      public SuspendedRedrawToken(Control targetControl) {
        SendMessage(this._targetControl = targetControl.Handle, WM_SETREDRAW, false, 0);
      }

      ~SuspendedRedrawToken() {
        this._Dispose(false);
      }

      private void _Dispose(bool isManagedDisposal) {
        SendMessage(this._targetControl, WM_SETREDRAW, true, 0);
        if (isManagedDisposal)
          GC.SuppressFinalize(this);
      }

      public void Dispose() => this._Dispose(true);
    }

    #endregion

    /// <summary>
    /// Stops the layout and returns a token which will continue layoutint on disposal.
    /// </summary>
    /// <param name="this">The this.</param>
    /// <returns></returns>
    public static ISuspendedLayoutToken PauseLayout(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return new SuspendedLayoutToken(@this);
    }

    public static ISuspendedRedrawToken PauseRedraw(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return new SuspendedRedrawToken(@this);
    }

    /// <summary>
    /// Executes the given action with the current control after a period of time.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="dueTime">The time to wait until execution.</param>
    /// <param name="action">The action.</param>
    public static void SetTimeout<TControl>(this TControl @this, TimeSpan dueTime, Action<TControl> action) where TControl : Control {
      if (@this == null)
        throw new NullReferenceException();
      if (action == null)
        throw new ArgumentNullException(nameof(action));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      Async(@this, t => {
        Thread.Sleep(dueTime);
        SafelyInvoke(t, action);
      });
    }

    /// <summary>
    /// Gets the position of a given control relative to its form.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns>The position of it relative to it's form.</returns>
    public static Drawing.Point GetLocationOnForm(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var result = @this.Location;
      var c = @this;
      for (; !(c is Form); c = c.Parent)
        result.Offset(c.Location);
      return result;
    }

    /// <summary>
    /// Gets the position of a given control relative to the screen.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns>The position of it relative to it's screen.</returns>
    public static Drawing.Point GetLocationOnScreen(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return @this.PointToScreen(Drawing.Point.Empty);
    }

    public static Drawing.Point GetLocationOnClient(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

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
    public static bool SafelyInvoke(this Control @this, Action task, bool async = true) {
      if (@this == null)
        throw new NullReferenceException();
      if (task == null)
        throw new ArgumentNullException(nameof(task));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return SafelyInvoke(@this, new __ActionWithDummyParameterWrapper { method = task }.Invoke, async);
    }


    /// <summary>
    /// Safelies executes an action and returns the result..
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="function">The function.</param>
    /// <returns>Whatever the method returned.</returns>
    public static TResult SafelyInvoke<TResult>(this Control @this, Func<TResult> function) {
      if (@this == null)
        throw new NullReferenceException();
      if (function == null)
        throw new ArgumentNullException(nameof(function));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      return SafelyInvoke(@this, new __FunctionWithDummyParameterWrapper<TResult> { function = function }.Invoke);
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
      if (@this == null)
        throw new NullReferenceException();
      if (function == null)
        throw new ArgumentNullException(nameof(function));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.IsDisposed)
        throw new ObjectDisposedException(nameof(@this));

      if (@this.IsHandleCreated)
        return @this.InvokeRequired ? (TResult)@this.Invoke(function, @this) : function(@this);

      var context = SynchronizationContext.Current;
      if (context != null) {
        var wrapper = new __ReturnValueWithDummyParameterWrapper<TControl, TResult> {
          control = @this,
          function = function
        };
        context.Send(wrapper.Invoke, null);
        return wrapper.result;
      }

      if (Application.MessageLoop)
        return function(@this);

      throw new InvalidOperationException("Handle not yet created");
    }

    /// <summary>
    /// Safely invokes the given code on the control's thread.
    /// </summary>
    /// <typeparam name="TControl">The type of the control.</typeparam>
    /// <param name="this">This Control.</param>
    /// <param name="task">The task to perform in its thread.</param>
    /// <param name="async">if set to <c>true</c>, do not wait when passed to the gui thread.</param>
    /// <returns>
    ///   <c>true</c> when no thread switch was needed; otherwise, <c>false</c>.
    /// </returns>
    public static bool SafelyInvoke<TControl>(this TControl @this, Action<TControl> task, bool async = true) where TControl : Control {
      if (@this == null)
        throw new NullReferenceException();
      if (task == null)
        throw new ArgumentNullException(nameof(task));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.IsDisposed)
        throw new ObjectDisposedException(nameof(@this));

      if (@this.IsHandleCreated)
        if (@this.InvokeRequired) {
          if (async) {
            @this.BeginInvoke(task, @this);
          } else
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

      if (async) {
        @this.HandleCreated += new __HandleCallback<TControl> { method = task }.Invoke;
      } else {
        using (var eventWaiter = new ManualResetEventSlim(false)) {
          @this.HandleCreated += new __HandleCallback<TControl> { method = task, resetEvent = eventWaiter }.Invoke;
          eventWaiter.Wait();
        }
      }

      return false;
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
      if (@this == null)
        throw new NullReferenceException();
      if (task == null)
        throw new ArgumentNullException(nameof(task));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.InvokeRequired) {
        task();
        return false;
      }

      task.BeginInvoke(task.EndInvoke, null);
      return true;
    }

    /// <summary>
    /// Executes a task in a thread that is definitely not the GUI thread.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="task">The task.</param>
    /// <returns><c>true</c> when a thread switch was needed; otherwise, <c>false</c>.</returns>
#pragma warning disable CC0072 // Remove Async termination when method is not asynchronous.
    public static bool Async<TControl>(this TControl @this, Action<TControl> task) where TControl : Control {
#pragma warning restore CC0072 // Remove Async termination when method is not asynchronous.
      if (@this == null)
        throw new NullReferenceException();
      if (task == null)
        throw new ArgumentNullException(nameof(task));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      if (@this.InvokeRequired) {
        task(@this);
        return false;
      }

      task.BeginInvoke(@this, task.EndInvoke, null);
      return true;
    }

    /// <summary>
    /// Runs something in the gui thread before executing a task in a different thread.
    /// </summary>
    /// <typeparam name="TControl">The type of the type.</typeparam>
    /// <param name="this">The this.</param>
    /// <param name="syncPreAction">The sync pre action.</param>
    /// <param name="task">The task.</param>
    /// <param name="syncPostAction">The sync post action.</param>
#pragma warning disable CC0072 // Remove Async termination when method is not asynchronous.
    public static void Async<TControl>(this TControl @this, Action<TControl> syncPreAction, Action task, Action<TControl> syncPostAction = null) where TControl : Control {
#pragma warning restore CC0072 // Remove Async termination when method is not asynchronous.
      if (@this == null)
        throw new NullReferenceException();
      if (syncPreAction == null)
        throw new ArgumentNullException(nameof(syncPreAction));
      if (task == null)
        throw new ArgumentNullException(nameof(task));
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      SafelyInvoke(@this, syncPreAction, false);
      Async(@this, () => {
        try {
          task();
        } finally {
          if (syncPostAction != null)
            SafelyInvoke(@this, syncPostAction, false);
        }
      });
    }

    /// <summary>
    /// Gets the trimmed text property, and converts empty values automatically to <c>null</c>.
    /// </summary>
    /// <param name="this">This control.</param>
    /// <returns>A string with text or <c>null</c>.</returns>
    public static string GetTextProperty(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

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
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

      var children = @this.Controls;
      if (predicate == null) {
        for (var i = children.Count - 1; i >= 0; --i) {
          var child = children[i];
          children.RemoveAt(i);
          child.Dispose();
        }
      } else {
        for (var i = children.Count - 1; i >= 0; --i) {
          var child = children[i];
#pragma warning disable CC0031 // Check for null before calling a delegate
          if (predicate(child)) {
#pragma warning restore CC0031 // Check for null before calling a delegate
            children.RemoveAt(i);
            child.Dispose();
          }
        }
      }
    }

    /// <summary>
    /// Returns all child controls, including their child controls.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <returns>An enumeration of child controls.</returns>
    public static IEnumerable<Control> AllControls(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

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
    /// enables double buffering on the given control.
    /// </summary>
    /// <param name="this">The control</param>
    public static void EnableDoubleBuffering(this Control @this) {
      if (@this == null)
        throw new NullReferenceException();

      if (SystemInformation.TerminalServerSession)
        return;

      var controlType = @this.GetType();
      var pi = controlType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
      if (pi == null)
        return;

      pi.SetValue(@this, true, null);
    }

    /// <summary>
    /// Tries to duplicate the given control.
    /// </summary>
    /// <param name="this">This Control.</param>
    /// <param name="newName">The name for the new control; defaults to auto-name.</param>
    /// <returns>A new control with the same properties as the given one.</returns>
    public static Control Duplicate(this Control @this, string newName = null) {
      if (@this == null)
        throw new NullReferenceException();
#if SUPPORTS_CONTRACTS
      Contract.EndContractBlock();
#endif

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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = @this.ContextMenu,
#endif
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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = @this.ContextMenu,
#endif
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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = @this.ContextMenu,
#endif
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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = @this.ContextMenu,
#endif
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
#if !NET5_0_OR_GREATER && !NETSTANDARD && !NETCOREAPP
          ContextMenu = @this.ContextMenu,
#endif
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

    private static void _AddBinding<TControl, TSource>(this TControl @this, object bindingSource, Expression<Func<TControl, TSource, bool>> expression, Type controlType, Type sourceType, DataSourceUpdateMode mode, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null)
      where TControl : Control {

      string _GetPropertyName(MemberExpression member) {
        if (
          member.Expression is ParameterExpression parameter
          && parameter.Type == sourceType
        )
          return member.Member.Name;

        if (!(member.Expression is MemberExpression parent))
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

      string _GetBindingSourcePropertyName(Expression e) {
        switch (e) {
          case MemberExpression member: // controlProperty = bs.PropertyName
            return _GetPropertyName(member);

          case UnaryExpression convert // controlProperty = (cast)bs.PropertyName
            when convert.NodeType == ExpressionType.Convert
                 && convert.Operand is MemberExpression member
            :
            return _GetPropertyName(member);

          case MethodCallExpression call // controlProperty = bs.PropertyName.ToString()
            when call.Method.Name == nameof(ToString)
                 && call.Arguments.Count == 0
                 && call.Object is MemberExpression member
            :
            return _GetPropertyName(member);
        }

        return null;
      }

      string _GetControlPropertyName(Expression e) {
        switch (e) {
          case MemberExpression member
            when member.Expression is ParameterExpression parameter
                 && parameter.Type == controlType
            :
            return member.Member.Name;

          case UnaryExpression convert
            when convert.NodeType == ExpressionType.Convert
                 && convert.Operand is MemberExpression member
                 && member.Expression is ParameterExpression parameter
                 && parameter.Type == controlType
            :
            return member.Member.Name;

          default:
            return null;
        }
      }

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

      if (!(expression.Body is BinaryExpression body) || body.NodeType != ExpressionType.Equal)
        throw new ArgumentException(excMessage);

      var propertyName = _GetControlPropertyName(body.Left) ?? throw new ArgumentException(excMessage);
      var dataMember = _GetBindingSourcePropertyName(body.Right) ?? throw new ArgumentException(excMessage);

      var binding = new Binding(propertyName, bindingSource, dataMember, true) { DataSourceUpdateMode = mode };
      if (customConversionHandler != null)
        binding.Format += customConversionHandler;

      if (bindingCompleteCallback != null)
        binding.BindingComplete += bindingCompleteCallback;

      @this.DataBindings.Add(binding);
    }

    public static void AddBinding<TControl, TSource>(this TControl @this, TSource source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
      => _AddBinding(@this, source, expression, @this.GetType(), source.GetType(), mode, customConversionHandler, bindingCompleteCallback)
    ;

    public static void AddBinding<TControl, TSource>(this TControl @this, object source, Expression<Func<TControl, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) where TControl : Control
      => _AddBinding(@this, source, expression, typeof(TControl), typeof(TSource), mode, customConversionHandler, bindingCompleteCallback)
    ;

    #region to make life easier

    public static void AddBinding<TSource>(this Label @this, object source, Expression<Func<Label, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<Label, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this CheckBox @this, object source, Expression<Func<CheckBox, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<CheckBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this TextBox @this, object source, Expression<Func<TextBox, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<TextBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this NumericUpDown @this, object source, Expression<Func<NumericUpDown, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<NumericUpDown, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this RadioButton @this, object source, Expression<Func<RadioButton, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<RadioButton, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this Button @this, object source, Expression<Func<Button, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<Button, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this GroupBox @this, object source, Expression<Func<GroupBox, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<GroupBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this ComboBox @this, object source, Expression<Func<ComboBox, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<ComboBox, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);
    public static void AddBinding<TSource>(this DateTimePicker @this, object source, Expression<Func<DateTimePicker, TSource, bool>> expression, DataSourceUpdateMode mode = DataSourceUpdateMode.OnPropertyChanged, ConvertEventHandler customConversionHandler = null, BindingCompleteEventHandler bindingCompleteCallback = null) => AddBinding<DateTimePicker, TSource>(@this, source, expression, mode, customConversionHandler, bindingCompleteCallback);

    #endregion


  }
}