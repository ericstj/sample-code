using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public IEnumerable<object> Generators() => _generators;


        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    
}