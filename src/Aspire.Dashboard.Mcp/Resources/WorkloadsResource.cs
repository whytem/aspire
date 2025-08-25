// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Aspire.Dashboard.Mcp.Providers;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Resources;

/// <summary>
/// MCP resource that provides access to workload information.
/// </summary>
[McpServerResourceType]
public class WorkloadsResource
{
    /// <summary>
    /// Lists all available workloads.
    /// </summary>
    [McpServerResource]
    [Description("List all workloads in the AppHost")]
    public static async Task<string> ListWorkloads(
        IMcpServerDataProvider? dataProvider,  // Injected via DI
        CancellationToken cancellationToken)
    {
        if (dataProvider == null || !dataProvider.IsAvailable)
        {
            return "MCP server data provider is not available. Ensure the Dashboard is configured with a resource service client.";
        }
        
        try
        {
            return await dataProvider.ListWorkloadsAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return $"Error listing workloads: {ex.Message}";
        }
    }
}