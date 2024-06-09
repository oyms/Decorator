using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skaar.Decorator.SyntaxBuilders;

abstract class BuilderBase
{
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
    public override string ToString() => Node().ToFullString();
}