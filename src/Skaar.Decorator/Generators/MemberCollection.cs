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
            var isCollision = IsCollision(membersWithSameName);
            foreach (var member in membersWithSameName)
            {
                yield return member.GetSource(isCollision);
            }
        }
    }

    private static bool IsCollision(IEnumerable<MemberImplementation> membersWithSameName)
    {
        var members = membersWithSameName.ToList();
        if (members.Count() < 2) return false;
        if (members.All(m => m is MethodImplememtation))
        {
            var names = members
                .Cast<MethodImplememtation>()
                .Select(m => m.GetSignature())
                .ToList();
            return names.Distinct().Count() != names.Count();
        }

        return true;
    }

    public override string ToString() => string.Join("\n", GetSources(_members));
}