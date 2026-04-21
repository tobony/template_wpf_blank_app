using System.Reflection;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class AboutPageViewModel : PageViewModelBase
{
    public AboutPageViewModel()
        : base("About", "Application metadata, support ownership, and extension notes.")
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    public string Version { get; }

    public string SupportContact => "support@company.local";

    public string DeploymentModel => "Template desktop shell for line-of-business internal tools";
}
