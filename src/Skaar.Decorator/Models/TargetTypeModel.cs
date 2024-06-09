using System;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Models;

record TargetTypeModel(string Namespace, string Name, SyntaxNode Node, TargetType Type) : IEquatable<TargetTypeModel>
{
    public string Namespace { get; } = Namespace;
    public string Name { get; } = Name;
    public TargetType Type { get; } = Type;
    public SyntaxNode Node { get; } = Node;
    bool IEquatable<TargetTypeModel>.Equals(TargetTypeModel other) => 
        Name == other.Name && 
        Namespace == other.Namespace &&
        Node.ToFullString() == other.Node.ToFullString();
}