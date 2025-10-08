using System;
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
                    throw new NotImplementedException("Only implement interface types");
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
        
        var methods = targetType.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => m.MethodKind switch
            {
                MethodKind.Ordinary => GetMethodSource(m, innerPropertyName, baseTypeName),
                MethodKind.PropertyGet => null,
                MethodKind.PropertySet => null,
                MethodKind.EventAdd => null,
                MethodKind.EventRemove => null,
                _ => $"// Not implemented {m.Name}\n"
            })
            .Where(s => s != null)
            .Select(s => s!);
        var properties = targetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(x => GetPropertySource(x, innerPropertyName, baseTypeName));
        var events = targetType.GetMembers()
            .OfType<IEventSymbol>()
            .Select(x => GetEventSource(x, innerPropertyName));
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
                  
                  {{string.Join("\n", properties)}}
                  {{string.Join("\n", methods)}}
                  {{string.Join("\n", events)}}
              }
              """;
    }

    private string GetEventSource(IEventSymbol symbol, string innerPropertyName)
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

    private string GetPropertySource(IPropertySymbol symbol, string innerPropertyName, string innerTypeName)
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

    private string GetMethodSource(IMethodSymbol symbol, string innerPropertyName, string innerTypeName)
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