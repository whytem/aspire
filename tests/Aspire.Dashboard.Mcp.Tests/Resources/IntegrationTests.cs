// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Mcp.Providers;
using Aspire.Dashboard.Mcp.Resources;
using Xunit;

namespace Aspire.Dashboard.Mcp.Tests.Resources;

/// <summary>
/// Integration tests that verify the full workflow of listing resources
/// and then getting console logs for one of the returned results.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public async Task ListResourcesThenGetLogs_FullWorkflow_Success()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            // Simulate the actual format returned by DashboardMcpServerDataProvider
            ResourcesToReturn = @"=== Available Resources ===

- webfrontend
  Display Name: WebFrontEnd
  Type: Project
  State: Running
  Can retrieve logs: Yes

- apiservice
  Display Name: API Service
  Type: Project
  State: Running
  Can retrieve logs: Yes

- cache
  Display Name: Redis Cache
  Type: Container
  State: Starting
  Can retrieve logs: No

Use GetResourceLogs(""<resource-name>"") to retrieve logs for a specific resource.",
            LogsToReturn = @"[1] Starting ASP.NET Core application...
[2] Now listening on: http://localhost:5000
[3] Application started. Press Ctrl+C to shut down.
[4] Hosting environment: Development
[5] Content root path: /app"
        };
        
        // Act - Step 1: List resources
        var resourcesList = await AppHostResourcesResource.ListResources(
            provider,
            CancellationToken.None);
        
        // Assert - Verify resources list contains expected resources
        Assert.NotNull(resourcesList);
        Assert.Contains("webfrontend", resourcesList);
        Assert.Contains("apiservice", resourcesList);
        Assert.Contains("cache", resourcesList);
        
        // Act - Step 2: Extract a resource name from the list (simulating parsing)
        // In a real scenario, the client would parse this output
        var resourceName = "webfrontend"; // Extract first resource name
        
        // Act - Step 3: Get console logs for the extracted resource
        var logs = await ConsoleLogsResource.GetConsoleLogs(
            provider,
            resourceName,
            CancellationToken.None);
        
        // Assert - Verify we got logs for the resource
        Assert.NotNull(logs);
        Assert.Contains("Starting ASP.NET Core application", logs);
        Assert.Contains("Now listening on", logs);
        Assert.Contains("Application started", logs);
    }
    
    [Fact]
    public async Task ListResourcesThenGetLogs_ResourceSpecificLogs_Success()
    {
        // Arrange - Provider that returns different logs based on resource name
        var provider = new ResourceAwareMcpServerDataProvider
        {
            IsAvailable = true,
            ResourcesToReturn = @"=== Available Resources ===

- frontend
  Display Name: Frontend
  Type: Project
  State: Running
  Can retrieve logs: Yes

- backend
  Display Name: Backend
  Type: Project  
  State: Running
  Can retrieve logs: Yes

Use GetResourceLogs(""<resource-name>"") to retrieve logs for a specific resource."
        };
        
        // Act & Assert - Get logs for frontend
        var frontendLogs = await ConsoleLogsResource.GetConsoleLogs(
            provider,
            "frontend",
            CancellationToken.None);
        
        Assert.NotNull(frontendLogs);
        Assert.Contains("Frontend application logs", frontendLogs);
        Assert.DoesNotContain("Backend service logs", frontendLogs);
        
        // Act & Assert - Get logs for backend
        var backendLogs = await ConsoleLogsResource.GetConsoleLogs(
            provider,
            "backend",
            CancellationToken.None);
        
        Assert.NotNull(backendLogs);
        Assert.Contains("Backend service logs", backendLogs);
        Assert.DoesNotContain("Frontend application logs", backendLogs);
    }
    
    [Fact]
    public async Task ListResourcesThenGetLogs_NonExistentResource_ReturnsAppropriateMessage()
    {
        // Arrange
        var provider = new ResourceAwareMcpServerDataProvider
        {
            IsAvailable = true,
            ResourcesToReturn = @"=== Available Resources ===

- webapi
  Display Name: Web API
  Type: Project
  State: Running
  Can retrieve logs: Yes

Use GetResourceLogs(""<resource-name>"") to retrieve logs for a specific resource."
        };
        
        // Act - Try to get logs for a resource that wasn't in the list
        var logs = await ConsoleLogsResource.GetConsoleLogs(
            provider,
            "nonexistent",
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(logs);
        Assert.Contains("No logs available for resource: nonexistent", logs);
    }
    
    // Test implementation of the provider
    private sealed class TestMcpServerDataProvider : IMcpServerDataProvider
    {
        public bool IsAvailable { get; set; }
        public string LogsToReturn { get; set; } = string.Empty;
        public string ResourcesToReturn { get; set; } = string.Empty;
        
        public Task<string> GetConsoleLogsAsync(string resourceName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(LogsToReturn);
        }
        
        public Task<string> ListAppHostResourcesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResourcesToReturn);
        }
        
        public Task<string> ExecuteResourceCommandAsync(string resourceId, string action, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"State change '{action}' initiated for resource '{resourceId}'.");
        }
    }
    
    // Resource-aware provider that returns different logs based on resource name
    private sealed class ResourceAwareMcpServerDataProvider : IMcpServerDataProvider
    {
        public bool IsAvailable { get; set; }
        public string ResourcesToReturn { get; set; } = string.Empty;
        
        public Task<string> GetConsoleLogsAsync(string resourceName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(resourceName switch
            {
                "frontend" => "Frontend application logs: React app started on port 3000",
                "backend" => "Backend service logs: API listening on port 5000", 
                _ => $"No logs available for resource: {resourceName}"
            });
        }
        
        public Task<string> ListAppHostResourcesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ResourcesToReturn);
        }
        
        public Task<string> ExecuteResourceCommandAsync(string resourceId, string action, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(resourceId switch
            {
                "frontend" => "Successfully initiated Start operation for resource 'frontend'.",
                "backend" => "Successfully initiated Stop operation for resource 'backend'.",
                _ => $"Resource '{resourceId}' not found."
            });
        }
    }
}