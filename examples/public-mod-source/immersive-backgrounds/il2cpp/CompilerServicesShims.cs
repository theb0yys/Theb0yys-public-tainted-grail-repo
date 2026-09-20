using System;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field |
    AttributeTargets.GenericParameter | AttributeTargets.Module | AttributeTargets.Parameter |
    AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
internal sealed class NullableAttribute : Attribute
{
    public NullableAttribute(byte value) => NullableFlags = new[] { value };
    public NullableAttribute(byte[] value) => NullableFlags = value;
    public readonly byte[] NullableFlags;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Interface |
    AttributeTargets.Method | AttributeTargets.Struct, Inherited = false)]
internal sealed class NullableContextAttribute : Attribute
{
    public NullableContextAttribute(byte value) => Flag = value;
    public readonly byte Flag;
}
