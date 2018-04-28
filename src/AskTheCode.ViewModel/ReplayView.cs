using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using Microsoft.Msagl.Drawing;

namespace AskTheCode.ViewModel
{
    public class ReplayView : NotifyPropertyChangedBase
    {
        private HeapView heap;

        private StatementFlowView nextStatement;
        private int heapVersion;

        private VariableReplayView selectedVariable;

        internal ReplayView(ToolView toolView)
        {
            this.ToolView = toolView;
            this.heap = new HeapView(this);
            this.StepAwayCommand = new Command(this.StepAway);
            this.StepBackCommand = new Command(this.StepBack);
            this.StepOverCommand = new Command(this.StepOver);
            this.StepIntoCommand = new Command(this.StepInto);
            this.StepOutCommand = new Command(this.StepOut);
        }

        public ObservableCollection<VariableReplayView> Variables { get; } =
            new ObservableCollection<VariableReplayView>();

        public VariableReplayView SelectedVariable
        {
            get { return this.selectedVariable; }
            set { this.SetProperty(ref this.selectedVariable, value); }
        }

        public IGraphViewerConsumer Heap => this.heap;

        public Command StepAwayCommand { get; }

        public Command StepBackCommand { get; }

        public Command StepOverCommand { get; }

        public Command StepIntoCommand { get; }

        public Command StepOutCommand { get; }

        internal ToolView ToolView { get; }

        // TODO: Do it incrementally
        // TODO: Heap
        internal void Update(StatementFlowView nextStatement)
        {
            if (nextStatement == this.nextStatement)
            {
                return;
            }

            this.Variables.Clear();

            this.nextStatement = nextStatement;
            if (this.nextStatement == null)
            {
                this.heapVersion = 0;
                return;
            }

            this.heapVersion = this.nextStatement.MethodFlowView.StartHeapVersion;

            var varMap = new Dictionary<string, VariableReplayView>();

            foreach (var statement in this.nextStatement.MethodFlowView.StatementFlows)
            {
                if (statement == this.nextStatement)
                {
                    break;
                }

                if (statement.DisplayRecord.VariableName is string varName)
                {
                    if (!varMap.TryGetValue(varName, out var varView))
                    {
                        varView = new VariableReplayView(
                            varName,
                            statement.Value,
                            statement.Type,
                            statement.HeapLocation);
                        varMap.Add(varName, varView);
                        this.Variables.Add(varView);
                    }

                    varView.Value = statement.Value;
                    varView.Type = statement.Type;
                    varView.HeapLocation = statement.HeapLocation;
                }

                if (statement.HeapLocation?.HeapVersion > this.heapVersion)
                {
                    this.heapVersion = statement.HeapLocation.Value.HeapVersion;
                }
                else if (statement.CalledMethod?.EndHeapVersion > this.heapVersion)
                {
                    this.heapVersion = statement.CalledMethod.EndHeapVersion;
                }
            }

            this.heap.Redraw();
        }

        private void StepAway()
        {
            var caller = this.nextStatement?.MethodFlowView?.Caller;
            if (caller == null)
            {
                return;
            }

            var curMethod = this.nextStatement.MethodFlowView;
            var callStatement = caller.StatementFlows.First(s => s.CalledMethod == curMethod);
            this.UpdateToolSelectedStatement(callStatement);
        }

        private void StepBack()
        {
            if (this.nextStatement == null)
            {
                return;
            }

            var statements = this.nextStatement.MethodFlowView.StatementFlows;
            int trgStatementIndex = this.nextStatement.Index - 1;
            if (trgStatementIndex >= 0)
            {
                this.UpdateToolSelectedStatement(statements[trgStatementIndex]);
            }
            else
            {
                this.StepOut();
            }
        }

        private void StepOver()
        {
            if (this.nextStatement == null)
            {
                return;
            }

            var statements = this.nextStatement.MethodFlowView.StatementFlows;
            int trgStatementIndex = this.nextStatement.Index + 1;
            if (trgStatementIndex < statements.Count)
            {
                this.UpdateToolSelectedStatement(statements[trgStatementIndex]);
            }
            else
            {
                this.StepOut();
            }
        }

        private void StepInto()
        {
            var called = this.nextStatement?.CalledMethod;
            if (called == null)
            {
                // Standard behaviour as in a common IDE
                this.StepOver();
                return;
            }

            var afterParamsStmt = called.StatementFlows
                .FirstOrDefault(s => !(s.DisplayRecord.FlowNode is EnterFlowNode));
            this.UpdateToolSelectedStatement(afterParamsStmt ?? called.StatementFlows.First());
        }

        private void StepOut()
        {
            var caller = this.nextStatement?.MethodFlowView?.Caller;
            if (caller == null)
            {
                return;
            }

            var curMethod = this.nextStatement.MethodFlowView;
            var callSite = caller.StatementFlows.First(s => s.CalledMethod == curMethod);
            int trgStatementIndex = callSite.Index + 1;
            if (trgStatementIndex < caller.StatementFlows.Count)
            {
                this.UpdateToolSelectedStatement(caller.StatementFlows[trgStatementIndex]);
            }
        }

        private void UpdateToolSelectedStatement(StatementFlowView statement)
        {
            // The last assignment will cause calling this.Update(statement)
            this.ToolView.SelectedPath = statement.MethodFlowView.PathView;
            this.ToolView.SelectedPath.SelectedMethodFlow = statement.MethodFlowView;

            if (this.ToolView.SelectedPath.SelectedMethodFlow.SelectedStatementFlow == statement)
            {
                // Force update
                this.ToolView.SelectedPath.SelectedMethodFlow.SelectedStatementFlow = null;
            }

            this.ToolView.SelectedPath.SelectedMethodFlow.SelectedStatementFlow = statement;
        }

        private class HeapView : NotifyPropertyChangedBase, IGraphViewerConsumer
        {
            private readonly ReplayView owner;

            private IViewer graphViewer;

            public HeapView(ReplayView owner)
            {
                this.owner = owner;
            }

            public IViewer GraphViewer
            {
                get { return this.graphViewer; }
                set { this.SetProperty(ref this.graphViewer, value); }
            }

            public void Redraw()
            {
                if (this.GraphViewer == null)
                {
                    return;
                }

                var graph = new Graph();
                graph.Attr.LayerDirection = LayerDirection.LR;

                if (this.owner.nextStatement != null)
                {
                    var heap = this.owner.nextStatement.MethodFlowView.PathView.ExecutionModel.HeapModel;
                    int version = this.owner.heapVersion;

                    var stackNode = graph.AddNode("stack");
                    stackNode.LabelText = "Stack";

                    // TODO: Consider creating null as a separate location for each null field
                    foreach (var location in heap.GetLocations(version))
                    {
                        var node = graph.AddNode(location.Id.ToString());

                        node.LabelText = location.IsNull ? "NULL" : $"[#{location.Id}]";

                        foreach (var reference in heap.GetReferences(location))
                        {
                            graph.AddEdge(location.Id.ToString(), reference.Field.ToString(), reference.LocationId.ToString());
                        }

                        // TODO: Use proper value models to display these
                        foreach (var value in heap.GetValues(location))
                        {
                            node.LabelText += $"\n{value.Field} = {value.Value}";
                        }
                    }

                    foreach (var stackVar in this.owner.Variables.Where(v => v.HeapLocation != null))
                    {
                        graph.AddEdge(stackNode.Id, stackVar.Variable, stackVar.HeapLocation.Value.Id.ToString());
                    }
                }

                this.GraphViewer.Graph = graph;
            }

            protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
            {
                if (propertyName == nameof(this.GraphViewer))
                {
                    this.Redraw();
                }
            }
        }
    }
}
