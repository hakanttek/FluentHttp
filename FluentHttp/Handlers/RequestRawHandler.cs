using System.Net;
using System.Security.Principal;

namespace FluentHttp.Handlers;

public delegate Task RequestRawHandler(HttpListenerRequest request, HttpListenerResponse response, IPrincipal? user = null, CancellationToken cancel = default);