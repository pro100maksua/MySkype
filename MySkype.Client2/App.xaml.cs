using Avalonia;
using Avalonia.Markup.Xaml;

namespace MySkype.Client2
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
