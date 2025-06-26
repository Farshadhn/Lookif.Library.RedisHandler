# Lookif.Library.RedisHandler

A simple, dependency-injectable Redis handler library for .NET, providing easy key-value operations (with optional serialization and TTL) using StackExchange.Redis.

## Features
- Dependency Injection (DI) ready
- Simple key-value operations (add, read, remove)
- Generic methods for storing/retrieving any serializable type
- Regex-based key operations
- Optional TTL (expiration) in minutes
- Supports Redis authentication and database selection

## Installation
1. Add the NuGet package:
   ```sh
   dotnet add package StackExchange.Redis
   ```
2. Add this library to your solution/project.

## Configuration
You can configure Redis using code or bind from `appsettings.json`:

**appsettings.json:**
```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "Username": "your-username", // optional
    "Password": "your-password", // optional
    "Database": 0
  }
}
```

**Program.cs / Startup.cs:**
```csharp
using Lookif.Library.RedisHandler;

// Bind from configuration
services.AddRedisHandler(configuration.GetSection("Redis").Bind);

// Or configure directly
services.AddRedisHandler(options =>
{
    options.Configuration = "localhost:6379";
    options.Username = "your-username"; // optional
    options.Password = "your-password"; // optional
    options.Database = 0;
});
```

## Usage
Inject `RedisHandlerService` into your service or controller:

```csharp
public class MyService
{
    private readonly RedisHandlerService _redis;
    public MyService(RedisHandlerService redis) => _redis = redis;

    public async Task Example()
    {
        // Add a string value with 10 minutes TTL
        await _redis.AddAsync("mykey", "myvalue", 10);

        // Add an object
        var obj = new MyClass { Name = "Test" };
        await _redis.AddAsync("objkey", obj, 30); // 30 minutes TTL

        // Read a string
        var value = await _redis.ReadAsync("mykey");

        // Read an object
        var myObj = await _redis.ReadAsync<MyClass>("objkey");

        // Remove a key
        await _redis.RemoveAsync("mykey");

        // Regex read
        var allMatching = await _redis.ReadByRegexAsync("^obj");
    }
}

public class MyClass
{
    public string Name { get; set; }
}
```

## License
[MIT](LICENSE)  