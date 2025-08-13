using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solution.Data.Repositories;
using Solution.Domain.Entities;

namespace Solution.ViewModels;

public partial class WorkOrdersViewModel : ObservableObject
{
    private readonly IWorkOrderRepository repository;

    [ObservableProperty] private ObservableCollection<WorkOrder> items = new();
    [ObservableProperty] private WorkOrder? selected;
    [ObservableProperty] private WorkOrder editModel = new() { DueDate = DateTime.Today, Status = WorkOrderStatus.Planned };

    // Filters & paging
    [ObservableProperty] private string itemCodeFilter = string.Empty;
    [ObservableProperty] private WorkOrderStatus? statusFilter;
    [ObservableProperty] private DateTime? dueFrom = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? dueTo = DateTime.Today.AddDays(7);
    [ObservableProperty] private int page = 1;
    [ObservableProperty] private int totalPages = 1;
    [ObservableProperty] private int pageSize = 100;
    [ObservableProperty] private int totalCount = 0;

    public WorkOrdersViewModel(IWorkOrderRepository repository)
    {
        this.repository = repository;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await LoadPageAsync();
    }

    [RelayCommand]
    public async Task LoadPageAsync()
    {
        var (items, total) = await repository.GetPageAsync(
            string.IsNullOrWhiteSpace(ItemCodeFilter) ? null : ItemCodeFilter,
            StatusFilter,
            DueFrom,
            DueTo,
            (Page - 1) * PageSize,
            PageSize);
        Items = new ObservableCollection<WorkOrder>(items);
        TotalCount = total;
        TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        if (Page > TotalPages) Page = TotalPages;
    }

    [RelayCommand]
    public void New()
    {
        EditModel = new WorkOrder { DueDate = DateTime.Today, Status = WorkOrderStatus.Planned, Quantity = 1 };
    }

    [RelayCommand]
    public void Edit()
    {
        if (Selected is null) return;
        EditModel = new WorkOrder
        {
            Id = Selected.Id,
            ItemCode = Selected.ItemCode,
            Quantity = Selected.Quantity,
            DueDate = Selected.DueDate,
            Status = Selected.Status
        };
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EditModel.ItemCode)) return;

        var exists = Items.Any(x => x.Id == EditModel.Id);
        if (exists) await repository.UpdateAsync(EditModel);
        else await repository.AddAsync(EditModel);

        await LoadAsync();
        New();
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        if (Selected is null) return;
        await repository.DeleteAsync(Selected.Id);
        await LoadAsync();
    }
}