using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.Decorator.Generators;

public class Decorators: GeneratorBase
{
    public void Generate(IncrementalGeneratorInitializationContext context, IncrementalValueProvider<ImmutableArray<DecoratorClassParameters>> result)
    {
        context.RegisterSourceOutput(result, (ctx, decoratorParams) =>
        {
            foreach (var param in decoratorParams)
            {
                var targetType = param.TargetType;
                 var isInterface = targetType.TypeKind == TypeKind.Interface;
                 if (!isInterface)
                 {
                     throw new NotImplementedException("Only implement interface types");
                 }
                ctx.AddSource($"{param.Namespace}.{param.ClassName}.g.cs", SourceText.From(GetClassSource(param), Encoding.UTF8));
            }
        });
    }

    private string GetClassSource(DecoratorClassParameters classParams)
    {
        var visibility = classParams.TargetType.DeclaredAccessibility.ToString().ToLower();
        var baseTypeName = classParams.TargetType.ToDisplayString();
        var methods = classParams.TargetType.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => m.MethodKind switch
            {
                MethodKind.Ordinary => GetMethodSource(m),
                _ => $"// Not implemented {m.Name}\n"
            });
        return 
            $$"""
            #nullable enable
            
            namespace {{classParams.Namespace}};               
                          
            {{GeneratedCodeAttribute}}
            {{visibility}} abstract class {{classParams.ClassName}} : {{baseTypeName}}
            {
                protected {{baseTypeName}} __Inner { get; }
                protected {{classParams.ClassName}}({{baseTypeName}} inner)
                {
                    __Inner = inner;
                }
                
                {{string.Join("", methods)}}
            }
            """;
    }

    private string GetMethodSource(IMethodSymbol methodSymbol)
    {
        var returnKeyword = methodSymbol.ReturnsVoid ? "" : "return ";
        return
            $$"""
                  
                  ///<inheritdoc/>
                  public virtual {{methodSymbol.ReturnType}} {{methodSymbol.Name}}({{string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type} {p.Name}"))}})
                  {
                      {{returnKeyword}}__Inner.{{methodSymbol.Name}}({{string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))}});   
                  }     
              """;
    }
}