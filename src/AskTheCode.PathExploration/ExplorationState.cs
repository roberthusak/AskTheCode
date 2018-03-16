using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    public class ExplorationState
    {
        public ExplorationState(Path path, CallSiteStack callSiteStack, SmtSolverHandler solverHandler)
        {
            Contract.Requires(path != null);
            Contract.Requires(solverHandler != null);

            this.Path = path;
            this.CallSiteStack = callSiteStack;
            this.SolverHandler = solverHandler;
        }

        public Path Path { get; private set; }

        public CallSiteStack CallSiteStack { get; private set; }

        public SmtSolverHandler SolverHandler { get; internal set; }

        public void Merge(ExplorationState state, SmtSolverHandler solverHandler)
        {
            Contract.Requires(state != null);
            Contract.Requires(solverHandler != null);
            Contract.Requires(state.Path.Node == this.Path.Node);
            Contract.Requires(state.CallSiteStack.Equals(this.CallSiteStack));

            this.Path = new Path(
                this.Path.Preceeding.AddRange(state.Path.Preceeding),
                Math.Max(this.Path.Depth, state.Path.Depth),
                this.Path.Node,
                this.Path.LeadingEdges.AddRange(state.Path.LeadingEdges));
            this.SolverHandler = solverHandler;
        }
    }
}
