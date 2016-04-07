using System.Collections.Generic;

namespace AskTheCode.Core
{
    public enum InspectionNodeState
    {
        Unexplored,
        Exploring,
        Explored
    }

    public sealed class InspectionNode
    {
        internal InspectionNode(
            InspectionContext context,
            InspectionNode parent,
            InspectionLocation inspectionLocation,
            InspectionConditions inspectionConditions)
        {
            this.Context = context;
            this.Parent = parent;
            this.Location = inspectionLocation;
            this.Conditions = inspectionConditions;
        }

        public InspectionContext Context { get; private set; }

        public InspectionNode Parent { get; private set; }

        public InspectionLocation Location { get; private set; }

        public InspectionConditions Conditions { get; private set; }

        public InspectionNodeState State { get; internal set; }

        public IEnumerable<InspectionNode> Children { get; internal set; }
    }
}