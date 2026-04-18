using System.Windows;

namespace Analysis.KioUI.Views;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            var fadeIn = (System.Windows.Media.Animation.Storyboard)Resources["FadeIn"];
            fadeIn?.Begin();
        };
    }

    public void UpdateStatus(string status)
    {
        Dispatcher.Invoke(() =>
        {
            DataContext = new { StatusText = status };
        });
    }
}
