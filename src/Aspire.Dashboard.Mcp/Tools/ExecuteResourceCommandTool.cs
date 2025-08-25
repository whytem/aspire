// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Aspire.Dashboard.Mcp.Providers;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Tools;

/// <summary>
/// MCP server tool for executing commands on AppHost resources.
/// </summary>
[McpServerToolType]
public static class ExecuteResourceCommandTool
{
    /// <summary>
    /// Defines the available commands for resource management.
    /// </summary>
    public enum ResourceCommand
    {
        /// <summary>
        /// Start a stopped or failed resource.
        /// </summary>
        Start,
        
        /// <summary>
        /// Stop a running or starting resource.
        /// </summary>
        Stop,
        
        /// <summary>
        /// Restart a running resource.
        /// </summary>
        Restart
    }

    /// <summary>
    /// Executes a command on an AppHost resource.
    /// </summary>
    /// <param name="dataProvider">The MCP server data provider.</param>
    /// <param name="resourceId">The ID of the resource to manage.</param>
    /// <param name="command">The command to execute on the resource (Start, Stop, or Restart).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A message indicating the result of the operation.</returns>
    [McpServerTool(Name = "execute_resource_command")]
    [Description("Execute a command (Start, Stop, or Restart) on an AppHost resource")]
    public static async Task<string> ExecuteResourceCommand(
        IMcpServerDataProvider? dataProvider,
        [Required, Description("The ID of the resource to manage")] string resourceId,
        [Required, Description("The command to execute: Start, Stop, or Restart")] ResourceCommand command,
        CancellationToken cancellationToken)
    {
        if (dataProvider == null || !dataProvider.IsAvailable)
        {
            return "MCP server data provider is not available. Ensure the Dashboard is configured with a resource service client.";
        }

        if (string.IsNullOrEmpty(resourceId))
        {
            return "Resource ID is required.";
        }

        try
        {
            var result = await dataProvider.ExecuteResourceCommandAsync(resourceId, command.ToString(), cancellationToken).ConfigureAwait(false);
            return result ?? $"Command '{command}' initiated for resource '{resourceId}'.";
        }
        catch (InvalidOperationException ex)
        {
            return $"Invalid operation: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error executing command for resource '{resourceId}': {ex.Message}";
        }
    }
}