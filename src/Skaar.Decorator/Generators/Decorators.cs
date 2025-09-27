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
                
                {{string.Join("\n", methods)}}
            }
            """;
    }

    private string GetMethodSource(IMethodSymbol methodSymbol)
    {
        var returnKeyword = methodSymbol.ReturnsVoid ? "" : "return ";
        var genericParameters = methodSymbol.IsGenericMethod ? 
            $"<{string.Join(", ", methodSymbol.TypeParameters.Select(t => t.Name))}>" : 
            "";
        var genericConstraints = methodSymbol.IsGenericMethod
            ? " " + string.Join(" ", methodSymbol.TypeParameters.Select(BuildConstraintClause))
            : "";
        return
            $$"""
                  
                  ///<inheritdoc/>
                  public virtual {{methodSymbol.ReturnType}} {{methodSymbol.Name}}{{genericParameters}}({{string.Join(", ", methodSymbol.Parameters.Select(p => $"{p.Type} {p.Name}"))}}){{genericConstraints}}
                  {
                      {{returnKeyword}}__Inner.{{methodSymbol.Name}}{{genericParameters}}({{string.Join(", ", methodSymbol.Parameters.Select(p => p.Name))}});   
                  }     
              """;
    }
    
    private string BuildConstraintClause(ITypeParameterSymbol tp)
    {
        var parts = new System.Collections.Generic.List<string>();

        // class / struct / unmanaged / notnull
        if (tp.HasReferenceTypeConstraint)
        {
            var ann = tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class";
            parts.Add(ann);
        }
        if (tp.HasValueTypeConstraint) parts.Add("struct");
        if (tp.HasUnmanagedTypeConstraint) parts.Add("unmanaged");
        if (tp.HasNotNullConstraint) parts.Add("notnull");

        // type constraints
        if (tp.ConstraintTypes.Length > 0)
        {
            parts.AddRange(tp.ConstraintTypes.Select(t => t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        // new()
        if (tp.HasConstructorConstraint) parts.Add("new()");

        return parts.Count == 0 ? "" : $"where {tp.Name} : {string.Join(", ", parts)}";
    }
}