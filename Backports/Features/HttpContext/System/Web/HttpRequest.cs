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

// HttpRequest is only available in .NET Framework 4.0+ via System.Web.dll
#if !SUPPORTS_HTTPCONTEXT

using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace System.Web;

/// <summary>
/// Enables reading of HTTP values sent by a client during a Web request.
/// </summary>
public sealed class HttpRequest {

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpRequest"/> class.
  /// </summary>
  public HttpRequest() { }

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpRequest"/> class with the specified filename, URL, and query string.
  /// </summary>
  /// <param name="filename">The name of the file associated with the request.</param>
  /// <param name="url">The URL of the request.</param>
  /// <param name="queryString">The query string of the request.</param>
  public HttpRequest(string filename, string url, string? queryString) {
    this.FilePath = filename;
    this.RawUrl = url;
    if (Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
      this.Url = uri;
      this.Path = uri.AbsolutePath;
    } else {
      this.Path = url;
    }
    if (!string.IsNullOrEmpty(queryString))
      _ParseQueryString(queryString, this.QueryString);
  }

  private static void _ParseQueryString(string queryString, NameValueCollection collection) {
    if (queryString.StartsWith("?"))
      queryString = queryString.Substring(1);

    foreach (var pair in queryString.Split('&')) {
      if (string.IsNullOrEmpty(pair))
        continue;

      var index = pair.IndexOf('=');
      if (index < 0) {
        collection.Add(Uri.UnescapeDataString(pair), null);
      } else {
        var key = Uri.UnescapeDataString(pair.Substring(0, index));
        var value = Uri.UnescapeDataString(pair.Substring(index + 1));
        collection.Add(key, value);
      }
    }
  }

  /// <summary>
  /// Gets or sets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.
  /// </summary>
  public string HttpMethod { get; set; } = "GET";

  /// <summary>
  /// Gets or sets the virtual path of the current request.
  /// </summary>
  public string Path { get; set; } = "/";

  /// <summary>
  /// Gets or sets the raw URL of the current request.
  /// </summary>
  public string RawUrl { get; set; } = "/";

  /// <summary>
  /// Gets or sets information about the URL of the current request.
  /// </summary>
  public Uri? Url { get; set; }

  /// <summary>
  /// Gets or sets information about the URL of the client's previous request that linked to the current URL.
  /// </summary>
  public Uri? UrlReferrer { get; set; }

  /// <summary>
  /// Gets the collection of HTTP headers.
  /// </summary>
  public NameValueCollection Headers { get; } = new();

  /// <summary>
  /// Gets the collection of HTTP query string variables.
  /// </summary>
  public NameValueCollection QueryString { get; } = new();

  /// <summary>
  /// Gets a collection of form variables.
  /// </summary>
  public NameValueCollection Form { get; } = new();

  /// <summary>
  /// Gets a combined collection of <see cref="QueryString"/>, <see cref="Form"/>, <see cref="Cookies"/>, and <see cref="ServerVariables"/> items.
  /// </summary>
  public NameValueCollection Params {
    get {
      var result = new NameValueCollection();
      result.Add(this.QueryString);
      result.Add(this.Form);
      foreach (var key in this.Cookies.AllKeys)
        if (key != null)
          result.Add(key, this.Cookies[key]?.Value);
      result.Add(this.ServerVariables);
      return result;
    }
  }

  /// <summary>
  /// Gets a collection of cookies sent by the client.
  /// </summary>
  public HttpCookieCollection Cookies { get; } = new();

  /// <summary>
  /// Gets a collection of Web server variables.
  /// </summary>
  public NameValueCollection ServerVariables { get; } = new();

  /// <summary>
  /// Gets or sets the contents of the incoming HTTP entity body.
  /// </summary>
  public Stream InputStream { get; set; } = Stream.Null;

  /// <summary>
  /// Gets or sets the MIME content type of the incoming request.
  /// </summary>
  public string ContentType { get; set; } = "";

  /// <summary>
  /// Gets or sets the length, in bytes, of content sent by the client.
  /// </summary>
  public int ContentLength { get; set; }

  /// <summary>
  /// Gets or sets the character set of the entity-body.
  /// </summary>
  public Encoding ContentEncoding { get; set; } = Encoding.UTF8;

  /// <summary>
  /// Gets the raw user agent string of the client browser.
  /// </summary>
  public string? UserAgent => this.Headers["User-Agent"];

  /// <summary>
  /// Gets or sets the IP host address of the remote client.
  /// </summary>
  public string? UserHostAddress { get; set; }

  /// <summary>
  /// Gets or sets the DNS name of the remote client.
  /// </summary>
  public string? UserHostName { get; set; }

  /// <summary>
  /// Gets a sorted string array of client language preferences.
  /// </summary>
  public string[]? UserLanguages { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the HTTP connection uses secure sockets (HTTPS).
  /// </summary>
  public bool IsSecureConnection { get; set; }

  /// <summary>
  /// Gets or sets a value indicating whether the request is from the local computer.
  /// </summary>
  public bool IsLocal { get; set; }

  /// <summary>
  /// Gets or sets the virtual path of the application root.
  /// </summary>
  public string ApplicationPath { get; set; } = "/";

  /// <summary>
  /// Gets or sets the physical file system path of the application root.
  /// </summary>
  public string? PhysicalApplicationPath { get; set; }

  /// <summary>
  /// Gets or sets the physical file system path corresponding to the requested URL.
  /// </summary>
  public string? PhysicalPath { get; set; }

  /// <summary>
  /// Gets or sets the virtual path of the current request.
  /// </summary>
  public string FilePath { get; set; } = "/";

  /// <summary>
  /// Gets or sets additional path information for a resource with a URL extension.
  /// </summary>
  public string? PathInfo { get; set; }

  /// <summary>
  /// Gets the specified object from the <see cref="QueryString"/>, <see cref="Form"/>, <see cref="Cookies"/>, or <see cref="ServerVariables"/> collections.
  /// </summary>
  /// <param name="key">The key to retrieve.</param>
  /// <returns>The value from the combined collections, or <c>null</c> if not found.</returns>
  public string? this[string key] => this.Params[key];

}

#endif
