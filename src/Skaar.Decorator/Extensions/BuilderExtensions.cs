using System.Text;
using Microsoft.CodeAnalysis.Text;
using Skaar.Decorator.SyntaxBuilders;

namespace Skaar.Decorator.Extensions;

static class BuilderExtensions
{
    public static SourceText GetSource(this BuilderBase builder) =>
        SourceText.From(builder.Node().ToFullString(), Encoding.UTF8);
}