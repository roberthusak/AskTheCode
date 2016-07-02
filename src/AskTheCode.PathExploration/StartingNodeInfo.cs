using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public class StartingNodeInfo
    {
        public StartingNodeInfo(FlowNode node, int? assignmentIndex)
        {
            Contract.Requires(node != null);

            this.Node = node;
            this.AssignmentIndex = assignmentIndex;
        }

        public FlowNode Node { get; private set; }

        public int? AssignmentIndex { get; private set; }

        public Assignment? Assignment
        {
            get
            {
                if (this.AssignmentIndex == null)
                {
                    return null;
                }

                var innerNode = this.Node as InnerFlowNode;
                if (innerNode == null)
                {
                    return null;
                }

                return innerNode.Assignments[this.AssignmentIndex.Value];
            }
        }
    }
}