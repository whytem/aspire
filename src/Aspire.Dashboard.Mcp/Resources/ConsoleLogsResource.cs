// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Aspire.Dashboard.Mcp.Providers;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Resources;

/// <summary>
/// MCP resource that provides access to console logs of a resource in the AppHost.
/// </summary>
[McpServerResourceType]
public class ConsoleLogsResource
{
    /// <summary>
    /// Gets console logs for a specific AppHost resource.
    /// </summary>
    [McpServerResource(UriTemplate = "resource://console_logs/{resourceName}", Name = "console_logs")]
    [Description("Get console logs for a specific AppHost resource")]
    public static async Task<string> GetConsoleLogs(
        IMcpServerDataProvider? dataProvider,
        string resourceName,
        CancellationToken cancellationToken)
    {
        if (dataProvider == null || !dataProvider.IsAvailable)
        {
            return "MCP server data provider is not available. Ensure the Dashboard is configured with a resource service client.";
        }
        
        if (string.IsNullOrEmpty(resourceName))
        {
            return "Resource name is required.";
        }

        try
        {
            var logs = await dataProvider.GetConsoleLogsAsync(resourceName, cancellationToken).ConfigureAwait(false);
            return logs ?? $"No logs found for resource '{resourceName}'.";
        }
        catch (Exception ex)
        {
            return $"Error getting logs for resource '{resourceName}': {ex.Message}";
        }
    }
}