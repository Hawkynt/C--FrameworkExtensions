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

using System.IO;
using System.Text;
using System.Threading;

#if SUPPORTS_TASK_RUN
using System.Threading.Tasks;
#endif

namespace System.Diagnostics;

/// <summary>
/// Provides extension methods for <see cref="System.Diagnostics.ProcessStartInfo"/>.
/// </summary>
public static partial class ProcessStartInfoExtensions {

  #region nested types

  public delegate void ConsoleOutputHandler(ICurrentConsoleOutput output);

  /// <summary>
  /// Interface representing the current console output.
  /// </summary>
  public interface ICurrentConsoleOutput {

    /// <summary>
    /// Gets the current line of console output.
    /// </summary>
    string CurrentLine { get; }

    /// <summary>
    /// Gets the total text of console output so far.
    /// </summary>
    string TotalText { get; }
  }

  /// <summary>
  /// Interface representing the result of a console execution.
  /// </summary>
  public interface IConsoleResult {

    /// <summary>
    /// Gets the standard output text.
    /// </summary>
    string StandardOutput { get; }

    /// <summary>
    /// Gets the standard error text.
    /// </summary>
    string StandardError { get; }

    /// <summary>
    /// Gets the exit code of the process.
    /// </summary>
    int ExitCode { get; }
  }

  /// <summary>
  /// Interface representing the result of an asynchronous redirected run.
  /// </summary>
  public interface IRedirectedRunAsyncResult : IAsyncResult, IDisposable {

    /// <summary>
    /// Gets the console result of the process execution.
    /// </summary>
    public IConsoleResult Result { get; }

    /// <summary>
    /// Gets the process that is being run.
    /// </summary>
    public Process Process { get; }
  }

  private sealed class RedirectedRunAsyncResult : IRedirectedRunAsyncResult {
    #region nested types

    private sealed class CurrentConsoleOutput(string currentLine, StringBuilder totalText) : ICurrentConsoleOutput {
      #region Implementation of ICurrentConsoleOutput

      public string CurrentLine { get; } = currentLine;

      public string TotalText => totalText.ToString();

      #endregion
    }

    private sealed class ConsoleResult(int exitCode, StringBuilder stdout, StringBuilder stderr) : IConsoleResult {
      #region Implementation of IConsoleResult

      public string StandardOutput => stdout.ToString();
      public string StandardError => stderr.ToString();
      public int ExitCode { get; } = exitCode;

      #endregion
    }

    #endregion

    private readonly ManualResetEvent _isExited = new(false);
    private readonly StringBuilder _stderr = new();
    private readonly StringBuilder _stdout = new();
    public IConsoleResult Result { get; private set; }
    public Process Process { get; }

    public RedirectedRunAsyncResult(ProcessStartInfo info, ConsoleOutputHandler stdoutCallback, ConsoleOutputHandler stderrCallback, AsyncCallback callback, object state) {
      this.AsyncState = state;

      info.UseShellExecute = false;
      info.RedirectStandardOutput = true;
      info.RedirectStandardError = true;
      info.CreateNoWindow = true;
      info.WindowStyle = ProcessWindowStyle.Hidden;

      var process = this.Process = new() { StartInfo = info, EnableRaisingEvents = true };
      process.OutputDataReceived += OutputDataReceived;
      process.ErrorDataReceived += ErrorDataReceived;
      process.Exited += Exited;

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      return;

      void Exited(object _, EventArgs __) {
        this.Result = new ConsoleResult(this.Process.ExitCode, this._stdout, this._stderr);
        this._isExited.Set();
        callback?.Invoke(this);
      }

      void ErrorDataReceived(object _, DataReceivedEventArgs e) {
        var line = e.Data;

        if (line == null)
          return;

        stderrCallback?.Invoke(new CurrentConsoleOutput(line, this._stderr));
        this._stderr.AppendLine(line);
      }

      void OutputDataReceived(object _, DataReceivedEventArgs e) {
        var line = e.Data;

        if (line == null)
          return;

        stdoutCallback?.Invoke(new CurrentConsoleOutput(line, this._stdout));
        this._stdout.AppendLine(line);
      }
    }

    #region Implementation of IAsyncResult

    public bool IsCompleted => this.AsyncWaitHandle.WaitOne(0);
    public WaitHandle AsyncWaitHandle => this._isExited;
    public object AsyncState { get; }
    public bool CompletedSynchronously => false;

    #endregion

    #region Implementation of IDisposable

    public void Dispose() => this.Process.Dispose();

    #endregion
  }

  #endregion

  /// <summary>
  /// Executes a process synchronously with redirected standard output and standard error streams.
  /// </summary>
  /// <param name="this">The <see cref="System.Diagnostics.ProcessStartInfo"/> object containing the information necessary to start the process.</param>
  /// <param name="stdoutCallback">An optional delegate that handles standard output data received from the process. Can be <see langword="null"/>.</param>
  /// <param name="stderrCallback">An optional delegate that handles standard error data received from the process. Can be <see langword="null"/>.</param>
  /// <returns>An <see cref="IConsoleResult"/> containing the result of the process execution, including any output and error data.</returns>
  /// <remarks>
  /// The method automatically adjust properties of the given <see cref="ProcessStartInfo"/>:
  ///   UseShellExecute = false,
  ///   RedirectStandardOutput = true,
  ///   RedirectStandardError = true,
  ///   CreateNoWindow = true,
  ///   WindowStyle = ProcessWindowStyle.Hidden
  /// </remarks>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// ConsoleOutputHandler stdoutHandler = (sender, args) => Console.WriteLine($"Output: {args.Data}");
  /// ConsoleOutputHandler stderrHandler = (sender, args) => Console.WriteLine($"Error: {args.Data}");
  ///
  /// IConsoleResult result = psi.RedirectedRun(stdoutHandler, stderrHandler);
  ///
  /// Console.WriteLine("Process completed with exit code: " + result.ExitCode);
  /// Console.WriteLine("Standard Output: " + result.StandardOutput);
  /// Console.WriteLine("Standard Error: " + result.StandardError);
  /// </code>
  /// This example demonstrates how to execute a process synchronously and handle its output using callbacks.
  /// </example>
  public static IConsoleResult RedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null) {
    var token = BeginRedirectedRun(@this, stdoutCallback, stderrCallback);
    return EndRedirectedRun(@this, token);
  }

  /// <summary>
  /// Begins an asynchronous process execution with redirected standard output and standard error streams.
  /// </summary>
  /// <param name="this">The <see cref="ProcessStartInfo"/> object containing the information necessary to start the process.</param>
  /// <param name="stdoutCallback">An optional delegate that handles standard output data received from the process. Can be <see langword="null"/>.</param>
  /// <param name="stderrCallback">An optional delegate that handles standard error data received from the process. Can be <see langword="null"/>.</param>
  /// <param name="callback">An optional delegate to be called when the asynchronous operation is completed. Can be <see langword="null"/>.</param>
  /// <param name="state">An optional user-defined object that contains information about the asynchronous operation. Can be <see langword="null"/>.</param>
  /// <returns>An <see cref="IRedirectedRunAsyncResult"/> representing the result of the asynchronous operation.</returns>
  /// <remarks>
  /// The method automatically adjust properties of the given <see cref="ProcessStartInfo"/>:
  ///   UseShellExecute = false,
  ///   RedirectStandardOutput = true,
  ///   RedirectStandardError = true,
  ///   CreateNoWindow = true,
  ///   WindowStyle = ProcessWindowStyle.Hidden
  /// </remarks>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// ConsoleOutputHandler stdoutHandler = (sender, args) => Console.WriteLine($"Output: {args.Data}");
  /// ConsoleOutputHandler stderrHandler = (sender, args) => Console.WriteLine($"Error: {args.Data}");
  ///
  /// IRedirectedRunAsyncResult result = psi.BeginRedirectedRun(stdoutHandler, stderrHandler, null, null);
  /// </code>
  /// This example demonstrates how to start a process asynchronously with both standard output and standard error streams redirected, and handlers to process the output.
  /// </example>
  public static IRedirectedRunAsyncResult BeginRedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null, AsyncCallback callback = null, object state = null)
    => new RedirectedRunAsyncResult(@this, stdoutCallback, stderrCallback, callback, state)
  ;

  /// <summary>
  /// Ends an asynchronous process execution that was started with <see cref="BeginRedirectedRun"/> and retrieves the result.
  /// </summary>
  /// <param name="this">The <see cref="ProcessStartInfo"/> object that started the asynchronous process.</param>
  /// <param name="asyncResult">The <see cref="IAsyncResult"/> object representing the status of the asynchronous operation.</param>
  /// <returns>An <see cref="IConsoleResult"/> containing the result of the process execution, including any output and error data.</returns>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// ConsoleOutputHandler stdoutHandler = (sender, args) => Console.WriteLine($"Output: {args.Data}");
  /// ConsoleOutputHandler stderrHandler = (sender, args) => Console.WriteLine($"Error: {args.Data}");
  ///
  /// IAsyncResult asyncResult = psi.BeginRedirectedRun(stdoutHandler, stderrHandler, null, null);
  /// IConsoleResult result = psi.EndRedirectedRun(asyncResult);
  /// 
  /// Console.WriteLine("Process completed with exit code: " + result.ExitCode);
  /// Console.WriteLine("Standard Output: " + result.StandardOutput);
  /// Console.WriteLine("Standard Error: " + result.StandardError);
  /// </code>
  /// This example demonstrates how to end an asynchronous process execution and retrieve its result.
  /// </example>
  public static IConsoleResult EndRedirectedRun(this ProcessStartInfo @this, IAsyncResult asyncResult) {
    if (asyncResult is not RedirectedRunAsyncResult redirectedRunAsyncResult)
      throw new InvalidCastException();

    redirectedRunAsyncResult.AsyncWaitHandle.WaitOne();
    var result = redirectedRunAsyncResult.Result;
    redirectedRunAsyncResult.Dispose();
    return result;
  }

#if SUPPORTS_TASK_RUN

  /// <summary>
  /// Executes a process asynchronously with redirected standard output and standard error streams.
  /// </summary>
  /// <param name="this">The <see cref="ProcessStartInfo"/> object containing the information necessary to start the process.</param>
  /// <param name="stdoutCallback">An optional delegate that handles standard output data received from the process. Can be <see langword="null"/>.</param>
  /// <param name="stderrCallback">An optional delegate that handles standard error data received from the process. Can be <see langword="null"/>.</param>
  /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with an <see cref="IConsoleResult"/> containing the result of the process execution.</returns>
  /// <remarks>
  /// The method automatically adjust properties of the given <see cref="ProcessStartInfo"/>:
  ///   UseShellExecute = false,
  ///   RedirectStandardOutput = true,
  ///   RedirectStandardError = true,
  ///   CreateNoWindow = true,
  ///   WindowStyle = ProcessWindowStyle.Hidden
  /// </remarks>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// ConsoleOutputHandler stdoutHandler = (sender, args) => Console.WriteLine($"Output: {args.Data}");
  /// ConsoleOutputHandler stderrHandler = (sender, args) => Console.WriteLine($"Error: {args.Data}");
  ///
  /// IConsoleResult result = await psi.RedirectedRunAsync(stdoutHandler, stderrHandler);
  ///
  /// Console.WriteLine("Process completed with exit code: " + result.ExitCode);
  /// Console.WriteLine("Standard Output: " + result.StandardOutput);
  /// Console.WriteLine("Standard Error: " + result.StandardError);
  /// </code>
  /// This example demonstrates how to execute a process asynchronously and handle its output using callbacks.
  /// </example>
  public static Task<IConsoleResult> RedirectedRunAsync(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null)
    => RedirectedRunAsync(@this, CancellationToken.None, stdoutCallback, stderrCallback)
    ;

  /// <summary>
  /// Executes a process asynchronously with redirected standard output and standard error streams, with support for cancellation.
  /// </summary>
  /// <param name="this">The <see cref="ProcessStartInfo"/> object containing the information necessary to start the process.</param>
  /// <param name="cancellation">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.</param>
  /// <param name="stdoutCallback">An optional delegate that handles standard output data received from the process. Can be <see langword="null"/>.</param>
  /// <param name="stderrCallback">An optional delegate that handles standard error data received from the process. Can be <see langword="null"/>.</param>
  /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with an <see cref="IConsoleResult"/> containing the result of the process execution.</returns>
  /// <remarks>
  /// The method automatically adjust properties of the given <see cref="ProcessStartInfo"/>:
  ///   UseShellExecute = false,
  ///   RedirectStandardOutput = true,
  ///   RedirectStandardError = true,
  ///   CreateNoWindow = true,
  ///   WindowStyle = ProcessWindowStyle.Hidden
  /// </remarks>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  /// ConsoleOutputHandler stdoutHandler = (sender, args) => Console.WriteLine($"Output: {args.Data}");
  /// ConsoleOutputHandler stderrHandler = (sender, args) => Console.WriteLine($"Error: {args.Data}");
  ///
  /// try
  /// {
  ///    IConsoleResult result = await psi.RedirectedRunAsync(cts.Token, stdoutHandler, stderrHandler);
  ///    Console.WriteLine("Process completed with exit code: " + result.ExitCode);
  ///    Console.WriteLine("Standard Output: " + result.StandardOutput);
  ///    Console.WriteLine("Standard Error: " + result.StandardError);
  /// }
  /// catch (OperationCanceledException)
  /// {
  ///    Console.WriteLine("Process execution was cancelled.");
  /// }
  /// </code>
  /// This example demonstrates how to execute a process asynchronously with cancellation support and handle its output using callbacks.
  /// </example>
  public static Task<IConsoleResult> RedirectedRunAsync(this ProcessStartInfo @this, CancellationToken cancellation, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null) {
    var asyncResult = BeginRedirectedRun(@this, stdoutCallback, stderrCallback);
    const int RUNNING = 0;
    const int ENDED = 1;
    var isEnded = RUNNING;
    cancellation.Register(OnCancellationRequested);
    return Task.Run(WaitTillFinished, cancellation);

    void OnCancellationRequested() {
      try {
        if (Volatile.Read(ref isEnded) == RUNNING && !asyncResult.Process.HasExited)
          asyncResult.Process.Kill();
      } catch {
        ;
      }
    }

    IConsoleResult WaitTillFinished() {
      try {
        return @this.EndRedirectedRun(asyncResult);
      } finally {
        Volatile.Write(ref isEnded, ENDED);
      }
    }
  }

#endif

  /// <summary>
  /// Gets the <see cref="FileInfo"/> object representing the file that the process will execute.
  /// </summary>
  /// <param name="this">The <see cref="System.Diagnostics.ProcessStartInfo"/> object containing the information necessary to start the process.</param>
  /// <returns>A <see cref="FileInfo"/> object representing the file specified in the <see cref="System.Diagnostics.ProcessStartInfo.FileName"/> property.</returns>
  /// <example>
  /// <code>
  /// ProcessStartInfo psi = new ProcessStartInfo
  /// {
  ///     FileName = "example.exe",
  ///     Arguments = "--example-arg"
  /// };
  ///
  /// FileInfo fileInfo = psi.File();
  /// Console.WriteLine("Process will execute file: " + fileInfo.FullName);
  /// </code>
  /// This example demonstrates how to get the <see cref="FileInfo"/> object for the file that the process will execute.
  /// </example>
  public static FileInfo File(this ProcessStartInfo @this)
    => @this.FileName.IsNullOrWhiteSpace() ? null : new FileInfo(@this.FileName);
}
