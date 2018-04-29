using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.PathExploration.Heap;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    internal class PathConditionHandler : PathVariableVersionHandler
    {
        private readonly ISolver smtSolver;

        public PathConditionHandler(
            SmtContextHandler smtContextHandler,
            ISolver smtSolver,
            Path path,
            StartingNodeInfo startingNode,
            ISymbolicHeap heap)
            : base(path, startingNode, smtContextHandler)
        {
            Contract.Requires(smtSolver != null);
            Contract.Requires(heap != null);

            this.smtSolver = smtSolver;
            this.Heap = heap;

            this.smtSolver.Push();
        }

        internal ISymbolicHeap Heap { get; }

        protected override void OnAfterPathRetracted(int popCount)
        {
            // It is done as batch for performance reasons
            if (popCount > 0)
            {
                this.smtSolver.Pop(popCount);
            }
        }

        protected override void OnBeforePathStepExtended(FlowEdge edge)
        {
            this.smtSolver.Push();

            if (edge is OuterFlowEdge outerEdge)
            {
                if (outerEdge.Kind == OuterFlowEdgeKind.MethodCall
                    && outerEdge.To is EnterFlowNode enterNode
                    && ((CallFlowNode)outerEdge.From).IsObjectCreation)
                {
                    // This is needed in the case when the exploration itself started from a constructor (or from a
                    // method called by it). We need to let the heap know that this object is not a part of the input
                    // heap. Notice that we tell the heap that we might have already marked this object as such.
                    var newVar = enterNode.Parameters[0];
                    var versionedVar = new VersionedVariable(newVar, this.GetVariableVersion(newVar));
                    this.Heap.AllocateNew(versionedVar, mightBeRepeated: true);
                }
            }
        }

        protected override void OnAfterPathStepExtended(FlowEdge edge)
        {
            if (edge is OuterFlowEdge outerEdge)
            {
                if (outerEdge.Kind == OuterFlowEdgeKind.Return
                    && ((CallFlowNode)outerEdge.To).IsObjectCreation)
                {
                    // When first encountering the constructor call, assert that the resulting reference must be
                    // a newly allocated object that couldn't have be marked as such before.
                    var newVar = outerEdge.From.Graph.Nodes.OfType<EnterFlowNode>().First().Parameters[0];
                    var versionedVar = new VersionedVariable(newVar, this.GetVariableVersion(newVar));
                    this.Heap.AllocateNew(versionedVar, mightBeRepeated: false);
                }
            }
        }

        protected override void OnAfterPathStepRetracted(FlowEdge edge)
        {
            if (edge is OuterFlowEdge outerEdge
                && (outerEdge.Kind == OuterFlowEdgeKind.MethodCall || outerEdge.Kind == OuterFlowEdgeKind.Return))
            {
                var callNode = outerEdge.From as CallFlowNode ?? (CallFlowNode)outerEdge.To;
                if (callNode.IsObjectCreation)
                {
                    this.Heap.Retract();
                }
            }
        }

        protected override void OnConditionAsserted(BoolHandle condition)
        {
            this.smtSolver.AddAssertion(this.NameProvider, condition);
        }

        protected override void OnVariableAssigned(
            FlowVariable variable,
            int lastVersion,
            Expression value)
        {
            if (variable.IsReference)
            {
                var leftRef = new VersionedVariable(variable, lastVersion);
                var rightRef = this.GetVersioned((FlowVariable)value);

                this.Heap.AssertEquality(true, leftRef, rightRef);
            }
            else if (value.Sort == Sort.Bool && value is ReferenceComparisonVariable refComp)
            {
                value = this.GetReferenceComparisonExpression(refComp);
            }

            if (!variable.IsReference)
            {
                this.AssertEquals(variable, lastVersion, value);
            }
        }

        protected override void OnVariableAssignmentRetracted(
            FlowVariable variable,
            int assignedVersion,
            Expression value)
        {
            if (variable.IsReference
                || (value.Sort == Sort.Bool && value is ReferenceComparisonVariable))
            {
                this.Heap.Retract();
            }
        }

        protected override void OnReferenceEqualityAsserted(
            bool areEqual,
            VersionedVariable left,
            VersionedVariable right)
        {
            this.Heap.AssertEquality(areEqual, left, right);
        }

        protected override void OnReferenceEqualityRetracted(
            bool areEqual,
            VersionedVariable left,
            VersionedVariable right)
        {
            this.Heap.Retract();
        }

        protected override void OnFieldReadAsserted(
            VersionedVariable result,
            VersionedVariable reference,
            IFieldDefinition field)
        {
            this.Heap.ReadField(result, reference, field);
        }

        protected override void OnFieldReadRetracted(
            VersionedVariable result,
            VersionedVariable reference,
            IFieldDefinition field)
        {
            this.Heap.Retract();
        }

        protected override void OnFieldWriteAsserted(
            VersionedVariable reference,
            IFieldDefinition field,
            Expression value)
        {
            this.Heap.WriteField(reference, field, value);
        }

        protected override void OnFieldWriteRetracted(
            VersionedVariable reference,
            IFieldDefinition field,
            Expression value)
        {
            this.Heap.Retract();
        }

        private void AssertEquals(FlowVariable variable, int version, Expression value)
        {
            var symbolName = this.SmtContextHandler.GetVariableVersionSymbol(variable, version);
            var symbolWrapper = ExpressionFactory.NamedVariable(variable.Sort, symbolName);

            var equal = (BoolHandle)ExpressionFactory.Equal(symbolWrapper, value);
            this.smtSolver.AddAssertion(this.NameProvider, equal);
        }

        private Expression GetReferenceComparisonExpression(ReferenceComparisonVariable refComp)
        {
            var varLeft = this.GetVersioned(refComp.Left);
            var varRight = this.GetVersioned(refComp.Right);
            return this.Heap.GetEqualityExpression(refComp.AreEqual, varLeft, varRight);
        }
    }
}
