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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using Guard;
#if !SUPPORTS_HTTPCLIENT
using System.Collections.Specialized;
#endif
#if SUPPORTS_WEBCLIENT_ASYNC
using System.Threading.Tasks;
#endif
#if SUPPORTS_HTTPCLIENT
using System.Collections.Concurrent;
using System.Net.Http;
#endif

namespace System;

public static partial class UriExtensions {
  private static readonly Dictionary<HttpRequestHeader, string> _DEFAULT_HEADERS = new() { { HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12" }, { HttpRequestHeader.Accept, "*/*" }, { HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5" }, { HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7" }, };

  private static void _Execute(Action call, int retryCount) {
    Exception ex = null;
    while (retryCount-- >= 0)
      try {
        call();
      } catch (Exception e) {
        ex = e;
      }

    throw ex ?? new NotSupportedException("Should never be here");
  }

  private static TResult _Execute<TResult>(Func<TResult> call, int retryCount) {
    Exception ex = null;
    while (retryCount-- >= 0)
      try {
        return call();
      } catch (Exception e) {
        ex = e;
      }

    throw ex ?? new NotSupportedException("Should never be here");
  }

#if SUPPORTS_HTTPCLIENT || SUPPORTS_WEBCLIENT_ASYNC

  private static async Task<TResult> _Execute<TResult>(Func<Task<TResult>> call, int retryCount) {
    Exception ex = null;
    while (retryCount-- >= 0)
      try {
        return await call();
      } catch (Exception e) {
        ex = e;
      }

    throw ex ?? new NotSupportedException("Should never be here");
  }

#endif

#if SUPPORTS_HTTPCLIENT
  private static readonly ConcurrentDictionary<HttpRequestHeader, string> _httpHeaderNameCache = new();

  private static string _Convert(HttpRequestHeader header) {
    string Convert(string s) {

      // Create a StringBuilder with an initial capacity based on the original string's length
      // ReSharper disable once VariableHidesOuterVariable
      StringBuilder result = new(s.Length + 5);
      
      // first character is uppercase in all cases
      result.Append(s[0] + ('a' - 'A'));

      // Iterate over each character in the PascalCase name
      for (var i = 1; i < s.Length; ++i) {
        var c = s[i];

        // If the current character is uppercase
        if (c is >= 'A' and <= 'Z') {
          // insert a hyphen before it
          result.Append('-');
          // and convert to lowercase
          result.Append(c + ('a' - 'A'));
          continue;
        }

        // Append the current character
        result.Append(c);
      }

      return result.ToString();
    }

    var headerName = Enum.GetName(typeof(HttpRequestHeader), header);

    // Check if the formatted name for this header is already in the cache
    if (_httpHeaderNameCache.TryGetValue(header, out var result))
      return result;

    if (headerName != null)
      result = Convert(headerName);

    // Add the formatted header name to the cache
    _httpHeaderNameCache.TryAdd(header, result);

    return result;
  }

#endif

  /// <summary>
  /// Reads the full textual content from the specified <see cref="Uri"/> using optional HTTP POST and custom headers.
  /// </summary>
  /// <param name="this">The <see cref="Uri"/> to read from.</param>
  /// <param name="encoding">
  /// (Optional: defaults to UTF-8) The <see cref="Encoding"/> to use when decoding the response body.
  /// </param>
  /// <param name="retryCount">
  /// (Optional: defaults to 0) The number of retry attempts to make in case of transient failures.
  /// </param>
  /// <param name="headers">
  /// (Optional) A sequence of HTTP headers to include with the request.
  /// </param>
  /// <param name="postValues">
  /// (Optional) A dictionary of key-value pairs to be sent as application/x-www-form-urlencoded POST data.
  /// </param>
  /// <returns>The response body as a decoded <see cref="string"/>.</returns>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="WebException">Thrown if the request fails and <paramref name="retryCount"/> is exhausted.</exception>
  /// <remarks>
  /// If <paramref name="postValues"/> is <see langword="null"/>, a GET request is made; otherwise, a POST request is performed.
  /// </remarks>
  /// <example>
  /// <code>
  /// var uri = new Uri("https://example.com/api");
  /// var content = uri.ReadAllText(encoding: Encoding.UTF8, retryCount: 2);
  /// Console.WriteLine(content);
  /// </code>
  /// </example>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static string ReadAllText(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
    Against.ThisIsNull(@this);

    Trace.WriteLine(@this.AbsoluteUri);

    if (@this.IsFile)
      return encoding == null ? File.ReadAllText(@this.LocalPath) : File.ReadAllText(@this.LocalPath, encoding);

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client,headers);
    var content = postValues == null ? null : new FormUrlEncodedContent(postValues);
    return _Execute(() => {
      var response = (postValues == null ? client.GetAsync(@this) : client.PostAsync(@this, content)).Result;
      response.EnsureSuccessStatusCode();
      return encoding == null 
        ? response.Content.ReadAsStringAsync().Result 
        : encoding.GetString(response.Content.ReadAsByteArrayAsync().Result)
        ;
    }, retryCount);

#else

    using WebClient client = new();
    _SetClientHeaders(client, headers);
    if (encoding != null)
      client.Encoding = encoding;

    NameValueCollection nameValCollection = [];
    if (postValues != null)
      foreach (var kvp in postValues)
        nameValCollection.Add(kvp.Key, kvp.Value);

    return _Execute(() => postValues == null ? client.DownloadString(@this) : (encoding ?? Encoding.Default).GetString(client.UploadValues(@this, nameValCollection)), retryCount);

#endif
  }

#if SUPPORTS_HTTPCLIENT
  private static void _SetClientHeaders(HttpClient webClient, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers) {
    foreach (var header in headers ?? _DEFAULT_HEADERS)
      webClient.DefaultRequestHeaders.Add( _Convert(header.Key), header.Value);
  }
#else
  private static void _SetClientHeaders(WebClient webClient, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers) {
    foreach (var header in headers ?? _DEFAULT_HEADERS)
      webClient.Headers.Add(header.Key, header.Value);
  }
#endif

  /// <summary>
  /// Sends an HTTP request to the specified <see cref="Uri"/> and returns the final resolved response URI,
  /// including any redirections that occurred.
  /// </summary>
  /// <param name="this">The target <see cref="Uri"/>.</param>
  /// <param name="retryCount">
  /// (Optional: defaults to 0) The number of times to retry the request in case of network failures.
  /// </param>
  /// <param name="headers">
  /// (Optional) A collection of custom <see cref="HttpRequestHeader"/> values to include in the request.
  /// </param>
  /// <param name="postValues">
  /// (Optional) If provided, a POST request with the specified form-encoded data is sent; otherwise, a GET request is performed.
  /// </param>
  /// <returns>The resolved <see cref="Uri"/> after completing the request, accounting for HTTP redirects.</returns>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="WebException">Thrown if the HTTP request fails after all retry attempts.</exception>
  /// <remarks>
  /// This method can be used to track endpoint redirection behavior or confirm final download URLs.
  /// </remarks>
  /// <example>
  /// <code>
  /// var original = new Uri("http://example.org/redirect");
  /// var final = original.GetResponseUri(retryCount: 1);
  /// Console.WriteLine($"Final location: {final}");
  /// </code>
  /// </example>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static Uri GetResponseUri(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
    Against.ThisIsNull(@this);

    Trace.WriteLine(@this.AbsoluteUri);

    if (@this.IsFile)
      return @this;

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client, headers);
    var content = postValues == null ? null : new FormUrlEncodedContent(postValues);
    return _Execute(() => {
      var response = (postValues == null ? client.GetAsync(@this) : client.PostAsync(@this, content)).Result;
      response.EnsureSuccessStatusCode();
      return response.RequestMessage?.RequestUri;
    }, retryCount);

#else

    using WebClientFake client = new();
    _SetClientHeaders(client, headers);

    NameValueCollection nameValCollection = [];
    if (postValues != null)
      foreach (var kvp in postValues)
        nameValCollection.Add(kvp.Key, kvp.Value);

    return _Execute(
      () => {
        if (postValues == null) {
          client.DownloadData(@this);
          return client.ResponseUri;
        }

        client.UploadValues(@this, nameValCollection);
        return client.ResponseUri;
      },
      retryCount
    );

#endif
  }

#if !SUPPORTS_HTTPCLIENT

  private sealed class WebClientFake : WebClient {
    public Uri ResponseUri { get; private set; }

    #region Overrides of WebClient

    protected override WebResponse GetWebResponse(WebRequest request) {
      try {
        var response = base.GetWebResponse(request);
        this.ResponseUri = response.ResponseUri;
        return response;
      } catch (WebException) {
        return null;
      }
    }

    protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result) {
      var response = base.GetWebResponse(request, result);
      this.ResponseUri = response.ResponseUri;
      return response;
    }

    #endregion
  }

#endif

#if SUPPORTS_WEBCLIENT_ASYNC || SUPPORTS_HTTPCLIENT
  /// <summary>
  ///   Reads all text.
  /// </summary>
  /// <param name="this">This Uri.</param>
  /// <param name="encoding">The encoding.</param>
  /// <param name="retryCount">The retry count.</param>
  /// <returns>The text of the target url</returns>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static async Task<string> ReadAllTextTaskAsync(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
    Against.ThisIsNull(@this);

    if (@this.IsFile)
      return await new Task<string>(() => encoding == null ? File.ReadAllText(@this.AbsolutePath) : File.ReadAllText(@this.AbsolutePath, encoding));

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client, headers);
    return await _Execute(async() => {
      var response = await client.GetAsync(@this);
      response.EnsureSuccessStatusCode();
      return encoding == null 
        ? await response.Content.ReadAsStringAsync() 
        : encoding.GetString(await response.Content.ReadAsByteArrayAsync())
        ;
    }, retryCount);

#else

    using WebClient client = new();
    _SetClientHeaders(client, headers);
    if (encoding != null)
      client.Encoding = encoding;

    return await _Execute(async () => await client.DownloadStringTaskAsync(@this), retryCount);

#endif
  }
#endif

  /// <summary>
  /// Downloads the raw binary content from the specified <see cref="Uri"/> as a byte array.
  /// </summary>
  /// <param name="this">The target <see cref="Uri"/> to read from.</param>
  /// <param name="retryCount">
  /// (Optional: defaults to 0) The number of retry attempts in case of transient request failures.
  /// </param>
  /// <param name="headers">
  /// (Optional) A collection of custom <see cref="HttpRequestHeader"/> values to include with the request.
  /// </param>
  /// <param name="postValues">
  /// (Optional) If specified, a POST request with the given form-encoded data is sent; otherwise, a GET request is used.
  /// </param>
  /// <returns>The response body as a byte array.</returns>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="WebException">Thrown if the request fails after exhausting all retry attempts.</exception>
  /// <remarks>
  /// Use this method to retrieve non-textual data such as images, binaries, or file downloads.
  /// </remarks>
  /// <example>
  /// <code>
  /// var uri = new Uri("https://example.com/image.png");
  /// byte[] imageData = uri.ReadAllBytes();
  /// File.WriteAllBytes("downloaded.png", imageData);
  /// </code>
  /// </example>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static byte[] ReadAllBytes(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
    Against.ThisIsNull(@this);

    if (@this.IsFile)
      return File.ReadAllBytes(@this.LocalPath);

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client, headers);
    var content = postValues == null ? null : new FormUrlEncodedContent(postValues);
    return _Execute(() => {
      var response = (postValues == null ? client.GetAsync(@this) : client.PostAsync(@this, content)).Result;
      response.EnsureSuccessStatusCode();
      return response.Content.ReadAsByteArrayAsync().Result;
    }, retryCount);

#else

    using WebClient client = new();
    _SetClientHeaders(client, headers);

    NameValueCollection nameValCollection = [];
    if (postValues != null)
      foreach (var kvp in postValues)
        nameValCollection.Add(kvp.Key, kvp.Value);

    return _Execute(() => postValues == null ? client.DownloadData(@this) : client.UploadValues(@this, nameValCollection), retryCount);

#endif
  }

  /// <summary>
  /// Downloads the content from the specified <see cref="Uri"/> and writes it to the given <see cref="FileInfo"/> location.
  /// </summary>
  /// <param name="this">The <see cref="Uri"/> to download from.</param>
  /// <param name="file">The destination <see cref="FileInfo"/> to save the downloaded content.</param>
  /// <param name="overwrite">
  /// (Optional: defaults to <see langword="false"/>) Indicates whether to overwrite the target file if it already exists.
  /// </param>
  /// <param name="retryCount">
  /// (Optional: defaults to 0) Number of retry attempts in case of request failures.
  /// </param>
  /// <param name="headers">
  /// (Optional) Custom <see cref="HttpRequestHeader"/> entries to include in the request.
  /// </param>
  /// <param name="postValues">
  /// (Optional) If provided, sends the request using HTTP POST with the specified form data; otherwise, a GET request is performed.
  /// </param>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="file"/> is <see langword="null"/>.</exception>
  /// <exception cref="IOException">Thrown if the file exists and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
  /// <exception cref="WebException">Thrown if the request fails after retry attempts are exhausted.</exception>
  /// <remarks>
  /// This method is suitable for saving web content to disk, including files retrieved through redirects or form submissions.
  /// </remarks>
  /// <example>
  /// <code>
  /// var uri = new Uri("https://example.com/report.pdf");
  /// var file = new FileInfo("report.pdf");
  /// uri.DownloadToFile(file, overwrite: true);
  /// </code>
  /// </example>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static void DownloadToFile(this Uri @this, FileInfo file, bool overwrite = false, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
    Against.ThisIsNull(@this);

    if (@this.IsFile) {
      File.Copy(@this.LocalPath, file.FullName, overwrite);
      return;
    }

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client, headers);
    var content = postValues == null ? null : new FormUrlEncodedContent(postValues);
    _Execute(() => {
      using FileStream fileStream = new(file.FullName, overwrite ? FileMode.Create : FileMode.CreateNew);
      var response = (postValues == null ? client.GetAsync(@this) : client.PostAsync(@this, content)).Result;
      response.EnsureSuccessStatusCode();
      response.Content.CopyToAsync(fileStream).Wait();
    }, retryCount);

#else

    using WebClient client = new();
    _SetClientHeaders(client, headers);

    NameValueCollection nameValCollection = [];
    if (postValues != null)
      foreach (var kvp in postValues)
        nameValCollection.Add(kvp.Key, kvp.Value);

    _Execute(
      () => {
        if (file.Exists && !overwrite)
          throw new("Target file already exists");

        if (postValues == null)
          client.DownloadFile(@this, file.FullName);
        else
          File.WriteAllBytes(file.FullName, client.UploadValues(@this, nameValCollection));
      },
      retryCount
    );

#endif
  }

#if SUPPORTS_WEBCLIENT_ASYNC || SUPPORTS_HTTPCLIENT
  /// <summary>
  ///   Reads all bytes.
  /// </summary>
  /// <param name="this">This Uri.</param>
  /// <param name="retryCount">The retry count.</param>
  /// <param name="headers">The headers.</param>
  /// <returns>
  ///   The bytes of the target url
  /// </returns>
  [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
  public static async Task<byte[]> ReadAllBytesTaskAsync(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
    Against.ThisIsNull(@this);

    if (@this.IsFile)
      return await new Task<byte[]>(() => File.ReadAllBytes(@this.AbsolutePath));

#if SUPPORTS_HTTPCLIENT
    using HttpClient client = new();
    _SetClientHeaders(client, headers);
    return await _Execute(async () => {
      var response = await client.GetAsync(@this);
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadAsByteArrayAsync();
    }, retryCount);

#else

    using WebClient client = new();
    _SetClientHeaders(client, headers);

    return await _Execute(async () => await client.DownloadDataTaskAsync(@this), retryCount);

#endif
  }
#endif

  /// <summary>
  /// Gets the base portion of the specified <see cref="Uri"/>, excluding any path segments, query parameters, or fragments.
  /// </summary>
  /// <param name="this">The target <see cref="Uri"/>.</param>
  /// <returns>
  /// A new <see cref="Uri"/> containing only the scheme, host, and port (if specified).
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// Useful when you want to construct additional relative URIs based on a root or authority.
  /// </remarks>
  /// <example>
  /// <code>
  /// var uri = new Uri("https://example.com/api/data?id=123");
  /// var baseUri = uri.BaseUri(); // "https://example.com"
  /// </code>
  /// </example>
  public static Uri BaseUri(this Uri @this) {
    Against.ThisIsNull(@this);

    return new(
      $"{@this.Scheme}://{(
        @this.IsFile
          ? @this.IsUnc
            ? @this.DnsSafeHost
            : IO.Path.GetPathRoot(@this.LocalPath)
          : @this.DnsSafeHost + (@this.IsDefaultPort ? string.Empty : ":" + @this.Port)
      )}"
    );
  }

  /// <summary>
  /// Appends a relative path to the base <see cref="Uri"/> and returns the resulting <see cref="Uri"/>.
  /// </summary>
  /// <param name="this">The base <see cref="Uri"/> to append to.</param>
  /// <param name="path">The relative path to append. May include subdirectories or file names.</param>
  /// <returns>
  /// A new <see cref="Uri"/> combining the base URI with the specified relative path.
  /// </returns>
  /// <exception cref="NullReferenceException">Thrown when <paramref name="this"/> is <see langword="null"/>.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
  /// <remarks>
  /// This method ensures the relative path is resolved correctly, avoiding issues with slashes.
  /// </remarks>
  /// <example>
  /// <code>
  /// var baseUri = new Uri("https://example.com/api/");
  /// var fullUri = baseUri.Path("users/123");
  /// // fullUri is "https://example.com/api/users/123"
  /// </code>
  /// </example>
  public static Uri Path(this Uri @this, string path) {
    Against.ThisIsNull(@this);

    const char SLASH = '/';
    return path.IsNullOrWhiteSpace()
        ? @this
        : path.StartsWith(SLASH)
          ? new(@this.BaseUri().AbsoluteUri.TrimEnd(SLASH) + path)
          : new Uri(@this.AbsoluteUri.TrimEnd(SLASH) + SLASH + path)
      ;
  }
}
