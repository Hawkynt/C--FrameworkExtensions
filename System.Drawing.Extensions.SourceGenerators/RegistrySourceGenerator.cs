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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Hawkynt.ColorProcessing.SourceGenerators;

/// <summary>
/// Emits compile-time registry population for the four Hawkynt ColorProcessing registries so
/// that runtime <c>Assembly.GetTypes()</c> scans are eliminated on trim/AOT targets.
/// </summary>
/// <remarks>
/// <para>
/// For each of <c>ScalerInfoAttribute</c>, <c>FilterInfoAttribute</c>, <c>DithererAttribute</c>
/// and <c>QuantizerAttribute</c>, this generator finds all types that carry the attribute in
/// the compilation, captures the attribute's constructor/named arguments, and emits a
/// <c>partial</c> augmentation of the corresponding registry that implements
/// <c>_CollectFromSourceGenerator(List&lt;TDescriptor&gt;)</c> by directly constructing each
/// descriptor — no <see cref="System.Reflection"/>, no <see cref="System.Activator"/>.
/// </para>
/// <para>
/// The registry classes keep a reflection-based fallback for old TFMs and for the case where
/// the generator did not run (belt and suspenders); the fallback is skipped whenever the
/// generator contributed at least one entry. De-duplication is on the concrete type symbol.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RegistrySourceGenerator : IIncrementalGenerator {

  private const string ScalerAttr = "Hawkynt.ColorProcessing.Resizing.ScalerInfoAttribute";
  private const string FilterAttr = "Hawkynt.ColorProcessing.Filtering.FilterInfoAttribute";
  private const string DithererAttr = "Hawkynt.ColorProcessing.Dithering.DithererAttribute";
  private const string QuantizerAttr = "Hawkynt.ColorProcessing.Quantization.QuantizerAttribute";

  /// <inheritdoc />
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    var scalerCandidates = this._CollectCandidates(context, ScalerAttr);
    var filterCandidates = this._CollectCandidates(context, FilterAttr);
    var dithererCandidates = this._CollectCandidates(context, DithererAttr);
    var quantizerCandidates = this._CollectCandidates(context, QuantizerAttr);

    context.RegisterSourceOutput(scalerCandidates.Collect(), (ctx, items) => Emitter.EmitScalers(ctx, items));
    context.RegisterSourceOutput(filterCandidates.Collect(), (ctx, items) => Emitter.EmitFilters(ctx, items));
    context.RegisterSourceOutput(dithererCandidates.Collect(), (ctx, items) => Emitter.EmitDitherers(ctx, items));
    context.RegisterSourceOutput(quantizerCandidates.Collect(), (ctx, items) => Emitter.EmitQuantizers(ctx, items));
  }

  private IncrementalValuesProvider<RegistryCandidate> _CollectCandidates(
    IncrementalGeneratorInitializationContext context,
    string attributeFqn)
    => context.SyntaxProvider.ForAttributeWithMetadataName(
      attributeFqn,
      predicate: static (node, _) => node is TypeDeclarationSyntax,
      transform: static (ctx, ct) => Transform(ctx, ct)
    ).Where(static c => c is not null)!;

  private static RegistryCandidate? Transform(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
    if (ctx.TargetSymbol is not INamedTypeSymbol type) return null;
    if (type.IsAbstract || type.IsStatic) return null;
    if (type.IsGenericType) return null;                          // exclude generics (open or bound) — requirements
    if (type.DeclaredAccessibility != Accessibility.Public) return null;
    if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct)) return null;

    // Require a usable public parameterless constructor (structs always have one).
    var hasParameterlessCtor = type.TypeKind == TypeKind.Struct
      || type.InstanceConstructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public);
    if (!hasParameterlessCtor && type.TypeKind == TypeKind.Class)
      return null;

    ct.ThrowIfCancellationRequested();

    var attr = ctx.Attributes[0];
    var attrData = CaptureAttribute(attr);

    // Inspect static members relevant to the various registries.
    var supportedScales = false;
    var dithererProps = new List<string>();
    var quantizerProps = new List<string>();

    foreach (var member in type.GetMembers()) {
      if (member is not IPropertySymbol prop || !prop.IsStatic || prop.DeclaredAccessibility != Accessibility.Public || prop.GetMethod is null) continue;
      var propTypeFqn = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
      if (prop.Name == "SupportedScales" && propTypeFqn is "global::Hawkynt.ColorProcessing.Resizing.ScaleFactor[]")
        supportedScales = true;
      else if (TypeImplements(prop.Type, "Hawkynt.ColorProcessing.IDitherer"))
        dithererProps.Add(prop.Name);
      else if (TypeImplements(prop.Type, "Hawkynt.ColorProcessing.IQuantizer"))
        quantizerProps.Add(prop.Name);
    }

    var file = ctx.TargetNode.SyntaxTree.FilePath ?? string.Empty;
    var span = ctx.TargetNode.SpanStart;

    return new RegistryCandidate(
      FullyQualifiedTypeName: type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
      FileHint: file,
      SyntaxOrder: span,
      HasParameterlessCtor: hasParameterlessCtor,
      HasSupportedScalesStaticProperty: supportedScales,
      ImplementsIRescaler: TypeImplements(type, "Hawkynt.ColorProcessing.Resizing.IRescaler"),
      ImplementsIResampler: TypeImplements(type, "Hawkynt.ColorProcessing.Resizing.IResampler"),
      ImplementsIPixelFilter: TypeImplements(type, "Hawkynt.ColorProcessing.Filtering.IPixelFilter"),
      ImplementsIDitherer: TypeImplements(type, "Hawkynt.ColorProcessing.IDitherer"),
      ImplementsIQuantizer: TypeImplements(type, "Hawkynt.ColorProcessing.IQuantizer"),
      HasStaticIDithererProperties: dithererProps.Count > 0,
      HasStaticIQuantizerProperties: quantizerProps.Count > 0,
      StaticIDithererPropertyNames: new EquatableArray<string>(dithererProps.ToArray()),
      StaticIQuantizerPropertyNames: new EquatableArray<string>(quantizerProps.ToArray()),
      Attribute: attrData
    );
  }

  private static bool TypeImplements(ITypeSymbol type, string interfaceFqn) {
    foreach (var i in type.AllInterfaces)
      if (i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::" + interfaceFqn)
        return true;
    return false;
  }

  private static AttributeData CaptureAttribute(Microsoft.CodeAnalysis.AttributeData attr) {
    var ctorArgs = new List<TypedValue>(attr.ConstructorArguments.Length);
    foreach (var arg in attr.ConstructorArguments)
      ctorArgs.Add(ToTypedValue(arg));

    var namedArgs = new List<NamedArg>(attr.NamedArguments.Length);
    foreach (var kv in attr.NamedArguments)
      namedArgs.Add(new NamedArg(kv.Key, ToTypedValue(kv.Value)));

    return new AttributeData(
      AttributeFullyQualifiedName: attr.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? string.Empty,
      ConstructorArguments: new EquatableArray<TypedValue>(ctorArgs.ToArray()),
      NamedArguments: new EquatableArray<NamedArg>(namedArgs.ToArray())
    );
  }

  private static TypedValue ToTypedValue(TypedConstant c) {
    if (c.IsNull) return new TypedValue("null", null);

    switch (c.Kind) {
      case TypedConstantKind.Enum: {
        var enumTypeFqn = c.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        // Resolve enum member name from value if possible.
        var enumType = c.Type as INamedTypeSymbol;
        var memberName = enumType?.GetMembers()
          .OfType<IFieldSymbol>()
          .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, c.Value))?
          .Name;
        return memberName is not null
          ? new TypedValue($"{enumTypeFqn}.{memberName}", null)
          : new TypedValue($"({enumTypeFqn})({c.Value})", null);
      }
      case TypedConstantKind.Primitive when c.Value is string s:
        return new TypedValue(EscapeStringLiteral(s), s);
      case TypedConstantKind.Primitive when c.Value is bool b:
        return new TypedValue(b ? "true" : "false", null);
      case TypedConstantKind.Primitive when c.Value is char ch:
        return new TypedValue("'" + (ch == '\'' ? "\\'" : ch.ToString()) + "'", null);
      case TypedConstantKind.Primitive:
        return new TypedValue(FormatPrimitive(c.Value), null);
      case TypedConstantKind.Type when c.Value is ITypeSymbol ts:
        return new TypedValue($"typeof({ts.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})", null);
      default:
        return new TypedValue(FormatPrimitive(c.Value), null);
    }
  }

  private static string FormatPrimitive(object? value) => value switch {
    null => "null",
    string s => EscapeStringLiteral(s),
    bool b => b ? "true" : "false",
    float f => f.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "f",
    double d => d.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "d",
    decimal m => m.ToString(System.Globalization.CultureInfo.InvariantCulture) + "m",
    _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "default"
  };

  internal static string EscapeStringLiteral(string? value) {
    if (value is null) return "null";
    var sb = new StringBuilder(value.Length + 2);
    sb.Append('"');
    foreach (var ch in value) {
      switch (ch) {
        case '\\': sb.Append("\\\\"); break;
        case '"':  sb.Append("\\\""); break;
        case '\r': sb.Append("\\r"); break;
        case '\n': sb.Append("\\n"); break;
        case '\t': sb.Append("\\t"); break;
        case '\0': sb.Append("\\0"); break;
        default:
          if (ch < 0x20) sb.Append($"\\u{(int)ch:X4}");
          else sb.Append(ch);
          break;
      }
    }
    sb.Append('"');
    return sb.ToString();
  }
}
