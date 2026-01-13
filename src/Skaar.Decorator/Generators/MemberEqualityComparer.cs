using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class MemberEqualityComparer : IEqualityComparer<MemberImplementation>
{
    private readonly SymbolEqualityComparer _inner = SymbolEqualityComparer.Default;
    public bool Equals(MemberImplementation x, MemberImplementation y)
    {
        return _inner.Equals(x.Symbol, y.Symbol);
    }

    public int GetHashCode(MemberImplementation obj)
    {
        return _inner.GetHashCode(obj.Symbol);
    }
}