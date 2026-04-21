using System.Reflection;

namespace TemplateWpfBlankApp.App.ViewModels;

public sealed class AboutPageViewModel : PageViewModelBase
{
    public AboutPageViewModel()
        : base("About", "Application metadata, deployment details, support ownership, and licensing notes.")
    {
        var assembly = Assembly.GetExecutingAssembly();
        Version = assembly.GetName().Version?.ToString() ?? "1.0.0";
        BuildInformation = $"{assembly.GetName().Name} / .NET 8 WPF shell";
    }

    public string Version { get; }

    public string BuildInformation { get; }

    public string DeploymentChannel => "Internal template channel";

    public string SupportContact => "support@company.local";

    public string LicenseNotice => "MIT license for the template shell. Replace with your internal licensing notice as needed.";

    public string OpenSourceNotice => "Review downstream dependencies when replacing the mock services with SQL, SharePoint, Graph, or Power BI adapters.";
}
