using ClockTransactionsTransmiter.Helpers;
using ClockTransactionsTransmiter.Logics;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ClockTransactionsTransmiter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //Application.Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;

            InitializerLogic.Instance.Init($"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClockTransactionsTransmiter.sqlite")};");
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            HandleException(exception);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.SetObserved();
        }

        private void HandleException(Exception ex)
        {
            ExceptionHelper.ShowErrorMessage(ex);
        }
    }
}
