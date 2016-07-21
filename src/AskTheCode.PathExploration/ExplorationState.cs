using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.PathExploration
{
    public class ExplorationState
    {
        public ExplorationState(Path path, SmtSolverHandler solverHandler)
        {
            Contract.Requires(path != null);
            Contract.Requires(solverHandler != null);

            this.Path = path;
            this.SolverHandler = solverHandler;
        }

        public Path Path { get; private set; }

        public SmtSolverHandler SolverHandler { get; internal set; }

        public void Merge(ExplorationState node, SmtSolverHandler solverHandler)
        {
            Contract.Requires(node != null);
            Contract.Requires(solverHandler != null);
            Contract.Requires(node.Path.Node == this.Path.Node);

            this.Path = new Path(
                this.Path.Preceeding.AddRange(node.Path.Preceeding),
                Math.Max(this.Path.Depth, node.Path.Depth),
                this.Path.Node,
                this.Path.LeadingEdges.AddRange(node.Path.LeadingEdges));
            this.SolverHandler = solverHandler;
        }
    }
}
