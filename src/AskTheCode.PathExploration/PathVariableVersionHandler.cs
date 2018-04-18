using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Operations;
using AskTheCode.ControlFlowGraphs.Overlays;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    internal class PathVariableVersionHandler
    {
        private readonly StartingNodeInfo startingNode;

        private readonly Stack<LocalFlowVariableOverlay<int>> callStack;
        private readonly FlowGraphsVariableOverlay<VariableVersionInfo> variableVersions;

        private readonly OperationAssertionHandler operationAssertionHandler;
        private readonly OperationRetractionHandler operationRetractionHandler;

        private Stack<CallExtensionKind> callExtensionKindStack;

        public PathVariableVersionHandler(
            Path path,
            StartingNodeInfo startingNode,
            SmtContextHandler smtContextHandler)
            : this()
        {
            Contract.Requires(path != null);
            Contract.Requires(startingNode != null);
            Contract.Requires(smtContextHandler != null);

            this.startingNode = startingNode;
            this.callStack = new Stack<LocalFlowVariableOverlay<int>>();
            this.variableVersions = new FlowGraphsVariableOverlay<VariableVersionInfo>(() => new VariableVersionInfo());
            this.callExtensionKindStack = new Stack<CallExtensionKind>();
            this.Path = path;
            this.SmtContextHandler = smtContextHandler;
        }

        protected PathVariableVersionHandler(PathVariableVersionHandler other)
            : this()
        {
            this.startingNode = other.startingNode;
            this.callStack = new Stack<LocalFlowVariableOverlay<int>>(
                other.callStack.Select(overlay => overlay.Clone()));
            this.variableVersions = other.variableVersions.Clone(varInfo => varInfo.Clone());
            this.Path = other.Path;
            this.SmtContextHandler = other.SmtContextHandler;
            this.callExtensionKindStack = new Stack<CallExtensionKind>(other.callExtensionKindStack.Reverse());
        }

        private PathVariableVersionHandler()
        {
            this.operationAssertionHandler = new OperationAssertionHandler(this);
            this.operationRetractionHandler = new OperationRetractionHandler(this);

            this.NameProvider = new VersionedNameProvider(this);
        }

        private enum CallExtensionKind : byte
        {
            Free,
            CallStackBound
        }

        public Path Path { get; private set; }

        public INameProvider<Variable> NameProvider { get; }

        protected SmtContextHandler SmtContextHandler { get; }

        public int GetVariableVersion(FlowVariable variable)
        {
            // TODO: Consider turning null into a global variable to avoid this check
            if (variable == References.Null)
            {
                return VersionedVariable.Null.Version;
            }
            else
            {
                return this.variableVersions[variable].CurrentVersion;
            }
        }

        public VersionedVariable GetVersioned(FlowVariable variable)
        {
            int version = this.GetVariableVersion(variable);
            return new VersionedVariable(variable, version);
        }

        public void Update(Path path)
        {
            Contract.Requires(path != null);

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
                    var retractingEdge = currentRetracting.LeadingEdges.Single();
                    this.OnBeforePathStepRetracted(retractingEdge);

                    this.Retract(retractingEdge);
                    currentRetracting = currentRetracting.Preceeding.Single();

                    this.Path = currentRetracting;

                    this.OnAfterPathStepRetracted(retractingEdge);
                }
                else
                {
                    pathStack.Push(targetRetracting);

                    // TODO: Handle merged nodes
                    targetRetracting = targetRetracting.Preceeding.Single();
                }
            }

            this.OnAfterPathRetracted(popCount);

            while (pathStack.Count > 0)
            {
                var currentPath = pathStack.Pop();


                // TODO: Handle merged nodes
                var edge = currentPath.LeadingEdges.Single();

                this.OnBeforePathStepExtended(edge);
                this.Extend(edge);
                this.OnAfterPathStepExtended(edge);

                this.Path = currentPath;
            }

            // TODO: Turn back to Contract.Ensures when possible
            Contract.Assert(this.Path == path);
        }

        public void RetractToRoot()
        {
            // TODO: Consider making it faster by creating a simplified version of Update() instead
            var path = this.Path;
            while (!path.IsRoot)
            {
                path = path.Preceeding.First();
            }

            this.Update(path);
        }

        public void ProcessStartingNode()
        {
            Contract.Requires(this.Path.Node == this.startingNode.Node);

            if (this.startingNode.Node is InnerFlowNode innerNode && this.startingNode.AssignmentIndex != null)
            {
                if (this.startingNode.IsAssertionChecked)
                {
                    if (this.startingNode.Operation is Assignment assignment)
                    {
                        var assertionVar = assignment.Variable;
                        if (assignment.Value is ReferenceComparisonVariable refComp)
                        {
                            var leftVar = this.GetVersioned(refComp.Left);
                            var rightVar = this.GetVersioned(refComp.Right);
                            this.OnReferenceEqualityAsserted(!refComp.AreEqual, leftVar, rightVar);
                        }
                        else
                        {
                            this.OnConditionAsserted(!(BoolHandle)assertionVar);
                        }
                    }
                    else if (this.startingNode.Operation is FieldOperation fieldOp)
                    {
                        // Check whether can the operation fail due to null dereference
                        var leftVar = this.GetVersioned(fieldOp.Reference);
                        var nullVar = this.GetVersioned(References.Null);
                        this.OnReferenceEqualityAsserted(true, leftVar, nullVar);
                    }
                }

                // TODO: Consider skipping the assertion itself in the case of heap
                int operationCount = this.startingNode.AssignmentIndex.Value + 1;
                var initialOperations = innerNode.Operations
                    .Take(operationCount)
                    .Reverse();
                this.AssertOperations(initialOperations);
            }
        }

        protected void RetractStartingNode()
        {
            // TODO: Reflect heap
            var innerNode = this.startingNode.Node as InnerFlowNode;
            if (innerNode != null && this.startingNode.AssignmentIndex != null)
            {
                int operationCount = this.startingNode.AssignmentIndex.Value + 1;
                this.RetractOperations(innerNode.Operations.Take(operationCount));
            }
        }

        protected virtual void OnBeforePathStepExtended(FlowEdge edge)
        {
        }

        protected virtual void OnAfterPathStepExtended(FlowEdge edge)
        {
        }

        protected virtual void OnBeforePathStepRetracted(FlowEdge edge)
        {
        }

        protected virtual void OnAfterPathStepRetracted(FlowEdge edge)
        {
        }

        protected virtual void OnAfterPathRetracted(int popCount)
        {
        }

        protected virtual void OnConditionAsserted(BoolHandle condition)
        {
        }

        protected virtual void OnRandomVariableRetracted(FlowVariable variable, int version)
        {
        }

        protected virtual void OnVariableAssigned(FlowVariable variable, int lastVersion, Expression value)
        {
        }

        /// <remarks>
        /// Beware that when this method is invoked, the version of the variable is already retracted to the one on the
        /// left side of the assignment.
        /// </remarks>
        protected virtual void OnVariableAssignmentRetracted(
            FlowVariable variable,
            int assignedVersion,
            Expression value)
        {
        }

        protected virtual void OnReferenceEqualityAsserted(
            bool areEqual,
            VersionedVariable left,
            VersionedVariable right)
        {
        }

        protected virtual void OnReferenceEqualityRetracted(
            bool areEqual,
            VersionedVariable left,
            VersionedVariable right)
        {
        }

        protected virtual void OnFieldReadAsserted(
            VersionedVariable result,
            VersionedVariable reference,
            IFieldDefinition field)
        {
        }

        protected virtual void OnFieldReadRetracted(
            VersionedVariable result,
            VersionedVariable reference,
            IFieldDefinition field)
        {
        }

        protected virtual void OnFieldWriteAsserted(
            VersionedVariable reference,
            IFieldDefinition field,
            Expression value)
        {
        }

        protected virtual void OnFieldWriteRetracted(
            VersionedVariable reference,
            IFieldDefinition field,
            Expression value)
        {
        }

        private void Extend(FlowEdge edge)
        {
            if (edge is InnerFlowEdge)
            {
                var innerEdge = (InnerFlowEdge)edge;
                Expression condExpr = innerEdge.Condition.Expression;
                if (condExpr != ExpressionFactory.True)
                {
                    if (condExpr is ReferenceComparisonVariable refComp)
                    {
                        var varLeft = this.GetVersioned(refComp.Left);
                        var varRight = this.GetVersioned(refComp.Right);
                        this.OnReferenceEqualityAsserted(refComp.AreEqual, varLeft, varRight);
                    }
                    else
                    {
                        this.OnConditionAsserted(innerEdge.Condition);
                    }
                }

                var innerNode = edge.From as InnerFlowNode;
                if (innerNode != null)
                {
                    this.AssertOperations(innerNode.Operations.Reverse());
                }
                else if ((edge.From as CallFlowNode)?.Location.CanBeExplored == false)
                {
                    this.ExtendUnmodelledCall((CallFlowNode)edge.From);
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

        private void Retract(FlowEdge edge)
        {
            if (edge is InnerFlowEdge innerEdge)
            {
                if (innerEdge.Condition.Expression is ReferenceComparisonVariable refComp)
                {
                    var varLeft = this.GetVersioned(refComp.Left);
                    var varRight = this.GetVersioned(refComp.Right);
                    this.OnReferenceEqualityRetracted(refComp.AreEqual, varLeft, varRight);
                }

                var innerNode = edge.From as InnerFlowNode;
                if (innerNode != null)
                {
                    this.RetractOperations(innerNode.Operations);
                }
                else if ((edge.From as CallFlowNode)?.Location.CanBeExplored == false)
                {
                    this.RetractUnmodelledCall((CallFlowNode)edge.From);
                }
            }
            else
            {
                Contract.Assert(edge is OuterFlowEdge);
                var outerEdge = (OuterFlowEdge)edge;
                if (outerEdge.Kind == OuterFlowEdgeKind.Return)
                {
                    this.RetractReturn(outerEdge);
                }
                else
                {
                    Contract.Assert(outerEdge.Kind == OuterFlowEdgeKind.MethodCall);
                    this.RetractCall(outerEdge);
                }
            }
        }

        private void ExtendUnmodelledCall(CallFlowNode callNode)
        {
            Contract.Requires(!callNode.Location.CanBeExplored);

            foreach (var returnedVariable in callNode.ReturnAssignments)
            {
                this.variableVersions[returnedVariable].PushNewVersion();
            }
        }

        private void RetractUnmodelledCall(CallFlowNode callNode)
        {
            Contract.Requires(!callNode.Location.CanBeExplored);

            foreach (var returnedVariable in callNode.ReturnAssignments)
            {
                var versionInfo = this.variableVersions[returnedVariable];
                versionInfo.PopVersion();
                this.OnRandomVariableRetracted(returnedVariable, versionInfo.CurrentVersion);
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

            if (this.callStack.Count > 0)
            {
                this.callExtensionKindStack.Push(CallExtensionKind.CallStackBound);

                // Restore the versions from the last call of the method
                var frame = this.callStack.Pop();
                foreach (var variable in callerGraph.LocalVariables)
                {
                    this.variableVersions[variable].PushVersion(frame[variable]);
                }
            }
            else
            {
                this.callExtensionKindStack.Push(CallExtensionKind.Free);

                // TODO: Create new versions only in the case of recursion
                // When there is no known call stack, make new versions of local variables
                foreach (var variable in callerGraph.LocalVariables)
                {
                    this.variableVersions[variable].PushNewVersion();
                }
            }

            // Assert the argument passing, skipping the first parameter in the case of constructor
            int startParamIndex = callNode.IsObjectCreation ? 1 : 0;
            for (int i = startParamIndex; i < paramVersions.Length; i++)
            {
                this.OnVariableAssigned(enterNode.Parameters[i], paramVersions[i], callNode.Arguments[i]);
            }
        }

        private void RetractCall(OuterFlowEdge outerEdge)
        {
            if (this.callExtensionKindStack.Pop() == CallExtensionKind.CallStackBound)
            {
                // If we know we might return to this stack frame during the further retraction,
                // store the current variable versions to the call stack
                var callerGraph = outerEdge.From.Graph;
                var frame = new LocalFlowVariableOverlay<int>();
                foreach (var variable in callerGraph.LocalVariables)
                {
                    frame[variable] = this.variableVersions[variable].PopVersion();
                }

                this.callStack.Push(frame);
            }

            var enterNode = (EnterFlowNode)outerEdge.To;
            foreach (var param in enterNode.Parameters)
            {
                // TODO: Consider passing also the values instead of null
                this.OnVariableAssignmentRetracted(param, this.variableVersions[param].CurrentVersion, null);
            }
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
                this.OnVariableAssigned(callNode.ReturnAssignments[i], returnVersions[i], returnNode.ReturnValues[i]);
            }
        }

        private void RetractReturn(OuterFlowEdge outerEdge)
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
                var versionInfo = this.variableVersions[assignedVariable];
                versionInfo.PopVersion();
                // TODO: Consider passing also the values instead of null (or removing the parameter)
                this.OnVariableAssignmentRetracted(assignedVariable, versionInfo.CurrentVersion, null);
            }

            this.callStack.Pop();
        }

        private void AssertOperations(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                this.operationAssertionHandler.Visit(operation);
            }
        }

        private void RetractOperations(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                this.operationRetractionHandler.Visit(operation);
            }
        }

        protected class VersionedNameProvider : INameProvider<Variable>
        {
            private PathVariableVersionHandler owner;

            public VersionedNameProvider(PathVariableVersionHandler owner)
            {
                this.owner = owner;
            }

            public SymbolName GetName(Variable variable)
            {
                if (variable is FlowVariable flowVariable)
                {
                    int version = this.owner.GetVariableVersion(flowVariable);
                    return this.owner.SmtContextHandler.GetVariableVersionSymbol(flowVariable, version);
                }

                // TODO: Implement handling of ReferenceComparisonVariable to enable it to appear in structured expressions
                //       (now it can be only on the right side of an assignment by itself)
                throw new InvalidOperationException();
            }
        }

        private class OperationAssertionHandler : OperationVisitor
        {
            private readonly PathVariableVersionHandler parent;

            public OperationAssertionHandler(PathVariableVersionHandler parent)
            {
                this.parent = parent;
            }

            public override void VisitAssignment(Assignment assignment)
            {
                int lastVersion = this.PostIncrementVersion(assignment.Variable);

                this.parent.OnVariableAssigned(assignment.Variable, lastVersion, assignment.Value);
            }

            public override void VisitFieldRead(FieldRead fieldRead)
            {
                int lastResultVersion = this.PostIncrementVersion(fieldRead.ResultStore);

                this.parent.OnFieldReadAsserted(
                    new VersionedVariable(fieldRead.ResultStore, lastResultVersion),
                    this.parent.GetVersioned(fieldRead.Reference),
                    fieldRead.Field);
            }

            public override void VisitFieldWrite(FieldWrite fieldWrite)
            {
                this.parent.OnFieldWriteAsserted(
                    this.parent.GetVersioned(fieldWrite.Reference),
                    fieldWrite.Field,
                    fieldWrite.Value);
            }

            private int PostIncrementVersion(FlowVariable variable)
            {
                var versionInfo = this.parent.variableVersions[variable];
                int lastVersion = versionInfo.CurrentVersion;
                versionInfo.PushNewVersion();
                return lastVersion;
            }
        }

        private class OperationRetractionHandler : OperationVisitor
        {
            private readonly PathVariableVersionHandler parent;

            public OperationRetractionHandler(PathVariableVersionHandler parent)
            {
                this.parent = parent;
            }

            public override void VisitAssignment(Assignment assignment)
            {
                var variable = assignment.Variable;
                int assignedVersion = this.PreDecrementVersion(variable);

                this.parent.OnVariableAssignmentRetracted(assignment.Variable, assignedVersion, assignment.Value);
            }

            public override void VisitFieldRead(FieldRead fieldRead)
            {
                int resultVersion = this.PreDecrementVersion(fieldRead.ResultStore);

                this.parent.OnFieldReadRetracted(
                    new VersionedVariable(fieldRead.ResultStore, resultVersion),
                    this.parent.GetVersioned(fieldRead.Reference),
                    fieldRead.Field);
            }

            public override void VisitFieldWrite(FieldWrite fieldWrite)
            {
                this.parent.OnFieldWriteRetracted(
                    this.parent.GetVersioned(fieldWrite.Reference),
                    fieldWrite.Field,
                    fieldWrite.Value);
            }

            private int PreDecrementVersion(FlowVariable variable)
            {
                var versionInfo = this.parent.variableVersions[variable];
                versionInfo.PopVersion();
                return versionInfo.CurrentVersion;
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

            private VariableVersionInfo(VariableVersionInfo other)
            {
                this.LastUsedVersion = other.LastUsedVersion;
                this.versions = new Stack<int>(other.versions.Reverse());
            }

            public int LastUsedVersion { get; private set; }

            public int CurrentVersion => this.versions.Peek();

            public VariableVersionInfo Clone()
            {
                return new VariableVersionInfo(this);
            }

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
    }
}
