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
        //private MethodFlowView currentMethod;

        private VariableReplayView selectedVariable;

        internal ReplayView(ToolView toolView)
        {
            this.ToolView = toolView;
            this.StepIntoCommand = new Command(this.StepInto);
        }

        public ObservableCollection<VariableReplayView> Variables { get; } =
            new ObservableCollection<VariableReplayView>();

        public VariableReplayView SelectedVariable
        {
            get { return this.selectedVariable; }
            set { this.SetProperty(ref this.selectedVariable, value); }
        }

        public Command StepIntoCommand { get; }

        internal ToolView ToolView { get; }

        // TODO: Do it incrementally
        // TODO: Heap
        internal void Update(StatementFlowView nextStatement)
        {
            this.Variables.Clear();

            if (nextStatement == null)
            {
                return;
            }

            var varMap = new Dictionary<string, VariableReplayView>();

            foreach (var statement in nextStatement.MethodFlowView.StatementFlows)
            {
                if (statement == nextStatement)
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

        private void StepInto()
        {
            // TODO
        }
    }
}
