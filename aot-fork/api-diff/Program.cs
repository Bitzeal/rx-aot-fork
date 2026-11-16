using System.Reflection;
using System.Runtime.InteropServices;
using PublicApiGenerator;

// Prints the public API of the assembly given as the first argument (lib or ref assembly).
// Assembly metadata (repository URL, commit) differs between builds and is left out.
var path = Path.GetFullPath(args[0]);
var resolver = new PathAssemblyResolver(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll").Append(path));
using var context = new MetadataLoadContext(resolver);
var assembly = context.LoadFromAssemblyPath(path);
Console.Write(assembly.GeneratePublicApi(new ApiGeneratorOptions
{
    AllowNamespacePrefixes = ["System", "Microsoft"],
    ExcludeAttributes = ["System.Reflection.AssemblyMetadataAttribute"],
}));
