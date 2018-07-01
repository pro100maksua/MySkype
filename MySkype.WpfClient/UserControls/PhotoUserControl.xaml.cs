using System.Windows;
using System.Windows.Media;

namespace MySkype.WpfClient.UserControls
{
    public partial class PhotoUserControl
    {
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(nameof(Radius), typeof(double),
                typeof(PhotoUserControl), new PropertyMetadata(null));

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource),
                typeof(PhotoUserControl), new PropertyMetadata(null));

        public PhotoUserControl()
        {
            InitializeComponent();
        }
    }
}
