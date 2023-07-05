using System.Windows;

namespace Demo.Async.SynchronizationContext.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((MainWindowViewModel)DataContext).TextBox = textBox;
        }
    }
}
