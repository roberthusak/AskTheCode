using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Operations;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphBuilder
    {
        private FlowNodeId.Provider nodeIdProvider;
        private InnerFlowEdgeId.Provider edgeIdProvider;
        private LocalFlowVariableId.Provider variableIdProvider;

        public FlowGraphBuilder()
        {
        }

        public FlowGraphBuilder(FlowGraphId newGraphId)
        {
            this.StartBuildingNewGraph(newGraphId);
        }

        public FlowGraph Graph { get; private set; }

        public void StartBuildingNewGraph(FlowGraphId newGraphId)
        {
            Contract.Requires<InvalidOperationException>(this.Graph == null);

            this.nodeIdProvider = new FlowNodeId.Provider();
            this.edgeIdProvider = new InnerFlowEdgeId.Provider();
            this.variableIdProvider = new LocalFlowVariableId.Provider();

            this.Graph = new FlowGraph(newGraphId, this);
        }

        public FrozenHandler<FlowGraph> FreezeAndReleaseGraph()
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires(this.Graph.Builder == this);
            Contract.Requires(this.Graph.CanFreeze);

            // TODO: Convert back to this when supported
            ////Contract.Ensures(this.Graph == null);
            ////Contract.Ensures(Contract.Result<FrozenHandler<FlowGraph>>().Value != null);
            ////Contract.Ensures(Contract.Result<FrozenHandler<FlowGraph>>().Value.Builder == null);

            var result = this.Graph.Freeze();

            Contract.Assert(this.Graph == null);
            Contract.Assert(result.Value != null);
            Contract.Assert(result.Value.Builder == null);
            return result;
        }

        public EnterFlowNode AddEnterNode(IEnumerable<FlowVariable> parameters = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new EnterFlowNode(this.Graph, nodeId, parameters ?? Enumerable.Empty<FlowVariable>());
            this.Graph.MutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.Graph.MutableNodes.IndexOf(node));

            return node;
        }

        // TODO: Add even more overloads (for example, consider directly using immutable array) and optimize their calls
        public InnerFlowNode AddInnerNode(IEnumerable<Operation> operations = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new InnerFlowNode(this.Graph, nodeId, operations ?? Enumerable.Empty<Operation>());
            this.Graph.MutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.Graph.MutableNodes.IndexOf(node));

            return node;
        }

        public InnerFlowNode AddInnerNode(params Operation[] operations)
        {
            return this.AddInnerNode(operations.AsEnumerable());
        }

        public CallFlowNode AddCallNode(
            IRoutineLocation location,
            IEnumerable<Expression> arguments = null,
            IEnumerable<FlowVariable> returnAssignments = null,
            CallKind kind = CallKind.Static)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(location != null, nameof(location));
            Contract.Requires<ArgumentException>(kind != CallKind.ObjectCreation || location.IsConstructor, nameof(kind));

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new CallFlowNode(
                this.Graph,
                nodeId,
                location,
                arguments ?? Enumerable.Empty<Expression>(),
                returnAssignments ?? Enumerable.Empty<FlowVariable>(),
                kind);
            this.Graph.MutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.Graph.MutableNodes.IndexOf(node));

            return node;
        }

        public ReturnFlowNode AddReturnNode(IEnumerable<Expression> returnValues = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new ReturnFlowNode(this.Graph, nodeId, returnValues ?? Enumerable.Empty<Expression>());
            this.Graph.MutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.Graph.MutableNodes.IndexOf(node));

            return node;
        }

        public ThrowExceptionFlowNode AddThrowExceptionNode(
            IRoutineLocation constructorLocation,
            IEnumerable<Expression> arguments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(constructorLocation != null, nameof(constructorLocation));

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new ThrowExceptionFlowNode(
                this.Graph,
                nodeId,
                constructorLocation,
                arguments ?? Enumerable.Empty<Expression>());
            this.Graph.MutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.Graph.MutableNodes.IndexOf(node));

            return node;
        }

        public InnerFlowEdge AddEdge(FlowNode from, FlowNode to)
        {
            return this.AddEdge(from, to, true);
        }

        public InnerFlowEdge AddEdge(FlowNode from, FlowNode to, BoolHandle condition)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(from != null, nameof(from));
            Contract.Requires<ArgumentNullException>(to != null, nameof(to));
            Contract.Requires<ArgumentException>(from.Graph == this.Graph, nameof(from));
            Contract.Requires<ArgumentException>(to.Graph == this.Graph, nameof(to));

            var edgeId = this.edgeIdProvider.GenerateNewId();
            var edge = new InnerFlowEdge(edgeId, from, to, condition);
            this.Graph.MutableEdges.Add(edge);
            Contract.Assert(edgeId.Value == this.Graph.MutableEdges.IndexOf(edge));

            edge.From.MutableOutgoingEdges.Add(edge);
            edge.To.MutableIngoingEdges.Add(edge);

            return edge;
        }

        public LocalFlowVariable AddLocalVariable(Sort sort, string displayName = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));

            return this.AddLocalVariableImpl(sort, displayName, null);
        }

        public LocalFlowVariable AddLocalVariable(
            Sort sort,
            Func<LocalFlowVariableId, string> displayNameCallback)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));
            Contract.Requires<ArgumentNullException>(displayNameCallback != null, nameof(displayNameCallback));

            return this.AddLocalVariableImpl(sort, null, displayNameCallback);
        }

        public ReferenceComparisonVariable AddReferenceComparisonVariable(
            bool areEqual,
            FlowVariable left,
            FlowVariable right)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(left != null, nameof(left));
            Contract.Requires<ArgumentNullException>(right != null, nameof(right));

            return this.AddReferenceComparisonVariableImpl(areEqual, left, right);
        }

        internal void ReleaseGraph()
        {
            this.Graph = null;
        }

        private LocalFlowVariable AddLocalVariableImpl(
            Sort sort,
            string displayName = null,
            Func<LocalFlowVariableId, string> displayNameCallback = null)
        {
            var variableId = this.variableIdProvider.GenerateNewId();

            if (displayName == null && displayNameCallback != null)
            {
                displayName = displayNameCallback.Invoke(variableId);
            }

            var variable = new LocalFlowVariable(this.Graph, variableId, sort, displayName);
            this.Graph.MutableLocalVariables.Add(variable);
            Contract.Assert(variableId.Value == this.Graph.MutableLocalVariables.IndexOf(variable));

            return variable;
        }

        private ReferenceComparisonVariable AddReferenceComparisonVariableImpl(bool areEqual, FlowVariable left, FlowVariable right)
        {
            var variableId = this.variableIdProvider.GenerateNewId();

            // TODO: Cache the existing variables
            var variable = new ReferenceComparisonVariable(this.Graph, variableId, areEqual, left, right);
            this.Graph.MutableLocalVariables.Add(variable);
            Contract.Assert(variableId.Value == this.Graph.MutableLocalVariables.IndexOf(variable));

            return variable;
        }
    }
}
