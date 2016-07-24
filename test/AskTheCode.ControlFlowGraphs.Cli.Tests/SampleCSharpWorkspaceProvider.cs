using System;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Cli.Tests
{
    public static class SampleCSharpWorkspaceProvider
    {
        public static Workspace CreateWorkspaceFromSingleFile(string filepath)
        {
            var workspace = new AdhocWorkspace();

            string projName = "MethodSampleProject";
            var projectId = ProjectId.CreateNewId();
            var versionStamp = VersionStamp.Create();
            var projectInfo = ProjectInfo.Create(projectId, versionStamp, projName, projName, LanguageNames.CSharp);
            var newProject = workspace.AddProject(projectInfo);

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var system = MetadataReference.CreateFromFile(typeof(Debug).Assembly.Location);
            newProject = newProject.AddMetadataReference(mscorlib);
            newProject = newProject.AddMetadataReference(system);
            workspace.TryApplyChanges(newProject.Solution);

            string sourceContents = File.ReadAllText(filepath);
            workspace.AddDocument(newProject.Id, Path.GetFileName(filepath), SourceText.From(sourceContents));

            return workspace;
        }
    }
}
