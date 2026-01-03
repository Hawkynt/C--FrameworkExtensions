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

// CallSite is available in net40+ via BCL, and in net20 via our polyfill.
// Since NUnit requires net35+, these tests run against the BCL implementation on net40+.
// They serve to verify expected behavior and document the API contract.
#if SUPPORTS_DYNAMIC

using System;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("Dynamic")]
[Category("CallSite")]
public class CallSiteTests {

  #region CallSite Creation

  [Test]
  [Category("HappyPath")]
  public void CallSite_Create_ReturnsNonNullInstance() {
    var binder = new TestBinder();
    var site = CallSite<Func<CallSite, object, object>>.Create(binder);
    Assert.That(site, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void CallSite_Create_HasBinder() {
    var binder = new TestBinder();
    var site = CallSite<Func<CallSite, object, object>>.Create(binder);
    Assert.That(site.Binder, Is.SameAs(binder));
  }

  [Test]
  [Category("HappyPath")]
  public void CallSite_Create_HasTarget() {
    var binder = new TestBinder();
    var site = CallSite<Func<CallSite, object, object>>.Create(binder);
    Assert.That(site.Target, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void CallSite_Create_HasUpdate() {
    var binder = new TestBinder();
    var site = CallSite<Func<CallSite, object, object>>.Create(binder);
    Assert.That(site.Update, Is.Not.Null);
  }

  #endregion

  #region CallSiteBinder

  [Test]
  [Category("HappyPath")]
  public void CallSiteBinder_UpdateLabel_IsNotNull() {
    var label = CallSiteBinder.UpdateLabel;
    Assert.That(label, Is.Not.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void CallSiteBinder_UpdateLabel_HasName() {
    var label = CallSiteBinder.UpdateLabel;
    Assert.That(label.Name, Is.Not.Null.And.Not.Empty);
  }

  #endregion

  #region Null Binder Behavior

  [Test]
  [Category("HappyPath")]
  public void CallSite_Create_NullBinder_BehaviorVariesByRuntime() {
    // BCL behavior differs:
    // - .NET Framework (net48): allows null binder at creation time
    // - .NET Core/.NET 5+: throws ArgumentNullException immediately
    try {
      var site = CallSite<Func<CallSite, object, object>>.Create(null);
      // If we get here, null binder was allowed (net48 behavior)
      Assert.That(site, Is.Not.Null);
      Assert.That(site.Binder, Is.Null);
    } catch (ArgumentNullException) {
      // Expected on .NET Core/.NET 5+ - just pass
      Assert.Pass("ArgumentNullException thrown as expected on this runtime");
    }
  }

  #endregion

  #region CallInfo

  [Test]
  [Category("HappyPath")]
  public void CallInfo_DefaultConstructor_HasZeroArguments() {
    var info = new CallInfo(0);
    Assert.That(info.ArgumentCount, Is.EqualTo(0));
    Assert.That(info.ArgumentNames.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void CallInfo_WithArguments_HasCorrectCount() {
    var info = new CallInfo(3);
    Assert.That(info.ArgumentCount, Is.EqualTo(3));
  }

  [Test]
  [Category("HappyPath")]
  public void CallInfo_WithNamedArguments_HasCorrectNames() {
    var info = new CallInfo(3, "first", "second", "third");
    Assert.That(info.ArgumentCount, Is.EqualTo(3));
    Assert.That(info.ArgumentNames.Count, Is.EqualTo(3));
    Assert.That(info.ArgumentNames[0], Is.EqualTo("first"));
    Assert.That(info.ArgumentNames[1], Is.EqualTo("second"));
    Assert.That(info.ArgumentNames[2], Is.EqualTo("third"));
  }

  [Test]
  [Category("HappyPath")]
  public void CallInfo_ArgumentNames_IsReadOnly() {
    var info = new CallInfo(2, "a", "b");
    Assert.That(info.ArgumentNames, Is.InstanceOf<ReadOnlyCollection<string>>());
  }

  #endregion

  #region Helper Classes

  private class TestBinder : CallSiteBinder {
    public override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel) =>
      Expression.Return(returnLabel, Expression.Constant(args[0]), typeof(object));
  }

  #endregion

}

#endif
