Decorator
===

[![Static Badge](https://img.shields.io/badge/Wikipedia-Decorator_pattern-blue?label=Wikipedia&link=https%3A%2F%2Fen.wikipedia.org%2Fwiki%2FDecorator_pattern)](https://en.wikipedia.org/wiki/Decorator_pattern)

This library generates code for wrapper classes, enabling
easy implementation of decorators.

```csharp
// Generated a wrapper base class
[assembly:Skaar.Decorator.GenerateDecorator<Skaar.Decorator.Example.IComponent>]

namespace Skaar.Decorator.Example;

interface IComponent
{
    string Operation(int value);
}

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

```

The code generated assumes a fairly recent version of C#.

[Documentation on GitHub](https://github.com/oyms/Decorator/blob/main/README.md)

![Icon](https://raw.githubusercontent.com/oyms/Decorator/refs/heads/main/resources/logo.svg)