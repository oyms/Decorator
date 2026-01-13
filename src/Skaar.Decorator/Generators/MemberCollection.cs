using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

class MemberCollection
{
    private readonly HashSet<MemberImplementation> _members = new(new MemberEqualityComparer());
    public void Add(MemberImplementation member)
    {
        _members.Add(member);
    }

    public void AddRange(IEnumerable<MemberImplementation> members)
    {
        foreach (var member in members.Where(m => m != null))
        {
            Add(member);
        }
    }

    private IEnumerable<string> GetSources(IEnumerable<MemberImplementation> members)
    {
        foreach (var membersWithSameName in _members.GroupBy(m => m.Name).OrderBy(g => g.Key))
        {
            var isCollision = membersWithSameName.Count() > 1;
            foreach (var member in membersWithSameName)
            {
                yield return member.GetSource(isCollision);
            }
        }
    }

    public override string ToString() => string.Join("\n", GetSources(_members));
}

class MemberEqualityComparer : IEqualityComparer<MemberImplementation>
{
    private SymbolEqualityComparer _inner = SymbolEqualityComparer.Default;
    public bool Equals(MemberImplementation x, MemberImplementation y)
    {
        return _inner.Equals(x.Symbol, y.Symbol);
    }

    public int GetHashCode(MemberImplementation obj)
    {
        return _inner.GetHashCode(obj.Symbol);
    }
}