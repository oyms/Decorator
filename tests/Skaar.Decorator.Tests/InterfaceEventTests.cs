namespace Skaar.Decorator.Tests;

public class InterfaceEventTests
{
    [Fact]
    public void TestEvent()
    {
        string? actual = null;
        var expected = "expected value";
        var inner = new Eksempel4();
        var target = new Decorator4(inner);
        target.SomethingHappened += (_, args) => actual = args.Payload;

        inner.Trigger(expected);
        
        Assert.Equal(expected, actual);
    }

    class Decorator4 : InterfaceEventTests_Eksempel4Decorator
    {
        public Decorator4(IEksempel4 inner) : base(inner)
        {
        }

        public override event EventHandler<Eksempel4EventArgs> SomethingHappened
        {
            add => base.SomethingHappened += value;
            remove => base.SomethingHappened -= value;
        }
    }

    class Eksempel4 : IEksempel4
    {
        public void Trigger(string payload)
        {
            SomethingHappened?.Invoke(this, new Eksempel4EventArgs{Payload = payload});
        }
        public event EventHandler<Eksempel4EventArgs>? SomethingHappened;
    }

    [GenerateDecorator]
    public interface IEksempel4
    {
        event EventHandler<Eksempel4EventArgs> SomethingHappened;
        event EventHandler<Eksempel4EventArgs> SomethingElseHappened;
    }

}
public class Eksempel4EventArgs : EventArgs
{
    public string Payload { get; set; }
}