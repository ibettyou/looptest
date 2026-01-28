using System;
using System.Windows;

namespace Loopback
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            ShowErrorDialog("Fatal Error", ex?.Message ?? "Unknown error", ex?.StackTrace);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowErrorDialog("Error", e.Exception.Message, e.Exception.StackTrace);
            e.Handled = true;
        }

        private void ShowErrorDialog(string title, string message, string stackTrace)
        {
            MessageBox.Show($"{message}\n\nStack Trace:\n{stackTrace}", title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
