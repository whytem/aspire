// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Aspire.Hosting;

internal class FailedToApplyEnvironmentException : DistributedApplicationException
{
    public FailedToApplyEnvironmentException() { }
    public FailedToApplyEnvironmentException(string message) : base(message) { }
    public FailedToApplyEnvironmentException(string message, Exception inner) : base(message, inner) { }
}

internal class ExecutableArgumentsTooLongException : DistributedApplicationException
{
    public ExecutableArgumentsTooLongException(string resourceName, int actualSize, int maxSize)
        : base($"The arguments for executable resource '{resourceName}' are too long. The serialized arguments require {actualSize} bytes, but the maximum allowed is {maxSize} bytes. Consider reducing the size of the arguments or using environment variables instead.")
    {
        ResourceName = resourceName;
        ActualSize = actualSize;
        MaxSize = maxSize;
    }

    public string ResourceName { get; }
    public int ActualSize { get; }
    public int MaxSize { get; }
}
