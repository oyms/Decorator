using System.Linq;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Model;

class GenericTypeParameters(ITypeParameterSymbol[] symbols)
{
    public static GenericTypeParameters Get(IMethodSymbol symbol)
    {
        return symbol.IsGenericMethod ? new(symbol.TypeParameters.ToArray()) : new([]);   
    }
    public static GenericTypeParameters Get(INamedTypeSymbol symbol)
    {
        var isGeneric = symbol.IsGenericType &&
                        (symbol.IsUnboundGenericType ||
                         symbol.TypeArguments.Any(a => a.TypeKind == TypeKind.TypeParameter)
                        );
        if (isGeneric)
        {
            if (symbol.IsUnboundGenericType)
            {
                return new(symbol.OriginalDefinition.TypeParameters.ToArray());
            }
            else
            {
                return new(symbol.TypeArguments.OfType<ITypeParameterSymbol>().ToArray());
            }
        }
        else
        {
            return new([]);
        }
    }   
    public override string ToString()
    {
        if (symbols.Length == 0)
        {
            return string.Empty;
        }

        return $"<{string.Join(", ", symbols.Select(t => t.Name))}>";
    }
    
    public string Constraints()
    {
        var clauses = symbols.Select(BuildConstraintClause).ToList();
        if (clauses.Any())
        {
            return $"  {string.Join(" ", clauses)}";
        }

        return string.Empty;
    }

    private string BuildConstraintClause(ITypeParameterSymbol tp)
    {
        var parts = new System.Collections.Generic.List<string>();

        // class / struct / unmanaged / notnull
        if (tp.HasReferenceTypeConstraint)
        {
            var ann = tp.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated ? "class?" : "class";
            parts.Add(ann);
        }

        if (tp.HasValueTypeConstraint) parts.Add("struct");
        if (tp.HasUnmanagedTypeConstraint) parts.Add("unmanaged");
        if (tp.HasNotNullConstraint) parts.Add("notnull");

        // type constraints
        if (tp.ConstraintTypes.Length > 0)
        {
            parts.AddRange(tp.ConstraintTypes.Select(t =>
                t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        // new()
        if (tp.HasConstructorConstraint) parts.Add("new()");

        return parts.Count == 0 ? "" : $"where {tp.Name} : {string.Join(", ", parts)}";
    }
}