#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Guard;

namespace System.Windows.Forms;

public static partial class ControlExtensions {

  /// <summary>
  /// Suspends the layout logic for the <see cref="Control"/> and returns a token that resumes layout when disposed.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.Control"/> instance.</param>
  /// <returns>An <see cref="ISuspendedLayoutToken"/> that resumes layout when disposed.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Control control = new Control();
  /// using (control.PauseLayout())
  /// {
  ///     // Perform multiple layout changes here
  ///     control.Size = new Size(100, 100);
  ///     control.Location = new Point(10, 10);
  /// }
  /// // Layout is resumed after the using block
  /// </code>
  /// </example>
  public static ISuspendedLayoutToken PauseLayout(this Control @this) {
    Against.ThisIsNull(@this);

    return new SuspendedLayoutToken(@this);
  }

  /// <summary>
  /// Suspends the redraw logic for the <see cref="Control"/> and returns a token that resumes redraw when disposed.
  /// </summary>
  /// <param name="this">The <see cref="System.Windows.Forms.Control"/> instance.</param>
  /// <returns>An <see cref="ISuspendedRedrawToken"/> that resumes redraw when disposed.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Control control = new Control();
  /// using (control.PauseRedraw())
  /// {
  ///     // Perform multiple changes here without triggering redraws
  ///     control.Size = new Size(100, 100);
  ///     control.Location = new Point(10, 10);
  /// }
  /// // Redraw is resumed after the using block
  /// </code>
  /// </example>
  public static ISuspendedRedrawToken PauseRedraw(this Control @this) {
    Against.ThisIsNull(@this);

    return new SuspendedRedrawToken(@this);
  }

  /// <summary>
  /// Sets a timeout to perform an <see cref="Action"/> on the <see cref="Control"/> after the specified time interval.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="dueTime">The time interval after which the <see cref="Action"/> will be performed.</param>
  /// <param name="action">The action to perform on the <see cref="Control"/>.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.SetTimeout(TimeSpan.FromSeconds(5), btn => btn.Text = "Timeout reached!");
  /// // After 5 seconds, the button's text will change to "Timeout reached!".
  /// </code>
  /// </example>
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
  /// Gets the location of the <see cref="Control"/> relative to the top-left corner of the form.
  /// </summary>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <returns>The location of the <see cref="Control"/> relative to the top-left corner of the form.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// Form form = new Form();
  /// form.Controls.Add(button);
  /// Point location = button.GetLocationOnForm();
  /// Console.WriteLine($"Button location on form: {location}");
  /// // Output: Button location on form: {X,Y} (actual coordinates)
  /// </code>
  /// </example>
  public static Point GetLocationOnForm(this Control @this) {
    Against.ThisIsNull(@this);

    var result = @this.Location;
    for (var control = @this; control is not Form; control = control.Parent)
      result.Offset(control.Location);

    return result;
  }

  /// <summary>
  /// Gets the location of the <see cref="Control"/> relative to the top-left corner of the screen.
  /// </summary>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <returns>The location of the <see cref="Control"/> relative to the top-left corner of the screen.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// Form form = new Form();
  /// form.Controls.Add(button);
  /// Point screenLocation = button.GetLocationOnScreen();
  /// Console.WriteLine($"Button location on screen: {screenLocation}");
  /// // Output: Button location on screen: {X,Y} (actual coordinates)
  /// </code>
  /// </example>
  public static Point GetLocationOnScreen(this Control @this) {
    Against.ThisIsNull(@this);

    return @this.PointToScreen(Point.Empty);
  }

  /// <summary>
  /// Gets the location of the <see cref="Control"/> relative to the top-left corner of its parent control's client area.
  /// </summary>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <returns>The location of the <see cref="Control"/> relative to the top-left corner of its parent control's client area.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// Panel panel = new Panel();
  /// panel.Controls.Add(button);
  /// Point clientLocation = button.GetLocationOnClient();
  /// Console.WriteLine($"Button location on client area: {clientLocation}");
  /// // Output: Button location on client area: {X,Y} (actual coordinates)
  /// </code>
  /// </example>
  public static Point GetLocationOnClient(this Control @this) {
    Against.ThisIsNull(@this);

    var result = Point.Empty;
    for (var control = @this; control.Parent != null; control = control.Parent)
      result.Offset(control.Location);

    return result;
  }

  /// <summary>
  /// Safely invokes the specified <see cref="Action"/> on the control's thread.
  /// </summary>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="task">The action to be invoked.</param>
  /// <param name="async">(Optional: defaults to <see langword="true"/>) If set to <see langword="true"/>, the action is invoked asynchronously; otherwise, it is invoked synchronously.</param>
  /// <returns><see langword="true"/> when no thread switch was needed; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ObjectDisposedException">Thrown if <paramref name="this"/> is already disposed.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.SafelyInvoke(() => button.Text = "Clicked", async: false);
  /// // The button's text is now set to "Clicked".
  /// </code>
  /// </example>
  public static bool SafelyInvoke(this Control @this, Action task, bool async = true) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(task);

    return SafelyInvoke(@this, new __ActionWithDummyParameterWrapper { method = task }.Invoke, async);
  }


  /// <summary>
  /// Safely invokes the specified <see cref="Func{TResult}"/> on the <see cref="Control"/>'s thread and returns the result.
  /// </summary>
  /// <typeparam name="TResult">The type of the result returned by the <see cref="Func{TResult}"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="function">The <see cref="Func{TResult}"/> to be invoked.</param>
  /// <returns>The result of the <see cref="Func{TResult}"/> invocation.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="function"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ObjectDisposedException">Thrown if <paramref name="this"/> is already disposed.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// string result = button.SafelyInvoke(() => button.Text);
  /// Console.WriteLine($"Button text: {result}");
  /// // Output: Button text: [button text]
  /// </code>
  /// </example>
  public static TResult SafelyInvoke<TResult>(this Control @this, Func<TResult> function) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNull(function);

    return SafelyInvoke(@this, new __FunctionWithDummyParameterWrapper<TResult> { function = function }.Invoke);
  }

  /// <summary>
  /// Safely invokes the specified <see cref="Func{TControl, TResult}"/> on the control's thread and returns the result.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <typeparam name="TResult">The type of the result returned by the <see cref="Func{TControl, TResult}"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="function">The <see cref="Func{TControl, TResult}"/> to be invoked, which takes the <see cref="Control"/> as a parameter.</param>
  /// <returns>The result of the <see cref="Func{TControl, TResult}"/> invocation.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="function"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ObjectDisposedException">Thrown if <paramref name="this"/> is already disposed.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// string result = button.SafelyInvoke(btn => btn.Text);
  /// Console.WriteLine($"Button text: {result}");
  /// // Output: Button text: [button text]
  /// </code>
  /// </example>
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
  /// Safely invokes the specified <see cref="Action{TControl}"/> on the <see cref="Control"/>'s thread.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="task">The action to be invoked, which takes the <see cref="Control"/> as a parameter.</param>
  /// <param name="async">(Optional: defaults to <see langword="true"/>) If set to <see langword="true"/>, the action is invoked asynchronously; otherwise, it is invoked synchronously.</param>
  /// <returns><see langword="true"/> when no thread switch was needed; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ObjectDisposedException">Thrown if <paramref name="this"/> is already disposed.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.SafelyInvoke(btn => btn.Text = "Clicked", async: false);
  /// // The button's text is now set to "Clicked".
  /// </code>
  /// </example>
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
  /// Invokes the specified <see cref="Action"/> not on the <see cref="Control"/>'s thread.
  /// </summary>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="task">The <see cref="Action"/> to be invoked.</param>
  /// <returns><see langword="false"/> when no thread switch was needed; otherwise, <see langword="true"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.Async(() => button.Text = "Clicked");
  /// // The button's text will be set to "Clicked" asynchronously.
  /// </code>
  /// </example>
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
  /// Invokes the specified <see cref="Action{TControl}"/> not on the <see cref="Control"/>'s thread.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="task">The <see cref="Action{TControl}"/> to be invoked.</param>
  /// <returns><see langword="false"/> when no thread switch was needed; otherwise, <see langword="true"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.Async(btn => btn.Text = "Clicked");
  /// // The button's text will be set to "Clicked" asynchronously.
  /// </code>
  /// </example>
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
  /// Invokes the specified <see cref="Action"/> not on the <see cref="Control"/>'s thread with the pre- and post-actions executed on the <see cref="Control"/>'s thread.
  /// </summary>
  /// <typeparam name="TControl">The type of the <see cref="Control"/>.</typeparam>
  /// <param name="this">The <see cref="Control"/> instance.</param>
  /// <param name="syncPreAction">The synchronous pre-action to be invoked on the <see cref="Control"/>'s thread before the asynchronous action.</param>
  /// <param name="task">The action to be invoked asynchronously on a different thread.</param>
  /// <param name="syncPostAction">(Optional: defaults to <see langword="null"/>) The synchronous post-action to be invoked on the <see cref="Control"/>'s thread after the asynchronous action.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="syncPreAction"/> or <paramref name="task"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// button.Async(
  ///     syncPreAction: btn => btn.Text = "Starting...",
  ///     task: () => Thread.Sleep(1000), // Simulate async work
  ///     syncPostAction: btn => btn.Text = "Completed"
  /// );
  /// // The button's text will be set to "Starting...", then after 1 second, it will change to "Completed".
  /// </code>
  /// </example>
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
  /// Gets the trimmed text property of the <see cref="Control"/>, and converts empty values automatically to <see langword="null"/>.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <returns>A <see cref="string"/> with text or <see langword="null"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// TextBox textBox = new TextBox();
  /// textBox.Text = "  Sample Text  ";
  /// string result = textBox.GetTextProperty();
  /// Console.WriteLine(result); // Output: "Sample Text"
  /// 
  /// textBox.Text = "  ";
  /// result = textBox.GetTextProperty();
  /// Console.WriteLine(result == null); // Output: True
  /// </code>
  /// </example>
  public static string GetTextProperty(this Control @this) {
    Against.ThisIsNull(@this);

    var text = @this.Text;
    if (text == null)
      return null;

    text = text.Trim();
    return text.Length <= 0 ? null : text;
  }

  /// <summary>
  /// Clears child controls of the <see cref="Control"/> based on an optional predicate, disposing the cleared child controls.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <param name="predicate">(Optional: defaults to <see langword="null"/>) The predicate to determine which child controls should be cleared. If <see langword="null"/>, all child controls are cleared.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Panel panel = new Panel();
  /// panel.Controls.Add(new Button { Text = "Button1" });
  /// panel.Controls.Add(new TextBox { Text = "TextBox1" });
  /// panel.ClearChildren(ctrl => ctrl is Button);
  /// // The panel now only contains the TextBox.
  /// </code>
  /// </example>
  public static void ClearChildren(this Control @this, Predicate<Control> predicate = null) {
    Against.ThisIsNull(@this);

    var children = @this.Controls;
    if (predicate is {} p)
      for (var i = children.Count - 1; i >= 0; --i) {
        var child = children[i];
        if (!p(child))
          continue;

        children.RemoveAt(i);
        child.Dispose();
      }
    else
      for (var i = children.Count - 1; i >= 0; --i) {
        var child = children[i];
        children.RemoveAt(i);
        child.Dispose();
      }
  }

  /// <summary>
  /// Recursively enumerates all child controls of the <see cref="Control"/>.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <returns>An <see cref="IEnumerable{Control}"/> containing all child controls.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Form form = new Form();
  /// Panel panel = new Panel();
  /// Button button = new Button();
  /// panel.Controls.Add(button);
  /// form.Controls.Add(panel);
  /// IEnumerable&lt;Control&gt; allControls = form.AllControls();
  /// foreach (var control in allControls)
  /// {
  ///     Console.WriteLine(control.GetType().Name);
  /// }
  /// // Output will include Panel and Button
  /// </code>
  /// </example>
  public static IEnumerable<Control> AllControls(this Control @this) {
    Against.ThisIsNull(@this);

    foreach (Control child in @this.Controls) {
      yield return child;
      foreach (var subchild in child.AllControls())
        yield return subchild;
    }
  }

  /// <summary>
  /// Determines whether the control is <see langword="null"/> or disposed.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <returns><see langword="true"/> if the control is <see langword="null"/> or disposed; otherwise, <see langword="false"/>.</returns>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// bool isNullOrDisposed = button.IsNullOrDisposed();
  /// Console.WriteLine(isNullOrDisposed); // Output: False
  ///
  /// button.Dispose();
  /// isNullOrDisposed = button.IsNullOrDisposed();
  /// Console.WriteLine(isNullOrDisposed); // Output: True
  /// </code>
  /// </example>
  public static bool IsNullOrDisposed([NotNullWhen(false)] this Control @this) => @this == null || @this.IsDisposed;

  /// <summary>
  /// Enables double buffering on the specified control to reduce flicker.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Panel panel = new Panel();
  /// panel.EnableDoubleBuffering();
  /// // The panel now has double buffering enabled to reduce flicker.
  /// </code>
  /// </example>
  public static void EnableDoubleBuffering(this Control @this) {
    Against.ThisIsNull(@this);

    if (SystemInformation.TerminalServerSession)
      return;

    @this
      .GetType()
      .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
      ?.SetValue(@this, true, null)
      ;

  }

  /// <summary>
  /// Creates a duplicate of the specified control, assigning a new name.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <param name="newName">(Optional: defaults to <see langword="null"/>) The new name to assign to the duplicated control. If <see langword="null"/>, a new GUID is used.</param>
  /// <returns>A new <see cref="Control"/> that is a duplicate of the original control.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <example>
  /// <code>
  /// Button button = new Button { Text = "Click Me", Name = "button1" };
  /// Button duplicateButton = button.Duplicate("button2") as Button;
  /// // The duplicateButton now has the same properties as the original button, but with the name "button2".
  /// </code>
  /// </example>
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
        var result = new ComboBox {
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
            return result;

          result.Items.AddRange(items.Cast<object>().ToArray());
          result.SelectedItem = box.SelectedItem;
          result.SelectedIndex = box.SelectedIndex;
        } else {
          result.DataSource = box.DataSource;
          result.SelectedValue = box.SelectedValue;
        }

        return result;
      }
      default: {
        var controlType = @this.GetType();
        var result = (Control)Activator.CreateInstance(controlType);

        foreach (var property in controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
          if (property.CanWrite && property.Name != nameof(Control.WindowTarget))
            property.SetValue(result, property.GetValue(@this, null), null);

        result.Name = newName;
        return result;
      }

    }
  }

  /// <summary>
  /// Determines whether the control is in design mode.
  /// </summary>
  /// <param name="this">This <see cref="Control"/> instance.</param>
  /// <returns><see langword="true"/> if the control is in design mode; otherwise, <see langword="false"/>.</returns>
  /// <exception cref="System.NullReferenceException">Thrown if <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// Use this because <see cref="System.ComponentModel.Component.DesignMode">Component.DesignMode</see> is only set after <see cref="Form.Load"/> was invoked.
  /// </remarks>
  /// <example>
  /// <code>
  /// Button button = new Button();
  /// bool isInDesignMode = button.IsDesignMode();
  /// Console.WriteLine(isInDesignMode); // Output: True or False depending on the context
  /// </code>
  /// </example>
  public static bool IsDesignMode(this Control @this)
    => LicenseManager.UsageMode == LicenseUsageMode.Designtime
       || IsDesignModeDetector.Instance.GetDesignModePropertyValue(@this);
}
