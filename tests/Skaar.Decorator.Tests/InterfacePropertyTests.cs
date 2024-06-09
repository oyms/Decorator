namespace Skaar.Decorator.Tests;

public class InterfacePropertyTests
{
    [Fact]
    public void Test1()
    {
        var target = new Decorator2(new Eksempel2());
        Assert.NotNull(target.PropertyGetOnly);
        target.ProperySetOnly = true;
        Assert.Equal("True", target.Property0);
    }

    class Decorator2 : InterfacePropertyTests_Eksempel2Decorator
    {
        public Decorator2(IEksempel2 inner) : base(inner)
        {
        }

        public override object PropertyGetOnly => new object();

        public override bool ProperySetOnly
        {
            set => Property0 = value.ToString();
        }
    }

    class Eksempel2 : IEksempel2
    {
        public string Property0 { get; set; }
        public object PropertyGetOnly { get; }
        public bool ProperySetOnly { get; set; }
    }

    [GenerateDecorator]
    public interface IEksempel2
    {
        public string Property0 { get; set; }
        public object PropertyGetOnly { get; }
        public bool ProperySetOnly { set; }
        
    }
}

