using System.Net;
using System.Security.Principal;

namespace FluentHttp;

public delegate Task RequestHandler(HttpListenerRequest request, HttpListenerResponse response, IPrincipal? User = null, CancellationToken cancel = default);