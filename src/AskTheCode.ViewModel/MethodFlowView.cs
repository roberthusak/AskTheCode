using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.PathExploration;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    public class MethodFlowView : NotifyPropertyChangedBase
    {
        private readonly MethodLocation location;
        private readonly int startIndex;
        private readonly int endIndex;

        private List<StatementFlowView> statementFlows;
        private StatementFlowView selectedStatementFlow;
        private List<MethodFlowView> callees;

        private bool isExpanded;
        private bool isSelected;

        internal MethodFlowView(
            PathView pathView,
            MethodFlowView caller,
            MethodLocation location,
            int startIndex,
            int endIndex,
            int startHeapVersion,
            int endHeapVersion)
        {
            this.PathView = pathView;
            this.Caller = caller;
            this.location = location;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
            this.StartHeapVersion = startHeapVersion;
            this.EndHeapVersion = endHeapVersion;
        }

        public string Name
        {
            get { return this.location.ToString(); }
        }

        public List<StatementFlowView> StatementFlows
        {
            get
            {
                if (this.statementFlows == null)
                {
                    this.Initialize();
                }

                return this.statementFlows;
            }
        }

        public StatementFlowView SelectedStatementFlow
        {
            get { return this.selectedStatementFlow; }
            set { this.SetProperty(ref this.selectedStatementFlow, value); }
        }

        public MethodFlowView Caller { get; }

        public List<MethodFlowView> Callees
        {
            get
            {
                if (this.callees == null)
                {
                    this.Initialize();
                }

                return this.callees;
            }
        }

        public int StartHeapVersion { get; }

        public int EndHeapVersion { get; }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set { this.SetProperty(ref this.isExpanded, value); }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set { this.SetProperty(ref this.isSelected, value); }
        }

        internal PathView PathView { get; }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.SelectedStatementFlow))
            {
                this.UpdateSelectedStatement();
            }
            else if (propertyName == nameof(this.IsSelected))
            {
                if (this.IsSelected)
                {
                    this.PathView.SelectedMethodFlow = this;
                }
                else if (this.PathView.SelectedMethodFlow == this)
                {
                    this.PathView.SelectedMethodFlow = null;
                }
            }
        }

        private static int AdvanceHeapVersion(int heapVersion, ExecutionModel executionModel, int nodeIndex)
        {
            int nodeMaxHeapVersion = executionModel.HeapLocations[nodeIndex]
                .Select(l => l.HeapVersion)
                .DefaultIfEmpty()
                .Max();
            return Math.Max(heapVersion, nodeMaxHeapVersion);
        }

        private void Initialize()
        {
            this.statementFlows = new List<StatementFlowView>();
            this.callees = new List<MethodFlowView>();

            var toolView = this.PathView.ToolView;
            var syntaxTree = this.location.Method.Locations[0].SourceTree;
            var text = syntaxTree.GetText();
            ////var document = toolView.CurrentSolution.GetDocument(syntaxTree);  // TODO: Consider storing this

            var executionModel = this.PathView.ExecutionModel;
            var flowGraphId = executionModel.PathNodes[this.startIndex].Graph.Id;
            var displayGraph = toolView.GraphProvider.GetDisplayGraph(flowGraphId);

            int heapVersion = this.StartHeapVersion;
            bool endLoop = false;
            for (int i = this.startIndex; i <= this.endIndex && !endLoop; i++)
            {
                heapVersion = AdvanceHeapVersion(heapVersion, executionModel, i);

                var flowNode = executionModel.PathNodes[i];
                MethodFlowView callee = null;

                if (flowNode is CallFlowNode && i != this.endIndex)
                {
                    // Traverse nested method calls
                    callee = this.ProcessCallee(executionModel, i, heapVersion);

                    if (callee.endIndex < this.endIndex)
                    {
                        // Nested call whose result is displayed in the subsequent flow of this method
                        i = callee.endIndex + 1;
                        heapVersion = callee.EndHeapVersion;
                        Contract.Assert(flowNode == executionModel.PathNodes[i]);
                    }
                    else
                    {
                        // Nested call somewhere in which the execution ends
                        endLoop = true;
                    }
                }

                // Produce statements from display nodes
                this.ProcessStatements(text, displayGraph, executionModel, i, callee);
            }

            Contract.Assert(heapVersion <= this.EndHeapVersion);
        }

        private MethodFlowView ProcessCallee(ExecutionModel executionModel, int callNodeIndex, int heapVersion)
        {
            int nestedLevel = 0;
            int startIndex = callNodeIndex + 1;
            int endIndex;
            int startHeapVersion = heapVersion;
            for (int j = startIndex; j <= this.endIndex; j++)
            {
                heapVersion = AdvanceHeapVersion(heapVersion, executionModel, j);

                var calleeflowNode = executionModel.PathNodes[j];
                if (calleeflowNode is EnterFlowNode)
                {
                    // Method call; notice that this must happen on the first iteration (when j == calleeStart)
                    nestedLevel++;
                }
                else if (calleeflowNode is ReturnFlowNode)
                {
                    // Returning from a method
                    nestedLevel--;

                    if (nestedLevel == 0)
                    {
                        // Produce method called from the original call node
                        endIndex = j;
                        var callee = this.AddCallee(startIndex, endIndex, startHeapVersion, heapVersion);

                        // Update i to correspond to the second part of the call node (stored in flowNode)
                        callNodeIndex = endIndex + 1;

                        return callee;
                    }
                }
            }

            Contract.Assert(nestedLevel != 0);

            // The execution model ends in the called function, so just display the call in this method
            endIndex = this.endIndex;
            return this.AddCallee(startIndex, endIndex, startHeapVersion, heapVersion);
        }

        private MethodFlowView AddCallee(int startIndex, int endIndex, int startHeapVersion, int endHeapVersion)
        {
            var toolView = this.PathView.ToolView;
            var graph = this.PathView.ExecutionModel.PathNodes[startIndex].Graph;
            var calleeLocation = toolView.GraphProvider.GetLocation(graph.Id);
            var callee = new MethodFlowView(
                this.PathView,
                this,
                calleeLocation,
                startIndex,
                endIndex,
                startHeapVersion,
                endHeapVersion);
            this.callees.Add(callee);

            return callee;
        }

        private void ProcessStatements(
            SourceText text,
            DisplayGraph displayGraph,
            ExecutionModel executionModel,
            int nodeIndex,
            MethodFlowView calledMethod = null)
        {
            var modelManager = this.PathView.ToolView.GraphProvider.ModelManager;
            var flowNode = executionModel.PathNodes[nodeIndex];
            bool isLastInnerNode =
                (nodeIndex == executionModel.PathNodes.Length - 1)
                && flowNode is InnerFlowNode;
            var nodeInterpretations = executionModel.NodeInterpretations[nodeIndex];
            var heapLocations = executionModel.HeapLocations[nodeIndex];

            // TODO: Consider optimizing
            // TODO: Group the records by their display nodes
            var displayRecords = new List<DisplayNodeRecord>();
            foreach (var displayNode in displayGraph.Nodes)
            {
                displayRecords.AddRange(displayNode.Records.Where(record => record.FlowNode == flowNode));

                // Temporary: Replace by this if only the last result of a DisplayNode is important
                // (Behaves weirdly in case of methods and their arguments, as they are distributed between
                //  two FlowNodes)
                ////var displayRecord = displayNode.Records.LastOrDefault(record => record.FlowNode == flowNode);
                ////if (displayRecord != null)
                ////{
                ////    displayRecords.Add(displayRecord);
                ////}
            }

            foreach (var displayRecord in displayRecords)
            {
                Contract.Assert(displayRecord != null);
                string statement = text.ToString(displayRecord.Span);
                string value = null;
                string type = null;
                HeapModelLocation? heapLocation = null;
                if (displayRecord.Type != null)
                {
                    var modelFactory = modelManager.TryGetFactory(displayRecord.Type);
                    if (modelFactory.ValueKind == ValueModelKind.Interpretation)
                    {
                        // Hide the remaining portion of the inner CFG node where the exploration started from
                        if (isLastInnerNode && displayRecord.FirstVariableIndex >= nodeInterpretations.Length)
                        {
                            continue;
                        }

                        var sortRequirements = modelFactory.GetExpressionSortRequirements(displayRecord.Type);
                        var interpretations = nodeInterpretations
                            .Skip(displayRecord.FirstVariableIndex)
                            .Take(sortRequirements.Count)
                            .ToArray();

                        if (interpretations.Length != 0
                            && interpretations.All(interpretation => interpretation != null))
                        {
                            var valueModel = modelFactory.GetValueModel(displayRecord.Type, interpretations);

                            value = valueModel.ValueText;
                            type = displayRecord.Type.Name;
                        }
                    }
                    else
                    {
                        Contract.Assert(modelFactory.ValueKind == ValueModelKind.Reference);

                        bool locationExists = displayRecord.FirstVariableIndex < heapLocations.Length;

                        // Hide the remaining portion of the inner CFG node where the exploration started from
                        if (isLastInnerNode && !locationExists)
                        {
                            continue;
                        }

                        if (locationExists)
                        {
                            heapLocation = heapLocations[displayRecord.FirstVariableIndex];
                            var valueModel = modelFactory.GetValueModel(
                                displayRecord.Type,
                                heapLocation.Value,
                                executionModel.HeapModel);

                            value = valueModel.ValueText;
                            type = displayRecord.Type.Name;
                        }
                    }
                }

                // Display a call only on the last statement of a call node
                // (In future, display node records concerning argument evaluation might be added)
                var called = (calledMethod != null && displayRecord == displayRecords.Last()) ? calledMethod : null;

                var statementFlow = new StatementFlowView(
                    this,
                    this.statementFlows.Count,
                    displayRecord,
                    !string.IsNullOrEmpty(statement) ? statement : displayRecord.VariableName,
                    value,
                    type,
                    heapLocation,
                    called);
                this.statementFlows.Add(statementFlow);
            }
        }

        private async void UpdateSelectedStatement()
        {
            var methodLocation = this.location.Method.Locations.Single();
            Contract.Assert(methodLocation.IsInSource);
            var text = await methodLocation.SourceTree.GetTextAsync();

            var highlights = new Dictionary<HighlightType, IEnumerable<TextSpan>>();
            if (this.SelectedStatementFlow != null)
            {
                highlights.Add(HighlightType.Standard, new[] { this.SelectedStatementFlow.DisplayRecord.Span });
            }

            var document = this.PathView.ToolView.CurrentSolution.GetDocument(methodLocation.SourceTree);
            this.PathView.ToolView.IdeServices.OpenDocument(document);
            this.PathView.ToolView.IdeServices.HighlightText(text, highlights);

            this.PathView.ToolView.Replay.Update(this.SelectedStatementFlow);
        }
    }
}
