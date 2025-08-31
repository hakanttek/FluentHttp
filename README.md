# FluentHttp

🚀 A lightweight, fluent, and attribute-driven HTTP server for .NET, built on top of `HttpListener` with dependency injection, JSON support, and clean routing.

FluentHttp makes it easy to create minimal HTTP APIs in .NET without the overhead of ASP.NET Core. It's ideal for embedded servers, microservices, tools, or prototyping.

---

## ✨ Features

- **Fluent API** – Chainable configuration for clean, readable code.
- **Attribute-Based Parameter Binding** – Use `[Body]`, `[Query]`, and `[Header]` to bind request data.
- **Full HTTP Method Support** – Includes standard (GET, POST, etc.) and extended methods (PATCH, WebDAV, CalDAV).
- **Built-in JSON Serialization** – Automatic JSON parsing and response with `System.Text.Json`.
- **Dependency Injection Ready** – Full integration with `Microsoft.Extensions.DependencyInjection`.
- **Customizable Fallback & Error Handling** – Define fallback routes and exception handlers.
- **Logging Support** – Integrated with `Microsoft.Extensions.Logging`.
- **CancellationToken Support** – For graceful async handling.
- **Minimal Setup** – No web host or complex configuration required.



## 🚀 Getting Started

### 1. Install via NuGet (Coming Soon)

> _Note: Currently, this is a standalone project. Package distribution will be available soon._

```bash
# Coming soon:
# dotnet add package FluentHttp
```



### 2. Create a Simple Server
```csharp
using FluentHttp;
using FluentHttp.Attributes;
using FluentHttp.Models;
using System.Net;

await HttpServer.Create(options =>
{
    options.AddScoped<FooService>();
    options.AddLogger(); // Optional: adds console logging
})
.Get("/foo", async (FooService service, [Query] int? bar, [Query] string? qux) =>
{
    var foos = await service.GetAsync(bar, qux);
    return foos.Any() 
        ? HttpStatusCode.OK.Data(foos)
        : HttpStatusCode.NotFound.Data("No foos found!");
})
.Post("/foo", async (FooService service, [Body] Foo foo) =>
{
    await service.AddAsync(foo);
    return HttpStatusCode.Created;
})
.Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Not found!" }))
.ListenOn(5000)
.StartAsync();
```



## 🛠️ Routing & Endpoints

FluentHttp supports all major HTTP methods via extension methods:

```csharp
server.Get("/path", handler);
server.Post("/path", handler);
server.Put("/path", handler);
server.Delete("/path", handler);
server.Patch("/path", handler);

// WebDAV & Extended Methods
server.PropFind("/path", handler);
server.MkCol("/path", handler);
server.Copy("/path", handler);
server.Move("/path", handler);
server.Lock("/path", handler);
server.Unlock("/path", handler);
server.Search("/path", handler);
server.Report("/path", handler);
// ... and more
```

Use `.EndPoint(method, path, handler)` for custom or non-standard methods.



## 📥 Parameter Binding

FluentHttp automatically binds parameters using attributes:

| Attribute   | Binds From             | Example |
|-------------|------------------------|---------|
| `[Body]`    | Request body (JSON)    | `[Body] User user` |
| `[Query]`   | Query string parameter | `[Query(Name = "name")] string userName` |
| `[Header]`  | HTTP header            | `[Header] string Authorization` |

Special types are also auto-injected:

 - `CancellationToken`
 - `HttpListenerRequest`
 - `HttpListenerResponse`
 - `IPrincipal`

```csharp
.Get("/user", async ([Query] int id, UserService service) =>
{
    var user = await service.GetByIdAsync(id);
    return user != null ? HttpStatusCode.OK.Data(user) : HttpStatusCode.NotFound;
})
```



## 🔌 Dependency Injection

FluentHttp integrates seamlessly with `IServiceCollection`:

```csharp
await HttpServer.Create(services =>
{
    services.AddScoped<UserService>();
    services.AddSingleton<ILogger, ConsoleLogger>();
    services.AddLogging(); // Optional: for logging
})
// ... routes
.StartAsync();
```



## 🎛️ Configuration

#### JSON Serialization

Customize JSON behavior:
```csharp
.JsonSerializerOptions(opts =>
{
    opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.WriteIndented = true;
})
```

#### Server Listening

Listen on specific ports or full URLs:
```charp
.ListenOn(5000)                          // http://localhost:5000/
.ListenOn("http://localhost:8080/api/")  // With path prefix
.ListenOn(5000, 5001, 5002)              // Multiple ports
```

## 🧯 Error & Fallback Handling

#### Fallback Route

Handle unmatched routes:
```csharp
.Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Route not found" }));
```

#### Exception Handling

Customize global exception response:
```csharp
.HandleException(async context =>
{
    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    await context.Response.JsonAsync(new { error = "Something went wrong!" });
})
```



## 📦 Models & Response Helpers

`HttpResult`

Used to standardize responses:
```csharp
return new HttpResult(HttpStatusCode.OK, data);
```



#### Extension Methods

Chain responses easily:
```csharp
HttpStatusCode.OK.Data(new { id = 1, name = "Mustermann" })
HttpStatusCode.Created
HttpStatusCode.NoContent
```

Supports both sync and async returns:

 - `HttpStatusCode`
 - `Task<HttpStatusCode>`
 - `HttpResult`
 - `Task<HttpResult>`
 - `Task`



## 📁 Project Structure
```markdown
FluentHttp/
├── HttpServer.cs              // Core server logic
├── DependencyInjection.cs     // IServiceCollection extensions
├── HttpServerExtensions.cs    // HTTP method helpers
├── HttpListenerRequestExtensions.cs // Body parsing
├── Models/
│   ├── HttpResult.cs
│   └── HttpResultExtensions.cs
└── Attributes/
    ├── BodyAttribute.cs
    ├── QueryAttribute.cs
    └── HeaderAttribute.cs
```



## 🧪 Use Cases
 - Embedded HTTP servers in desktop apps
 - Local development tools
 - IoT or internal services
 - CLI tools with web interfaces
 - Lightweight APIs without ASP.NET overhead



## 📚 License
MIT License – See [LICENSE](https://github.com/hakanttek/FluentHttp/blob/master/LICENSE.txt) for details.



## 💬 Feedback & Contributions
Contributions, issues, and feature requests are welcome! Feel free to open an issue or submit a PR.

> Note: This project is currently in active development. API may evolve before v1.0. 
