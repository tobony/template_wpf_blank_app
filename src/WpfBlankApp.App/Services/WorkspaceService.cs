using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using WpfBlankApp.App.Infrastructure;
using WpfBlankApp.App.Models;

namespace WpfBlankApp.App.Services;

public sealed class WorkspaceService : ObservableObject
{
    private readonly IActivityLogService _activityLogService;
    private WorkItem? _selectedItem;
    private WorkItem? _editableItem;
    private string _validationMessage = string.Empty;
    private bool _isCreatingNewItem;
    private bool _deleteConfirmationRequested;
    private string _bulkImportFilePath = "sample-bulk-update.csv";
    private string _bulkMappingSummary = "Owner → Owner, Status → Status, Notes → Notes";
    private bool _bulkExecutionConfirmationRequested;
    private string _bulkExecutionMessage = "Preview a file before execution.";
    private DateTimeOffset? _lastServerRefreshAt;
    private string _syncStatusMessage = "No sync has been run yet.";

    public WorkspaceService(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
        SeedItems();
        SelectedItem = Items.FirstOrDefault();
    }

    public ObservableCollection<WorkItem> Items { get; } = new();

    public ObservableCollection<BulkUpdatePreviewRow> BulkPreviewRows { get; } = new();

    public ObservableCollection<SyncConflictRecord> SyncConflicts { get; } = new();

    public ObservableCollection<SyncConflictRecord> FailedSyncQueue { get; } = new();

    public WorkItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                DeleteConfirmationRequested = false;
                ValidationMessage = string.Empty;
                EditableItem = value?.Clone();
                IsCreatingNewItem = false;
            }
        }
    }

    public WorkItem? EditableItem
    {
        get => _editableItem;
        private set => SetProperty(ref _editableItem, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public bool IsCreatingNewItem
    {
        get => _isCreatingNewItem;
        private set => SetProperty(ref _isCreatingNewItem, value);
    }

    public bool DeleteConfirmationRequested
    {
        get => _deleteConfirmationRequested;
        private set => SetProperty(ref _deleteConfirmationRequested, value);
    }

    public string BulkImportFilePath
    {
        get => _bulkImportFilePath;
        set => SetProperty(ref _bulkImportFilePath, value);
    }

    public string BulkMappingSummary
    {
        get => _bulkMappingSummary;
        private set => SetProperty(ref _bulkMappingSummary, value);
    }

    public int BulkValidCount => BulkPreviewRows.Count(row => row.ValidationStatus == "Valid");

    public int BulkWarningCount => BulkPreviewRows.Count(row => row.ValidationStatus == "Warning");

    public int BulkErrorCount => BulkPreviewRows.Count(row => row.ValidationStatus == "Error");

    public bool BulkExecutionConfirmationRequested
    {
        get => _bulkExecutionConfirmationRequested;
        private set => SetProperty(ref _bulkExecutionConfirmationRequested, value);
    }

    public string BulkExecutionMessage
    {
        get => _bulkExecutionMessage;
        private set => SetProperty(ref _bulkExecutionMessage, value);
    }

    public DateTimeOffset? LastServerRefreshAt
    {
        get => _lastServerRefreshAt;
        private set => SetProperty(ref _lastServerRefreshAt, value);
    }

    public string SyncStatusMessage
    {
        get => _syncStatusMessage;
        private set => SetProperty(ref _syncStatusMessage, value);
    }

    public void UseSampleImportFile()
    {
        BulkImportFilePath = "sample-bulk-update.csv";
        _activityLogService.Add("Bulk Update", "Selected the sample CSV import file.", BulkImportFilePath);
    }

    public Task RefreshAsync()
    {
        var refreshTime = DateTimeOffset.Now;
        foreach (var item in Items)
        {
            item.LastUpdated = refreshTime;
            item.LastSyncResult = "Server refresh completed";
        }

        LastServerRefreshAt = refreshTime;
        SyncStatusMessage = $"Server refresh completed at {refreshTime:yyyy-MM-dd HH:mm:ss}.";
        _activityLogService.Add("Data", "Refreshed the shared work item list.", $"Rows available: {Items.Count}");
        return Task.CompletedTask;
    }

    public async Task<string> ExportDataAsync()
    {
        var exportFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WpfBlankApp",
            "exports");
        Directory.CreateDirectory(exportFolder);

        var path = Path.Combine(exportFolder, $"data-view-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.csv");
        var builder = new StringBuilder();
        builder.AppendLine("Id,Title,SourceSystem,Owner,Status,LastUpdated,IsReadOnly,IsActive");

        foreach (var item in Items)
        {
            builder.AppendLine(string.Join(",",
                item.Id.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.Title),
                EscapeCsv(item.SourceSystem),
                EscapeCsv(item.Owner),
                EscapeCsv(item.Status),
                item.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                item.IsReadOnly,
                item.IsActive));
        }

        await File.WriteAllTextAsync(path, builder.ToString());
        _activityLogService.Add("Data", "Exported the current data view.", path);
        return path;
    }

    public void BeginCreateNew()
    {
        DeleteConfirmationRequested = false;
        ValidationMessage = string.Empty;
        IsCreatingNewItem = true;
        EditableItem = new WorkItem
        {
            Id = Items.Any() ? Items.Max(item => item.Id) + 1 : 1001,
            Title = string.Empty,
            SourceSystem = "SQL Database",
            Owner = string.Empty,
            Status = "Draft",
            Notes = string.Empty,
            LastUpdated = DateTimeOffset.Now,
            IsActive = true,
            LastSyncResult = "Local draft created",
        };
        _activityLogService.Add("Edit", "Started a new draft item.", $"Draft ID: {EditableItem.Id}");
    }

    public Task SaveAsync()
    {
        if (EditableItem is null)
        {
            return Task.CompletedTask;
        }

        var validationErrors = ValidateEditableItem(EditableItem).ToArray();
        if (validationErrors.Length > 0)
        {
            ValidationMessage = string.Join(" ", validationErrors);
            _activityLogService.Add("Edit", "Validation blocked save.", ValidationMessage, "Warning");
            return Task.CompletedTask;
        }

        if (!IsCreatingNewItem && SelectedItem?.IsReadOnly == true)
        {
            ValidationMessage = "Read-only rows cannot be edited until administrative approval is granted.";
            _activityLogService.Add("Edit", "Blocked save for a read-only row.", ValidationMessage, "Warning");
            return Task.CompletedTask;
        }

        EditableItem.LastUpdated = DateTimeOffset.Now;
        EditableItem.LastSyncResult = IsCreatingNewItem ? "Created locally" : "Edited locally";

        if (IsCreatingNewItem)
        {
            Items.Insert(0, EditableItem.Clone());
            SelectedItem = Items.First();
            ValidationMessage = "Saved a new record.";
            _activityLogService.Add("Edit", $"Created '{SelectedItem.Title}'.", $"Owner: {SelectedItem.Owner} / Status: {SelectedItem.Status}");
            return Task.CompletedTask;
        }

        if (SelectedItem is null)
        {
            return Task.CompletedTask;
        }

        SelectedItem.CopyFrom(EditableItem);
        ValidationMessage = "Saved the current record.";
        _activityLogService.Add("Edit", $"Saved '{SelectedItem.Title}'.", $"Owner: {SelectedItem.Owner} / Status: {SelectedItem.Status}");
        return Task.CompletedTask;
    }

    public void ResetEdit()
    {
        DeleteConfirmationRequested = false;
        ValidationMessage = string.Empty;
        EditableItem = IsCreatingNewItem ? new WorkItem
        {
            Id = Items.Any() ? Items.Max(item => item.Id) + 1 : 1001,
            SourceSystem = "SQL Database",
            Status = "Draft",
            LastUpdated = DateTimeOffset.Now,
            IsActive = true,
            LastSyncResult = "Local draft reset",
        } : SelectedItem?.Clone();
        _activityLogService.Add("Edit", "Reset the edit form to the current snapshot.");
    }

    public Task DeleteOrDeactivateAsync()
    {
        if (EditableItem is null)
        {
            return Task.CompletedTask;
        }

        if (!DeleteConfirmationRequested)
        {
            DeleteConfirmationRequested = true;
            ValidationMessage = IsCreatingNewItem
                ? "Click Delete / Deactivate again to discard the new draft."
                : "Click Delete / Deactivate again to mark the selected record inactive.";
            _activityLogService.Add("Edit", "Requested delete/deactivate confirmation.", ValidationMessage, "Warning");
            return Task.CompletedTask;
        }

        DeleteConfirmationRequested = false;

        if (IsCreatingNewItem)
        {
            EditableItem = SelectedItem?.Clone();
            IsCreatingNewItem = false;
            ValidationMessage = "Discarded the new draft.";
            _activityLogService.Add("Edit", "Discarded the new draft item.");
            return Task.CompletedTask;
        }

        if (SelectedItem is null || SelectedItem.IsReadOnly)
        {
            ValidationMessage = "Read-only rows require administrative approval before deactivation.";
            _activityLogService.Add("Edit", "Blocked deactivation for a read-only row.", ValidationMessage, "Warning");
            return Task.CompletedTask;
        }

        SelectedItem.IsActive = false;
        SelectedItem.Status = "Inactive";
        SelectedItem.LastUpdated = DateTimeOffset.Now;
        SelectedItem.LastSyncResult = "Marked inactive locally";
        EditableItem = SelectedItem.Clone();
        ValidationMessage = "The selected record was marked inactive.";
        _activityLogService.Add("Edit", $"Marked '{SelectedItem.Title}' inactive.");
        return Task.CompletedTask;
    }

    public Task GenerateBulkPreviewAsync()
    {
        BulkPreviewRows.Clear();

        var previewRows = new[]
        {
            CreatePreviewRow(1, Items[0], "Update owner", Items[0].Owner, "Operations Excellence", "Valid", false, "Mapped using the active CSV profile."),
            CreatePreviewRow(2, Items[1], "Refresh status", Items[1].Status, "Queued for refresh", "Valid", false, "Ready for execution."),
            CreatePreviewRow(3, Items[2], "Deactivate row", "Active", "Inactive", "Warning", true, "Read-only row requires approval before commit."),
            new BulkUpdatePreviewRow
            {
                RowNumber = 4,
                ItemTitle = "New external row",
                ProposedAction = "Insert into SharePoint list",
                OriginalValue = "Unmapped target",
                UpdatedValue = "Operations Requests",
                ValidationStatus = "Error",
                CanRetry = true,
                Notes = "Required site/list target is not mapped in the active profile.",
            },
        };

        foreach (var row in previewRows)
        {
            BulkPreviewRows.Add(row);
        }

        BulkExecutionConfirmationRequested = false;
        BulkExecutionMessage = "Preview ready. Review warnings and errors before execution.";
        BulkMappingSummary = "CSV columns mapped: Title → Item, Owner → Owner, Status → Status, Active → IsActive";
        OnPropertyChanged(nameof(BulkValidCount));
        OnPropertyChanged(nameof(BulkWarningCount));
        OnPropertyChanged(nameof(BulkErrorCount));
        _activityLogService.Add("Bulk Update", "Generated a bulk update preview.", $"Rows previewed: {BulkPreviewRows.Count}");
        return Task.CompletedTask;
    }

    public Task ExecuteBulkUpdateAsync()
    {
        if (!BulkPreviewRows.Any())
        {
            BulkExecutionMessage = "Generate a preview before execution.";
            _activityLogService.Add("Bulk Update", "Attempted execution without a preview.", BulkExecutionMessage, "Warning");
            return Task.CompletedTask;
        }

        if (!BulkExecutionConfirmationRequested)
        {
            BulkExecutionConfirmationRequested = true;
            BulkExecutionMessage = "Click Execute again to confirm the staged bulk update.";
            _activityLogService.Add("Bulk Update", "Requested execution confirmation.", BulkExecutionMessage, "Warning");
            return Task.CompletedTask;
        }

        var succeeded = BulkPreviewRows.Count(row => row.ValidationStatus is "Valid" or "Warning");
        BulkExecutionConfirmationRequested = false;
        BulkExecutionMessage = $"Executed {succeeded} staged rows. {BulkErrorCount} rows remain in the retry queue.";
        _activityLogService.Add("Bulk Update", "Executed staged bulk rows.", BulkExecutionMessage, BulkErrorCount > 0 ? "Warning" : "Information");
        return Task.CompletedTask;
    }

    public Task RetryFailedBulkUpdatesAsync()
    {
        if (BulkErrorCount == 0)
        {
            BulkExecutionMessage = "No failed rows are waiting for retry.";
            return Task.CompletedTask;
        }

        var retriedRows = BulkPreviewRows
            .Select(row => row.ValidationStatus == "Error"
                ? new BulkUpdatePreviewRow
                {
                    RowNumber = row.RowNumber,
                    ItemTitle = row.ItemTitle,
                    ProposedAction = row.ProposedAction,
                    OriginalValue = row.OriginalValue,
                    UpdatedValue = row.UpdatedValue,
                    ValidationStatus = "Warning",
                    CanRetry = false,
                    Notes = "Retry queued with fallback mapping. Administrative review is still recommended.",
                }
                : row)
            .ToArray();

        BulkPreviewRows.Clear();
        foreach (var row in retriedRows)
        {
            BulkPreviewRows.Add(row);
        }

        BulkExecutionMessage = "Moved failed rows into a retry-ready warning state.";
        OnPropertyChanged(nameof(BulkValidCount));
        OnPropertyChanged(nameof(BulkWarningCount));
        OnPropertyChanged(nameof(BulkErrorCount));
        _activityLogService.Add("Bulk Update", "Queued failed rows for retry.", BulkExecutionMessage, "Warning");
        return Task.CompletedTask;
    }

    public Task ReloadFromServerAsync()
    {
        var now = DateTimeOffset.Now;
        LastServerRefreshAt = now;
        SyncConflicts.Clear();
        SyncConflicts.Add(new SyncConflictRecord
        {
            WorkItemId = 1002,
            ItemTitle = "Power BI usage snapshot",
            LocalValue = "Status = Queued for refresh",
            RemoteValue = "Status = Synced",
            ResolutionHint = "Review the remote refresh before overwriting the status.",
            CanRetry = true,
        });
        SyncStatusMessage = $"Server refresh completed at {now:yyyy-MM-dd HH:mm:ss}. 1 conflict detected.";
        _activityLogService.Add("Sync", "Reloaded the latest remote snapshot.", SyncStatusMessage);
        return Task.CompletedTask;
    }

    public Task PushLocalChangesAsync()
    {
        FailedSyncQueue.Clear();
        FailedSyncQueue.Add(new SyncConflictRecord
        {
            WorkItemId = 1003,
            ItemTitle = "Vendor onboarding audit",
            LocalValue = "Owner = Procurement",
            RemoteValue = "Read-only approval required",
            ResolutionHint = "Retry after an administrator approves the change.",
            CanRetry = true,
        });

        foreach (var item in Items.Where(item => item.IsActive))
        {
            item.ServerLastSyncedAt = DateTimeOffset.Now;
            item.LastSyncResult = item.IsReadOnly ? "Sync skipped - approval required" : "Local changes pushed";
        }

        SyncStatusMessage = $"Pushed local changes. {FailedSyncQueue.Count} items remain in the retry queue.";
        _activityLogService.Add("Sync", "Applied local changes to the remote template queue.", SyncStatusMessage, FailedSyncQueue.Count > 0 ? "Warning" : "Information");
        return Task.CompletedTask;
    }

    public Task RetryFailedSyncAsync()
    {
        if (FailedSyncQueue.Count == 0)
        {
            SyncStatusMessage = "No failed sync items are waiting for retry.";
            return Task.CompletedTask;
        }

        FailedSyncQueue.Clear();
        SyncStatusMessage = "Retried the failed sync queue. All pending items are ready for the next push.";
        _activityLogService.Add("Sync", "Retried failed sync items.", SyncStatusMessage);
        return Task.CompletedTask;
    }

    private static BulkUpdatePreviewRow CreatePreviewRow(int rowNumber, WorkItem item, string action, string originalValue, string updatedValue, string validationStatus, bool canRetry, string notes)
    {
        return new BulkUpdatePreviewRow
        {
            RowNumber = rowNumber,
            ItemTitle = item.Title,
            ProposedAction = action,
            OriginalValue = originalValue,
            UpdatedValue = updatedValue,
            ValidationStatus = validationStatus,
            CanRetry = canRetry,
            Notes = notes,
        };
    }

    private static IEnumerable<string> ValidateEditableItem(WorkItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title))
        {
            yield return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(item.SourceSystem))
        {
            yield return "Source system is required.";
        }

        if (string.IsNullOrWhiteSpace(item.Owner))
        {
            yield return "Owner is required.";
        }

        if (string.IsNullOrWhiteSpace(item.Status))
        {
            yield return "Status is required.";
        }
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
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
            LastSyncResult = "Synced from SharePoint",
            ServerLastSyncedAt = DateTimeOffset.Now.AddMinutes(-20),
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
            LastSyncResult = "Awaiting refresh",
            ServerLastSyncedAt = DateTimeOffset.Now.AddMinutes(-40),
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
            LastSyncResult = "Approval required",
            ServerLastSyncedAt = DateTimeOffset.Now.AddHours(-1),
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
            LastSyncResult = "Local changes pushed",
            ServerLastSyncedAt = DateTimeOffset.Now.AddMinutes(-5),
        });
    }
}
