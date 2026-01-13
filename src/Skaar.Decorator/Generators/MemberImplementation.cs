using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

abstract class MemberImplementation(ISymbol symbol)
{
    public ISymbol Symbol => symbol;
    public string Name => symbol.Name;
    public abstract string GetSource(bool hasNameCollision);

    protected string CreateIntermediateName() =>
        $"__{symbol
            .ContainingType
            .ToDisplayString()
            .Replace('.', '_')
            .Replace('<', '_')
            .Replace('>', '_')
        }_{symbol.Name}";
}