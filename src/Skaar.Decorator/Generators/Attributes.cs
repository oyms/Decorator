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
        var nameSyntax = attributeSyntax.Name switch
        {
            GenericNameSyntax g => g,
            QualifiedNameSyntax { Right: GenericNameSyntax g } => g,
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

        if (nameSyntax!.TypeArgumentList.Arguments.Count != 1)
        {
            return null;
        }
                    
        var typeArgSyntax = nameSyntax.TypeArgumentList.Arguments[0];
        var typeInfo = model.GetTypeInfo(typeArgSyntax);
        if (typeInfo.Type is not INamedTypeSymbol targetType)
            return null;

        string? className = null;
        string? targetNamespace = null;
        
        if (attributeSyntax.ArgumentList is { } args)
        {
            foreach (var arg in args.Arguments)
            {
                if (arg.NameEquals is { Name.Identifier.Text: var id })
                {
                    var constValue = model.GetConstantValue(arg.Expression);
                    if (!constValue.HasValue) continue;
                    switch (id)
                    {
                        case "ClassName":
                            className = constValue.Value as string;
                            break;
                        case "Namespace":
                            targetNamespace = constValue.Value as string;
                            break;
                    }
                }
            }
        }
        
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