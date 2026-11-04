using System;

namespace Skaar.Decorator;

/// <summary>
/// This attribute will generate a decorator class wrapping the given type.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class GenerateDecoratorAttribute : Attribute
{
    public GenerateDecoratorAttribute(Type targetType)
    {
        TargetType = targetType;
    }
    ///<summary>The type of the inner value this type wraps.</summary>
    public Type TargetType { get; }
    ///<summary>The name of the generated class</summary>
    public string? ClassName { get; set; }
    ///<summary>The namespace of the generated class</summary>
    public string? Namespace { get; set; }
}

/// <summary>
/// This attribute will generate a decorator class wrapping the given type.
/// </summary>
/// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class GenerateDecoratorAttribute<T> : Attribute where T : class
{
    ///<summary>The name of the generated class</summary>
    public string? ClassName { get; set; }
    ///<summary>The namespace of the generated class</summary>
    public string? Namespace { get; set; }
}
