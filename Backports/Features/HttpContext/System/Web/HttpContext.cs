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

// HttpContext is only available in .NET Framework 4.0+ via System.Web.dll
#if !SUPPORTS_HTTPCONTEXT

using System.Collections;
using System.Security.Principal;
using System.Threading;

namespace System.Web;

/// <summary>
/// Encapsulates all HTTP-specific information about an individual HTTP request.
/// </summary>
public sealed class HttpContext {

  private static readonly ThreadLocal<HttpContext?> _current = new();

  /// <summary>
  /// Gets or sets the <see cref="HttpContext"/> object for the current HTTP request.
  /// </summary>
  public static HttpContext? Current {
    get => _current.Value;
    set => _current.Value = value;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="HttpContext"/> class.
  /// </summary>
  /// <param name="request">The <see cref="HttpRequest"/> object for the current HTTP request.</param>
  /// <param name="response">The <see cref="HttpResponse"/> object for the current HTTP response.</param>
  public HttpContext(HttpRequest request, HttpResponse response) {
    this.Request = request ?? throw new ArgumentNullException(nameof(request));
    this.Response = response ?? throw new ArgumentNullException(nameof(response));
  }

  /// <summary>
  /// Gets the <see cref="HttpRequest"/> object for the current HTTP request.
  /// </summary>
  public HttpRequest Request { get; }

  /// <summary>
  /// Gets the <see cref="HttpResponse"/> object for the current HTTP response.
  /// </summary>
  public HttpResponse Response { get; }

  /// <summary>
  /// Gets a key/value collection that can be used to organize and share data between an <see cref="System.Web.IHttpModule"/> interface and an <see cref="System.Web.IHttpHandler"/> interface during an HTTP request.
  /// </summary>
  public IDictionary Items { get; } = new Hashtable();

  /// <summary>
  /// Gets or sets security information for the current HTTP request.
  /// </summary>
  public IPrincipal? User { get; set; }

  /// <summary>
  /// Gets a value that indicates whether the current HTTP request is in debug mode.
  /// </summary>
  public bool IsDebuggingEnabled { get; set; }

  /// <summary>
  /// Gets a value that indicates whether custom errors are enabled for the current HTTP request.
  /// </summary>
  public bool IsCustomErrorEnabled { get; set; }

  /// <summary>
  /// Gets or sets a value that specifies whether the <see cref="System.Web.Security.UrlAuthorizationModule"/> object should skip the authorization check for the current request.
  /// </summary>
  public bool SkipAuthorization { get; set; }

  /// <summary>
  /// Gets the <see cref="DateTime"/> object that represents the initial timestamp of the current HTTP request.
  /// </summary>
  public DateTime Timestamp { get; } = DateTime.UtcNow;

  /// <summary>
  /// Gets or sets an object that contains errors accumulated while processing an HTTP request.
  /// </summary>
  public Exception? Error { get; set; }

  /// <summary>
  /// Gets an array of errors accumulated while processing an HTTP request.
  /// </summary>
  public Exception[] AllErrors => this.Error != null ? new[] { this.Error } : Array.Empty<Exception>();

  /// <summary>
  /// Clears all errors for the current HTTP request.
  /// </summary>
  public void ClearError() => this.Error = null;

  /// <summary>
  /// Adds an exception to the exception collection for the current HTTP request.
  /// </summary>
  /// <param name="errorInfo">The <see cref="Exception"/> to add to the exception collection.</param>
  public void AddError(Exception errorInfo) {
    if (this.Error == null)
      this.Error = errorInfo;
    else
      this.Error = new AggregateException(this.Error, errorInfo);
  }

  /// <summary>
  /// Gets the <see cref="IHttpHandler"/> object that represents the currently executing handler.
  /// </summary>
  public IHttpHandler? Handler { get; set; }

  /// <summary>
  /// Gets the <see cref="IHttpHandler"/> object for the parent handler.
  /// </summary>
  public IHttpHandler? PreviousHandler { get; set; }

  /// <summary>
  /// Gets the <see cref="IHttpHandler"/> object for the current handler.
  /// </summary>
  public IHttpHandler? CurrentHandler => this.Handler;

  /// <summary>
  /// Rewrites the URL using the given path.
  /// </summary>
  /// <param name="path">The replacement path.</param>
  public void RewritePath(string path) {
    ArgumentNullException.ThrowIfNull(path);
    this.Request.Path = path;
    this.Request.RawUrl = path;
  }

  /// <summary>
  /// Rewrites the URL using the given path and a Boolean value that specifies whether the virtual path for server resources is modified.
  /// </summary>
  /// <param name="path">The replacement path.</param>
  /// <param name="rebaseClientPath">true to reset the virtual path; false to keep the virtual path unchanged.</param>
  public void RewritePath(string path, bool rebaseClientPath) => this.RewritePath(path);

  /// <summary>
  /// Rewrites the URL using the given virtual path, path information, and query string information.
  /// </summary>
  /// <param name="filePath">The virtual path to the resource that services the request.</param>
  /// <param name="pathInfo">Additional path information to use for the URL redirect.</param>
  /// <param name="queryString">The request query string to use for the URL redirect.</param>
  public void RewritePath(string filePath, string pathInfo, string? queryString) {
    ArgumentNullException.ThrowIfNull(filePath);
    this.Request.FilePath = filePath;
    this.Request.PathInfo = pathInfo;
    this.Request.Path = filePath;
    this.Request.RawUrl = queryString != null ? $"{filePath}?{queryString}" : filePath;
  }

  /// <summary>
  /// Rewrites the URL by using the given virtual path, path information, query string information, and a Boolean value that specifies whether the client file path is set to the rewrite path.
  /// </summary>
  /// <param name="filePath">The virtual path to the resource that services the request.</param>
  /// <param name="pathInfo">Additional path information to use for the URL redirect.</param>
  /// <param name="queryString">The request query string to use for the URL redirect.</param>
  /// <param name="setClientFilePath">true to set the file path used for client resources to the value of the <paramref name="filePath"/> parameter; otherwise, false.</param>
  public void RewritePath(string filePath, string pathInfo, string? queryString, bool setClientFilePath)
    => this.RewritePath(filePath, pathInfo, queryString);

  /// <summary>
  /// Returns an object for the current service type.
  /// </summary>
  /// <param name="serviceType">The type of service object to get.</param>
  /// <returns>The current service type, or <c>null</c> if no service of type <paramref name="serviceType"/> is found.</returns>
  public object? GetService(Type serviceType) => null;

}

/// <summary>
/// Defines the contract that ASP.NET implements to synchronously process HTTP Web requests using a custom HTTP handler.
/// </summary>
public interface IHttpHandler {

  /// <summary>
  /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler"/> interface.
  /// </summary>
  /// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects used to service HTTP requests.</param>
  void ProcessRequest(HttpContext context);

  /// <summary>
  /// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
  /// </summary>
  bool IsReusable { get; }

}

/// <summary>
/// Provides module initialization and disposal events to the implementing class.
/// </summary>
public interface IHttpModule {

  /// <summary>
  /// Initializes a module and prepares it to handle requests.
  /// </summary>
  /// <param name="context">An <see cref="HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application.</param>
  void Init(HttpApplication context);

  /// <summary>
  /// Disposes of the resources (other than memory) used by the module that implements <see cref="IHttpModule"/>.
  /// </summary>
  void Dispose();

}

/// <summary>
/// Defines the methods, properties, and events that are common to all application objects in an ASP.NET application.
/// </summary>
public class HttpApplication : IDisposable {

  /// <summary>
  /// Gets the current <see cref="HttpContext"/> object.
  /// </summary>
  public HttpContext? Context => HttpContext.Current;

  /// <summary>
  /// Gets the <see cref="HttpRequest"/> object for the current request.
  /// </summary>
  public HttpRequest? Request => this.Context?.Request;

  /// <summary>
  /// Gets the <see cref="HttpResponse"/> object for the current HTTP response.
  /// </summary>
  public HttpResponse? Response => this.Context?.Response;

  /// <summary>
  /// Gets or sets the <see cref="IPrincipal"/> security information for the current HTTP request.
  /// </summary>
  public IPrincipal? User {
    get => this.Context?.User;
    set {
      if (this.Context != null)
        this.Context.User = value;
    }
  }

  /// <summary>
  /// Releases all resources used by the <see cref="HttpApplication"/> class.
  /// </summary>
  public virtual void Dispose() { }

  /// <summary>
  /// Executes custom initialization code after all event handler modules have been added.
  /// </summary>
  public virtual void Init() { }

}

#endif
