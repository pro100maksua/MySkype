using Avalonia;
using Avalonia.Logging.Serilog;
using MySkype.Client.Views;

namespace MySkype.Client
{
    class Program
    {
        static void Main()
        {
            BuildAvaloniaApp().Start<MainWindowView>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
