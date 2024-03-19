using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

//Questo generatore è un esempio di come leggere i dati da un file di testo e generare il codice sorgente in maniera incrementale
//Legge le query da un file di testo e genera una classe con un metodo per ogni query

namespace NetConfSG.Example.Generator
{

    [Generator]
    public class QueryDapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //IncrementalValuesProvider è un provider che permette di combinare i risultati di altri provider in maniera incrementale
            //E' un tipo opaco che non può essere usato direttamente che permette di combinare i risultati di altri provider in maniera incrementale tramite metodi di estesione

            //Il provider di file di testo è un filtro che permette di selezionare solo i file di testo con estensione .txt
            IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".sql"));
            
            //Metto in una tupla il nome del file e il suo contenuto tramite il provider di file di testo del passo precedente
            IncrementalValuesProvider<(string name, string content)> namesAndContents = textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path), content: text.GetText(cancellationToken)!.ToString()));

            //Registra l'output del generatore
            //Il metodo GenerateCommandsCode verrà chiamato per generare il codice sorgente
            //passando il risultato del provider di file di testo con il nome e il contenuto del file come
            //ImmutableArray<(string name, string content)> in maniera incrementale
            context.RegisterSourceOutput(namesAndContents.Collect(), GenerateQueryCode);
        }

        //Genera il codice sorgente
        private void GenerateQueryCode(SourceProductionContext context, ImmutableArray<(string name, string content)> array)
        {
            if (array.Length == 0) return;
            
            //Genero il codice sorgente con una classe che contiene un metodo per ogni query
            var sb = new StringBuilder();

            foreach (var item in array)
            {
                if(item.content != null)
                    //Genero una metodo pubblico per ogni query
                    sb.AppendLine($"public IEnumerable<dynamic> {item.name}() {{ return _dbConn.Query(\"\"\"\r\n{item.content}\r\n\"\"\");}}");
            }

            //La classe generata avrà un costruttore che accetta un IDbConnection
            var code = $$"""
                using Dapper;
                using System.Data;

                public class DapperQuery
                {
                    private IDbConnection _dbConn;

                    public DapperQuery(IDbConnection dbConn)
                    {
                        IDbConnection _dbConn = dbConn;
                    }

                    {{sb.ToString()}}
                }
                """;                

            //Aggiungo il codice sorgente generato al contesto di generazione
            context.AddSource("DapperQuery.g.cs", code);
        }
    }
}
