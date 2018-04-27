using AskTheCode.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace AskTheCode.Wpf
{

    /// <summary>
    /// Interaction logic for ToolPanel.
    /// </summary>
    public partial class ToolPanel : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolPanel"/> class.
        /// </summary>
        public ToolPanel()
        {
            this.InitializeComponent();
        }

        // TODO: Replace with a behaviour eventually
        private void SelectedMethodFlowChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var toolView = this.DataContext as ToolView;
            if (toolView != null)
            {
                toolView.SelectedPath.SelectedMethodFlow = e.NewValue as MethodFlowView;
            }
        }
    }
}