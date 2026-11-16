using System.Reflection;
using System.Runtime.Loader;
using PublicApiGenerator;

// Prints the public API of the System.Reactive.dll given as the first argument.
var context = new AssemblyLoadContext(args[0], isCollectible: false);
var assembly = context.LoadFromAssemblyPath(args[0]);
Console.Write(ApiGenerator.GeneratePublicApi(assembly, new ApiGeneratorOptions { AllowNamespacePrefixes = ["System", "Microsoft"] }));
