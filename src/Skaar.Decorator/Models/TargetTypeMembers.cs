using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Skaar.Decorator.Models;

class TargetTypeMembers : IEquatable<TargetTypeMembers>, IEnumerable<MemberDeclarationSyntax>
{
    private readonly List<MemberDeclarationSyntax> _members;

    public TargetTypeMembers(SyntaxList<MemberDeclarationSyntax> members)
    {
        _members = members.OrderBy(m => m.ToFullString()).ToList();
    }

    public override string ToString() => 
        string.Join("\n", _members.Select(m => m.ToFullString()));

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_members).GetEnumerator();

    public bool Equals(TargetTypeMembers? other) => ToString().Equals(other?.ToString());

    public IEnumerator<MemberDeclarationSyntax> GetEnumerator() => _members.GetEnumerator();

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TargetTypeMembers)obj);
    }

    public override int GetHashCode() => ToString().GetHashCode();
}