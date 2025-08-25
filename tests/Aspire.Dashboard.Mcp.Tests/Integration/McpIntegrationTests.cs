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
    public async Task McpServer_AppStarted_EchoToolWorks()
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

        // Assert - Verify Echo tool is available
        var tools = await client.ListToolsAsync();
        var echoTool = tools.FirstOrDefault(t => t.Name == "echo");
        Assert.NotNull(echoTool);
        Assert.Contains("Echoes the message back to the client", echoTool.Description);

        // Test the Echo tool
        var testMessage = "Hello from test!";
        var arguments = new Dictionary<string, object?>
        {
            ["message"] = testMessage
        };

        var response = await client.CallToolAsync("echo", arguments);
        
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.NotEmpty(response.Content);
        
        // ContentBlock has a Type, cast to TextContentBlock for text content
        var firstContent = response.Content.First();
        Assert.Equal("text", firstContent.Type);
        var textContent = firstContent as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Equal($"Hello from C#: {testMessage}", textContent.Text);
    }

    [Fact]
    public async Task McpServer_AppStarted_ReverseEchoToolWorks()
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

        // Assert - Verify ReverseEcho tool is available
        var tools = await client.ListToolsAsync();
        var reverseEchoTool = tools.FirstOrDefault(t => t.Name == "reverse_echo");
        Assert.NotNull(reverseEchoTool);
        Assert.Contains("Echoes in reverse the message sent by the client", reverseEchoTool.Description);

        // Test the ReverseEcho tool
        var testMessage = "hello";
        var arguments = new Dictionary<string, object?>
        {
            ["message"] = testMessage
        };

        var response = await client.CallToolAsync("reverse_echo", arguments);
        
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.NotEmpty(response.Content);
        
        // ContentBlock has a Type, cast to TextContentBlock for text content
        var firstContent = response.Content.First();
        Assert.Equal("text", firstContent.Type);
        var textContent = firstContent as TextContentBlock;
        Assert.NotNull(textContent);
        Assert.Equal("olleh", textContent.Text);
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

        // Assert - Verify both tools are available
        var tools = await client.ListToolsAsync();
        Assert.NotNull(tools);
        Assert.Equal(2, tools.Count);
        
        // Verify Echo tool
        var echoTool = tools.FirstOrDefault(t => t.Name == "echo");
        Assert.NotNull(echoTool);
        Assert.Contains("Echoes the message back to the client", echoTool.Description);
        
        // Verify ReverseEcho tool  
        var reverseEchoTool = tools.FirstOrDefault(t => t.Name == "reverse_echo");
        Assert.NotNull(reverseEchoTool);
        Assert.Contains("Echoes in reverse the message sent by the client", reverseEchoTool.Description);
    }
}