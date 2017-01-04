using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
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
using AskTheCode.PathExploration;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Z3;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
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

        private Workspace csharpWorkspace;
        private Document csharpDocument;
        private SemanticModel csharpSemanticModel;
        private MethodDeclarationSyntax[] csharpMethodSyntaxes;
        private TypeModelManager cliModelManager;
        private bool csharpIntermediate = true;
        private GraphDepth csharpGraphDepth;

        private FlowGraph currentFlowGraph;
        private FlowNode currentFlowNode;
        private ObservableCollection<KeyValuePair<string, List<string>>> foundPaths =
            new ObservableCollection<KeyValuePair<string, List<string>>>();

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
            this.aglGraphViewer.MouseDown += this.AglGraphViewer_MouseDown;

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
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && File.Exists(args[1]))
            {
                string file = args[1];
                string suffix = System.IO.Path.GetExtension(file);
                if (suffix == ".cs")
                {
                    this.csharpWorkspace = SampleCSharpWorkspaceProvider.CreateWorkspaceFromSingleFile(file);
                }
                else if (suffix == ".csproj")
                {
                    this.csharpWorkspace = MSBuildWorkspace.Create();
                    var project = await ((MSBuildWorkspace)this.csharpWorkspace).OpenProjectAsync(file);
                }
            }

            if (this.csharpWorkspace == null)
            {
                this.csharpMethodTab.IsEnabled = false;
            }
            else
            {
                this.foundPathsView.ItemsSource = this.foundPaths;
                this.cliModelManager = new TypeModelManager();
                this.csharpGraphDepth = GraphDepth.Statement;

                this.documentSelectionCombo.ItemsSource = this.csharpWorkspace.CurrentSolution.Projects
                    .SelectMany(project => project.Documents)
                    .ToArray();
            }
        }

        private void AglGraphViewer_MouseDown(object sender, MsaglMouseEventArgs e)
        {
            if (this.currentFlowGraph == null)
            {
                return;
            }

            var viewerNode = this.aglGraphViewer.ObjectUnderMouseCursor as IViewerNode;
            if (viewerNode != null)
            {
                viewerNode.MarkedForDragging = true;

                int id = int.Parse(viewerNode.Node.Id);
                this.currentFlowNode = this.currentFlowGraph.Nodes[id];

                this.nodeIdLabel.Content = id.ToString();
                this.exploreButton.IsEnabled = true;

                var assignments = (this.currentFlowNode as InnerFlowNode)?.Assignments;
                if (assignments != null && assignments.Count > 0 && assignments.Last().Variable.Sort == Sort.Bool)
                {
                    this.assertionCheckBox.IsEnabled = true;
                }
                else
                {
                    this.assertionCheckBox.IsEnabled = false;
                }

                foreach (var node in this.aglGraphViewer.Graph.Nodes)
                {
                    node.Attr.LineWidth = 1;
                }

                viewerNode.Node.Attr.LineWidth = 3;
            }
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

            this.ResetCurrentFlowGraphInformation(flowGraph);

            e.Handled = true;
        }

        private async void MethodSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await this.BuildCSharpFlowGraph();
            e.Handled = true;
        }

        private async void DocumentSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var document = e.AddedItems.OfType<Document>().FirstOrDefault();

            if (document != null)
            {
                this.csharpDocument = document;
                var root = await this.csharpDocument.GetSyntaxRootAsync();

                this.csharpMethodSyntaxes = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
                this.csharpSemanticModel = await this.csharpDocument.GetSemanticModelAsync();
                this.methodSelectionCombo.ItemsSource = this.csharpMethodSyntaxes;
            }
        }

        private async void CSharpDepthRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Content.ToString();
            this.csharpGraphDepth = (GraphDepth)Enum.Parse(typeof(GraphDepth), value);

            await this.BuildCSharpFlowGraph();
            e.Handled = true;
        }

        private async void IntermediateCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            this.csharpIntermediate = (this.intermediateCheckBox.IsChecked == true);

            await this.BuildCSharpFlowGraph();
        }

        private async Task BuildCSharpFlowGraph()
        {
            int index = this.methodSelectionCombo.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            var methodSyntax = this.csharpMethodSyntaxes[index];
            var builder = new CSharpGraphBuilder(
                this.cliModelManager,
                this.csharpDocument.Id,
                this.csharpSemanticModel,
                methodSyntax);

            if (this.csharpIntermediate)
            {
                var buildGraph = await builder.BuildAsync(this.csharpGraphDepth);

                var aglGraph = this.csharpGraphConverter.Convert(buildGraph, this.csharpGraphDepth);
                aglGraph.Attr.LayerDirection = LayerDirection.TB;
                this.aglGraphViewer.Graph = aglGraph;

                this.ResetCurrentFlowGraphInformation();
            }
            else
            {
                var buildGraph = await builder.BuildAsync(GraphDepth.Value);

                var flowGraphTranslator = new FlowGraphTranslator(buildGraph, builder.DisplayGraph, new FlowGraphId(0));
                var flowGraph = flowGraphTranslator.Translate().FlowGraph;

                var aglGraph = this.flowGraphConverter.Convert(flowGraph);
                aglGraph.Attr.LayerDirection = LayerDirection.TB;
                this.aglGraphViewer.Graph = aglGraph;

                this.ResetCurrentFlowGraphInformation(flowGraph);
            }
        }

        private void ResetCurrentFlowGraphInformation(FlowGraph flowGraph = null)
        {
            this.currentFlowGraph = flowGraph;
            this.currentFlowNode = null;

            if (this.tabs.SelectedItem != this.csharpMethodTab || this.intermediateCheckBox.IsChecked == false)
            {
                this.propertiesPanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.propertiesPanel.Visibility = Visibility.Collapsed;
            }

            this.graphIdLabel.Content = this.currentFlowGraph?.Id.Value.ToString() ?? string.Empty;
            this.nodeIdLabel.Content = string.Empty;
            this.exploreButton.IsEnabled = false;
        }

        private async void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            this.exploreButton.IsEnabled = false;
            this.exploreProgress.IsIndeterminate = true;
            this.foundPaths.Clear();

            bool isAssertChecked = (this.assertionCheckBox.IsEnabled && this.assertionCheckBox.IsChecked == true);
            int? assignmentIndex = isAssertChecked ?
                ((InnerFlowNode)this.currentFlowNode).Assignments.Count - 1 : (int?)null;
            var startNode = new StartingNodeInfo(this.currentFlowNode, assignmentIndex, isAssertChecked);
            var graphProvider = new DummyFlowGraphProvider();
            var z3Factory = new ContextFactory();
            var options = new ExplorationOptions();

            int timeoutSeconds;
            if (int.TryParse(this.timeoutText.Text, out timeoutSeconds))
            {
                options.TimeoutSeconds = timeoutSeconds;
            }

            var explorationContext = new ExplorationContext(graphProvider, z3Factory, startNode, options);
            explorationContext.ExecutionModels.Subscribe(this.ExecutionModelFound);
            await explorationContext.ExploreAsync();

            this.exploreButton.IsEnabled = true;
            this.exploreProgress.IsIndeterminate = false;
        }

        private async void ExecutionModelFound(ExecutionModel executionModel)
        {
            var modelList = new List<string>();
            for (int i = 0; i < executionModel.NodeInterpretations.Length; i++)
            {
                var node = executionModel.PathNodes[i];
                var interpretations = executionModel.NodeInterpretations[i];
                var innerNode = node as InnerFlowNode;
                if (innerNode != null)
                {
                    this.AddNodeModels(modelList, interpretations, (j) => innerNode.Assignments[j].Variable);
                }
                else if (node is EnterFlowNode)
                {
                    var enterNode = node as EnterFlowNode;
                    this.AddNodeModels(modelList, interpretations, (j) => enterNode.Parameters[j]);
                }
                else if (node is CallFlowNode)
                {
                    var callNode = node as CallFlowNode;
                    this.AddNodeModels(modelList, interpretations, (j) => callNode.ReturnAssignments[j]);
                }
            }

            modelList.Reverse();

            await this.Dispatcher.InvokeAsync(() =>
            {
                string pathName = $"Path {this.foundPaths.Count}";
                var pathData = new KeyValuePair<string, List<string>>(pathName, modelList);
                this.foundPaths.Add(pathData);
            });
        }

        private void AddNodeModels(List<string> modelList, ImmutableArray<Interpretation> interpretations, Func<int, Variable> variableProvider)
        {
            for (int i = 0; i < interpretations.Length; i++)
            {
                int reversedIndex = interpretations.Length - 1 - i;
                string paramName = variableProvider(reversedIndex).DisplayName;
                var interpretation = interpretations[i];
                if (interpretation != null)
                {
                    string line = $"{paramName} = {interpretation.Value}";
                    modelList.Add(line);
                }
            }
        }
    }
}
