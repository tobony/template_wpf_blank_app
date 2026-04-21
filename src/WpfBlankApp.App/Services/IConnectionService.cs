using System.Collections.ObjectModel;
using WpfBlankApp.App.Models;

namespace WpfBlankApp.App.Services;

public interface IConnectionService
{
    ObservableCollection<ConnectionDefinition> Connections { get; }

    Task TestConnectionAsync(ConnectionDefinition? connection);

    Task TestAllAsync();

    IReadOnlyList<ConnectionDefinition> CreateSnapshot();

    void ApplySnapshot(IEnumerable<ConnectionDefinition> connections);
}
