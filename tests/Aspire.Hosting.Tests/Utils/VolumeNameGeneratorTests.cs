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
        
        // Simulate dotnet run (no special configuration)
        var dotnetRunBuilder = TestDistributedApplicationBuilder.Create();
        
        // Simulate F5 debugging (has publisher config but no explicit operation)
        var f5DebuggingBuilder = TestDistributedApplicationBuilder.Create("Publishing:Publisher=manifest");

        var dotnetRunVolumePrefix = $"{Sanitize(dotnetRunBuilder.Environment.ApplicationName).ToLowerInvariant()}-{dotnetRunBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";
        var f5DebuggingVolumePrefix = $"{Sanitize(f5DebuggingBuilder.Environment.ApplicationName).ToLowerInvariant()}-{f5DebuggingBuilder.Configuration["AppHost:Sha256"]!.ToLowerInvariant()[..10]}";

        var dotnetRunResource = dotnetRunBuilder.AddResource(new TestResource("myresource"));
        var f5DebuggingResource = f5DebuggingBuilder.AddResource(new TestResource("myresource"));

        var dotnetRunVolumeName = Generate(dotnetRunResource, "data");
        var f5DebuggingVolumeName = Generate(f5DebuggingResource, "data");

        Assert.Equal($"{dotnetRunVolumePrefix}-{dotnetRunResource.Resource.Name}-data", dotnetRunVolumeName);
        Assert.Equal($"{f5DebuggingVolumePrefix}-{f5DebuggingResource.Resource.Name}-data", f5DebuggingVolumeName);
        
        // This is the key assertion: F5 and dotnet run should produce the same volume names
        Assert.Equal(dotnetRunVolumeName, f5DebuggingVolumeName);
    }
}
