using AdonisUI.Controls;
using ClockTransactionsTransmiter.ViewModels;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace ClockTransactionsTransmiter
{
    /// <summary>
    /// Interaction logic for CoverWindow.xaml
    /// </summary>
    public partial class CoverWindow : AdonisWindow
    {
        private MainViewModel mainViewModel;

        public CoverWindow()
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

        private void SelectBootargsFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.CheckFileExists = true;
            fileDialog.Title = "Select Bootargs File";
            fileDialog.CheckPathExists = true;
            fileDialog.DefaultExt = "*.bootargs";
            fileDialog.Filter = "Bootargs File(*.bootargs)|*.bootargs";
            if (fileDialog.ShowDialog() ?? false)
            {
                mainViewModel.EditingSettings.BootargsFilePath = fileDialog.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectBootargsFile();
        }
    }
}
