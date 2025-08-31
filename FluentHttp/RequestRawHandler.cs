using System.Net;
using System.Security.Principal;

namespace FluentHttp;

public delegate Task RequestRawHandler(HttpListenerRequest request, HttpListenerResponse response, IPrincipal? user = null, CancellationToken cancel = default);