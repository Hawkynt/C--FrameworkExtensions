# Contributing guidelines

## Foreword

These extensions are meant to be extended by everyone who is firm in C#.
I appreciate every idea making these classes 
* go faster
* use less memory
* have more throughput
* less worst/average-case-complexity
* or simply teaching them new tricks
  
However, this is not my daily business and I kindly ask you for some time to read through your pull-requests and issues.

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

## Are you keen on refactoring?

You can go on and refactor whatever you think is necessary to make the code more readable or adept to new .Net versions. 
However don't make the code slower or more memory-hungry during refactoring. 
Pay kind attention to details, escpecially all that compiler-sugar. 
C# is doing a lot under the hood which you only see when using [dotPeek](https://www.jetbrains.com/decompiler/), [dnSpy](https://github.com/dnSpy/dnSpy), [Reflector](https://www.red-gate.com/products/reflector/) or any other decompilation tool.

Ask if unsure and learn about micro-optimizations. 
If you really feel it, create benchmark code under "Tests" like the one already in place.

BTW: I am using JetBrains [ReSharper](https://www.jetbrains.com/resharper/) so don't wonder upon specific comments for it.

## Hardcore-Fixer?

Everyone can learn new things and nobody is perfect. 
Some things just don't work yet like I like them to be. 
If you gonna fix actions, code, tests or whatever just let me know.
