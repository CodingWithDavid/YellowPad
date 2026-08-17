using System.Windows;

namespace YellowPad;

public partial class MainWindow : Window
{
    public MainWindow(IServiceProvider services)
    {
        Resources.Add("services", services);
        InitializeComponent();
    }
}
