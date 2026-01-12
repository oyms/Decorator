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
                      /// <exception cref="System.NotSupportedException">Cannot implement static abstract methods on a decorator</exception>
                      static {{symbol.ReturnType}} {{innerTypeName}}.{{symbol.Name}}{{genericParameters}}({{parameterList}}){{genericParameters.Constraints()}}
                      {
                           throw new System.NotSupportedException("Cannot implement static abstract methods on a decorator");
                      }     
                  """;
        }
        if (hasNameCollision)
        {
            var containingType = symbol.ContainingType;
            var containingTypeName = containingType.ToDisplayString();
            var intermediateMethodName = CreateIntermediateName();
            return
                $$"""

                      /// <remarks>
                      /// Override this to customize {{containingTypeName}}.{{symbol.Name}}.
                      /// </remarks>
                      protected virtual {{symbol.ReturnType}} {{intermediateMethodName}}({{parameterList}}){{genericParameters.Constraints()}}
                      {
                        var that = ({{containingTypeName}}) {{innerPropertyName}};
                        {{returnKeyword}}that.{{symbol.Name}}{{genericParameters}}({{string.Join(", ", symbol.Parameters.Select(p => p.Name))}});  
                      }
                      
                      /// <inheritdoc/>
                      {{symbol.ReturnType}} {{containingTypeName}}.{{symbol.Name}}{{genericParameters}}({{parameterList}}){{genericParameters.Constraints()}}
                      {
                          {{returnKeyword}}this.{{intermediateMethodName}}{{genericParameters}}({{string.Join(", ", symbol.Parameters.Select(p => p.Name))}});   
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