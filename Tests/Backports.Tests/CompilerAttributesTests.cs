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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("CompilerAttributes")]
public class CompilerAttributesTests {

  #region CallerArgumentExpressionAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void CallerArgumentExpressionAttribute_Constructor_SetsParameterName() {
    var attribute = new CallerArgumentExpressionAttribute("value");
    Assert.That(attribute.ParameterName, Is.EqualTo("value"));
  }

  [Test]
  [Category("HappyPath")]
  public void CallerArgumentExpressionAttribute_InheritsFromAttribute() {
    var attribute = new CallerArgumentExpressionAttribute("test");
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void CallerArgumentExpressionAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(CallerArgumentExpressionAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(AttributeTargets.Parameter));
  }

  [Test]
  [Category("EdgeCase")]
  public void CallerArgumentExpressionAttribute_WithEmptyString_SetsEmptyParameterName() {
    var attribute = new CallerArgumentExpressionAttribute(string.Empty);
    Assert.That(attribute.ParameterName, Is.EqualTo(string.Empty));
  }

  #endregion

  #region DoesNotReturnAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void DoesNotReturnAttribute_InheritsFromAttribute() {
    var attribute = new DoesNotReturnAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void DoesNotReturnAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(DoesNotReturnAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(AttributeTargets.Method));
  }

  #endregion

  #region NotNullAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void NotNullAttribute_InheritsFromAttribute() {
    var attribute = new NotNullAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void NotNullAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(NotNullAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    var expectedTargets = AttributeTargets.Field | AttributeTargets.Parameter |
                          AttributeTargets.Property | AttributeTargets.ReturnValue;
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region NotNullWhenAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void NotNullWhenAttribute_Constructor_WithTrue_SetsReturnValue() {
    var attribute = new NotNullWhenAttribute(true);
    Assert.That(attribute.ReturnValue, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void NotNullWhenAttribute_Constructor_WithFalse_SetsReturnValue() {
    var attribute = new NotNullWhenAttribute(false);
    Assert.That(attribute.ReturnValue, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void NotNullWhenAttribute_InheritsFromAttribute() {
    var attribute = new NotNullWhenAttribute(true);
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void NotNullWhenAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(NotNullWhenAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(AttributeTargets.Parameter));
  }

  #endregion

  #region ExperimentalAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void ExperimentalAttribute_Constructor_SetsDiagnosticId() {
    var attribute = new ExperimentalAttribute("DIAG001");
    Assert.That(attribute.DiagnosticId, Is.EqualTo("DIAG001"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExperimentalAttribute_UrlFormat_DefaultsToNull() {
    var attribute = new ExperimentalAttribute("DIAG001");
    Assert.That(attribute.UrlFormat, Is.Null);
  }

  [Test]
  [Category("HappyPath")]
  public void ExperimentalAttribute_UrlFormat_CanBeSet() {
    var attribute = new ExperimentalAttribute("DIAG001") {
      UrlFormat = "https://example.com/diag/{0}"
    };
    Assert.That(attribute.UrlFormat, Is.EqualTo("https://example.com/diag/{0}"));
  }

  [Test]
  [Category("HappyPath")]
  public void ExperimentalAttribute_InheritsFromAttribute() {
    var attribute = new ExperimentalAttribute("TEST");
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void ExperimentalAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(ExperimentalAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);

    var expectedTargets = AttributeTargets.Assembly | AttributeTargets.Module |
                          AttributeTargets.Class | AttributeTargets.Struct |
                          AttributeTargets.Enum | AttributeTargets.Constructor |
                          AttributeTargets.Method | AttributeTargets.Property |
                          AttributeTargets.Field | AttributeTargets.Event |
                          AttributeTargets.Interface | AttributeTargets.Delegate;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region StackTraceHiddenAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void StackTraceHiddenAttribute_InheritsFromAttribute() {
    var attribute = new StackTraceHiddenAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void StackTraceHiddenAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(StackTraceHiddenAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);

    var expectedTargets = AttributeTargets.Class | AttributeTargets.Struct |
                          AttributeTargets.Constructor | AttributeTargets.Method;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region DisallowNullAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void DisallowNullAttribute_InheritsFromAttribute() {
    var attribute = new DisallowNullAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void DisallowNullAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(DisallowNullAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);

    var expectedTargets = AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter;
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(expectedTargets));
  }

  [Test]
  [Category("HappyPath")]
  public void DisallowNullAttribute_Usage_MethodParameterCompiles() {
    Assert.DoesNotThrow(() => MethodWithDisallowNull("test"));
  }

  private static void MethodWithDisallowNull([DisallowNull] string value) {
    _ = value;
  }

  #endregion

  #region CollectionBuilderAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void CollectionBuilderAttribute_Constructor_SetsProperties() {
    var attribute = new CollectionBuilderAttribute(typeof(string), "Create");
    Assert.That(attribute.BuilderType, Is.EqualTo(typeof(string)));
    Assert.That(attribute.MethodName, Is.EqualTo("Create"));
  }

  [Test]
  [Category("HappyPath")]
  public void CollectionBuilderAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(CollectionBuilderAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.False);

    var expectedTargets = AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region InlineArrayAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void InlineArrayAttribute_Constructor_SetsLength() {
    var attribute = new InlineArrayAttribute(10);
    Assert.That(attribute.Length, Is.EqualTo(10));
  }

  [Test]
  [Category("HappyPath")]
  public void InlineArrayAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(InlineArrayAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.ValidOn, Is.EqualTo(AttributeTargets.Struct));
  }

  #endregion

  #region ModuleInitializerAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void ModuleInitializerAttribute_InheritsFromAttribute() {
    var attribute = new ModuleInitializerAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void ModuleInitializerAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(ModuleInitializerAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.False);
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(AttributeTargets.Method));
  }

  #endregion

  #region SkipLocalsInitAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void SkipLocalsInitAttribute_InheritsFromAttribute() {
    var attribute = new SkipLocalsInitAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void SkipLocalsInitAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(SkipLocalsInitAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);

    var expectedTargets = AttributeTargets.Module | AttributeTargets.Class |
                          AttributeTargets.Struct | AttributeTargets.Interface |
                          AttributeTargets.Constructor | AttributeTargets.Method |
                          AttributeTargets.Property | AttributeTargets.Event;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  [Test]
  [Category("HappyPath")]
  public void SkipLocalsInitAttribute_Usage_MethodCompiles() {
    Assert.DoesNotThrow(() => {
      var result = MethodWithSkipLocalsInit();
      Assert.That(result, Is.GreaterThanOrEqualTo(0));
    });
  }

  [SkipLocalsInit]
  private static int MethodWithSkipLocalsInit() {
    var buffer = new int[10];
    return buffer[0];
  }

  #endregion

  #region UnscopedRefAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void UnscopedRefAttribute_InheritsFromAttribute() {
    var attribute = new UnscopedRefAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void UnscopedRefAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(UnscopedRefAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);

    var expectedTargets = AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region OverloadResolutionPriorityAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void OverloadResolutionPriorityAttribute_Constructor_SetsPriority() {
    var attribute = new OverloadResolutionPriorityAttribute(5);
    Assert.That(attribute.Priority, Is.EqualTo(5));
  }

  [Test]
  [Category("HappyPath")]
  public void OverloadResolutionPriorityAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(OverloadResolutionPriorityAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.False);

    var expectedTargets = AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region RequiredMemberAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void RequiredMemberAttribute_InheritsFromAttribute() {
    var attribute = new RequiredMemberAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void RequiredMemberAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(RequiredMemberAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.False);

    var expectedTargets = AttributeTargets.Class | AttributeTargets.Struct |
                          AttributeTargets.Field | AttributeTargets.Property;
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(expectedTargets));
  }

  #endregion

  #region CompilerFeatureRequiredAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void CompilerFeatureRequiredAttribute_Constructor_SetsFeatureName() {
    var attribute = new CompilerFeatureRequiredAttribute("TestFeature");
    Assert.That(attribute.FeatureName, Is.EqualTo("TestFeature"));
  }

  [Test]
  [Category("HappyPath")]
  public void CompilerFeatureRequiredAttribute_IsOptional_DefaultsFalse() {
    var attribute = new CompilerFeatureRequiredAttribute("Test");
    Assert.That(attribute.IsOptional, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void CompilerFeatureRequiredAttribute_HasCorrectConstants() {
    Assert.That(CompilerFeatureRequiredAttribute.RefStructs, Is.EqualTo("RefStructs"));
    Assert.That(CompilerFeatureRequiredAttribute.RequiredMembers, Is.EqualTo("RequiredMembers"));
  }

  [Test]
  [Category("HappyPath")]
  public void CompilerFeatureRequiredAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(CompilerFeatureRequiredAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.True);
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(AttributeTargets.All));
  }

  #endregion

  #region SetsRequiredMembersAttribute Tests

  [Test]
  [Category("HappyPath")]
  public void SetsRequiredMembersAttribute_InheritsFromAttribute() {
    var attribute = new SetsRequiredMembersAttribute();
    Assert.That(attribute, Is.InstanceOf<Attribute>());
  }

  [Test]
  [Category("HappyPath")]
  public void SetsRequiredMembersAttribute_HasCorrectAttributeUsage() {
    var usageAttribute = typeof(SetsRequiredMembersAttribute)
      .GetCustomAttribute<AttributeUsageAttribute>();
    Assert.That(usageAttribute, Is.Not.Null);
    Assert.That(usageAttribute!.Inherited, Is.False);
    Assert.That(usageAttribute.AllowMultiple, Is.False);
    Assert.That(usageAttribute.ValidOn, Is.EqualTo(AttributeTargets.Constructor));
  }

  #endregion

}
