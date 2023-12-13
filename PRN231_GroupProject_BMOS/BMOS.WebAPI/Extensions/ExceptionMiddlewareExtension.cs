using BMOS.WebAPI.Middlewares;

namespace BMOS.WebAPI.Extensions
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
