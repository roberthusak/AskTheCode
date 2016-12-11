using System;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;

namespace AskTheCode.PathExploration
{
    internal class VariableVersionManager
    {
        private FlowGraphsVariableOverlay<int> lastUsedVersions = new FlowGraphsVariableOverlay<int>();

        public VariableVersionManager()
        {
        }

        public void CreateNewVersion(FlowVariable variable)
        {
            this.lastUsedVersions[variable]++;
        }

        public void RetractVersion(FlowVariable variable)
        {
            this.lastUsedVersions[variable]--;
        }

        public int GetVersion(FlowVariable variable)
        {
            return this.lastUsedVersions[variable];
        }
    }
}