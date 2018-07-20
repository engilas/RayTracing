using System.Windows;

namespace RayTracing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
	    private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
	        _viewModel = new MainViewModel();
	        this.DataContext = _viewModel;
        }
    }
}
