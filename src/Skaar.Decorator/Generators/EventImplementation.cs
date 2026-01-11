using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class EventImplementation(IEventSymbol symbol, string innerPropertyName) : MemberImplementation(symbol)
{
    public override string GetSource(bool hasNameCollision)
    {
        return
            $$"""
              /// <inheritdoc/>
              public virtual event {{symbol.Type.ToDisplayString()}} {{symbol.Name}}
                  {
                    add => {{innerPropertyName}}.{{symbol.Name}} += value;
                    remove => {{innerPropertyName}}.{{symbol.Name}} -= value;
                  }
              """;
    }
}