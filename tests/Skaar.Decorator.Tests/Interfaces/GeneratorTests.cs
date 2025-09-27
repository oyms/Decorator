using NSubstitute;
using Shouldly;
using Skaar.Decorator.Tests.Interfaces;

[assembly:
    Skaar.Decorator.GenerateDecorator<GeneratorTests_Interface0>(
        ClassName = nameof(GeneratorTests_Interface0_DecoratorBase),
        Namespace = "Skaar.Decorator.Tests.Interfaces")]

namespace Skaar.Decorator.Tests.Interfaces;

public class GeneratorTests
{
    [Fact]
    public void WithMethodWithReturnValue_CallsInnerMethod()
    {
        const string expected = "Some string";
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        inner.Method().Returns(expected);
        
        target.Method().ShouldBe(expected);
    }
    
    [Fact]
    public void WithVoidMethod_CallsInnerMethod()
    {
        const string expected = "Some string";
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        
        target.OtherMethod(expected);
        
        inner.Received().OtherMethod(expected);
    }
}

file class TestTarget0 : Skaar.Decorator.Tests.Interfaces.GeneratorTests_Interface0_DecoratorBase
{
    public TestTarget0(GeneratorTests_Interface0 inner) : base(inner)
    {
    }
}

public interface GeneratorTests_Interface0
{
    /// <summary>
    /// Method with return value
    /// </summary>
    string Method();
    /// <summary>
    /// A void method
    /// </summary>
    void OtherMethod(string input);
}

// TODO: Properties
// TODO: Fields
// TODO: events
// TODO Generic interfaces