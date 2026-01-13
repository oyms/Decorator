Decorator
===

<img alt="icon" style="width: 200px;" src="./resources/logo.svg" />

> [!TIP]
> A simple way to create decorator classes for interfaces.

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

A code generator that renders wrapper classes for interfaces
to facilitate [the decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern).

<a title="Trashtoy, Public domain, via Wikimedia Commons" href="https://commons.wikimedia.org/wiki/File:Decorator_UML_class_diagram.svg"><img width="512" alt="Decorator UML class diagram" src="https://upload.wikimedia.org/wikipedia/commons/thumb/e/e9/Decorator_UML_class_diagram.svg/512px-Decorator_UML_class_diagram.svg.png?20090613125941"></a>

[![Static Badge](https://img.shields.io/badge/Wikipedia-Decorator_pattern-blue?label=Wikipedia&link=https%3A%2F%2Fen.wikipedia.org%2Fwiki%2FDecorator_pattern)](https://en.wikipedia.org/wiki/Decorator_pattern)

## Usage

[![NuGet Version](https://img.shields.io/nuget/v/Skaar.Decorator.svg)](https://www.nuget.org/packages/Skaar.Decorator) 

```bash
dotnet add package Skaar.Decorator
```

Add assembly attributes for the interfaces and wrapper classes
you need.

```csharp
[assembly:Skaar.Decorator.GenerateDecorator<IInterfaceType>(
    ClassName = "NameOfTheBaseClass",      //Optional. Defaults to name of interface + "DecoratorBase"
    Namespace = "NamespaceOfTheBaseClass"  //Optional. Defaults to the namespace of the interface
    )]
```

This generates an abstract base class that implements the interface.
A protected constructor takes the concrete component.
Virtual implementations of methods, properties, indexer and events are generated,
all calling the inner component.

Create your own concrete decorator class inheriting from the generated base class.
Override the relevant operations.

> [!NOTE]
> The inner type must be an interface.
> It must be public or internal.
> It cannot be a nested type.

Static abstract methods are implemented with
a `NotSupportedException`, as there is no way to find the
actual target type in a static context.

The wrapper class implements `Skaar.Decorator.IDecorator<T>` 
where `T` is the inner type. It has a property `Inner`
to access the inner instance.
When `T` has static methods, the wrapper class
implements `Skaar.Decorator.IDecorator<object>`, as `T` 
cannot be used as a generic argument.

Unbound generic interfaces can be used as inner types,
using the overload of the attribute taking a type as argument.

```csharp
[assembly: Skaar.Decorator.GenerateDecorator(typeof(IMyInterface<,>)]
```

This will generate a generic wrapper class.

## Example

There is an interface you want to decorate. This type may, or may not be imported from another library.

```csharp
public interface IComponent
{
    string Operation(int value);
}
```

You then generate a wrapper class using the attribute.

```csharp
[assembly:Skaar.Decorator.GenerateDecorator<IComponent>]
```

Then you can create a decorator, inheriting from the generated class.
In the class, you override the methods you need to intercept.

```csharp
public class ConcreteDecorator(IComponent inner) : IComponentDecoratorBase(inner)
{
    public override string Operation(int value)
    {
        return $"Decorated: {value}" + base.Operation(value);
    }
}

```

## Name collisions

In the case of a name collision (when the interface in question inherits other interfaces, and they have members 
with the same name; for instance a both a property and a method named `Operation`), the generated class will 
create an intermediate protected member for you to override. 
The implemented member is implemented explicitly to the interface from which it came.