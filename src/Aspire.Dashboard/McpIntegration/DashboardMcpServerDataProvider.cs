// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using Aspire.Dashboard.Mcp.Providers;
#pragma warning disable IDE0005 // Using directive is necessary
using Aspire.Dashboard.Model;
using Aspire.Dashboard.ServiceClient;
#pragma warning restore IDE0005

namespace Aspire.Dashboard.McpIntegration;

/// <summary>
/// Dashboard implementation of the MCP server data provider.
/// Provides access to various Dashboard data for MCP resources.
/// </summary>
public class DashboardMcpServerDataProvider : IMcpServerDataProvider
{
    private readonly IDashboardClient? _dashboardClient;
    
    public DashboardMcpServerDataProvider(IDashboardClient? dashboardClient)
    {
        _dashboardClient = dashboardClient;
    }
    
    public bool IsAvailable => _dashboardClient?.IsEnabled ?? false;
    
    public async Task<string> GetConsoleLogsAsync(string resourceName, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || _dashboardClient == null)
        {
            return "Dashboard client is not enabled.";
        }
        
        var logs = new StringBuilder();
        
        try
        {
            var lineCount = 0;
            const int maxLines = 100;
            
            // Use a timeout for the subscription
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            await foreach (var batch in _dashboardClient.SubscribeConsoleLogs(resourceName, cts.Token).ConfigureAwait(false))
            {
                foreach (var logLine in batch)
                {
                    if (logLine.IsErrorMessage)
                    {
                        logs.AppendLine(CultureInfo.InvariantCulture, $"[{logLine.LineNumber}] ERROR: {logLine.Content}");
                    }
                    else
                    {
                        logs.AppendLine(CultureInfo.InvariantCulture, $"[{logLine.LineNumber}] {logLine.Content}");
                    }
                    
                    lineCount++;
                    if (lineCount >= maxLines)
                    {
                        logs.AppendLine(CultureInfo.InvariantCulture, $"\n... (showing first {maxLines} lines)");
                        return logs.ToString();
                    }
                }
            }
            
            if (lineCount == 0)
            {
                logs.AppendLine(CultureInfo.InvariantCulture, $"No console logs available for resource: {resourceName}");
                logs.AppendLine("(The resource may not exist, may not be running, or may not have produced any output yet)");
            }
        }
        catch (OperationCanceledException)
        {
            if (logs.Length == 0)
            {
                logs.AppendLine(CultureInfo.InvariantCulture, $"No logs received for resource: {resourceName}");
            }
        }
        catch (Exception ex)
        {
            return $"Error fetching logs for resource '{resourceName}': {ex.Message}";
        }
        
        return logs.ToString();
    }
    
    public async Task<string> ListAppHostResourcesAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || _dashboardClient == null)
        {
            return "Dashboard client is not enabled.";
        }
        
        var result = new StringBuilder();
        result.AppendLine("=== Available Resources ===\n");
        
        try
        {
            var subscription = await _dashboardClient.SubscribeResourcesAsync(cancellationToken).ConfigureAwait(false);
            
            var hasResources = false;
            foreach (var resource in subscription.InitialState)
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"- {resource.Name}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Display Name: {resource.DisplayName}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Type: {resource.ResourceType}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  State: {resource.State}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Can retrieve logs: {(resource.State == "Running" || resource.State == "Starting" ? "Yes" : "No")}");
                result.AppendLine();
                hasResources = true;
            }
            
            if (!hasResources)
            {
                result.AppendLine("No resources found.");
            }
            else
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"Use GetResourceLogs(\"<resource-name>\") to retrieve logs for a specific resource.");
            }
        }
        catch (Exception ex)
        {
            return $"Error listing resources: {ex.Message}";
        }
        
        return result.ToString();
    }
    
    public async Task<string> ExecuteResourceCommandAsync(string resourceId, string command, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || _dashboardClient == null)
        {
            return "Dashboard client is not enabled.";
        }
        
        try
        {
            // Get the current resources to validate the resource exists
            var subscription = await _dashboardClient.SubscribeResourcesAsync(cancellationToken).ConfigureAwait(false);
            var resource = subscription.InitialState.FirstOrDefault(r => r.Name == resourceId);
            
            if (resource == null)
            {
                return $"Resource '{resourceId}' not found.";
            }
            
            // Map the command to the appropriate command name
            var commandName = command.ToUpperInvariant() switch
            {
                "START" => "resource-start",
                "STOP" => "resource-stop",
                "RESTART" => "resource-restart",
                _ => null
            };
            
            if (commandName == null)
            {
                return $"Invalid command '{command}'. Valid commands are: Start, Stop, Restart.";
            }
            
            // Find the command in the resource's available commands
            var resourceCommand = resource.Commands.FirstOrDefault(c => c.Name == commandName);
            
            if (resourceCommand == null)
            {
                return $"Command '{command}' is not available for resource '{resourceId}'.";
            }
            
            if (resourceCommand.State != CommandViewModelState.Enabled)
            {
                return $"Command '{command}' is not enabled for resource '{resourceId}' in its current state ({resource.State}).";
            }
            
            // Execute the command using the Dashboard client
            var response = await _dashboardClient.ExecuteResourceCommandAsync(
                resourceId, 
                resource.ResourceType, 
                resourceCommand, 
                cancellationToken).ConfigureAwait(false);
            
            if (response.Kind == ResourceCommandResponseKind.Succeeded)
            {
                return $"Successfully executed {command} on resource '{resourceId}'.";
            }
            else if (response.Kind == ResourceCommandResponseKind.Cancelled)
            {
                return $"Command {command} was cancelled for resource '{resourceId}'.";
            }
            else
            {
                return $"Failed to execute {command} on resource '{resourceId}': {response.ErrorMessage ?? "Unknown error"}";
            }
        }
        catch (Exception ex)
        {
            return $"Error executing command for resource '{resourceId}': {ex.Message}";
        }
    }
}