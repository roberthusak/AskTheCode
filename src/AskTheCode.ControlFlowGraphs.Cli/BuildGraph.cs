﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Consider making freezable
    internal class BuildGraph
    {
        private BuildNodeId.Provider nodeIdProvider = new BuildNodeId.Provider();
        private BuildVariableId.Provider variableIdProvider = new BuildVariableId.Provider();

        public BuildGraph(DocumentId documentId, BaseMethodDeclarationSyntax methodSyntax)
        {
            this.DocumentId = documentId;
            this.MethodSyntax = methodSyntax;

            this.EnterNode = this.AddNode(methodSyntax);
            this.EnterNode.Operation = new BorderOperation(SpecialOperationKind.Enter, null, null);
        }

        public DocumentId DocumentId { get; private set; }

        public BaseMethodDeclarationSyntax MethodSyntax { get; }

        public BuildNode EnterNode { get; private set; }

        public List<BuildNode> Nodes { get; } = new List<BuildNode>();

        public List<BuildVariable> Variables { get; } = new List<BuildVariable>();

        public Dictionary<ISymbol, ITypeModel> DefinedVariableModels { get; } = new Dictionary<ISymbol, ITypeModel>();

        public ReferenceModel LocalInstanceModel { get; set; }

        public BuildNode AddNode(SyntaxNode syntax)
        {
            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new BuildNode(nodeId, syntax);
            this.Nodes.Add(node);
            Contract.Assert(nodeId.Value == this.Nodes.IndexOf(node));

            return node;
        }

        public BuildVariable AddVariable(Sort sort, ISymbol symbol, VariableOrigin origin)
        {
            var variableId = this.variableIdProvider.GenerateNewId();
            var variable = new BuildVariable(variableId, sort, symbol, origin);
            this.Variables.Add(variable);
            Contract.Assert(variableId.Value == this.Variables.IndexOf(variable));

            return variable;
        }
    }
}
