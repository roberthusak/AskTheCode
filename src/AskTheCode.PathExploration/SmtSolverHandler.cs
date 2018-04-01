using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.PathExploration.Heap;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

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
            StartingNodeInfo startingNode,
            ISymbolicHeapFactory heapFactory)
            : this(
                contextHandler,
                smtSolver,
                null)
        {
            var heap = heapFactory.Create(new SymbolicHeapContext(this));
            this.pathConditionHandler = new PathConditionHandler(contextHandler, smtSolver, path, startingNode, heap);
        }

        private SmtSolverHandler(
            SmtContextHandler contextHandler,
            ISolver smtSolver,
            PathConditionHandler pathConditionHandler)
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
            // TODO: Clone the symbolic heap!
            throw new NotImplementedException();
        }

        public ExplorationResultKind Solve(Path path)
        {
            Contract.Requires(path != null);

            this.pathConditionHandler.Update(path);
            Contract.Assert(this.Path == path);

            SolverResult solverResult;
            if (!this.pathConditionHandler.Heap.CanBeSatisfiable)
            {
                solverResult = SolverResult.Unsat;
            }
            else
            {
                solverResult = this.smtSolver.Solve();
            }

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

            // TODO: Turn back to Contract.Ensures when possible
            Contract.Assert(this.Path == path);
            Contract.Assert(this.LastResultKind != null);

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

            var creator = new ExecutionModelCreator(
                this.pathConditionHandler,
                this.contextHandler,
                this.smtSolver.Model);
            creator.CreateExecutionModel();
            return new ExecutionModel(
                creator.NodeStack.ToImmutableArray(),
                creator.InterpretationStack.ToImmutableArray(),
                creator.ReferenceModelStack.ToImmutableArray());
        }

        private class SymbolicHeapContext : ISymbolicHeapContext
        {
            private readonly SmtSolverHandler parent;

            public SymbolicHeapContext(SmtSolverHandler parent)
            {
                this.parent = parent;
            }
        }

        private class ExecutionModelCreator : PathVariableVersionHandler
        {
            private readonly SmtContextHandler smtContextHandler;
            private readonly IModel smtModel;

            private Stack<Interpretation> currentNodeInterpretations = new Stack<Interpretation>();
            private Stack<Interpretation> nextNodeInterpretations = new Stack<Interpretation>();
            private Stack<IReferenceModel> currentNodeReferenceModels = new Stack<IReferenceModel>();
            private Stack<IReferenceModel> nextNodeReferenceModels = new Stack<IReferenceModel>();
            private bool areAssignmentsPostponedToNextNode = false;

            public ExecutionModelCreator(
                PathConditionHandler pathConditionHandler,
                SmtContextHandler smtContextHandler,
                IModel smtModel)
                : base(pathConditionHandler, new EmptySymbolicHeapContext())
            {
                this.smtContextHandler = smtContextHandler;
                this.smtModel = smtModel;
            }

            public Stack<FlowNode> NodeStack { get; private set; }

            public Stack<ImmutableArray<Interpretation>> InterpretationStack { get; private set; }

            public Stack<ImmutableArray<IReferenceModel>> ReferenceModelStack { get; private set; }

            /// <remarks>
            /// This function is expected to be called only once.
            /// </remarks>
            public void CreateExecutionModel()
            {
                if (this.NodeStack != null)
                {
                    throw new InvalidOperationException();
                }

                this.NodeStack = new Stack<FlowNode>();
                this.InterpretationStack = new Stack<ImmutableArray<Interpretation>>();
                this.ReferenceModelStack = new Stack<ImmutableArray<IReferenceModel>>();

                var enterNode = this.Path.Node as EnterFlowNode;
                if (enterNode != null)
                {
                    foreach (var param in enterNode.Parameters)
                    {
                        int version = this.GetVariableVersion(param);
                        var symbolName = this.smtContextHandler.GetVariableVersionSymbol(param, version);
                        var interpretation = this.smtModel.GetInterpretation(symbolName);
                        this.nextNodeInterpretations.Push(interpretation);
                    }
                }

                this.RetractToRoot();

                this.NodeStack.Push(this.Path.Node);
                this.RetractStartingNode();
                this.OnAfterPathStepRetracted();
            }

            protected override void OnBeforePathStepRetracted(FlowEdge retractingEdge)
            {
                this.NodeStack.Push(retractingEdge.From);
                this.areAssignmentsPostponedToNextNode = retractingEdge is OuterFlowEdge;

                // Swap the next node interpretations with the emptied stack of the current one making it ready for the
                // current node
                var hlpInt = this.currentNodeInterpretations;
                this.currentNodeInterpretations = this.nextNodeInterpretations;
                this.nextNodeInterpretations = hlpInt;

                // Do the same in the case of reference models
                var hlpRef = this.currentNodeReferenceModels;
                this.currentNodeReferenceModels = this.nextNodeReferenceModels;
                this.nextNodeReferenceModels = hlpRef;
            }

            protected override void OnRandomVariableRetracted(FlowVariable variable, int version)
            {
                this.PushInterpretation(variable, version);
            }

            protected override void OnVariableAssignmentRetracted(
                FlowVariable variable,
                int assignedVersion,
                Expression value)
            {
                this.PushInterpretation(variable, assignedVersion);
            }

            protected override void OnAfterPathStepRetracted()
            {
                this.InterpretationStack.Push(this.currentNodeInterpretations.ToImmutableArray());
                this.currentNodeInterpretations.Clear();

                this.ReferenceModelStack.Push(this.currentNodeReferenceModels.ToImmutableArray());
                this.currentNodeReferenceModels.Clear();
            }

            private void PushInterpretation(FlowVariable variable, int assignedVersion)
            {
                if (variable.IsReference)
                {
                    var versionedVar = new VersionedVariable(variable, assignedVersion);
                    var referenceModel = this.Heap.GetReferenceModel(this.smtModel, versionedVar);
                    if (this.areAssignmentsPostponedToNextNode)
                    {
                        this.nextNodeReferenceModels.Push(referenceModel);
                    }
                    else
                    {
                        this.currentNodeReferenceModels.Push(referenceModel);
                    }
                }
                else
                {
                    var symbolName = this.smtContextHandler.GetVariableVersionSymbol(variable, assignedVersion);
                    var interpretation = this.smtModel.GetInterpretation(symbolName);
                    if (this.areAssignmentsPostponedToNextNode)
                    {
                        this.nextNodeInterpretations.Push(interpretation);
                    }
                    else
                    {
                        this.currentNodeInterpretations.Push(interpretation);
                    }
                }
            }

            private class EmptySymbolicHeapContext : ISymbolicHeapContext
            {
            }
        }
    }
}
