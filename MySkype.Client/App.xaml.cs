using Avalonia;
using Avalonia.Markup.Xaml;

namespace MySkype.Client
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}