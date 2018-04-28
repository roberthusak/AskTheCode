using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ViewModel
{
    public class ReplayView : NotifyPropertyChangedBase
    {
        private StatementFlowView nextStatement;

        private VariableReplayView selectedVariable;

        internal ReplayView(ToolView toolView)
        {
            this.ToolView = toolView;
            this.StepOutCommand = new Command(this.StepOut);
            this.StepBackCommand = new Command(this.StepBack);
            this.StepOverCommand = new Command(this.StepOver);
            this.StepIntoCommand = new Command(this.StepInto);
        }

        public ObservableCollection<VariableReplayView> Variables { get; } =
            new ObservableCollection<VariableReplayView>();

        public VariableReplayView SelectedVariable
        {
            get { return this.selectedVariable; }
            set { this.SetProperty(ref this.selectedVariable, value); }
        }

        public Command StepOutCommand { get; }

        public Command StepBackCommand { get; }

        public Command StepOverCommand { get; }

        public Command StepIntoCommand { get; }

        internal ToolView ToolView { get; }

        // TODO: Do it incrementally
        // TODO: Heap
        internal void Update(StatementFlowView nextStatement)
        {
            this.Variables.Clear();

            this.nextStatement = nextStatement;
            if (this.nextStatement == null)
            {
                return;
            }

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
                        varView = new VariableReplayView(varName, statement.Value, statement.Type, null);
                        varMap.Add(varName, varView);
                        this.Variables.Add(varView);
                    }

                    varView.Value = statement.Value;
                    varView.Type = statement.Type;
                }
            }
        }

        private void StepOut()
        {
            // TODO
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
        }

        private void StepInto()
        {
            // TODO
        }

        private void UpdateToolSelectedStatement(StatementFlowView statement)
        {
            // The last assignment will cause calling this.Update(statement)
            this.ToolView.SelectedPath = statement.MethodFlowView.PathView;
            this.ToolView.SelectedPath.SelectedMethodFlow = statement.MethodFlowView;
            this.ToolView.SelectedPath.SelectedMethodFlow.SelectedStatementFlow = statement;
        }
    }
}
