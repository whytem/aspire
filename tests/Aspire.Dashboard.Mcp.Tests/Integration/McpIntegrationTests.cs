// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

extern alias DashboardTests;

using DashboardTests::Aspire.Dashboard.Tests.Integration;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Xunit;

namespace Aspire.Dashboard.Mcp.Tests.Integration;

public class McpIntegrationTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task McpServer_AppStarted_ExecuteResourceCommandToolWorks()
    {
        // Arrange - Create Dashboard with MCP enabled (it's already configured in DashboardWebApplication)
        await using var app = IntegrationTestHelpers.CreateDashboardWebApplication(testOutputHelper);

        // Act - Start the Dashboard
        await app.StartAsync();

        // Get the frontend endpoint to access MCP
        var frontendEndpoint = app.FrontendSingleEndPointAccessor().EndPoint;
        var mcpEndpoint = $"http://{frontendEndpoint}/mcp";

        // Create MCP client and connect to the server
        using var httpClient = new HttpClient();
        var transportOptions = new SseClientTransportOptions 
        { 
            Endpoint = new Uri(mcpEndpoint)
        };
        var clientTransport = new SseClientTransport(transportOptions, httpClient, ownsHttpClient: false);

        var client = await McpClientFactory.CreateAsync(clientTransport);

        // Assert - Verify ExecuteResourceCommand tool is available
        var tools = await client.ListToolsAsync();
        var executeResourceCommandTool = tools.FirstOrDefault(t => t.Name == "execute_resource_command");
        Assert.NotNull(executeResourceCommandTool);
        Assert.Contains("Execute a command (Start, Stop, or Restart) on an AppHost resource", executeResourceCommandTool.Description);

        // Test the ExecuteResourceCommand tool
        var arguments = new Dictionary<string, object?>
        {
            ["resourceId"] = "test-resource",
            ["action"] = "Start"
        };

        var response = await client.CallToolAsync("execute_resource_command", arguments);
        
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.NotEmpty(response.Content);
        
        // ContentBlock has a Type, cast to TextContentBlock for text content
        var firstContent = response.Content.First();
        Assert.Equal("text", firstContent.Type);
        var textContent = firstContent as TextContentBlock;
        Assert.NotNull(textContent);
        // The response will vary based on whether the resource exists, but it should contain a meaningful message
        Assert.NotNull(textContent.Text);
    }
    
    [Fact]
    public async Task McpServer_ConsoleLogsResource_ListsResources()
    {
        // Arrange - Create Dashboard with MCP enabled (it's already configured in DashboardWebApplication)
        await using var app = IntegrationTestHelpers.CreateDashboardWebApplication(testOutputHelper);

        // Act - Start the Dashboard
        await app.StartAsync();

        // Get the frontend endpoint to access MCP
        var frontendEndpoint = app.FrontendSingleEndPointAccessor().EndPoint;
        var mcpEndpoint = $"http://{frontendEndpoint}/mcp";

        // Create MCP client and connect to the server
        using var httpClient = new HttpClient();
        var transportOptions = new SseClientTransportOptions 
        { 
            Endpoint = new Uri(mcpEndpoint)
        };
        var clientTransport = new SseClientTransport(transportOptions, httpClient, ownsHttpClient: false);

        var client = await McpClientFactory.CreateAsync(clientTransport);

        // Act - List available resources
        var resources = await client.ListResourcesAsync();
        
        // Assert
        Assert.NotNull(resources);
        
        // Check that resources are registered
        // Note: MCP resources are registered with their method names
        var resourceNames = resources.Select(r => r.Name).ToList();
        
        // The ConsoleLogsResource should have registered its methods
        // We just verify that resources are being registered (exact names may vary)
        Assert.NotEmpty(resources);
    }
    
    [Fact]
    public async Task McpServer_AppStarted_ListToolsReturnsAll()
    {
        // Arrange - Create Dashboard with MCP enabled (it's already configured in DashboardWebApplication)
        await using var app = IntegrationTestHelpers.CreateDashboardWebApplication(testOutputHelper);

        // Act - Start the Dashboard
        await app.StartAsync();

        // Get the frontend endpoint to access MCP
        var frontendEndpoint = app.FrontendSingleEndPointAccessor().EndPoint;
        var mcpEndpoint = $"http://{frontendEndpoint}/mcp";

        // Create MCP client and connect to the server
        using var httpClient = new HttpClient();
        var transportOptions = new SseClientTransportOptions 
        { 
            Endpoint = new Uri(mcpEndpoint)
        };
        var clientTransport = new SseClientTransport(transportOptions, httpClient, ownsHttpClient: false);

        var client = await McpClientFactory.CreateAsync(clientTransport);

        // Assert - Verify ExecuteResourceCommand tool is available
        var tools = await client.ListToolsAsync();
        Assert.NotNull(tools);
        Assert.Single(tools);
        
        // Verify ExecuteResourceCommand tool
        var executeResourceCommandTool = tools.FirstOrDefault(t => t.Name == "execute_resource_command");
        Assert.NotNull(executeResourceCommandTool);
        Assert.Contains("Execute a command (Start, Stop, or Restart) on an AppHost resource", executeResourceCommandTool.Description);
    }
}