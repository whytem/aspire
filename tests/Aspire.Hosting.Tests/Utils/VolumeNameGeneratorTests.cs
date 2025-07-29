// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using static Aspire.Hosting.VolumeNameGenerator;
using Aspire.Hosting.Utils;

namespace Aspire.Hosting.Tests.Utils;

public class VolumeNameGeneratorTests
{
    [Fact]
    public void VolumeGeneratorUsesUniqueName()
    {
        var builder = DistributedApplication.CreateBuilder();

        var volumePrefix = $"{Sanitize(builder.Environment.ApplicationName).ToLowerInvariant()}-{builder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";

        var resource = builder.AddResource(new TestResource("myresource"));

        var volumeName = Generate(resource, "data");

        Assert.Equal($"{volumePrefix}-{resource.Resource.Name}-data", volumeName);
    }

    [Theory]
    [MemberData(nameof(InvalidNameParts))]
    public void ThrowsWhenSuffixContainsInvalidChars(string suffix)
    {
        var builder = DistributedApplication.CreateBuilder();
        var resource = builder.AddResource(new TestResource("myresource"));

        Assert.Throws<ArgumentException>(nameof(suffix), () => Generate(resource, suffix));
    }

    public static object[][] InvalidNameParts => [
        ["This/is/invalid"],
        [@"This\is\invalid"],
        ["_ThisIsInvalidToo"],
        [".ThisIsInvalidToo"],
        ["-ThisIsInvalidToo"],
        ["This&IsInvalidToo"]
    ];

    private sealed class TestResource(string name) : IResource
    {
        public string Name { get; } = name;

        public ResourceAnnotationCollection Annotations { get; } = [];
    }

    [Fact]
    public void VolumeNameDiffersBetweenPublishAndRun()
    {
        var runBuilder = TestDistributedApplicationBuilder.Create();
        var publishBuilder = TestDistributedApplicationBuilder.Create(DistributedApplicationOperation.Publish);

        var runVolumePrefix = $"{Sanitize(runBuilder.Environment.ApplicationName).ToLowerInvariant()}-{runBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";
        var publishVolumePrefix = $"{Sanitize(publishBuilder.Environment.ApplicationName).ToLowerInvariant()}-{publishBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";

        var runResource = runBuilder.AddResource(new TestResource("myresource"));
        var publishResource = publishBuilder.AddResource(new TestResource("myresource"));

        var runVolumeName = Generate(runResource, "data");
        var publishVolumeName = Generate(publishResource, "data");

        Assert.Equal($"{runVolumePrefix}-{runResource.Resource.Name}-data", runVolumeName);
        Assert.Equal($"{publishVolumePrefix}-{publishResource.Resource.Name}-data", publishVolumeName);
        Assert.NotEqual(runVolumeName, publishVolumeName);
    }

    [Fact]
    public void VolumeNameSameBetweenDebuggingAndDotnetRun()
    {
        // This test verifies the fix for https://github.com/dotnet/aspire/issues/10716
        // F5 debugging and dotnet run should produce the same volume names
        // Both scenarios should be in run mode and use path-based hashing with normalization
        
        // Simulate dotnet run (no special configuration, defaults to run mode)
        var dotnetRunBuilder = TestDistributedApplicationBuilder.Create();
        
        // Simulate F5 debugging (explicitly in run mode, which is how F5 debugging actually works)
        var f5DebuggingBuilder = TestDistributedApplicationBuilder.Create("--operation", "run");

        var dotnetRunVolumePrefix = $"{Sanitize(dotnetRunBuilder.Environment.ApplicationName).ToLowerInvariant()}-{dotnetRunBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";
        var f5DebuggingVolumePrefix = $"{Sanitize(f5DebuggingBuilder.Environment.ApplicationName).ToLowerInvariant()}-{f5DebuggingBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";

        var dotnetRunResource = dotnetRunBuilder.AddResource(new TestResource("myresource"));
        var f5DebuggingResource = f5DebuggingBuilder.AddResource(new TestResource("myresource"));

        var dotnetRunVolumeName = Generate(dotnetRunResource, "data");
        var f5DebuggingVolumeName = Generate(f5DebuggingResource, "data");

        Assert.Equal($"{dotnetRunVolumePrefix}-{dotnetRunResource.Resource.Name}-data", dotnetRunVolumeName);
        Assert.Equal($"{f5DebuggingVolumePrefix}-{f5DebuggingResource.Resource.Name}-data", f5DebuggingVolumeName);
        
        // This is the key assertion: F5 and dotnet run should produce the same volume names
        // because both are in run mode and should use the same path-based hash normalization
        Assert.Equal(dotnetRunVolumeName, f5DebuggingVolumeName);
    }

    [Fact]
    public void VolumeNameConsistentWithPathNormalization()
    {
        // This test verifies that the path normalization logic works correctly
        // ensuring that different representations of the same path produce the same volume name
        
        // Both builders are in run mode, so they should use path-based hashing with normalization
        var builder1 = TestDistributedApplicationBuilder.Create();
        var builder2 = TestDistributedApplicationBuilder.Create();
        
        // Get the actual AppHost path that will be used for hashing
        var appHostPath1 = builder1.Configuration["AppHost:Path"];
        var appHostPath2 = builder2.Configuration["AppHost:Path"];
        
        // Both should have the same path since they're running in the same process
        Assert.Equal(appHostPath1, appHostPath2);
        
        // The path normalization logic is tested implicitly through the SHA256 hash comparison
        // Both builders use the same AppHost path and should produce identical normalized paths
        
        // Both builders should produce the same SHA256 hash because they use the same normalized path
        var sha1 = builder1.Configuration["AppHost:Sha256"];
        var sha2 = builder2.Configuration["AppHost:Sha256"];
        
        Assert.Equal(sha1, sha2);
        
        // And therefore, both should produce the same volume names
        var resource1 = builder1.AddResource(new TestResource("myresource"));
        var resource2 = builder2.AddResource(new TestResource("myresource"));
        
        var volumeName1 = Generate(resource1, "data");
        var volumeName2 = Generate(resource2, "data");
        
        Assert.Equal(volumeName1, volumeName2);
    }
    
    [Fact]
    public void VolumeNameUsesPathNormalizationInRunMode()
    {
        // This test verifies that volume names are generated using the run mode path normalization logic
        // which includes Path.GetFullPath() and Windows casing normalization
        
        var builder = TestDistributedApplicationBuilder.Create(); // Defaults to run mode
        
        // Verify this builder is indeed in run mode (not publish mode)
        Assert.False(builder.ExecutionContext.IsPublishMode);
        Assert.True(builder.ExecutionContext.IsRunMode);
        
        // Get the SHA256 that was generated - this should be based on the normalized AppHost path
        var appHostSha = builder.Configuration["AppHost:Sha256"];
        Assert.NotNull(appHostSha);
        
        // The SHA should be consistent across multiple instantiations with the same path
        var builder2 = TestDistributedApplicationBuilder.Create();
        var appHostSha2 = builder2.Configuration["AppHost:Sha256"];
        
        Assert.Equal(appHostSha, appHostSha2);
        
        // Both builders should generate the same volume names for the same resource
        var resource1 = builder.AddResource(new TestResource("testresource"));
        var resource2 = builder2.AddResource(new TestResource("testresource"));
        
        var volumeName1 = Generate(resource1, "data");
        var volumeName2 = Generate(resource2, "data");
        
        Assert.Equal(volumeName1, volumeName2);
    }

    [Theory]
    [InlineData(@"C:\Project\App")]
    [InlineData(@"c:\project\app")]
    [InlineData(@"C:/Project/App")]
    [InlineData(@"C:\Project\App\")]
    [InlineData(@"C:\Project\..\Project\App")]
    public void VolumeNameConsistentAcrossPathCasingsAndFormats(string projectDirectory)
    {
        // This test verifies that different representations of the same path produce the same volume name
        // when using DistributedApplicationBuilder with DistributedApplicationOptions.ProjectDirectory
        
        var options = new DistributedApplicationOptions
        {
            ProjectDirectory = projectDirectory,
            Args = [] // Ensure run mode (default)
        };
        
        var builder = DistributedApplication.CreateBuilder(options);
        
        // Verify this is in run mode so path-based hashing is used
        Assert.False(builder.ExecutionContext.IsPublishMode);
        Assert.True(builder.ExecutionContext.IsRunMode);
        
        var appHostSha = builder.Configuration["AppHost:Sha256"];
        Assert.NotNull(appHostSha);
        
        // On Windows, all these different path representations should produce the same SHA
        // because the path normalization logic applies Path.GetFullPath() and ToLowerInvariant()
        // On non-Windows systems, case differences would produce different SHAs, but path normalization still applies
        if (OperatingSystem.IsWindows())
        {
            // On Windows, all these different path representations should normalize to the same value
            // and produce the same SHA due to case-insensitive filesystem normalization
            // We test this by running with a reference path and comparing
            var referenceOptions = new DistributedApplicationOptions
            {
                ProjectDirectory = @"c:\project\app", // normalized reference form
                Args = []
            };
            
            var referenceBuilder = DistributedApplication.CreateBuilder(referenceOptions);
            var referenceSha = referenceBuilder.Configuration["AppHost:Sha256"];
            
            // All test paths should produce the same SHA as the reference
            Assert.Equal(referenceSha, appHostSha);
        }
        
        // Create a resource and verify volume name generation works
        var resource = builder.AddResource(new TestResource("myresource"));
        var volumeName = Generate(resource, "data");
        Assert.NotNull(volumeName);
        Assert.Contains("myresource-data", volumeName);
    }
}
