using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

//Questo generatore è un esempio di come leggere i dati da un file di testo e generare il codice sorgente in maniera incrementale
//Legge i comandi da un file di testo e genera una classe per ogni comando
//Il file di testo deve contenere un comando per riga

namespace NetConfSG.Example.Generator
{

    [Generator]
    public class CommandListGeneratorFromFile : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //IncrementalValuesProvider è un provider che permette di combinare i risultati di altri provider in maniera incrementale
            //E' un tipo opaco che non può essere usato direttamente che permette di combinare i risultati di altri provider in maniera incrementale tramite metodi di estesione

            //Il provider di file di testo è un filtro che permette di selezionare solo i file di testo con estensione .txt
            IncrementalValuesProvider<AdditionalText> textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".txt"));
            
            //Metto in una tupla il nome del file e il suo contenuto tramite il provider di file di testo del passo precedente
            IncrementalValuesProvider<(string name, string content)> namesAndContents = textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path), content: text.GetText(cancellationToken)!.ToString()));

            //Registra l'output del generatore
            //Il metodo GenerateCommandsCode verrà chiamato per generare il codice sorgente
            //passando il risultato del provider di file di testo con il nome e il contenuto del file come
            //ImmutableArray<(string name, string content)> in maniera incrementale
            context.RegisterSourceOutput(namesAndContents.Collect(), GenerateCommandsCode);
        }

        //Genera il codice sorgente
        private void GenerateCommandsCode(SourceProductionContext context, ImmutableArray<(string name, string content)> array)
        {
            //Genero il codice sorgente con una classe per ogni comando trovato all'interno del file di testo
            var sb = new StringBuilder();

            foreach (var item in array)
            {
                StringReader reader = new StringReader(item.content);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    //Genero una classe pubblica per ogni comando
                    sb.AppendLine($"public class {line} {{}}");
                };
            }

            //Aggiungo il codice sorgente generato al contesto di generazione
            context.AddSource("CommandListFromFile.g.cs", sb.ToString());
        }
    }
}
