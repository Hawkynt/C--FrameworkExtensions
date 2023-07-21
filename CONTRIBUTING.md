# Contributing guidelines

## Foreword

These extensions are meant to be extended by everyone who is firm in [C#](https://dotnet.microsoft.com/en-us/learn/csharp).

I appreciate every idea making these classes 
* go faster
* use less memory
* have more throughput
* less worst/average-case-[complexity](https://en.wikipedia.org/wiki/Big_O_notation)
* fix things Microsoft has [overseen](https://softwareengineering.stackexchange.com/questions/296445/whats-the-use-of-any-in-a-c-list)
* or simply teaching them new tricks
  
However, this is not my daily business and I kindly ask you for some time to read through your pull-requests and issues.
Please make it as easy for me as you can by avoiding things like
* different code style
* hard-to-figure-out-stuff without meaningful comments
* huge changesets (try to push more often, that helps)
* endless discussions (I'm the captain and the captain is in charge of the spaceship)

## So, you are brave enough?

There are some guidelines for extensions which have proven one's worth:
* Every referenced assembly/package should have its own project/assembly
* Use folders for every part of the namespace
* Every file in there should have a name that is build like this: "**Type**.cs"
* The namespace in the files is always the same namespace as the original type is in
* The classname is always "**Type**Extensions". The class is always "internal/public static partial", thus allowing us to extend it further in a given project
  by adding another partial class with the same name
* All public methods must be static
* The first parameter of all "public" methods must be the type itself and is called "**@this**" or alternatively "*This*" (only for old legacy code)
* For extensions to static classes like "Math" or "Activator",
  there is no "This"-parameter
* Get a test for your contribution under "Tests" following the examples already in place
* Do not write [too many](https://en.wikipedia.org/wiki/Equivalence_partitioning) tests
* Do write [enough](https://en.wikipedia.org/wiki/Boundary-value_analysis) tests

## Are you keen on refactoring?

You can go on and refactor whatever you think is necessary to make the code more readable or adept to new .Net versions. 
However don't make the code slower or more memory-hungry during refactoring. 
Pay kind attention to details, escpecially all that compiler-sugar ([async](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/async)/[await](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/await), [yield](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield), [lambdas](https://medium.com/criteo-engineering/beware-lambda-captures-383efe3a4345), [Patterns](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns), [LINQ](https://www.youtube.com/watch?v=Dv_nsoEmC7s&list=PLzQZKn8ki7X1XhXSjaSQpRr4Am1uFK4fo)) and [boxing](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing).
C# is doing a lot under the hood which you only see when using [dotPeek](https://www.jetbrains.com/decompiler/), [dnSpy](https://github.com/dnSpy/dnSpy), [Reflector](https://www.red-gate.com/products/reflector/), [ILSpy](https://github.com/icsharpcode/ILSpy) or any other decompilation tool.
You should make yourself comfortable with the [difference](https://www.c-sharpcorner.com/article/stack-vs-heap-memory-c-sharp/) between [heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals) and [stack](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc) allocations, know the [large object heap](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap) and its size.

Ask if unsure and [learn](https://www.youtube.com/watch?v=Tb2Fx9qku_o) about [micro-optimizations](https://www.specbranch.com/posts/intro-to-micro-optimization/) and [why they are needed](https://medium.com/google-developers/the-truth-about-preventative-optimizations-ccebadfd3eb5).
If you really feel it, create benchmark code under "Tests" like the one already in place.

BTW: I am using JetBrains [ReSharper](https://www.jetbrains.com/resharper/) so don't wonder upon specific comments for it.

## Hardcore-Fixer?

Everyone can learn new things and nobody is perfect. 
Some things just don't work yet like I like them to be. 
If you gonna fix actions, code, tests or whatever just let me know.

## Code-Conventions

### Naming Style
* based loosely on Microsoft [guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
* using [camelCase](https://www.theserverside.com/answer/Pascal-case-vs-camel-case-Whats-the-difference) `changeTheWorld` and [PascalCase](https://www.freecodecamp.org/news/snake-case-vs-camel-case-vs-pascal-case-vs-kebab-case-whats-the-difference/) `LeaveAsIs`
* everything that is a variable is camelCase: `int myInt = 40;` 
* everything private/protected is prefixed by underscore: `private string _myText;`
* constants cry for help: `private const int _MY_SECRET_ID = 0xdeadbeef;`
* methods want to *start doing* something *big*: `public void InsertStuffIntoDatabase() { }`
* interfaces are selfish: `public interface IKnowBetter { }`
* generic type parameters do *T*-poses: `public void DoThatThing<TItem, TResult>(Func<TItem, TResult> renderer) { }`
* enums, enum-members, classes, namespaces, structs, records, properties all use PascalCase: `public class Car { }`

### Formatting Style
* there is not tab, only two spaces for indendation
* brackets are [K&R](https://en.wikipedia.org/wiki/Indentation_style#K&R_style)-style
* indent statements in a bracketed scope and also if they are single-statement blocks
``` cs
public void TestStuff(bool assertionFailed) {
  if (assertionFailed)
    FailHard();
  else {
    KillingMeSoftly();
  }
}
```
* when, for whatever reason, you need to shorten lines, do so at operators or after commas and indent
``` cs
public void MarkStuff(
  string text,
  int appendCount,
  bool zOrderReversal,
  IntPtr dangerousThingy
) {
  
  var linqResult = (
    from character in text
    where zOrderReversal
    select character
  ).ToArray();

  var lastResort = text
    .Where(c => c < ' ')
    .Select(c => (char)(c + 65))
    ;

  Func<int, bool> lambda = l 
    => (l < appendCount)
    && zOrderReversal
    ;

  var whatEverFits = text.Any()
    ? appendCount
    : linqResult.Length
    ;

}
```

* regions go where code goes
``` cs
class TaxiDriver {

  // BAD:
#region ctor
  public TaxiDriver() { }
#endregion

  // GOOD:
  #region dtor
  ~TaxiDriver() { }
  #endregion

}

```

* the only valid way to indent switch-statements is this
``` cs
switch (onWhateverVariable) {
  case 0:   // single line statement
    break;
  case 1: { // scope with explicit "fall-through"
    var text = "I need a dollar";
    goto case 2;
  }
  case 2: { // block statement with scope
    var text = "I need a block";
    return "later";
  }
  default:  // early return
    return "early";
}
```

* there is lots of space(s) but never more than one contiguous blank line
``` cs
var spacesBefore = "andAfterEqualSigns";
var spaces = "BeforeAnd" + "AfterOperators";
var spaceBag = new [] { "spaces", "after", "commas" }

// this also applies to for, foreach, while, lock, using, fixed, catch, when, etc.
if (spaceIsNotEnclosingConditions) {
  // just here to make it multiline
  throw new InvalidOperationException("I'm gonna cry" + ((99 + 1) * 10) + " times")
}

return "successfully typed spaces";
```

* put spaces around operators but beware of these special cases
``` cs
++i;
j++;
k = -k;
l = +k;
pointer = &value;
copy = *pointer;
result = !input;
```

### File Layout
* usings first
* use file based namespaces
* put nested classes up or separate them into partials named Class.NestedClass.cs
* put constants below
* try to get fields now
* now props
* now ctor/dtor/Dispose
* now methods
* if a field is only needed for single method (e.g. backing field, memoization cache), put it directly in front of the method
* partial classes are OK, especially when classes grow big and have logically-connected blocks

### Namespaces
* order alphabetically (if not logically coherent in a #if-directive)
* no global usings file
* no global:: prefix
* try to avoid static aliasing

### Syntax Style
* use `var` for all declarations possible
* spare the type after new if the compiler knows what you doing, use together with var to your maximum decrease in typing
``` cs
// BAD:
Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();

// GOOD:
Dictionary<string, List<string>> cache = new();

// ALSO GOOD:
var cache = new Dictionary<string, List<string>>();
```
* when assigning `null` or `default`, prefer explicit type over var
``` cs
// BAD:
var cache = (Dictionary<string, List<string>>)null;

// WORSE:
Dictionary<string, List<string>> cache = (Dictionary<string, List<string>>)null;
Dictionary<string, List<string>> cache = default(Dictionary<string, List<string>>);

// GOOD:
Dictionary<string, List<string>> cache = null;

// ALSO GOOD:
Dictionary<string, List<string>> cache = default;
```
* use `this.` for everything that accesses an instance member
* use keywords for types when available (like `string`, `int`, `float`, `bool`)
* use explicit access modifiers (like `public`, `internal`)
* use expression bodies when possible
``` cs
// BAD:
public int CalculateStuff() {
  return (int)(15 * 2 + Math.PI);
}

// GOOD:
public int CalculateOtherStuff() => (int)(15 * 2 + Math.PI);

// ALSO GOOD:
public double CalculateTheThirdStuff() 
  => 15 * 2 + Math.PI
  ;
```

* use anonymous/calculated properties when possible
``` cs
//BAD:
private int _chargeBackingField;
public int Charge {
  get {
    return this._chargeBackingField;
  }
  set {
    this._chargeBackingField = value;
  }
}

// WORSE: C# is not Java
private int _chargeBackingField;
public int GetCharge() {
    return this._chargeBackingField;
}
public void SetCharge(int value) {
    this._chargeBackingField = value;
}

// BETTER:
private int _chargeBackingField;
public int Charge {
  get => this._chargeBackingField;
  set => this._chargeBackingField = value;
}

// GOOD:
public int Charge { get; set; }
```

* use property and collection initializer syntax when applicable
``` cs
// BAD:
var guest = new User();
guest.Name = "Alex";

var guests = new List<User>();
guests.Add(guest);

// GOOD:
var guest = new User { Name = "Alex" };
var guests = new List<User> { guest };

// GOOD: combining with inlining in simple statements is OK
var guests = new List<User> { new User { Name = "Alex" } };
```

### Null-Checking and validation

* use [Guard](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/Against.cs)-clauses instead of checking yourself

``` cs
public static void DoSomething(this string @this, string other, int count) {
  Guard.Against.ThisIsNull(@this);
  Guard.Against.ArgumentIsNull(other);
  Guard.Against.CountBelowOrEqualZero(count);
  ...
}
```

* [guard-clauses](https://maximegel.medium.com/what-are-guard-clauses-and-how-to-use-them-350c8f1b6fd2) come first and they always check given parameters - they're meant to be used as preconditions upon entering a method - do not use them inside the method

``` cs
public static void DoSomething(this string @this) {
  ...
  // BAD: late-checking preconditions
  Guard.Against.ThisIsNull(@this);
  ...
  var index = ...; // someindex

  // BAD: check on local variable
  Guard.Against.IndexOutOfRange(count);
  ...
}
```

* do validate all public code ([taint-mode](https://en.wikipedia.org/wiki/Taint_checking)), avoid duplicate validations in private code

``` cs
public static void DoSomething(this string @this) {
  Guard.Against.ThisIsNull(@this);
  _DoSpecificStuff(@this);
}

private static void _DoSpecificStuff(string @this) {
  // BAD: double-checking
  Guard.Against.ThisIsNull(@this);
  ...
}
```

* use throw-helpers from the [AlwaysThrow](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/AlwaysThrow.cs)-class

``` cs
public static void DoSomething<T>(this IEnumerable<T> @this) {
  ...
  if (!@this.Any())
    AlwaysThrow.NoElements();
}
```

* if no suitable checks or throw-helpers exist (yet), add your own into the Guard-class taking existing code as an example

``` cs
/* Goes into Against.cs */
// GOOD: better debug experience because method is stepped through
[DebuggerHidden]
// GOOD: compiler will try to inline the call and optimize it away entirely if not needed
#if SUPPORTS_INLINING
[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
public static void EarlyMornings(DateTime date) {
  if(date.Hour < 7)
    AlwaysThrow.NoCokeYet();
}

/* Goes into AlwaysThrow.cs */
[DebuggerHidden]
// GOOD: never inline throw-helpers, it would hurt performance by preventing the calling method to get inlined
[MethodImpl(MethodImplOptions.NoInlining)]
// GOOD: tell the static code analyser that we won't return ever
[DoesNotReturn]
// GOOD: name like the exception you're gonna throw
public static void ArgumentOutOfRangeException(string parameterName, string message) => throw new ArgumentOutOfRangeException(parameterName, message);
```

* when adding specific stuff for numbers use the [Against.T4.tt](https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/Corlib.Extensions/Guard/Against.T4.tt) to avoid writing nearly identical code for each number type

``` cs
<#foreach (var type in new[]{"sbyte", "short", "int", "long", "float", "double", "decimal"}) {#>
  [DebuggerHidden]
#if SUPPORTS_INLINING
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
  // GOOD: use CallerMemberName and CallerArgumentExpression to let the compiler work for you
  public static void NegativeValues(<#=type#> value, [CallerMemberName] string caller = null, [CallerArgumentExpression(nameof(value))] string expression = null) {
    if (value < 0)
      AlwaysThrow.ArgumentBelowRangeException(expression ?? nameof(value), value, 0, caller);
  }
<#}#>
```

### Other stuff
* the endless loop lets the for cry
``` cs
for (;;) {
  ...
}
```

* do not depend on more than [Backports](https://www.nuget.org/packages/FrameworkExtensions.Backports) and [Corlib.Extensions](https://www.nuget.org/packages/FrameworkExtensions.Corlib)


### Architecture
* use [design-patterns](https://en.wikipedia.org/wiki/Software_design_pattern)
* use [SOLID](https://en.wikipedia.org/wiki/SOLID)
* when you violate stuff on purpose, comment why you did so