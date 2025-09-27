#nullable enable
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Skaar.Decorator.Generators;

public class Interfaces(string @namespace) : GeneratorBase
{
    public const string InterfaceName = "IDecorator";
    public const string PropertyName = "Inner";
    public void Generate(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{@namespace}.{InterfaceName}.g.cs", SourceText.From(InterfaceSource(InterfaceName), Encoding.UTF8));
        });
    }

    private string InterfaceSource(string interfaceName)
    {
        return
        $$"""
              #nullable enable
              #pragma warning disable CS0436 // Type may be defined multiple times
              namespace {{@namespace}};

              /// <summary>
              /// A interface indicating that the class is a decorator wrapper..
              /// </summary>
              /// <typeparam name="T">The type of the inner value this type wraps.</typeparam>
              {{GeneratedCodeAttribute}}
              public interface {{interfaceName}}<T>
              {
                ///<summary>The inner value this type wraps.</summary>
                public T {{PropertyName}} { get; }
              }

              """;
    }
}