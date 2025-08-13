using System.Windows;
using Solution.ViewModels;

namespace Solution;

public partial class MainWindow : Window
{
    public MainWindow(WorkOrdersViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, __) => await vm.LoadPageCommand.ExecuteAsync(null);
    }
}