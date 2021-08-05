
using Microsoft.CodeAnalysis;


namespace generatorBenchmark
{
    public class NoopGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }

    
    public class NoopSyntaxReceiverGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new NoopSyntaxReceiver());
        }

        private class NoopSyntaxReceiver : ISyntaxReceiver
        {
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            { } 
        }
    }
    
}