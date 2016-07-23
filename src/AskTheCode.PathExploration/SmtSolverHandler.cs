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
        private SmtContextHandler contextHandler;
        private ISolver smtSolver;
        private ExplorationResult lastResult;

        private VersionedNameProvider nameProvider;
        private FlowGraphsVariableOverlay<int> variableVersions = new FlowGraphsVariableOverlay<int>();

        internal SmtSolverHandler(
            SmtContextHandler contextHandler,
            ISolver smtSolver,
            Path path,
            StartingNodeInfo startingNode)
            : this(contextHandler, smtSolver, path)
        {
            this.smtSolver.Push();

            var innerNode = startingNode.Node as InnerFlowNode;
            if (innerNode != null && startingNode.AssignmentIndex != null)
            {
                var initialAssignments = innerNode.Assignments
                    .Take(startingNode.AssignmentIndex.Value + 1)
                    .Reverse();
                this.AssertAssignments(initialAssignments);
            }
        }

        private SmtSolverHandler(SmtContextHandler contextHandler, ISolver smtSolver, Path path)
        {
            this.contextHandler = contextHandler;
            this.smtSolver = smtSolver;
            this.Path = path;

            this.nameProvider = new VersionedNameProvider(this);
        }

        public Path Path { get; private set; }

        public ExplorationResultKind? LastResultKind { get; private set; }

        public ExplorationResult LastResult
        {
            get { return this.GetLastResultImpl(); }
        }

        public SmtSolverHandler Clone()
        {
            var cloned = new SmtSolverHandler(this.contextHandler, this.smtSolver, this.Path);
            cloned.lastResult = this.lastResult;
            cloned.LastResultKind = this.LastResultKind;

            // TODO: Clone the variable versions! (we need to make the overlay cloneable/enumerable)
            throw new NotImplementedException();
        }

        public ExplorationResultKind Solve(Path path)
        {
            Contract.Requires(path != null);
            Contract.Ensures(this.Path == path);
            Contract.Ensures(this.LastResultKind != null);

            int popCount = 0;
            var pathStack = new Stack<Path>();
            var currentRetracting = this.Path;
            var targetRetracting = path;
            while (currentRetracting != targetRetracting)
            {
                if (currentRetracting.Depth > targetRetracting.Depth)
                {
                    popCount++;

                    // TODO: Handle merged nodes
                    currentRetracting = currentRetracting.Preceeding.Single();
                }
                else
                {
                    pathStack.Push(targetRetracting);

                    // TODO: Handle merged nodes
                    targetRetracting = targetRetracting.Preceeding.Single();
                }
            }

            if (popCount > 0)
            {
                this.RetractVariableVersions(popCount);
                this.smtSolver.Pop(popCount);
            }

            this.Path = currentRetracting;

            while (pathStack.Count > 0)
            {
                var currentPath = pathStack.Pop();

                this.smtSolver.Push();

                // TODO: Handle merged nodes
                var edge = currentPath.LeadingEdges.Single();
                var node = currentPath.Node;

                this.AssertEdgeCondition(edge);
                var innerNode = currentPath.Node as InnerFlowNode;
                if (innerNode != null)
                {
                    this.AssertAssignments(innerNode.Assignments.Reverse());
                }

                this.Path = currentPath;
            }

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
                var innerNode = node as InnerFlowNode;
                if (innerNode != null)
                {
                    var interpretations = new List<Interpretation>();
                    foreach (var assignment in innerNode.Assignments.Reverse())
                    {
                        int variableVersion = variableVersions[assignment.Variable];
                        var variableSymbolName = this.contextHandler.GetVariableVersionSymbol(assignment.Variable, variableVersion);
                        variableVersions[assignment.Variable]++;

                        var interpretation = this.smtSolver.Model.GetInterpretation(variableSymbolName);
                        interpretations.Add(interpretation);
                    }

                    nodeInterpretations.Add(interpretations.ToImmutableArray());
                }
                else
                {
                    nodeInterpretations.Add(ImmutableArray<Interpretation>.Empty);
                }
            }

            return new ExecutionModel(pathNodes.ToImmutableArray(), nodeInterpretations.ToImmutableArray());
        }

        private void RetractVariableVersions(int nodeCount)
        {
            Contract.Requires(nodeCount <= this.Path.Depth);

            var path = this.Path;
            for (int i = 0; i < nodeCount; i++)
            {
                var innerNode = path.Node as InnerFlowNode;
                if (innerNode != null)
                {
                    // The order of the assertions is not important here
                    foreach (var assignment in innerNode.Assignments)
                    {
                        this.variableVersions[assignment.Variable]--;
                        Contract.Assert(this.variableVersions[assignment.Variable] >= 0);
                    }
                }

                // TODO: Handle merged nodes
                path = path.Preceeding.Single();
            }
        }

        // TODO: Handle also border nodes and interprocedural value flow
        private void AssertEdgeCondition(FlowEdge edge)
        {
            if (edge.Condition.Expression != ExpressionFactory.True)
            {
                this.smtSolver.AddAssertion(this.nameProvider, edge.Condition);
            }
        }

        private void AssertAssignments(IEnumerable<Assignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                this.variableVersions[assignment.Variable]++;
                var assignmentWrapper = new FlowVariableAssignmentWrapper(assignment.Variable);
                var equal = (BoolHandle)ExpressionFactory.Equal(assignmentWrapper, assignment.Value);
                this.smtSolver.AddAssertion(this.nameProvider, equal);
            }
        }

        private class VersionedNameProvider : INameProvider<Variable>
        {
            private SmtSolverHandler owner;

            public VersionedNameProvider(SmtSolverHandler owner)
            {
                this.owner = owner;
            }

            public SymbolName GetName(Variable variable)
            {
                bool assignment = false;

                var flowVariable = variable as FlowVariable;
                if (flowVariable == null)
                {
                    var assignmentWrapper = variable as FlowVariableAssignmentWrapper;
                    if (assignmentWrapper != null)
                    {
                        flowVariable = assignmentWrapper.Variable;
                        assignment = true;
                    }
                }

                if (flowVariable == null)
                {
                    throw new InvalidOperationException();
                }

                int version = this.owner.variableVersions[flowVariable];
                if (assignment)
                {
                    // In case of assignment, the recently raised version should be applied only to the right side
                    Contract.Assert(version > 0);
                    version--;
                }

                return this.owner.contextHandler.GetVariableVersionSymbol(flowVariable, version);
            }
        }
    }
}
