using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skaar.Decorator.Models;

record TargetName(string Name, string Namespace, IEnumerable<string> ContainingTypeNames, TargetType Type)
{
    private readonly string _name = Name;

    public string Name
    {
        get
        {
            if (Type == TargetType.Interface && _name.Length > 1 && _name[0] == 'I') return _name.Substring(1);
            return _name;
        }
    }

    public string Namespace { get; } = Namespace;
    public IEnumerable<string> ContainingTypeNames { get; } = ContainingTypeNames;
    public TargetType Type { get; } = Type;

    public string WrapperName => ContainingTypeNames.Any() ? $"{string.Join("_", ContainingTypeNames)}_{Name}Decorator": $"{Name}Decorator";
    public string WrapperFullName => $"{ToString()}Decorator";
    
    public override string ToString()
    {
        var bld = new StringBuilder(Namespace);
        if (ContainingTypeNames.Any())
        {
            bld.AppendFormat(".{0}", string.Join(".", ContainingTypeNames));
        }
        bld.AppendFormat(".{0}", _name);
        return bld.ToString();
    }
}