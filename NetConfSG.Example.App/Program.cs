using NetConfSG.Example.App;
using System.Data.Common;
using System.Reflection;

Console.WriteLine("List of commands");
Console.WriteLine("================");
Console.WriteLine("");

//Get all types end with Command name in the current assembly using reflection
//This canno't be use with AOT compilation
Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t.Name.EndsWith("Command")).ToList()
        .ForEach(t => Console.WriteLine(t.Name));

//CommandList.Commands.ToList().ForEach(c => Console.WriteLine(c));