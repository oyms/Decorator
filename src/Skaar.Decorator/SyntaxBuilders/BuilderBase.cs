using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skaar.Decorator.SyntaxBuilders;

abstract class BuilderBase
{
    public abstract string Name { get; }
    public abstract SyntaxNode Node();
    protected AttributeListSyntax BuildGeneratedCodeAttribute()
    {
        var assName = Assembly.GetExecutingAssembly().GetName();
        return SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
            SyntaxFactory.Attribute(SyntaxFactory.ParseName(typeof(GeneratedCodeAttribute).FullName))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(assName.Name))),
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(assName.Version.ToString())))
                )));
    }

    protected SyntaxToken[] Modifiers(params SyntaxKind[] kind) => 
        kind.Select(k => SyntaxFactory.Token(k)).ToArray();
    public override string ToString() => Node().ToFullString();
}