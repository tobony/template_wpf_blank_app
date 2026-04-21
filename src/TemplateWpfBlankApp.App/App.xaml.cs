using System.Windows;
using TemplateWpfBlankApp.App.Models;
using TemplateWpfBlankApp.App.Services;
using TemplateWpfBlankApp.App.ViewModels;

namespace TemplateWpfBlankApp.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var profile = AppProfile.CreateDefault();
        var activityLogService = new ActivityLogService();
        var profileStore = new JsonProfileStore();
        var connectionService = new MockConnectionService(profile.Connections);
        var workspaceService = new WorkspaceService(activityLogService);

        var mainWindowViewModel = new MainWindowViewModel(profile, activityLogService, profileStore, connectionService, workspaceService);
        var mainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        MainWindow = mainWindow;
        mainWindow.Show();

        await mainWindowViewModel.InitializeAsync();
    }
}
