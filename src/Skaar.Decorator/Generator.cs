using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skaar.Decorator.Extensions;
using Skaar.Decorator.Models;
using Skaar.Decorator.SyntaxBuilders;

namespace Skaar.Decorator;

[Generator]
public class Generator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateAttribute);

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(Constants.AttributeFullName,
            (node, _) => node is InterfaceDeclarationSyntax,
            BuildModel);
        
        context.RegisterSourceOutput(pipeline, RenderDecoratorClass);
    }

    private TargetTypeModel BuildModel(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
    {
        var targetSymbol = ctx.TargetSymbol;
        var name = new TargetName(
            targetSymbol.Name, 
            targetSymbol.ContainingNamespace?.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                    SymbolDisplayGlobalNamespaceStyle.Omitted)), 
            ContainingTypeNames(targetSymbol),
            TargetType.Interface);
        return new TargetTypeModel(
            Name: name,
            Node: ctx.TargetNode,
            Type: TargetType.Interface);
    }

    private IEnumerable<string> ContainingTypeNames(ISymbol symbol)
    {
        var parent = symbol.ContainingType;
        if (parent is null) yield break;
        foreach (var containingTypeName in ContainingTypeNames(parent))
        {
            yield return containingTypeName;
        }

        yield return parent.Name;
    }

    private void RenderDecoratorClass(SourceProductionContext ctx, TargetTypeModel model)
    {
        switch (model.Type)
        {
            case TargetType.Interface:
                var builder = new InterfaceDecoratorBuilder(
                    model.Name, 
                    (InterfaceDeclarationSyntax)model.Node);
                ctx.AddSource(builder.Name, builder.GetSource());
                break;
            default: throw new NotSupportedException();
        }
    }


    private void GenerateAttribute(IncrementalGeneratorPostInitializationContext ctx)
    {
        var builder = new GenerateDecoratorAttributeBuilder();
        ctx.AddSource(builder.Name, builder.GetSource());
    }
}