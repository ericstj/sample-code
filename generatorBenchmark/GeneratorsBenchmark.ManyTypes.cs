using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace generatorBenchmark
{

    public partial class GeneratorsBenchmark
    {
        private Compilation? _manyTypesCompilation;
        [GlobalSetup(Target = nameof(ManyTypes))]
        public void SetupManyTypes()
        {
            var source = new StringBuilder();
            source.AppendLine("namepace MyCode");
            source.AppendLine("{");

            for (int i = 0; i < 10; i++)
            {
                source.AppendLine($"    public class MyPoco{i}");
                source.AppendLine(@"
    {
        public void SomeMethod() {}
        public string Prop { get; set; }
    }");
            }
            source.AppendLine("}");

            _manyTypesCompilation = CreateCompilation(source.ToString());
        }
        
        [GlobalCleanup(Target = nameof(ManyTypes))]
        public void CleanupManyTypes() => _manyTypesCompilation = null;

        [Benchmark]
        [ArgumentsSource(nameof(Generators))]
        public void ManyTypes(ISourceGenerator generator)
        {
            GeneratorDriver  driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGeneratorsAndUpdateCompilation(_singleTypeCompilation!, out var outputCompilation, out var diagnostics);
        }
    }
    
}