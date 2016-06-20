using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.Tests;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.ControlFlowGraphs.Tests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace ControlFlowGraphViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FlowToMsaglGraphConverter flowGraphConverter;
        private CSharpBuildToMsaglGraphConverter csharpGraphConverter;
        private GraphViewer aglGraphViewer;

        private MethodInfo[] flowGeneratorMethods;

        private SemanticModel csharpSemanticModel;
        private MethodDeclarationSyntax[] csharpMethodSyntaxes;
        private TypeModelManager cliModelManager;
        private GraphDepth csharpGraphDepth;

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.flowGraphConverter = new FlowToMsaglGraphConverter();
            this.csharpGraphConverter = new CSharpBuildToMsaglGraphConverter();

            this.aglGraphViewer = new GraphViewer()
            {
                LayoutEditingEnabled = false
            };
            this.aglGraphViewer.BindToPanel(this.graphViewerPanel);

            // Symbolic CFGs
            this.flowGeneratorMethods =
                typeof(SampleFlowGraphGenerator)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(
                    methodInfo => methodInfo.ReturnType == typeof(FlowGraph)
                    && !methodInfo.ContainsGenericParameters
                    && methodInfo.GetParameters().Length == 0)
                .ToArray();
            this.graphSelectionCombo.ItemsSource = this.flowGeneratorMethods;
            this.graphSelectionCombo.SelectedIndex = 0;

            // C# CFGs built from the syntax trees of the sample methods
            var workspace = SampleCSharpWorkspaceProvider.MethodSampleClass();
            var document = workspace.CurrentSolution.Projects.Single().Documents.Single();
            var root = await document.GetSyntaxRootAsync();

            this.csharpMethodSyntaxes = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
            this.methodSelectionCombo.ItemsSource = this.csharpMethodSyntaxes;
            this.csharpSemanticModel = await document.GetSemanticModelAsync();
            this.cliModelManager = new TypeModelManager();
            this.csharpGraphDepth = GraphDepth.Statement;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo;
            if (this.tabs.SelectedItem == this.flowGraphTab)
            {
                combo = this.graphSelectionCombo;
            }
            else if (this.tabs.SelectedItem == this.csharpMethodTab)
            {
                combo = this.methodSelectionCombo;
            }
            else
            {
                return;
            }

            // Select the first value if nothing is selected or reselect the current value
            int previousIndex = combo.SelectedIndex;
            if (previousIndex == -1)
            {
                combo.SelectedIndex = 0;
            }
            else
            {
                combo.SelectedIndex = -1;
                combo.SelectedIndex = previousIndex;
            }
        }

        private void GraphSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = this.graphSelectionCombo.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            var methodInfo = this.flowGeneratorMethods[index];
            var flowGraph = (FlowGraph)methodInfo.Invoke(null, null);

            var aglGraph = this.flowGraphConverter.Convert(flowGraph);
            aglGraph.Attr.LayerDirection = LayerDirection.TB;
            this.aglGraphViewer.Graph = aglGraph;

            e.Handled = true;
        }

        private async void MethodSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await this.BuildCSharpFlowGraph();
            e.Handled = true;
        }

        private async void CSharpDepthRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Content.ToString();
            this.csharpGraphDepth = (GraphDepth)Enum.Parse(typeof(GraphDepth), value);

            await this.BuildCSharpFlowGraph();
            e.Handled = true;
        }

        private async Task BuildCSharpFlowGraph()
        {
            int index = this.methodSelectionCombo.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            var methodSyntax = this.csharpMethodSyntaxes[index];
            var builder = new CSharpFlowGraphBuilder(this.cliModelManager, this.csharpSemanticModel, methodSyntax);
            await builder.BuildAsync(this.csharpGraphDepth);

            var aglGraph = this.csharpGraphConverter.Convert(builder.Graph);
            aglGraph.Attr.LayerDirection = LayerDirection.TB;
            this.aglGraphViewer.Graph = aglGraph;
        }
    }
}
