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
        private FlowGraphNodeId.Provider nodeIdProvider;
        private FlowGraphEdgeId.Provider edgeIdProvider;
        private FlowGraphLocalVariableId.Provider variableIdProvider;

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

            this.nodeIdProvider = new FlowGraphNodeId.Provider();
            this.edgeIdProvider = new FlowGraphEdgeId.Provider();
            this.variableIdProvider = new FlowGraphLocalVariableId.Provider();

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

        public FlowGraphEnterNode AddEnterNode(IEnumerable<FlowGraphVariable> parameters = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new FlowGraphEnterNode(this.Graph, nodeId, parameters ?? Enumerable.Empty<FlowGraphVariable>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        // TODO: Add even more overloads (for example, consider directly using immutable array) and optimize their calls
        public FlowGraphInnerNode AddInnerNode(FlowGraphVariable assignmentVariable, Expression assignmentValue)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            return this.AddInnerNode(new[] { new Assignment(assignmentVariable, assignmentValue) });
        }

        public FlowGraphInnerNode AddInnerNode(IEnumerable<Assignment> assignments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new FlowGraphInnerNode(this.Graph, nodeId, assignments ?? Enumerable.Empty<Assignment>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public FlowGraphCallNode AddCallNode(
            ILocation location,
            IEnumerable<Expression> arguments = null,
            IEnumerable<FlowGraphVariable> returnAssignments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(location != null, nameof(location));

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new FlowGraphCallNode(
                this.Graph,
                nodeId,
                location,
                arguments ?? Enumerable.Empty<Expression>(),
                returnAssignments ?? Enumerable.Empty<FlowGraphVariable>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public FlowGraphReturnNode AddReturnNode(IEnumerable<Expression> returnValues = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new FlowGraphReturnNode(this.Graph, nodeId, returnValues ?? Enumerable.Empty<Expression>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public FlowGraphThrowExceptionNode AddThrowExceptionNode(
            ILocation constructorLocation,
            IEnumerable<Expression> arguments = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(constructorLocation != null, nameof(constructorLocation));

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new FlowGraphThrowExceptionNode(
                this.Graph,
                nodeId,
                constructorLocation,
                arguments ?? Enumerable.Empty<Expression>());
            this.Graph.MutableNodes.Add(node);

            return node;
        }

        public FlowGraphEdge AddEdge(FlowGraphNode from, FlowGraphNode to)
        {
            return this.AddEdge(from, to, true);
        }

        public FlowGraphEdge AddEdge(FlowGraphNode from, FlowGraphNode to, BoolHandle condition)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(from != null, nameof(from));
            Contract.Requires<ArgumentNullException>(to != null, nameof(to));
            Contract.Requires<ArgumentException>(from.Graph == this.Graph, nameof(from));
            Contract.Requires<ArgumentException>(to.Graph == this.Graph, nameof(to));

            var edgeId = this.edgeIdProvider.GenerateNewId();
            var edge = new FlowGraphEdge(edgeId, from, to, condition);
            this.Graph.MutableEdges.Add(edge);

            return edge;
        }

        public FlowGraphLocalVariable AddLocalVariable(Sort sort, string displayName = null)
        {
            Contract.Requires<InvalidOperationException>(this.Graph != null);
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));

            return this.AddLocalVariableImpl(sort, displayName, null);
        }

        public FlowGraphLocalVariable AddLocalVariable(
            Sort sort,
            Func<FlowGraphLocalVariableId, string> displayNameCallback)
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

        private FlowGraphLocalVariable AddLocalVariableImpl(
            Sort sort,
            string displayName = null,
            Func<FlowGraphLocalVariableId, string> displayNameCallback = null)
        {
            var variableId = this.variableIdProvider.GenerateNewId();

            if (displayName == null && displayNameCallback != null)
            {
                displayName = displayNameCallback.Invoke(variableId);
            }

            var variable = new FlowGraphLocalVariable(this.Graph, variableId, sort, displayName);
            this.Graph.MutableLocalVariables.Add(variable);

            return variable;
        }
    }
}
