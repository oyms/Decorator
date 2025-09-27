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
        var innerPropertyName = GetSafePropertyName("Inner",  classParams.TargetType.GetMembers());
        var visibility = classParams.TargetType.DeclaredAccessibility.ToString().ToLower();
        var baseTypeName = classParams.TargetType.ToDisplayString();
        var methods = classParams.TargetType.GetMembers()
            .OfType<IMethodSymbol>()
            .Select(m => m.MethodKind switch
            {
                MethodKind.Ordinary => GetMethodSource(m, innerPropertyName),
                MethodKind.PropertyGet => null,
                MethodKind.PropertySet => null,
                MethodKind.EventAdd  => null,
                MethodKind.EventRemove => null,
                _ => $"// Not implemented {m.Name}\n"
            })
            .Where(s => s != null)
            .Select(s => s!);
        var properties = classParams.TargetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Select(x => GetPropertySource(x, innerPropertyName));
        var events = classParams.TargetType.GetMembers()
            .OfType<IEventSymbol>()
            .Select(x => GetEventSource(x, innerPropertyName));
        return 
            $$"""
            #nullable enable
            
            namespace {{classParams.Namespace}};               
                          
            {{GeneratedCodeAttribute}}
            {{visibility}} abstract class {{classParams.ClassName}} : {{baseTypeName}}
            {
                protected {{baseTypeName}} {{innerPropertyName}} { get; }
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

    private string GetSafePropertyName(string suggestion, ImmutableArray<ISymbol> symbols)
    {
        if (symbols.Any(s => s.Name == suggestion)) return GetSafePropertyName("_" + suggestion, symbols);
        return suggestion;
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

    private string GetPropertySource(IPropertySymbol symbol, string innerPropertyName)
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

    private string GetMethodSource(IMethodSymbol symbol, string innerPropertyName)
    {
        var returnKeyword = symbol.ReturnsVoid ? "" : "return ";
        var genericParameters = symbol.IsGenericMethod ? 
            $"<{string.Join(", ", symbol.TypeParameters.Select(t => t.Name))}>" : 
            "";
        var genericConstraints = symbol.IsGenericMethod
            ? " " + string.Join(" ", symbol.TypeParameters.Select(BuildConstraintClause))
            : "";
        return
            $$"""
                  
                  /// <inheritdoc/>
                  public virtual {{symbol.ReturnType}} {{symbol.Name}}{{genericParameters}}({{string.Join(", ", symbol.Parameters.Select(p => $"{p.Type} {p.Name}"))}}){{genericConstraints}}
                  {
                      {{returnKeyword}}{{innerPropertyName}}.{{symbol.Name}}{{genericParameters}}({{string.Join(", ", symbol.Parameters.Select(p => p.Name))}});   
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