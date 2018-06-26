using MySkype.WpfClient.ViewModels;

namespace MySkype.WpfClient.Views
{
    public partial class MainWindowView
    {
        public MainWindowView()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();
        }
    }
}