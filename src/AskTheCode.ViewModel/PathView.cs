using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.PathExploration;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ViewModel
{
    public class PathView : NotifyPropertyChangedBase
    {
        private ObservableCollection<MethodFlowView> methodFlows;
        private MethodFlowView selectedMethodFlow;

        internal PathView(ToolView toolView, ExecutionModel executionModel)
        {
            this.ToolView = toolView;
            this.ExecutionModel = executionModel;

            this.Name = this.ComputeName();
        }

        public string Name { get; }

        // TODO: Consider changing into a immutable collection (still lazy loaded)
        public ObservableCollection<MethodFlowView> MethodFlows
        {
            get
            {
                if (this.methodFlows == null)
                {
                    this.methodFlows = new ObservableCollection<MethodFlowView>();

                    var pathNodes = this.ExecutionModel.PathNodes;
                    Contract.Assert(pathNodes.Length > 0);

                    var rootLocation = this.ToolView.GraphProvider.GetLocation(pathNodes[0].Graph.Id);
                    int maxHeapVersion = this.ExecutionModel.HeapModel.MaxVersion;
                    var root = new MethodFlowView(this, null, rootLocation, 0, pathNodes.Length - 1, 0, maxHeapVersion);

                    this.methodFlows.Add(root);
                }

                return this.methodFlows;
            }
        }

        public MethodFlowView SelectedMethodFlow
        {
            get { return this.selectedMethodFlow; }
            set { this.SetProperty(ref this.selectedMethodFlow, value); }
        }

        internal ToolView ToolView { get; private set; }

        internal ExecutionModel ExecutionModel { get; private set; }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.SelectedMethodFlow))
            {
                this.UpdateSelectedMethod(previousValue as MethodFlowView);
            }
        }

        private void UpdateSelectedMethod(MethodFlowView previousValue)
        {
            if (previousValue != null)
            {
                previousValue.IsSelected = false;
            }

            if (this.SelectedMethodFlow == null)
            {
                return;
            }

            // Select the appropriate method
            this.SelectedMethodFlow.IsSelected = true;

            // Expand all the methods leading to it in the chain
            var caller = this.SelectedMethodFlow.Caller;
            while (caller != null)
            {
                caller.IsExpanded = true;
                caller = caller.Caller;
            }
        }

        private string ComputeName()
        {
            // TODO: Consider displaying only the methods in the call stack of the target
            var result = new List<string>();
            int lastCfgId = -1;
            foreach (var node in this.ExecutionModel.PathNodes)
            {
                ControlFlowGraphs.FlowGraphId cfgId = node.Graph.Id;
                if (cfgId.Value != lastCfgId)
                {
                    var location = this.ToolView.GraphProvider.GetLocation(cfgId);
                    result.Add(location.ToString());

                    lastCfgId = cfgId.Value;
                }
            }

            return string.Join(" \u2192 ", result);
        }
    }
}
