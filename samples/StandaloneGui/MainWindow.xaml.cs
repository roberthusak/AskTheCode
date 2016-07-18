using System;
using System.Collections.Generic;
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
using AskTheCode.ControlFlowGraphs.Cli.Tests;
using Microsoft.CodeAnalysis;

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

        public Document OpenedDocument { get; private set; }

        public async Task OpenDocument(Document document)
        {
            this.OpenedDocument = document;
            var sourceText = await this.OpenedDocument.GetTextAsync();
            this.code.Text = sourceText.ToString();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var workspace = SampleCSharpWorkspaceProvider.MethodSampleClass();
            var document = workspace.CurrentSolution.Projects.Single().Documents.Single();
            await this.OpenDocument(document);
        }

        private void DisplayCfg_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}
