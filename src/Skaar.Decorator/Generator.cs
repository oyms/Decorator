using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Skaar.Decorator.Generators;

namespace Skaar.Decorator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var @namespace = GetType().Namespace!;
        var attr = new Attributes(@namespace);
        var decorators = new WrapperClasses(@namespace);
        var interfaces = new Interfaces(@namespace);
        
        attr.Generate(context);
        interfaces.Generate(context);
        
        GenerateDecoratorClasses(context, attr, decorators);
    }

    private void GenerateDecoratorClasses(
        IncrementalGeneratorInitializationContext context, 
        Attributes attr, 
        WrapperClasses decorators)
    {
        var result = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is AttributeSyntax,
                attr.ParseAttribute)
            .Where(r => r is not null)
            .Select((r, _) => r!)
            .Collect();
        
        decorators.Generate(context, result);
    }
}