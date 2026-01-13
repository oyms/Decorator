using NSubstitute;
using Shouldly;
// ReSharper disable All

[assembly: Skaar.Decorator.GenerateDecorator<Skaar.Decorator.Tests.Collisions.IMain>]
[assembly: Skaar.Decorator.GenerateDecorator<Skaar.Decorator.Tests.Collisions.IMain1>]

namespace Skaar.Decorator.Tests.Collisions;

public class GeneratorTests
{
    [Fact]
    public void WithNameCollisions_ShouldNotFail()
    {
        var inner = Substitute.For<IMain>();
        ((IEnumerable<bool>)inner).GetEnumerator().Returns(new[]{true}.ToList().GetEnumerator());
        ((ISub0)inner).Name0.Returns("Name0");
        ((ISub0)inner).Name1(Arg.Any<int>()).Returns("Name1");
        ((ISub1)inner).Name1.Returns("Name1");
        ((ISub1)inner).Name0(Arg.Any<int>()).Returns("Name0");
        
        var target = new TestTarget0(inner);
        var t0 = target as ISub0;
        var t1 = target as ISub1;
        
        ((IEnumerable<bool>)inner).First().ShouldBe(true);
        t0.Name0.ShouldBe("Name0");
        t0.Name1(1).ShouldBe("Name1");
        t1.Name1.ShouldBe("Name1");
        t1.Name0(1).ShouldBe("Name0");
    }
}

file class TestTarget0(IMain inner)
    : IMainDecoratorBase(inner);

file class TestTarget1(IMain1 inner)
    : IMain1DecoratorBase(inner)
{
}


public interface IMain : ISub0, ISub1, ISub2, IEnumerable<bool>, IEnumerable<int>;

public interface IMain1 : ISub0, ISub3
{
    string Name0(int intArgument);
    string Name0(int intArgument, bool otherArgument);
    int Name1(int argument);
}

public interface ISub0
{
    string Name0 { get; set; }
    string Name1(int intArgument);
    string NonCollidingName(int intArgument);
}
public interface ISub1
{
    string Name1 { get; set; }
    string Name0(int intArgument);
    
    string AlsoNonCollidingName(int intArgument);
}

public interface ISub2
{
    string Name0(int intArgument);
    event EventHandler<int> Name1;
}

public interface ISub3
{
    bool Name2();
    string Name2(int intArgument);
    byte Name2(string stringArgument);
}