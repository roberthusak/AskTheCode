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
        private MethodLocation location;
        private int startIndex;
        private int length;

        private List<StatementFlowView> statementFlows;
        private StatementFlowView selectedStatementFlow;

        internal MethodFlowView(PathView pathView, MethodLocation location, int startIndex, int length)
        {
            this.PathView = pathView;
            this.location = location;
            this.startIndex = startIndex;
            this.length = length;
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
                    this.statementFlows = new List<StatementFlowView>();

                    var toolView = this.PathView.ToolView;
                    var syntaxTree = this.location.Method.Locations[0].SourceTree;
                    var text = syntaxTree.GetText();
                    ////var document = toolView.CurrentSolution.GetDocument(syntaxTree);  // TODO: Consider storing this

                    var executionModel = this.PathView.ExecutionModel;
                    FlowGraphId flowGraphId = executionModel.PathNodes[this.startIndex].Graph.Id;
                    var displayGraph = toolView.GraphProvider.GetDisplayGraph(flowGraphId);

                    // TODO: Consider optimizing
                    // TODO: Group the records by their display nodes
                    // TODO: Divide into more functions to make more readable
                    for (int i = this.startIndex; i < this.startIndex + this.length; i++)
                    {
                        var flowNode = executionModel.PathNodes[i];

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
                                bool isLastInnerNode =
                                    (i == executionModel.PathNodes.Length - 1)
                                    && displayRecord.FlowNode is InnerFlowNode;

                                var modelFactory = toolView.GraphProvider.ModelManager.TryGetFactory(displayRecord.Type);
                                if (modelFactory.ValueKind == ValueModelKind.Interpretation)
                                {
                                    var nodeInterpretations = executionModel.NodeInterpretations[i];

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

                                    var heapLocations = executionModel.HeapLocations[i];
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

                return this.statementFlows;
            }
        }

        public StatementFlowView SelectedStatementFlow
        {
            get { return this.selectedStatementFlow; }
            set { this.SetProperty(ref this.selectedStatementFlow, value); }
        }

        internal PathView PathView { get; private set; }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.SelectedStatementFlow))
            {
                this.UpdateSelectedStatement();
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
        }
    }
}
