using System.Linq;
using Microsoft.CodeAnalysis;
using Skaar.Decorator.Model;

namespace Skaar.Decorator.Generators;

class MethodImplememtation(IMethodSymbol symbol, string innerPropertyName, string innerTypeName)
    : MemberImplementation(symbol)
{
    public override string GetSource(bool hasNameCollision)
    {
        var returnKeyword = symbol.ReturnsVoid ? "" : "return ";
        var genericParameters = GenericTypeParameters.Get(symbol);
        var parameterList = string.Join(", ", symbol.Parameters.Select(p => $"{p.Type} {p.Name}"));
        if (symbol.IsStatic)
        {
            return
                $$"""
                      
                      /// <inheritdoc/>
                      static {{symbol.ReturnType}} {{innerTypeName}}.{{symbol.Name}}{{genericParameters}}({{parameterList}}){{genericParameters.Constraints()}}
                      {
                           throw new NotImplementedException("Cannot implement static abstract methods on a decorator");
                      }     
                  """;
        }
        else
        {
            return
                $$"""
                      
                      /// <inheritdoc/>
                      public virtual {{symbol.ReturnType}} {{symbol.Name}}{{genericParameters}}({{parameterList}}){{genericParameters.Constraints()}}
                      {
                          {{returnKeyword}}{{innerPropertyName}}.{{symbol.Name}}{{genericParameters}}({{string.Join(", ", symbol.Parameters.Select(p => p.Name))}});   
                      }     
                  """;
        }
    }
}