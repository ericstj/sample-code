using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace generatorBenchmark
{

    public partial class GeneratorsBenchmark
    {
        private Compilation[]? _manySingleTypeCompilations;
        [GlobalSetup(Target = nameof(ManySingleTypeCompilations))]
        public void SetupManySingleTypeCompilations()
        {
            var compilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");
            _manySingleTypeCompilations = Enumerable.Repeat(compilation, 10).ToArray();
        }
        
        [GlobalCleanup(Target = nameof(ManySingleTypeCompilations))]
        public void CleanupManySingleTypeCompilations() => _manySingleTypeCompilations = null;

        [Benchmark]
        [ArgumentsSource(nameof(Generators))]
        public void ManySingleTypeCompilations(ISourceGenerator generator)
        {
            GeneratorDriver  driver = CSharpGeneratorDriver.Create(generator);
            foreach(var compilation in _manySingleTypeCompilations!)
            {
                driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            }
        }
    }
    
}