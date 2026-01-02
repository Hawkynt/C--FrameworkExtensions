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
using System.Threading;
using NUnit.Framework;

namespace Backports.Tests;

[TestFixture]
[Category("Unit")]
[Category("Backports")]
[Category("SpinWait")]
public class SpinWaitTests {

  #region Count Property

  [Test]
  [Category("HappyPath")]
  public void SpinWait_Count_InitiallyZero() {
    SpinWait spinner = default;
    Assert.That(spinner.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_Count_IncrementsAfterSpinOnce() {
    SpinWait spinner = default;
    spinner.SpinOnce();
    Assert.That(spinner.Count, Is.EqualTo(1));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_Count_IncrementsMultipleTimes() {
    SpinWait spinner = default;
    for (var i = 0; i < 5; ++i)
      spinner.SpinOnce();
    Assert.That(spinner.Count, Is.EqualTo(5));
  }

  #endregion

  #region NextSpinWillYield Property

  [Test]
  [Category("HappyPath")]
  public void SpinWait_NextSpinWillYield_FalseInitiallyOnMultiProcessor() {
    if (Environment.ProcessorCount == 1)
      Assert.Ignore("Test requires multi-processor system");

    SpinWait spinner = default;
    Assert.That(spinner.NextSpinWillYield, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_NextSpinWillYield_TrueOnSingleProcessor() {
    if (Environment.ProcessorCount != 1)
      Assert.Ignore("Test requires single-processor system");

    SpinWait spinner = default;
    Assert.That(spinner.NextSpinWillYield, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_NextSpinWillYield_EventuallyBecomesTrue() {
    SpinWait spinner = default;
    var becameTrue = false;
    for (var i = 0; i < 100; ++i) {
      spinner.SpinOnce();
      if (spinner.NextSpinWillYield) {
        becameTrue = true;
        break;
      }
    }
    Assert.That(becameTrue, Is.True);
  }

  #endregion

  #region Reset Method

  [Test]
  [Category("HappyPath")]
  public void SpinWait_Reset_SetsCountToZero() {
    SpinWait spinner = default;
    spinner.SpinOnce();
    spinner.SpinOnce();
    spinner.Reset();
    Assert.That(spinner.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_Reset_AllowsReuseOfSpinner() {
    SpinWait spinner = default;
    for (var i = 0; i < 15; ++i)
      spinner.SpinOnce();
    spinner.Reset();
    Assert.That(spinner.Count, Is.EqualTo(0));
    spinner.SpinOnce();
    Assert.That(spinner.Count, Is.EqualTo(1));
  }

  #endregion

  #region SpinOnce Method

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinOnce_DoesNotThrow() {
    SpinWait spinner = default;
    Assert.DoesNotThrow(() => spinner.SpinOnce());
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinOnce_MultipleCallsDoNotThrow() {
    SpinWait spinner = default;
    Assert.DoesNotThrow(() => {
      for (var i = 0; i < 20; ++i)
        spinner.SpinOnce();
    });
  }

  #endregion

  #region SpinUntil Static Methods

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntil_WaitsUntilConditionMet() {
    var counter = 0;
    SpinWait.SpinUntil(() => {
      ++counter;
      return counter >= 5;
    });
    Assert.That(counter, Is.GreaterThanOrEqualTo(5));
  }

  [Test]
  [Category("Exception")]
  public void SpinWait_SpinUntil_NullCondition_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => SpinWait.SpinUntil(null!));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithTimeout_ReturnsTrueWhenConditionMet() {
    var result = SpinWait.SpinUntil(() => true, 1000);
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithTimeout_ReturnsFalseOnTimeout() {
    var result = SpinWait.SpinUntil(() => false, 50);
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithZeroTimeout_ChecksConditionOnce() {
    var callCount = 0;
    var result = SpinWait.SpinUntil(() => {
      ++callCount;
      return false;
    }, 0);
    Assert.That(result, Is.False);
    Assert.That(callCount, Is.EqualTo(1));
  }

  [Test]
  [Category("Exception")]
  public void SpinWait_SpinUntilWithTimeout_NullCondition_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => SpinWait.SpinUntil(null, 1000));
  }

  [Test]
  [Category("Exception")]
  public void SpinWait_SpinUntilWithTimeout_NegativeTimeout_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => SpinWait.SpinUntil(() => true, -2));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithTimeSpan_ReturnsTrueWhenConditionMet() {
    var result = SpinWait.SpinUntil(() => true, TimeSpan.FromMilliseconds(1000));
    Assert.That(result, Is.True);
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithTimeSpan_ReturnsFalseOnTimeout() {
    var result = SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(50));
    Assert.That(result, Is.False);
  }

  [Test]
  [Category("Exception")]
  public void SpinWait_SpinUntilWithTimeSpan_NegativeTimeSpan_ThrowsArgumentOutOfRangeException() {
    Assert.Throws<ArgumentOutOfRangeException>(() => SpinWait.SpinUntil(() => true, TimeSpan.FromMilliseconds(-2)));
  }

  [Test]
  [Category("HappyPath")]
  public void SpinWait_SpinUntilWithInfiniteTimeout_WaitsUntilConditionMet() {
    var counter = 0;
    var result = SpinWait.SpinUntil(() => {
      ++counter;
      return counter >= 3;
    }, Timeout.Infinite);
    Assert.That(result, Is.True);
    Assert.That(counter, Is.GreaterThanOrEqualTo(3));
  }

  #endregion

  #region Edge Cases

  [Test]
  [Category("EdgeCase")]
  public void SpinWait_DefaultValue_HasZeroCount() {
    SpinWait spinner = default;
    Assert.That(spinner.Count, Is.EqualTo(0));
  }

  [Test]
  [Category("EdgeCase")]
  public void SpinWait_MultipleResets_WorkCorrectly() {
    SpinWait spinner = default;
    for (var cycle = 0; cycle < 3; ++cycle) {
      for (var i = 0; i < 5; ++i)
        spinner.SpinOnce();
      Assert.That(spinner.Count, Is.EqualTo(5));
      spinner.Reset();
      Assert.That(spinner.Count, Is.EqualTo(0));
    }
  }

  #endregion

}
