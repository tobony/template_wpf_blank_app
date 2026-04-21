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
            Notes = "Template endpoint for transactional updates.",
        });
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "SharePoint Site",
            SystemType = "SharePoint",
            Endpoint = "https://contoso.sharepoint.com/sites/operations",
            Authentication = "Microsoft 365 SSO",
            Notes = "Template endpoint for documents and lists.",
        });
        profile.Connections.Add(new ConnectionDefinition
        {
            Name = "Power BI Workspace",
            SystemType = "Power BI",
            Endpoint = "powerbi://api.powerbi.com/v1.0/myorg/Operations",
            Authentication = "Microsoft 365 SSO",
            Notes = "Template endpoint for semantic models and reports.",
        });
        return profile;
    }
}
