// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Dashboard.Mcp.Providers;

/// <summary>
/// Provides data access for MCP server resources from the Dashboard.
/// This interface is implemented by the Dashboard to provide various data types to MCP resources.
/// Alternatively, it could be implemented as a direct gRPC client to the resource server
/// </summary>
public interface IMcpServerDataProvider
{
    /// <summary>
    /// Gets whether the provider is available and can provide data.
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Gets console logs for a specific AppHost resource.
    /// </summary>
    /// <param name="resourceName">The name of the AppHost resource.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The console logs as a formatted string.</returns>
    Task<string> GetConsoleLogsAsync(string resourceName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all available AppHost resources.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted list of resources.</returns>
    Task<string> ListAppHostResourcesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a command on an AppHost resource.
    /// </summary>
    /// <param name="resourceId">The ID of the resource.</param>
    /// <param name="command">The command to execute (Start, Stop, or Restart).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message describing the result of the operation.</returns>
    Task<string> ExecuteResourceCommandAsync(string resourceId, string command, CancellationToken cancellationToken = default);
}