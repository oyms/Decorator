using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class PropertyImplementation(IPropertySymbol symbol, string innerPropertyName, string innerTypeName)
    : MemberImplementation(symbol)
{
    public override string GetSource(bool hasNameCollision)
    {
        if (symbol.IsStatic)
        {
            return GenerateStaticProperty();
        }

        if (symbol.IsIndexer)
        {
            return GenerateIndexer();
        }
        else
        {
            return GenerateRegularProperty(hasNameCollision);
        }
    }

    private string GenerateRegularProperty(bool hasNameCollision)
    {
        if (hasNameCollision) return GenerateExplicitRegularProperty();

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
        return $$"""
                     /// <inheritdoc/>
                     public virtual {{symbol.Type.ToDisplayString()}} {{symbol.Name}} 
                     {
                        {{getter}}
                        {{setter}}
                     }   
                 """;
    }

    private string GenerateExplicitRegularProperty()
    {
        var containingType = symbol.ContainingType;
        var containingTypeName = containingType.ToDisplayString();
        var intermediatePropertyName = "__" + containingTypeName.Replace('.', '_') + "_" + symbol.Name;

        var intermediateProperty = GenerateIntermediateProperty(intermediatePropertyName, containingTypeName);

        var getter = symbol.IsWriteOnly
            ? string.Empty
            : $$"""
                    
                        get 
                        {
                            return {{intermediatePropertyName}};
                        }
                """;
        var setter = symbol.IsReadOnly
            ? string.Empty
            : $$"""
                 set
                        {
                            {{intermediatePropertyName}} = value;
                        }
                """;
        return $$"""
                    {{intermediateProperty}}

                     /// <inheritdoc/>
                     {{symbol.Type.ToDisplayString()}} {{containingTypeName}}.{{symbol.Name}} 
                     {
                        {{getter}}
                        {{setter}}
                     }   
                 """;
    }

    private object GenerateIntermediateProperty(string propertyName, string containingTypeName)
    {
        var getter = symbol.IsWriteOnly
            ? string.Empty
            : $$"""
                    
                        get 
                        {
                            var that = ({{containingTypeName}}) {{innerPropertyName}};
                            return that.{{symbol.Name}};
                        }
                """;
        var setter = symbol.IsReadOnly
            ? string.Empty
            : $$"""
                 set
                        {
                            var that = ({{containingTypeName}}) {{innerPropertyName}};
                            that.{{symbol.Name}}= value;
                        }
                """;
        return $$"""
                     
                     /// <remarks>
                     /// Override this to customize {{containingTypeName}}.{{symbol.Name}}.
                     /// </remarks>
                     protected virtual {{symbol.Type.ToDisplayString()}} {{propertyName}} 
                     {
                        {{getter}}
                        {{setter}}
                     }   
                 """;
    }

    private string GenerateIndexer()
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

    private string GenerateStaticProperty()
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
}