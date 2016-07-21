using Microsoft.CodeAnalysis.CSharp;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuilderVisitor : CSharpSyntaxVisitor
    {
        public BuilderVisitor(CSharpGraphBuilder.BuildingContext context)
        {
            this.Context = context;
        }

        protected CSharpGraphBuilder.BuildingContext Context { get; private set; }
    }
}
