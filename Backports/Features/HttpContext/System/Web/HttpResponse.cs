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

// HttpResponse is only available in .NET Framework 4.0+ via System.Web.dll
#if !SUPPORTS_HTTPCONTEXT

using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace System.Web;

/// <summary>
/// Encapsulates HTTP response information from an ASP.NET operation.
/// </summary>
public sealed class HttpResponse {

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpResponse"/> class.
  /// </summary>
  public HttpResponse() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpResponse"/> class with the specified <see cref="TextWriter"/>.
  /// </summary>
  /// <param name="writer">A <see cref="TextWriter"/> object.</param>
  public HttpResponse(TextWriter writer) => this.Output = writer ?? throw new ArgumentNullException(nameof(writer));

  /// <summary>
  /// Gets or sets the HTTP status code of the output returned to the client.
  /// </summary>
  public int StatusCode { get; set; } = 200;

  /// <summary>
  /// Gets or sets the HTTP status string of the output returned to the client.
  /// </summary>
  public string StatusDescription { get; set; } = "OK";

  /// <summary>
  /// Gets the collection of response headers.
  /// </summary>
  public NameValueCollection Headers { get; } = new();

  /// <summary>
  /// Gets the response cookie collection.
  /// </summary>
  public HttpCookieCollection Cookies { get; } = new();

  /// <summary>
  /// Gets or sets the HTTP MIME type of the output stream.
  /// </summary>
  public string ContentType { get; set; } = "text/html";

  /// <summary>
  /// Gets or sets the HTTP character set of the output stream.
  /// </summary>
  public Encoding ContentEncoding { get; set; } = Encoding.UTF8;

  /// <summary>
  /// Gets or sets the value of the HTTP Location header.
  /// </summary>
  public string? RedirectLocation {
    get => this.Headers["Location"];
    set {
      if (value != null)
        this.Headers["Location"] = value;
      else
        this.Headers.Remove("Location");
    }
  }

  /// <summary>
  /// Enables binary output to the outgoing HTTP content body.
  /// </summary>
  public Stream OutputStream { get; set; } = Stream.Null;

  /// <summary>
  /// Enables output of text to the outgoing HTTP response stream.
  /// </summary>
  public TextWriter Output { get; set; } = TextWriter.Null;

  /// <summary>
  /// Gets or sets a value indicating whether to buffer output and send it after the complete page is finished processing.
  /// </summary>
  public bool Buffer { get; set; } = true;

  /// <summary>
  /// Gets or sets a value indicating whether to buffer output and send it after the complete response is finished processing.
  /// </summary>
  public bool BufferOutput { get; set; } = true;

  /// <summary>
  /// Gets or sets the Cache-Control HTTP header.
  /// </summary>
  public string? CacheControl {
    get => this.Headers["Cache-Control"];
    set {
      if (value != null)
        this.Headers["Cache-Control"] = value;
      else
        this.Headers.Remove("Cache-Control");
    }
  }

  /// <summary>
  /// Gets or sets the value of the Http Charset header.
  /// </summary>
  public string Charset { get; set; } = "utf-8";

  /// <summary>
  /// Gets or sets a value that specifies whether to suppress the default headers.
  /// </summary>
  public bool SuppressContent { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether to send HTTP content to the client.
  /// </summary>
  public bool IsClientConnected { get; set; } = true;

  /// <summary>
  /// Gets a value indicating whether the response has been written to.
  /// </summary>
  public bool HasStarted { get; private set; }

  /// <summary>
  /// Writes a string to the HTTP response output stream.
  /// </summary>
  /// <param name="s">The string to write to the HTTP output stream.</param>
  public void Write(string s) {
    this.HasStarted = true;
    this.Output.Write(s);
  }

  /// <summary>
  /// Writes the specified object to the HTTP response stream.
  /// </summary>
  /// <param name="obj">The object to write to the HTTP output stream.</param>
  public void Write(object obj) {
    this.HasStarted = true;
    this.Output.Write(obj);
  }

  /// <summary>
  /// Writes a character to the HTTP response output stream.
  /// </summary>
  /// <param name="ch">The character to write to the HTTP output stream.</param>
  public void Write(char ch) {
    this.HasStarted = true;
    this.Output.Write(ch);
  }

  /// <summary>
  /// Writes an array of characters to the HTTP response output stream.
  /// </summary>
  /// <param name="buffer">The character array to write.</param>
  /// <param name="index">The position in the character array where writing starts.</param>
  /// <param name="count">The number of characters to write, beginning at <paramref name="index"/>.</param>
  public void Write(char[] buffer, int index, int count) {
    this.HasStarted = true;
    this.Output.Write(buffer, index, count);
  }

  /// <summary>
  /// Writes a string followed by a line terminator to the HTTP response output stream.
  /// </summary>
  /// <param name="s">The string to write to the HTTP output stream.</param>
  public void WriteLine(string s) {
    this.HasStarted = true;
    this.Output.WriteLine(s);
  }

  /// <summary>
  /// Writes a line terminator to the HTTP response output stream.
  /// </summary>
  public void WriteLine() {
    this.HasStarted = true;
    this.Output.WriteLine();
  }

  /// <summary>
  /// Sends all currently buffered output to the client.
  /// </summary>
  public void Flush() => this.Output.Flush();

  /// <summary>
  /// Clears all content output from the buffer stream.
  /// </summary>
  public void Clear() { }

  /// <summary>
  /// Clears all headers and content output from the buffer stream.
  /// </summary>
  public void ClearContent() => this.Clear();

  /// <summary>
  /// Clears all headers from the buffer stream.
  /// </summary>
  public void ClearHeaders() => this.Headers.Clear();

  /// <summary>
  /// Redirects the client to a new URL.
  /// </summary>
  /// <param name="url">The target location.</param>
  public void Redirect(string url) => this.Redirect(url, true);

  /// <summary>
  /// Redirects the client to a new URL and specifies whether execution of the current page should terminate.
  /// </summary>
  /// <param name="url">The target location.</param>
  /// <param name="endResponse">Indicates whether execution of the current page should terminate.</param>
  public void Redirect(string url, bool endResponse) {
    this.StatusCode = 302;
    this.StatusDescription = "Found";
    this.RedirectLocation = url;
  }

  /// <summary>
  /// Performs a permanent redirect from the requested URL to the specified URL.
  /// </summary>
  /// <param name="url">The target location.</param>
  public void RedirectPermanent(string url) => this.RedirectPermanent(url, true);

  /// <summary>
  /// Performs a permanent redirect from the requested URL to the specified URL, and specifies whether execution of the current page should terminate.
  /// </summary>
  /// <param name="url">The target location.</param>
  /// <param name="endResponse">Indicates whether execution of the current page should terminate.</param>
  public void RedirectPermanent(string url, bool endResponse) {
    this.StatusCode = 301;
    this.StatusDescription = "Moved Permanently";
    this.RedirectLocation = url;
  }

  /// <summary>
  /// Adds an HTTP header to the output stream.
  /// </summary>
  /// <param name="name">The name of the HTTP header.</param>
  /// <param name="value">The value of the header.</param>
  public void AddHeader(string name, string value) => this.Headers.Add(name, value);

  /// <summary>
  /// Adds an HTTP header to the output stream.
  /// </summary>
  /// <param name="name">The name of the HTTP header to add.</param>
  /// <param name="value">The value of the header.</param>
  public void AppendHeader(string name, string value) => this.Headers.Add(name, value);

  /// <summary>
  /// Adds a Set-Cookie HTTP header to the output stream and sets the cookie.
  /// </summary>
  /// <param name="cookie">The cookie to add to the output stream.</param>
  public void SetCookie(HttpCookie cookie) {
    ArgumentNullException.ThrowIfNull(cookie);
    this.Cookies.Set(cookie);
  }

  /// <summary>
  /// Adds the specified cookie to the cookie collection.
  /// </summary>
  /// <param name="cookie">The cookie to add to the response.</param>
  public void AppendCookie(HttpCookie cookie) {
    ArgumentNullException.ThrowIfNull(cookie);
    this.Cookies.Add(cookie);
  }

  /// <summary>
  /// Sends all currently buffered output to the client, stops execution of the page, and raises the EndRequest event.
  /// </summary>
  public void End() => this.Flush();

  /// <summary>
  /// Writes the specified file directly to an HTTP response output stream.
  /// </summary>
  /// <param name="filename">The name of the file to write to the HTTP output stream.</param>
  public void WriteFile(string filename) {
    this.HasStarted = true;
    using var stream = File.OpenRead(filename);
    stream.CopyTo(this.OutputStream);
  }

  /// <summary>
  /// Writes the contents of the specified file to the HTTP response output stream as a file block.
  /// </summary>
  /// <param name="filename">The name of the file to write to the HTTP output stream.</param>
  /// <param name="offset">The byte position in the file where writing will start.</param>
  /// <param name="size">The number of bytes to write.</param>
  public void WriteFile(string filename, long offset, long size) {
    this.HasStarted = true;
    using var stream = File.OpenRead(filename);
    stream.Seek(offset, SeekOrigin.Begin);
    var buffer = new byte[Math.Min(size, 81920)];
    var remaining = size;
    while (remaining > 0) {
      var toRead = (int)Math.Min(remaining, buffer.Length);
      var read = stream.Read(buffer, 0, toRead);
      if (read == 0)
        break;
      this.OutputStream.Write(buffer, 0, read);
      remaining -= read;
    }
  }

  /// <summary>
  /// Writes the specified array of bytes to the HTTP output stream.
  /// </summary>
  /// <param name="buffer">The bytes to write to the output stream.</param>
  public void BinaryWrite(byte[] buffer) {
    ArgumentNullException.ThrowIfNull(buffer);
    this.HasStarted = true;
    this.OutputStream.Write(buffer, 0, buffer.Length);
  }

}

#endif
