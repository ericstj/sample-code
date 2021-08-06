using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;


namespace generatorBenchmark
{
    public partial class GeneratorsBenchmark
    {
        static readonly string[] AnalyzerFiles = new []
        {
            @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\6.0.0-preview.7.21377.19\analyzers\dotnet\cs\System.Text.Json.SourceGeneration.dll",
            @"C:\Program Files\dotnet\packs\Microsoft.AspNetCore.App.Ref\6.0.0-preview.7.21378.6\analyzers\dotnet\cs\Microsoft.Extensions.Logging.Generators.dll"
        };

        public GeneratorsBenchmark()
        {
            _generators.Add(new NoopGenerator());
            _generators.Add(new NoopSyntaxReceiverGenerator());

            var loader = new AnalyzerAssemblyLoader();
            foreach(var analyzerFile in AnalyzerFiles)
            {
                var analyzerReference = new AnalyzerFileReference(analyzerFile, loader);

                var generators = analyzerReference.GetGenerators(LanguageNames.CSharp);

                _generators.AddRange(generators);
            }
        }
 
        private sealed class AnalyzerAssemblyLoader : IAnalyzerAssemblyLoader
        {
            public void AddDependencyLocation(string fullPath) { }
 
            public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
        }

        List<ISourceGenerator> _generators = new List<ISourceGenerator>();
        public IEnumerable<object> Generators() => _generators.Select(g => new GeneratorParameter(g));


        /// <summary>
        /// Wrap the generator to get a shorter name
        /// </summary>
        public sealed class GeneratorParameter : ISourceGenerator
        {
            private ISourceGenerator Value { get; }
            public GeneratorParameter(ISourceGenerator inner) => Value = inner;
            public override string ToString() => Value.GetType().Name;

            public void Initialize(GeneratorInitializationContext context) => Value.Initialize(context);
            public void Execute(GeneratorExecutionContext context) => Value.Execute(context);
        }

        private static Compilation CreateCompilation(long numTypes)
        {
            const long typesPerFile = 100000;
            List<string> sources = new List<string>();

            long typesWritten = 0;

            while(typesWritten < numTypes)
            {
                var source = new StringBuilder();
                source.AppendLine("namepace MyCode");
                source.AppendLine("{");

                while (typesWritten++ < numTypes)
                {
                    source.AppendLine($"    public class MyPoco{typesWritten}");
                    source.AppendLine(@"
        {
            public void SomeMethod() {}
            public string Prop { get; set; }
        }");
                    if ((typesWritten % typesPerFile) == 0) break;
                }
                source.AppendLine("}");
                sources.Add(source.ToString());
            }

            return CreateCompilation(sources);
        }
        private static Compilation CreateCompilation(IEnumerable<string> sources)
            => CSharpCompilation.Create("compilation",
                sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray(),
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    
}