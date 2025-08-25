// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Dashboard.Mcp.Providers;

/// <summary>
/// Provides data access for MCP server resources from the Dashboard.
/// This interface is implemented by the Dashboard to provide various data types to MCP resources.
/// </summary>
public interface IMcpServerDataProvider
{
    /// <summary>
    /// Gets whether the provider is available and can provide data.
    /// </summary>
    bool IsAvailable { get; }
    
    /// <summary>
    /// Gets console logs for a specific workload.
    /// </summary>
    /// <param name="workloadName">The name of the workload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The console logs as a formatted string.</returns>
    Task<string> GetWorkloadLogsAsync(string workloadName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Lists all available workloads.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A formatted list of workloads.</returns>
    Task<string> ListWorkloadsAsync(CancellationToken cancellationToken = default);
}