using System.Collections.ObjectModel;
using TemplateWpfBlankApp.App.Infrastructure;
using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public sealed class WorkspaceService : ObservableObject
{
    private readonly IActivityLogService _activityLogService;
    private WorkItem? _selectedItem;
    private WorkItem? _editableItem;

    public WorkspaceService(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
        SeedItems();
        SelectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<WorkItem> Items { get; } = new();

    public ObservableCollection<BulkUpdatePreviewRow> BulkPreviewRows { get; } = new();

    public WorkItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                EditableItem = value?.Clone();
            }
        }
    }

    public WorkItem? EditableItem
    {
        get => _editableItem;
        private set => SetProperty(ref _editableItem, value);
    }

    public Task RefreshAsync()
    {
        foreach (var item in Items)
        {
            item.LastUpdated = DateTimeOffset.Now;
        }

        _activityLogService.Add("Data", "Refreshed the shared work item list.", $"Rows available: {Items.Count}");
        return Task.CompletedTask;
    }

    public Task SaveAsync()
    {
        if (SelectedItem is null || EditableItem is null)
        {
            return Task.CompletedTask;
        }

        EditableItem.LastUpdated = DateTimeOffset.Now;
        SelectedItem.CopyFrom(EditableItem);
        _activityLogService.Add("Edit", $"Saved '{SelectedItem.Title}'.", $"Owner: {SelectedItem.Owner} / Status: {SelectedItem.Status}");
        return Task.CompletedTask;
    }

    public void ResetEdit()
    {
        EditableItem = SelectedItem?.Clone();
        _activityLogService.Add("Edit", "Reset the edit form to the selected item snapshot.");
    }

    public Task GenerateBulkPreviewAsync()
    {
        BulkPreviewRows.Clear();

        var rowNumber = 1;
        foreach (var item in Items.Take(4))
        {
            BulkPreviewRows.Add(new BulkUpdatePreviewRow
            {
                RowNumber = rowNumber++,
                ItemTitle = item.Title,
                ProposedAction = $"Update owner to {item.Owner} and refresh status",
                ValidationStatus = item.IsReadOnly ? "Warning" : "Valid",
                Notes = item.IsReadOnly ? "Read-only row requires approval." : "Ready for preview and execution.",
            });
        }

        BulkPreviewRows.Add(new BulkUpdatePreviewRow
        {
            RowNumber = rowNumber,
            ItemTitle = "New external row",
            ProposedAction = "Insert into SharePoint list",
            ValidationStatus = "Error",
            Notes = "Required site/list target is not mapped in the active profile.",
        });

        _activityLogService.Add("Bulk Update", "Generated a bulk update preview.", $"Rows previewed: {BulkPreviewRows.Count}");
        return Task.CompletedTask;
    }

    private void SeedItems()
    {
        Items.Clear();
        Items.Add(new WorkItem
        {
            Id = 1001,
            Title = "Quarterly policy review",
            SourceSystem = "SharePoint List",
            Owner = "Operations Team",
            Status = "Ready",
            Notes = "Template row for list-based editing and sync workflows.",
            LastUpdated = DateTimeOffset.Now.AddMinutes(-20),
        });
        Items.Add(new WorkItem
        {
            Id = 1002,
            Title = "Power BI usage snapshot",
            SourceSystem = "Power BI Dataset",
            Owner = "Analytics Team",
            Status = "Draft",
            Notes = "Template row for semantic model refresh and annotation.",
            LastUpdated = DateTimeOffset.Now.AddMinutes(-15),
        });
        Items.Add(new WorkItem
        {
            Id = 1003,
            Title = "Vendor onboarding audit",
            SourceSystem = "SQL Database",
            Owner = "Procurement",
            Status = "Pending Approval",
            Notes = "Template row for DB-backed forms and approval metadata.",
            LastUpdated = DateTimeOffset.Now.AddMinutes(-7),
            IsReadOnly = true,
        });
        Items.Add(new WorkItem
        {
            Id = 1004,
            Title = "Graph-driven access request",
            SourceSystem = "Microsoft Graph",
            Owner = "IT Admin",
            Status = "Synced",
            Notes = "Template row for directory or mail-connected automation.",
            LastUpdated = DateTimeOffset.Now.AddMinutes(-3),
        });
    }
}
