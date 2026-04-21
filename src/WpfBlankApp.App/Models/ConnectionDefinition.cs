using WpfBlankApp.App.Infrastructure;

namespace WpfBlankApp.App.Models;

public sealed class ConnectionDefinition : ObservableObject
{
    private string _name = string.Empty;
    private string _systemType = string.Empty;
    private string _endpoint = string.Empty;
    private string _authentication = string.Empty;
    private string _authenticationStatus = "Not authenticated";
    private DateTimeOffset? _authenticationExpiresAt;
    private ConnectionState _status;
    private DateTimeOffset? _lastChecked;
    private string _notes = string.Empty;
    private bool _isAdminManaged;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string SystemType
    {
        get => _systemType;
        set => SetProperty(ref _systemType, value);
    }

    public string Endpoint
    {
        get => _endpoint;
        set => SetProperty(ref _endpoint, value);
    }

    public string Authentication
    {
        get => _authentication;
        set => SetProperty(ref _authentication, value);
    }

    public string AuthenticationStatus
    {
        get => _authenticationStatus;
        set => SetProperty(ref _authenticationStatus, value);
    }

    public DateTimeOffset? AuthenticationExpiresAt
    {
        get => _authenticationExpiresAt;
        set => SetProperty(ref _authenticationExpiresAt, value);
    }

    public ConnectionState Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public DateTimeOffset? LastChecked
    {
        get => _lastChecked;
        set => SetProperty(ref _lastChecked, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool IsAdminManaged
    {
        get => _isAdminManaged;
        set => SetProperty(ref _isAdminManaged, value);
    }

    public ConnectionDefinition Clone()
    {
        return new ConnectionDefinition
        {
            Name = Name,
            SystemType = SystemType,
            Endpoint = Endpoint,
            Authentication = Authentication,
            AuthenticationStatus = AuthenticationStatus,
            AuthenticationExpiresAt = AuthenticationExpiresAt,
            Status = Status,
            LastChecked = LastChecked,
            Notes = Notes,
            IsAdminManaged = IsAdminManaged,
        };
    }
}
