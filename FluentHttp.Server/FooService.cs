using System.Collections.Concurrent;

namespace FluentHttp.Server
{
    public record Foo(int Bar, string Qux);
    
    public class FooService
    {
        private readonly ConcurrentBag<Foo> _repo = new();

        public Task AddAsync(Foo foo)
        {
            _repo.Add(foo);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Foo>> GetAsync(int? bar, string? qux)
        {
            IEnumerable<Foo> res = _repo;
            if (bar is not null)
                res = res.Where(foo => foo.Bar == bar);
            if (qux is not null)
                res = res.Where(foo => foo.Qux == qux);
            return Task.CompletedTask.ContinueWith(_ => res);
        }
    }
}