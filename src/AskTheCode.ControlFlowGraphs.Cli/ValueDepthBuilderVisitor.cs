using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class ValueDepthBuilderVisitor : ExpressionDepthBuilderVisitor
    {
        public ValueDepthBuilderVisitor(IBuildingContext context)
            : base(context)
        {
        }
    }
}
