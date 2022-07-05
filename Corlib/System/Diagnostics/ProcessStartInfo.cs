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

using System.IO;
using System.Text;
using System.Threading;

namespace System.Diagnostics
{
    internal static partial class ProcessStartInfoExtensions
    {
        #region nested types

        public delegate void ConsoleOutputHandler(ICurrentConsoleOutput output);

        public interface ICurrentConsoleOutput
        {
            string CurrentLine { get; }
            string TotalText { get; }
        }

        public interface IConsoleResult
        {
            string StandardOutput { get; }
            string StandardError { get; }
            int ExitCode { get; }
        }

        private class RedirectedRunAsyncResult : IAsyncResult, IDisposable
        {
            #region nested types

            private class CurrentConsoleOutput : ICurrentConsoleOutput
            {
                private readonly StringBuilder _totalText;

                public CurrentConsoleOutput(string currentLine, StringBuilder totalText)
                {
                    this.CurrentLine = currentLine;
                    this._totalText = totalText;
                }

                #region Implementation of ICurrentConsoleOutput
                public string CurrentLine { get; }

                public string TotalText => this._totalText.ToString();

                #endregion
            }

            private class ConsoleResult : IConsoleResult
            {
                private readonly StringBuilder _stderr;
                private readonly StringBuilder _stdout;

                public ConsoleResult(int exitCode, StringBuilder stdout, StringBuilder stderr)
                {
                    this.ExitCode = exitCode;
                    this._stdout = stdout;
                    this._stderr = stderr;
                }

                #region Implementation of IConsoleResult

                public string StandardOutput => this._stdout.ToString();
                public string StandardError => this._stderr.ToString();
                public int ExitCode { get; }

                #endregion
            }

            #endregion

            private readonly ManualResetEvent _isExited = new ManualResetEvent(false);
            private readonly Process _process;
            private readonly StringBuilder _stderr = new StringBuilder();
            private readonly StringBuilder _stdout = new StringBuilder();
            private readonly ConsoleOutputHandler _stdoutCallback;
            private readonly ConsoleOutputHandler _stderrCallback;
            private readonly AsyncCallback _callback;
            public IConsoleResult Result { get; private set; }

            public RedirectedRunAsyncResult(ProcessStartInfo info, ConsoleOutputHandler stdoutCallback, ConsoleOutputHandler stderrCallback, AsyncCallback callback, object state)
            {
                this._stdoutCallback = stdoutCallback;
                this._stderrCallback = stderrCallback;
                this._callback = callback;
                this.AsyncState = state;

                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                var process = this._process = new Process { StartInfo = info, EnableRaisingEvents = true };
                process.OutputDataReceived += this._process_OutputDataReceived;
                process.ErrorDataReceived += this._process_ErrorDataReceived;
                process.Exited += this._process_Exited;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            private void _process_Exited(object _, EventArgs __)
            {
                this.Result = new ConsoleResult(this._process.ExitCode, this._stdout, this._stderr);
                this._isExited.Set();
                this._callback?.Invoke(this);
            }

            private void _process_ErrorDataReceived(object _, DataReceivedEventArgs e)
            {
                var line = e.Data;

                if (line == null)
                    return;

                this._stderrCallback?.Invoke(new CurrentConsoleOutput(line, this._stderr));
                this._stderr.AppendLine(line);
            }

            private void _process_OutputDataReceived(object _, DataReceivedEventArgs e)
            {
                var line = e.Data;

                if (line == null)
                    return;

                this._stdoutCallback?.Invoke(new CurrentConsoleOutput(line, this._stdout));
                this._stdout.AppendLine(line);
            }

            #region Implementation of IAsyncResult

            public bool IsCompleted => this.AsyncWaitHandle.WaitOne(0);
            public WaitHandle AsyncWaitHandle => this._isExited;
            public object AsyncState { get; }
            public bool CompletedSynchronously => false;

            #endregion

            #region Implementation of IDisposable

            public void Dispose() => this._process.Dispose();

            #endregion
        }

        #endregion

        public static IAsyncResult BeginRedirectedRun(this ProcessStartInfo @this, ConsoleOutputHandler stdoutCallback = null, ConsoleOutputHandler stderrCallback = null, AsyncCallback callback = null, object state = null)
          => new RedirectedRunAsyncResult(@this, stdoutCallback, stderrCallback, callback, state)
          ;

        public static IConsoleResult EndRedirectedRun(this ProcessStartInfo @this, IAsyncResult asyncResult)
        {
            Debug.Assert(asyncResult is RedirectedRunAsyncResult);

            var value = (RedirectedRunAsyncResult)asyncResult;
            value.AsyncWaitHandle.WaitOne();
            var result = value.Result;
            value.Dispose();
            return result;
        }

        public static FileInfo File(this ProcessStartInfo @this)=>string.IsNullOrWhiteSpace(@this.FileName)?null: new FileInfo(@this.FileName);

    }
}