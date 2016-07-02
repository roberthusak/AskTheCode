using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class FlowGraphsNodeOverlay<T> : FlowGraphOverlay<FlowNodeOverlay<T>>
    {
        public FlowGraphsNodeOverlay(Func<T> defaultValueFactory = null)
            : base(() => new FlowNodeOverlay<T>(defaultValueFactory))
        {
        }

        public T this[FlowNode node]
        {
            get { return this[node.Graph][node]; }
            set { this[node.Graph][node] = value; }
        }
    }
}
