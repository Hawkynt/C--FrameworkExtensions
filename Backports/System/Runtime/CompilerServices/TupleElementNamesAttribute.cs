#if !SUPPORTS_TUPLE_ELEMENT_NAMES_ATTRIBUTE

namespace System.Runtime.CompilerServices;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Struct)]
[CLSCompliant(false)]
#if COMPILE_TO_EXTENSION_DLL
public
#else
internal
# endif
class TupleElementNamesAttribute:Attribute {
  public string[] TransformNames { get; }
  public TupleElementNamesAttribute(string[] transformNames) => this.TransformNames = transformNames;
}

#endif