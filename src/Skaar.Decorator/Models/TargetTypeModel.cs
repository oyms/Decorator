namespace Skaar.Decorator.Models;

record TargetTypeModel(string Namespace, string Name, TargetTypeMembers Members)
{
    public string Namespace { get; } = Namespace;
    public string Name { get; } = Name;
    public TargetTypeMembers Members { get; } = Members;
    public string KastMeg { get; set; }
}