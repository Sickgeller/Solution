using Solution.Data.Repositories;

namespace Solution.WinForms;

public partial class Form1 : Form
{
    private readonly IWorkOrderRepository _repo;

    public Form1(IWorkOrderRepository repo)
    {
        _repo = repo;
        InitializeComponent();
        this.Load += async (_, __) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var items = await _repo.GetAllAsync();
        dataGridView1.DataSource = items;
    }
}
