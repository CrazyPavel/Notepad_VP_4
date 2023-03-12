using Avalonia.Controls;
using Avalonia.Interactivity;
using NotePad.ViewModels;

namespace NotePad.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
        private void DoubleTap(object sender, RoutedEventArgs e)
        {
            var tap = (MainWindowViewModel?)DataContext;
            if (tap == null) return;

            var src = e.Source;
            if (src == null) return;

            var name = src.GetType().Name;
            if (name == "Image" || name == "ContentPresenter" || name == "TextBlock") tap.DoubleTap();
        }
    }
}