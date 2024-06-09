using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Skaar.Decorator.Tests;

public class InterfaceMethodTests
{
    [Fact]
    public void Test1()
    {
        var target = new DecoratorEksempel0(new Eksempel0());
        target.Method0();
        target.Method1(1, string.Empty, true);
        Assert.True(target.Method2(""));
    }

    class DecoratorEksempel0 : InterfaceMethodTests_Eksempel0Decorator
    {
        public DecoratorEksempel0(IEksempel0 inner) : base(inner)
        {
        }

        public override void Method0()
        {
        }

        public override void Method1(int a, string b, bool c)
        {
        }
    }

    class Eksempel0 : IEksempel0
    {
        public void Method0()
        {
            throw new NotImplementedException();
        }

        public void Method1(int a, string b, bool c)
        {
            throw new NotImplementedException();
        }

        public bool Method2(string value)
        {
            return true;
        }
    }

    [GenerateDecorator]
    public interface IEksempel0
    {
        void Method0();
        void Method1(int a, string b, bool c);
        bool Method2(string value);
    }
}

[GenerateDecorator]
public interface UtenBaseklasse
{
    
}

public class A
{
    public class B
    {
        public class C
        {
            [GenerateDecorator]
            public interface IWhatever
            {
                public int Count(out int outvalue, in bool inValue, ref string refValue, params string[] para_mette);
                public bool TryParse([MinLength(7)]string value,[NotNullWhen(true)] out int? result);
            }
        }
    }
}

