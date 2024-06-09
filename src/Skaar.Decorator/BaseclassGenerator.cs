using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Skaar.Decorator.Extensions;
using Skaar.Decorator.Models;
using Skaar.Decorator.SyntaxBuilders;

namespace Skaar.Decorator;

[Generator]
public class BaseclassGenerator: IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(GenerateAttribute);

        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(Constants.AttributeFullName,
            (node, _) => node is InterfaceDeclarationSyntax,
            BuildModel);
        
        context.RegisterSourceOutput(pipeline, RenderDecoratorClass);
    }

    private static TargetTypeModel BuildModel(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
    {
        var targetSymbol = ctx.TargetSymbol;
        return new TargetTypeModel(
            Namespace: targetSymbol.ContainingNamespace?.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                    SymbolDisplayGlobalNamespaceStyle.Omitted)),
            Name: $"{targetSymbol.Name}Decorator".TrimStart('I'),//Smarter handling
            Node: ctx.TargetNode,
            Type: TargetType.Interface);
    }

    private static void RenderDecoratorClass(SourceProductionContext ctx, TargetTypeModel model)
    {
        switch (model.Type)
        {
            case TargetType.Interface:
                var builder = new InterfaceDecoratorBuilder(model.Name, model.Namespace,
                    (InterfaceDeclarationSyntax)model.Node);
                ctx.AddSource(builder.Name, builder.GetSource());
                break;
            default: throw new NotSupportedException();
        }
    }

    private static string RenderMembers(TargetTypeMembers model, string prefix)
    {
        var bld = new StringBuilder();
        foreach (var member in model)
        {

            if (member is MethodDeclarationSyntax method)
            {
                bld.AppendLine(
                    $"{prefix}public virtual {method.ReturnType} {method.Identifier}{method.ParameterList.ToFullString()} {{");
                var callInner = $"_inner.{method.Identifier}{method.ParameterList.OpenParenToken}{string.Join(", ", method.ParameterList.Parameters.Select(p => p.Identifier))}{method.ParameterList.CloseParenToken};";
                if (method.ReturnType.IsNotNull)
                {
                    bld.AppendLine($"{prefix}\treturn {callInner}");
                }
                else
                {
                    bld.AppendLine($"{prefix}\t{callInner}");
                }
                bld.AppendLine($"{prefix}}}");
            }
        }

        return bld.ToString();
    }

    private static void GenerateAttribute(IncrementalGeneratorPostInitializationContext ctx)
    {
        var builder = new GenerateDecoratorAttributeBuilder();
        ctx.AddSource(builder.Name, builder.GetSource());
    }
}