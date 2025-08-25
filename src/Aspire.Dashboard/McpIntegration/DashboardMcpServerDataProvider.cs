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
    
    public async Task<string> GetWorkloadLogsAsync(string workloadName, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || _dashboardClient == null)
        {
            return "Dashboard client is not enabled.";
        }
        
        var logs = new StringBuilder();
        logs.AppendLine(CultureInfo.InvariantCulture, $"=== Console Logs for '{workloadName}' ===\n");
        
        try
        {
            var lineCount = 0;
            const int maxLines = 100;
            
            // Use a timeout for the subscription
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            
            await foreach (var batch in _dashboardClient.SubscribeConsoleLogs(workloadName, cts.Token).ConfigureAwait(false))
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
                
                // Just get the first batch for this minimal example
                if (lineCount > 0)
                {
                    break;
                }
            }
            
            if (lineCount == 0)
            {
                logs.AppendLine(CultureInfo.InvariantCulture, $"No console logs available for workload: {workloadName}");
                logs.AppendLine("(The workload may not exist, may not be running, or may not have produced any output yet)");
            }
        }
        catch (OperationCanceledException)
        {
            if (logs.Length == 0)
            {
                logs.AppendLine(CultureInfo.InvariantCulture, $"No logs received for workload: {workloadName}");
            }
        }
        catch (Exception ex)
        {
            return $"Error fetching logs for workload '{workloadName}': {ex.Message}";
        }
        
        return logs.ToString();
    }
    
    public async Task<string> ListWorkloadsAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable || _dashboardClient == null)
        {
            return "Dashboard client is not enabled.";
        }
        
        var result = new StringBuilder();
        result.AppendLine("=== Available Workloads ===\n");
        
        try
        {
            var subscription = await _dashboardClient.SubscribeResourcesAsync(cancellationToken).ConfigureAwait(false);
            
            var hasWorkloads = false;
            foreach (var resource in subscription.InitialState)
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"- {resource.Name}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Display Name: {resource.DisplayName}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Type: {resource.ResourceType}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  State: {resource.State}");
                result.AppendLine(CultureInfo.InvariantCulture, $"  Can retrieve logs: {(resource.State == "Running" || resource.State == "Starting" ? "Yes" : "No")}");
                result.AppendLine();
                hasWorkloads = true;
            }
            
            if (!hasWorkloads)
            {
                result.AppendLine("No workloads found.");
            }
            else
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"Use GetWorkloadLogs(\"<workload-name>\") to retrieve logs for a specific workload.");
            }
        }
        catch (Exception ex)
        {
            return $"Error listing workloads: {ex.Message}";
        }
        
        return result.ToString();
    }
}