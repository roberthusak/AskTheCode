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
using Microsoft.Msagl.WpfGraphControl;

namespace StandaloneGui
{
    /// <summary>
    /// Interaction logic for MsaglGraphPresenter.xaml
    /// </summary>
    public partial class MsaglGraphPresenter : UserControl
    {
        public static readonly DependencyProperty GraphViewerProperty = DependencyProperty.Register(
                "GraphViewer",
                typeof(GraphViewer),
                typeof(MsaglGraphPresenter),
                new PropertyMetadata(new GraphViewer()));

        public MsaglGraphPresenter()
        {
            this.InitializeComponent();

            this.GraphViewer.LayoutEditingEnabled = false;
            this.GraphViewer.BindToPanel(this.graphViewerPanel);
        }

        public GraphViewer GraphViewer
        {
            get { return (GraphViewer)this.GetValue(GraphViewerProperty); }
        }
    }
}
