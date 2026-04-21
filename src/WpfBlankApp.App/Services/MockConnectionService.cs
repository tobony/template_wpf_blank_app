using System.Collections.ObjectModel;
using WpfBlankApp.App.Models;

namespace WpfBlankApp.App.Services;

public sealed class MockConnectionService : IConnectionService
{
    public MockConnectionService(IEnumerable<ConnectionDefinition> initialConnections)
    {
        ApplySnapshot(initialConnections);
    }

    public ObservableCollection<ConnectionDefinition> Connections { get; } = new();

    public async Task TestConnectionAsync(ConnectionDefinition? connection)
    {
        if (connection is null)
        {
            return;
        }

        connection.Status = ConnectionState.Unknown;
        connection.Notes = "Testing connection...";
        await Task.Delay(300);

        if (string.IsNullOrWhiteSpace(connection.Endpoint))
        {
            connection.Status = ConnectionState.Error;
            connection.Notes = "Endpoint is required before testing.";
            connection.AuthenticationStatus = "Missing endpoint";
        }
        else if (connection.Endpoint.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                 connection.Endpoint.Contains("sandbox", StringComparison.OrdinalIgnoreCase))
        {
            connection.Status = ConnectionState.Attention;
            connection.Notes = "Connected, but the endpoint appears to be non-production.";
            connection.AuthenticationStatus = "Connected to non-production";
            connection.AuthenticationExpiresAt = DateTimeOffset.Now.AddMinutes(30);
        }
        else
        {
            connection.Status = ConnectionState.Connected;
            connection.Notes = "Connection test completed successfully with the current SSO template settings.";
            connection.AuthenticationStatus = "Connected";
            connection.AuthenticationExpiresAt = DateTimeOffset.Now.AddHours(4);
        }

        connection.LastChecked = DateTimeOffset.Now;
    }

    public async Task TestAllAsync()
    {
        foreach (var connection in Connections)
        {
            await TestConnectionAsync(connection);
        }
    }

    public IReadOnlyList<ConnectionDefinition> CreateSnapshot()
    {
        return Connections.Select(connection => connection.Clone()).ToArray();
    }

    public void ApplySnapshot(IEnumerable<ConnectionDefinition> connections)
    {
        Connections.Clear();
        foreach (var connection in connections)
        {
            var clone = connection.Clone();
            clone.Status = ConnectionState.Unknown;
            clone.LastChecked = null;
            if (string.IsNullOrWhiteSpace(clone.Notes))
            {
                clone.Notes = "Not tested yet.";
            }

            if (string.IsNullOrWhiteSpace(clone.AuthenticationStatus))
            {
                clone.AuthenticationStatus = "Not authenticated";
            }

            Connections.Add(clone);
        }
    }
}
