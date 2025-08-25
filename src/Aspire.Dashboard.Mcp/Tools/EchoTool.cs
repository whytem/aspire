// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Aspire.Dashboard.Mcp.Tools;

/// <summary>
/// Sample MCP server tools for testing the MCP integration.
/// </summary>
[McpServerToolType]
public static class EchoTool
{
    /// <summary>
    /// Echoes the message back to the client with a greeting.
    /// </summary>
    /// <param name="message">The message to echo.</param>
    /// <returns>The echoed message with a greeting.</returns>
    [McpServerTool, Description("Echoes the message back to the client")]
    public static string Echo(string message) => $"Hello from C#: {message}";

    /// <summary>
    /// Echoes the message in reverse order.
    /// </summary>
    /// <param name="message">The message to reverse.</param>
    /// <returns>The reversed message.</returns>
    [McpServerTool, Description("Echoes in reverse the message sent by the client")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}