using System.Collections.Generic;

namespace AskTheCode.Core
{
    public class InspectionNode
    {
        internal InspectionNode(
            InspectionContext context,
            InspectionNode parent,
            InspectionLocation inspectionLocation,
            InspectionConditions inspectionConditions)
        {
            this.Context = context;
            this.Parent = parent;
            this.InspectionLocation = inspectionLocation;
            this.InspectionConditions = inspectionConditions;
        }

        public InspectionContext Context { get; private set; }

        public InspectionNode Parent { get; private set; }

        public InspectionLocation InspectionLocation { get; private set; }

        public InspectionConditions InspectionConditions { get; private set; }

        public InspectionNodeState State { get; internal set; }

        public IEnumerable<InspectionNode> Children { get; internal set; }
    }

    public enum InspectionNodeState
    {
        Unexplored,
        Exploring,
        Explored
    }
}