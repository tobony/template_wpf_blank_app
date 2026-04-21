using System.Collections.ObjectModel;
using TemplateWpfBlankApp.App.Infrastructure;

namespace TemplateWpfBlankApp.App.Models;

public sealed class AppProfile : ObservableObject
{
    private string _profileName = "Default";
    private string _environmentName = "Test";
    private string _userAlias = Environment.UserName;
    private string _defaultPage = "Home";
    private string _logLevel = "Information";
    private string _visibleColumns = "Id,Title,SourceSystem,Owner,Status,LastUpdated";
    private bool _useReadOnlyMode;
    private bool _showAdvancedPages;
    private bool _isAdministrator;
    private string _defaultStatusFilter = "All";
    private string _defaultOwnerFilter = "All";
    private int _requestTimeoutSeconds = 30;
    private bool _isFavorite;
    private DateTimeOffset? _lastUsedAt = DateTimeOffset.Now;
    private ObservableCollection<ConnectionDefinition> _connections = new();

    public string ProfileName
    {
        get => _profileName;
        set => SetProperty(ref _profileName, value);
    }

    public string EnvironmentName
    {
        get => _environmentName;
        set => SetProperty(ref _environmentName, value);
    }

    public string UserAlias
    {
        get => _userAlias;
        set => SetProperty(ref _userAlias, value);
    }

    public string DefaultPage
    {
        get => _defaultPage;
        set => SetProperty(ref _defaultPage, value);
    }

    public string LogLevel
    {
        get => _logLevel;
        set => SetProperty(ref _logLevel, value);
    }

    public string VisibleColumns
    {
        get => _visibleColumns;
        set => SetProperty(ref _visibleColumns, value);
    }

    public bool UseReadOnlyMode
    {
        get => _useReadOnlyMode;
        set => SetProperty(ref _useReadOnlyMode, value);
    }

    public bool ShowAdvancedPages
    {
        get => _showAdvancedPages;
        set => SetProperty(ref _showAdvancedPages, value);
    }

    public bool IsAdministrator
    {
        get => _isAdministrator;
        set => SetProperty(ref _isAdministrator, value);
    }

    public string DefaultStatusFilter
    {
        get => _defaultStatusFilter;
        set => SetProperty(ref _defaultStatusFilter, value);
    }

    public string DefaultOwnerFilter
    {
        get => _defaultOwnerFilter;
        set => SetProperty(ref _defaultOwnerFilter, value);
    }

    public int RequestTimeoutSeconds
    {
        get => _requestTimeoutSeconds;
        set => SetProperty(ref _requestTimeoutSeconds, value);
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetProperty(ref _isFavorite, value);
    }

    public DateTimeOffset? LastUsedAt
    {
        get => _lastUsedAt;
        set => SetProperty(ref _lastUsedAt, value);
    }

    public ObservableCollection<ConnectionDefinition> Connections
    {
        get => _connections;
        set => SetProperty(ref _connections, value);
    }

    public AppProfile Clone()
    {
        var clone = new AppProfile
        {
            ProfileName = ProfileName,
            EnvironmentName = EnvironmentName,
            UserAlias = UserAlias,
            DefaultPage = DefaultPage,
            LogLevel = LogLevel,
            VisibleColumns = VisibleColumns,
            UseReadOnlyMode = UseReadOnlyMode,
            ShowAdvancedPages = ShowAdvancedPages,
            IsAdministrator = IsAdministrator,
            DefaultStatusFilter = DefaultStatusFilter,
            DefaultOwnerFilter = DefaultOwnerFilter,
            RequestTimeoutSeconds = RequestTimeoutSeconds,
            IsFavorite = IsFavorite,
            LastUsedAt = LastUsedAt,
        };

        foreach (var connection in Connections)
        {
            clone.Connections.Add(connection.Clone());
        }

        return clone;
    }

    public void CopyFrom(AppProfile other)
    {
        ProfileName = other.ProfileName;
        EnvironmentName = other.EnvironmentName;
        UserAlias = other.UserAlias;
        DefaultPage = other.DefaultPage;
        LogLevel = other.LogLevel;
        VisibleColumns = other.VisibleColumns;
        UseReadOnlyMode = other.UseReadOnlyMode;
        ShowAdvancedPages = other.ShowAdvancedPages;
        IsAdministrator = other.IsAdministrator;
        DefaultStatusFilter = other.DefaultStatusFilter;
        DefaultOwnerFilter = other.DefaultOwnerFilter;
        RequestTimeoutSeconds = other.RequestTimeoutSeconds;
        IsFavorite = other.IsFavorite;
        LastUsedAt = other.LastUsedAt;

        Connections.Clear();
        foreach (var connection in other.Connections)
        {
            Connections.Add(connection.Clone());
        }

        OnPropertyChanged(nameof(Connections));
    }

    public static AppProfile CreateDefault()
    {
        var profile = new AppProfile();
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "Internal SQL",
            SystemType = "SQL Database",
            Endpoint = "Server=sql.company.local;Database=Operations;",
            Authentication = "Integrated SSO",
            AuthenticationStatus = "Connected",
            AuthenticationExpiresAt = DateTimeOffset.Now.AddHours(6),
            Notes = "Template endpoint for transactional updates.",
            IsAdminManaged = true,
        });
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "SharePoint Site",
            SystemType = "SharePoint",
            Endpoint = "https://contoso.sharepoint.com/sites/operations",
            Authentication = "Microsoft 365 SSO",
            AuthenticationStatus = "Token expires soon",
            AuthenticationExpiresAt = DateTimeOffset.Now.AddMinutes(45),
            Notes = "Template endpoint for documents and lists.",
        });
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "Microsoft Graph",
            SystemType = "Microsoft Graph",
            Endpoint = "https://graph.microsoft.com/v1.0/sites/root",
            Authentication = "Microsoft 365 SSO",
            AuthenticationStatus = "Connected",
            AuthenticationExpiresAt = DateTimeOffset.Now.AddHours(2),
            Notes = "Template endpoint for mail, Teams, and directory automation.",
        });
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "Power BI Workspace",
            SystemType = "Power BI",
            Endpoint = "powerbi://api.powerbi.com/v1.0/myorg/Operations",
            Authentication = "Microsoft 365 SSO",
            AuthenticationStatus = "Re-authentication required",
            AuthenticationExpiresAt = null,
            Notes = "Template endpoint for semantic models and reports.",
        });
        return profile;
    }
}
