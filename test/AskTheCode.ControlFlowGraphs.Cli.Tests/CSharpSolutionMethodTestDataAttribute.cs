using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Cli.Tests
{
    public class CSharpSolutionMethodTestDataAttribute : Attribute, ITestDataSource
    {
        private readonly string projectLocation;

        private MSBuildWorkspace workspace;
        private Project project;

        public CSharpSolutionMethodTestDataAttribute(string projectLocation)
        {
            this.projectLocation = projectLocation;
        }

        public string ClassNameFilter { get; set; }

        public string MethodNameFilter { get; set; }

        /// <summary>
        /// Recursively retrieve all the methods from the loaded project.
        /// </summary>
        /// <param name="methodInfo">The test method the data are generated for.</param>
        /// <returns>Enumeration of two-item arrays consisting of an instance of <see cref="Solution"/>
        /// and an instance of <see cref="MethodLocation"/>.</returns>
        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
        {
            this.EnsureWorkspaceAndProject();

            var compilation = this.project.GetCompilationAsync().Result;

            var namespaceQueue = new Queue<INamespaceSymbol>();
            var typeQueue = new Queue<ITypeSymbol>();

            namespaceQueue.Enqueue(compilation.Assembly.GlobalNamespace);

            while (namespaceQueue.Count > 0)
            {
                var ns = namespaceQueue.Dequeue();

                foreach (var nestedNs in ns.GetNamespaceMembers())
                {
                    namespaceQueue.Enqueue(nestedNs);
                }

                foreach (var type in ns.GetTypeMembers())
                {
                    typeQueue.Enqueue(type);
                }
            }

            while (typeQueue.Count > 0)
            {
                var type = typeQueue.Dequeue();

                foreach (var nestedType in type.GetTypeMembers())
                {
                    typeQueue.Enqueue(nestedType);
                }

                if (this.ClassNameFilter == null || type.Name == this.ClassNameFilter)
                {
                    foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (this.MethodNameFilter == null || method.Name == this.MethodNameFilter)
                        {
                            var location = new MethodLocation(method);
                            yield return new object[] { this.workspace.CurrentSolution, location };
                        }
                    }
                }
            }
        }

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            return ((MethodLocation)data[1]).Method.ToDisplayString();
        }

        private void EnsureWorkspaceAndProject()
        {
            this.workspace = MSBuildWorkspace.Create();
            this.project = this.workspace.OpenProjectAsync(this.projectLocation).Result;
        }
    }
}
