using Microsoft.CodeAnalysis.CSharp;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuilderVisitor : CSharpSyntaxVisitor
    {
        public BuilderVisitor(IBuildingContext context)
        {
            this.Context = context;
        }

        protected IBuildingContext Context { get; private set; }
    }
}
