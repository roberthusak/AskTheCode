using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphBuilder
    {
        private FlowNodeId.Provider nodeIdProvider;
        private FlowEdgeId.Provider edgeIdProvider;
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
            this.edgeIdProvider = new FlowEdgeId.Provider();
            this.variableIdProvider = new LocalFlowVariableId.Provider();

            this.Graph = new FlowGraph(newGraphId, this);
        }

        public FrozenHandler<FlowGraph> FreezeAndReleaseGraph()
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires(this.Graph.Builder == this);
            Contract.Requires(this.Graph.CanFreeze);
            Contract.Ensures(this.Graph == null);
            Contract.Ensures(Contract.Result<FrozenHandler<FlowGraph>>().Value != null);
            Contract.Ensures(Contract.Result<FrozenHandler<FlowGraph>>().Value.Builder == null);

            return this.Graph.Freeze();
        }

        public EnterFlowNode AddEnterNode(IEnumerable<FlowVariable> parameters = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new EnterFlowNode(this.Graph, nodeId, parameters ?? Enumerable.Empty<FlowVariable>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        // TODO: Add even more overloads (for example, consider directly using immutable array) and optimize their calls
        public InnerFlowNode AddInnerNode(FlowVariable assignmentVariable, Expression assignmentValue)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            return this.AddInnerNode(new[] { new Assignment(assignmentVariable, assignmentValue) });
        }

        public InnerFlowNode AddInnerNode(IEnumerable<Assignment> assignments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new InnerFlowNode(this.Graph, nodeId, assignments ?? Enumerable.Empty<Assignment>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public CallFlowNode AddCallNode(
            ILocation location,
            IEnumerable<Expression> arguments = null,
            IEnumerable<FlowVariable> returnAssignments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(location != null, nameof(location));

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new CallFlowNode(
                this.Graph,
                nodeId,
                location,
                arguments ?? Enumerable.Empty<Expression>(),
                returnAssignments ?? Enumerable.Empty<FlowVariable>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public ReturnFlowNode AddReturnNode(IEnumerable<Expression> returnValues = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new ReturnFlowNode(this.Graph, nodeId, returnValues ?? Enumerable.Empty<Expression>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public ThrowExceptionFlowNode AddThrowExceptionNode(
            ILocation constructorLocation,
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

            return node;
        }

        public FlowEdge AddEdge(FlowNode from, FlowNode to)
        {
            return this.AddEdge(from, to, true);
        }

        public FlowEdge AddEdge(FlowNode from, FlowNode to, BoolHandle condition)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(from != null, nameof(from));
            Contract.Requires<ArgumentNullException>(to != null, nameof(to));
            Contract.Requires<ArgumentException>(from.Graph == this.Graph, nameof(from));
            Contract.Requires<ArgumentException>(to.Graph == this.Graph, nameof(to));

            var edgeId = this.edgeIdProvider.GenerateNewId();
            var edge = new FlowEdge(edgeId, from, to, condition);
            this.Graph.MutableEdges.Add(edge);

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

            return variable;
        }
    }
}
