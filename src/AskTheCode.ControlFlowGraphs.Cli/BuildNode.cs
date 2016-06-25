using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuildNode
    {
        private List<BuildEdge> outgoingEdges = new List<BuildEdge>();
        private SyntaxNode syntax;
        private SyntaxNodeOrToken? labelOverride;
        private ITypeModel variableModel;
        private ITypeModel valueModel;
        private CallData callData;

        public BuildNode(SyntaxNode syntax)
        {
            this.Syntax = syntax;
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

        public CallData CallData
        {
            get { return this.callData; }
            set { DataHelper.SetOnceAssert(ref this.callData, value); }
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

        public void SwapCallData(BuildNode other)
        {
            DataHelper.Swap(ref this.callData, ref other.callData);
        }

        public void SwapContents(BuildNode other)
        {
            this.SwapOutgoingEdges(other);
            this.SwapSyntax(other);
            this.SwapLabelOverride(other);
            this.SwapVariableModel(other);
            this.SwapValueModel(other);
            this.SwapCallData(other);
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
