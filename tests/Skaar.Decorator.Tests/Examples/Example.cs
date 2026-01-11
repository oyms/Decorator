// Generated a wrapper base class

using Skaar.Decorator.Tests.Examples;

[assembly:Skaar.Decorator.GenerateDecorator<IComponent>]

namespace Skaar.Decorator.Tests.Examples;

// Create a decorator inheriting from the generated class.
// Override the relevant operations.
class ConcreteDecorator(IComponent inner) : IComponentDecoratorBase(inner)
{
    public override string Operation(int value)
    {
        // Decorate calls to component operation
        return $"Decorated: {base.Operation(value + 1)}";
    }
}

interface IComponent
{
    string Operation(int value);
}

