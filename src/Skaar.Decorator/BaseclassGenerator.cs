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
using Skaar.Decorator.Models;

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
        
        context.RegisterSourceOutput(pipeline, RenderDecoratorClassFromInterface);
    }

    private static TargetTypeModel BuildModel(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
    {
        var targetSymbol = ctx.TargetSymbol;
        var targetNode = ctx.TargetNode as InterfaceDeclarationSyntax;
        return new TargetTypeModel(
            Namespace: targetSymbol.ContainingNamespace?.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                    SymbolDisplayGlobalNamespaceStyle.Omitted)),
            Name: targetSymbol.Name,
            Members: new(targetNode.Members))
        {
            KastMeg = string.Join(" ", targetNode.Members.ToFullString())
        };
    }

    private static void RenderDecoratorClassFromInterface(SourceProductionContext ctx, TargetTypeModel model)
    {
        var className = model.Name.TrimStart('I') + "Decorator";
        var innerTypeName = $"{model.Namespace}.{model.Name}";
        var source = SourceText.From($$"""
           namespace {{Constants.Namespace}} {
              public abstract class {{className}} : {{innerTypeName}} {
                private readonly {{innerTypeName}} _inner;
                protected {{className}}({{innerTypeName}} inner) {
                    _inner = inner;
                }
                {{RenderMembers(model.Members, "     ")}}
              }
           }
           """, Encoding.UTF8);
        ctx.AddSource(model.Namespace+"."+model.Name+"Decorator", source);
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
        ctx.AddSource(Constants.AttributeName, SourceText.From(
                $$"""
                  using System;

                  namespace {{Constants.Namespace}} {
                      /// <summary>
                      /// Provides a generated base class to facilitate a decorator
                      /// <seealso href="https://en.wikipedia.org/wiki/Decorator_pattern" />
                      /// </summary>
                      {{GeneratedCodeAttribute()}}
                      [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
                      public class GenerateDecoratorAttribute : Attribute
                      {       
                      }
                  }
                  """, Encoding.UTF8));;
    }

    private static string GeneratedCodeAttribute()
    {
        var assName = Assembly.GetExecutingAssembly().GetName();
        return $"[System.CodeDom.Compiler.GeneratedCode(\"{assName.Name}\", \"{assName.Version}\")]";
    }
}