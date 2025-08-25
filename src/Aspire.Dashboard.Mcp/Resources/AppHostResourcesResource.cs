// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Aspire.Dashboard.Mcp.Providers;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Resources;

/// <summary>
/// MCP resource that provides access to AppHost resources information.
/// </summary>
[McpServerResourceType]
public class AppHostResourcesResource
{
    /// <summary>
    /// Lists all available AppHost resources.
    /// </summary>
    [McpServerResource(Name = "apphost_resources")]
    [Description("List all resources in the AppHost")]
    public static async Task<string> ListResources(
        IMcpServerDataProvider? dataProvider,
        CancellationToken cancellationToken)
    {
        if (dataProvider == null || !dataProvider.IsAvailable)
        {
            return "MCP server data provider is not available. Ensure the Dashboard is configured with a resource service client.";
        }
        
        try
        {
            return await dataProvider.ListAppHostResourcesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return $"Error listing AppHost resources: {ex.Message}";
        }
    }
}