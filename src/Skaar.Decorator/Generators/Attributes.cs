#nullable enable
using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skaar.Decorator.Generators;

public class Attributes(string @namespace) : GeneratorBase
{
    public const string AttributeName = AttributeShortName + "Attribute";
    public const string AttributeShortName = "GenerateDecorator";

    public DecoratorClassParameters? ParseAttribute(GeneratorSyntaxContext context, CancellationToken _ = default) 
        => ParseAttribute((AttributeSyntax)context.Node, context.SemanticModel);

    public DecoratorClassParameters? ParseAttribute(AttributeSyntax attributeSyntax, SemanticModel model)
    {
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
}