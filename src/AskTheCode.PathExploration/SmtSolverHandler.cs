using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.ControlFlowGraphs.TypeSystem;
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
            var heap = heapFactory.Create(new SolverSymbolicHeapContext(this));
            this.pathConditionHandler = new PathConditionHandler(contextHandler, smtSolver, path, startingNode, heap);
            this.pathConditionHandler.ProcessStartingNode();
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

            var heap = this.pathConditionHandler.Heap;

            SolverResult solverResult;
            if (!heap.CanBeSatisfiable)
            {
                solverResult = SolverResult.Unsat;
            }
            else
            {
                // TODO: Turn Assumptions into a method to express expected computation
                if (heap.Assumptions.Length > 0)
                {
                    solverResult = this.smtSolver.Solve(heap.Assumptions);
                }
                else
                {
                    solverResult = this.smtSolver.Solve();
                }
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

            var heapModelRecorder = this.pathConditionHandler.Heap.GetModelRecorder(this.smtSolver.Model);
            var creator = new ExecutionModelCreator(
                this.pathConditionHandler,
                this.contextHandler,
                this.smtSolver.Model,
                heapModelRecorder);
            creator.CreateExecutionModel();
            return new ExecutionModel(
                heapModelRecorder.GetModel(),
                creator.NodeStack.ToImmutableArray(),
                creator.InterpretationStack.ToImmutableArray(),
                creator.ReferenceModelStack.ToImmutableArray());
        }

        private class SolverSymbolicHeapContext : ISymbolicHeapContext
        {
            private readonly SmtSolverHandler owner;

            public SolverSymbolicHeapContext(SmtSolverHandler owner)
            {
                this.owner = owner;
            }

            public void AddAssertion(BoolHandle assertion)
            {
                this.owner.smtSolver.AddAssertion(this.owner.pathConditionHandler.NameProvider, assertion);
            }

            public VersionedVariable GetVersioned(FlowVariable variable)
            {
                int version = this.owner.pathConditionHandler.GetVariableVersion(variable);
                return new VersionedVariable(variable, version);
            }

            public NamedVariable GetNamedVariable(VersionedVariable variable)
            {
                var name = this.owner.contextHandler.GetVariableVersionSymbol(variable.Variable, variable.Version);
                return ExpressionFactory.NamedVariable(variable.Variable.Sort, name);
            }

            public NamedVariable CreateVariable(Sort sort, string name = null)
            {
                return this.owner.contextHandler.CreateVariable(sort, name);
            }
        }

        private class ExecutionModelCreator : PathVariableVersionHandler
        {
            private readonly SmtContextHandler smtContextHandler;
            private readonly IModel smtModel;
            private readonly IHeapModelRecorder heapModelRecorder;

            private Stack<Interpretation> currentNodeInterpretations = new Stack<Interpretation>();
            private Stack<Interpretation> nextNodeInterpretations = new Stack<Interpretation>();
            private Stack<HeapModelLocation> currentNodeHeapLocations = new Stack<HeapModelLocation>();
            private Stack<HeapModelLocation> nextNodeHeapLocations = new Stack<HeapModelLocation>();
            private bool areAssignmentsPostponedToNextNode = false;

            public ExecutionModelCreator(
                PathConditionHandler pathConditionHandler,
                SmtContextHandler smtContextHandler,
                IModel smtModel,
                IHeapModelRecorder heapModelRecorder)
                : base(pathConditionHandler)
            {
                this.smtContextHandler = smtContextHandler;
                this.smtModel = smtModel;
                this.heapModelRecorder = heapModelRecorder;
            }

            public Stack<FlowNode> NodeStack { get; private set; }

            public Stack<ImmutableArray<Interpretation>> InterpretationStack { get; private set; }

            public Stack<ImmutableArray<HeapModelLocation>> ReferenceModelStack { get; private set; }

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
                this.ReferenceModelStack = new Stack<ImmutableArray<HeapModelLocation>>();

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
                this.PushNodeInterpretations();
            }

            protected override void OnBeforePathStepRetracted(FlowEdge edge)
            {
                this.NodeStack.Push(edge.From);
                this.areAssignmentsPostponedToNextNode = edge is OuterFlowEdge;

                // Swap the next node interpretations with the emptied stack of the current one making it ready for the
                // current node
                var hlpInt = this.currentNodeInterpretations;
                this.currentNodeInterpretations = this.nextNodeInterpretations;
                this.nextNodeInterpretations = hlpInt;

                // Do the same in the case of reference models
                var hlpRef = this.currentNodeHeapLocations;
                this.currentNodeHeapLocations = this.nextNodeHeapLocations;
                this.nextNodeHeapLocations = hlpRef;

                // Identify the creation of new objects on the heap
                if (edge is OuterFlowEdge outerEdge
                    && outerEdge.Kind == OuterFlowEdgeKind.MethodCall
                    && outerEdge.From is CallFlowNode callNode
                    && callNode.IsConstructorCall)
                {
                    var newVar = callNode.ReturnAssignments[0];
                    var versionedVar = new VersionedVariable(newVar, this.GetVariableVersion(newVar));
                    this.heapModelRecorder.AllocateNew(versionedVar);
                }
            }

            protected override void OnAfterPathStepRetracted(FlowEdge edge)
            {
                this.PushNodeInterpretations();
            }

            protected override void OnRandomVariableRetracted(FlowVariable variable, int version)
            {
                this.PushInterpretation(new VersionedVariable(variable, version));
            }

            protected override void OnVariableAssignmentRetracted(
                FlowVariable variable,
                int assignedVersion,
                Expression value)
            {
                this.PushInterpretation(new VersionedVariable(variable, assignedVersion));
            }

            protected override void OnReferenceEqualityRetracted(
                bool areEqual,
                VersionedVariable left,
                VersionedVariable right)
            {
                // TODO: Store the comparison result to the interpretation stack when expected in ViewModel

                // Only let the heap model know that we are interested in them
                // so that it displays them later
                this.heapModelRecorder.GetLocation(left);
                this.heapModelRecorder.GetLocation(right);
            }

            protected override void OnFieldReadRetracted(
                VersionedVariable result,
                VersionedVariable reference,
                IFieldDefinition field)
            {
                this.heapModelRecorder.ReadField(reference, field);
                this.PushInterpretation(result);
            }

            protected override void OnFieldWriteRetracted(
                VersionedVariable reference,
                IFieldDefinition field,
                Expression value)
            {
                if (value.Sort == References.Sort)
                {
                    if (!(value is FlowVariable valVar))
                    {
                        throw new NotSupportedException("Only versioned flow variables supported as references");
                    }

                    this.heapModelRecorder.WriteReferenceField(reference, field, this.GetVersioned(valVar));
                }
                else
                {
                    var valueInterpretation = this.smtModel.GetInterpretation(this.NameProvider, value);
                    this.heapModelRecorder.WriteValueField(reference, field, valueInterpretation);
                }
            }

            private void PushInterpretation(VersionedVariable variable)
            {
                if (variable.Variable.IsReference)
                {
                    var heapLocation = this.heapModelRecorder.GetLocation(variable);
                    if (this.areAssignmentsPostponedToNextNode)
                    {
                        this.nextNodeHeapLocations.Push(heapLocation);
                    }
                    else
                    {
                        this.currentNodeHeapLocations.Push(heapLocation);
                    }
                }
                else
                {
                    var symbolName = this.smtContextHandler.GetVariableVersionSymbol(
                        variable.Variable,
                        variable.Version);
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

            private void PushNodeInterpretations()
            {
                this.InterpretationStack.Push(this.currentNodeInterpretations.ToImmutableArray());
                this.currentNodeInterpretations.Clear();

                this.ReferenceModelStack.Push(this.currentNodeHeapLocations.ToImmutableArray());
                this.currentNodeHeapLocations.Clear();
            }
        }
    }
}
