using ClockTransactionsTransmiter.ViewModels;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClockTransactionsTransmiter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = new MainViewModel();
            this.DataContext = mainViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainViewModel.ReGenCts();

            Task.Run(async () =>
            {
                await mainViewModel.HandleEmployees();
            }, mainViewModel.cts_get_results.Token);
            Task.Run(async () =>
            {
                await mainViewModel.SyncEmployees();
            }, mainViewModel.cts_get_results.Token);

            Task.Run(async () =>
            {
                await mainViewModel.HandleRecords();
            }, mainViewModel.cts_get_results.Token);
            Task.Run(async () =>
            {
                await mainViewModel.SyncRecords();
            }, mainViewModel.cts_get_results.Token);

            Task.Run(async () =>
            {
                await mainViewModel.GetResults();
            }, mainViewModel.cts_get_results.Token);

            Task.Run(async () =>
            {
                await mainViewModel.Start();
            }, mainViewModel.cts_get_results.Token);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            mainViewModel.Stop();
        }
    }
}