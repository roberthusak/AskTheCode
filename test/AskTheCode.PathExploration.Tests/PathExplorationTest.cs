using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.SmtLibStandard.Z3;
using AskTheCode.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.PathExploration.Tests
{
    [TestClass]
    public class PathExplorationTest
    {
        [TestMethod]
        [CSharpProjectExplorationTestData(@"..\..\..\inputs\csharp\EvaluationTests\EvaluationTests.csproj")]
        public async Task ExplorationResultIsCorrect(
            CSharpFlowGraphProvider graphProvider,
            IMethodSymbol methodSymbol,
            InvocationExpressionSyntax targetSyntax)
        {
            string assertionName = ((MemberAccessExpressionSyntax)targetSyntax.Expression).Name.Identifier.Text;
            bool isAssertion = assertionName.EndsWith("Assert");
            bool shouldFindPath = assertionName.Contains("Invalid");

            var methodLocation = new MethodLocation(methodSymbol);
            var flowGraph = await graphProvider.GetFlowGraphAsync(methodLocation);
            var displayGraph = graphProvider.GetDisplayGraph(flowGraph.Id);

            var displayNode = displayGraph.Nodes.First(n => n.Span.Contains(targetSyntax.Span));
            var displayRecord = displayNode.Records.Last();
            var startInfo = new StartingNodeInfo(displayRecord.FlowNode, displayRecord.OperationIndex, isAssertion);
            var options = new ExplorationOptions()
            {
                FinalNodeRecognizer = new PublicMethodEntryRecognizer()
            };
            var exploration = new ExplorationContext(graphProvider, new ContextFactory(), startInfo, options);

            // Cancel after the first found path to save time
            bool success = !shouldFindPath;
            var cancelSource = new CancellationTokenSource();
            exploration.ExecutionModelsObservable
                .Subscribe(m => { success = shouldFindPath; cancelSource.Cancel(); }, cancelSource.Token);
            bool wasExhaustive = await exploration.ExploreAsync(cancelSource);

            string message = shouldFindPath ? "No path was found" : "A path was found, although it shouldn't have been";
            if (success && !shouldFindPath && !wasExhaustive)
            {
                success = false;
                message = $"No path was found during the {options.TimeoutSeconds} second timeout";
            }

            Assert.IsTrue(success, message);
        }
    }
}
