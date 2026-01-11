using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

abstract class MemberImplementation(ISymbol symbol)
{
    public string Name => symbol.Name;
    public abstract string GetSource(bool hasNameCollision);
}