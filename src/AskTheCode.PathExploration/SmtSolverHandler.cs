using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Heap;
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
                var heapAssumptions = heap.GetAssumptions();
                if (heapAssumptions.Length > 0)
                {
                    solverResult = this.smtSolver.Solve(this.pathConditionHandler.NameProvider, heapAssumptions);
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
                creator.Nodes.ToImmutableArray(),
                creator.Interpretations.ToImmutableArray(),
                creator.ReferenceModels.ToImmutableArray());
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

            private List<Interpretation> currentNodeInterpretations = new List<Interpretation>();
            private List<Interpretation> nextNodeInterpretations = new List<Interpretation>();
            private List<HeapModelLocation> currentNodeHeapLocations = new List<HeapModelLocation>();
            private List<HeapModelLocation> nextNodeHeapLocations = new List<HeapModelLocation>();
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

            public List<FlowNode> Nodes { get; private set; }

            public List<ImmutableArray<Interpretation>> Interpretations { get; private set; }

            public List<ImmutableArray<HeapModelLocation>> ReferenceModels { get; private set; }

            /// <remarks>
            /// This function is expected to be called only once.
            /// </remarks>
            public void CreateExecutionModel()
            {
                if (this.Nodes != null)
                {
                    throw new InvalidOperationException();
                }

                this.Nodes = new List<FlowNode>();
                this.Interpretations = new List<ImmutableArray<Interpretation>>();
                this.ReferenceModels = new List<ImmutableArray<HeapModelLocation>>();

                var enterNode = this.Path.Node as EnterFlowNode;
                if (enterNode != null)
                {
                    // Simulate passing the arguments from a non-existent call node
                    this.areAssignmentsPostponedToNextNode = true;

                    foreach (var param in enterNode.Parameters)
                    {
                        this.AddVariableValue(this.GetVersioned(param));
                    }

                    this.areAssignmentsPostponedToNextNode = false;
                }

                this.RetractToRoot();

                this.Nodes.Add(this.Path.Node);
                this.RetractStartingNode();
                this.AddNodeValues();
            }

            protected override void OnBeforePathStepRetracted(FlowEdge edge)
            {
                this.Nodes.Add(edge.From);
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
                    && callNode.IsObjectCreation)
                {
                    // "this" must be the first variable in the constructor by convention
                    var thisVar = edge.To.Graph.LocalVariables[0];

                    Contract.Assert(thisVar.IsReference);

                    var versionedVar = new VersionedVariable(thisVar, this.GetVariableVersion(thisVar));
                    this.heapModelRecorder.AllocateNew(versionedVar);
                }
            }

            protected override void OnAfterPathStepRetracted(FlowEdge edge)
            {
                this.AddNodeValues();
            }

            protected override void OnRandomVariableRetracted(FlowVariable variable, int version)
            {
                this.AddVariableValue(new VersionedVariable(variable, version));
            }

            protected override void OnVariableAssignmentRetracted(
                FlowVariable variable,
                int assignedVersion,
                Expression value)
            {
                this.AddVariableValue(new VersionedVariable(variable, assignedVersion));
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
                this.AddVariableValue(result);
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

                this.AddVariableHeapLocation(reference);
            }

            private void AddVariableValue(VersionedVariable variable)
            {
                if (variable.Variable.IsReference)
                {
                    this.AddVariableHeapLocation(variable);
                }
                else
                {
                    this.AddVariableInterpretation(variable);
                }
            }

            private void AddVariableHeapLocation(VersionedVariable variable)
            {
                Contract.Requires(variable.Variable.IsReference);

                var heapLocation = this.heapModelRecorder.GetLocation(variable);
                if (this.areAssignmentsPostponedToNextNode)
                {
                    this.nextNodeHeapLocations.Add(heapLocation);
                }
                else
                {
                    this.currentNodeHeapLocations.Add(heapLocation);
                }
            }

            private void AddVariableInterpretation(VersionedVariable variable)
            {
                Contract.Requires(!variable.Variable.IsReference);

                var symbolName = this.smtContextHandler.GetVariableVersionSymbol(
                    variable.Variable,
                    variable.Version);
                var interpretation = this.smtModel.GetInterpretation(symbolName);
                if (this.areAssignmentsPostponedToNextNode)
                {
                    this.nextNodeInterpretations.Add(interpretation);
                }
                else
                {
                    this.currentNodeInterpretations.Add(interpretation);
                }
            }

            private void AddNodeValues()
            {
                this.Interpretations.Add(this.currentNodeInterpretations.ToImmutableArray());
                this.currentNodeInterpretations.Clear();

                this.ReferenceModels.Add(this.currentNodeHeapLocations.ToImmutableArray());
                this.currentNodeHeapLocations.Clear();
            }
        }
    }
}
