using FluentHttp;

await HttpServer.Create().ListenOn(5000).StartAsync();