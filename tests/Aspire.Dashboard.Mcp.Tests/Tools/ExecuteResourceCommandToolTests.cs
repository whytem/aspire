// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Dashboard.Mcp.Providers;
using Aspire.Dashboard.Mcp.Tools;
using Xunit;

namespace Aspire.Dashboard.Mcp.Tests.Tools;

public class ExecuteResourceCommandToolTests
{
    [Fact]
    public async Task ExecuteResourceCommand_Start_Success()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            StateChangeResult = "Successfully initiated Start operation for resource 'test-resource'. Current state: Stopped."
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Start,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Successfully initiated Start", result);
        Assert.Contains("test-resource", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_Stop_Success()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            StateChangeResult = "Successfully initiated Stop operation for resource 'test-resource'. Current state: Running."
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Stop,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Successfully initiated Stop", result);
        Assert.Contains("test-resource", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_Restart_Success()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            StateChangeResult = "Successfully initiated Restart operation for resource 'test-resource'. Current state: Running."
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Restart,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Successfully initiated Restart", result);
        Assert.Contains("test-resource", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_NoProvider_ReturnsWarning()
    {
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            null,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Start,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("MCP server data provider is not available", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_EmptyResourceId_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider { IsAvailable = true };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "",
            ExecuteResourceCommandTool.ResourceCommand.Start,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Resource ID is required", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_ResourceNotFound_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            StateChangeResult = "Resource 'nonexistent' not found."
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "nonexistent",
            ExecuteResourceCommandTool.ResourceCommand.Start,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Resource 'nonexistent' not found", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_InvalidStateTransition_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            StateChangeResult = "Cannot Start resource 'test-resource' in Running state."
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Start,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Cannot Start resource", result);
        Assert.Contains("Running state", result);
    }
    
    [Fact]
    public async Task ExecuteResourceCommand_ProviderThrows_ReturnsError()
    {
        // Arrange
        var provider = new TestMcpServerDataProvider
        {
            IsAvailable = true,
            ShouldThrow = true
        };
        
        // Act
        var result = await ExecuteResourceCommandTool.ExecuteResourceCommand(
            provider,
            "test-resource",
            ExecuteResourceCommandTool.ResourceCommand.Stop,
            CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
        Assert.Contains("Invalid operation", result);
    }
    
    // Test implementation of the provider
    private sealed class TestMcpServerDataProvider : IMcpServerDataProvider
    {
        public bool IsAvailable { get; set; }
        public string LogsToReturn { get; set; } = string.Empty;
        public string ResourcesToReturn { get; set; } = string.Empty;
        public string StateChangeResult { get; set; } = string.Empty;
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
            return Task.FromResult(StateChangeResult);
        }
    }
}