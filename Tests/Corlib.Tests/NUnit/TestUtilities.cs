using NUnit.Framework;
using System;

namespace Corlib.Tests.NUnit; 

internal class TestUtilities {
  public static void ExecuteTest<TResult>(Func<TResult> resultProvider, TResult expected, Type? exception) {
    if (exception == null)
      Assert.That(resultProvider(), Is.EqualTo(expected));
    else
      Assert.That(resultProvider, Throws.TypeOf(exception));
  }

}