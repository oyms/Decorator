using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class EventImplementation(IEventSymbol symbol, string innerPropertyName) : MemberImplementation(symbol)
{
    public override string GetSource(bool hasNameCollision)
    {
        if (hasNameCollision)
        {
            var containingType = symbol.ContainingType;
            var containingTypeName = containingType.ToDisplayString();
            var intermediateEventName = CreateIntermediateName();
            return
                $$"""
                  
                      #region {{containingTypeName}}.{{symbol.Name}}
                      /// <remarks>
                      /// Override this to customize {{containingTypeName}}.{{symbol.Name}}.
                      /// </remarks>
                      protected virtual event {{symbol.Type.ToDisplayString()}} {{intermediateEventName}}
                          {
                            add => (({{containingTypeName}}){{innerPropertyName}}).{{symbol.Name}} += value;
                            remove => (({{containingTypeName}}){{innerPropertyName}}).{{symbol.Name}} -= value;
                          }
                      
                      /// <inheritdoc/>
                      event {{symbol.Type.ToDisplayString()}} {{containingTypeName}}.{{symbol.Name}}
                          {
                            add => this.{{intermediateEventName}} += value;
                            remove => this.{{intermediateEventName}} -= value;
                          }
                      #endregion  
                  
                  """;
        }
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