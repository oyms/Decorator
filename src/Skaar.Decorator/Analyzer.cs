using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Skaar.Decorator;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeArgument(IsPublicOrPrivate), SyntaxKind.Attribute);
        context.RegisterSyntaxNodeAction(AnalyzeArgument(IsNested), SyntaxKind.Attribute);
        context.RegisterSyntaxNodeAction(AnalyzeArgument(IsInterface), SyntaxKind.Attribute);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [InvalidVisibility, InvalidType, IsNestedType];

#pragma warning disable RS1032
    private static readonly DiagnosticDescriptor InvalidVisibility = new(
        id: "DECORATOR001",
        title: "Invalid Visibility",
        messageFormat:
        $"Target types specified in [{Generators.Attributes.AttributeName}] attribute must be public or internal.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidType = new(
        id: "DECORATOR002",
        title: "Invalid type",
        messageFormat:
        $"Target types specified in [{Generators.Attributes.AttributeName}] attribute must be interfaces.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor IsNestedType = new(
        id: "DECORATOR003",
        title: "Nested type",
        messageFormat:
        $"Target types specified in [{Generators.Attributes.AttributeName}] cannot be nested in another type.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private void IsPublicOrPrivate(SyntaxNodeAnalysisContext context, ISymbol symbol, SyntaxNode node)
    {
        var isPublicOrInternal = symbol.DeclaredAccessibility == Accessibility.Public ||
                                 symbol.DeclaredAccessibility == Accessibility.Internal;

        var isFileScoped = symbol is INamedTypeSymbol typeSymbol && typeSymbol.IsFileLocal;
        
        if (!isPublicOrInternal || isFileScoped)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidVisibility,
                node.GetLocation()
            ));
        }
    }
    
    private void IsNested(SyntaxNodeAnalysisContext context, ISymbol symbol, SyntaxNode node)
    {
        var isNested = symbol.ContainingType != null;
        if (isNested)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                IsNestedType,
                node.GetLocation()
            ));
        }
    }    
    
    private void IsInterface(SyntaxNodeAnalysisContext context, ISymbol symbol, SyntaxNode node)
    {
        if (symbol is INamedTypeSymbol named && named.TypeKind != TypeKind.Interface)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidType,
                node.GetLocation()
            ));
        }
    }

    private Action<SyntaxNodeAnalysisContext> AnalyzeArgument(
        Action<SyntaxNodeAnalysisContext, ISymbol, SyntaxNode> action)
    {
        return ctx =>
        {
            var attributes = new Generators.Attributes(Generator.Namespace);
            var parameters = attributes.ParseAttribute(
               (AttributeSyntax) ctx.Node, ctx.SemanticModel
            );
            if (parameters is null) return;
            action(ctx, parameters.TargetType, ctx.Node);
        };
    }
}