using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skaar.Decorator.Models;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Skaar.Decorator.SyntaxBuilders;

class InterfaceDecoratorBuilder(TargetName name, InterfaceDeclarationSyntax targetNode)
    : BuilderBase
{
    public override string Name => name.WrapperFullName;
    public override SyntaxNode Node()
    {
        var namespaceDeclaration = SF
            .NamespaceDeclaration(SF.ParseName(name.Namespace))
            .NormalizeWhitespace();
        var classDeclaration = SF.ClassDeclaration(name.WrapperName)
            .AddModifiers(Modifiers(SyntaxKind.PublicKeyword,SyntaxKind.AbstractKeyword))
            .AddBaseListTypes(SF.SimpleBaseType(ParseTargetTypeName()))
            .AddAttributeLists(BuildGeneratedCodeAttribute())
            .AddMembers(BuildMembers().ToArray());

        return SF.CompilationUnit()
            .AddMembers(namespaceDeclaration.AddMembers(classDeclaration))
            .NormalizeWhitespace();
    }

    private TypeSyntax ParseTargetTypeName() => 
        SF.ParseTypeName(name.ToString());

    private IEnumerable<MemberDeclarationSyntax> BuildMembers()
    {
        var innerField = SF.IdentifierName("_inner");
        yield return SF.FieldDeclaration(
            SF.VariableDeclaration(ParseTargetTypeName())
                .AddVariables(SF.VariableDeclarator(innerField.Identifier)))
            .AddModifiers(Modifiers(SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword));
        yield return SF.ConstructorDeclaration(name.WrapperName)
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
                        targetMethod.ParameterList.Parameters.Select(BuildParameter).ToArray()
                    )
                    .WithBody(SF.Block(BuildMethodBody(innerField, targetMethod)));
            }
        }
    }

    private ParameterSyntax BuildParameter(ParameterSyntax targetParameter)
    {
        var result = SF.Parameter(targetParameter.Identifier)
            .WithType(targetParameter.Type);
        
        foreach (var targetParameterModifier in targetParameter.Modifiers)
        {
            result = result.AddModifiers(targetParameterModifier);
        }

        return result;
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
                        .Select(BuildArgument)
                        .ToArray()
                );
        }

        if (IsVoidMethod(targetMethod))
        {
            return SF.ExpressionStatement(expression);
        }
        return SF.ReturnStatement(expression);

        ArgumentSyntax BuildArgument(ParameterSyntax targetParameter)
        {
            var result = SF.Argument(SF.IdentifierName(targetParameter.Identifier));
            if (targetParameter.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
            {
                result = result.WithRefOrOutKeyword(SF.Token(SyntaxKind.OutKeyword));
            }            
            if (targetParameter.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
            {
                result = result.WithRefOrOutKeyword(SF.Token(SyntaxKind.RefKeyword));
            }

            return result;
        }
    }
    
    private bool IsVoidMethod(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.ReturnType is PredefinedTypeSyntax predefinedType &&
               predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);
    }
}