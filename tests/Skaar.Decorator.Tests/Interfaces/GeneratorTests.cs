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
    
    [Fact]
    public void WithGenericMethod_CallsInnerMethod()
    {
        var expected = new object();
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        inner.GenericMethod<object, string>(Arg.Any<object>()).Returns(true);
        
        target.GenericMethod<object, string>(expected).ShouldBeTrue();
        
        inner.Received().GenericMethod<object, string>(expected);
    }    
    
    [Fact]
    public void WithProperty_GetterAndSetter_CallsInnerMethod()
    {
        var expected = 42d;
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        inner.Property0.Returns(expected);
        
        target.Property0.ShouldBe(expected);
        target.Property0 = expected + 1;
        
        inner.Received().Property0 = expected + 1;
    }    
    
    [Fact]
    public void WithProperty_GetterOnly_CallsInnerMethod()
    {
        var expected = true;
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        inner.Property1.Returns(expected);
        
        target.Property1.ShouldBe(expected);
    }   
    
    [Fact]
    public void WithProperty_SetterOnly_CallsInnerMethod()
    {
        var expected = new byte[]{1,2,3};
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        
        target.Property2 = expected;
        
        inner.Received().Property2 = expected;   
    }    
    [Fact]
    public void WithProperty_Indexer_CallsInnerMethod()
    {
        var expected = "Some string";
        var inner = Substitute.For<GeneratorTests_Interface0>();
        var target = new TestTarget0(inner);
        inner[1, true].Returns(expected);
        
        target[1, true].ShouldBe(expected);
        target[2, true] = expected;
        
        inner.Received()[2, true] = expected;   
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

    /// <summary>
    /// A generic method
    /// </summary>
    bool GenericMethod<T, TT>(T someValue) where T :  class, new() where TT : class;
    
    double Property0 { get; set; }
    bool Property1 { get; }
    byte[] Property2 { set; }
    
    string this[int i, bool x] { get; set; }

    event EventHandler<EventArgs> SomeEvent;
    
    int Inner { set; }
}

// TODO Generic interfaces