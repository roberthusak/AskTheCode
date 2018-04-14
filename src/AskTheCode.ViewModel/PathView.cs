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

                    var pathNodes = this.ExecutionModel.PathNodes;
                    Contract.Assert(pathNodes.Length > 0);

                    int currentSegmentStart = 0;
                    var currentGraph = pathNodes[0].Graph;
                    for (int i = 0; i < pathNodes.Length; i++)
                    {
                        if (pathNodes[i].Graph != currentGraph)
                        {
                            var location = this.ToolView.GraphProvider.GetLocation(currentGraph.Id);
                            var methodFlow = new MethodFlowView(
                                this,
                                location,
                                currentSegmentStart,
                                i - currentSegmentStart);
                            this.methodFlows.Add(methodFlow);
                            currentSegmentStart = i;
                            currentGraph = pathNodes[i].Graph;
                        }
                    }

                    var locationLast = this.ToolView.GraphProvider.GetLocation(currentGraph.Id);
                    var methodFlowLast = new MethodFlowView(
                        this,
                        locationLast,
                        currentSegmentStart,
                        pathNodes.Length - currentSegmentStart);
                    this.methodFlows.Add(methodFlowLast);
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
