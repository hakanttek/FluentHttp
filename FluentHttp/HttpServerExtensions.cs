using FluentHttp.Handlers;

namespace FluentHttp
{
    public static class HttpServerExtensions
    {
        #region Core RFC 7231
        public static HttpServer Get(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("GET", path, handler);

        public static HttpServer Head(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("HEAD", path, handler);

        public static HttpServer Post(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("POST", path, handler);

        public static HttpServer Put(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("PUT", path, handler);

        public static HttpServer Delete(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("DELETE", path, handler);

        public static HttpServer Connect(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("CONNECT", path, handler);

        public static HttpServer Options(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("OPTIONS", path, handler);

        public static HttpServer Trace(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("TRACE", path, handler);
        #endregion

        #region Additional RFC 5789
        public static HttpServer Patch(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("PATCH", path, handler);
        #endregion

        #region WebDAV (RFC 4918)
        public static HttpServer PropFind(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("PROPFIND", path, handler);

        public static HttpServer PropPatch(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("PROPPATCH", path, handler);

        public static HttpServer MkCol(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("MKCOL", path, handler);

        public static HttpServer Copy(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("COPY", path, handler);

        public static HttpServer Move(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("MOVE", path, handler);

        public static HttpServer Lock(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("LOCK", path, handler);

        public static HttpServer Unlock(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("UNLOCK", path, handler);
        #endregion

        #region WebDAV/DeltaV/CalDAV/CardDAV
        public static HttpServer Search(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("SEARCH", path, handler);

        public static HttpServer MkCalendar(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("MKCALENDAR", path, handler);

        public static HttpServer Report(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("REPORT", path, handler);

        public static HttpServer CheckIn(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("CHECKIN", path, handler);

        public static HttpServer CheckOut(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("CHECKOUT", path, handler);

        public static HttpServer Merge(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("MERGE", path, handler);

        public static HttpServer Acl(this HttpServer server, string path, RequestRawHandler handler)
            => server.EndPoint("ACL", path, handler);
        #endregion
    }
}