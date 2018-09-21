using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.PathExploration;
using AskTheCode.PathExploration.Heuristics;
using AskTheCode.SmtLibStandard.Z3;
using AskTheCode.ViewModel;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceEvaluation
{
    class Program
    {
        private const string InputsProject = @"..\..\..\inputs\csharp\EvaluationTests\EvaluationTests.csproj";
        private const string AttributeName = "PerformanceEvaluationAttribute";

        private const int RepeatCount = 10;

        static void Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var workspace = MSBuildWorkspace.Create();
            var project = workspace.OpenProjectAsync(InputsProject).Result;
            var graphProvider = new CSharpFlowGraphProvider(workspace.CurrentSolution);
            var results = new List<ResultRow[]>();

            foreach (var doc in project.Documents)
            {
                var syntaxTree = doc.GetSyntaxTreeAsync().Result;
                SemanticModel semanticModel = null;

                var attributesEnumerable = syntaxTree.GetRoot().DescendantNodes()
                    .Where(n => n.IsKind(SyntaxKind.Attribute));
                foreach (AttributeSyntax attr in attributesEnumerable)
                {
                    if (semanticModel == null)
                    {
                        semanticModel = doc.GetSemanticModelAsync().Result;
                    }

                    var attrInfo = semanticModel.GetSymbolInfo(attr);
                    if (attrInfo.Symbol?.ContainingType?.Name != AttributeName)
                    {
                        continue;
                    }

                    var programName = semanticModel.GetConstantValue(attr.ArgumentList.Arguments.First().Expression).ToString();

                    var methodDeclaration = (BaseMethodDeclarationSyntax)attr.FirstAncestorOrSelf<MethodDeclarationSyntax>()
                        ?? attr.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
                    var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
                    if (methodSymbol == null)
                    {
                        continue;
                    }

                    var invocationsEnumerable = methodDeclaration.DescendantNodes()
                        .Where(n => n.IsKind(SyntaxKind.InvocationExpression));
                    foreach (InvocationExpressionSyntax invocation in invocationsEnumerable)
                    {
                        if (invocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                        {
                            string methodName = ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Text;
                            if (IsEvaluationMethodName(methodName))
                            {
                                var resultSet = EvaluateProgram(graphProvider, programName, methodSymbol, invocation);
                                results.Add(resultSet);
                            }
                        }
                    }
                }
            }

            using (var writer = new StreamWriter("results.csv"))
            {
                writer.WriteLine(ResultRow.Header);
                foreach (var set in results)
                {
                    foreach (var row in set)
                    {
                        writer.WriteLine(row);
                    }
                }
            }
        }

        private static ResultRow[] EvaluateProgram(
            CSharpFlowGraphProvider graphProvider,
            string programName,
            IMethodSymbol methodSymbol,
            InvocationExpressionSyntax invocation)
        {
            string assertionName = ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier.Text;
            bool isAssertion = assertionName.EndsWith("Assert");
            bool shouldFindPath = assertionName.Contains("Invalid");

            var methodLocation = new MethodLocation(methodSymbol);
            var flowGraph = graphProvider.GetFlowGraphAsync(methodLocation).Result;
            var displayGraph = graphProvider.GetDisplayGraph(flowGraph.Id);

            var displayNode = displayGraph.Nodes.First(n => n.Span.Contains(invocation.Span));
            var displayRecord = displayNode.Records.Last();
            var startInfo = new StartingNodeInfo(displayRecord.FlowNode, displayRecord.OperationIndex, isAssertion);
            var options = new ExplorationOptions()
            {
                FinalNodeRecognizer = new PublicMethodEntryRecognizer(),
                TimeoutSeconds = 40
            };

            var heuristics = GetHeuristicFactories().ToArray();

            var results = new ResultRow[heuristics.Length * RepeatCount];

            int j = 0;
            foreach ((string heuristicName, var heuristicFactory) in heuristics)
            {
                options.SmtHeuristicFactory = heuristicFactory;

                for (int i = 0; i < RepeatCount; i++)
                {
                    var stopwatch = new Stopwatch();
                    long? ticksFirst = null;
                    int? callsFirst = null;

                    var exploration = new ExplorationContext(graphProvider, new ContextFactory(), startInfo, options);
                    exploration.ExecutionModelsObservable
                        .Subscribe(m => { ticksFirst = stopwatch.ElapsedTicks; callsFirst = Explorer.SolverCallCount; });

                    stopwatch.Start();
                    exploration.ExploreAsync().ContinueWith(t => stopwatch.Stop()).Wait();
                    long ticksTotal = stopwatch.ElapsedTicks;

                    results[j * RepeatCount + i] = new ResultRow
                    {
                        Heuristic = heuristicName,
                        Program = programName,
                        FirstCounterexampleTime = ticksFirst / (double)Stopwatch.Frequency,
                        FirstCounterexampleCalls = callsFirst,
                        TotalTime = ticksTotal / (double)Stopwatch.Frequency,
                        TotalCalls = Explorer.SolverCallCount
                    };
                }

                j++;
            }

            return results;
        }

        private static IEnumerable<(string name, IHeuristicFactory<ISmtHeuristic> heuristicFactory)> GetHeuristicFactories()
        {
            //yield return ("EntryPoint", new SimpleHeuristicFactory<EntryPointSmtHeuristic>());
            yield return ("FixedInterval(2)", new FixedIntervalSmtHeuristicFactory(2));
            yield return ("FixedInterval(5)", new FixedIntervalSmtHeuristicFactory(5));
            yield return ("FixedInterval(10)", new FixedIntervalSmtHeuristicFactory(10));
            yield return ("MultipleIngoing", new SimpleHeuristicFactory<MultipleIngoingSmtHeuristic>());
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
    }
}
