using WpfBlankApp.App.Infrastructure;

namespace WpfBlankApp.App.Models;

public sealed class WorkItem : ObservableObject
{
    private int _id;
    private string _title = string.Empty;
    private string _sourceSystem = string.Empty;
    private string _owner = string.Empty;
    private string _status = string.Empty;
    private string _notes = string.Empty;
    private DateTimeOffset _lastUpdated;
    private bool _isReadOnly;
    private bool _isActive = true;
    private string _lastSyncResult = "Not synced yet";
    private DateTimeOffset? _serverLastSyncedAt;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string SourceSystem
    {
        get => _sourceSystem;
        set => SetProperty(ref _sourceSystem, value);
    }

    public string Owner
    {
        get => _owner;
        set => SetProperty(ref _owner, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public DateTimeOffset LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string LastSyncResult
    {
        get => _lastSyncResult;
        set => SetProperty(ref _lastSyncResult, value);
    }

    public DateTimeOffset? ServerLastSyncedAt
    {
        get => _serverLastSyncedAt;
        set => SetProperty(ref _serverLastSyncedAt, value);
    }

    public WorkItem Clone()
    {
        return new WorkItem
        {
            Id = Id,
            Title = Title,
            SourceSystem = SourceSystem,
            Owner = Owner,
            Status = Status,
            Notes = Notes,
            LastUpdated = LastUpdated,
            IsReadOnly = IsReadOnly,
            IsActive = IsActive,
            LastSyncResult = LastSyncResult,
            ServerLastSyncedAt = ServerLastSyncedAt,
        };
    }

    public void CopyFrom(WorkItem other)
    {
        Id = other.Id;
        Title = other.Title;
        SourceSystem = other.SourceSystem;
        Owner = other.Owner;
        Status = other.Status;
        Notes = other.Notes;
        LastUpdated = other.LastUpdated;
        IsReadOnly = other.IsReadOnly;
        IsActive = other.IsActive;
        LastSyncResult = other.LastSyncResult;
        ServerLastSyncedAt = other.ServerLastSyncedAt;
    }
}
