using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.PathExploration;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ViewModel
{
    // TODO: Consider moving to another project (e.g. PathExploration.Cli?) - it is more of an logic than just view
    public class PublicMethodEntryRecognizer : IEntryPointRecognizer
    {
        public IFlowGraphProvider FlowGraphProvider { get; set; }

        public bool IsFinalNode(FlowNode node)
        {
            if (!(node is EnterFlowNode))
            {
                return false;
            }
            else
            {
                var location = this.FlowGraphProvider.GetLocation(node.Graph.Id) as MethodLocation;
                if (location == null)
                {
                    throw new InvalidOperationException();
                }

                return location.Method.DeclaredAccessibility == Accessibility.Public;
            }
        }
    }
}
