using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuildNode : IIdReferenced<BuildNodeId>
    {
        private FlowNodeFlags flags;
        private List<BuildEdge> outgoingEdges = new List<BuildEdge>();
        private SyntaxNode syntax;
        private SyntaxNodeOrToken? labelOverride;
        private ITypeModel variableModel;
        private ITypeModel valueModel;
        private SpecialOperation operation;
        private DisplayNode displayNode;

        public BuildNode(BuildNodeId id, SyntaxNode syntax)
        {
            this.Id = id;
            this.Syntax = syntax;
        }

        public BuildNodeId Id { get; }

        public FlowNodeFlags Flags
        {
            get => this.flags;
            set => this.flags = value;
        }

        public List<BuildEdge> OutgoingEdges
        {
            get { return this.outgoingEdges; }
        }

        // TODO: Optimize the type if necessary (make 2 fields?)
        public SyntaxNode Syntax
        {
            get { return this.syntax; }
            set { this.syntax = value; }
        }

        public SyntaxNodeOrToken? LabelOverride
        {
            get { return this.labelOverride; }
            set { this.labelOverride = value; }
        }

        public SyntaxNodeOrToken Label
        {
            get { return (this.LabelOverride != null) ? this.LabelOverride.Value : this.Syntax; }
        }

        public ITypeModel VariableModel
        {
            get { return this.variableModel; }
            set { this.SetVariableModelImpl(value); }
        }

        public ITypeModel ValueModel
        {
            get { return this.valueModel; }
            set { DataHelper.SetOnceAssert(ref this.valueModel, value); }
        }

        public SpecialOperation Operation
        {
            get { return this.operation; }
            set { DataHelper.SetOnceAssert(ref this.operation, value); }
        }

        public DisplayNode DisplayNode
        {
            get { return this.displayNode; }
            set { DataHelper.SetOnceAssert(ref this.displayNode, value); }
        }

        public void SwapFlags(BuildNode other)
        {
            DataHelper.Swap(ref this.flags, ref other.flags);
        }

        public void SwapOutgoingEdges(BuildNode other)
        {
            DataHelper.Swap(ref this.outgoingEdges, ref other.outgoingEdges);
        }

        public void SwapSyntax(BuildNode other)
        {
            DataHelper.Swap(ref this.syntax, ref other.syntax);
        }

        public void SwapLabelOverride(BuildNode other)
        {
            DataHelper.Swap(ref this.labelOverride, ref other.labelOverride);
        }

        public void SwapVariableModel(BuildNode other)
        {
            DataHelper.Swap(ref this.variableModel, ref other.variableModel);
        }

        public void SwapValueModel(BuildNode other)
        {
            DataHelper.Swap(ref this.valueModel, ref other.valueModel);
        }

        public void SwapBorderData(BuildNode other)
        {
            DataHelper.Swap(ref this.operation, ref other.operation);
        }

        public void SwapDisplayNode(BuildNode other)
        {
            DataHelper.Swap(ref this.displayNode, ref other.displayNode);
        }

        public void SwapContents(BuildNode other)
        {
            this.SwapFlags(other);
            this.SwapOutgoingEdges(other);
            this.SwapSyntax(other);
            this.SwapLabelOverride(other);
            this.SwapVariableModel(other);
            this.SwapValueModel(other);
            this.SwapBorderData(other);
            this.SwapDisplayNode(other);
        }

        public BuildEdge AddEdge(BuildNode to, Expression valueCondition = null)
        {
            var edge = new BuildEdge(to, valueCondition);
            this.OutgoingEdges.Add(edge);

            return edge;
        }

        public void AddEdge(BuildEdge edge)
        {
            this.OutgoingEdges.Add(edge);
        }

        public BuildEdge GetSingleEdge()
        {
            if (this.OutgoingEdges.Count != 1)
            {
                // TODO: Add a message and put to resources
                throw new InvalidOperationException();
            }

            var edge = this.OutgoingEdges.Single();
            Contract.Assert(edge.ValueCondition == null);

            return edge;
        }

        public bool TryGetSingleEdge(out BuildEdge edge)
        {
            if (this.OutgoingEdges.Count == 1)
            {
                edge = this.OutgoingEdges.Single();
                Contract.Assert(edge.ValueCondition == null);

                return true;
            }
            else
            {
                edge = null;

                return false;
            }
        }

        public bool TryGetTwoBooleanEdges(out BuildEdge trueEdge, out BuildEdge falseEdge)
        {
            if (this.OutgoingEdges.Count == 2)
            {
                trueEdge = this.OutgoingEdges.First(edge => edge.ValueCondition == ExpressionFactory.True);
                falseEdge = this.OutgoingEdges.First(edge => edge.ValueCondition == ExpressionFactory.False);

                return (trueEdge != null && falseEdge != null);
            }
            else
            {
                trueEdge = null;
                falseEdge = null;

                return false;
            }
        }

        private void SetVariableModelImpl(ITypeModel value)
        {
            // TODO: Analyze the possibility of overwriting a temporary variable that is being used somewhere else
            Contract.Assert(
                this.variableModel == null
                || Contract.ForAll(
                    value.AssignmentLeft,
                    variable => ((BuildVariable)variable).Origin == VariableOrigin.Temporary));

            this.variableModel = value;
        }

        // TODO: Add proper hashing
    }
}
