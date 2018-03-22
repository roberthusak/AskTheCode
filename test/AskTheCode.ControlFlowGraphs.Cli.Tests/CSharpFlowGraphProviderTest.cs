using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Cli.Tests
{
    [TestClass]
    public class CSharpFlowGraphProviderTest
    {
        private const string InputsBaseDir = @"..\..\..\inputs\csharp\";

        [TestMethod]
        [CSharpSolutionMethodTestData(InputsBaseDir + @"Sample\Sample.csproj")]
        public void SampleProjectMethodsFlowGraphCreationDoesntThrow(Solution solution, MethodLocation method)
        {
            ConstructControlFlowGraph(solution, method);
        }

        [TestMethod]
        [CSharpSolutionMethodTestData(InputsBaseDir + @"EvaluationTests\EvaluationTests.csproj")]
        public void EvaluationProjectMethodsFlowGraphCreationDoesntThrow(Solution solution, MethodLocation method)
        {
            ConstructControlFlowGraph(solution, method);
        }

        private static void ConstructControlFlowGraph(Solution solution, MethodLocation method)
        {
            var flowGraphProvider = new CSharpFlowGraphProvider(solution);
            var result = flowGraphProvider.GetFlowGraphAsync(method).Result;

            Assert.AreNotEqual(null, result);
        }
    }
}
