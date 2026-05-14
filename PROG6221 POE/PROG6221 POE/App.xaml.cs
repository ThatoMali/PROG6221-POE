using System.Windows;

namespace PROG6221_POE
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle unhandled exceptions
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Error: {args.Exception.Message}",
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true;
            };
        }
    }
}