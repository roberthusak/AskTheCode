using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.PathExploration;

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
        }

        // TODO: Change to something actually useful
        public string Name
        {
            get { return $"A path (length: {this.ExecutionModel.PathNodes.Length})"; }
        }

        // TODO: Consider changing into a immutable collection (still lazy loaded)
        public ObservableCollection<MethodFlowView> MethodFlows
        {
            get
            {
                if (this.methodFlows == null)
                {
                    this.methodFlows = new ObservableCollection<MethodFlowView>();

                    // TODO: Handle multiple methods in one execution model
                    var flowGraphId = this.ExecutionModel.PathNodes[0].Graph.Id;
                    var location = this.ToolView.GraphProvider.GetLocation(flowGraphId);
                    var methodFlow = new MethodFlowView(this, location, 0, this.ExecutionModel.PathNodes.Length);
                    this.methodFlows.Add(methodFlow);
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
    }
}
