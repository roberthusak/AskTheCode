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
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Operations;
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
        private HeapToMsaglGraphConverter heapGraphConverter;
        private GraphViewer aglGraphViewer;
        private GraphViewer aglHeapViewer;

        private TestFlowGraphProvider sampleGraphProvider;

        private Workspace csharpWorkspace;
        private Document csharpDocument;
        private SemanticModel csharpSemanticModel;
        private BaseMethodDeclarationSyntax[] csharpMethodSyntaxes;
        private TypeModelManager cliModelManager;
        private bool csharpIntermediate = true;
        private GraphDepth csharpGraphDepth;

        private FlowGraph currentFlowGraph;
        private FlowNode currentFlowNode;
        private ObservableCollection<KeyValuePair<string, List<string>>> foundPaths =
            new ObservableCollection<KeyValuePair<string, List<string>>>();

        private List<ExecutionModel> foundPathModels = new List<ExecutionModel>();

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.flowGraphConverter = new FlowToMsaglGraphConverter();
            this.csharpGraphConverter = new CSharpBuildToMsaglGraphConverter();
            this.heapGraphConverter = new HeapToMsaglGraphConverter();

            this.aglGraphViewer = new GraphViewer()
            {
                LayoutEditingEnabled = false
            };
            this.aglGraphViewer.BindToPanel(this.graphViewerPanel);
            this.aglGraphViewer.MouseDown += this.AglGraphViewer_MouseDown;

            this.aglHeapViewer = new GraphViewer()
            {
                LayoutEditingEnabled = false
            };
            this.aglHeapViewer.BindToPanel(this.heapViewerPanel);

            // Symbolic CFGs
            this.sampleGraphProvider = new TestFlowGraphProvider(typeof(SampleFlowGraphGenerator));
            this.graphSelectionCombo.ItemsSource = this.sampleGraphProvider.GeneratedMethodLocations;
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

                var operations = (this.currentFlowNode as InnerFlowNode)?.Operations;
                if (operations != null && operations.Count > 0
                    && operations.Last() is Assignment assignment && assignment.Variable.Sort == Sort.Bool)
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

            var location = this.sampleGraphProvider.GeneratedMethodLocations[index];
            var flowGraph = this.sampleGraphProvider.GetFlowGraphAsync(location).Result;

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

                this.csharpMethodSyntaxes = root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>().ToArray();
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
            this.foundPathModels.Clear();

            bool isAssertChecked = (this.assertionCheckBox.IsEnabled && this.assertionCheckBox.IsChecked == true);
            int? assignmentIndex = isAssertChecked ?
                ((InnerFlowNode)this.currentFlowNode).Operations.Count - 1 : (int?)null;
            var startNode = new StartingNodeInfo(this.currentFlowNode, assignmentIndex, isAssertChecked);
            var z3Factory = new ContextFactory();
            var options = new ExplorationOptions();

            int timeoutSeconds;
            if (int.TryParse(this.timeoutText.Text, out timeoutSeconds))
            {
                options.TimeoutSeconds = timeoutSeconds;
            }

            var explorationContext = new ExplorationContext(this.sampleGraphProvider, z3Factory, startNode, options);
            explorationContext.ExecutionModelsObservable.Subscribe(this.ExecutionModelFound);
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
                var heapLocations = executionModel.HeapLocations[i];
                var innerNode = node as InnerFlowNode;
                if (innerNode != null)
                {
                    var assignedVariables = new List<FlowVariable>();
                    foreach (var op in innerNode.Operations)
                    {
                        if (op is Assignment assignment)
                        {
                            assignedVariables.Add(assignment.Variable);
                        }
                        else if (op is FieldRead fieldRead)
                        {
                            assignedVariables.Add(fieldRead.ResultStore);
                        }
                        else if (op is FieldWrite fieldWrite)
                        {
                            assignedVariables.Add(fieldWrite.Reference);
                        }
                    }

                    this.AddNodeModels(modelList, interpretations, heapLocations, assignedVariables);
                }
                else if (node is EnterFlowNode)
                {
                    var enterNode = node as EnterFlowNode;
                    this.AddNodeModels(modelList, interpretations, heapLocations, enterNode.Parameters);
                }
                else if (node is CallFlowNode)
                {
                    // The values are provided only for the second pass of the call node, after returning
                    if (interpretations.Length > 0 || heapLocations.Length > 0)
                    {
                        var callNode = node as CallFlowNode;
                        this.AddNodeModels(modelList, interpretations, heapLocations, callNode.ReturnAssignments);
                    }
                }
            }

            await this.Dispatcher.InvokeAsync(() =>
            {
                string pathName = $"Path {this.foundPaths.Count}";
                var pathData = new KeyValuePair<string, List<string>>(pathName, modelList);
                this.foundPaths.Add(pathData);
                this.foundPathModels.Add(executionModel);
            });
        }

        private void AddNodeModels(
            List<string> modelList,
            ImmutableArray<Interpretation> interpretations,
            ImmutableArray<HeapModelLocation> heapLocations,
            IReadOnlyList<FlowVariable> variables)
        {
            int intrIndex = 0;
            int locIndex = 0;
            foreach (var variable in variables)
            {
                string value = null;

                if (variable.IsReference)
                {
                    var location = heapLocations[locIndex];
                    value = location.ToString();

                    locIndex++;
                }
                else
                {
                    var interpretation = interpretations[intrIndex];
                    value = interpretation?.Value.ToString();

                    intrIndex++;
                }

                string line = $"{variable.DisplayName} = {value ?? "<any>"}";
                modelList.Add(line);
            }
        }

        private void FoundPathsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.foundPathsView.SelectedIndex < 0)
            {
                this.UpdateHeapModel(null, 0);
                return;
            }

            var executionModel = this.foundPathModels[this.foundPathsView.SelectedIndex];
            var heapModel = executionModel.HeapModel;

            this.heapSlider.IsEnabled = true;
            this.heapSlider.Minimum = 0;
            this.heapSlider.Maximum = heapModel.MaxVersion;

            int prevValue = (int)this.heapSlider.Value;
            this.heapSlider.Value = 0;

            // Display the first version by default
            if (prevValue == 0)
            {
                this.UpdateHeapModel(heapModel, 0);
            }
        }

        private void HeapSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var executionModel = this.foundPathModels[this.foundPathsView.SelectedIndex];

            this.UpdateHeapModel(executionModel.HeapModel, (int)e.NewValue);
        }

        private void UpdateHeapModel(IHeapModel heapModel, int version)
        {
            if (heapModel == null)
            {
                this.heapSlider.IsEnabled = false;
                this.aglHeapViewer.Graph = new Graph();
            }
            else
            {
                var aglGraph = this.heapGraphConverter.Convert(heapModel, version);
                aglGraph.Attr.LayerDirection = LayerDirection.LR;
                this.aglHeapViewer.Graph = aglGraph;
            }
        }
    }
}
