using System;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Models;

record TargetTypeModel(TargetName Name, SyntaxNode Node, TargetType Type) : IEquatable<TargetTypeModel>
{
    public TargetName Name { get; } = Name;
    public TargetType Type { get; } = Type;
    public SyntaxNode Node { get; } = Node;
    bool IEquatable<TargetTypeModel>.Equals(TargetTypeModel other) => 
        Name == other.Name && 
        Node.ToFullString() == other.Node.ToFullString();
}