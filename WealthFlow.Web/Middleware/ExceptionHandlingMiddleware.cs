using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WealthFlow.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uygulama genelinde işlenmemiş bir hata yakalandı.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // API kontrolü
            var isApiRequest = context.Request.Path.StartsWithSegments("/api");

            if (isApiRequest)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                
                var response = new
                {
                    status = context.Response.StatusCode,
                    message = "Sunucuda beklenmedik bir hata oluştu.",
                    error = exception.Message
                };

                var json = JsonSerializer.Serialize(response);
                return context.Response.WriteAsync(json);
            }
            else
            {
                // MVC hatası durumunda hata sayfasına yönlendirme
                context.Response.Redirect("/Home/Error");
                return Task.CompletedTask;
            }
        }
    }
}
