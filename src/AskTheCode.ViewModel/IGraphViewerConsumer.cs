using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using Microsoft.Msagl.Drawing;

namespace AskTheCode.ViewModel
{
    public interface IGraphViewerConsumer
    {
        IViewer GraphViewer { get; set; }
    }
}
