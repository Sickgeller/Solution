using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Solution.Data.Repositories;
using Solution.Domain.Entities;

namespace Solution.WinForms;

public class WorkOrdersBoardForm : Form
{
    private readonly IWorkOrderRepository _repo;
    private TextBox txtItem;
    private ComboBox cmbStatus;
    private DateTimePicker dtFrom;
    private CheckBox chkFrom;
    private DateTimePicker dtTo;
    private CheckBox chkTo;
    private Button btnSearch, btnAdd, btnEdit, btnDelete, btnSave, btnCancel;
    private ComboBox cmbPageSize;
    private Button btnPrev, btnNext;
    private Label lblPageInfo, lblSumInfo;
    private SplitContainer split;
    private DataGridView grid;
    private GroupBox grpEdit;
    private TextBox txtItemEdit;
    private NumericUpDown numQtyEdit;
    private DateTimePicker dtDueEdit;
    private ComboBox cmbStatusEdit;

    private WorkOrder? _selected;
    private bool _isNew;

    public WorkOrdersBoardForm(IWorkOrderRepository repo)
    {
        _repo = repo;
        InitializeUi();
        this.Load += async (_, __) => await LoadAsync();
    }

    private void InitializeUi()
    {
        this.Text = "WorkOrders (Board)";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Width = 1024;
        this.Height = 640;
        this.MinimumSize = new Size(1000, 640);
        this.AutoScaleMode = AutoScaleMode.Dpi;

        var top = new Panel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(8)
        };

        // 필터 및 버튼 컨트롤 생성
        txtItem = new TextBox { Left = 8, Top = 14, Width = 140, PlaceholderText = "품목" };
        cmbStatus = new ComboBox { Left = 156, Top = 14, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
        var statusItems = new List<object> { "전체" };
        statusItems.AddRange(Enum.GetValues(typeof(WorkOrderStatus)).Cast<object>());
        cmbStatus.DataSource = statusItems;

        chkFrom = new CheckBox { Left = 282, Top = 20, Width = 16 };
        dtFrom = new DateTimePicker { Left = 302, Top = 14, Width = 110, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };
        chkTo = new CheckBox { Left = 418, Top = 20, Width = 16 };
        dtTo = new DateTimePicker { Left = 438, Top = 14, Width = 110, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };

        btnSearch = new Button { Left = 558, Top = 12, Width = 70, Text = "검색" };
        btnAdd = new Button { Left = 634, Top = 12, Width = 70, Text = "신규" };
        btnEdit = new Button { Left = 710, Top = 12, Width = 70, Text = "편집" };
        btnDelete = new Button { Left = 786, Top = 12, Width = 70, Text = "삭제" };
        cmbPageSize = new ComboBox { Left = 862, Top = 12, Width = 70, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbPageSize.Items.AddRange(new object[] { 50, 100, 200 });
        cmbPageSize.SelectedIndex = 1; // 100
        btnPrev = new Button { Left = 940, Top = 12, Width = 30, Text = "<" };
        btnNext = new Button { Left = 974, Top = 12, Width = 30, Text = ">" };
        lblPageInfo = new Label { Left = 1010, Top = 18, AutoSize = true, Text = "0/0" };

        btnSearch.Click += async (_, __) => await LoadAsync();
        btnAdd.Click += (_, __) => BeginNew();
        btnEdit.Click += (_, __) => BeginEdit();
        btnDelete.Click += async (_, __) => await DeleteAsync();

        top.Controls.AddRange(new Control[] { txtItem, cmbStatus, chkFrom, dtFrom, chkTo, dtTo, btnSearch, btnAdd, btnEdit, btnDelete, cmbPageSize, btnPrev, btnNext, lblPageInfo });

        split = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 650 };
        split.Margin = new Padding(0, 6, 0, 0); // 상단 툴바와 내용 사이 여백

        grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            MultiSelect = false,
            AutoGenerateColumns = false, // 수동 컬럼 생성
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            ColumnHeadersVisible = true,
            RowHeadersVisible = false,
            AllowUserToAddRows = false
        };

        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkOrder.Id), HeaderText = "ID", ReadOnly = true });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkOrder.ItemCode), HeaderText = "품목코드" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkOrder.Quantity), HeaderText = "수량" });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(WorkOrder.DueDate),
            HeaderText = "납기",
            DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(WorkOrder.Status), HeaderText = "상태" });

        split.Panel1.Controls.Add(grid);

        split.Panel2.Padding = new Padding(12);
        grpEdit = new GroupBox { Dock = DockStyle.Fill, Text = "편집", Padding = new Padding(16) };

        txtItemEdit = new TextBox { Left = 16, Top = 64, Width = 200, PlaceholderText = "품목코드" };
        numQtyEdit = new NumericUpDown { Left = 16, Top = 116, Width = 200, Minimum = 1, Maximum = 1000000, Value = 1 };
        dtDueEdit = new DateTimePicker { Left = 16, Top = 168, Width = 200, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd" };
        cmbStatusEdit = new ComboBox { Left = 16, Top = 220, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbStatusEdit.DataSource = Enum.GetValues(typeof(WorkOrderStatus));

        btnSave = new Button { Left = 16, Top = 272, Width = 86, Text = "저장" };
        btnCancel = new Button { Left = 108, Top = 272, Width = 86, Text = "취소" };
        btnSave.Click += async (_, __) => await SaveAsync();
        btnCancel.Click += (_, __) => ClearEdit();

        grpEdit.Controls.AddRange(new Control[] {
            new Label{ Left=16, Top=44, Text="품목"},
            txtItemEdit,
            new Label{ Left=16, Top=96, Text="수량"},
            numQtyEdit,
            new Label{ Left=16, Top=148, Text="납기"},
            dtDueEdit,
            new Label{ Left=16, Top=200, Text="상태"},
            cmbStatusEdit,
            btnSave,
            btnCancel
        });
        split.Panel2.Controls.Add(grpEdit);

        // footer sum label (once)
        lblSumInfo = new Label { Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleRight };
        split.Panel1.Controls.Add(lblSumInfo);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 처음엔 AutoSize
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(split, 0, 1);
        this.Controls.Add(layout);

        int preferredHeight = top.GetPreferredSize(Size.Empty).Height;
        layout.RowStyles[0] = new RowStyle(SizeType.Absolute, preferredHeight + 6);

        grid.SelectionChanged += (_, __) => SyncSelection();
        btnPrev.Click += async (_, __) => { if (_page > 1) { _page--; await LoadPageAsync(); } };
        btnNext.Click += async (_, __) => { if (_page < _totalPages) { _page++; await LoadPageAsync(); } };
        cmbPageSize.SelectedIndexChanged += async (_, __) => { _page = 1; await LoadPageAsync(); };

    }

    private bool Filter(WorkOrder x)
    {
        if (!string.IsNullOrWhiteSpace(txtItem.Text) && !(x.ItemCode?.Contains(txtItem.Text, StringComparison.OrdinalIgnoreCase) ?? false)) return false;
        if (cmbStatus.SelectedItem is WorkOrderStatus st && x.Status != st) return false;
        if (chkFrom.Checked && x.DueDate.Date < dtFrom.Value.Date) return false;
        if (chkTo.Checked && x.DueDate.Date > dtTo.Value.Date) return false;
        return true;
    }

    private int _page = 1;
    private int _totalPages = 1;
    private int PageSize => (int)(cmbPageSize.SelectedItem ?? 100);

    private async Task LoadAsync() => await LoadPageAsync();

    private async Task LoadPageAsync()
    {
        var (items, total) = await _repo.GetPageAsync(
            string.IsNullOrWhiteSpace(txtItem.Text) ? null : txtItem.Text,
            cmbStatus.SelectedItem is WorkOrderStatus st ? st : null,
            dtFrom.Value.Date,
            dtTo.Value.Date,
            (_page - 1) * PageSize,
            PageSize);
        var filtered = items.ToList();

        // 안전하게 기존 바인딩 해제
        var bs = new BindingSource();
        bs.DataSource = new BindingList<WorkOrder>(filtered);
        grid.DataSource = null;
        grid.DataSource = bs;

        _totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        lblPageInfo.Text = $"{_page}/{_totalPages}  (총 {total:N0}건)";
        // Sum strictly from the rows currently displayed in the grid to avoid any mismatch
        var pageSum = 0;
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem is WorkOrder w)
                pageSum += w.Quantity;
        }
        lblSumInfo.Text = $"건수(현재 페이지): {grid.Rows.Count:N0}";

        if (grid.Rows.Count > 0)
        {
            // 첫 행을 선택(헤더가 없던 상황에서 CurrentRow가 잘못될 수 있으니 안전하게 설정)
            grid.CurrentCell = grid.Rows[0].Cells[0];
            grid.Rows[0].Selected = true;
        }

        SyncSelection();
    }

    private void SyncSelection()
    {
        if (grid.CurrentRow?.DataBoundItem is WorkOrder w)
        {
            _selected = w;
        }
    }

    private void BeginNew()
    {
        _isNew = true;
        _selected = new WorkOrder { Id = Guid.Empty, DueDate = DateTime.Today, Status = WorkOrderStatus.Planned, Quantity = 1 };
        txtItemEdit.Text = _selected.ItemCode;
        numQtyEdit.Value = _selected.Quantity <= 0 ? 1 : _selected.Quantity;
        dtDueEdit.Value = _selected.DueDate;
        cmbStatusEdit.SelectedItem = _selected.Status;
    }

    private void BeginEdit()
    {
        if (_selected is null) return;
        _isNew = false;
        txtItemEdit.Text = _selected.ItemCode;
        numQtyEdit.Value = _selected.Quantity <= 0 ? 1 : _selected.Quantity;
        dtDueEdit.Value = _selected.DueDate;
        cmbStatusEdit.SelectedItem = _selected.Status;
    }

    private void ClearEdit()
    {
        txtItemEdit.Text = string.Empty;
        numQtyEdit.Value = 1;
        dtDueEdit.Value = DateTime.Today;
        cmbStatusEdit.SelectedItem = WorkOrderStatus.Planned;
    }

    private async Task SaveAsync()
    {
        if (_selected is null) return;
        _selected.ItemCode = txtItemEdit.Text.Trim();
        _selected.Quantity = (int)numQtyEdit.Value;
        _selected.DueDate = dtDueEdit.Value.Date;
        _selected.Status = (WorkOrderStatus)cmbStatusEdit.SelectedItem!;

        if (_isNew)
            await _repo.AddAsync(_selected);
        else
            await _repo.UpdateAsync(_selected);

        await LoadAsync();
        ClearEdit();
    }

    private async Task DeleteAsync()
    {
        if (_selected is null) return;
        await _repo.DeleteAsync(_selected.Id);
        await LoadAsync();
    }
}
