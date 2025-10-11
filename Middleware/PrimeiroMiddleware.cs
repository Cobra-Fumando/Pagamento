namespace Pic.Middleware
{
    public class PrimeiroMiddleware
    {
        private readonly RequestDelegate _next;
        public PrimeiroMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-Powered-By", out var valor))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"Mensagem\": \"Cabeçalho X-Powered-By é obrigatório.\"}");
                return;
            }
            else if (!string.Equals(valor, "Pic", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"Mensagem\": \"Valor inválido para o cabeçalho X-Powered-By.\"}");
                return;
            }

            await _next(context);
        }
    }
}
