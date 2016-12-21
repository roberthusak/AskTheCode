using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;

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
                        var nodeInterpretations = executionModel.NodeInterpretations[i];

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

                        displayRecords.Reverse();
                        foreach (var displayRecord in displayRecords)
                        {
                            Contract.Assert(displayRecord != null);
                            string statement = text.ToString(displayRecord.Span);
                            string value = null;
                            string type = null;
                            if (displayRecord.Type != null)
                            {
                                // Hide the remaining portion of the inner CFG node where the exploration started from
                                if (i == 0
                                    && displayRecord.FlowNode is InnerFlowNode
                                    && displayRecord.FirstVariableIndex >= nodeInterpretations.Length)
                                {
                                    continue;
                                }

                                var modelFactory = toolView.GraphProvider.ModelManager.TryGetFactory(displayRecord.Type);
                                var sortRequirements = modelFactory.GetExpressionSortRequirements(displayRecord.Type);
                                var interpretations = nodeInterpretations
                                    .Reverse()
                                    .Skip(displayRecord.FirstVariableIndex)
                                    .Take(sortRequirements.Count)
                                    .ToArray();

                                if (interpretations.Length != 0
                                    && interpretations.All(interpretation => interpretation != null))
                                {
                                    var valueModel = modelFactory.GetValueModel(displayRecord.Type, interpretations);
                                    Contract.Assert(valueModel != null);
                                    value = valueModel.ValueText;
                                    type = displayRecord.Type.Name;
                                }
                            }

                            var statementFlow = new StatementFlowView(this, displayRecord, statement, value, type);
                            this.statementFlows.Add(statementFlow);
                        }
                    }

                    // TODO: Consider doing this sooner (on the path level?)
                    this.statementFlows.Reverse();
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
    }
}
