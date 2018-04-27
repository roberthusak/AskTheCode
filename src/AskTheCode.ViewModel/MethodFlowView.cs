using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
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

        internal MethodFlowView(
            PathView pathView,
            MethodFlowView caller,
            MethodLocation location,
            int startIndex,
            int endIndex)
        {
            this.PathView = pathView;
            this.Caller = caller;
            this.location = location;
            this.startIndex = startIndex;
            this.endIndex = endIndex;
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

        internal PathView PathView { get; }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.SelectedStatementFlow))
            {
                this.UpdateSelectedStatement();
            }
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
            var flowGraph = executionModel.PathNodes[this.startIndex].Graph;
            var flowGraphId = flowGraph.Id;
            var displayGraph = toolView.GraphProvider.GetDisplayGraph(flowGraphId);

            // TODO: Divide into more functions to make more readable
            for (int i = this.startIndex; i <= this.endIndex; i++)
            {
                var flowNode = executionModel.PathNodes[i];

                if (flowNode is CallFlowNode && i != this.endIndex)
                {
                    // Traverse nested method calls, possibly modifying i
                    int nestedLevel = 0;
                    int calleeStart = i + 1;
                    int calleeEnd;
                    for (int j = calleeStart; j <= this.endIndex; j++)
                    {
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
                                calleeEnd = j;
                                this.AddCallee(calleeStart, calleeEnd);

                                // Update i to correspond to the second part of the call node (stored in flowNode)
                                i = calleeEnd + 1;
                                Contract.Assert(flowNode == executionModel.PathNodes[i]);

                                break;
                            }
                        }
                    }

                    if (nestedLevel != 0)
                    {
                        // The execution model ends in the called function, so keep i the same and just display the call
                        calleeEnd = this.endIndex;
                        this.AddCallee(calleeStart, calleeEnd);
                    }
                }

                // Produce statements from display nodes
                bool isLastInnerNode =
                    (i == executionModel.PathNodes.Length - 1)
                    && flowNode is InnerFlowNode;
                var nodeInterpretations = executionModel.NodeInterpretations[i];
                var heapLocations = executionModel.HeapLocations[i];

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
                    if (displayRecord.Type != null)
                    {
                        var modelFactory = toolView.GraphProvider.ModelManager.TryGetFactory(displayRecord.Type);
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
                                var valueModel = modelFactory.GetValueModel(
                                    displayRecord.Type,
                                    heapLocations[displayRecord.FirstVariableIndex],
                                    executionModel.HeapModel);

                                value = valueModel.ValueText;
                                type = displayRecord.Type.Name;
                            }
                        }
                    }

                    var statementFlow = new StatementFlowView(this, displayRecord, statement, value, type);
                    this.statementFlows.Add(statementFlow);
                }
            }
        }

        private void AddCallee(int calleeStart, int calleeEnd)
        {
            var toolView = this.PathView.ToolView;
            var graph = this.PathView.ExecutionModel.PathNodes[calleeStart].Graph;
            var calleeLocation = toolView.GraphProvider.GetLocation(graph.Id);
            var callee = new MethodFlowView(
                this.PathView,
                this,
                calleeLocation,
                calleeStart,
                calleeEnd);
            this.callees.Add(callee);
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
        }
    }
}
