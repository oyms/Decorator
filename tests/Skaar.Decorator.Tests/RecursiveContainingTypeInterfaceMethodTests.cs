using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Skaar.Decorator.Tests;

public class RecursiveContainingTypeInterfaceMethodTests
{
    [Fact]
    public void ArgumentsWithModifiers()
    {
        var target = new DecoratorEksempel1(new Eksempel1());

        var outvalue = 13;
        var refValue = "a";
        Assert.Equal(7 ,target.Count(out outvalue, true, ref refValue));
        Assert.True(target.TryParse("hei", out var o));
    }

    class DecoratorEksempel1 : RecursiveContainingTypeInterfaceMethodTests_A_B_C_Eksempel1Decorator
    {
        public DecoratorEksempel1(A.B.C.IEksempel1 inner) : base(inner)
        {
        }

        public override int Count(out int outvalue, in bool inValue, ref string refValue, params string[] para_mette)
        {
            outvalue = 7;
            return outvalue;
        }

        public override bool TryParse(string value, out int? result)
        {
            result = 14;
            return true;
        }
    }

    class Eksempel1 : A.B.C.IEksempel1
    {
        public int Count(out int outvalue, in bool inValue, ref string refValue, params string[] para_mette)
        {
            throw new NotImplementedException();
        }

        public bool TryParse(string value, out int? result)
        {
            throw new NotImplementedException();
        }
    }
    

    public class A
    {
        public class B
        {
            public class C
            {
                [GenerateDecorator]
                public interface IEksempel1
                {
                    public int Count(out int outvalue, in bool inValue, ref string refValue,
                        params string[] para_mette);

                    public bool TryParse([MinLength(7)] string value, [NotNullWhen(true)] out int? result);
                }
            }
        }
    }
}


