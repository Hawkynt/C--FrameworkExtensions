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

namespace Hawkynt.ColorProcessing.SourceGenerators;

/// <summary>
/// Equatable key/value pair used in the incremental pipeline cache. <see cref="KeyValuePair{TKey,TValue}"/> does not
/// implement <see cref="System.IEquatable{T}"/>, which <see cref="EquatableArray{T}"/> requires.
/// </summary>
internal sealed record NamedArg(string Name, TypedValue Value);

/// <summary>
/// Immutable, equatable description of a type that carries one of the registry attributes.
/// Kept as a plain record with primitive fields so the incremental pipeline can cache it cheaply.
/// </summary>
internal sealed record RegistryCandidate(
  string FullyQualifiedTypeName,
  string FileHint,
  int SyntaxOrder,
  bool HasParameterlessCtor,
  bool HasSupportedScalesStaticProperty,
  bool ImplementsIRescaler,
  bool ImplementsIResampler,
  bool ImplementsIPixelFilter,
  bool ImplementsIDitherer,
  bool ImplementsIQuantizer,
  bool HasStaticIDithererProperties,
  bool HasStaticIQuantizerProperties,
  EquatableArray<string> StaticIDithererPropertyNames,
  EquatableArray<string> StaticIQuantizerPropertyNames,
  AttributeData Attribute
);

/// <summary>
/// Captured attribute constructor / named argument data in a cache-friendly shape.
/// </summary>
internal sealed record AttributeData(
  string AttributeFullyQualifiedName,
  EquatableArray<TypedValue> ConstructorArguments,
  EquatableArray<NamedArg> NamedArguments
);

/// <summary>
/// One attribute argument captured as a C# source literal (already serialised).
/// </summary>
/// <param name="SourceLiteral">e.g. <c>"\"Bicubic\""</c>, <c>"1981"</c>, <c>"Hawkynt.ColorProcessing.Resizing.ScalerCategory.Resampler"</c>, <c>"null"</c>.</param>
/// <param name="RawString">The underlying string value for non-string args this is null; for strings, the unescaped value (used for ordering/dedup).</param>
internal sealed record TypedValue(string SourceLiteral, string? RawString);
