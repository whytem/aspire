// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisDistributedCache("redis1");
builder.AddKeyedRedisDistributedCache("redis2");
builder.AddKeyedRedisDistributedCache("redis2", "redis3");

var app = builder.Build();

app.MapGet("/redis1", async (IConnectionMultiplexer connection) =>
{
    var redisValue = await connection.GetDatabase().StringGetAsync("Key1");
    return redisValue.HasValue ? redisValue.ToString() : "(null)";
});

app.MapGet("/redis2", async ([FromKeyedServices("redis2")] IConnectionMultiplexer connection) =>
{
    var redisValue = await connection.GetDatabase().StringGetAsync("Key2");
    return redisValue.HasValue ? redisValue.ToString() : "(null)";
});

app.MapGet("/redis1/set", async (IConnectionMultiplexer connection) =>
{
    return await connection.GetDatabase().StringSetAsync("Key1", $"{DateTime.Now}");

});

app.MapGet("/redis2/set", async ([FromKeyedServices("redis2")] IConnectionMultiplexer connection) =>
{
    return await connection.GetDatabase().StringSetAsync("Key2", $"{DateTime.Now}");
});

app.MapGet("/cache1", async (IDistributedCache cache) =>
{
    var redisValue = await cache.GetStringAsync("Key1");
    return redisValue != null ? redisValue.ToString() : "(null)";
});

app.MapGet("/cache2", async ([FromKeyedServices("redis2")] IDistributedCache cache) =>
{
    var redisValue = await cache.GetStringAsync("Key2");
    return redisValue != null ? redisValue.ToString() : "(null)";
});

app.MapGet("/cache3", async ([FromKeyedServices("redis3")] IDistributedCache cache) =>
{
    var redisValue = await cache.GetStringAsync("Key3");
    return redisValue != null ? redisValue.ToString() : "(null)";
});

app.MapGet("/cache1/set", async (IDistributedCache cache) =>
{
    await cache.SetStringAsync("Key1", $"{DateTime.Now}");
    return Results.Ok();
});

app.MapGet("/cache2/set", async ([FromKeyedServices("redis2")] IDistributedCache cache) =>
{
    await cache.SetStringAsync("Key2", $"{DateTime.Now}");
    return Results.Ok();
});

app.MapGet("/cache3/set", async ([FromKeyedServices("redis3")] IDistributedCache cache) =>
{
    await cache.SetStringAsync("Key3", $"{DateTime.Now}");
    return Results.Ok();
});

app.MapDefaultEndpoints();

app.Run();
