using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace NetConfSG.Example.Generator
{
    [Generator]
    public class CommandListGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(n=>n.Identifier.Text.EndsWith("Command"));

            var compilation = context.CompilationProvider.Combine(provider.Collect());

            context.RegisterSourceOutput(compilation, Execute);

            var fileprovider = context.AdditionalTextsProvider.Where(t => t.Path.EndsWith("txt"));

            context.RegisterSourceOutput(fileprovider.Collect(), ExecuteFileTxt);
        }

        private void ExecuteFileTxt(SourceProductionContext context, ImmutableArray<AdditionalText> array)
        {
            var listcmd = array.Select(c => System.IO.Path.GetFileNameWithoutExtension(c.Path));

            var code = string.Join("\n",listcmd.Select(c => $"public class {c} {{}}"));

            context.AddSource("CommandListExternal.g.cs", code);
        }

        private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
        {
            var compilation = tuple.Left;
            var classes = tuple.Right;

            var listcmd = string.Join(",", classes.Select(c => $"\"{c.Identifier.Text}\""));

            var code = $$"""
                public static partial class CommandList
                {
                    public static readonly string[] Commands = new string[] { {{listcmd}} };
                }
            """;

            context.AddSource("CommandList.g.cs", code);
        }
    }
}
