// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable IDE0005 // Using directive is necessary
using ModelContextProtocol.AspNetCore;
#pragma warning restore IDE0005

namespace Aspire.Dashboard.Mcp;

/// <summary>
/// Extension methods for configuring MCP (Model Context Protocol) server in the Dashboard.
/// </summary>
public static class McpServiceExtensions
{
    /// <summary>
    /// Adds MCP server services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDashboardMcpServer(this IServiceCollection services)
    {
        // Add MCP server - minimal configuration for Aspire integration
        services.AddMcpServer()
            .WithHttpTransport() // HTTP/SSE transport
            .WithToolsFromAssembly(typeof(McpServiceExtensions).Assembly) // Auto-discover tools from this assembly
            .WithResourcesFromAssembly(typeof(McpServiceExtensions).Assembly); // Auto-discover resources from this assembly

        return services;
    }

    /// <summary>
    /// Configures the MCP server endpoints in the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapMcpServer(this WebApplication app)
    {
        // Map MCP endpoints
        app.MapMcp("/mcp");
        
        return app;
    }
}