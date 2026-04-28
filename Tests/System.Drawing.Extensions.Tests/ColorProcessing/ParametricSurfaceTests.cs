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
using System.Collections.ObjectModel;
using System.Linq;
using Hawkynt.ColorProcessing;
using Hawkynt.ColorProcessing.Dithering;
using Hawkynt.ColorProcessing.Filtering;
using Hawkynt.ColorProcessing.Filtering.Filters;
using Hawkynt.ColorProcessing.Quantization;
using NUnit.Framework;

namespace System.Drawing.Tests.ColorProcessing;

/// <summary>
/// Verifies the additive parameter surface across filters, ditherers and quantizers.
/// </summary>
/// <remarks>
/// <para>
/// Two invariants are checked for every parametric variant:
/// </para>
/// <list type="number">
///   <item>The descriptor exposes a non-empty <see cref="ParameterDescriptor"/> list.</item>
///   <item>Building with default values returns the correct algorithm type — i.e. the
///         backwards-compat path through <c>CreateWith(null)</c> works without throwing.</item>
/// </list>
/// <para>
/// Output byte-equality of defaults vs. fixed entries is covered by the goldens; here we
/// confirm at the registry level that the parametric default factory equals the fixed
/// factory by type and that two different parameter values yield two different
/// configurations (sanity check).
/// </para>
/// </remarks>
[TestFixture]
[Category("Unit")]
[Category("ColorProcessing")]
[Category("ParametricSurface")]
public class ParametricSurfaceTests {

  /// <summary>net35-friendly wrapper: <see cref="Dictionary{TKey,TValue}"/> does not implement
  /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> on legacy frameworks.</summary>
  private static IReadOnlyDictionary<string, object?> Vals(params (string k, object? v)[] entries) {
    var d = new Dictionary<string, object?>();
    foreach (var e in entries) d[e.k] = e.v;
    return new ReadOnlyDictionary<string, object?>(d);
  }

  [Test]
  public void ParameterDescriptor_Construction_Validates() {
    Assert.Throws<ArgumentException>(() => new ParameterDescriptor("", typeof(int), 0));
    Assert.Throws<ArgumentNullException>(() => new ParameterDescriptor("x", null!, 0));

    var d = ParameterDescriptor.Int("size", 8, 2, 32);
    Assert.That(d.Name, Is.EqualTo("size"));
    Assert.That(d.Type, Is.EqualTo(typeof(int)));
    Assert.That(d.DefaultValue, Is.EqualTo(8));
    Assert.That(d.MinValue, Is.EqualTo(2));
    Assert.That(d.MaxValue, Is.EqualTo(32));
  }

  [Test]
  public void ParameterDescriptor_ChoiceFactory_PopulatesAllowedValues() {
    var d = ParameterDescriptor.Choice("size", 8, 2, 4, 8, 16, 32);
    Assert.That(d.AllowedValues.Count, Is.EqualTo(5));
    Assert.That(d.AllowedValues, Is.EquivalentTo(new object[] { 2, 4, 8, 16, 32 }));
  }

  [Test]
  public void ParameterMetadata_UnknownKey_ReturnsEmpty() {
    var p = ParameterMetadata.GetParameters("definitely-not-registered");
    Assert.That(p, Is.Not.Null);
    Assert.That(p.Count, Is.EqualTo(0));
    Assert.That(ParameterMetadata.GetBuilder("definitely-not-registered"), Is.Null);
  }

  // -- Ditherers -----------------------------------------------------------------------

  [TestCase("Bayer.Parametric")]
  [TestCase("VoidAndCluster.Parametric")]
  [TestCase("ClusterDot.Parametric")]
  [TestCase("BlueNoise.Parametric")]
  [TestCase("FloydSteinberg.Parametric")]
  [TestCase("Stucki.Parametric")]
  [TestCase("Burkes.Parametric")]
  [TestCase("JarvisJudiceNinke.Parametric")]
  public void Ditherer_ParametricVariant_RegistersWithMetadata(string key) {
    // Touch the registry so static ctors fire.
    _ = DithererRegistry.All.ToArray();

    var p = ParameterMetadata.GetParameters(key);
    Assert.That(p.Count, Is.GreaterThan(0), $"Expected non-empty parameter list for '{key}'.");
    var builder = ParameterMetadata.GetBuilder(key);
    Assert.That(builder, Is.Not.Null, $"Expected builder for '{key}'.");

    var def = builder!(Vals());
    Assert.That(def, Is.Not.Null);
    Assert.That(def, Is.InstanceOf<IDitherer>());
  }

  [Test]
  public void Ditherer_BayerParametric_DifferentSizes_ProduceDifferentMatrixSizes() {
    _ = DithererRegistry.All.ToArray();
    var b = ParameterMetadata.GetBuilder("Bayer.Parametric")!;
    var b4 = (IDitherer)b(Vals(("size", 4)));
    var b16 = (IDitherer)b(Vals(("size", 16)));
    Assert.That(b4, Is.Not.SameAs(b16));
    if (b4 is OrderedDitherer od4 && b16 is OrderedDitherer od16)
      Assert.That(od4.MatrixSize, Is.Not.EqualTo(od16.MatrixSize));
  }

  [Test]
  public void Ditherer_FloydSteinbergParametric_SerpentineFlag_FlipsType() {
    _ = DithererRegistry.All.ToArray();
    var b = ParameterMetadata.GetBuilder("FloydSteinberg.Parametric")!;
    var linear = b(Vals(("serpentine", false)));
    var serp = b(Vals(("serpentine", true)));
    Assert.That(linear.GetType(), Is.Not.EqualTo(serp.GetType()),
      "Serpentine variant must surface a distinct type.");
  }

  [Test]
  public void DithererRegistry_FindByName_ResolvesParametricVariants() {
    var d = DithererRegistry.FindByName("Bayer (parametric)");
    Assert.That(d, Is.Not.Null);
    Assert.That(d!.Parameters.Count, Is.GreaterThan(0));
  }

  // -- Quantizers ----------------------------------------------------------------------

  [TestCase("KMeans.Parametric")]
  [TestCase("BisectingKMeans.Parametric")]
  [TestCase("MeanShift.Parametric")]
  [TestCase("Dbscan.Parametric")]
  [TestCase("NeuQuant.Parametric")]
  public void Quantizer_ParametricVariant_RegistersWithMetadata(string key) {
    _ = QuantizerRegistry.All.ToArray();

    var p = ParameterMetadata.GetParameters(key);
    Assert.That(p.Count, Is.GreaterThan(0), $"Expected non-empty parameter list for '{key}'.");
    var builder = ParameterMetadata.GetBuilder(key);
    Assert.That(builder, Is.Not.Null, $"Expected builder for '{key}'.");

    var def = builder!(Vals());
    Assert.That(def, Is.Not.Null);
    Assert.That(def, Is.InstanceOf<IQuantizer>());
  }

  [Test]
  public void Quantizer_KMeansParametric_DifferentIterations_RoundTripIntoStruct() {
    _ = QuantizerRegistry.All.ToArray();
    var b = ParameterMetadata.GetBuilder("KMeans.Parametric")!;
    var slow = (KMeansQuantizer)b(Vals(("maxIterations", 50)));
    var slower = (KMeansQuantizer)b(Vals(("maxIterations", 200)));
    Assert.That(slow.MaxIterations, Is.EqualTo(50));
    Assert.That(slower.MaxIterations, Is.EqualTo(200));
  }

  [Test]
  public void Quantizer_DbscanParametric_DifferentEpsilon_RoundTrips() {
    _ = QuantizerRegistry.All.ToArray();
    var b = ParameterMetadata.GetBuilder("Dbscan.Parametric")!;
    var a = (DbscanQuantizer)b(Vals(("epsilon", 0.05f), ("minPoints", 10)));
    Assert.That(a.Epsilon, Is.EqualTo(0.05f));
    Assert.That(a.MinPoints, Is.EqualTo(10));
  }

  [Test]
  public void QuantizerRegistry_FindByName_ResolvesParametricVariants() {
    var q = QuantizerRegistry.FindByName("K-Means (parametric)");
    Assert.That(q, Is.Not.Null);
    Assert.That(q!.Parameters.Count, Is.GreaterThan(0));
  }

  // -- Filters -------------------------------------------------------------------------

  [TestCase("GaussianBlur.Parametric")]
  [TestCase("Bilateral.Parametric")]
  [TestCase("UnsharpMask.Parametric")]
  [TestCase("Median.Parametric")]
  [TestCase("Erode.Parametric")]
  [TestCase("Dilate.Parametric")]
  public void Filter_ParametricVariant_RegistersWithMetadata(string key) {
    _ = FilterRegistry.All.ToArray();

    var p = ParameterMetadata.GetParameters(key);
    Assert.That(p.Count, Is.GreaterThan(0), $"Expected non-empty parameter list for '{key}'.");
    var builder = ParameterMetadata.GetBuilder(key);
    Assert.That(builder, Is.Not.Null, $"Expected builder for '{key}'.");

    var def = builder!(Vals());
    Assert.That(def, Is.Not.Null);
    Assert.That(def, Is.InstanceOf<IPixelFilter>());
  }

  [Test]
  public void Filter_GaussianBlurParametric_DifferentRadii_RoundTrip() {
    _ = FilterRegistry.All.ToArray();
    var b = ParameterMetadata.GetBuilder("GaussianBlur.Parametric")!;
    var small = b(Vals(("radiusX", 1), ("radiusY", 1)));
    var big = b(Vals(("radiusX", 5), ("radiusY", 5)));
    Assert.That(small, Is.InstanceOf<GaussianBlur>());
    Assert.That(big, Is.InstanceOf<GaussianBlur>());
    // Different radii yield different UsesFrameAccess
    Assert.That(((GaussianBlur)small).UsesFrameAccess, Is.False);
    Assert.That(((GaussianBlur)big).UsesFrameAccess, Is.True);
  }

  [Test]
  public void FilterRegistry_FindByName_ResolvesParametricVariants() {
    var f = FilterRegistry.FindByName("GaussianBlur (parametric)");
    Assert.That(f, Is.Not.Null);
    Assert.That(f!.Parameters.Count, Is.GreaterThan(0));
  }

  // -- Backward-compat invariants ------------------------------------------------------

  [Test]
  public void Registry_AllExistingFixedDefaultEntries_StillRegister() {
    // Sanity baseline: every well-known fixed entry that was present before this work
    // remains discoverable.
    Assert.That(DithererRegistry.FindByName("ErrorDiffusion_FloydSteinberg"), Is.Not.Null);
    // Bayer32x32Ditherer exposes a static Instance property of type IDitherer; the source
    // generator registers it as "Bayer32x32_Instance"; the reflection fallback uses the same naming.
    Assert.That(DithererRegistry.FindByName("Bayer32x32_Instance"), Is.Not.Null);
    Assert.That(QuantizerRegistry.FindByName("Octree"), Is.Not.Null);
    Assert.That(QuantizerRegistry.FindByName("K-Means"), Is.Not.Null);
    Assert.That(FilterRegistry.FindByName("GaussianBlur"), Is.Not.Null);
    Assert.That(FilterRegistry.FindByName("BilateralFilter"), Is.Not.Null);
  }

  [Test]
  public void DithererDescriptor_Default_HasEmptyParameters() {
    var d = DithererRegistry.FindByName("ErrorDiffusion_FloydSteinberg");
    Assert.That(d, Is.Not.Null);
    Assert.That(d!.Parameters.Count, Is.EqualTo(0));
    Assert.That(d.ParameterKey, Is.Null);
  }

  [Test]
  public void DithererDescriptor_CreateWith_NullDictionary_FallsBackToDefault() {
    var d = DithererRegistry.FindByName("Bayer (parametric)");
    Assert.That(d, Is.Not.Null);
    var inst = d!.CreateWith(null);
    Assert.That(inst, Is.Not.Null);
    Assert.That(inst, Is.InstanceOf<IDitherer>());
  }
}
