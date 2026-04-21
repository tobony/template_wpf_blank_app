using System.Collections.ObjectModel;
using TemplateWpfBlankApp.App.Models;

namespace TemplateWpfBlankApp.App.Services;

public interface IConnectionService
{
    ObservableCollection<ConnectionDefinition> Connections { get; }

    Task TestConnectionAsync(ConnectionDefinition? connection);

    Task TestAllAsync();

    IReadOnlyList<ConnectionDefinition> CreateSnapshot();

    void ApplySnapshot(IEnumerable<ConnectionDefinition> connections);
}
