using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace generatorBenchmark
{

    public partial class GeneratorsBenchmark
    {
        private Compilation? _singleTypeCompilation;
        [GlobalSetup(Target = nameof(SingleType))]
        public void SetupSingleType()
        {
            _singleTypeCompilation = CreateCompilation(@"
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
        }
        
        [GlobalCleanup(Target = nameof(SingleType))]
        public void CleanupSingleType() => _singleTypeCompilation = null;

        [Benchmark]
        [ArgumentsSource(nameof(Generators))]
        public void SingleType(ISourceGenerator generator)
        {
            GeneratorDriver  driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(_singleTypeCompilation!, out var outputCompilation, out var diagnostics);
        }
    }
    
}