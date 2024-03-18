using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;

//Questo generatore è un esempio di come leggere le classi direttametne dal codice sorgente e generare il codice sorgente in maniera incrementale
//Recupera tutte le classi che terminano con "Command" e genera una classe statica con un array di stringhe con i nomi delle classi trovate

//Il codice sorgente generato sarà simile a questo:
//public static partial class CommandList
//{
//    public static readonly string[] Commands = new string[] { "Command1", "Command2", "Command3" };   
//}
    
namespace NetConfSG.Example.Generator
{
    //L'attributo [Generator] permette di registrare il generatore nel progetto e di abilitare la generazione del codice sorgente in fase di compilazione
    //Il generatore deve implementare l'interfaccia IIncrementalGenerator
    [Generator]
    public class CommandListGenerator : IIncrementalGenerator
    {
        //ATTENZIONE:
        //Nella documentaione https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview trovate ancora la vecchia implementazione tramite l'interfaccia ISourceGenerator che è deprecata

        //L'implementazione con Incremental Generator è più efficiente in quanto permette di generare il codice sorgente in maniera incrementale
        //https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md

        //Initialize viene chiamato una sola volta per ogni generatore
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //Registra il provider di sintassi del codice sorgente
            //Il provider di sintassi è un filtro che permette di selezionare solo le classi che terminano con "Command"
            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(n=>n.Identifier.Text.EndsWith("Command"));

            //Combina i provider di sintassi
            //Il compilatore chiamerà il metodo Collect() per ottenere i risultati dei provider di sintassi
            //CompilationProvider è un provider che permette di combinare i risultati dei provider di sintassi con i dati di compilazione
            var compilation = context.CompilationProvider.Combine(provider.Collect());

            //Registra l'output del generatore
            //Il metodo GenerateCommandListCode verrà chiamato per generare il codice sorgente
            //passando il risultato del provider di sintassi e il risultato del provider di compilazione combinati assieme in una tupla in maniera incrementale
            context.RegisterSourceOutput(compilation, GenerateCommandListCode);
        }

        //Genera il codice sorgente
        private void GenerateCommandListCode(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
        {
            //Recupero il risultato del provider di compilazione
            var compilation = tuple.Left;
            //Recupero le classi che terminano con "Command"
            var classes = tuple.Right;
            
            //Per ogni classe recupero il nome e lo inserisco in una lista separata da virgole
            var listcmd = string.Join(",", classes.Select(c => $"\"{c.Identifier.Text}\""));

            //Genero il codice sorgente con lista di comandi com array di stringhe in una classe statica
             var code = $$"""
                public static partial class CommandList
                {
                    public static readonly string[] Commands = new string[] { {{listcmd}} };
                }
            """;

            //Aggiungo il codice sorgente al contesto di compilazione
            //Il parametro hintName è il nome del file sorgente che verrà generato
            //Come convenzione si usa il nome con l'estensione .g.cs
            context.AddSource("CommandList.g.cs", code);
        }
    }
}
