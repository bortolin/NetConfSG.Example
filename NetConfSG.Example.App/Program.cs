// See https://aka.ms/new-console-template for more information
using NetConfSG.Example.App;
using System.Reflection;


Console.WriteLine("Application Menu");

// Get all types end with Command name in the current assembly using reflection
//This canno't be use with AOT compilation
//Assembly.GetExecutingAssembly().GetTypes()
//        .Where(t=>t.Name.EndsWith("Command")).ToList()
//        .ForEach(t=>Console.WriteLine(t.Name));

CommandList.Commands.ToList().ForEach(c => Console.WriteLine(c));
