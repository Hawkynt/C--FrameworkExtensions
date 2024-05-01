namespace System;

/// <summary>
/// Used to force the compiler to chose a method-overload with a class constraint on a generic type.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class __ClassForcingTag<T> where T : class { private __ClassForcingTag() { } }
/// <summary>
/// Used to force the compiler to chose a method-overload with a struct constraint on a generic type.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class __StructForcingTag<T> where T : struct { private __StructForcingTag() { } }
