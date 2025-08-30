using FluentHttp;

await HttpServer.Create()
    .Get("/foo", async (request, response, User) => {
        response.StatusCode = 200;
        await response.JsonAsync(new { message = "Hello, World!" });
    })
    .ListenOn(5000)
    .StartAsync();