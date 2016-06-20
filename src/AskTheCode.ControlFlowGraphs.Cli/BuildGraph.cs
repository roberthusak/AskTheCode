using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuildGraph
    {
        private BuildVariableId.Provider variableIdProvider = new BuildVariableId.Provider();

        public BuildGraph(MethodDeclarationSyntax methodSyntax)
        {
            this.EnterNode = new BuildNode(methodSyntax);
            this.Nodes.Add(this.EnterNode);
        }

        public BuildNode EnterNode { get; private set; }

        public List<BuildNode> Nodes { get; } = new List<BuildNode>();

        public List<BuildVariable> Variables { get; } = new List<BuildVariable>();

        public Dictionary<ISymbol, ITypeModel> DefinedVariableModels { get; } = new Dictionary<ISymbol, ITypeModel>();

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
