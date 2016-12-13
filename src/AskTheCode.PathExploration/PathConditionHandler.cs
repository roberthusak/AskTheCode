using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.PathExploration
{
    internal class PathConditionHandler
    {
        private readonly SmtContextHandler contextHandler;
        private readonly ISolver smtSolver;
        private readonly VersionedNameProvider nameProvider;

        private readonly Stack<LocalFlowVariableOverlay<int>> callStack =
            new Stack<LocalFlowVariableOverlay<int>>();

        private readonly FlowGraphsVariableOverlay<VariableVersionInfo> variableVersions =
            new FlowGraphsVariableOverlay<VariableVersionInfo>(() => new VariableVersionInfo());

        public PathConditionHandler(
            SmtContextHandler contextHandler,
            ISolver smtSolver,
            Path path,
            StartingNodeInfo startingNode)
            : this(contextHandler, smtSolver, path)
        {
            Contract.Requires(contextHandler != null);
            Contract.Requires(smtSolver != null);
            Contract.Requires(path != null);
            Contract.Requires(startingNode != null);

            this.smtSolver.Push();

            var innerNode = startingNode.Node as InnerFlowNode;
            if (innerNode != null && startingNode.AssignmentIndex != null)
            {
                if (startingNode.IsAssertionChecked)
                {
                    var assertionVar = innerNode.Assignments[startingNode.AssignmentIndex.Value].Variable;
                    this.smtSolver.AddAssertion(this.nameProvider, !(BoolHandle)assertionVar);
                }

                int assignmentsCount = startingNode.AssignmentIndex.Value + 1;
                var initialAssignments = innerNode.Assignments
                    .Take(assignmentsCount)
                    .Reverse();
                this.AssertAssignments(initialAssignments);
            }
        }

        private PathConditionHandler(SmtContextHandler contextHandler, ISolver smtSolver, Path path)
        {
            this.contextHandler = contextHandler;
            this.smtSolver = smtSolver;
            this.Path = path;

            this.nameProvider = new VersionedNameProvider(this);
        }

        public Path Path { get; private set; }

        public void Update(Path path)
        {
            Contract.Requires(path != null);
            Contract.Ensures(this.Path == path);

            int popCount = 0;
            var pathStack = new Stack<Path>();
            var currentRetracting = this.Path;
            var targetRetracting = path;
            while (currentRetracting != targetRetracting)
            {
                if (currentRetracting.Depth > targetRetracting.Depth)
                {
                    popCount++;

                    // TODO: Handle merged nodes
                    this.RetractVersions(currentRetracting.LeadingEdges.Single());
                    currentRetracting = currentRetracting.Preceeding.Single();
                }
                else
                {
                    pathStack.Push(targetRetracting);

                    // TODO: Handle merged nodes
                    targetRetracting = targetRetracting.Preceeding.Single();
                }
            }

            // It is done as batch for performance reasons
            if (popCount > 0)
            {
                this.smtSolver.Pop(popCount);
            }

            this.Path = currentRetracting;

            while (pathStack.Count > 0)
            {
                var currentPath = pathStack.Pop();

                this.smtSolver.Push();

                // TODO: Handle merged nodes
                var edge = currentPath.LeadingEdges.Single();
                this.Extend(edge);

                this.Path = currentPath;
            }
        }

        private void Extend(FlowEdge edge)
        {
            if (edge is InnerFlowEdge)
            {
                var innerEdge = (InnerFlowEdge)edge;
                if (innerEdge.Condition.Expression != ExpressionFactory.True)
                {
                    this.smtSolver.AddAssertion(this.nameProvider, innerEdge.Condition);
                }

                var innerNode = edge.From as InnerFlowNode;
                if (innerNode != null)
                {
                    this.AssertAssignments(innerNode.Assignments.Reverse());
                }
            }
            else
            {
                Contract.Assert(edge is OuterFlowEdge);
                var outerEdge = (OuterFlowEdge)edge;
                if (outerEdge.Kind == OuterFlowEdgeKind.Return)
                {
                    this.ExtendReturn(outerEdge);
                }
                else
                {
                    Contract.Assert(outerEdge.Kind == OuterFlowEdgeKind.MethodCall);
                    this.ExtendCall(outerEdge);
                }
            }
        }

        private void RetractVersions(FlowEdge edge)
        {
            if (edge is InnerFlowEdge)
            {
                var innerNode = edge.From as InnerFlowNode;
                if (innerNode != null)
                {
                    this.RetractAssignmentVersions(innerNode.Assignments);
                }
            }
            else
            {
                Contract.Assert(edge is OuterFlowEdge);
                var outerEdge = (OuterFlowEdge)edge;
                if (outerEdge.Kind == OuterFlowEdgeKind.Return)
                {
                    this.RetractReturnVersions(outerEdge);
                }
                else
                {
                    Contract.Assert(outerEdge.Kind == OuterFlowEdgeKind.MethodCall);
                    this.RetractCallVersions(outerEdge);
                }
            }
        }

        private void ExtendCall(OuterFlowEdge outerEdge)
        {
            Contract.Requires(outerEdge.Kind == OuterFlowEdgeKind.MethodCall);

            var callNode = (CallFlowNode)outerEdge.From;
            var enterNode = (EnterFlowNode)outerEdge.To;
            var callerGraph = callNode.Graph;

            // Obtain the current versions of the parameters
            var paramVersions = enterNode.Parameters
                .Select((variable) => this.variableVersions[variable].CurrentVersion)
                .ToArray();

            // TODO: Distinguish in the retracting
            if (this.callStack.Count > 0)
            {
                // Restore the versions from the last call of the method
                var frame = this.callStack.Pop();
                foreach (var variable in callerGraph.LocalVariables)
                {
                    this.variableVersions[variable].PushVersion(frame[variable]);
                }
            }
            else
            {
                // TODO: Create new versions only in the case of recursion
                // When there is no known call stack, make new versions of local variables
                foreach (var variable in callerGraph.LocalVariables)
                {
                    this.variableVersions[variable].PushNewVersion();
                }
            }

            // Assert the argument passing
            for (int i = 0; i < paramVersions.Length; i++)
            {
                this.AssertEquals(enterNode.Parameters[i], paramVersions[i], callNode.Arguments[i]);
            }
        }

        private void RetractCallVersions(OuterFlowEdge outerEdge)
        {
            var callerGraph = outerEdge.From.Graph;
            var frame = new LocalFlowVariableOverlay<int>();
            foreach (var variable in callerGraph.LocalVariables)
            {
                frame[variable] = this.variableVersions[variable].PopVersion();
            }

            this.callStack.Push(frame);
        }

        private void ExtendReturn(OuterFlowEdge outerEdge)
        {
            Contract.Requires(outerEdge.Kind == OuterFlowEdgeKind.Return);

            var callNode = (CallFlowNode)outerEdge.To;
            var returnNode = (ReturnFlowNode)outerEdge.From;
            var callerGraph = callNode.Graph;
            var calledGraph = returnNode.Graph;

            // Create new versions for the purpose of the return assignments
            Contract.Assert(callNode.ReturnAssignments.Count == 0
                || callNode.ReturnAssignments.Count == returnNode.ReturnValues.Count);
            var returnVersions = new int[callNode.ReturnAssignments.Count];
            for (int i = 0; i < returnVersions.Length; i++)
            {
                var variable = callNode.ReturnAssignments[i];
                var versionInfo = this.variableVersions[variable];
                returnVersions[i] = versionInfo.CurrentVersion;
                versionInfo.PushNewVersion();
            }

            var frame = new LocalFlowVariableOverlay<int>();
            foreach (var variable in callerGraph.LocalVariables)
            {
                var versionInfo = this.variableVersions[variable];
                frame[variable] = versionInfo.CurrentVersion;
                versionInfo.PushNewVersion();
            }

            this.callStack.Push(frame);

            // Assert the return assignments
            for (int i = 0; i < returnVersions.Length; i++)
            {
                this.AssertEquals(callNode.ReturnAssignments[i], returnVersions[i], returnNode.ReturnValues[i]);
            }
        }

        private void RetractReturnVersions(OuterFlowEdge outerEdge)
        {
            var callNode = (CallFlowNode)outerEdge.To;
            var callerGraph = callNode.Graph;

            // Retract either restored or new versions
            foreach (var variable in callerGraph.LocalVariables)
            {
                this.variableVersions[variable].PopVersion();
            }

            // Retract assignments after the return
            foreach (var assignedVariable in callNode.ReturnAssignments)
            {
                this.variableVersions[assignedVariable].PopVersion();
            }

            this.callStack.Pop();
        }

        private void AssertAssignments(IEnumerable<Assignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                var variable = assignment.Variable;
                var versionInfo = this.variableVersions[variable];
                int lastVersion = versionInfo.CurrentVersion;
                versionInfo.PushNewVersion();

                this.AssertEquals(variable, lastVersion, assignment.Value);
            }
        }

        private void AssertEquals(FlowVariable variable, int version, Expression value)
        {
            var symbolName = this.contextHandler.GetVariableVersionSymbol(variable, version);
            var symbolWrapper = new ConcreteVariableSymbolWrapper(variable, symbolName);

            var equal = (BoolHandle)ExpressionFactory.Equal(symbolWrapper, value);
            this.smtSolver.AddAssertion(this.nameProvider, equal);
        }

        private void RetractAssignmentVersions(IEnumerable<Assignment> assignments)
        {
            foreach (var assignment in assignments)
            {
                var versionInfo = this.variableVersions[assignment.Variable];
                versionInfo.PopVersion();
            }
        }

        private class VariableVersionInfo
        {
            private readonly Stack<int> versions;

            public VariableVersionInfo()
            {
                this.LastUsedVersion = 0;
                this.versions = new Stack<int>();
                this.versions.Push(this.LastUsedVersion);
            }

            public int LastUsedVersion { get; private set; }

            public int CurrentVersion => this.versions.Peek();

            public void PushVersion(int version)
            {
                Contract.Requires(version <= this.LastUsedVersion);

                this.versions.Push(version);
            }

            public int PushNewVersion()
            {
                int version = this.LastUsedVersion + 1;
                this.LastUsedVersion = version;
                this.versions.Push(version);

                return version;
            }

            public int PopVersion()
            {
                int version = this.versions.Pop();
                if (version == this.LastUsedVersion)
                {
                    this.LastUsedVersion--;
                }

                return version;
            }
        }

        private class ConcreteVariableSymbolWrapper : Variable
        {
            public ConcreteVariableSymbolWrapper(FlowVariable variable, SymbolName symbolName)
                : base(variable.Sort)
            {
                Contract.Requires(variable != null);
                Contract.Requires(symbolName.IsValid);

                this.Variable = variable;
                this.SymbolName = symbolName;
            }

            public override string DisplayName
            {
                get { return this.SymbolName.ToString(); }
            }

            public FlowVariable Variable { get; private set; }

            public SymbolName SymbolName { get; private set; }
        }

        private class VersionedNameProvider : INameProvider<Variable>
        {
            private PathConditionHandler owner;

            public VersionedNameProvider(PathConditionHandler owner)
            {
                this.owner = owner;
            }

            public SymbolName GetName(Variable variable)
            {
                var flowVariable = variable as FlowVariable;
                if (flowVariable != null)
                {
                    int version = this.owner.variableVersions[flowVariable].CurrentVersion;
                    return this.owner.contextHandler.GetVariableVersionSymbol(flowVariable, version);
                }
                else
                {
                    var symbolWrapper = variable as ConcreteVariableSymbolWrapper;
                    if (symbolWrapper != null)
                    {
                        return symbolWrapper.SymbolName;
                    }
                }

                throw new InvalidOperationException();
            }
        }
    }
}
