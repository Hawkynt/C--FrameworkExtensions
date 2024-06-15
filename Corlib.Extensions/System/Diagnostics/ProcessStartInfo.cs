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

namespace System.Diagnostics;

public static partial class ProcessStartInfoExtensions {
  #region nested types

  public delegate void ConsoleOutputHandler(ICurrentConsoleOutput output);

  public interface ICurrentConsoleOutput {
    string CurrentLine { get; }
    string TotalText { get; }
  }

  public interface IConsoleResult {
    string StandardOutput { get; }
    string StandardError { get; }
    int ExitCode { get; }
  }

  public interface IRedirectedRunAsyncResult : IAsyncResult, IDisposable {
    public IConsoleResult Result { get; }
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
    private readonly ConsoleOutputHandler _stdoutCallback;
    private readonly ConsoleOutputHandler _stderrCallback;
    private readonly AsyncCallback _callback;
    public IConsoleResult Result { get; private set; }
    public Process Process { get; }

    public RedirectedRunAsyncResult(ProcessStartInfo info, ConsoleOutputHandler stdoutCallback, ConsoleOutputHandler stderrCallback, AsyncCallback callback, object state) {
      this._stdoutCallback = stdoutCallback;
      this._stderrCallback = stderrCallback;
      this._callback = callback;
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
        this._callback?.Invoke(this);
      }

      void ErrorDataReceived(object _, DataReceivedEventArgs e) {
        var line = e.Data;

        if (line == null)
          return;

        this._stderrCallback?.Invoke(new CurrentConsoleOutput(line, this._stderr));
        this._stderr.AppendLine(line);
      }

      void OutputDataReceived(object _, DataReceivedEventArgs e) {
        var line = e.Data;

        if (line == null)
          return;

        this._stdoutCallback?.Invoke(new CurrentConsoleOutput(line, this._stdout));
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

  public static IRedirectedRunAsyncResult BeginRedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null, AsyncCallback callback = null, object state = null)
    => new RedirectedRunAsyncResult(@this, stdoutCallback, stderrCallback, callback, state);

  public static IConsoleResult EndRedirectedRun(this ProcessStartInfo @this, IAsyncResult asyncResult) {
    if (asyncResult is not RedirectedRunAsyncResult redirectedRunAsyncResult)
      throw new InvalidCastException();

    redirectedRunAsyncResult.AsyncWaitHandle.WaitOne();
    var result = redirectedRunAsyncResult.Result;
    redirectedRunAsyncResult.Dispose();
    return result;
  }

  public static FileInfo File(this ProcessStartInfo @this)
    => @this.FileName.IsNullOrWhiteSpace() ? null : new FileInfo(@this.FileName);
}
