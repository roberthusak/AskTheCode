using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.PathExploration
{
    public class SmtSolverHandler
    {
        private readonly SmtContextHandler contextHandler;
        private readonly ISolver smtSolver;
        private readonly PathConditionHandler pathConditionHandler;

        private ExplorationResult lastResult;

        internal SmtSolverHandler(
            SmtContextHandler contextHandler,
            ISolver smtSolver,
            Path path,
            StartingNodeInfo startingNode)
            : this(
                contextHandler,
                smtSolver,
                new PathConditionHandler(contextHandler, smtSolver, path, startingNode))
        {
        }

        private SmtSolverHandler(SmtContextHandler contextHandler, ISolver smtSolver, PathConditionHandler pathConditionHandler)
        {
            this.contextHandler = contextHandler;
            this.smtSolver = smtSolver;
            this.pathConditionHandler = pathConditionHandler;
        }

        public Path Path => this.pathConditionHandler.Path;

        public ExplorationResultKind? LastResultKind { get; private set; }

        public ExplorationResult LastResult
        {
            get { return this.GetLastResultImpl(); }
        }

        public SmtSolverHandler Clone()
        {
            var cloned = new SmtSolverHandler(this.contextHandler, this.smtSolver, this.pathConditionHandler);
            cloned.lastResult = this.lastResult;
            cloned.LastResultKind = this.LastResultKind;

            // TODO: Clone the underlying SMT solver and path condition handler!
            // TODO: Clone the variable versions of the latter! (we need to make the overlay cloneable/enumerable)
            throw new NotImplementedException();
        }

        public ExplorationResultKind Solve(Path path)
        {
            Contract.Requires(path != null);
            Contract.Ensures(this.Path == path);
            Contract.Ensures(this.LastResultKind != null);

            this.pathConditionHandler.Update(path);
            Contract.Assert(this.Path == path);

            var solverResult = this.smtSolver.Solve();

            switch (solverResult)
            {
                case SolverResult.Sat:
                    this.LastResultKind = ExplorationResultKind.Reachable;
                    break;
                case SolverResult.Unsat:
                    this.LastResultKind = ExplorationResultKind.Unreachable;
                    break;
                case SolverResult.Unknown:
                default:
                    Contract.Assert(solverResult == SolverResult.Unknown);
                    this.LastResultKind = ExplorationResultKind.Unknown;
                    break;
            }

            // Force to recreate it next time
            this.lastResult = null;

            return this.LastResultKind.Value;
        }

        private ExplorationResult GetLastResultImpl()
        {
            Contract.Requires(this.LastResultKind != null);

            if (this.lastResult == null)
            {
                switch (this.LastResultKind.Value)
                {
                    case ExplorationResultKind.Unknown:
                        this.lastResult = ExplorationResult.CreateUnknown();
                        break;
                    case ExplorationResultKind.Unreachable:
                        var counterExample = this.CreatePathCounterExample();
                        this.lastResult = ExplorationResult.CreateUnreachable(counterExample);
                        break;
                    case ExplorationResultKind.Reachable:
                    default:
                        Contract.Assert(this.LastResultKind.Value == ExplorationResultKind.Reachable);
                        var executionModel = this.CreateExecutionModel();
                        this.lastResult = ExplorationResult.CreateReachable(executionModel);
                        break;
                }
            }

            return this.lastResult;
        }

        private PathCounterExample CreatePathCounterExample()
        {
            Contract.Requires(this.smtSolver.UnsatisfiableCore != null);

            // TODO
            return new PathCounterExample();
        }

        // TODO: Handle merged nodes
        private ExecutionModel CreateExecutionModel()
        {
            Contract.Requires(this.smtSolver.Model != null);

            var smtModel = this.smtSolver.Model;
            var pathNodes = new List<FlowNode>();

            // TODO: Include root
            for (var path = this.Path; !path.IsRoot; path = path.Preceeding.Single())
            {
                pathNodes.Add(path.Node);
            }

            pathNodes.Reverse();

            var variableVersions = new FlowGraphsVariableOverlay<int>();
            var nodeInterpretations = new List<ImmutableArray<Interpretation>>();

            foreach (var node in pathNodes)
            {
                List<Interpretation> interpretations = null;

                var innerNode = node as InnerFlowNode;
                if (innerNode != null)
                {
                    interpretations = new List<Interpretation>();
                    foreach (var assignment in innerNode.Assignments.Reverse())
                    {
                        var interpretation = this.GetVariableInterpretation(variableVersions, assignment.Variable);
                        interpretations.Add(interpretation);
                        variableVersions[assignment.Variable]++;
                    }
                }
                else if (node is EnterFlowNode)
                {
                    var enterNode = node as EnterFlowNode;
                    interpretations = new List<Interpretation>();
                    foreach (var param in enterNode.Parameters.Reverse())
                    {
                        var interpretation = this.GetVariableInterpretation(variableVersions, param);
                        interpretations.Add(interpretation);
                    }
                }
                else if (node is CallFlowNode)
                {
                    var callNode = node as CallFlowNode;
                    interpretations = new List<Interpretation>();
                    foreach (var assignedVariable in callNode.ReturnAssignments.Reverse())
                    {
                        var interpretation = this.GetVariableInterpretation(variableVersions, assignedVariable);
                        interpretations.Add(interpretation);
                        variableVersions[assignedVariable]++;
                    }
                }

                if (interpretations != null)
                {
                    nodeInterpretations.Add(interpretations.ToImmutableArray());
                }
                else
                {
                    nodeInterpretations.Add(ImmutableArray<Interpretation>.Empty);
                }
            }

            return new ExecutionModel(pathNodes.ToImmutableArray(), nodeInterpretations.ToImmutableArray());
        }

        private Interpretation GetVariableInterpretation(FlowGraphsVariableOverlay<int> variableVersions, FlowVariable variable)
        {
            int version = variableVersions[variable];
            var symbolName = this.contextHandler.GetVariableVersionSymbol(variable, version);

            var interpretation = this.smtSolver.Model.GetInterpretation(symbolName);
            return interpretation;
        }
    }
}
