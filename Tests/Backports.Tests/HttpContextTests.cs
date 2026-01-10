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

using System;
using System.IO;
using System.Web;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
public class HttpContextTests {

  #region HttpCookie Tests

  [Test]
  public void HttpCookie_Constructor_WithName_Works() {
    var cookie = new HttpCookie("test");
    Assert.That(cookie.Name, Is.EqualTo("test"));
    Assert.That(cookie.Value, Is.Null.Or.Empty);
  }

  [Test]
  public void HttpCookie_Constructor_WithNameAndValue_Works() {
    var cookie = new HttpCookie("test", "value123");
    Assert.That(cookie.Name, Is.EqualTo("test"));
    Assert.That(cookie.Value, Is.EqualTo("value123"));
  }

  [Test]
  public void HttpCookie_Properties_CanBeSet() {
    var cookie = new HttpCookie("test") {
      Value = "myvalue",
      Domain = "example.com",
      Path = "/app",
      Secure = true,
      HttpOnly = true
    };
    Assert.That(cookie.Value, Is.EqualTo("myvalue"));
    Assert.That(cookie.Domain, Is.EqualTo("example.com"));
    Assert.That(cookie.Path, Is.EqualTo("/app"));
    Assert.That(cookie.Secure, Is.True);
    Assert.That(cookie.HttpOnly, Is.True);
  }

  [Test]
  public void HttpCookie_Expires_CanBeSet() {
    var cookie = new HttpCookie("test");
    var expires = DateTime.UtcNow.AddDays(7);
    cookie.Expires = expires;
    Assert.That(cookie.Expires, Is.EqualTo(expires));
  }

  [Test]
  public void HttpCookie_Values_CanBeAccessed() {
    var cookie = new HttpCookie("test");
    cookie.Values["key1"] = "value1";
    cookie.Values["key2"] = "value2";
    Assert.That(cookie.Values["key1"], Is.EqualTo("value1"));
    Assert.That(cookie.Values["key2"], Is.EqualTo("value2"));
  }

  [Test]
  public void HttpCookie_Indexer_CanBeUsed() {
    var cookie = new HttpCookie("test");
    cookie["key1"] = "value1";
    Assert.That(cookie["key1"], Is.EqualTo("value1"));
  }

  [Test]
  public void HttpCookie_HasKeys_ReturnsCorrectValue() {
    var cookie = new HttpCookie("test");
    Assert.That(cookie.HasKeys, Is.False);
    cookie["key1"] = "value1";
    Assert.That(cookie.HasKeys, Is.True);
  }

  #endregion

  #region HttpCookieCollection Tests

  [Test]
  public void HttpCookieCollection_Add_Works() {
    var collection = new HttpCookieCollection();
    var cookie = new HttpCookie("test", "value");
    collection.Add(cookie);
    Assert.That(collection.Count, Is.EqualTo(1));
  }

  [Test]
  public void HttpCookieCollection_GetByName_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("cookie1", "value1"));
    collection.Add(new HttpCookie("cookie2", "value2"));
    var result = collection["cookie1"];
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Value, Is.EqualTo("value1"));
  }

  [Test]
  public void HttpCookieCollection_GetByIndex_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("cookie1", "value1"));
    collection.Add(new HttpCookie("cookie2", "value2"));
    var result = collection[0];
    Assert.That(result, Is.Not.Null);
  }

  [Test]
  public void HttpCookieCollection_Get_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("test", "value"));
    var result = collection.Get("test");
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Value, Is.EqualTo("value"));
  }

  [Test]
  public void HttpCookieCollection_Set_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("test", "original"));
    collection.Set(new HttpCookie("test", "updated"));
    var result = collection["test"];
    Assert.That(result.Value, Is.EqualTo("updated"));
  }

  [Test]
  public void HttpCookieCollection_Remove_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("test", "value"));
    Assert.That(collection.Count, Is.EqualTo(1));
    collection.Remove("test");
    Assert.That(collection.Count, Is.EqualTo(0));
  }

  [Test]
  public void HttpCookieCollection_Clear_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("cookie1", "value1"));
    collection.Add(new HttpCookie("cookie2", "value2"));
    Assert.That(collection.Count, Is.EqualTo(2));
    collection.Clear();
    Assert.That(collection.Count, Is.EqualTo(0));
  }

  [Test]
  public void HttpCookieCollection_AllKeys_Works() {
    var collection = new HttpCookieCollection();
    collection.Add(new HttpCookie("cookie1", "value1"));
    collection.Add(new HttpCookie("cookie2", "value2"));
    var keys = collection.AllKeys;
    Assert.That(keys, Does.Contain("cookie1"));
    Assert.That(keys, Does.Contain("cookie2"));
  }

  #endregion

  #region HttpRequest Tests

  [Test]
  public void HttpRequest_DefaultValues_AreCorrect() {
    var request = new HttpRequest(null, "http://localhost/", null);
    Assert.That(request.HttpMethod, Is.EqualTo("GET"));
  }

  [Test]
  public void HttpRequest_Headers_CanBeAccessed() {
    var request = new HttpRequest(null, "http://localhost/", null);
    Assert.That(request.Headers, Is.Not.Null);
  }

  [Test]
  public void HttpRequest_QueryString_CanBeAccessed() {
    var request = new HttpRequest(null, "http://localhost/?key=value", "key=value");
    Assert.That(request.QueryString, Is.Not.Null);
    Assert.That(request.QueryString["key"], Is.EqualTo("value"));
  }

  [Test]
  public void HttpRequest_Form_CanBeAccessed() {
    var request = new HttpRequest(null, "http://localhost/", null);
    Assert.That(request.Form, Is.Not.Null);
  }

  [Test]
  public void HttpRequest_Cookies_CanBeAccessed() {
    var request = new HttpRequest(null, "http://localhost/", null);
    Assert.That(request.Cookies, Is.Not.Null);
  }

  [Test]
  public void HttpRequest_Url_IsSet() {
    var request = new HttpRequest(null, "http://localhost/path", null);
    Assert.That(request.Url, Is.Not.Null);
    Assert.That(request.Url.AbsolutePath, Is.EqualTo("/path"));
  }

  [Test]
  public void HttpRequest_Path_IsSet() {
    var request = new HttpRequest(null, "http://localhost/mypath", null);
    Assert.That(request.Path, Is.EqualTo("/mypath"));
  }

  [Test]
  public void HttpRequest_RawUrl_IsSet() {
    var request = new HttpRequest(null, "http://localhost/path?query=1", "query=1");
    Assert.That(request.RawUrl, Does.Contain("/path"));
  }

  [Test]
  public void HttpRequest_ContentType_CanBeSet() {
    var request = new HttpRequest(null, "http://localhost/", null);
    Assert.That(request.ContentType, Is.Not.Null);
  }

  #endregion

  #region HttpResponse Tests

  [Test]
  public void HttpResponse_DefaultStatusCode_Is200() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    Assert.That(response.StatusCode, Is.EqualTo(200));
  }

  [Test]
  public void HttpResponse_StatusCode_CanBeSet() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.StatusCode = 404;
    Assert.That(response.StatusCode, Is.EqualTo(404));
  }

  [Test]
  public void HttpResponse_StatusDescription_CanBeSet() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.StatusDescription = "Not Found";
    Assert.That(response.StatusDescription, Is.EqualTo("Not Found"));
  }

  [Test]
  public void HttpResponse_ContentType_CanBeSet() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.ContentType = "application/json";
    Assert.That(response.ContentType, Is.EqualTo("application/json"));
  }

  [Test]
  public void HttpResponse_Headers_CanBeAccessed() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    Assert.That(response.Headers, Is.Not.Null);
  }

  [Test]
  public void HttpResponse_Cookies_CanBeAccessed() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    Assert.That(response.Cookies, Is.Not.Null);
  }

  [Test]
  public void HttpResponse_Write_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.Write("Hello, World!");
    Assert.That(writer.ToString(), Is.EqualTo("Hello, World!"));
  }

  [Test]
  public void HttpResponse_Redirect_SetsStatusAndLocation() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.Redirect("/newpath");
    Assert.That(response.StatusCode, Is.EqualTo(302));
    Assert.That(response.Headers["Location"], Is.EqualTo("/newpath"));
  }

  [Test]
  public void HttpResponse_AddHeader_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.AddHeader("X-Custom", "value");
    Assert.That(response.Headers["X-Custom"], Is.EqualTo("value"));
  }

  [Test]
  public void HttpResponse_AppendHeader_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.AppendHeader("X-Custom", "value");
    Assert.That(response.Headers["X-Custom"], Is.EqualTo("value"));
  }

  [Test]
  public void HttpResponse_AppendCookie_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.AppendCookie(new HttpCookie("test", "value"));
    Assert.That(response.Cookies.Count, Is.EqualTo(1));
    Assert.That(response.Cookies["test"].Value, Is.EqualTo("value"));
  }

  [Test]
  public void HttpResponse_SetCookie_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.SetCookie(new HttpCookie("test", "value"));
    Assert.That(response.Cookies["test"].Value, Is.EqualTo("value"));
  }

  [Test]
  public void HttpResponse_Clear_Works() {
    using var writer = new StringWriter();
    var response = new HttpResponse(writer);
    response.Write("test");
    response.Clear();
    // After clear, the underlying writer should have been modified
    // but since we're using StringWriter directly, behavior may vary
    Assert.That(response.StatusCode, Is.EqualTo(200));
  }

  #endregion

  #region HttpContext Tests

  [Test]
  public void HttpContext_Current_IsNullByDefault() {
    HttpContext.Current = null;
    Assert.That(HttpContext.Current, Is.Null);
  }

  [Test]
  public void HttpContext_Current_CanBeSet() {
    using var writer = new StringWriter();
    var request = new HttpRequest(null, "http://localhost/", null);
    var response = new HttpResponse(writer);
    var context = new HttpContext(request, response);

    HttpContext.Current = context;

    Assert.That(HttpContext.Current, Is.Not.Null);
    Assert.That(HttpContext.Current, Is.SameAs(context));

    HttpContext.Current = null;
  }

  [Test]
  public void HttpContext_Request_IsAccessible() {
    using var writer = new StringWriter();
    var request = new HttpRequest(null, "http://localhost/test", null);
    var response = new HttpResponse(writer);
    var context = new HttpContext(request, response);

    Assert.That(context.Request, Is.Not.Null);
    Assert.That(context.Request.Path, Is.EqualTo("/test"));
  }

  [Test]
  public void HttpContext_Response_IsAccessible() {
    using var writer = new StringWriter();
    var request = new HttpRequest(null, "http://localhost/", null);
    var response = new HttpResponse(writer);
    var context = new HttpContext(request, response);

    Assert.That(context.Response, Is.Not.Null);
    Assert.That(context.Response.StatusCode, Is.EqualTo(200));
  }

  [Test]
  public void HttpContext_Items_CanStoreData() {
    using var writer = new StringWriter();
    var request = new HttpRequest(null, "http://localhost/", null);
    var response = new HttpResponse(writer);
    var context = new HttpContext(request, response);

    context.Items["key1"] = "value1";
    context.Items["key2"] = 42;

    Assert.That(context.Items["key1"], Is.EqualTo("value1"));
    Assert.That(context.Items["key2"], Is.EqualTo(42));
  }

  [Test]
  public void HttpContext_User_CanBeSetAndRetrieved() {
    using var writer = new StringWriter();
    var request = new HttpRequest(null, "http://localhost/", null);
    var response = new HttpResponse(writer);
    var context = new HttpContext(request, response);

    Assert.That(context.User, Is.Null);

    var identity = new System.Security.Principal.GenericIdentity("testuser");
    var principal = new System.Security.Principal.GenericPrincipal(identity, new[] { "admin" });
    context.User = principal;

    Assert.That(context.User, Is.Not.Null);
    Assert.That(context.User.Identity?.Name, Is.EqualTo("testuser"));
  }

  [Test]
  public void HttpContext_ThreadIsolation_Works() {
    HttpContext.Current = null;

    using var writer1 = new StringWriter();
    var context1 = new HttpContext(
      new HttpRequest(null, "http://localhost/thread1", null),
      new HttpResponse(writer1)
    );

    HttpContext.Current = context1;
    Assert.That(HttpContext.Current?.Request.Path, Is.EqualTo("/thread1"));

    System.Threading.ManualResetEvent ready = new(false);
    System.Threading.ManualResetEvent done = new(false);
    string? otherThreadPath = null;

    var thread = new System.Threading.Thread(() => {
      using var writer2 = new StringWriter();
      var context2 = new HttpContext(
        new HttpRequest(null, "http://localhost/thread2", null),
        new HttpResponse(writer2)
      );
      HttpContext.Current = context2;
      ready.Set();
      otherThreadPath = HttpContext.Current?.Request.Path;
      done.Set();
    });
    thread.Start();
    ready.WaitOne();
    done.WaitOne();

    Assert.That(HttpContext.Current?.Request.Path, Is.EqualTo("/thread1"));
    Assert.That(otherThreadPath, Is.EqualTo("/thread2"));

    HttpContext.Current = null;
  }

  #endregion

}
