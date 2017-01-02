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
using AskTheCode.ViewModel;
using Microsoft.Msagl.WpfGraphControl;

namespace AskTheCode.Vsix
{
    /// <summary>
    /// Interaction logic for MsaglGraphPresenter.xaml
    /// </summary>
    public partial class MsaglGraphPresenter : UserControl
    {
        public static readonly DependencyProperty GraphViewerProperty = DependencyProperty.Register(
            "GraphViewer",
            typeof(GraphViewer),
            typeof(MsaglGraphPresenter));

        public static readonly DependencyProperty GraphViewerConsumerProperty = DependencyProperty.Register(
            "GraphViewerConsumer",
            typeof(IGraphViewerConsumer),
            typeof(MsaglGraphPresenter),
            new PropertyMetadata(GraphViewerConsumerChanged));

        private bool isInitialized = false;

        public MsaglGraphPresenter()
        {
            this.InitializeComponent();

            this.GraphViewer = new GraphViewer()
            {
                LayoutEditingEnabled = false
            };
        }

        public GraphViewer GraphViewer
        {
            get { return (GraphViewer)this.GetValue(GraphViewerProperty); }
            private set { this.SetValue(GraphViewerProperty, value); }
        }

        public IGraphViewerConsumer GraphViewerConsumer
        {
            get { return (IGraphViewerConsumer)this.GetValue(GraphViewerConsumerProperty); }
            set { this.SetValue(GraphViewerConsumerProperty, value); }
        }

        private static void GraphViewerConsumerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as MsaglGraphPresenter;
            if (self != null)
            {
                self.Dispatcher.InvokeAsync(() =>
                {
                    if (e.OldValue != null)
                    {
                        ((IGraphViewerConsumer)e.OldValue).GraphViewer = null;
                    }

                    if (e.NewValue != null)
                    {
                        ((IGraphViewerConsumer)e.NewValue).GraphViewer = self.GraphViewer;
                    }
                });
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.isInitialized)
            {
                this.GraphViewer.BindToPanel(this.graphViewerPanel);
                this.isInitialized = true;
            }
        }
    }
}
