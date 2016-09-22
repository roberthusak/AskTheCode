﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.Tests;
using AskTheCode.ViewModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;

namespace StandaloneGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        public MSBuildWorkspace Workspace { get; private set; }

        public Document OpenedDocument { get; private set; }

        public async Task OpenDocument(Document document)
        {
            this.OpenedDocument = document;
            var sourceText = await this.OpenedDocument.GetTextAsync();
            this.code.Text = sourceText.ToString();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Project project = null;
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && File.Exists(args[1]))
            {
                string file = args[1];
                string suffix = System.IO.Path.GetExtension(file);
                if (suffix == ".csproj")
                {
                    this.Workspace = MSBuildWorkspace.Create();
                    project = await this.Workspace.OpenProjectAsync(file);
                }
            }

            var document = project?.Documents.FirstOrDefault();

            if (document == null)
            {
                MessageBox.Show("Please pass the project to open as an argument of the program");
                this.Close();
                return;
            }

            await this.OpenDocument(document);

            this.DataContext = new ToolView(new SimpleIdeServices(this));
        }

        private class SimpleIdeServices : IIdeServices
        {
            private readonly MainWindow window;

            public SimpleIdeServices(MainWindow window)
            {
                this.window = window;
            }

            public Workspace Workspace
            {
                get { return this.window.Workspace; }
            }

            public Document GetOpenedDocument()
            {
                return this.window.OpenedDocument;
            }

            public void HighlightText(SourceText text, IDictionary<HighlightType, IEnumerable<TextSpan>> highlights)
            {
                throw new NotImplementedException();
            }

            public async void OpenDocument(Document document)
            {
                await this.window.OpenDocument(document);
            }

            public void SelectText(SourceText text, TextSpan selectedSpan)
            {
                throw new NotImplementedException();
            }

            public bool TryGetCaretPosition(out Document document, out int position)
            {
                document = this.window.OpenedDocument;
                position = this.window.code.CaretIndex;

                return true;
            }

            public bool TryGetSelectedText(out Document document, out TextSpan selectedSpan)
            {
                throw new NotImplementedException();
            }
        }
    }
}
