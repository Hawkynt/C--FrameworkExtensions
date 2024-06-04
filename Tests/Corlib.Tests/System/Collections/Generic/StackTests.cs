using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Corlib.Tests.System.Collections.Generic {
  
  [TestFixture]
  internal class StackTests {

    [Test]
    public static void ExchangeTest() {
      var stack = new Stack<int>();
      stack.Push(1);

      Assert.That(stack.ToArray(), Is.EquivalentTo(new[]{ 1 }));

      stack.Exchange(2);
      Assert.That(stack.ToArray(), Is.EquivalentTo(new[] { 2 }));

      Assert.That(()=>((Stack<int>)null!).Exchange(2), Throws.Exception.TypeOf<NullReferenceException>());

      stack.Pop();
      Assert.That(()=>stack.Exchange(2),Throws.InvalidOperationException);

      stack.Push(1);
      stack.Push(2);
      stack.Exchange(3);
      Assert.That(stack.ToArray(), Is.EquivalentTo(new[] { 3, 1 }));
    }

  }
}
