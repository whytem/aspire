// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ModelContextProtocol.Client;

Console.WriteLine("Aspire Dashboard MCP Client Demo");
Console.WriteLine("=================================\n");

// Configure the MCP client to connect to the Aspire Dashboard
var dashboardUrl = "http://localhost:15888";
Console.WriteLine($"Connecting to Aspire Dashboard at {dashboardUrl}/mcp...");

// Create HTTP client for SSE transport
using var httpClient = new HttpClient { BaseAddress = new Uri(dashboardUrl) };

// Configure SSE transport options
var transportOptions = new SseClientTransportOptions
{
    Endpoint = new Uri($"{dashboardUrl}/mcp")
};

// Create the SSE transport
var transport = new SseClientTransport(transportOptions, httpClient);

try
{
    // Create and connect the MCP client
    Console.WriteLine("Initializing MCP client...");
    var client = await McpClientFactory.CreateAsync(transport);
    
    // Display server information
    Console.WriteLine($"\n✓ Connected to MCP Server!");
    Console.WriteLine($"  Server: {client.ServerInfo?.Name ?? "Unknown"}");
    Console.WriteLine($"  Version: {client.ServerInfo?.Version ?? "Unknown"}");
    
    // List available tools
    Console.WriteLine("\n═══════════════════════════════════");
    Console.WriteLine("Available Tools");
    Console.WriteLine("═══════════════════════════════════");
    
    var tools = await client.ListToolsAsync();
    
    if (tools.Count > 0)
    {
        Console.WriteLine($"\nFound {tools.Count} tool(s):");
        foreach (var tool in tools)
        {
            Console.WriteLine($"\n  📌 {tool.Name}");
            Console.WriteLine($"     Description: {tool.Description}");
        }
    }
    else
    {
        Console.WriteLine("\n  No tools available from the server.");
    }
    
    // List available resources
    Console.WriteLine("\n═══════════════════════════════════");
    Console.WriteLine("Available Resources");
    Console.WriteLine("═══════════════════════════════════");
    
    var resources = await client.ListResourcesAsync();
    
    if (resources.Count > 0)
    {
        Console.WriteLine($"\nFound {resources.Count} resource(s):");
        foreach (var resource in resources)
        {
            Console.WriteLine($"\n  📦 {resource.Name}");
            Console.WriteLine($"     URI: {resource.Uri}");
            if (!string.IsNullOrEmpty(resource.Description))
            {
                Console.WriteLine($"     Description: {resource.Description}");
            }
            if (!string.IsNullOrEmpty(resource.MimeType))
            {
                Console.WriteLine($"     MIME Type: {resource.MimeType}");
            }
        }
    }
    else
    {
        Console.WriteLine("\n  No resources available from the server.");
    }
    
    // List available resource templates
    Console.WriteLine("\n═══════════════════════════════════");
    Console.WriteLine("Available Resource Templates");
    Console.WriteLine("═══════════════════════════════════");
    
    var resourceTemplates = await client.ListResourceTemplatesAsync();
    
    if (resourceTemplates.Count > 0)
    {
        Console.WriteLine($"\nFound {resourceTemplates.Count} resource template(s):");
        foreach (var template in resourceTemplates)
        {
            Console.WriteLine($"\n  📋 {template.Name}");
            Console.WriteLine($"     URI Template: {template.UriTemplate}");
            if (!string.IsNullOrEmpty(template.Description))
            {
                Console.WriteLine($"     Description: {template.Description}");
            }
            if (!string.IsNullOrEmpty(template.MimeType))
            {
                Console.WriteLine($"     MIME Type: {template.MimeType}");
            }
        }
    }
    else
    {
        Console.WriteLine("\n  No resource templates available from the server.");
    }
    
    // Interactive menu for testing
    Console.WriteLine("\n═══════════════════════════════════");
    Console.WriteLine("Interactive Testing");
    Console.WriteLine("═══════════════════════════════════");
    
    while (true)
    {
        Console.WriteLine("\nWhat would you like to do?");
        Console.WriteLine("  1. Test a tool");
        Console.WriteLine("  2. Read a resource");
        Console.WriteLine("  3. Read a resource template");
        Console.WriteLine("  4. Exit");
        Console.Write("\nChoice (1-4): ");
        
        var choice = Console.ReadLine();
        
        if (choice == "4" || string.IsNullOrWhiteSpace(choice))
        {
            break;
        }
        
        if (choice == "1" && tools.Count > 0)
        {
            Console.WriteLine("\nAvailable tools:");
            for (int i = 0; i < tools.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {tools[i].Name}");
            }
            Console.Write("Select tool number: ");
            
            if (int.TryParse(Console.ReadLine(), out int toolIndex) && 
                toolIndex > 0 && toolIndex <= tools.Count)
            {
                var selectedTool = tools[toolIndex - 1];
                Console.WriteLine($"\nTesting tool: {selectedTool.Name}");
                
                // For now, handle echo and reverse_echo tools specifically
                if (selectedTool.Name == "echo" || selectedTool.Name == "reverse_echo")
                {
                    Console.Write("Enter a message: ");
                    var message = Console.ReadLine() ?? "Test message";
                    
                    var arguments = new Dictionary<string, object?>
                    {
                        ["message"] = message
                    };
                    
                    var response = await client.CallToolAsync(
                        selectedTool.Name,
                        arguments,
                        progress: null,
                        cancellationToken: default);
                    
                    if (response.IsError != true && response.Content.Count > 0)
                    {
                        var content = response.Content.First();
                        if (content is ModelContextProtocol.Protocol.TextContentBlock textContent)
                        {
                            Console.WriteLine($"Response: {textContent.Text}");
                        }
                    }
                    else if (response.IsError == true)
                    {
                        Console.WriteLine($"Error: {response.Content.FirstOrDefault()}");
                    }
                }
                else
                {
                    Console.WriteLine($"Tool '{selectedTool.Name}' requires custom implementation.");
                }
            }
        }
        else if (choice == "2" && resources.Count > 0)
        {
            Console.WriteLine("\nAvailable resources:");
            for (int i = 0; i < resources.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {resources[i].Name} ({resources[i].Uri})");
            }
            Console.Write("Select resource number: ");
            
            if (int.TryParse(Console.ReadLine(), out int resourceIndex) && 
                resourceIndex > 0 && resourceIndex <= resources.Count)
            {
                var selectedResource = resources[resourceIndex - 1];
                Console.WriteLine($"\nReading resource: {selectedResource.Name}");
                
                try
                {
                    var resourceContent = await client.ReadResourceAsync(selectedResource.Uri);
                    
                    if (resourceContent.Contents.Count > 0)
                    {
                        Console.WriteLine("\nResource content:");
                        foreach (var content in resourceContent.Contents)
                        {
                            if (content is ModelContextProtocol.Protocol.TextResourceContents textContent)
                            {
                                Console.WriteLine(textContent.Text);
                            }
                            else if (content is ModelContextProtocol.Protocol.BlobResourceContents blobContent)
                            {
                                Console.WriteLine($"[Binary content: {blobContent.Blob.Length} bytes]");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading resource: {ex.Message}");
                }
            }
        }
        else if (choice == "3" && resourceTemplates.Count > 0)
        {
            Console.WriteLine("\nAvailable resource templates:");
            for (int i = 0; i < resourceTemplates.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {resourceTemplates[i].Name} ({resourceTemplates[i].UriTemplate})");
            }
            Console.Write("Select resource template number: ");
            
            if (int.TryParse(Console.ReadLine(), out int templateIndex) && 
                templateIndex > 0 && templateIndex <= resourceTemplates.Count)
            {
                var selectedTemplate = resourceTemplates[templateIndex - 1];
                Console.WriteLine($"\nSelected template: {selectedTemplate.Name}");
                Console.WriteLine($"URI Template: {selectedTemplate.UriTemplate}");
                
                // Extract parameters from the URI template
                var uriTemplate = selectedTemplate.UriTemplate;
                var parameterMatches = System.Text.RegularExpressions.Regex.Matches(uriTemplate, @"\{(\w+)\}");
                
                var parameters = new Dictionary<string, string>();
                foreach (System.Text.RegularExpressions.Match match in parameterMatches)
                {
                    var paramName = match.Groups[1].Value;
                    Console.Write($"Enter value for '{paramName}': ");
                    var paramValue = Console.ReadLine() ?? "";
                    parameters[paramName] = paramValue;
                }
                
                // Build the actual URI by replacing parameters
                var actualUri = uriTemplate;
                foreach (var param in parameters)
                {
                    actualUri = actualUri.Replace($"{{{param.Key}}}", param.Value);
                }
                
                Console.WriteLine($"\nReading resource from URI: {actualUri}");
                
                try
                {
                    var resourceContent = await client.ReadResourceAsync(actualUri);
                    
                    if (resourceContent.Contents.Count > 0)
                    {
                        Console.WriteLine("\nResource content:");
                        foreach (var content in resourceContent.Contents)
                        {
                            if (content is ModelContextProtocol.Protocol.TextResourceContents textContent)
                            {
                                Console.WriteLine(textContent.Text);
                            }
                            else if (content is ModelContextProtocol.Protocol.BlobResourceContents blobContent)
                            {
                                Console.WriteLine($"[Binary content: {blobContent.Blob.Length} bytes]");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading resource: {ex.Message}");
                }
            }
        }
    }
    
    // Clean up
    await client.DisposeAsync();
    Console.WriteLine("\n✓ Disconnected from MCP server.");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    }
}

if (Environment.UserInteractive && !Console.IsInputRedirected)
{
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}