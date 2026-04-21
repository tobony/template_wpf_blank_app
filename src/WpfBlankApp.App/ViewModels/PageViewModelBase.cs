namespace WpfBlankApp.App.ViewModels;

public abstract class PageViewModelBase : ViewModelBase
{
    protected PageViewModelBase(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
