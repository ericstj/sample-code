using Microsoft.Extensions.DependencyModel;

var ctx= DependencyContext.Default;

Console.WriteLine($"Runtime libraries: {ctx.RuntimeLibraries.Count}");
Console.WriteLine($"Duplicates: {ctx.RuntimeLibraries.Count - ctx.RuntimeLibraries.Distinct().Count()}");

foreach (var type in  ctx.RuntimeLibraries.GroupBy(l => l.Type))
{
    Console.WriteLine($"Type {type.Key}: {type.Count()}");
}
