# C# Framework Extensions [![Build Status](https://img.shields.io/github/actions/workflow/status/Hawkynt/C--FrameworkExtensions/dotnet.yml?branch=master "Build Status")](https://github.com/Hawkynt/C--FrameworkExtensions/actions)

Extensions to the [.NET Framework](https://en.wikipedia.org/wiki/.NET_Framework) and [.Net Core](https://en.wikipedia.org/wiki/.NET) for use with [C# programming language](https://en.wikipedia.org/wiki/C_Sharp_(programming_language)) or any other compatible .[NET language](https://en.wikipedia.org/wiki/List_of_.NET_libraries_and_frameworks).

This is the folder where all extensions to .NET classes go.

## Licensing
* [LGPL-3.0](https://en.wikipedia.org/wiki/GNU_Lesser_General_Public_License)
* give credit to ***Hawkynt*** when using
* please do **pull**-requests if you add something
* report issues to and get in contact at [GitHub](https://github.com/Hawkynt/C--FrameworkExtensions)
* always deliver the LICENSE file to your code's customer

## Rules
There are some guidelines for extensions which have proven one's worth:
* Every referenced assembly/package should have its own project/assembly
* Use folders for every part of the namespace
* Every file in there should have a name that is build like this: "**Type**.cs"
* The namespace in the files is always the same namespace as the original type is in
* The classname is always "**Type**Extensions". The class is always "internal/public static partial", thus allowing us to extend it further in a given project
  by adding another partial class with the same name
* All public methods must be static
* The first parameter of all "public" methods must be the type itself and is called "**@this**" or alternatively "*This*" (only for old code)
* For extensions to static classes like "Math" or "Activator",
  there is no "This"-parameter
