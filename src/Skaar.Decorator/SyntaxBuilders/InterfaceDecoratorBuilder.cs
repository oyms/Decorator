using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Skaar.Decorator.SyntaxBuilders;

class InterfaceDecoratorBuilder(string name, string @namespace, InterfaceDeclarationSyntax targetNode)
    : BuilderBase
{
    public override string Name => $"{@namespace}.{name}";
    public override SyntaxNode Node()
    {
        var namespaceDeclaration = SF
            .NamespaceDeclaration(SF.ParseName(@namespace))
            .NormalizeWhitespace();
        var classDeclaration = SF.ClassDeclaration(name)
            .AddModifiers(Modifiers(SyntaxKind.PublicKeyword,SyntaxKind.AbstractKeyword))
            .AddBaseListTypes(SF.SimpleBaseType(ParseTargetTypeName()))
            .AddAttributeLists(BuildGeneratedCodeAttribute())
            .AddMembers(BuildMembers().ToArray());

        return SF.CompilationUnit()
            .AddMembers(namespaceDeclaration.AddMembers(classDeclaration))
            .NormalizeWhitespace();
    }

    private TypeSyntax ParseTargetTypeName() => 
        SF.ParseTypeName(targetNode.Identifier.ToString());

    private IEnumerable<MemberDeclarationSyntax> BuildMembers()
    {
        var innerField = SF.IdentifierName("_inner");
        yield return SF.FieldDeclaration(
            SF.VariableDeclaration(ParseTargetTypeName())
                .AddVariables(SF.VariableDeclarator(innerField.Identifier)))
            .AddModifiers(Modifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword));
        yield return SF.ConstructorDeclaration(name)
            .AddModifiers(Modifiers(SyntaxKind.ProtectedKeyword))
            .AddParameterListParameters(
                SF.Parameter(SF.Identifier("inner"))
                    .WithType(ParseTargetTypeName())
                    )
                    .WithBody(
                        SF.Block(
                            SF.ExpressionStatement(
                            SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, innerField, SF.IdentifierName("inner"))
                            )
                        )
                    );
        foreach (var targetMember in targetNode.Members)
        {
            if (targetMember is MethodDeclarationSyntax targetMethod)
            {
                yield return SF.MethodDeclaration(targetMethod.ReturnType, targetMethod.Identifier)
                    .AddModifiers(Modifiers(SyntaxKind.PublicKeyword, SyntaxKind.VirtualKeyword))
                    .AddParameterListParameters(
                        targetMethod.ParameterList.Parameters.Select(p =>
                                SF.Parameter(p.Identifier)
                                    .WithType(p.Type))
                            .ToArray()
                    )
                    .WithBody(SF.Block(BuildMethodBody(innerField, targetMethod)));
            }
        }
    }

    private StatementSyntax BuildMethodBody(IdentifierNameSyntax innerField, MethodDeclarationSyntax targetMethod)
    {
        var expression = SF.InvocationExpression(
            SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                innerField,
                SF.IdentifierName(targetMethod.Identifier)
            )
        );
        if (targetMethod.ParameterList.Parameters.Any())
        {
            expression = expression.AddArgumentListArguments(
                    targetMethod.ParameterList
                        .Parameters
                        .Select(p => SF.Argument(SF.IdentifierName(p.Identifier)))
                        .ToArray()
                );
        }

        if (IsVoidMethod(targetMethod))
        {
            return SF.ExpressionStatement(expression);
        }
        return SF.ReturnStatement(expression);
    }
    
    private bool IsVoidMethod(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.ReturnType is PredefinedTypeSyntax predefinedType &&
               predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);
    }
}