// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Mcp.Providers;
using Aspire.Dashboard.Mcp.Resources;
using Xunit;

namespace Aspire.Dashboard.Mcp.Tests.Resources;

public class AppHostResourcesResourceTests
{
    [Fact]
    public async Task ListResources_WithProvider_ReturnsList()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            ResourcesToReturn = "Resource1\nResource2\nResource3"
        };
        
        // Act
        var result = await AppHostResourcesResource.ListResources(
            provider,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Resource1\nResource2\nResource3", result);
    }
    
    [Fact]
    public async Task ListResources_NoProvider_ReturnsWarning()
    {
        // Act
        var result = await AppHostResourcesResource.ListResources(
            null,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("MCP server data provider is not available", result);
    }
    
    [Fact]
    public async Task ListResources_ProviderThrows_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            ShouldThrow = true
        };
        
        // Act
        var result = await AppHostResourcesResource.ListResources(
            provider,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error listing AppHost resources", result);
    }
    
    // Test implementation of the provider
    private sealed class TestMcpServerDataProvider : IMcpServerDataProvider
    {
        public bool IsAvailable { get; set; }
        public string LogsToReturn { get; set; } = string.Empty;
        public string ResourcesToReturn { get; set; } = string.Empty;
        public bool ShouldThrow { get; set; }
        
        public Task<string> GetConsoleLogsAsync(string resourceName, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }
            return Task.FromResult(LogsToReturn);
        }
        
        public Task<string> ListAppHostResourcesAsync(CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }
            return Task.FromResult(ResourcesToReturn);
        }
        
        public Task<string> ExecuteResourceCommandAsync(string resourceId, string action, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }
            return Task.FromResult($"State change '{action}' initiated for resource '{resourceId}'.");
        }
    }
}