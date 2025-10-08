using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Skaar.Decorator.Generators;

public abstract class GeneratorBase
{
    
    string ToolName => Assembly.GetExecutingAssembly().GetName().Name;
    Version ToolVersion => Assembly.GetExecutingAssembly().GetName().Version;
    protected string GeneratedCodeAttribute =>
        $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";

    protected string GetSafePropertyName(string suggestion, ImmutableArray<ISymbol> symbols)
    {
        if (symbols.Any(s => s.Name == suggestion)) return GetSafePropertyName("_" + suggestion, symbols);
        return suggestion;
    }
}