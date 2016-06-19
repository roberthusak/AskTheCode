using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Cli.Tests
{
    public static class SampleCSharpWorkspaceProvider
    {
        public static Workspace MethodSampleClass()
        {
            var workspace = new AdhocWorkspace();

            string projName = "MethodSampleProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            newProject = newProject.AddMetadataReference(mscorlib);
            workspace.TryApplyChanges(newProject.Solution);

            string sourceContents = File.ReadAllText("inputs\\MethodSampleClass.cs");
            workspace.AddDocument(newProject.Id, "MethodSampleClass.cs", SourceText.From(sourceContents));

            return workspace;
        }
    }
}
