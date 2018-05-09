using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Operations;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    using IngoingEdgesOverlay = OrdinalOverlay<BuildNodeId, BuildNode, List<FlowGraphTranslator.EdgeInfo>>;

    internal class FlowGraphTranslator
    {
        private readonly FlowGraphId flowGraphId;

        private FlowGraphBuilder builder;
        private IngoingEdgesOverlay ingoingEdges;
        private ExpressionTranslator expressionTranslator;
        private OrdinalOverlay<BuildVariableId, BuildVariable, FlowVariable> buildToFlowVariablesMap;
        private OrdinalOverlay<BuildNodeId, BuildNode, FlowNodeMappedInfo> buildToFlowNodesMap;

        public FlowGraphTranslator(BuildGraph buildGraph, DisplayGraph displayGraph, FlowGraphId flowGraphId)
        {
            this.BuildGraph = buildGraph;
            this.DisplayGraph = displayGraph;
            this.flowGraphId = flowGraphId;
        }

        public BuildGraph BuildGraph { get; }

        public FlowGraph FlowGraph { get; private set; }

        public DisplayGraph DisplayGraph { get; }

        // TODO: Consider splitting into multiple methods to increase readability
        public GeneratedGraphs Translate()
        {
            this.builder = new FlowGraphBuilder(this.flowGraphId);
            this.ingoingEdges = this.ComputeIngoingEdges(this.BuildGraph);

            this.expressionTranslator = new ExpressionTranslator(this);
            this.buildToFlowVariablesMap = new OrdinalOverlay<BuildVariableId, BuildVariable, FlowVariable>();
            this.buildToFlowNodesMap = new OrdinalOverlay<BuildNodeId, BuildNode, FlowNodeMappedInfo>();
            var nodeQueue = new Queue<BuildNode>();
            var edgeQueue = new Queue<EdgeInfo>();
            var visitedNodes = new OrdinalOverlay<BuildNodeId, BuildNode, bool>();

            var buildParameters = this.BuildGraph.Variables
                .Where(v => v.Origin == VariableOrigin.Parameter || v.Origin == VariableOrigin.This);
            var flowParameters = new List<FlowVariable>();
            foreach (var parameter in buildParameters)
            {
                flowParameters.Add(this.TranslateVariable(parameter));
            }

            var flowEnterNode = this.builder.AddEnterNode(flowParameters);
            this.buildToFlowNodesMap[this.BuildGraph.EnterNode] = flowEnterNode;
            visitedNodes[this.BuildGraph.EnterNode] = true;

            Contract.Assert(this.BuildGraph.EnterNode.OutgoingEdges.Count == 1);
            var firstNonEnterNode = this.BuildGraph.EnterNode.OutgoingEdges.Single().To;
            nodeQueue.Enqueue(firstNonEnterNode);
            visitedNodes[firstNonEnterNode] = true;

            while (nodeQueue.Count > 0)
            {
                var buildNode = nodeQueue.Dequeue();
                Contract.Assert(visitedNodes[buildNode]);
                Contract.Assert(this.buildToFlowNodesMap[buildNode].FlowNode == null);

                BuildNode firstBuildNode, lastBuildNode;

                var flowNode = this.TryTranslateBorderNode(buildNode);
                if (flowNode != null)
                {
                    firstBuildNode = buildNode;
                    lastBuildNode = buildNode;

                    this.buildToFlowNodesMap[buildNode] = flowNode;
                }
                else
                {
                    this.ProcessInnerNodesSequence(buildNode, out firstBuildNode, out lastBuildNode, out var operations);
                    flowNode = this.builder.AddInnerNode(operations);
                    this.MapAssignmentsToFlowNode(firstBuildNode, lastBuildNode, flowNode);
                }

                // TODO: Try to get rid of the empty nodes (empty blocks etc.)
                Contract.Assert(flowNode != null);

                foreach (var edge in lastBuildNode.OutgoingEdges)
                {
                    if (!visitedNodes[edge.To])
                    {
                        nodeQueue.Enqueue(edge.To);
                        visitedNodes[edge.To] = true;
                    }
                }

                foreach (var edgeInfo in this.ingoingEdges[firstBuildNode])
                {
                    var flowFromNode = this.buildToFlowNodesMap[edgeInfo.From].FlowNode;
                    if (flowFromNode == null)
                    {
                        edgeQueue.Enqueue(edgeInfo);
                    }
                    else
                    {
                        this.TranslateEdge(edgeInfo.Edge, edgeInfo.From, flowFromNode, flowNode);
                    }
                }
            }

            while (edgeQueue.Count > 0)
            {
                var edgeInfo = edgeQueue.Dequeue();
                FlowNode flowFrom = this.buildToFlowNodesMap[edgeInfo.From];
                FlowNode flowTo = this.buildToFlowNodesMap[edgeInfo.Edge.To];

                Contract.Assert(flowFrom != null);
                Contract.Assert(flowTo != null);
                this.TranslateEdge(edgeInfo.Edge, edgeInfo.From, flowFrom, flowTo);
            }

            this.FlowGraph = this.builder.FreezeAndReleaseGraph();

            this.FinishDisplayGraph();

            return new GeneratedGraphs(this.FlowGraph, this.DisplayGraph);
        }

        // TODO: Consider changing the branching into assertions where possible
        private void FinishDisplayGraph()
        {
            foreach (var buildNode in this.BuildGraph.Nodes)
            {
                var flowNodeInfo = this.buildToFlowNodesMap[buildNode];
                if (flowNodeInfo.FlowNode != null)
                {
                    var displayNode = buildNode.DisplayNode;
                    if (displayNode == null)
                    {
                        continue;
                    }

                    if (buildNode.Operation?.Kind == SpecialOperationKind.Enter)
                    {
                        int valAssignOffset = 0;
                        int refAssignOffset = 0;

                        ParameterListSyntax parameterList = ((ParameterListSyntax)buildNode.Syntax);
                        var thisParam = this.BuildGraph.Variables.FirstOrDefault(v => v.Origin == VariableOrigin.This);
                        if (thisParam != null)
                        {
                            // Instance methods have the instance reference as the first parameter
                            var record = new DisplayNodeRecord(
                                flowNodeInfo.FlowNode,
                                new TextSpan(parameterList.OpenParenToken.Span.End, 0),
                                refAssignOffset,
                                this.BuildGraph.LocalInstanceModel.Type,
                                thisParam.DisplayName);
                            displayNode.AddRecord(record);

                            refAssignOffset++;
                        }

                        foreach (var parameterSyntax in parameterList.Parameters)
                        {
                            string parameterName = parameterSyntax.Identifier.Text;
                            var parameterModel =
                                (from kvp in this.BuildGraph.DefinedVariableModels
                                 where kvp.Key.Kind == SymbolKind.Parameter && kvp.Key.Name == parameterName
                                 select kvp.Value).FirstOrDefault();

                            if (parameterModel != null)
                            {
                                bool isRefModel = parameterModel.Factory.ValueKind == ValueModelKind.Reference;

                                var record = new DisplayNodeRecord(
                                    flowNodeInfo.FlowNode,
                                    parameterSyntax.Span,
                                    isRefModel ? valAssignOffset : refAssignOffset,
                                    parameterModel.Type,
                                    parameterSyntax.Identifier.Text);
                                displayNode.AddRecord(record);

                                if (isRefModel)
                                {
                                    refAssignOffset += parameterModel.AssignmentLeft.Count;
                                }
                                else
                                {
                                    valAssignOffset += parameterModel.AssignmentLeft.Count;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Offset and type
                        int assignmentOffset = -1;
                        int operationOffset = -1;
                        ITypeSymbol type = null;
                        string variableName = null;
                        if (buildNode.VariableModel != null)
                        {
                            assignmentOffset = flowNodeInfo.AssignmentOffset;
                            operationOffset = flowNodeInfo.OperationOffset;
                            type = buildNode.VariableModel.Type;
                            if (buildNode.VariableModel.AssignmentLeft.FirstOrDefault() is BuildVariable buildVar
                                && buildVar.Origin != VariableOrigin.Temporary)
                            {
                                variableName = buildVar.DisplayName;
                            }
                        }
                        else if (buildNode.Operation?.Kind == SpecialOperationKind.FieldWrite)
                        {
                            assignmentOffset = flowNodeInfo.AssignmentOffset;
                            operationOffset = flowNodeInfo.OperationOffset;
                            type = ((HeapOperation)buildNode.Operation).Reference.Type;
                        }

                        var record = new DisplayNodeRecord(flowNodeInfo.FlowNode, buildNode.Label.Span, assignmentOffset, type, variableName, operationOffset);
                        displayNode.AddRecord(record);
                    }

                    foreach (var buildEdge in buildNode.OutgoingEdges)
                    {
                        if (buildEdge.To.DisplayNode == null)
                        {
                            continue;
                        }

                        // Prevent self loops and multiple edges
                        var targetDisplayNode = buildEdge.To.DisplayNode;
                        if (displayNode != targetDisplayNode
                            && !displayNode.OutgoingEdges.Any(displayEdge => displayEdge.To == targetDisplayNode))
                        {
                            // TODO: Edge label
                            this.DisplayGraph.AddEdge(displayNode, targetDisplayNode);
                        }

                        //var targetFlowNodeInfo = this.buildToFlowNodesMap[buildEdge.To];
                        //if (targetFlowNodeInfo.FlowNode != null)
                        //{
                        //    // We cannot step into the middle of the CFG node
                        //    Contract.Assert(targetFlowNodeInfo.AssignmentOffset == 0);


                        //}
                    }
                }
            }

            this.DisplayGraph.Freeze();
        }

        private IngoingEdgesOverlay ComputeIngoingEdges(BuildGraph graph)
        {
            var ingoing = new IngoingEdgesOverlay(() => new List<EdgeInfo>());

            var visited = new OrdinalOverlay<BuildNodeId, BuildNode, bool>();

            var stack = new Stack<BuildNode>();
            stack.Push(graph.EnterNode);
            visited[graph.EnterNode] = true;

            while (stack.Count > 0)
            {
                var node = stack.Pop();

                foreach (var edge in node.OutgoingEdges)
                {
                    var edgeInfo = new EdgeInfo(edge, node);
                    ingoing[edge.To].Add(edgeInfo);

                    if (!visited[edge.To])
                    {
                        stack.Push(edge.To);
                        visited[edge.To] = true;
                    }
                }
            }

            return ingoing;
        }

        private FlowNode TryTranslateBorderNode(BuildNode buildNode)
        {
            var borderOp = buildNode.Operation as BorderOperation;
            if (borderOp == null || borderOp.Kind == SpecialOperationKind.Assertion)
            {
                return null;
            }

            if (borderOp.Kind == SpecialOperationKind.MethodCall || borderOp.Kind == SpecialOperationKind.ExceptionThrow)
            {
                MethodLocation location;
                IEnumerable<Expression> flowArguments;

                if (borderOp.Arguments.Any(arg => arg == null))
                {
                    // We cannot model method calls without properly modelling all their arguments first
                    location = new MethodLocation(borderOp.Method, isExplorationDisabled: true);
                    flowArguments = Enumerable.Empty<Expression>();
                }
                else
                {
                    // TODO: Enable a configurable and extensible approach instead of this hack
                    // Disable exploring the methods from the tool evaluation
                    bool isExplorationDisabled =
                        borderOp.Method.ContainingType.ToString() == "EvaluationTests.Annotations.Evaluation";

                    location = new MethodLocation(borderOp.Method, isExplorationDisabled);
                    var buildArguments = borderOp.Arguments.SelectMany(typeModel => typeModel.AssignmentRight);
                    flowArguments = buildArguments.Select(expression => this.TranslateExpression(expression));
                }

                if (borderOp.Kind == SpecialOperationKind.MethodCall)
                {
                    var returnAssignments = buildNode.VariableModel?.AssignmentLeft
                        .Select(buildVar => this.TranslateVariable(buildVar));

                    // We don't allow calling base constructors, so the only way to call it is with the "new" operator
                    // TODO: Propagate the information about constructor call other way when the above is supported
                    var callKind = (borderOp.Method.MethodKind == MethodKind.Constructor)
                        ? CallKind.ObjectCreation
                        : borderOp.Method.IsStatic
                            ? CallKind.Static
                            : CallKind.Instance;

                    bool isObjectCreation = (borderOp.Method.MethodKind == MethodKind.Constructor);

                    return this.builder.AddCallNode(location, flowArguments, returnAssignments, callKind);
                }
                else
                {
                    Contract.Assert(borderOp.Kind == SpecialOperationKind.ExceptionThrow);

                    return this.builder.AddThrowExceptionNode(location, flowArguments);
                }
            }
            else
            {
                Contract.Assert(borderOp.Kind == SpecialOperationKind.Return);

                var returnValues = buildNode.ValueModel?.AssignmentRight
                    .Select(expression => this.TranslateExpression(expression))
                    .ToImmutableArray();

                if ((returnValues == null || returnValues.Value.Length == 0)
                    && this.BuildGraph.MethodSyntax.Kind() == SyntaxKind.ConstructorDeclaration)
                {
                    // A constructor returns "this" variable by convention
                    var buildThis = this.BuildGraph.Variables.First(v => v.Origin == VariableOrigin.This);
                    returnValues = ImmutableArray.Create((Expression)this.TranslateVariable(buildThis));
                }

                return this.builder.AddReturnNode(returnValues);
            }
        }

        // TODO: Substitute the temporary variables only used once in the sequence to reduce the number of assignments
        private void ProcessInnerNodesSequence(
            BuildNode buildNode,
            out BuildNode firstBuildNode,
            out BuildNode lastBuildNode,
            out List<Operation> operations)
        {
            operations = new List<Operation>();
            var curNode = buildNode;
            firstBuildNode = curNode;

            while (true)
            {
                if (curNode.Operation == null)
                {
                    var nodeAssignments = this.TranslateAssignments(curNode.VariableModel, curNode.ValueModel);
                    operations.AddRange(nodeAssignments);
                }
                else if (curNode.Operation is HeapOperation heapOp)
                {
                    var heapOps = this.TranslateHeapOperations(curNode.VariableModel, curNode.ValueModel, heapOp);
                    operations.AddRange(heapOps);
                }
                else
                {
                    Contract.Assert(curNode.Operation.Kind == SpecialOperationKind.Assertion);
                }

                if (curNode.OutgoingEdges.Count != 1)
                {
                    break;
                }

                var nextNode = curNode.OutgoingEdges.Single().To;

                if ((nextNode.Operation is BorderOperation && nextNode.Operation.Kind != SpecialOperationKind.Assertion)
                    || this.ingoingEdges[nextNode].Count > 1)
                {
                    break;
                }

                Contract.Assert(this.ingoingEdges[nextNode].Single().From == curNode);
                curNode = nextNode;
            }

            lastBuildNode = curNode;
        }

        private void MapAssignmentsToFlowNode(
            BuildNode firstBuildNode,
            BuildNode lastBuildNode,
            FlowNode flowNode)
        {
            int valAssignOffset = 0;
            int refAssignOffset = 0;
            int opOffset = 0;
            foreach (var processedNode in this.GetBuildNodesSequenceRange(firstBuildNode, lastBuildNode))
            {
                // Note that result of the field write will be the updated reference and a new heap version
                bool isRefModel =
                    processedNode.Operation?.Kind == SpecialOperationKind.FieldWrite
                    || processedNode.VariableModel?.Factory.ValueKind == ValueModelKind.Reference;

                this.buildToFlowNodesMap[processedNode] = new FlowNodeMappedInfo(
                    flowNode,
                    isRefModel ? refAssignOffset : valAssignOffset,
                    opOffset);

                if (isRefModel)
                {
                    if (processedNode.VariableModel?.AssignmentLeft.Count is int count)
                    {
                        refAssignOffset += count;
                    }
                    else
                    {
                        Contract.Assert(processedNode.Operation.Kind == SpecialOperationKind.FieldWrite);

                        var heapOp = (HeapOperation)processedNode.Operation;
                        refAssignOffset += heapOp.Reference.AssignmentLeft.Count;
                    }
                }
                else
                {
                    valAssignOffset += processedNode.VariableModel?.AssignmentLeft.Count ?? 0;
                }

                opOffset++;
            }
        }

        private IEnumerable<Assignment> TranslateAssignments(
            ITypeModel variableModel,
            ITypeModel valueModel)
        {
            if (variableModel == null || valueModel == null || variableModel.AssignmentLeft.Count == 0)
            {
                Contract.Assert(valueModel == null || valueModel.AssignmentRight.Count == 0);

                yield break;
            }

            var variables = variableModel.AssignmentLeft;
            var values = valueModel.AssignmentRight;
            Contract.Assert(variables.Count == values.Count);

            for (int i = 0; i < variables.Count; i++)
            {
                Contract.Assert(variables[i].Sort == values[i].Sort);

                var translatedVariable = this.TranslateVariable(variables[i]);
                var translatedValue = this.TranslateExpression(values[i]);
                Contract.Assert(translatedVariable.Sort == variables[i].Sort);
                Contract.Assert(translatedVariable.Sort == translatedValue.Sort);

                yield return new Assignment(translatedVariable, translatedValue);
            }
        }

        private IEnumerable<Operation> TranslateHeapOperations(
            ITypeModel variableModel,
            ITypeModel valueModel,
            HeapOperation heapOp)
        {
            if (heapOp.Kind == SpecialOperationKind.FieldRead)
            {
                if (variableModel == null)
                {
                    yield break;
                }

                var translatedReference = this.TranslateVariable(heapOp.Reference.AssignmentLeft.Single());
                var variables = variableModel.AssignmentLeft;
                Contract.Assert(variables.Count == heapOp.Fields.Length);

                for (int i = 0; i < variables.Count; i++)
                {
                    Contract.Assert(variables[i].Sort == heapOp.Fields[i].Sort);

                    yield return new FieldRead(
                        this.TranslateVariable(variables[i]),
                        translatedReference,
                        heapOp.Fields[i]);
                }
            }
            else
            {
                Contract.Assert(heapOp.Kind == SpecialOperationKind.FieldWrite);

                if (valueModel == null)
                {
                    yield break;
                }

                var translatedReference = this.TranslateVariable(heapOp.Reference.AssignmentLeft.Single());
                var values = valueModel.AssignmentRight;
                Contract.Assert(values.Count == heapOp.Fields.Length);

                for (int i = 0; i < values.Count; i++)
                {
                    Contract.Assert(values[i].Sort == heapOp.Fields[i].Sort);

                    yield return new FieldWrite(
                        translatedReference,
                        heapOp.Fields[i],
                        this.TranslateExpression(values[i]));
                }
            }
        }

        private InnerFlowEdge TranslateEdge(
            BuildEdge buildEdge,
            BuildNode buildFrom,
            FlowNode flowFrom,
            FlowNode flowTo)
        {
            BoolHandle condition;
            if (buildEdge.ValueCondition?.Sort == Sort.Bool)
            {
                Contract.Assert(buildFrom.VariableModel != null);
                Contract.Assert(buildFrom.VariableModel is BooleanModel);

                var variable = (BuildVariable)buildFrom.VariableModel.AssignmentLeft.Single();

                if (variable.Origin == VariableOrigin.Temporary
                    && buildFrom.ValueModel?.AssignmentRight?.Single() is Expression valExpr
                    && References.IsReferenceComparison(
                        this.TranslateExpression(valExpr),
                        out bool areEqual,
                        out var left,
                        out var right))
                {
                    // Reference comparison stored to a temporary variable
                    // (as such, we suppose it is not used anywhere else)
                    if (buildEdge.ValueCondition == ExpressionFactory.False)
                    {
                        condition = this.builder.AddReferenceComparison(!areEqual, left, right);
                    }
                    else
                    {
                        Contract.Assert(buildEdge.ValueCondition == ExpressionFactory.True);
                        condition = this.builder.AddReferenceComparison(areEqual, left, right);
                    }
                }
                else
                {
                    // General boolean variable
                    condition = (BoolHandle)this.TranslateVariable(variable);
                    if (buildEdge.ValueCondition == ExpressionFactory.False)
                    {
                        condition = !condition;
                    }
                    else
                    {
                        Contract.Assert(buildEdge.ValueCondition == ExpressionFactory.True);
                    }
                }
            }
            else if (buildEdge.ValueCondition != null)
            {
                Contract.Assert(buildFrom.VariableModel != null);

                var variables = buildFrom.VariableModel.AssignmentLeft
                    .Select(buildVar => this.TranslateVariable(buildVar))
                    .ToArray();

                // TODO: Update when BuildEdge.ValueCondition is changed to IValueModel
                var values = buildEdge.ValueCondition.ToSingular()
                    .Select(expression => this.TranslateExpression(expression))
                    .ToArray();

                // TODO: Consider overriding the comparison by the type models (string might use that)
                Contract.Assert(variables.Length == values.Length);
                if (variables.Length == 1)
                {
                    condition = (BoolHandle)ExpressionFactory.Equal(variables.Single(), values.Single());
                }
                else
                {
                    Contract.Assert(variables.Length > 1);

                    var conditions = new List<Expression>();
                    for (int i = 0; i < variables.Length; i++)
                    {
                        conditions.Add(ExpressionFactory.Equal(variables[i], values[i]));
                    }

                    condition = (BoolHandle)ExpressionFactory.And(conditions.ToArray());
                }
            }
            else
            {
                condition = true;
            }

            return this.builder.AddEdge(flowFrom, flowTo, condition);
        }

        private FlowVariable TranslateVariable(Variable variable)
        {
            Contract.Requires(variable is BuildVariable || variable == References.Null);

            if (variable == References.Null)
            {
                return References.Null;
            }
            else
            {
                var buildVariable = (BuildVariable)variable;
                var flowVariable = this.buildToFlowVariablesMap[buildVariable];

                if (flowVariable == null)
                {
                    flowVariable = this.builder.AddLocalVariable(
                        buildVariable.Sort,
                        (buildVariable.Origin != VariableOrigin.Temporary) ? buildVariable.ToString() : null);
                    this.buildToFlowVariablesMap[buildVariable] = flowVariable;
                }

                return flowVariable;
            }
        }

        private Expression TranslateExpression(Expression expression)
        {
            return this.expressionTranslator.Visit(expression);
        }

        private IEnumerable<BuildNode> GetBuildNodesSequenceRange(BuildNode first, BuildNode last)
        {
            for (var current = first; ; current = current.OutgoingEdges.Single().To)
            {
                yield return current;

                if (current == last)
                {
                    break;
                }
            }
        }

        internal class EdgeInfo
        {
            public EdgeInfo(BuildEdge edge, BuildNode from)
            {
                Contract.Requires(edge != null);
                Contract.Requires(from != null);
                Contract.Requires(from.OutgoingEdges.Contains(edge));

                this.Edge = edge;
                this.From = from;
            }

            public BuildEdge Edge { get; private set; }

            public BuildNode From { get; private set; }
        }

        private class ExpressionTranslator : ExpressionRewriter
        {
            private FlowGraphTranslator owner;

            public ExpressionTranslator(FlowGraphTranslator owner)
            {
                Contract.Requires(owner != null);

                this.owner = owner;
            }

            public override Expression VisitVariable(Variable variable)
            {
                Contract.Assert(variable is BuildVariable || variable == References.Null);

                return this.owner.TranslateVariable(variable);
            }

            public override Expression VisitFunction(Function function)
            {
                var flowFunction = base.VisitFunction(function);

                if ((flowFunction.Kind == ExpressionKind.Equal || flowFunction.Kind == ExpressionKind.Distinct)
                    && flowFunction.GetChild(0).Sort == References.Sort)
                {
                    Expression left = flowFunction.GetChild(0);
                    Expression right = flowFunction.GetChild(1);

                    // Instances of the reference sort cannot be a result of a function, so they must be variables
                    Contract.Assert(right.Sort == References.Sort);
                    Contract.Assert(flowFunction.Children.All(ch => ch.Kind == ExpressionKind.Variable));

                    return this.owner.builder.AddReferenceComparison(
                        flowFunction.Kind == ExpressionKind.Equal,
                        (FlowVariable)left,
                        (FlowVariable)right);
                }
                else if (flowFunction.Kind == ExpressionKind.Not
                    && References.IsReferenceComparison(flowFunction.GetChild(0), out bool areEqual, out var left, out var right))
                {
                    return this.owner.builder.AddReferenceComparison(!areEqual, left, right);
                }
                else
                {
                    return flowFunction;
                }
            }
        }
    }
}
