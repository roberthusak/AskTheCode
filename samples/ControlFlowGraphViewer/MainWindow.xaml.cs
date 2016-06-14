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
using AskTheCode.ControlFlowGraphs.Tests;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace ControlFlowGraphViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FlowToMsaglGraphConverter graphConverter;
        private GraphViewer aglGraphViewer;
        private MethodInfo[] constructionMethods;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
            this.graphSelectionCombo.SelectionChanged += this.GraphSelectionCombo_SelectionChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.graphConverter = new FlowToMsaglGraphConverter();

            this.aglGraphViewer = new GraphViewer()
            {
                LayoutEditingEnabled = false
            };
            this.aglGraphViewer.BindToPanel(this.graphViewerPanel);

            this.constructionMethods =
                typeof(SampleFlowGraphGenerator)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(
                    methodInfo => methodInfo.ReturnType == typeof(FlowGraph)
                    && !methodInfo.ContainsGenericParameters
                    && methodInfo.GetParameters().Length == 0)
                .ToArray();
            this.graphSelectionCombo.ItemsSource = this.constructionMethods;
            this.graphSelectionCombo.SelectedIndex = 0;
        }

        private void GraphSelectionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = this.graphSelectionCombo.SelectedIndex;
            if (index == -1)
            {
                return;
            }

            var methodInfo = this.constructionMethods[index];
            var flowGraph = (FlowGraph)methodInfo.Invoke(null, null);

            var aglGraph = this.graphConverter.Convert(flowGraph);
            aglGraph.Attr.LayerDirection = LayerDirection.TB;

            this.aglGraphViewer.Graph = aglGraph;
        }
    }
}
