namespace OpenAI
{
    public static class ServerConfig
    {
#if UNITY_EDITOR
        // Development URLs
        public const string DEV_SERVER_URL = "http://localhost:3000";  // Backend API
        public const string DEV_CLIENT_URL = "http://localhost:8080";  // WebGL client
        public const int DEV_PORT = 8080;  // WebGL development port
#else
            // Production URLs
            public const string PROD_SERVER_URL = "http://your-production-server.com";  // Change this
            public const string PROD_CLIENT_URL = "http://your-production-client.com";  // Change this
#endif

        // Get the appropriate URL based on environment
        public static string GetServerURL()
        {
#if UNITY_EDITOR
            return DEV_SERVER_URL;
#else
                return PROD_SERVER_URL;
#endif
        }
    }
}