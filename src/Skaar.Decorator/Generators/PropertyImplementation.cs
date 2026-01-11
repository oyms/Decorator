using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class PropertyImplementation(IPropertySymbol symbol, string innerPropertyName, string innerTypeName) : MemberImplementation(symbol)
{
    public override string GetSource(bool hasNameCollision)
    {
        if (symbol.IsIndexer)
        {
            var parameters = symbol.Parameters;
            var parameterList = string.Join(", ", parameters.Select(p => p.Name));
            var getter = symbol.IsWriteOnly
                ? string.Empty
                : $$"""
                        
                            get 
                            {
                                return {{innerPropertyName}}[{{parameterList}}];
                            }
                    """;
            var setter = symbol.IsReadOnly
                ? string.Empty
                : $$"""
                    set
                            {
                                {{innerPropertyName}}[{{parameterList}}] = value;
                            }
                    """;
            return
                $$"""
                        /// <inheritdoc/>
                        public virtual {{symbol.Type.ToDisplayString()}} this[{{string.Join(", ", parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"))}}]
                        {
                            {{getter}}
                            {{setter}}
                        }
                  """;
        }
        else if (symbol.IsStatic)
        {
            return
                $$"""

                      /// <inheritdoc/>
                      static {{symbol.Type.ToDisplayString()}} {{innerTypeName}}.{{symbol.Name}}
                      {
                         get { throw new NotImplementedException("Cannot implement static abstract properties on a decorator"); }
                         set { throw new NotImplementedException("Cannot implement static abstract properties on a decorator"); }
                      }     
                  """;
        }
        else
        {
            var getter = symbol.IsWriteOnly
                ? string.Empty
                : $$"""
                        
                            get 
                            {
                                return {{innerPropertyName}}.{{symbol.Name}};
                            }
                    """;
            var setter = symbol.IsReadOnly
                ? string.Empty
                : $$"""
                     set
                            {
                                {{innerPropertyName}}.{{symbol.Name}}= value;
                            }
                    """;
            return
                $$"""
                      /// <inheritdoc/>
                      public virtual {{symbol.Type.ToDisplayString()}} {{symbol.Name}} 
                      {
                         {{getter}}
                         {{setter}}
                      }   
                  """;
        }
    }
}