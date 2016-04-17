namespace AskTheCode.Vsix
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
        }

        // TODO: Replace with a behaviour eventually
        private void SelectedTreeNodeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var controlView = this.DataContext as ViewModel.ControlView;
            controlView.SelectedTreeNode = e.NewValue as ViewModel.TreeNodeView;
        }
    }
}