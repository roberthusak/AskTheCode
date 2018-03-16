using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal struct FlowNodeMappedInfo
    {
        public FlowNodeMappedInfo(FlowNode flowNode, int assignmentOffset = 0)
        {
            Contract.Requires(flowNode != null);

            this.FlowNode = flowNode;
            this.AssignmentOffset = assignmentOffset;
        }

        public FlowNode FlowNode { get; private set; }

        public int AssignmentOffset { get; private set; }

        public static implicit operator FlowNodeMappedInfo(FlowNode flowNode)
        {
            return new FlowNodeMappedInfo(flowNode);
        }

        public static implicit operator FlowNode(FlowNodeMappedInfo mappedInfo)
        {
            return mappedInfo.FlowNode;
        }
    }
}
