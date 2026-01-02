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
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("ConditionalWeakTable")]
public class ConditionalWeakTableTests {

  #region Add Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_Add_AddsEntry() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    var value = new TestValue { Data = 42 };

    table.Add(key, value);

    Assert.That(table.TryGetValue(key, out var retrieved), Is.True);
    Assert.That(retrieved.Data, Is.EqualTo(42));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_Add_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.Add(null, new TestValue()));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_Add_DuplicateKey_ThrowsArgumentException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    table.Add(key, new TestValue { Data = 1 });
    Assert.Throws<ArgumentException>(() => table.Add(key, new TestValue { Data = 2 }));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_Add_NullValue_Allowed() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    table.Add(key, null);
    Assert.That(table.TryGetValue(key, out var retrieved), Is.True);
    Assert.That(retrieved, Is.Null);
  }

  #endregion

  #region TryAdd Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_TryAdd_ReturnsTrue_WhenKeyDoesNotExist() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    var value = new TestValue { Data = 42 };

    var result = table.TryAdd(key, value);

    Assert.That(result, Is.True);
    Assert.That(table.TryGetValue(key, out var retrieved), Is.True);
    Assert.That(retrieved.Data, Is.EqualTo(42));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_TryAdd_ReturnsFalse_WhenKeyExists() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    table.Add(key, new TestValue { Data = 1 });

    var result = table.TryAdd(key, new TestValue { Data = 2 });

    Assert.That(result, Is.False);
    Assert.That(table.TryGetValue(key, out var retrieved), Is.True);
    Assert.That(retrieved.Data, Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_TryAdd_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.TryAdd(null, new TestValue()));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_TryAdd_NullValue_Allowed() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();

    var result = table.TryAdd(key, null);

    Assert.That(result, Is.True);
    Assert.That(table.TryGetValue(key, out var retrieved), Is.True);
    Assert.That(retrieved, Is.Null);
  }

  #endregion

  #region TryGetValue Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_TryGetValue_ReturnsTrue_WhenKeyExists() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    var value = new TestValue { Data = 42 };
    table.Add(key, value);

    var result = table.TryGetValue(key, out var retrieved);

    Assert.That(result, Is.True);
    Assert.That(retrieved, Is.SameAs(value));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_TryGetValue_ReturnsFalse_WhenKeyDoesNotExist() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();

    var result = table.TryGetValue(key, out var retrieved);

    Assert.That(result, Is.False);
    Assert.That(retrieved, Is.Null);
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_TryGetValue_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.TryGetValue(null, out _));
  }

  #endregion

  #region Remove Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_Remove_ReturnsTrue_WhenKeyExists() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    table.Add(key, new TestValue());

    var result = table.Remove(key);

    Assert.That(result, Is.True);
    Assert.That(table.TryGetValue(key, out _), Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_Remove_ReturnsFalse_WhenKeyDoesNotExist() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();

    var result = table.Remove(key);

    Assert.That(result, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_Remove_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.Remove(null));
  }

  #endregion

  #region GetOrCreateValue Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_GetOrCreateValue_CreatesValue_WhenKeyDoesNotExist() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();

    var value = table.GetOrCreateValue(key);

    Assert.That(value, Is.Not.Null);
    Assert.That(value.Data, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_GetOrCreateValue_ReturnsExisting_WhenKeyExists() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    var original = new TestValue { Data = 42 };
    table.Add(key, original);

    var value = table.GetOrCreateValue(key);

    Assert.That(value, Is.SameAs(original));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_GetOrCreateValue_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.GetOrCreateValue(null));
  }

  #endregion

  #region GetValue Method

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_GetValue_CreatesValue_WhenKeyDoesNotExist() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();

    var value = table.GetValue(key, k => new TestValue { Data = 99 });

    Assert.That(value.Data, Is.EqualTo(99));
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_GetValue_ReturnsExisting_WhenKeyExists() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    var original = new TestValue { Data = 42 };
    table.Add(key, original);
    var factoryCalled = false;

    var value = table.GetValue(key, k => {
      factoryCalled = true;
      return new TestValue { Data = 99 };
    });

    Assert.That(value, Is.SameAs(original));
    Assert.That(factoryCalled, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_GetValue_FactoryReceivesKey() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    object receivedKey = null;

    table.GetValue(key, k => {
      receivedKey = k;
      return new TestValue();
    });

    Assert.That(receivedKey, Is.SameAs(key));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_GetValue_NullKey_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    Assert.Throws<ArgumentNullException>(() => table.GetValue(null, k => new TestValue()));
  }

  [Test]
  [Category("Exception")]
  public void ConditionalWeakTable_GetValue_NullFactory_ThrowsArgumentNullException() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var key = new object();
    Assert.Throws<ArgumentNullException>(() => table.GetValue(key, null));
  }

  #endregion

  #region Weak Reference Behavior

  [Test]
  [Category("Integration")]
  public void ConditionalWeakTable_EntryRemoved_WhenKeyCollected() {
    var table = new ConditionalWeakTable<object, TestValue>();
    WeakReference weakKey = null;

    AddEntry(table, out weakKey);

    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();

    Assert.That(weakKey.IsAlive, Is.False);
  }

  [MethodImpl(MethodImplOptions.NoInlining)]
  private static void AddEntry(ConditionalWeakTable<object, TestValue> table, out WeakReference weakKey) {
    var key = new object();
    weakKey = new WeakReference(key);
    table.Add(key, new TestValue { Data = 42 });
  }

  #endregion

  #region Reference Identity

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_UsesReferenceEquality() {
    var table = new ConditionalWeakTable<string, TestValue>();
    var key1 = new string('a', 5);
    var key2 = new string('a', 5);

    table.Add(key1, new TestValue { Data = 1 });
    table.Add(key2, new TestValue { Data = 2 });

    Assert.That(table.TryGetValue(key1, out var value1), Is.True);
    Assert.That(table.TryGetValue(key2, out var value2), Is.True);
    Assert.That(value1.Data, Is.EqualTo(1));
    Assert.That(value2.Data, Is.EqualTo(2));
  }

  #endregion

  #region Thread Safety

  [Test]
  [Category("Integration")]
  public void ConditionalWeakTable_ConcurrentAccess_IsThreadSafe() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var keys = new object[100];
    for (var i = 0; i < keys.Length; ++i)
      keys[i] = new object();

    var threads = new Thread[4];
    var errors = 0;

    for (var t = 0; t < threads.Length; ++t) {
      var threadIndex = t;
      threads[t] = new Thread(() => {
        try {
          for (var i = 0; i < 1000; ++i) {
            var keyIndex = (threadIndex * 25 + i) % keys.Length;
            var key = keys[keyIndex];
            table.GetValue(key, k => new TestValue { Data = keyIndex });
            table.TryGetValue(key, out _);
          }
        } catch {
          Interlocked.Increment(ref errors);
        }
      });
    }

    foreach (var thread in threads)
      thread.Start();
    foreach (var thread in threads)
      thread.Join();

    Assert.That(errors, Is.EqualTo(0));
  }

  #endregion

  #region Multiple Entries

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_MultipleEntries_AllAccessible() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var keys = new object[10];
    for (var i = 0; i < keys.Length; ++i) {
      keys[i] = new object();
      table.Add(keys[i], new TestValue { Data = i });
    }

    for (var i = 0; i < keys.Length; ++i) {
      Assert.That(table.TryGetValue(keys[i], out var value), Is.True);
      Assert.That(value.Data, Is.EqualTo(i));
    }
  }

  [Test]
  [Category("HappyPath")]
  public void ConditionalWeakTable_RemoveMiddle_OthersStillAccessible() {
    var table = new ConditionalWeakTable<object, TestValue>();
    var keys = new object[5];
    for (var i = 0; i < keys.Length; ++i) {
      keys[i] = new object();
      table.Add(keys[i], new TestValue { Data = i });
    }

    table.Remove(keys[2]);

    Assert.That(table.TryGetValue(keys[0], out _), Is.True);
    Assert.That(table.TryGetValue(keys[1], out _), Is.True);
    Assert.That(table.TryGetValue(keys[2], out _), Is.False);
    Assert.That(table.TryGetValue(keys[3], out _), Is.True);
    Assert.That(table.TryGetValue(keys[4], out _), Is.True);
  }

  #endregion

  #region Helper Types

  private class TestValue {
    public int Data { get; set; }
  }

  #endregion

}
