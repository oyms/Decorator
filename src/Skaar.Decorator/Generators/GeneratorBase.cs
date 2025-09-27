using System;
using System.Reflection;

namespace Skaar.Decorator.Generators;

public abstract class GeneratorBase
{
    
    string ToolName => Assembly.GetExecutingAssembly().GetName().Name;
    Version ToolVersion => Assembly.GetExecutingAssembly().GetName().Version;
    protected string GeneratedCodeAttribute =>
        $"[System.CodeDom.Compiler.GeneratedCode(\"{ToolName}\", \"{ToolVersion}\")]";
}