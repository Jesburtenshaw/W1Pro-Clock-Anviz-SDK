using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClockTransactionsTransmiter.Helpers
{
    public static class ExceptionHelper
    {
        public static string GetEndMessage(this Exception ex)
        {
            return null == ex.InnerException ? ex.Message : ex.InnerException.Message;
        }

        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show($"Error Occured! {Environment.NewLine}{Environment.NewLine} {message}", 
                "Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
        }

        public static void ShowErrorMessage(Exception ex, string desc = "Error Occured", string caption = "Error")
        {
            MessageBox.Show($"{desc} {Environment.NewLine}{Environment.NewLine} {ex.GetEndMessage()}",
                caption,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}