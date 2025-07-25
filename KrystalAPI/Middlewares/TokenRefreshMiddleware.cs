namespace asp.net_core.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenRefreshMiddleware(RequestDelegate next) => _next = next;
        public async Task InvokeAsync(HttpContext context)
        {

        }
    }
}
