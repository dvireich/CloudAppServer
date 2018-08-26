using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLoader
{
    class CorsConstants
    {
        internal const string Origin = "Origin";
        internal const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        internal const string AccessControlRequestMethod = "Access-Control-Request-Method";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        internal const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        internal const string PreflightSuffix = "_preflight_";

        internal const string AllowedMethods = "GET,PUT,POST,DELETE,OPTIONS";
        internal const string AllowedHeaders = "Content-Length, Authorization, Origin, X-Requested-With, Content-Type, Accept, application/json";
        internal const string AllowedOriginALL = "*";
    }
}
