namespace Skaar.Decorator.Tests;

public class InterfaceIndexerTests
{
    [Fact]
    public void TestIndex()
    {
        const string expected = "some string";
        const int index = 7;
        
        var target = new Decorator3(new Eksempel3());
        target[index] = expected;
        
        Assert.Equal(expected, target[index]);
    }

    class Decorator3 : InterfaceIndexerTests_Eksempel3Decorator
    {
        private readonly Dictionary<int, string> _values = new();
        public Decorator3(IEksempel3 inner) : base(inner)
        {
        }

        public override string this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
    }

    class Eksempel3 : IEksempel3
    {
        public string this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
    
    [GenerateDecorator]
    public interface IEksempel3
    {
        string this[int index] { get; set; }
    }
}