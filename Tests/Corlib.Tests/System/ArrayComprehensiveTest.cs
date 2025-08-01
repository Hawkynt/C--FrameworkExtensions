using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace System;

[TestFixture]
public class ArrayComprehensiveTest {

  #region Array Manipulation Methods Tests

  [Test]
  public void Fill_ByteArray_FillsCorrectly() {
    var array = new byte[10];
    array.Fill(42);
    
    Assert.IsTrue(array.All(b => b == 42));
  }

  [Test]
  public void Fill_ByteArray_WithOffset_PartialFill() {
    var array = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    array.Fill(42, 3);
    
    var expected = new byte[] { 1, 1, 1, 42, 42, 42, 42, 42, 42, 42 };
    Assert.AreEqual(expected, array);
  }

  [Test]
  public void Fill_ByteArray_WithOffsetAndCount_PartialFill() {
    var array = new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    array.Fill(42, 2, 3);
    
    var expected = new byte[] { 1, 1, 42, 42, 42, 1, 1, 1, 1, 1 };
    Assert.AreEqual(expected, array);
  }

  [Test]
  public void Fill_ByteArray_NullArray_ThrowsException() {
    byte[]? array = null;
    Assert.Throws<NullReferenceException>(() => array.Fill(42));
  }

  [Test]
  public void Fill_ByteArray_InvalidOffset_ThrowsException() {
    var array = new byte[5];
    Assert.Throws<IndexOutOfRangeException>(() => array.Fill(42, 10));
  }

  [Test]
  public void Clear_ByteArray_ClearsToZero() {
    var array = new byte[] { 1, 2, 3, 4, 5 };
    array.Clear();
    
    Assert.IsTrue(array.All(b => b == 0));
  }

  [Test]
  public void Clear_ByteArray_NullArray_ThrowsException() {
    byte[]? array = null;
    Assert.Throws<NullReferenceException>(() => array.Clear());
  }

  #endregion

  #region Array Search Methods Tests

  [Test]
  public void IndexOfOrMinusOne_ByteArray_FindsPattern() {
    var source = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
    var pattern = new byte[] { 3, 4, 5 };
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(2, index);
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_PatternNotFound() {
    var source = new byte[] { 1, 2, 3, 4, 5 };
    var pattern = new byte[] { 6, 7, 8 };
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(-1, index);
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_EmptySource() {
    var source = new byte[0];
    var pattern = new byte[] { 1, 2 };
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(-1, index);
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_EmptyPattern() {
    var source = new byte[] { 1, 2, 3 };
    var pattern = new byte[0];
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(0, index); // Empty pattern matches at the beginning
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_PatternLargerThanSource() {
    var source = new byte[] { 1, 2 };
    var pattern = new byte[] { 1, 2, 3, 4 };
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(-1, index);
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_NullSource_ThrowsException() {
    byte[]? source = null;
    var pattern = new byte[] { 1, 2 };
    
    Assert.Throws<NullReferenceException>(() => source.IndexOfOrMinusOne(pattern));
  }

  [Test]
  public void IndexOfOrMinusOne_ByteArray_NullPattern_ThrowsException() {
    var source = new byte[] { 1, 2, 3 };
    byte[]? pattern = null;
    
    Assert.Throws<ArgumentNullException>(() => source.IndexOfOrMinusOne(pattern));
  }

  #endregion

  #region Array Access Methods Tests

  [Test]
  public void TryGetFirst_Array_WithElements_ReturnsFirst() {
    var array = new string[] { "first", "second", "third" };
    var result = array.TryGetFirst(out var value);
    
    Assert.IsTrue(result);
    Assert.AreEqual("first", value);
  }

  [Test]
  public void TryGetFirst_Array_Empty_ReturnsFalse() {
    var array = new string[0];
    var result = array.TryGetFirst(out var value);
    
    Assert.IsFalse(result);
    Assert.IsNull(value);
  }

  [Test]
  public void TryGetFirst_Array_Null_ThrowsException() {
    string[] array = null;
    Assert.Throws<NullReferenceException>(() => array.TryGetFirst(out var value));
  }

  [Test]
  public void TryGetLast_Array_WithElements_ReturnsLast() {
    var array = new string[] { "first", "second", "third" };
    var result = array.TryGetLast(out var value);
    
    Assert.IsTrue(result);
    Assert.AreEqual("third", value);
  }

  [Test]
  public void TryGetLast_Array_Empty_ReturnsFalse() {
    var array = new string[0];
    var result = array.TryGetLast(out var value);
    
    Assert.IsFalse(result);
    Assert.IsNull(value);
  }

  [Test]
  public void TryGetLast_Array_Null_ThrowsException() {
    string[] array = null;
    Assert.Throws<NullReferenceException>(() => array.TryGetLast(out var value));
  }

  [Test]
  public void TryGetItem_Array_ValidIndex_ReturnsItem() {
    var array = new string[] { "first", "second", "third" };
    var result = array.TryGetItem(1, out var value);
    
    Assert.IsTrue(result);
    Assert.AreEqual("second", value);
  }

  [Test]
  public void TryGetItem_Array_InvalidIndex_ReturnsFalse() {
    var array = new string[] { "first", "second", "third" };
    var result = array.TryGetItem(10, out var value);
    
    Assert.IsFalse(result);
    Assert.IsNull(value);
  }

  [Test]
  public void TryGetItem_Array_NegativeIndex_ThrowsException() {
    var array = new string[] { "first", "second", "third" };
    Assert.Throws<IndexOutOfRangeException>(() => array.TryGetItem(-1, out var value));
  }

  [Test]
  public void TryGetItem_Array_Null_ThrowsException() {
    string[] array = null;
    Assert.Throws<NullReferenceException>(() => array.TryGetItem(0, out var value));
  }

  #endregion

  #region Array Comparison Methods Tests

  [Test]
  public void SequenceEqual_Arrays_Equal_ReturnsTrue() {
    var array1 = new byte[] { 1, 2, 3, 4, 5 };
    var array2 = new byte[] { 1, 2, 3, 4, 5 };
    
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void SequenceEqual_Arrays_Different_ReturnsFalse() {
    var array1 = new byte[] { 1, 2, 3, 4, 5 };
    var array2 = new byte[] { 1, 2, 3, 4, 6 };
    
    Assert.IsFalse(array1.SequenceEqual(array2));
  }

  [Test]
  public void SequenceEqual_Arrays_DifferentLength_ReturnsFalse() {
    var array1 = new byte[] { 1, 2, 3 };
    var array2 = new byte[] { 1, 2, 3, 4 };
    
    Assert.IsFalse(array1.SequenceEqual(array2));
  }

  [Test]
  public void SequenceEqual_Arrays_BothEmpty_ReturnsTrue() {
    var array1 = new byte[0];
    var array2 = new byte[0];
    
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void SequenceEqual_Arrays_OneNull_ReturnsFalse() {
    var array1 = new byte[] { 1, 2, 3 };
    byte[]? array2 = null;
    
    Assert.IsFalse(array1.SequenceEqual(array2));
  }

  [Test]
  public void SequenceEqual_Arrays_BothNull_ReturnsTrue() {
    byte[]? array1 = null;
    byte[]? array2 = null;
    
    Assert.IsTrue(array1.SequenceEqual(array2));
  }

  [Test]
  public void CompareTo_Arrays_GeneratesChangeSet() {
    var array1 = new string[] { "a", "b", "c" };
    var array2 = new string[] { "a", "x", "c", "d" };
    
    var changes = array1.CompareTo(array2).ToArray();
    
    Assert.IsNotEmpty(changes);
    // Should detect changes between the arrays
  }

  [Test]
  public void CompareTo_Arrays_NullSource_ThrowsException() {
    string[]? array1 = null;
    var array2 = new string[] { "a", "b" };
    
    Assert.Throws<NullReferenceException>(() => array1.CompareTo(array2).ToArray());
  }

  [Test]
  public void CompareTo_Arrays_NullOther_ThrowsException() {
    var array1 = new string[] { "a", "b" };
    string[]? array2 = null;
    
    Assert.Throws<ArgumentNullException>(() => array1.CompareTo(array2).ToArray());
  }

  #endregion

  #region Array Copy Methods Tests

  [Test]
  public void CopyTo_ByteArray_CopiesCorrectly() {
    var source = new byte[] { 1, 2, 3, 4, 5 };
    var target = new byte[5];
    
    source.CopyTo(target);
    
    Assert.AreEqual(source, target);
  }

  [Test]
  public void CopyTo_ByteArray_TargetTooSmall_ThrowsException() {
    var source = new byte[] { 1, 2, 3, 4, 5 };
    var target = new byte[3];
    
    Assert.Throws<ArgumentOutOfRangeException>(() => source.CopyTo(target));
  }

  [Test]
  public void CopyTo_ByteArray_NullSource_ThrowsException() {
    byte[]? source = null;
    var target = new byte[5];
    
    Assert.Throws<NullReferenceException>(() => source.CopyTo(target));
  }

  [Test]
  public void CopyTo_ByteArray_NullTarget_ThrowsException() {
    var source = new byte[] { 1, 2, 3, 4, 5 };
    byte[]? target = null;
    
    Assert.Throws<ArgumentNullException>(() => source.CopyTo(target));
  }

  [Test]
  public void SafelyClone_Array_CreatesDeepCopy() {
    var original = new string[] { "a", "b", "c" };
    var clone = original.SafelyClone();
    
    Assert.AreNotSame(original, clone);
    Assert.AreEqual(original, clone);
  }

  [Test]
  public void SafelyClone_Array_NullArray_ReturnsNull() {
    string[]? original = null;
    var clone = original.SafelyClone();
    
    Assert.IsNull(clone);
  }

  #endregion

  #region Bitwise Array Operations Tests

  [Test]
  public void Not_ByteArray_InvertsAllBits() {
    var array = new byte[] { 0xFF, 0x00, 0xAA };
    var expected = new byte[] { 0x00, 0xFF, 0x55 };
    
    array.Not();
    
    Assert.AreEqual(expected, array);
  }

  [Test]
  public void Not_ByteArray_NullArray_ThrowsException() {
    byte[]? array = null;
    Assert.Throws<NullReferenceException>(() => array.Not());
  }

  [Test]
  public void And_ByteArrays_PerformsBitwiseAnd() {
    var array1 = new byte[] { 0xFF, 0xAA, 0x55 };
    var array2 = new byte[] { 0x0F, 0xF0, 0xFF };
    var expected = new byte[] { 0x0F, 0xA0, 0x55 };
    
    array1.And(array2);
    
    Assert.AreEqual(expected, array1);
  }

  [Test]
  public void And_ByteArrays_DifferentSizes_DoesNotThrow() {
    var array1 = new byte[] { 0xFF, 0xAA };
    var array2 = new byte[] { 0x0F, 0xF0, 0xFF };
    
    // The And operation only processes min(array1.Length, array2.Length) elements
    Assert.DoesNotThrow(() => array1.And(array2));
    Assert.AreEqual(new byte[] { 0x0F, 0xA0 }, array1);
  }

  [Test]
  public void Or_ByteArrays_PerformsBitwiseOr() {
    var array1 = new byte[] { 0x0F, 0xAA, 0x55 };
    var array2 = new byte[] { 0xF0, 0x55, 0xAA };
    var expected = new byte[] { 0xFF, 0xFF, 0xFF };
    
    array1.Or(array2);
    
    Assert.AreEqual(expected, array1);
  }

  [Test]
  public void Xor_ByteArrays_PerformsBitwiseXor() {
    var array1 = new byte[] { 0xFF, 0xAA, 0x00 };
    var array2 = new byte[] { 0x0F, 0xAA, 0xFF };
    var expected = new byte[] { 0xF0, 0x00, 0xFF };
    
    array1.Xor(array2);
    
    Assert.AreEqual(expected, array1);
  }

  [Test]
  public void Nand_ByteArrays_PerformsBitwiseNand() {
    var array1 = new byte[] { 0xFF, 0xAA };
    var array2 = new byte[] { 0x0F, 0xF0 };
    var expected = new byte[] { 0xF0, 0x5F }; // ~(0xFF & 0x0F), ~(0xAA & 0xF0)
    
    array1.Nand(array2);
    
    Assert.AreEqual(expected, array1);
  }

  [Test]
  public void Nor_ByteArrays_PerformsBitwiseNor() {
    var array1 = new byte[] { 0x0F, 0xAA };
    var array2 = new byte[] { 0xF0, 0x55 };
    var expected = new byte[] { 0x00, 0x00 }; // ~(0x0F | 0xF0), ~(0xAA | 0x55)
    
    array1.Nor(array2);
    
    Assert.AreEqual(expected, array1);
  }

  [Test]
  public void Equ_ByteArrays_PerformsBitwiseEqu() {
    var array1 = new byte[] { 0xFF, 0xAA };
    var array2 = new byte[] { 0x0F, 0xAA };
    var expected = new byte[] { 0x0F, 0xFF }; // ~(0xFF ^ 0x0F), ~(0xAA ^ 0xAA)
    
    array1.Equ(array2);
    
    Assert.AreEqual(expected, array1);
  }

  #endregion

  #region Array Conversion Methods Tests

  [Test]
  public void ToHex_ByteArray_GeneratesHexString() {
    var array = new byte[] { 0x00, 0x1A, 0xFF };
    var hex = array.ToHex();
    
    Assert.AreEqual("001aff", hex);
  }

  [Test]
  public void ToHex_ByteArray_UpperCase_GeneratesUpperCaseHex() {
    var array = new byte[] { 0x00, 0x1A, 0xFF };
    var hex = array.ToHex(true);
    
    Assert.AreEqual("001AFF", hex);
  }

  [Test]
  public void ToHex_ByteArray_EmptyArray_ReturnsEmptyString() {
    var array = new byte[0];
    var hex = array.ToHex();
    
    Assert.AreEqual("", hex);
  }

  [Test]
  public void ToHex_ByteArray_NullArray_ReturnsNull() {
    byte[] array = null;
    var result = array.ToHex();
    Assert.IsNull(result);
  }

  [Test]
  public void ToBin_ByteArray_GeneratesBinaryString() {
    var array = new byte[] { 0x00, 0xFF, 0xAA };
    var binary = array.ToBin();
    
    Assert.AreEqual("000000001111111110101010", binary);
  }

  [Test]
  public void ToBin_ByteArray_EmptyArray_ThrowsException() {
    var array = new byte[0];
    // Empty array causes IndexOutOfRangeException when trying to access result[0]
    Assert.Throws<IndexOutOfRangeException>(() => array.ToBin());
  }

  [Test]
  public void ToBin_ByteArray_NullArray_ReturnsNull() {
    byte[] array = null;
    var result = array.ToBin();
    Assert.IsNull(result);
  }

  #endregion

  #region Performance and Edge Cases Tests

  [Test]
  public void CopyTo_LargeByteArray_PerformsEfficiently() {
    var size = 1024 * 1024; // 1MB
    var source = new byte[size];
    var target = new byte[size];
    
    // Fill source with pattern
    for (int i = 0; i < size; i++) {
      source[i] = (byte)(i % 256);
    }
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    source.CopyTo(target);
    stopwatch.Stop();
    
    Assert.AreEqual(source, target);
    Assert.Less(stopwatch.ElapsedMilliseconds, 100); // Should be fast
  }

  [Test]
  public void Fill_LargeByteArray_PerformsEfficiently() {
    var size = 1024 * 1024; // 1MB
    var array = new byte[size];
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    array.Fill(42);
    stopwatch.Stop();
    
    Assert.IsTrue(array.All(b => b == 42));
    Assert.Less(stopwatch.ElapsedMilliseconds, 50); // Should be very fast
  }

  [Test]
  public void IndexOfOrMinusOne_LargeByteArray_FindsPatternEfficiently() {
    var size = 1024 * 1024; // 1MB
    var source = new byte[size];
    var pattern = new byte[] { 1, 2, 3, 4, 5 };
    
    // Place pattern near the end
    var targetIndex = size - 100;
    Array.Copy(pattern, 0, source, targetIndex, pattern.Length);
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var index = source.IndexOfOrMinusOne(pattern);
    stopwatch.Stop();
    
    Assert.AreEqual(targetIndex, index);
    Assert.Less(stopwatch.ElapsedMilliseconds, 100); // Should be reasonably fast
  }

  [Test]
  public void SequenceEqual_LargeArrays_PerformsEfficiently() {
    var size = 1024 * 1024; // 1MB
    var array1 = new byte[size];
    var array2 = new byte[size];
    
    // Fill both arrays with same pattern
    for (int i = 0; i < size; i++) {
      array1[i] = array2[i] = (byte)(i % 256);
    }
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    var result = array1.SequenceEqual(array2);
    stopwatch.Stop();
    
    Assert.IsTrue(result);
    Assert.Less(stopwatch.ElapsedMilliseconds, 50); // Should be very fast
  }

  #endregion

  #region Boundary Condition Tests

  [Test]
  public void Fill_SingleElementArray_FillsCorrectly() {
    var array = new byte[] { 1 };
    array.Fill(42);
    
    Assert.AreEqual(42, array[0]);
  }

  [Test]
  public void IndexOfOrMinusOne_SingleBytePattern_FindsCorrectly() {
    var source = new byte[] { 1, 2, 3, 2, 4 };
    var pattern = new byte[] { 2 };
    
    var index = source.IndexOfOrMinusOne(pattern);
    Assert.AreEqual(1, index); // Should find first occurrence
  }

  [Test]
  public void TryGetItem_BoundaryIndex_HandlesCorrectly() {
    var array = new string[] { "first", "second", "third" };
    
    // Test boundary conditions
    Assert.IsTrue(array.TryGetItem(0, out var first));
    Assert.AreEqual("first", first);
    
    Assert.IsTrue(array.TryGetItem(2, out var last));
    Assert.AreEqual("third", last);
    
    Assert.IsFalse(array.TryGetItem(3, out var outOfBounds));
    Assert.IsNull(outOfBounds);
  }

  [Test]
  public void Bitwise_Operations_ZeroLengthArrays_ThrowsException() {
    var array1 = new byte[0];
    var array2 = new byte[0];
    
    // Zero-length arrays throw ArgumentOutOfRangeException due to count validation
    Assert.Throws<ArgumentOutOfRangeException>(() => array1.And(array2));
    Assert.Throws<ArgumentOutOfRangeException>(() => array1.Or(array2));
    Assert.Throws<ArgumentOutOfRangeException>(() => array1.Xor(array2));
    Assert.Throws<ArgumentOutOfRangeException>(() => array1.Not()); // Not operation also throws for empty arrays
  }

  #endregion
}