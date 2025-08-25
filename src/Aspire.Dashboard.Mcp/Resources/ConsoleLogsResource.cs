// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Aspire.Dashboard.Mcp.Providers;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Resources;

/// <summary>
/// MCP resource that provides access to console logs of a workload in the AppHost.
/// </summary>
[McpServerResourceType]
public class ConsoleLogsResource
{
    /// <summary>
    /// Gets console logs for a specific workload.
    /// </summary>
    [McpServerResource(UriTemplate = "workload://logs/{workloadName}", Name = "workload_logs")]
    [Description("Get console logs for a specific workload")]
    public static async Task<string> GetConsoleLogs(
        IMcpServerDataProvider? dataProvider,
        string workloadName,
        CancellationToken cancellationToken)
    {
        if (dataProvider == null || !dataProvider.IsAvailable)
        {
            return "MCP server data provider is not available. Ensure the Dashboard is configured with a resource service client.";
        }
        
        if (string.IsNullOrEmpty(workloadName))
        {
            return "Workload name is required.";
        }

        try
        {
            var logs = await dataProvider.GetWorkloadLogsAsync(workloadName, cancellationToken).ConfigureAwait(false);
            return logs ?? $"No logs found for workload '{workloadName}'.";
        }
        catch (Exception ex)
        {
            return $"Error getting logs for workload '{workloadName}': {ex.Message}";
        }
    }
}