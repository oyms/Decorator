using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Skaar.Decorator.SyntaxBuilders;

class GenerateDecoratorAttributeBuilder : BuilderBase
{
    public override string Name => Constants.AttributeName;

    public override SyntaxNode Node()
    {
        var usingDirectives = new[]
        {
            SF.UsingDirective(SF.ParseName("System"))
        };
        var namespaceDeclaration = SF.NamespaceDeclaration(SF.ParseName(Constants.Namespace));
        var classDeclaration = SF.ClassDeclaration(Constants.AttributeName)
            .AddModifiers(Modifiers(SyntaxKind.PublicKeyword))
            .AddBaseListTypes(SF.SimpleBaseType(SF.ParseTypeName("Attribute")))
            .AddAttributeLists(
                BuildGeneratedCodeAttribute(),
                SF.AttributeList(SF.SingletonSeparatedList(
                        SF.Attribute(SF.ParseName("AttributeUsage"))
                            .AddArgumentListArguments(
                                SF.AttributeArgument(
                                    SF.BinaryExpression(
                                        SyntaxKind.BitwiseOrExpression,
                                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            SF.IdentifierName("AttributeTargets"),
                                            SF.IdentifierName("Class")),
                                        SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            SF.IdentifierName("AttributeTargets"),
                                            SF.IdentifierName("Interface"))
                                    )
                                )
                            )
                        )
                )
            )
            .AddMembers();

        return SF.CompilationUnit()
            .AddUsings(usingDirectives)
            .AddMembers(namespaceDeclaration.AddMembers(classDeclaration.WithLeadingTrivia(BuildTrivia())))
            .NormalizeWhitespace();
    }

    private SyntaxTriviaList BuildTrivia()
    {
        return SF.ParseLeadingTrivia("""
             /// <summary>
             /// Provides a generated base class to facilitate a decorator
             /// <seealso href="https://en.wikipedia.org/wiki/Decorator_pattern" />
             /// </summary>
                                     
            """).Add(SF.EndOfLine("\n"));
    }
    

}