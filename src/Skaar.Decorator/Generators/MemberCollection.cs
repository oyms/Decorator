using System.Collections.Generic;
using System.Linq;

namespace Skaar.Decorator.Generators;

class MemberCollection
{
    private List<MemberImplementation> _members = [];
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