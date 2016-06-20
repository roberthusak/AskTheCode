using Microsoft.CodeAnalysis.CSharp;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuilderVisitor : CSharpSyntaxVisitor
    {
        public BuilderVisitor(CSharpFlowGraphBuilder.BuildingContext context)
        {
            this.Context = context;
        }

        protected CSharpFlowGraphBuilder.BuildingContext Context { get; private set; }
    }
}
