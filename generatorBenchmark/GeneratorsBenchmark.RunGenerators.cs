using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace generatorBenchmark
{

    public partial class GeneratorsBenchmark
    {
        private Compilation[]? _compilations;

        [Params(1, 10)]
        public int Compilations {get; set;}
        
        [Params(1, 100, 1000, 10000)]
        public long Types {get; set;}


        [GlobalSetup(Target = nameof(RunGenerators))]
        public void SetupRunGenerators()
        {
            var compilation = CreateCompilation(Types);
            _compilations = Enumerable.Repeat(compilation, Compilations).ToArray();
        }
        
        [GlobalCleanup(Target = nameof(RunGenerators))]
        public void CleanupRunGenerators() => _compilations = null;

        [Benchmark]
        [ArgumentsSource(nameof(Generators))]
        public void RunGenerators(ISourceGenerator generator)
        {
            GeneratorDriver  driver = CSharpGeneratorDriver.Create(generator);
            foreach(var compilation in _compilations!)
            {
                driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
            }
        }
    }
    
}