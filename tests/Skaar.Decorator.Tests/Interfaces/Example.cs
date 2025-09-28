// Generated a wrapper base class
[assembly:Skaar.Decorator.GenerateDecorator<Skaar.Decorator.Example.IComponent>]

namespace Skaar.Decorator.Example;

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

