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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.IO;
using System.Net;
using System.Text;
#if SUPPORTS_WEBCLIENT_ASYNC
using System.Threading.Tasks;
#endif
#if SUPPORTS_HTTPCLIENT
using System.Collections.Concurrent;
using System.Net.Http;
#endif

// ReSharper disable PartialTypeWithSinglePart

namespace System {

#if COMPILE_TO_EXTENSION_DLL
  public
#else
  internal
#endif

  static partial class UriExtensions {

    private static readonly Dictionary<HttpRequestHeader, string> _DEFAULT_HEADERS = new() {
      {HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-GB; rv:1.9.2.12) Gecko/20101026 Firefox/3.6.12"},
      {HttpRequestHeader.Accept, "*/*"},
      {HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5"},
      {HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.7"},
    };

    private static void _Execute(Action call, int retryCount) {
      Exception ex = null;
      while (retryCount-- >= 0) {
        try {
          call();
        } catch (Exception e) {
          ex = e;
        }
      }

      throw ex ?? new NotSupportedException("Should never be here");
    }

    private static TResult _Execute<TResult>(Func<TResult> call, int retryCount) {
      Exception ex = null;
      while (retryCount-- >= 0) {
        try {
          return call();
        } catch (Exception e) {
          ex = e;
        }
      }

      throw ex ?? new NotSupportedException("Should never be here");
    }

#if SUPPORTS_HTTPCLIENT || SUPPORTS_WEBCLIENT_ASYNC

    private static async Task<TResult> _Execute<TResult>(Func<Task<TResult>> call, int retryCount) {
      Exception ex = null;
      while (retryCount-- >= 0) {
        try {
          return await call();
        } catch (Exception e) {
          ex = e;
        }
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
    /// Reads all text.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="postValues">The post values.</param>
    /// <returns>
    /// The text of the target url
    /// </returns>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static string ReadAllText(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
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

      NameValueCollection nameValCollection = new();
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
    
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static Uri GetResponseUri(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
#if SUPPORTS_CONTRACTS
      Contract.Requires(@this != null);
#endif
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

      NameValueCollection nameValCollection = new();
      if (postValues != null)
        foreach (var kvp in postValues)
          nameValCollection.Add(kvp.Key, kvp.Value);

      return _Execute(() => {
        if (postValues == null) {
          client.DownloadData(@this);
          return client.ResponseUri;
        }

        client.UploadValues(@this, nameValCollection);
        return client.ResponseUri;
      },retryCount);

#endif

    }

#if !SUPPORTS_HTTPCLIENT

    private class WebClientFake : WebClient {
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
    /// Reads all text.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <returns>The text of the target url</returns>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static async Task<string> ReadAllTextTaskAsync(this Uri @this, Encoding encoding = null, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
#if SUPPORTS_CONTRACTS
    Contract.Requires(@this != null);
#endif

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
    /// Reads all bytes.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="postValues">The post values.</param>
    /// <returns>
    /// The bytes of the target url
    /// </returns>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static byte[] ReadAllBytes(this Uri @this, int retryCount = 0, IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null, IDictionary<string, string> postValues = null) {
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

      NameValueCollection nameValCollection = new();
      if (postValues != null)
        foreach (var kvp in postValues)
          nameValCollection.Add(kvp.Key, kvp.Value);

      return _Execute(() => postValues == null ? client.DownloadData(@this) : client.UploadValues(@this, nameValCollection), retryCount);

#endif

    }

    /// <summary>
    /// Downloads to file.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="file">The file.</param>
    /// <param name="overwrite">if set to <c>true</c> overwrites target file.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="postValues">The post values.</param>
    /// <exception cref="System.Exception">Target file already exists</exception>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static void DownloadToFile(
      this Uri @this,
      FileInfo file,
      bool overwrite = false,
      int retryCount = 0,
      IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null,
      IDictionary<string, string> postValues = null) {
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

      NameValueCollection nameValCollection = new();
      if (postValues != null)
        foreach (var kvp in postValues)
          nameValCollection.Add(kvp.Key, kvp.Value);

      _Execute(() => {
        if (file.Exists && !overwrite)
          throw new("Target file already exists");

        if (postValues == null)
          client.DownloadFile(@this, file.FullName);
        else
          File.WriteAllBytes(file.FullName, client.UploadValues(@this, nameValCollection));
      }, retryCount);

#endif

    }

#if SUPPORTS_WEBCLIENT_ASYNC || SUPPORTS_HTTPCLIENT
    /// <summary>
    /// Reads all bytes.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="headers">The headers.</param>
    /// <returns>
    /// The bytes of the target url
    /// </returns>
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public static async Task<byte[]> ReadAllBytesTaskAsync(
      this Uri @this,
      int retryCount = 0,
      IEnumerable<KeyValuePair<HttpRequestHeader, string>> headers = null) {
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
    /// Gets the base part of the uri.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <returns></returns>
    public static Uri BaseUri(this Uri @this) => new(@this.Scheme + "://" + (
      @this.IsFile
        ? @this.IsUnc
          ? @this.DnsSafeHost
          : IO.Path.GetPathRoot(@this.LocalPath)
        : @this.DnsSafeHost + (@this.IsDefaultPort ? string.Empty : ":" + @this.Port)
      ));

    /// <summary>
    /// Gets a new uri from this one using a relative path.
    /// </summary>
    /// <param name="this">This Uri.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static Uri Path(this Uri @this, string path) {
#if SUPPORTS_CONTRACTS
      Contract.Ensures(Contract.Result<Uri>() != null);
#endif
      const char SLASH = '/';
      return path.IsNullOrWhiteSpace()
        ? @this
        : path.StartsWith(SLASH)
          ? new(@this.BaseUri().AbsoluteUri.TrimEnd(SLASH) + path)
          : new Uri(@this.AbsoluteUri.TrimEnd(SLASH) + SLASH + path)
          ;
    }
  }
}
