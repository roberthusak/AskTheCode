using AskTheCode.ControlFlowGraphs.Cli;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AskTheCode.PathExploration.Tests
{
    internal class CSharpProjectExplorationTestDataAttribute : Attribute, ITestDataSource
    {
        private string projectLocation;

        private MSBuildWorkspace workspace;
        private Project project;

        public CSharpProjectExplorationTestDataAttribute(string projectLocation)
        {
            this.projectLocation = projectLocation;
        }

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            this.EnsureWorkspaceAndProject();

            var graphProvider = new CSharpFlowGraphProvider(this.workspace.CurrentSolution);

            foreach (var doc in this.project.Documents)
            {
                var syntaxTree = doc.GetSyntaxTreeAsync().Result;
                var semanticModel = doc.GetSemanticModelAsync().Result;

                var invocationsEnumerable = syntaxTree.GetRoot().DescendantNodes()
                    .Where(n => n.IsKind(SyntaxKind.InvocationExpression));
                foreach (InvocationExpressionSyntax invocation in invocationsEnumerable)
                {
                    if (invocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        string methodName = ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Text;
                        if (IsEvaluationMethodName(methodName))
                        {
                            var methodDeclaration = (BaseMethodDeclarationSyntax)invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>()
                                ?? invocation.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
                            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
                            if (methodSymbol != null)
                            {
                                yield return new object[] { graphProvider, methodSymbol, invocation }; 
                            }
                        }
                    }
                }
            }
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            return $"{((IMethodSymbol)data[1]).ToDisplayString()}: {data[2]}";
        }

        private static bool IsEvaluationMethodName(string methodName)
        {
            switch (methodName)
            {
                case "ValidAssert":
                case "InvalidAssert":
                case "ValidUnreachable":
                case "InvalidUnreachable":
                    return true;

                default:
                    return false;
            }
        }

        private void EnsureWorkspaceAndProject()
        {
            this.workspace = MSBuildWorkspace.Create();
            this.project = this.workspace.OpenProjectAsync(this.projectLocation).Result;
        }
    }
}