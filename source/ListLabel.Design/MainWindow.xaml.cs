using Microsoft.Extensions.DependencyInjection;

namespace ListLabel.Design;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IServiceProvider serviceProvider = Setup.CreateServiceProvider();
    public MainWindowViewModel ViewModel { get; set; }
    public MainWindow()
    {
        ViewModel = serviceProvider.GetService<MainWindowViewModel>()!;
        InitializeComponent();
    }
}