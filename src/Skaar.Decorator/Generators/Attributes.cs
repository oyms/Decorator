#nullable enable
using System;
using System.CodeDom.Compiler;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.Decorator.Generators;

public class Attributes(string @namespace) : GeneratorBase
{
    public const string AttributeName = AttributeShortName + "Attribute";
    public const string AttributeShortName = "GenerateDecorator";

    public void Generate(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{AttributeName}.g.cs", SourceText.From(AttributeSource(AttributeName), Encoding.UTF8));
        });
    }

    public DecoratorClassParameters? ParseAttribute(GeneratorSyntaxContext context, CancellationToken _ = default)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;
        SimpleNameSyntax? nameSyntax = attributeSyntax.Name switch
        {
            GenericNameSyntax g => g,
            QualifiedNameSyntax { Right: GenericNameSyntax g } => g,
            IdentifierNameSyntax i => i,
            QualifiedNameSyntax { Right: IdentifierNameSyntax i } => i,
            _ => null
        };
        var name = nameSyntax?.Identifier.Text;
        if (
            name is not AttributeName &&
            name is not AttributeShortName)
        {
            return null;
        }

        var model = context.SemanticModel;
        var symbol = model.GetSymbolInfo(attributeSyntax).Symbol as IMethodSymbol;
        if (symbol?.ContainingType is not { } attributeType)
        {
            return null;
        }

        if (attributeType.Name is not AttributeName &&
            attributeType.ContainingNamespace?.Name != @namespace)
        {
            return null;
        }

        INamedTypeSymbol? targetType = null;
        
        if (nameSyntax is GenericNameSyntax gn)
        {
            if (gn.TypeArgumentList.Arguments.Count != 1)
            {
                return null;
            }
            var typeArgSyntax = gn.TypeArgumentList.Arguments[0];
            var typeInfo = model.GetTypeInfo(typeArgSyntax);
            if (typeInfo.Type is not INamedTypeSymbol genericArgumentSymbol)
            {
                return null;
            }
            targetType = genericArgumentSymbol;
        }
        

        string? className = null;
        string? targetNamespace = null;
        
        if (attributeSyntax.ArgumentList is { } args)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameEquals is { Name.Identifier.Text: var id })
                {
                    switch (id)
                    {
                        case "ClassName":
                            var constValueCn = model.GetConstantValue(arg.Expression);
                            if (constValueCn.HasValue)
                                className = constValueCn.Value as string;
                            break;
                        case "Namespace":
                            var constValueNs = model.GetConstantValue(arg.Expression);
                            if (constValueNs.HasValue)
                                targetNamespace = constValueNs.Value as string;
                            break;
                        case "TargetType":
                        {
                            if (arg.Expression is TypeOfExpressionSyntax typeOfExpr)
                            {
                                var typeInfo = model.GetTypeInfo(typeOfExpr.Type);
                                if (typeInfo.Type is INamedTypeSymbol namedType)
                                {
                                    targetType = namedType;
                                }
                            }
                            break;
                        }
                    }
                }
                else if (targetType == null && arg.NameEquals == null)
                {
                    if (arg.Expression is TypeOfExpressionSyntax typeOfExpr)
                    {
                        var typeInfo = model.GetTypeInfo(typeOfExpr.Type);
                        if (typeInfo.Type is INamedTypeSymbol namedType)
                        {
                            targetType = namedType;
                        }
                    }
                }
            }
            
        }
        
        if(targetType is null) return null;
        
        className ??= targetType.Name + "DecoratorBase";
        targetNamespace ??= targetType.ContainingNamespace.ToDisplayString();
                    
        return new DecoratorClassParameters(targetType, className, targetNamespace);
    }
    
    private string AttributeSource(string className) =>
        $$"""
          using System;

          #nullable enable
          #pragma warning disable CS0436 // Type may be defined multiple times
          namespace {{@namespace}};

          /// <summary>
          /// This attribute will generate a decorator class wrapping the given type.
          /// </summary>
          {{GeneratedCodeAttribute}}
          [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class| System.AttributeTargets.Interface, AllowMultiple = true)]
          public class {{className}} : System.Attribute 
          {
            public {{className}}(Type targetType)
            {
                TargetType = targetType;
            }
            ///<summary>The type of the inner value this type wraps.</summary>
            public Type TargetType { get; }
            ///<summary>The name of the generated class</summary>
            public string? ClassName { get; set; }
            ///<summary>The namespace of the generated class</summary>
            public string? Namespace { get; set; }
          }

          /// <summary>
          /// This attribute will generate a decorator class wrapping the given type.
          /// </summary>
          /// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
          {{GeneratedCodeAttribute}}
          [System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class| System.AttributeTargets.Interface, AllowMultiple = true)]
          public class {{className}}<T> : System.Attribute where T: class 
          {
            ///<summary>The name of the generated class</summary>
            public string? ClassName { get; set; }
            ///<summary>The namespace of the generated class</summary>
            public string? Namespace { get; set; }
          }

          """;
}