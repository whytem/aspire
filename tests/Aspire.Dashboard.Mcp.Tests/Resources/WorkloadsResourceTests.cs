// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Mcp.Providers;
using Aspire.Dashboard.Mcp.Resources;
using Xunit;

namespace Aspire.Dashboard.Mcp.Tests.Resources;

public class WorkloadsResourceTests
{
    [Fact]
    public async Task ListWorkloads_WithProvider_ReturnsList()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            WorkloadsToReturn = "Workload1\nWorkload2\nWorkload3"
        };
        
        // Act
        var result = await WorkloadsResource.ListWorkloads(
            provider,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Workload1\nWorkload2\nWorkload3", result);
    }
    
    [Fact]
    public async Task ListWorkloads_NoProvider_ReturnsWarning()
    {
        // Act
        var result = await WorkloadsResource.ListWorkloads(
            null,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("MCP server data provider is not available", result);
    }
    
    [Fact]
    public async Task ListWorkloads_ProviderThrows_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            ShouldThrow = true
        };
        
        // Act
        var result = await WorkloadsResource.ListWorkloads(
            provider,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Error listing workloads", result);
    }
    
    // Test implementation of the provider
    private sealed class TestMcpServerDataProvider : IMcpServerDataProvider
    {
        public bool IsAvailable { get; set; }
        public string LogsToReturn { get; set; } = string.Empty;
        public string WorkloadsToReturn { get; set; } = string.Empty;
        public bool ShouldThrow { get; set; }
        
        public Task<string> GetWorkloadLogsAsync(string workloadName, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }
            return Task.FromResult(LogsToReturn);
        }
        
        public Task<string> ListWorkloadsAsync(CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new InvalidOperationException("Test exception");
            }
            return Task.FromResult(WorkloadsToReturn);
        }
    }
}