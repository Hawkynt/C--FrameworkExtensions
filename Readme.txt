In this folder are the various extensions.

Each type that gets extended has its own file in the format
<Namespace>.<TypeName>Extension.cs

Classes are declared internal to avoid conflicts with multiple projects in the solution.
Classes are declared partial to allow extending them further.
Contracts are used to check conditions.