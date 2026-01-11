using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Skaar.Decorator.Model;

namespace Skaar.Decorator.Generators;

public class WrapperClasses(string toolNamespace) : GeneratorBase
{
    public void Generate(IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<ImmutableArray<DecoratorClassParameters>> result)
    {
        context.RegisterSourceOutput(result, (ctx, decoratorParams) =>
        {
            foreach (var param in decoratorParams)
            {
                var targetType = param.TargetType;
                var isInterface = targetType.TypeKind == TypeKind.Interface;
                if (!isInterface)
                {
                    //"Only implement interface types"
                    return;
                }

                ctx.AddSource($"{param.Namespace}.{param.ClassName}.g.cs",
                    SourceText.From(GetClassSource(param), Encoding.UTF8));
            }
        });
    }

    private string GetClassSource(DecoratorClassParameters classParams)
    {
        var targetType = classParams.TargetType.IsUnboundGenericType ? classParams.TargetType.OriginalDefinition : classParams.TargetType;
        var innerPropertyName = GetSafePropertyName("Inner", targetType.GetMembers());
        var baseTypeName = targetType.ToDisplayString();
        var targetTypeCanBeUsedAsGenericArgument =
            !(targetType.TypeKind == TypeKind.Interface &&
              targetType.GetMembers().Any(m => m.IsStatic && m.IsAbstract));
        var decoratorTypeArgument = targetTypeCanBeUsedAsGenericArgument ? baseTypeName : "object";
        var decoratorInterfaceName = $"{toolNamespace}.{Interfaces.InterfaceName}<{decoratorTypeArgument}>";
        var visibility = targetType.DeclaredAccessibility.ToString().ToLower();
        var genericParameters = GenericTypeParameters.Get(targetType);
        var members = new MemberCollection();

        members.AddRange(GetAllMembersRecursively(targetType)
            .OfType<IMethodSymbol>()
            .Select(m => m.MethodKind switch
            {
                MethodKind.Ordinary => new MethodImplememtation(m, innerPropertyName, baseTypeName),
                _ => null
            }));
        
        members.AddRange(GetAllMembersRecursively(targetType)
            .OfType<IPropertySymbol>()
            .Select(x => new PropertyImplementation(x, innerPropertyName, baseTypeName)));
        members.AddRange(GetAllMembersRecursively(targetType)
            .OfType<IEventSymbol>()
            .Select(x => new EventImplementation(x, innerPropertyName)));
        return
            $$"""
              #nullable enable

              namespace {{classParams.Namespace}};               
                            
              {{GeneratedCodeAttribute}}
              {{visibility}} abstract class {{classParams.ClassName}}{{genericParameters}} : {{baseTypeName}}, {{decoratorInterfaceName}}{{genericParameters.Constraints()}}
              {
                  protected {{baseTypeName}} {{innerPropertyName}} { get; }
                  {{decoratorTypeArgument}} {{decoratorInterfaceName}}.{{Interfaces.PropertyName}} => {{innerPropertyName}};
                  protected {{classParams.ClassName}}({{baseTypeName}} inner)
                  {
                      {{innerPropertyName}} = inner;
                  }
                  
                  {{members}}
              }
              """;
    }

    private IEnumerable<ISymbol> GetAllMembersRecursively(INamedTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            yield return member;
        }

        foreach (var baseMethod in symbol.Interfaces.SelectMany(GetAllMembersRecursively))
        {
            yield return baseMethod;
        }
    }
}