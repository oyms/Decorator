#nullable enable
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

public record DecoratorClassParameters
{
    public DecoratorClassParameters(INamedTypeSymbol targetType, string className, string @namespace)
    {
        Namespace = @namespace;
        TargetType = targetType;
        ClassName = className;
    }

    public string Namespace { get; }
    public INamedTypeSymbol TargetType { get; }
    public string ClassName { get; }
}