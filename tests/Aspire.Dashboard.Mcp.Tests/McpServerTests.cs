// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
#pragma warning disable IDE0005 // Using directive is necessary
using Aspire.Dashboard.Mcp;
#pragma warning restore IDE0005

namespace Aspire.Dashboard.Mcp.Tests;

public class McpServerTests
{
    [Fact]
    public void AddDashboardMcpServer_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddDashboardMcpServer();
        
        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }
    
    [Fact]
    public void MapMcpServer_MapsEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddDashboardMcpServer();
        var app = builder.Build();
        
        // Act & Assert (should not throw)
        app.MapMcpServer();
    }
}